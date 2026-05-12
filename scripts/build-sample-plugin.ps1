$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$pluginProject = Join-Path $repoRoot "plugins\SampleMyShopPlugin\SampleMyShopPlugin.csproj"
$pluginOut = Join-Path $repoRoot "plugins\SampleMyShopPlugin\bin\x64\Release\net8.0-windows10.0.19041.0"
$appPlugins = Join-Path $repoRoot "Plugins\SampleMyShopPlugin"

dotnet build $pluginProject -c Release -p:Platform=x64 -p:PublishTrimmed=false
New-Item -ItemType Directory -Force -Path $appPlugins | Out-Null
Copy-Item -Force (Join-Path $pluginOut "SampleMyShopPlugin.dll") $appPlugins
Copy-Item -Force (Join-Path $repoRoot "plugins\SampleMyShopPlugin\plugin.json") $appPlugins

Write-Host "Sample plugin copied to $appPlugins"
