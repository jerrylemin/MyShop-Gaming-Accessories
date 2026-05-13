[CmdletBinding()]
param(
    [string]$DbHost = "localhost",
    [int]$Port = 5432,
    [string]$Database = "myshop_gaming_accessories",
    [string]$Username = "postgres",
    [string]$Password = "jelly",
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [ValidateSet("x64", "x86", "ARM64")]
    [string]$Platform = "x64",
    [switch]$InstallPrerequisites,
    [switch]$ResetDatabase,
    [switch]$SkipPublish,
    [switch]$RunAfterSetup
)

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
        throw "$DisplayName is missing and winget is not available. Install it manually, then rerun this setup file."
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
        if (-not $InstallPrerequisites) {
            throw ".NET 8 SDK is missing. Install it or rerun: powershell -ExecutionPolicy Bypass -File .\setup.ps1 -InstallPrerequisites"
        }

        Install-WingetPackage "Microsoft.DotNet.SDK.8" ".NET 8 SDK"
    }

    Write-Ok ".NET 8 SDK is available"
}

function Ensure-WindowsAppRuntime {
    Write-Step "Checking Windows App Runtime 1.8"

    $runtime = Get-AppxPackage -Name "Microsoft.WindowsAppRuntime.1.8" -ErrorAction SilentlyContinue
    if (-not $runtime) {
        if (-not $InstallPrerequisites) {
            Write-Warning "Windows App Runtime 1.8 was not found. If the app cannot start, rerun setup with -InstallPrerequisites or install Windows App Runtime 1.8 manually."
            return
        }

        Install-WingetPackage "Microsoft.WindowsAppRuntime.1.8" "Windows App Runtime 1.8"
    }

    Write-Ok "Windows App Runtime check completed"
}

function Get-PostgresInstallation {
    $service = Get-CimInstance Win32_Service |
        Where-Object { $_.Name -like "postgresql-x64-*" } |
        Sort-Object Name -Descending |
        Select-Object -First 1

    if (-not $service) {
        if (-not $InstallPrerequisites) {
            throw "PostgreSQL service was not found. Install PostgreSQL or rerun: powershell -ExecutionPolicy Bypass -File .\setup.ps1 -InstallPrerequisites"
        }

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

    return [pscustomobject]@{
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

    $stdoutPath = [System.IO.Path]::GetTempFileName()
    $stderrPath = [System.IO.Path]::GetTempFileName()
    $sqlPath = [System.IO.Path]::GetTempFileName()

    try {
        Set-Content -Path $sqlPath -Value $Sql -Encoding UTF8 -NoNewline
        $arguments = @(
            "-h", $script:DbHost,
            "-p", $script:Port,
            "-U", $script:Username,
            "-d", $DatabaseName,
            "-v", "ON_ERROR_STOP=1",
            "-tA",
            "-f", $sqlPath
        )

        $process = Start-Process `
            -FilePath $script:PsqlExe `
            -ArgumentList $arguments `
            -NoNewWindow `
            -Wait `
            -PassThru `
            -RedirectStandardOutput $stdoutPath `
            -RedirectStandardError $stderrPath

        $stdout = if (Test-Path $stdoutPath) { Get-Content $stdoutPath -Raw } else { "" }
        $stderr = if (Test-Path $stderrPath) { Get-Content $stderrPath -Raw } else { "" }
        $output = @($stdout, $stderr) -join [Environment]::NewLine
    }
    finally {
        Remove-Item $stdoutPath, $stderrPath, $sqlPath -ErrorAction SilentlyContinue
    }

    if ($process.ExitCode -ne 0) {
        $message = ($output | Out-String).Trim()
        throw "psql failed: $message"
    }

    return ($output | Out-String).Trim()
}

function Test-PostgresCredential {
    try {
        [void](Invoke-Psql -DatabaseName "postgres" -Sql "SELECT 1;")
        return $true
    }
    catch {
        return $false
    }
}

function Enable-TemporaryTrustAuth {
    $script:PgHbaOriginal = Get-Content $script:PgHbaPath -Raw

    $patched = $script:PgHbaOriginal `
        -replace '(?m)^host\s+all\s+all\s+127\.0\.0\.1/32\s+\S+\s*$', 'host    all             all             127.0.0.1/32            trust' `
        -replace '(?m)^host\s+all\s+all\s+::1/128\s+\S+\s*$', 'host    all             all             ::1/128                 trust'

    Set-Content -Path $script:PgHbaPath -Value $patched -NoNewline
    & $script:PgCtlExe reload -D $script:Installation.DataDir | Out-Null
    Start-Sleep -Seconds 1
}

function Restore-AuthConfig {
    if ($null -ne $script:PgHbaOriginal) {
        Set-Content -Path $script:PgHbaPath -Value $script:PgHbaOriginal -NoNewline
        & $script:PgCtlExe reload -D $script:Installation.DataDir | Out-Null
        Start-Sleep -Seconds 1
    }
}

function Ensure-PostgresRole {
    if ($Username -notmatch '^[A-Za-z0-9_]+$') {
        throw "Database username '$Username' is not supported by this setup file."
    }

    $userLiteral = $Username.Replace("'", "''")
    $passwordLiteral = $Password.Replace("'", "''")
    $sql = @'
DO $role_fix$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '__USERNAME__') THEN
        EXECUTE format(
            'ALTER ROLE %I WITH LOGIN SUPERUSER CREATEDB CREATEROLE PASSWORD %L',
            '__USERNAME__',
            '__PASSWORD__');
    ELSE
        EXECUTE format(
            'CREATE ROLE %I WITH LOGIN SUPERUSER CREATEDB CREATEROLE PASSWORD %L',
            '__USERNAME__',
            '__PASSWORD__');
    END IF;
END
$role_fix$;
'@
    $sql = $sql.Replace("__USERNAME__", $userLiteral).Replace("__PASSWORD__", $passwordLiteral)

    if (Test-PostgresCredential) {
        [void](Invoke-Psql -DatabaseName "postgres" -Sql $sql)
        return
    }

    Enable-TemporaryTrustAuth
    try {
        [void](Invoke-Psql -DatabaseName "postgres" -Sql $sql -TrustAuth)
    }
    finally {
        Restore-AuthConfig
    }

    if (-not (Test-PostgresCredential)) {
        throw "Failed to verify the PostgreSQL credential for role '$Username'."
    }
}

function Ensure-Database {
    if ($Database -notmatch '^[A-Za-z0-9_]+$') {
        throw "Database name '$Database' is not supported by this setup file."
    }

    if ($ResetDatabase) {
        Write-Step "Recreating PostgreSQL database"
        [void](Invoke-Psql -DatabaseName "postgres" -Sql "DROP DATABASE IF EXISTS $Database WITH (FORCE);")
        [void](Invoke-Psql -DatabaseName "postgres" -Sql "CREATE DATABASE $Database OWNER $Username;")
        return
    }

    Write-Step "Creating PostgreSQL database if missing"
    $exists = Invoke-Psql -DatabaseName "postgres" -Sql "SELECT 1 FROM pg_database WHERE datname = '$Database';"
    if ($exists.Trim() -ne "1") {
        [void](Invoke-Psql -DatabaseName "postgres" -Sql "CREATE DATABASE $Database OWNER $Username;")
    }
}

function Get-RuntimeIdentifier {
    switch ($Platform) {
        "x64" { return "win-x64" }
        "x86" { return "win-x86" }
        "ARM64" { return "win-arm64" }
    }
}

$repoRoot = $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = (Get-Location).Path
}

Push-Location $repoRoot
try {
    Write-Step "Preparing MyShop Gaming Accessories POS setup"

    Ensure-DotNetSdk
    Ensure-WindowsAppRuntime

    Write-Step "Checking PostgreSQL"
    $script:DbHost = $DbHost
    $script:Port = $Port
    $script:Username = $Username
    $script:Password = $Password
    $script:PgHbaOriginal = $null
    $script:Installation = Get-PostgresInstallation
    $script:PsqlExe = Join-Path $script:Installation.BinDir "psql.exe"
    $script:PgCtlExe = Join-Path $script:Installation.BinDir "pg_ctl.exe"
    $script:PgHbaPath = Join-Path $script:Installation.DataDir "pg_hba.conf"
    Write-Ok "PostgreSQL service '$($script:Installation.ServiceName)' is running"

    Write-Step "Configuring PostgreSQL role and database"
    Ensure-PostgresRole
    Ensure-Database

    $connectionString = "Host=$DbHost;Port=$Port;Database=$Database;Username=$Username;Password=$Password;Include Error Detail=true"
    $env:MYSHOP_CONNECTION_STRING = $connectionString
    [Environment]::SetEnvironmentVariable("MYSHOP_CONNECTION_STRING", $connectionString, "User")

    Write-Step "Restoring .NET dependencies"
    Invoke-External "dotnet" @("restore", ".\ProjectTest.csproj")
    Invoke-External "dotnet" @("tool", "restore")

    Write-Step "Applying database migrations and seed data"
    Invoke-External "dotnet" @("run", "--project", ".\tools\DatabaseRebuilder\DatabaseRebuilder.csproj", "--", $connectionString)

    Write-Step "Building application"
    Invoke-External "dotnet" @("build", ".\ProjectTest.csproj", "-c", $Configuration, "-p:Platform=$Platform")

    $runtimeIdentifier = Get-RuntimeIdentifier
    $publishDir = Join-Path $repoRoot "submission\ProjectTest-$runtimeIdentifier"

    if (-not $SkipPublish) {
        Write-Step "Publishing application to $publishDir"
        Invoke-External "dotnet" @(
            "publish",
            ".\ProjectTest.csproj",
            "-c", $Configuration,
            "-p:Platform=$Platform",
            "-p:RuntimeIdentifier=$runtimeIdentifier",
            "-p:SelfContained=true",
            "-p:PublishSingleFile=false",
            "-p:PublishDir=$publishDir\"
        )

        $runCmdPath = Join-Path $publishDir "Run-ProjectTest.cmd"
        $runCmd = @(
            "@echo off",
            "cd /d %~dp0",
            'start "ProjectTest" "%~dp0ProjectTest.exe"'
        ) -join [Environment]::NewLine
        Set-Content -Path $runCmdPath -Value $runCmd -Encoding ASCII
        Write-Ok "Published app and launcher: $runCmdPath"
    }

    if ($RunAfterSetup) {
        $exePath = Join-Path $publishDir "ProjectTest.exe"
        if (-not (Test-Path $exePath)) {
            $exePath = Join-Path $env:LOCALAPPDATA "ProjectTest\artifacts\bin\$Platform\$Configuration\net8.0-windows10.0.19041.0\$runtimeIdentifier\ProjectTest.exe"
        }

        if (-not (Test-Path $exePath)) {
            throw "The app executable was not found. Build/publish completed, but setup could not auto-run the app."
        }

        Write-Step "Starting application"
        Start-Process -FilePath $exePath
    }

    Write-Host ""
    Write-Host "Setup completed." -ForegroundColor Green
    Write-Host "Default login: admin / MyShop123!"
    if (-not $SkipPublish) {
        Write-Host "To run the submitted app later, open: $publishDir\Run-ProjectTest.cmd"
    }
}
finally {
    Pop-Location
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}
