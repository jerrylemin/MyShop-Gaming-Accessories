param(
    [Parameter(Mandatory = $true)][string]$AppDir,
    [Parameter(Mandatory = $true)][string]$PrereqDir,
    [Parameter(Mandatory = $true)][string]$DatabaseTool,
    [Parameter(Mandatory = $true)][string]$RestoreScript,
    [Parameter(Mandatory = $true)][string]$DemoDump
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$logDir = Join-Path $env:ProgramData 'MyShop POS\Logs'
$logPath = Join-Path $logDir 'setup-log.txt'
$databaseName = 'myshop_gaming_accessories'
$appUser = 'myshop_app'

function New-InstallerPassword {
    $bytes = New-Object byte[] 24
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $rng.GetBytes($bytes)
        return [Convert]::ToBase64String($bytes)
    }
    finally {
        $rng.Dispose()
    }
}

$appPassword = if ($env:MYSHOP_APP_DATABASE_PASSWORD) { $env:MYSHOP_APP_DATABASE_PASSWORD } else { New-InstallerPassword }
$installPostgresAdminPassword = if ($env:MYSHOP_POSTGRES_ADMIN_PASSWORD) { $env:MYSHOP_POSTGRES_ADMIN_PASSWORD } else { New-InstallerPassword }

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

    $safeArguments = $Arguments -replace '--superpassword\s+"[^"]+"', '--superpassword "***"'
    Write-InstallLog "Running $Name installer: $Path $safeArguments"
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
    param([switch]$MyShopOnly)

    Get-Service -ErrorAction SilentlyContinue |
        Where-Object {
            if ($MyShopOnly) {
                $_.Name -eq 'myshop-postgresql-18'
            }
            else {
                $_.Name -like 'postgresql*' -or $_.DisplayName -like 'postgresql*' -or $_.Name -eq 'myshop-postgresql-16'
            }
        } |
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
    $myShopService = Get-PostgreSqlService -MyShopOnly
    if ($myShopService -and $env:MYSHOP_POSTGRES_ADMIN_PASSWORD) {
        Write-InstallLog "Existing MyShop PostgreSQL service found: $($myShopService.Name)"
        if ($myShopService.Status -ne 'Running') {
            Start-Service -Name $myShopService.Name
            Start-Sleep -Seconds 5
        }
        return 5433
    }

    $service = Get-PostgreSqlService
    if ($service -and $env:MYSHOP_POSTGRES_ADMIN_PASSWORD) {
        Write-InstallLog "Existing PostgreSQL service found and admin password was provided: $($service.Name)"
        if ($service.Status -ne 'Running') {
            Start-Service -Name $service.Name
            Start-Sleep -Seconds 5
        }
        return 5432
    }

    if ($service) {
        Write-InstallLog "Existing PostgreSQL service found without MYSHOP_POSTGRES_ADMIN_PASSWORD: $($service.Name). Installing isolated MyShop PostgreSQL service instead."
    }

    $port = 5432
    while (Test-PortOpen -Port $port) {
        $port++
        if ($port -gt 5442) {
            throw 'Could not find a free PostgreSQL port in range 5432-5442.'
        }
    }
    Write-InstallLog "Installing PostgreSQL for MyShop on port $port."

    $installDir = Join-Path $env:ProgramFiles 'PostgreSQL\18'
    $dataDir = Join-Path $env:ProgramData 'MyShop POS\PostgreSQL18\data'
    New-Item -ItemType Directory -Force -Path $dataDir | Out-Null

    $postgresArgs = @(
        '--mode unattended',
        '--unattendedmodeui none',
        "--superpassword `"$installPostgresAdminPassword`"",
        "--serverport $port",
        '--servicename myshop-postgresql-18',
        "--prefix `"$installDir`"",
        "--datadir `"$dataDir`"",
        '--disable-components stackbuilder'
    ) -join ' '

    Invoke-Installer `
        -Path (Join-Path $PrereqDir 'postgresql-18-windows-x64.exe') `
        -Arguments $postgresArgs `
        -Name 'PostgreSQL 18'

    $service = Get-Service -Name 'myshop-postgresql-18' -ErrorAction SilentlyContinue
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
    $candidatePasswords += $installPostgresAdminPassword
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
            '--log', (Join-Path $logDir 'restore-demo-database.log')
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

function Invoke-DatabaseRestoreOrSeed {
    param([int]$Port)

    $candidatePasswords = @()
    if ($env:MYSHOP_POSTGRES_ADMIN_PASSWORD) {
        $candidatePasswords += $env:MYSHOP_POSTGRES_ADMIN_PASSWORD
    }
    $candidatePasswords += $installPostgresAdminPassword
    $candidatePasswords = $candidatePasswords | Select-Object -Unique

    $appConnectionString = "Host=localhost;Port=$Port;Database=$databaseName;Username=$appUser;Password=$appPassword;Include Error Detail=true"

    foreach ($password in $candidatePasswords) {
        $adminConnectionString = "Host=localhost;Port=$Port;Database=postgres;Username=postgres;Password=$password;Include Error Detail=true"
        $args = @(
            '-ExecutionPolicy', 'Bypass',
            '-File', $RestoreScript,
            '-TargetConnectionString', $appConnectionString,
            '-AdminConnectionString', $adminConnectionString,
            '-DumpPath', $DemoDump,
            '-LogPath', (Join-Path $logDir 'restore-demo-database.log'),
            '-FallbackDatabaseTool', $DatabaseTool,
            '-AppDir', $AppDir
        )

        Write-InstallLog "Trying database restore from dump on port $Port."
        $process = Start-Process -FilePath (Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe') -ArgumentList $args -Wait -PassThru -WindowStyle Hidden
        Write-InstallLog "Database restore script exit code: $($process.ExitCode)"
        if ($process.ExitCode -eq 0) {
            return
        }
    }

    Write-InstallLog 'Restore script failed for all admin credentials. Falling back to database bootstrapper seed.'
    Invoke-DatabaseBootstrap -Port $Port
}

try {
    Write-InstallLog 'Starting MyShop POS installer bootstrap.'
    Ensure-DotNetDesktopRuntime
    Ensure-WindowsAppRuntime
    $postgresPort = Ensure-PostgreSql
    Invoke-DatabaseRestoreOrSeed -Port $postgresPort
    Write-InstallLog 'MyShop POS installer bootstrap finished.'
}
catch {
    Write-InstallLog $_.Exception.ToString()
    [System.Windows.Forms.MessageBox]::Show($_.Exception.Message, 'MyShop POS setup failed', 'OK', 'Error') | Out-Null
    exit 1
}
