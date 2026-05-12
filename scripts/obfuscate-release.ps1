param(
    [string]$Platform = "x64"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "ProjectTest.csproj"
$config = Join-Path $repoRoot "obfuscar.xml"

Set-Location $repoRoot

dotnet build $project -c Release -p:Platform=$Platform -p:PublishTrimmed=false

try {
    dotnet tool restore
}
catch {
    Write-Host "dotnet tool restore failed. If Windows blocked dotnet-tools.json, run:"
    Write-Host "  Unblock-File .\dotnet-tools.json"
    throw
}

dotnet obfuscar.console $config

Write-Host "Obfuscated output:"
Write-Host "  C:\Users\Administrator\AppData\Local\ProjectTest\artifacts\obfuscated\$Platform\Release"
