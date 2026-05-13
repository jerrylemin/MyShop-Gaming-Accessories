param(
    [switch]$SkipPrerequisiteDownload
)

$ErrorActionPreference = 'Stop'

$installerRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $installerRoot
$stagingRoot = Join-Path $installerRoot 'staging'
$appStaging = Join-Path $stagingRoot 'app'
$databaseStaging = Join-Path $stagingRoot 'database'
$prereqRoot = Join-Path $installerRoot 'prerequisites'
$outputRoot = Join-Path $installerRoot 'output'
$logRoot = Join-Path $installerRoot 'logs'

function Write-Step {
    param([string]$Message)
    Write-Host "==> $Message"
}

function Invoke-RepoCommand {
    param(
        [string]$FilePath,
        [string[]]$Arguments
    )

    Push-Location $repoRoot
    try {
        & $FilePath @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "$FilePath failed with exit code $LASTEXITCODE"
        }
    }
    finally {
        Pop-Location
    }
}

function Get-InnoCompiler {
    $candidates = @(
        (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe'),
        (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe')
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return $candidate
        }
    }

    $command = Get-Command iscc.exe -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $winget = Get-Command winget.exe -ErrorAction SilentlyContinue
    if ($winget) {
        Write-Step 'Inno Setup not found. Installing Inno Setup with winget.'
        $process = Start-Process -FilePath $winget.Source -ArgumentList @(
            'install',
            '--id', 'JRSoftware.InnoSetup',
            '--exact',
            '--silent',
            '--accept-package-agreements',
            '--accept-source-agreements'
        ) -Wait -PassThru -NoNewWindow
        if ($process.ExitCode -ne 0) {
            throw "winget could not install Inno Setup. Exit code: $($process.ExitCode)"
        }

        foreach ($candidate in $candidates) {
            if (Test-Path -LiteralPath $candidate) {
                return $candidate
            }
        }

        $localInstall = Get-ChildItem (Join-Path $env:LOCALAPPDATA 'Programs') -Recurse -Filter ISCC.exe -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -like '*Inno Setup*' } |
            Select-Object -First 1
        if ($localInstall) {
            return $localInstall.FullName
        }
    }

    throw 'Inno Setup 6 was not found. Install Inno Setup 6 or make ISCC.exe available on PATH.'
}

function Save-Download {
    param(
        [string]$Url,
        [string]$OutputPath
    )

    if (Test-Path -LiteralPath $OutputPath) {
        Write-Step "Using cached prerequisite: $OutputPath"
        return
    }

    Write-Step "Downloading $Url"
    Invoke-WebRequest -Uri $Url -OutFile $OutputPath
}

function New-IcoFromPng {
    param(
        [string]$PngPath,
        [string]$IcoPath
    )

    try {
        Add-Type -AssemblyName System.Drawing
        $bitmap = [System.Drawing.Bitmap]::new($PngPath)
        try {
            $resized = [System.Drawing.Bitmap]::new($bitmap, [System.Drawing.Size]::new(256, 256))
            try {
                $iconHandle = $resized.GetHicon()
                try {
                    $icon = [System.Drawing.Icon]::FromHandle($iconHandle)
                    try {
                        $stream = [System.IO.File]::Create($IcoPath)
                        try {
                            $icon.Save($stream)
                        }
                        finally {
                            $stream.Dispose()
                        }
                    }
                    finally {
                        $icon.Dispose()
                    }
                }
                finally {
                    [NativeMethods]::DestroyIcon($iconHandle) | Out-Null
                }
            }
            finally {
                $resized.Dispose()
            }
        }
        finally {
            $bitmap.Dispose()
        }
    }
    catch {
        Write-Warning "Could not create shortcut icon from $PngPath. Shortcuts will use the app executable icon. $($_.Exception.Message)"
    }
}

Add-Type @'
using System;
using System.Runtime.InteropServices;

public static class NativeMethods
{
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyIcon(IntPtr hIcon);
}
'@

New-Item -ItemType Directory -Force -Path $stagingRoot, $appStaging, $databaseStaging, $prereqRoot, $outputRoot, $logRoot | Out-Null

Write-Step 'Restoring project dependencies.'
Invoke-RepoCommand -FilePath 'dotnet' -Arguments @('restore', 'ProjectTest.csproj')

Write-Step 'Publishing MyShop POS app.'
if (Test-Path -LiteralPath $appStaging) {
    Remove-Item -LiteralPath $appStaging -Recurse -Force
}
Invoke-RepoCommand -FilePath 'dotnet' -Arguments @(
    'publish',
    'ProjectTest.csproj',
    '-p:PublishProfile=installer-win-x64',
    '-p:Platform=x64',
    '-p:PublishTrimmed=false'
)

Write-Step 'Preparing shortcut icon.'
New-IcoFromPng -PngPath (Join-Path $appStaging 'Assets\Square150x150Logo.scale-200.png') -IcoPath (Join-Path $appStaging 'MyShop.ico')

Write-Step 'Publishing database bootstrapper.'
if (Test-Path -LiteralPath $databaseStaging) {
    Remove-Item -LiteralPath $databaseStaging -Recurse -Force
}
Invoke-RepoCommand -FilePath 'dotnet' -Arguments @(
    'publish',
    'installer\database\MyShop.DatabaseBootstrapper.csproj',
    '-c', 'Release',
    '-r', 'win-x64',
    '--self-contained', 'true',
    '-o', $databaseStaging
)

if (-not $SkipPrerequisiteDownload) {
    Write-Step 'Preparing prerequisite installers.'
    Save-Download `
        -Url 'https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe' `
        -OutputPath (Join-Path $prereqRoot 'windowsdesktop-runtime-8-win-x64.exe')
    Save-Download `
        -Url 'https://aka.ms/windowsappsdk/1.8/latest/windowsappruntimeinstall-x64.exe' `
        -OutputPath (Join-Path $prereqRoot 'windowsappruntimeinstall-x64.exe')
    Save-Download `
        -Url 'https://get.enterprisedb.com/postgresql/postgresql-18.3-1-windows-x64.exe' `
        -OutputPath (Join-Path $prereqRoot 'postgresql-18-windows-x64.exe')
}

Write-Step 'Compiling setup.exe with Inno Setup.'
$iscc = Get-InnoCompiler
$setupScript = Join-Path $installerRoot 'setup.iss'
Push-Location $installerRoot
try {
    & $iscc $setupScript
    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup failed with exit code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}

$setupExe = Join-Path $outputRoot 'setup.exe'
if (-not (Test-Path -LiteralPath $setupExe)) {
    throw "setup.exe was not created at $setupExe"
}

Write-Step "Created $setupExe"
