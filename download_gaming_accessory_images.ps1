param(
    [switch]$DatasetOnly,
    [switch]$ImagesOnly
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$scriptPath = Join-Path $repoRoot "scripts\build_gaming_accessory_seed_assets.py"

if (-not (Test-Path $scriptPath)) {
    throw "The gaming accessories seed builder was not found at $scriptPath."
}

$arguments = @($scriptPath)
if ($DatasetOnly) {
    $arguments += "--dataset-only"
}

if ($ImagesOnly) {
    $arguments += "--images-only"
}

python @arguments
