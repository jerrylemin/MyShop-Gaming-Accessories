[CmdletBinding()]
param(
    [string]$ConnectionString,
    [string]$OutputPath,
    [string]$LogPath
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repoRoot 'installer\database\myshop_demo.dump'
}
if ([string]::IsNullOrWhiteSpace($LogPath)) {
    $LogPath = Join-Path $repoRoot 'installer\database\export-demo-database.log'
}

function Write-ExportLog {
    param([string]$Message)
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $LogPath) | Out-Null
    Add-Content -Path $LogPath -Value "[$([DateTimeOffset]::Now.ToString('O'))] $Message"
}

function Read-JsonConnectionString {
    $candidates = @(
        (Join-Path $repoRoot 'myshop.database.json'),
        (Join-Path $repoRoot 'installer\staging\app\myshop.database.json'),
        (Join-Path $env:ProgramFiles 'MyShop POS\myshop.database.json'),
        (Join-Path ${env:ProgramFiles(x86)} 'MyShop POS\myshop.database.json')
    )

    foreach ($candidate in $candidates) {
        if (-not (Test-Path -LiteralPath $candidate)) {
            continue
        }

        try {
            $json = Get-Content -LiteralPath $candidate -Raw | ConvertFrom-Json
            if (-not [string]::IsNullOrWhiteSpace($json.ConnectionString)) {
                Write-ExportLog "Using connection string from $candidate."
                return [string]$json.ConnectionString
            }
        }
        catch {
            Write-ExportLog "Could not read ${candidate}: $($_.Exception.Message)"
        }
    }

    return $null
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

try {
    Write-ExportLog 'Starting PostgreSQL custom-format demo database export.'

    if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
        $ConnectionString = $env:MYSHOP_CONNECTION_STRING
    }
    if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
        $ConnectionString = Read-JsonConnectionString
    }
    if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
        throw 'No connection string found. Set MYSHOP_CONNECTION_STRING or create myshop.database.json.'
    }

    $pgDump = Find-PostgresTool -Name 'pg_dump'
    if ([string]::IsNullOrWhiteSpace($pgDump)) {
        throw 'pg_dump was not found. Install PostgreSQL client tools or add pg_dump.exe to PATH.'
    }

    $parts = ConvertTo-ConnectionParts -Value $ConnectionString
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputPath) | Out-Null
    Remove-Item -LiteralPath $OutputPath -Force -ErrorAction SilentlyContinue

    $env:PGPASSWORD = $parts.Password
    $stdoutPath = [System.IO.Path]::GetTempFileName()
    $stderrPath = [System.IO.Path]::GetTempFileName()
    try {
        $arguments = @(
            '-h', $parts.Host,
            '-p', [string]$parts.Port,
            '-U', $parts.Username,
            '-d', $parts.Database,
            '-Fc',
            '-f', $OutputPath
        )

        Write-ExportLog "Running pg_dump -Fc for database '$($parts.Database)' on $($parts.Host):$($parts.Port)."
        & $pgDump @arguments > $stdoutPath 2> $stderrPath
        $exitCode = $LASTEXITCODE
        $stdout = if (Test-Path $stdoutPath) { Get-Content $stdoutPath -Raw } else { '' }
        $stderr = if (Test-Path $stderrPath) { Get-Content $stderrPath -Raw } else { '' }
        if (-not [string]::IsNullOrWhiteSpace($stdout)) { Write-ExportLog $stdout.Trim() }
        if (-not [string]::IsNullOrWhiteSpace($stderr)) { Write-ExportLog $stderr.Trim() }
        if ($exitCode -ne 0) {
            throw "pg_dump failed with exit code $exitCode."
        }
    }
    finally {
        Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
        Remove-Item $stdoutPath, $stderrPath -ErrorAction SilentlyContinue
    }

    $dump = Get-Item -LiteralPath $OutputPath -ErrorAction Stop
    if ($dump.Length -le 0) {
        throw "Dump was created but is empty: $OutputPath"
    }

    Write-ExportLog "Export completed: $($dump.FullName), $($dump.Length) bytes."
    Write-Host "Exported demo database dump: $($dump.FullName)"
}
catch {
    Write-ExportLog "ERROR: $($_.Exception.Message)"
    throw
}
