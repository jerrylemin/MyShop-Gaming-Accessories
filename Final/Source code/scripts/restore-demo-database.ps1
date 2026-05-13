[CmdletBinding()]
param(
    [string]$TargetConnectionString,
    [string]$AdminConnectionString,
    [string]$DumpPath,
    [string]$LogPath,
    [string]$FallbackDatabaseTool,
    [string]$AppDir,
    [switch]$ForceSeedFallback
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($DumpPath)) {
    $DumpPath = Join-Path $repoRoot 'installer\database\myshop_demo.dump'
}
if ([string]::IsNullOrWhiteSpace($LogPath)) {
    $LogPath = Join-Path $repoRoot 'installer\database\restore-demo-database.log'
}

function Write-RestoreLog {
    param([string]$Message)
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $LogPath) | Out-Null
    Add-Content -Path $LogPath -Value "[$([DateTimeOffset]::Now.ToString('O'))] $Message"
}

function ConvertTo-ConnectionParts {
    param([Parameter(Mandatory = $true)][string]$Value)

    $parts = @{}
    foreach ($segment in $Value.Split(';', [StringSplitOptions]::RemoveEmptyEntries)) {
        $index = $segment.IndexOf('=')
        if ($index -le 0) {
            continue
        }

        $key = $segment.Substring(0, $index).Trim()
        $val = $segment.Substring($index + 1).Trim()
        $parts[$key] = $val
    }

    [pscustomobject]@{
        Host = ($parts['Host'], $parts['Server'], 'localhost' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1)
        Port = [int](($parts['Port'], '5432' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1))
        Database = ($parts['Database'], 'myshop_gaming_accessories' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1)
        Username = ($parts['Username'], $parts['User ID'], $parts['User'], 'postgres' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1)
        Password = ($parts['Password'], $parts['Pwd'] | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1)
    }
}

function ConvertTo-ConnectionString {
    param($Parts, [string]$Database)
    return "Host=$($Parts.Host);Port=$($Parts.Port);Database=$Database;Username=$($Parts.Username);Password=$($Parts.Password);Include Error Detail=true"
}

function Find-PostgresTool {
    param([string]$Name)

    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $roots = @(
        (Join-Path $env:ProgramFiles 'PostgreSQL'),
        (Join-Path ${env:ProgramFiles(x86)} 'PostgreSQL')
    )

    foreach ($root in $roots) {
        if (-not (Test-Path -LiteralPath $root)) {
            continue
        }

        $match = Get-ChildItem -LiteralPath $root -Recurse -Filter "$Name.exe" -ErrorAction SilentlyContinue |
            Sort-Object FullName -Descending |
            Select-Object -First 1
        if ($match) {
            return $match.FullName
        }
    }

    return $null
}

function Invoke-Psql {
    param(
        [Parameter(Mandatory = $true)]$Connection,
        [Parameter(Mandatory = $true)][string]$Database,
        [Parameter(Mandatory = $true)][string]$Sql,
        [Parameter(Mandatory = $true)][string]$Psql
    )

    $sqlPath = [System.IO.Path]::GetTempFileName()
    $stdoutPath = [System.IO.Path]::GetTempFileName()
    $stderrPath = [System.IO.Path]::GetTempFileName()
    try {
        Set-Content -Path $sqlPath -Value $Sql -Encoding UTF8 -NoNewline
        $env:PGPASSWORD = $Connection.Password
        $arguments = @(
            '-h', $Connection.Host,
            '-p', [string]$Connection.Port,
            '-U', $Connection.Username,
            '-d', $Database,
            '-v', 'ON_ERROR_STOP=1',
            '-f', $sqlPath
        )
        & $Psql @arguments > $stdoutPath 2> $stderrPath
        $exitCode = $LASTEXITCODE
        $stdout = if (Test-Path $stdoutPath) { Get-Content $stdoutPath -Raw } else { '' }
        $stderr = if (Test-Path $stderrPath) { Get-Content $stderrPath -Raw } else { '' }
        if (-not [string]::IsNullOrWhiteSpace($stdout)) { Write-RestoreLog $stdout.Trim() }
        if (-not [string]::IsNullOrWhiteSpace($stderr)) { Write-RestoreLog $stderr.Trim() }
        if ($exitCode -ne 0) {
            throw "psql failed with exit code ${exitCode}: $stderr"
        }
    }
    finally {
        Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
        Remove-Item $sqlPath, $stdoutPath, $stderrPath -ErrorAction SilentlyContinue
    }
}

function Invoke-SeedFallback {
    param([string]$Reason)

    Write-RestoreLog "Falling back to EF migration/seed. Reason: $Reason"
    if ([string]::IsNullOrWhiteSpace($FallbackDatabaseTool) -or -not (Test-Path -LiteralPath $FallbackDatabaseTool)) {
        throw "Cannot run seed fallback because database bootstrapper is missing. $Reason"
    }

    $args = @(
        '--connection-string', $TargetConnectionString,
        '--app-dir', $AppDir,
        '--log', $LogPath
    )
    $process = Start-Process -FilePath $FallbackDatabaseTool -ArgumentList $args -Wait -PassThru -NoNewWindow
    Write-RestoreLog "Seed fallback exit code: $($process.ExitCode)"
    if ($process.ExitCode -ne 0) {
        throw "Seed fallback failed with exit code $($process.ExitCode)."
    }
}

try {
    Write-RestoreLog 'Starting demo database restore.'

    if ([string]::IsNullOrWhiteSpace($TargetConnectionString)) {
        $TargetConnectionString = $env:MYSHOP_CONNECTION_STRING
    }
    if ([string]::IsNullOrWhiteSpace($TargetConnectionString)) {
        throw 'Target connection string is missing. Pass -TargetConnectionString or set MYSHOP_CONNECTION_STRING.'
    }
    if ([string]::IsNullOrWhiteSpace($AppDir)) {
        $AppDir = $repoRoot
    }

    if ($ForceSeedFallback) {
        Invoke-SeedFallback -Reason 'ForceSeedFallback was requested.'
        return
    }
    if (-not (Test-Path -LiteralPath $DumpPath)) {
        Invoke-SeedFallback -Reason "Dump file is missing: $DumpPath"
        return
    }
    if ((Get-Item -LiteralPath $DumpPath).Length -le 0) {
        Invoke-SeedFallback -Reason "Dump file is empty: $DumpPath"
        return
    }

    $psql = Find-PostgresTool -Name 'psql'
    $pgRestore = Find-PostgresTool -Name 'pg_restore'
    $pgDump = Find-PostgresTool -Name 'pg_dump'
    if ([string]::IsNullOrWhiteSpace($psql) -or [string]::IsNullOrWhiteSpace($pgRestore)) {
        Invoke-SeedFallback -Reason 'psql or pg_restore was not found.'
        return
    }

    $target = ConvertTo-ConnectionParts -Value $TargetConnectionString
    $admin = if ([string]::IsNullOrWhiteSpace($AdminConnectionString)) {
        [pscustomobject]@{
            Host = $target.Host
            Port = $target.Port
            Database = 'postgres'
            Username = 'postgres'
            Password = $env:MYSHOP_POSTGRES_ADMIN_PASSWORD
        }
    }
    else {
        ConvertTo-ConnectionParts -Value $AdminConnectionString
    }

    if ([string]::IsNullOrWhiteSpace($admin.Password)) {
        Invoke-SeedFallback -Reason 'PostgreSQL admin password was not provided in MYSHOP_POSTGRES_ADMIN_PASSWORD or -AdminConnectionString.'
        return
    }

    $roleName = $target.Username.Replace('"', '""')
    $rolePassword = $target.Password.Replace("'", "''")
    $databaseName = $target.Database.Replace('"', '""')
    $ensureSql = @"
DO `$`$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '$($target.Username.Replace("'", "''"))') THEN
        EXECUTE format('ALTER ROLE %I WITH LOGIN PASSWORD %L', '$roleName', '$rolePassword');
    ELSE
        EXECUTE format('CREATE ROLE %I WITH LOGIN PASSWORD %L', '$roleName', '$rolePassword');
    END IF;
END
`$`$;
SELECT 'CREATE DATABASE "$databaseName" OWNER "$roleName" ENCODING ''UTF8'''
WHERE NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = '$($target.Database.Replace("'", "''"))')\gexec
GRANT ALL PRIVILEGES ON DATABASE "$databaseName" TO "$roleName";
"@
    Invoke-Psql -Connection $admin -Database 'postgres' -Sql $ensureSql -Psql $psql

    if ($pgDump) {
        $backupPath = Join-Path (Split-Path -Parent $LogPath) ("pre-restore-backup-{0:yyyyMMdd-HHmmss}.dump" -f (Get-Date))
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $backupPath) | Out-Null
        $backupStdErr = [System.IO.Path]::GetTempFileName()
        $env:PGPASSWORD = $admin.Password
        try {
            $backupArgs = @('-h', $admin.Host, '-p', [string]$admin.Port, '-U', $admin.Username, '-d', $target.Database, '-Fc', '-f', $backupPath)
            & $pgDump @backupArgs > $null 2> $backupStdErr
            $backupExitCode = $LASTEXITCODE
            $backupError = if (Test-Path $backupStdErr) { Get-Content $backupStdErr -Raw } else { '' }
            if (-not [string]::IsNullOrWhiteSpace($backupError)) {
                Write-RestoreLog $backupError.Trim()
            }
            Write-RestoreLog "Pre-restore backup attempted: $backupPath exit code $backupExitCode"
        }
        finally {
            Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
            Remove-Item $backupStdErr -ErrorAction SilentlyContinue
        }
    }
    else {
        Write-RestoreLog 'pg_dump was not found; restoring over demo database without pre-restore backup.'
    }

    $resetSql = @"
DROP SCHEMA IF EXISTS public CASCADE;
CREATE SCHEMA public AUTHORIZATION "$roleName";
GRANT ALL ON SCHEMA public TO "$roleName";
GRANT ALL ON SCHEMA public TO public;
"@
    Invoke-Psql -Connection $admin -Database $target.Database -Sql $resetSql -Psql $psql

    $env:PGPASSWORD = $admin.Password
    $stdoutPath = [System.IO.Path]::GetTempFileName()
    $stderrPath = [System.IO.Path]::GetTempFileName()
    try {
        $restoreArgs = @(
            '-h', $admin.Host,
            '-p', [string]$admin.Port,
            '-U', $admin.Username,
            '-d', $target.Database,
            '--no-owner',
            '--role', $target.Username,
            $DumpPath
        )
        Write-RestoreLog "Running pg_restore for $DumpPath."
        & $pgRestore @restoreArgs > $stdoutPath 2> $stderrPath
        $exitCode = $LASTEXITCODE
        $stdout = if (Test-Path $stdoutPath) { Get-Content $stdoutPath -Raw } else { '' }
        $stderr = if (Test-Path $stderrPath) { Get-Content $stderrPath -Raw } else { '' }
        if (-not [string]::IsNullOrWhiteSpace($stdout)) { Write-RestoreLog $stdout.Trim() }
        if (-not [string]::IsNullOrWhiteSpace($stderr)) { Write-RestoreLog $stderr.Trim() }
        if ($exitCode -ne 0) {
            throw "pg_restore failed with exit code ${exitCode}: $stderr"
        }
    }
    finally {
        Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
        Remove-Item $stdoutPath, $stderrPath -ErrorAction SilentlyContinue
    }

    $grantSql = @"
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO "$roleName";
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO "$roleName";
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO "$roleName";
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO "$roleName";
"@
    Invoke-Psql -Connection $admin -Database $target.Database -Sql $grantSql -Psql $psql

    $verifyTables = @(
        @{ Label = 'Categories'; Name = 'categories' },
        @{ Label = 'Products'; Name = 'products' },
        @{ Label = 'Orders'; Name = 'orders' },
        @{ Label = 'OrderItems'; Name = 'order_items' },
        @{ Label = 'Customers'; Name = 'customers' },
        @{ Label = 'Promotions'; Name = 'promotions' },
        @{ Label = 'Users'; Name = 'users' },
        @{ Label = 'CustomerLoyaltyTransactions'; Name = 'customer_loyalty_transactions'; Optional = $true }
    )
    foreach ($table in $verifyTables) {
        $sql = "SELECT COUNT(*) FROM public.$($table.Name);"
        try {
            Invoke-Psql -Connection $target -Database $target.Database -Sql $sql -Psql $psql
            Write-RestoreLog "Verified table $($table.Label) ($($table.Name))."
        }
        catch {
            if ($table.Optional) {
                Write-RestoreLog "Optional table $($table.Label) was not verified: $($_.Exception.Message)"
            }
            else {
                throw
            }
        }
    }

    $configPath = Join-Path $AppDir 'myshop.database.json'
    New-Item -ItemType Directory -Force -Path $AppDir | Out-Null
    @{ ConnectionString = $TargetConnectionString } | ConvertTo-Json | Set-Content -Path $configPath -Encoding UTF8
    Write-RestoreLog "Restore completed and app config written to $configPath."
}
catch {
    Write-RestoreLog "ERROR: $($_.Exception.Message)"
    throw
}
