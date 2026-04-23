param(
    [string]$DbHost = "localhost",
    [int]$Port = 5432,
    [string]$Database = "myshop_gaming_accessories",
    [string]$Username = "postgres",
    [string]$Password = "jelly"
)

$ErrorActionPreference = "Stop"

function Get-PostgresInstallation {
    $service = Get-CimInstance Win32_Service |
        Where-Object { $_.Name -like "postgresql-x64-*" } |
        Sort-Object Name -Descending |
        Select-Object -First 1

    if (-not $service) {
        throw "No PostgreSQL Windows service was found."
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

    $output = & $script:PsqlExe `
        -h $script:DbHost `
        -p $script:Port `
        -U $script:Username `
        -d $DatabaseName `
        -v ON_ERROR_STOP=1 `
        -tAc $Sql 2>&1

    if ($LASTEXITCODE -ne 0) {
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
        throw "Failed to verify the repaired PostgreSQL credential for $Username."
    }
}

function Reset-Database {
    if ($Database -notmatch '^[A-Za-z0-9_]+$') {
        throw "Database name '$Database' is not supported by this script."
    }

    [void](Invoke-Psql -DatabaseName "postgres" -Sql "DROP DATABASE IF EXISTS $Database WITH (FORCE);")
    [void](Invoke-Psql -DatabaseName "postgres" -Sql "CREATE DATABASE $Database OWNER $Username;")
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$script:DbHost = $DbHost
$script:Port = $Port
$script:Username = $Username
$script:Password = $Password
$script:PgHbaOriginal = $null
$script:Installation = Get-PostgresInstallation
$script:PsqlExe = Join-Path $script:Installation.BinDir "psql.exe"
$script:PgCtlExe = Join-Path $script:Installation.BinDir "pg_ctl.exe"
$script:PgHbaPath = Join-Path $script:Installation.DataDir "pg_hba.conf"
$connectionString = "Host=$DbHost;Port=$Port;Database=$Database;Username=$Username;Password=$Password;Include Error Detail=true"

Write-Host "Ensuring PostgreSQL role $Username exists with the requested password..."
Ensure-PostgresRole

Write-Host "Recreating database $Database..."
Reset-Database

Write-Host "Restoring local .NET tools..."
Push-Location $repoRoot
try {
    dotnet tool restore | Out-Host

    Write-Host "Applying migrations and seeding data..."
    dotnet run --project tools\DatabaseRebuilder\DatabaseRebuilder.csproj -- $connectionString | Out-Host
}
finally {
    Pop-Location
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}

$verificationSql = @"
SELECT
    (SELECT COUNT(*) FROM categories) AS categories,
    (SELECT COUNT(*) FROM products) AS products,
    (SELECT COUNT(*) FROM orders) AS orders,
    (SELECT COUNT(*) FROM order_items) AS order_items;
"@

$verification = Invoke-Psql -DatabaseName $Database -Sql $verificationSql
Write-Host "Verification:"
Write-Host $verification
