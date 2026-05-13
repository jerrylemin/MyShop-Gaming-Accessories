@echo off
setlocal

set "MYSHOP_FINAL_SETUP_BAT=%~f0"
set "MYSHOP_FINAL_RELEASE=%~dp0"

net session >nul 2>&1
if not "%errorlevel%"=="0" (
    echo Requesting administrator permission...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%ComSpec%' -ArgumentList '/c ""%MYSHOP_FINAL_SETUP_BAT%""' -Verb RunAs"
    exit /b
)

powershell -NoProfile -ExecutionPolicy Bypass -Command "$bat=$env:MYSHOP_FINAL_SETUP_BAT; $text=[System.IO.File]::ReadAllText($bat); $parts=[regex]::Split($text,'\r?\n# POWERSHELL_START\r?\n',2); if ($parts.Count -lt 2) { throw 'PowerShell payload was not found in setup.bat.' }; Invoke-Expression $parts[1]"
set "MYSHOP_FINAL_SETUP_EXIT=%errorlevel%"

if not "%MYSHOP_FINAL_SETUP_EXIT%"=="0" (
    echo.
    echo setup.bat failed with exit code %MYSHOP_FINAL_SETUP_EXIT%.
    pause
    exit /b %MYSHOP_FINAL_SETUP_EXIT%
)

echo.
echo setup.bat completed successfully.
pause
exit /b 0

# POWERSHELL_START
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Write-Ok {
    param([string]$Message)
    Write-Host "OK: $Message" -ForegroundColor Green
}

function Test-Command {
    param([string]$Name)
    return $null -ne (Get-Command $Name -ErrorAction SilentlyContinue)
}

function Invoke-External {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$FilePath failed with exit code $LASTEXITCODE."
    }
}

function Install-WingetPackage {
    param(
        [string]$PackageId,
        [string]$DisplayName
    )

    if (-not (Test-Command "winget")) {
        throw "$DisplayName is missing and winget is not available. Install $DisplayName manually, then rerun setup.bat."
    }

    Write-Step "Installing $DisplayName"
    Invoke-External "winget" @(
        "install",
        "--id", $PackageId,
        "--exact",
        "--silent",
        "--accept-package-agreements",
        "--accept-source-agreements"
    )
}

function Ensure-DotNetSdk {
    Write-Step "Checking .NET 8 SDK"
    $hasDotNet8 = $false
    if (Test-Command "dotnet") {
        $hasDotNet8 = (dotnet --list-sdks) -match "^8\."
    }

    if (-not $hasDotNet8) {
        Install-WingetPackage "Microsoft.DotNet.SDK.8" ".NET 8 SDK"
    }

    Write-Ok ".NET 8 SDK is available"
}

function Ensure-WindowsAppRuntime {
    Write-Step "Checking Windows App Runtime 1.8"
    $runtime = Get-AppxPackage -Name "Microsoft.WindowsAppRuntime.1.8" -ErrorAction SilentlyContinue
    if (-not $runtime) {
        Install-WingetPackage "Microsoft.WindowsAppRuntime.1.8" "Windows App Runtime 1.8"
    }

    Write-Ok "Windows App Runtime is available"
}

function Get-PostgresInstallation {
    Write-Step "Checking PostgreSQL"

    $service = Get-CimInstance Win32_Service |
        Where-Object { $_.Name -like "postgresql-x64-*" } |
        Sort-Object Name -Descending |
        Select-Object -First 1

    if (-not $service) {
        Install-WingetPackage "PostgreSQL.PostgreSQL" "PostgreSQL"
        $service = Get-CimInstance Win32_Service |
            Where-Object { $_.Name -like "postgresql-x64-*" } |
            Sort-Object Name -Descending |
            Select-Object -First 1
    }

    if (-not $service) {
        throw "PostgreSQL installation finished, but no postgresql-x64-* Windows service was found."
    }

    if ($service.State -ne "Running") {
        Start-Service -Name $service.Name
        Start-Sleep -Seconds 2
    }

    $matches = [regex]::Match($service.PathName, '"(?<bin>.+\\bin)\\pg_ctl\.exe".*-D "(?<data>.+)"')
    if (-not $matches.Success) {
        throw "Could not resolve PostgreSQL bin/data directories from service path: $($service.PathName)"
    }

    [pscustomobject]@{
        ServiceName = $service.Name
        BinDir = $matches.Groups["bin"].Value
        DataDir = $matches.Groups["data"].Value
    }
}

function Invoke-Psql {
    param(
        [Parameter(Mandatory = $true)]
        [string]$DatabaseName,
        [Parameter(Mandatory = $true)]
        [string]$Sql,
        [switch]$TrustAuth
    )

    if ($TrustAuth) {
        Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
    }
    else {
        $env:PGPASSWORD = $script:Password
    }

    $tempRoot = if ($script:SqlTempDir) { $script:SqlTempDir } else { $env:TEMP }
    New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null
    $sqlPath = Join-Path $tempRoot ("myshop-sql-" + [Guid]::NewGuid().ToString("N") + ".sql")
    try {
        [System.IO.File]::WriteAllText($sqlPath, $Sql, (New-Object System.Text.UTF8Encoding($false)))
        $arguments = @(
            "-h", $script:DbHost,
            "-p", $script:Port,
            "-U", $script:Username,
            "-d", $DatabaseName,
            "-v", "ON_ERROR_STOP=1",
            "-tA",
            "-f", $sqlPath
        )

        & $script:PsqlExe @arguments
        if ($LASTEXITCODE -ne 0) {
            throw "psql failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Remove-Item -LiteralPath $sqlPath -ErrorAction SilentlyContinue
    }
}

function Test-PostgresCredential {
    try {
        Invoke-Psql -DatabaseName "postgres" -Sql "SELECT 1;"
        return $true
    }
    catch {
        return $false
    }
}

function Enable-TemporaryTrustAuth {
    $script:PgHbaOriginal = Get-Content -LiteralPath $script:PgHbaPath -Raw
    $trustBlock = @"
host    all             all             127.0.0.1/32            trust
host    all             all             ::1/128                 trust

"@
    $patched = $trustBlock + $script:PgHbaOriginal

    Set-Content -LiteralPath $script:PgHbaPath -Value $patched -NoNewline
    & $script:PgCtlExe reload -D $script:Installation.DataDir | Out-Null
    Start-Sleep -Seconds 1
}

function Restore-AuthConfig {
    if ($null -ne $script:PgHbaOriginal) {
        Set-Content -LiteralPath $script:PgHbaPath -Value $script:PgHbaOriginal -NoNewline
        & $script:PgCtlExe reload -D $script:Installation.DataDir | Out-Null
        Start-Sleep -Seconds 1
    }
}

function Ensure-PostgresRoleAndDatabase {
    Write-Step "Configuring PostgreSQL role and database"

    $userLiteral = $script:Username.Replace("'", "''")
    $passwordLiteral = $script:Password.Replace("'", "''")
    $databaseLiteral = $script:Database.Replace("'", "''")

    $roleSql = @"
DO `$role_fix`$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '$userLiteral') THEN
        EXECUTE format('ALTER ROLE %I WITH LOGIN SUPERUSER CREATEDB CREATEROLE PASSWORD %L', '$userLiteral', '$passwordLiteral');
    ELSE
        EXECUTE format('CREATE ROLE %I WITH LOGIN SUPERUSER CREATEDB CREATEROLE PASSWORD %L', '$userLiteral', '$passwordLiteral');
    END IF;
END
`$role_fix`$;
"@

    if (Test-PostgresCredential) {
        Invoke-Psql -DatabaseName "postgres" -Sql $roleSql
    }
    else {
        Enable-TemporaryTrustAuth
        try {
            Invoke-Psql -DatabaseName "postgres" -Sql $roleSql -TrustAuth
        }
        finally {
            Restore-AuthConfig
        }
    }

    $existsSql = "SELECT 1 FROM pg_database WHERE datname = '$databaseLiteral';"
    $createSql = "CREATE DATABASE $script:Database OWNER $script:Username;"

    $env:PGPASSWORD = $script:Password
    $exists = & $script:PsqlExe -h $script:DbHost -p $script:Port -U $script:Username -d postgres -tA -c $existsSql
    if ($LASTEXITCODE -ne 0) {
        throw "Could not check whether database exists."
    }

    if (($exists | Out-String).Trim() -ne "1") {
        Invoke-Psql -DatabaseName "postgres" -Sql $createSql
    }
}

function New-DesktopShortcut {
    param(
        [string]$TargetPath,
        [string]$WorkingDirectory
    )

    Write-Step "Creating Desktop shortcut"
    $shortcutPath = Join-Path ([Environment]::GetFolderPath("Desktop")) "MyShop Gaming Accessories POS.lnk"
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $TargetPath
    $shortcut.WorkingDirectory = $WorkingDirectory
    $shortcut.IconLocation = $TargetPath
    $shortcut.Description = "MyShop Gaming Accessories POS"
    $shortcut.Save()
    Write-Ok "Shortcut created: $shortcutPath"
}

$releaseDir = (Resolve-Path -LiteralPath $env:MYSHOP_FINAL_RELEASE).Path
$finalDir = Split-Path -Parent $releaseDir
$sourceDir = Join-Path $finalDir "Source code"
$projectPath = Join-Path $sourceDir "ProjectTest.csproj"
$publishDir = Join-Path $releaseDir "App"
$appExe = Join-Path $publishDir "ProjectTest.exe"
$logDir = Join-Path $releaseDir "logs"
$logPath = Join-Path $logDir ("setup-bat-" + (Get-Date -Format "yyyyMMdd-HHmmss") + ".log")
$safeTempDir = Join-Path $logDir "temp"

New-Item -ItemType Directory -Path $logDir -Force | Out-Null
New-Item -ItemType Directory -Path $safeTempDir -Force | Out-Null
$env:TEMP = $safeTempDir
$env:TMP = $safeTempDir
$script:SqlTempDir = $safeTempDir
Start-Transcript -LiteralPath $logPath -Append | Out-Null

try {
    Write-Step "MyShop setup from Final folder"
    Write-Host "Final:   $finalDir"
    Write-Host "Source:  $sourceDir"
    Write-Host "Release: $releaseDir"
    Write-Host "Log:     $logPath"

    if (-not (Test-Path -LiteralPath $projectPath)) {
        throw "ProjectTest.csproj was not found. Keep Final\Source code next to Final\Release."
    }

    $script:DbHost = "localhost"
    $script:Port = 5432
    $script:Database = "myshop_gaming_accessories"
    $script:Username = "postgres"
    $script:Password = if ($env:MYSHOP_POSTGRES_ADMIN_PASSWORD) { $env:MYSHOP_POSTGRES_ADMIN_PASSWORD } else { "jelly" }
    $script:PgHbaOriginal = $null

    Ensure-DotNetSdk
    Ensure-WindowsAppRuntime

    $script:Installation = Get-PostgresInstallation
    $script:PsqlExe = Join-Path $script:Installation.BinDir "psql.exe"
    $script:PgCtlExe = Join-Path $script:Installation.BinDir "pg_ctl.exe"
    $script:PgHbaPath = Join-Path $script:Installation.DataDir "pg_hba.conf"
    Write-Ok "PostgreSQL service '$($script:Installation.ServiceName)' is running"

    Ensure-PostgresRoleAndDatabase

    $connectionString = "Host=$script:DbHost;Port=$script:Port;Database=$script:Database;Username=$script:Username;Password=$script:Password;Include Error Detail=true"
    [Environment]::SetEnvironmentVariable("MYSHOP_CONNECTION_STRING", $connectionString, "User")
    [Environment]::SetEnvironmentVariable("MYSHOP_CONNECTION_STRING", $connectionString, "Machine")
    $env:MYSHOP_CONNECTION_STRING = $connectionString
    Write-Ok "Database connection string saved to user and machine environment"

    Push-Location $sourceDir
    try {
        Write-Step "Restoring and publishing app from Final\Source code"
        Invoke-External "dotnet" @("restore", $projectPath)
        Invoke-External "dotnet" @(
            "publish",
            $projectPath,
            "-c", "Release",
            "-p:Platform=x64",
            "-p:RuntimeIdentifier=win-x64",
            "-p:SelfContained=true",
            "-p:PublishSingleFile=false",
            "-p:PublishDir=$publishDir\"
        )
    }
    finally {
        Pop-Location
    }

    if (-not (Test-Path -LiteralPath $appExe)) {
        throw "Published app executable was not found: $appExe"
    }

    $databaseConfigPath = Join-Path $publishDir "myshop.database.json"
    @{ ConnectionString = $connectionString } |
        ConvertTo-Json -Depth 2 |
        Set-Content -LiteralPath $databaseConfigPath -Encoding UTF8
    Write-Ok "Database config written: $databaseConfigPath"

    $runCmdPath = Join-Path $publishDir "Run-ProjectTest.cmd"
    $runCmd = @(
        "@echo off",
        "cd /d %~dp0",
        'start "ProjectTest" "%~dp0ProjectTest.exe"'
    ) -join [Environment]::NewLine
    Set-Content -LiteralPath $runCmdPath -Value $runCmd -Encoding ASCII

    New-DesktopShortcut -TargetPath $appExe -WorkingDirectory $publishDir

    Write-Step "Starting app to initialize migrations and seed demo database"
    Start-Process -FilePath $appExe -WorkingDirectory $publishDir

    Write-Host ""
    Write-Host "Setup completed." -ForegroundColor Green
    Write-Host "Login accounts:" -ForegroundColor Yellow
    Write-Host "  admin / MyShop123!"
    Write-Host "  moderator / MyShop123!"
    Write-Host "  sale / MyShop123!"
    Write-Host ""
    Write-Host "If PostgreSQL already has another password, rerun after setting MYSHOP_POSTGRES_ADMIN_PASSWORD."
}
finally {
    Stop-Transcript | Out-Null
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}
