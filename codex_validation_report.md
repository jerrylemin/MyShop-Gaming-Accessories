# MyShop Full-10 Validation Report

Updated: 2026-05-13

## Visual Studio Debug Build Optimization

- Added `global.json` to pin stable .NET 8 SDK. This machine initially had only SDK `10.0.300-preview`; SDK `8.0.420` was installed with `winget` and is now selected by the repo pin.
- Added `ProjectTest (Unpackaged Fast)` for fast Visual Studio debugging while keeping `ProjectTest (Package)` for MSIX verification.
- Debug builds now disable analyzers, documentation output, trimming, ReadyToRun, and app self-contained publish output. `WindowsAppSDKSelfContained` is intentionally left to the Windows App SDK targets so Visual Studio package/debug launch keeps the correct WinUI dependency layout.
- App build inputs now exclude `Assets/_source_product_images`, docs, scripts, tools, installer, tests, plugins, and package output folders from app item scanning/package inputs.
- `ProjectTest.Tests` remains on `net8.0-windows10.0.19041.0`; its app reference disables package generation for logic test runs.

Validation on 2026-05-13:

```powershell
dotnet --version
dotnet --list-sdks
dotnet restore ProjectTest.csproj
dotnet build ProjectTest.csproj -c Debug -p:Platform=x64
dotnet build ProjectTest.csproj -c Debug -p:Platform=x64 --no-restore
dotnet build ProjectTest.csproj -c Release -p:Platform=x64
dotnet publish ProjectTest.csproj -c Debug -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true -p:AppxPackageDir=AppPackages\ -p:AppxBundle=Never
dotnet test ProjectTest.slnx
dotnet test ProjectTest.Tests\ProjectTest.Tests.csproj -p:Platform=x64
```

Results:

- `dotnet --version`: `8.0.420`.
- `dotnet --list-sdks`: `8.0.420` and `10.0.300-preview.0.26177.108` installed; repo selects `8.0.420`.
- `dotnet restore ProjectTest.csproj`: passed.
- `dotnet build ProjectTest.csproj -c Debug -p:Platform=x64`: passed, 0 warnings.
- Warm `dotnet build ProjectTest.csproj -c Debug -p:Platform=x64 --no-restore`: passed, about 11 seconds in this workspace.
- `dotnet build ProjectTest.csproj -c Release -p:Platform=x64`: passed. Release still emits trim-analysis warnings because Release keeps the existing trimmed configuration.
- Debug MSIX publish with `GenerateAppxPackageOnBuild=true` and `AppxBundle=Never`: passed and produced `AppPackages\ProjectTest_1.0.0.0_x64_Debug_Test\ProjectTest_1.0.0.0_x64_Debug.msix`. It emitted a symbols-package warning because `mspdbcmf.exe` is not on this machine path.
- `dotnet test ProjectTest.slnx`: blocked by stable .NET 8 CLI with `MSB4068` because `.slnx` is not supported by this SDK.
- `dotnet test ProjectTest.Tests\ProjectTest.Tests.csproj -p:Platform=x64`: passed, 15 tests.
- Debug unpackaged launch validation passed: the app opened `MyShop Gaming Accessories POS`, and Dashboard, Products, Orders, Reports, and Settings navigation responded.

Microsoft.WinUI runtime check on 2026-05-13:

- Removed the Debug-only `WindowsAppSDKSelfContained=false` override from `Directory.Build.props`; forcing it can make Visual Studio packaged/unpackaged launch depend on an incorrect Windows App SDK dependency layout and surface `System.IO.FileNotFoundException` for `Microsoft.WinUI, Version=3.0.0.0`.
- `dotnet build-server shutdown`: passed.
- `dotnet clean ProjectTest.csproj -c Debug -p:Platform=x64`: passed.
- `dotnet restore ProjectTest.csproj`: passed.
- `dotnet build ProjectTest.csproj -c Debug -p:Platform=x64`: passed, 0 warnings.
- Verified `Microsoft.WinUI.dll` exists in the Debug x64 output folder.
- `dotnet publish ProjectTest.csproj -c Debug -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true -p:AppxPackageDir=AppPackages\ -p:AppxBundle=Never`: passed and produced the Debug MSIX package. The only warning was the existing `mspdbcmf.exe` symbols-package warning.

Visual Studio manual checklist:

1. Clean solution once.
2. Select `x64` and `Debug`.
3. Select `ProjectTest (Package)` and press Play for MSIX verification.
4. Stop the app, press Play again without changing code, and expect the second run to be faster due to incremental build.
5. Use `ProjectTest (Unpackaged Fast)` for normal code/debug loops.
6. Use `ProjectTest (Package)` only when validating package/deploy behavior.

## Startup Freeze Fix Validation

- Actual bug: startup could restore `Reports`, triggering heavy report load while the main window appeared visible but navigation was laggy or blocked.
- Fixed startup fallback: `Reports` and `ProductEdit` are coerced to `Dashboard`; only `Dashboard`, `Products`, `Orders`, and `Settings` can be restored.
- Fixed navigation blocking: `NavigationService` no longer uses `SaveAsync(...).GetAwaiter().GetResult()` on the UI path.
- Fixed Reports loading: report load is deferred, cancellable, guarded against stale updates, and split into core snapshot first, then ML/assistant insights.
- Fixed report rendering pressure: EF queries are no-tracking with cancellation; chart controls cap rendered points/items.

Commands run on 2026-05-13:

```powershell
dotnet restore ProjectTest.csproj
dotnet build ProjectTest.csproj -c Debug -p:Platform=x64
```

Results:

- `dotnet restore ProjectTest.csproj`: passed.
- `dotnet build ProjectTest.csproj -c Debug -p:Platform=x64`: passed, 0 warnings.
- Automated UI validation set local `LastOpenedScreen=Reports`, launched the Debug x64 app, verified startup landed on `Dashboard`, then invoked Dashboard, Products, Orders, Settings, Reports, Products, Reports, and Settings without navigation blocking.
- `scripts/ui-smoke.ps1` was also attempted, but its older window lookup/focus path failed at username focus before reaching the app navigation flow; the separate UI Automation validation above was used for the startup freeze scenario.

Manual test checklist for future sessions:

1. Open app and login with `admin / MyShop123!`.
2. Set or leave previous screen as Reports, close app, reopen app.
3. Verify startup opens Dashboard and menu is immediately clickable.
4. Click Dashboard, Products, Orders, Settings repeatedly.
5. Click Reports; verify Reports appears and loading does not block the left menu.
6. While Reports is loading, click Products; verify navigation happens immediately and report load is canceled.
7. Return to Reports; cached unchanged range should display quickly.
8. Click Apply Range; loading should not block app navigation.
9. Close app while on Reports and reopen; startup should not freeze.
10. Check output/logs for unexpected exceptions.

## Validation Status

- Full implementation pass complete.
- Installer/setup files were not rebuilt, deleted, renamed, or replaced.
- Debug x64 build passed.
- Logic tests passed.
- PostgreSQL rebuild and VerificationRunner passed in this environment.
- App launch verification passed with a responsive `MyShop Gaming Accessories POS` window.

## Required Commands

```powershell
dotnet restore ProjectTest.csproj
dotnet build ProjectTest.csproj -c Debug -p:Platform=x64
dotnet test ProjectTest.slnx
```

Results from 2026-05-12:

- `dotnet restore ProjectTest.csproj`: passed.
- `dotnet build ProjectTest.csproj -c Debug -p:Platform=x64`: passed, 0 warnings.
- `dotnet test ProjectTest.slnx`: passed, 15 tests.
- Test note: MSBuild reports an EF Core relational transitive version warning in the test project because Npgsql references EF relational 8.0.11 while the app references EF Core 8.0.22. It does not fail tests.

## GraphQL Demo

- Service: `Services/GraphQlPosService.cs`.
- UI: Settings -> GraphQL Demo.
- Queries:
  - `products(pageNumber, pageSize, keyword, categoryId, minPrice, maxPrice, sort)`.
  - `orders(pageNumber, pageSize, keyword, fromDate, toDate, status, customerId)`.
  - `reports(fromDate, toDate)`.
- Mutations:
  - `saveProduct(inputJson: String!)`.
  - `saveOrder(inputJson: String!)`.
- Demo: open Settings, click `Load Sample`, click `Execute GraphQL`, verify JSON result appears in the read-only result textbox.

## ML.Net And LLM Demo

- ML.Net service: `Services/MlInsightService.cs`.
- LLM service: `Services/LlmAssistantService.cs`.
- Demo ML.Net: open Reports, choose a date range, click `Apply Range`, inspect `ML.Net Forecast and Restock Insights`.
- Demo LLM not configured: leave Settings LLM key empty and verify Reports assistant says not configured.
- Demo LLM configured: set `MYSHOP_LLM_API_KEY` or Settings `API key`, optionally set an OpenAI-compatible chat-completions endpoint, then refresh Reports and verify the assistant summary comes from the endpoint.

## Roles And Loyalty Demo

- Seed/reference users: `admin`, `moderator`, and `sale` are ensured in the database.
- All three demo users use `MyShop123!` for this coursework build.
- Auto-login restores the saved username role instead of defaulting to admin.
- Sale users only see their own orders through `OrderQueryOptions.CurrentUserRole`.
- Sale users do not see product import price in Products detail.
- Logout shows a choice to keep or clear saved credentials.
- Customer loyalty: open Settings -> Customer Loyalty to view points/lifetime spend; in Orders choose a customer, mark the order Paid, save, then refresh Settings to see points increase.

## Obfuscation

- Config: `obfuscar.xml`.
- Script: `scripts/obfuscate-release.ps1`.
- Run locally:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\obfuscate-release.ps1 -Platform x64
```

- On this UNC workspace, `dotnet tool restore` may report that `dotnet-tools.json` is blocked by Windows. If so, run from a local clone or unblock the file before restoring tools. The app installer/setup files were not changed.

## Plugin Demo

- Sample project: `plugins/SampleMyShopPlugin`.
- Build/copy script:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-sample-plugin.ps1
```

- Restart the app and open Settings -> Plugins. The DLL plugin should show `Sample MyShop Plugin` with status `Loaded`; malformed plugin DLLs are caught by `PluginService` and shown as error rows instead of crashing the app.

## UI Automation Demo

- Local script: `scripts/ui-smoke.ps1`.
- It is intentionally separate from `dotnet test` because it needs a visible Windows desktop session.

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\ui-smoke.ps1 -AppPath "path\to\ProjectTest.exe"
```

## Optional PostgreSQL Runtime Validation

Run only when PostgreSQL is available locally:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\rebuild-dev-db.ps1
dotnet run --project .\tools\VerificationRunner\VerificationRunner.csproj
```

Results from 2026-05-12:

- PostgreSQL service `postgresql-x64-18` was running.
- Rebuild script passed and verified `5|110|440|866`.
- VerificationRunner passed and created order `442`; stock decreased from 18 to 17 and dashboard/report values returned.

## Demo Instructions Draft

- Login: use `admin / MyShop123!`; also verify `moderator` and `sale`.
- Products: open Products, use search/filter/paging, check gaming accessory specs and image gallery.
- Orders: create an order, choose customer, apply promotion, mark Paid, export invoice.
- Reports: choose date range, refresh, inspect charts, commissions, ML insights, and assistant summary.
- Settings: edit page size, LLM endpoint/key, plugins, and GraphQL demo after implementation.

## Known Issues Before Fixes

- Obfuscator and sample plugin project are missing.

## Invoice Export Validation

- Implemented PDF output in `Services/InvoiceExportService.cs`.
- Implemented `FileSavePicker` in Orders print/export command.
- Debug x64 build passed after this change.
- Demo: open Orders, select or create an order, click `Print / Export invoice`, choose a `.pdf` location, then open the saved PDF with the default PDF reader.
