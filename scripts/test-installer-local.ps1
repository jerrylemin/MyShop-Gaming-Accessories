[CmdletBinding()]
param(
    [string]$SetupPath,
    [switch]$Silent,
    [string]$InstallDir = "$env:ProgramFiles\MyShop POS",
    [string]$ConnectionString
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$logPath = Join-Path $repoRoot 'installer-test.log'

function Write-TestLog {
    param([string]$Message)
    Add-Content -Path $logPath -Value "[$([DateTimeOffset]::Now.ToString('O'))] $Message"
}

function Find-Setup {
    if (-not [string]::IsNullOrWhiteSpace($SetupPath) -and (Test-Path -LiteralPath $SetupPath)) {
        return (Resolve-Path -LiteralPath $SetupPath).Path
    }

    $match = Get-ChildItem -Path $repoRoot -Recurse -Filter setup.exe -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if ($match) {
        return $match.FullName
    }

    throw 'setup.exe was not found. Build the installer first.'
}

function ConvertTo-ConnectionParts {
    param([string]$Value)
    $parts = @{}
    foreach ($segment in $Value.Split(';', [StringSplitOptions]::RemoveEmptyEntries)) {
        $index = $segment.IndexOf('=')
        if ($index -le 0) { continue }
        $parts[$segment.Substring(0, $index).Trim()] = $segment.Substring($index + 1).Trim()
    }

    [pscustomobject]@{
        Host = ($parts['Host'], 'localhost' | Where-Object { $_ } | Select-Object -First 1)
        Port = [int](($parts['Port'], '5432' | Where-Object { $_ } | Select-Object -First 1))
        Database = ($parts['Database'], 'myshop_gaming_accessories' | Where-Object { $_ } | Select-Object -First 1)
        Username = ($parts['Username'], $parts['User ID'], 'myshop_app' | Where-Object { $_ } | Select-Object -First 1)
        Password = ($parts['Password'], $parts['Pwd'] | Where-Object { $_ } | Select-Object -First 1)
    }
}

function Find-PostgresTool {
    param([string]$Name)

    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    foreach ($root in @((Join-Path $env:ProgramFiles 'PostgreSQL'), (Join-Path ${env:ProgramFiles(x86)} 'PostgreSQL'))) {
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

$setup = Find-Setup
Write-TestLog "Using setup: $setup"
$arguments = if ($Silent) { '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART' } else { '' }
$process = Start-Process -FilePath $setup -ArgumentList $arguments -Wait -PassThru
Write-TestLog "Installer exit code: $($process.ExitCode)"
if ($process.ExitCode -ne 0) {
    throw "Installer failed with exit code $($process.ExitCode). See $logPath."
}

$exePath = Join-Path $InstallDir 'ProjectTest.exe'
if (-not (Test-Path -LiteralPath $exePath)) {
    throw "Installed app exe was not found: $exePath"
}
Write-TestLog "Verified app exe: $exePath"

$desktopShortcutCandidates = @(
    (Join-Path ([Environment]::GetFolderPath('Desktop')) 'MyShop Gaming Accessories POS.lnk'),
    (Join-Path ([Environment]::GetFolderPath('CommonDesktopDirectory')) 'MyShop Gaming Accessories POS.lnk')
)
$desktopShortcut = $desktopShortcutCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($desktopShortcut)) {
    throw "Desktop shortcut was not found. Checked: $($desktopShortcutCandidates -join '; ')"
}
Write-TestLog "Verified desktop shortcut: $desktopShortcut"

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $configPath = Join-Path $InstallDir 'myshop.database.json'
    if (Test-Path -LiteralPath $configPath) {
        $ConnectionString = [string]((Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json).ConnectionString)
    }
    elseif ($env:MYSHOP_CONNECTION_STRING) {
        $ConnectionString = $env:MYSHOP_CONNECTION_STRING
    }
}

if (-not [string]::IsNullOrWhiteSpace($ConnectionString)) {
    $psql = Find-PostgresTool -Name 'psql'
    if ($psql) {
        $parts = ConvertTo-ConnectionParts -Value $ConnectionString
        $env:PGPASSWORD = $parts.Password
        try {
            foreach ($table in @('products', 'customers', 'orders')) {
                $count = & $psql -h $parts.Host -p $parts.Port -U $parts.Username -d $parts.Database -tA -c "SELECT COUNT(*) FROM $table;"
                Write-TestLog "$table count: $count"
            }
        }
        finally {
            Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
        }
    }
    else {
        Write-TestLog 'psql was not found; skipped database count checks.'
    }
}
else {
    Write-TestLog 'No connection string found; skipped database count checks.'
}

Write-Host "Installer local test completed. Log: $logPath"
