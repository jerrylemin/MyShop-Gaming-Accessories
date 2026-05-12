# Visual Studio Build Audit

Updated: 2026-05-13

## Findings

- The machine was resolving this repo with `.NET SDK 10.0.300-preview` because the repo had no `global.json`.
- `ProjectTest.csproj` already targets `net8.0-windows10.0.19041.0`, so the SDK should be pinned to stable .NET 8 for consistent Visual Studio and CLI behavior.
- `Properties/launchSettings.json` had `ProjectTest (Package)` and a generic unpackaged profile, but no clearly named fast profile for normal code/debug loops.
- `Assets/GamingProducts` is required at runtime, but `Assets/_source_product_images` is a duplicate source-image cache. It has 330 files and about 7.56 MB that should not be scanned/copied into app build/package inputs.
- `ProjectTest.Tests` targets .NET 8 already, but test builds could pass `AnyCPU` to the app project and produce invalid package profile noise such as `win-AnyCPU.pubxml`.
- `ProjectTest.slnx` is supported by current Visual Studio/preview SDK tooling, but `dotnet test ProjectTest.slnx` is not supported by the stable .NET 8 CLI installed here. The stable CLI reports `MSB4068` on the XML `.slnx` root.

## Causes Of Slow Debug/Play

- SDK preview resolution added uncertainty and preview SDK startup/restore overhead.
- Debug builds still allowed analyzer/live-analysis/documentation defaults unless explicitly disabled.
- Package Play uses MSIX deploy targets by design; it is slower than an unpackaged project launch.
- Broad default item scanning could include non-runtime folders unless excluded early.
- Test project builds should not trigger package/deploy work when running logic tests.

## Files Audited

- `ProjectTest.csproj`
- `ProjectTest.slnx`
- `Properties/launchSettings.json`
- `Package.appxmanifest`
- `Directory.Build.props`
- `Directory.Build.targets`
- `setup.ps1`
- `README.md`
- `Assets`
- `tools`
- `installer`
- `ProjectTest.Tests`

## Not Changed

- `setup.exe` was not rebuilt.
- `installer` scripts and `.iss` files were not changed.
- `Package.appxmanifest` was not changed.
- Database schema and migrations were not changed.
- Existing `ProjectTest (Package)` profile was kept.
