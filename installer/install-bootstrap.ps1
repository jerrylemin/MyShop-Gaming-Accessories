param(
    [Parameter(Mandatory = $true)][string]$AppDir,
    [Parameter(Mandatory = $true)][string]$PrereqDir,
    [Parameter(Mandatory = $true)][string]$DatabaseTool
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$logDir = Join-Path $env:ProgramData 'MyShop POS\Logs'
$logPath = Join-Path $logDir 'install-bootstrap.log'
$databaseName = 'myshop_gaming_accessories'
$appUser = 'myshop_app'
$appPassword = 'MyShopApp#2026'
$defaultPostgresAdminPassword = 'MyShopAdmin#2026'

function Write-InstallLog {
    param([string]$Message)
    New-Item -ItemType Directory -Force -Path $logDir | Out-Null
    Add-Content -Path $logPath -Value "[$([DateTimeOffset]::Now.ToString('O'))] $Message"
}

function Invoke-Installer {
    param(
        [string]$Path,
        [string]$Arguments,
        [string]$Name
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "$Name installer is missing: $Path"
    }

    Write-InstallLog "Running $Name installer: $Path $Arguments"
    $process = Start-Process -FilePath $Path -ArgumentList $Arguments -Wait -PassThru
    Write-InstallLog "$Name installer exit code: $($process.ExitCode)"

    if ($process.ExitCode -notin @(0, 3010, 1641)) {
        throw "$Name installer failed with exit code $($process.ExitCode). See $logPath"
    }
}

function Test-DotNetDesktopRuntime {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnet) {
        return $false
    }

    $runtimes = & $dotnet.Source --list-runtimes 2>$null
    return ($runtimes | Select-String -Pattern '^Microsoft\.WindowsDesktop\.App 8\.' -Quiet)
}

function Ensure-DotNetDesktopRuntime {
    if (Test-DotNetDesktopRuntime) {
        Write-InstallLog '.NET Desktop Runtime 8 is already installed.'
        return
    }

    Invoke-Installer `
        -Path (Join-Path $PrereqDir 'windowsdesktop-runtime-8-win-x64.exe') `
        -Arguments '/install /quiet /norestart' `
        -Name '.NET Desktop Runtime 8'
}

function Test-WindowsAppRuntime {
    return [bool](Get-AppxPackage -Name 'Microsoft.WindowsAppRuntime.1.8' -ErrorAction SilentlyContinue)
}

function Ensure-WindowsAppRuntime {
    if (Test-WindowsAppRuntime) {
        Write-InstallLog 'Windows App Runtime 1.8 is already installed.'
        return
    }

    Invoke-Installer `
        -Path (Join-Path $PrereqDir 'windowsappruntimeinstall-x64.exe') `
        -Arguments '--quiet' `
        -Name 'Windows App Runtime 1.8'
}

function Get-PostgreSqlService {
    Get-Service -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like 'postgresql*' -or $_.DisplayName -like 'postgresql*' } |
        Sort-Object Name |
        Select-Object -First 1
}

function Test-PortOpen {
    param([int]$Port)
    try {
        $client = [System.Net.Sockets.TcpClient]::new()
        $async = $client.BeginConnect('127.0.0.1', $Port, $null, $null)
        $connected = $async.AsyncWaitHandle.WaitOne(600)
        if ($connected) {
            $client.EndConnect($async)
        }
        $client.Dispose()
        return $connected
    }
    catch {
        return $false
    }
}

function Ensure-PostgreSql {
    $service = Get-PostgreSqlService
    if ($service) {
        Write-InstallLog "Existing PostgreSQL service found: $($service.Name)"
        if ($service.Status -ne 'Running') {
            Start-Service -Name $service.Name
            Start-Sleep -Seconds 5
        }
        return 5432
    }

    $port = 5432
    if (Test-PortOpen -Port 5432) {
        $port = 5433
        Write-InstallLog 'Port 5432 is occupied before PostgreSQL install. Installing PostgreSQL on port 5433.'
    }

    $installDir = Join-Path $env:ProgramFiles 'PostgreSQL\16'
    $dataDir = Join-Path $env:ProgramData 'MyShop POS\PostgreSQL\data'
    New-Item -ItemType Directory -Force -Path $dataDir | Out-Null

    $postgresArgs = @(
        '--mode unattended',
        '--unattendedmodeui none',
        "--superpassword `"$defaultPostgresAdminPassword`"",
        "--serverport $port",
        '--servicename postgresql-x64-16',
        "--prefix `"$installDir`"",
        "--datadir `"$dataDir`"",
        '--disable-components stackbuilder'
    ) -join ' '

    Invoke-Installer `
        -Path (Join-Path $PrereqDir 'postgresql-16-windows-x64.exe') `
        -Arguments $postgresArgs `
        -Name 'PostgreSQL 16'

    $service = Get-Service -Name 'postgresql-x64-16' -ErrorAction SilentlyContinue
    if ($service -and $service.Status -ne 'Running') {
        Start-Service -Name $service.Name
    }

    return $port
}

function Invoke-DatabaseBootstrap {
    param([int]$Port)

    $candidatePasswords = @()
    if ($env:MYSHOP_POSTGRES_ADMIN_PASSWORD) {
        $candidatePasswords += $env:MYSHOP_POSTGRES_ADMIN_PASSWORD
    }
    $candidatePasswords += $defaultPostgresAdminPassword
    $candidatePasswords += 'jelly'
    $candidatePasswords += 'postgres'
    $candidatePasswords = $candidatePasswords | Select-Object -Unique

    foreach ($password in $candidatePasswords) {
        $args = @(
            '--host', 'localhost',
            '--port', $Port,
            '--admin-user', 'postgres',
            '--admin-password', $password,
            '--app-user', $appUser,
            '--app-password', $appPassword,
            '--database', $databaseName,
            '--app-dir', $AppDir,
            '--log', (Join-Path $logDir 'database-bootstrap.log')
        )

        Write-InstallLog "Trying database bootstrap on port $Port."
        $process = Start-Process -FilePath $DatabaseTool -ArgumentList $args -Wait -PassThru -WindowStyle Hidden
        if ($process.ExitCode -eq 0) {
            Write-InstallLog 'Database bootstrap completed.'
            return
        }
    }

    throw 'Database bootstrap failed. If PostgreSQL already existed, set MYSHOP_POSTGRES_ADMIN_PASSWORD to the postgres admin password and rerun setup.'
}

try {
    Write-InstallLog 'Starting MyShop POS installer bootstrap.'
    Ensure-DotNetDesktopRuntime
    Ensure-WindowsAppRuntime
    $postgresPort = Ensure-PostgreSql
    Invoke-DatabaseBootstrap -Port $postgresPort
    Write-InstallLog 'MyShop POS installer bootstrap finished.'
}
catch {
    Write-InstallLog $_.Exception.ToString()
    [System.Windows.Forms.MessageBox]::Show($_.Exception.Message, 'MyShop POS setup failed', 'OK', 'Error') | Out-Null
    exit 1
}
