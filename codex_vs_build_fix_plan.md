# Visual Studio Build Fix Plan

Updated: 2026-05-13

## Implemented

- Added `global.json` pinned to SDK `8.0.404` with `rollForward: latestFeature`.
- Installed .NET SDK `8.0.420` on this machine so the pin resolves to stable .NET 8 instead of SDK 10 preview.
- Added Debug-only build settings in `Directory.Build.props`:
  - analyzers disabled during build/live analysis
  - documentation generation disabled
  - portable debug symbols
  - trimming, ReadyToRun, and app self-contained publish disabled for Debug
  - `WindowsAppSDKSelfContained` is intentionally not forced in Debug so Visual Studio/MSIX targets can choose the correct WinUI dependency probing behavior
- Updated `ProjectTest.csproj`:
  - maps empty/AnyCPU platform to x64
  - excludes `docs`, `scripts`, `tools`, `installer`, `ProjectTest.Tests`, `plugins`, package output folders, and `Assets/_source_product_images` from app item scanning/package inputs
  - keeps runtime logos and `Assets/GamingProducts`
  - keeps Debug package bundle disabled with `AppxBundle=Never`
- Updated `Properties/launchSettings.json`:
  - kept `ProjectTest (Package)`
  - added `ProjectTest (Unpackaged Fast)` for fast development debugging
  - kept the existing generic unpackaged profile
- Updated `ProjectTest.Tests.csproj`:
  - maps empty/AnyCPU platform to x64
  - passes package-disabled properties to the app `ProjectReference` so logic tests do not generate MSIX package artifacts.

## Use These Profiles

- Fast coding/debugging: `ProjectTest (Unpackaged Fast)`.
- Packaging/MSIX verification: `ProjectTest (Package)`.

## Commands

Fast Debug build:

```powershell
dotnet build ProjectTest.csproj -c Debug -p:Platform=x64
```

Warm incremental Debug build:

```powershell
dotnet build ProjectTest.csproj -c Debug -p:Platform=x64 --no-restore
```

Release build:

```powershell
dotnet build ProjectTest.csproj -c Release -p:Platform=x64
```

Release package/publish:

```powershell
dotnet publish ProjectTest.csproj -c Release -p:Platform=x64 -p:PublishProfile=win-x64.pubxml
```

MSIX package check:

```powershell
dotnet publish ProjectTest.csproj -c Debug -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true -p:AppxPackageDir=AppPackages\ -p:AppxBundle=Never
```

## Notes

- Stable .NET 8 CLI does not support `dotnet test ProjectTest.slnx`; run `dotnet test ProjectTest.Tests\ProjectTest.Tests.csproj -p:Platform=x64` for CLI validation.
- Visual Studio can still open/use `ProjectTest.slnx`; the packaged profile remains available for MSIX checks.
- If Visual Studio reports `System.IO.FileNotFoundException` for `Microsoft.WinUI, Version=3.0.0.0`, clean the project once and rebuild after this change. The previous Debug-only `WindowsAppSDKSelfContained=false` override has been removed because it can interfere with Windows App SDK dependency layout/probing in VS launch profiles.
