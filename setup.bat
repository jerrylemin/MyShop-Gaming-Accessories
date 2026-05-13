@echo off
setlocal

set "MYSHOP_SETUP_BAT=%~f0"
set "MYSHOP_SETUP_ROOT=%~dp0"

net session >nul 2>&1
if not "%errorlevel%"=="0" (
    echo Requesting administrator permission...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%ComSpec%' -ArgumentList '/c ""%MYSHOP_SETUP_BAT%""' -Verb RunAs"
    exit /b
)

powershell -NoProfile -ExecutionPolicy Bypass -Command "$bat=$env:MYSHOP_SETUP_BAT; $text=[System.IO.File]::ReadAllText($bat); $parts=[regex]::Split($text,'\r?\n# POWERSHELL_START\r?\n',2); if ($parts.Count -lt 2) { throw 'PowerShell payload was not found in setup.bat.' }; Invoke-Expression $parts[1]"
set "MYSHOP_SETUP_EXIT=%errorlevel%"

if not "%MYSHOP_SETUP_EXIT%"=="0" (
    echo.
    echo setup.bat failed with exit code %MYSHOP_SETUP_EXIT%.
    pause
    exit /b %MYSHOP_SETUP_EXIT%
)

echo.
echo setup.bat completed successfully.
pause
exit /b 0

# POWERSHELL_START
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Write-SetupStep {
    param([string]$Message)
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Write-SetupOk {
    param([string]$Message)
    Write-Host "OK: $Message" -ForegroundColor Green
}

$repoRoot = $env:MYSHOP_SETUP_ROOT
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path -Parent $env:MYSHOP_SETUP_BAT
}

$repoRoot = (Resolve-Path -LiteralPath $repoRoot).Path
$setupScript = Join-Path $repoRoot "setup.ps1"
$runtimeIdentifier = "win-x64"
$publishDir = Join-Path $repoRoot "submission\ProjectTest-$runtimeIdentifier"
$exePath = Join-Path $publishDir "ProjectTest.exe"
$cmdPath = Join-Path $publishDir "Run-ProjectTest.cmd"
$desktopShortcut = Join-Path ([Environment]::GetFolderPath("Desktop")) "MyShop Gaming Accessories POS.lnk"
$logDir = Join-Path $repoRoot "installer\logs"
$logPath = Join-Path $logDir ("setup-bat-" + (Get-Date -Format "yyyyMMdd-HHmmss") + ".log")

New-Item -ItemType Directory -Path $logDir -Force | Out-Null

Write-SetupStep "Starting MyShop one-click setup"
Write-Host "Root: $repoRoot"
Write-Host "Log:  $logPath"

if (-not (Test-Path -LiteralPath $setupScript)) {
    throw "setup.ps1 was not found at $setupScript"
}

Push-Location $repoRoot
try {
    Write-SetupStep "Installing prerequisites, configuring PostgreSQL database, building and publishing app"
    Write-Host "This can take 5-20 minutes depending on .NET SDK, Windows App Runtime, PostgreSQL and antivirus state."

    $setupArgs = @(
        "-ExecutionPolicy", "Bypass",
        "-File", $setupScript,
        "-InstallPrerequisites",
        "-Configuration", "Release",
        "-Platform", "x64"
    )

    & powershell.exe @setupArgs 2>&1 | Tee-Object -FilePath $logPath
    if ($LASTEXITCODE -ne 0) {
        throw "setup.ps1 failed with exit code $LASTEXITCODE. See log: $logPath"
    }

    if (-not (Test-Path -LiteralPath $exePath)) {
        throw "Published app executable was not found: $exePath"
    }

    Write-SetupStep "Creating Desktop shortcut"
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($desktopShortcut)
    $shortcut.TargetPath = $exePath
    $shortcut.WorkingDirectory = $publishDir
    $shortcut.IconLocation = $exePath
    $shortcut.Description = "MyShop Gaming Accessories POS"
    $shortcut.Save()

    if (-not (Test-Path -LiteralPath $cmdPath)) {
        $runCmd = @(
            "@echo off",
            "cd /d %~dp0",
            'start "ProjectTest" "%~dp0ProjectTest.exe"'
        ) -join [Environment]::NewLine
        Set-Content -LiteralPath $cmdPath -Value $runCmd -Encoding ASCII
    }

    Write-SetupOk "Database was installed/configured by setup.ps1"
    Write-SetupOk "App executable: $exePath"
    Write-SetupOk "Desktop shortcut: $desktopShortcut"
    Write-Host ""
    Write-Host "Default login accounts:" -ForegroundColor Yellow
    Write-Host "  admin / MyShop123!"
    Write-Host "  moderator / MyShop123!"
    Write-Host "  sale / MyShop123!"
}
finally {
    Pop-Location
}
