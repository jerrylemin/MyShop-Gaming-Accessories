# MyShop Full-10 Validation Report

Updated: 2026-05-13

## Feature Reorganization Validation

Updated: 2026-05-13

Old flow issues:

- Settings contained GraphQL, Plugins, and Customer Loyalty, which made it too broad and harder to explain.
- Customer search was missing.
- Plugins were loaded during bootstrap instead of when the Plugins page was opened.
- License and LLM usage guidance was too light for demo/viva.

New flow after this pass:

- Customers owns search, add/edit/delete, loyalty, lifetime spend, paid order count, purchased products, and order history.
- GraphQL is now a Main Menu page with sample query, Execute, query textbox, result textbox, and status.
- Plugins is now a Main Menu page with refresh and plugin status list.
- Settings now contains General, Credentials, License, Backup / Restore, and LLM / SLM Assistant configuration only.
- Reports remains the place where the configured LLM / SLM Assistant is used.
- Startup still restores only lightweight pages and does not restore Reports, GraphQL, or Plugins.

Files changed:

- `Views/MainWindow.xaml`
- `Views/MainWindow.xaml.cs`
- `Views/Pages/CustomersPage.xaml`
- `ViewModels/CustomersViewModel.cs`
- `Repositories/CustomerRepository.cs`
- `Views/Pages/SettingsPage.xaml`
- `Views/Pages/SettingsPage.xaml.cs`
- `ViewModels/SettingsViewModel.cs`
- `Views/Pages/GraphQlPage.xaml`
- `Views/Pages/GraphQlPage.xaml.cs`
- `ViewModels/GraphQlViewModel.cs`
- `Views/Pages/PluginsPage.xaml`
- `Views/Pages/PluginsPage.xaml.cs`
- `ViewModels/PluginsViewModel.cs`
- `Models/PluginInfo.cs`
- `Services/AppBootstrapper.cs`
- `Services/PluginService.cs`
- `Services/LlmAssistantService.cs`
- `Views/Pages/ReportsPage.xaml`
- `README.md`
- `codex_feature_reorg_audit.md`
- `codex_feature_reorg_plan.md`
- `codex_validation_report.md`

Demo checklist:

1. Open app and login with `admin / MyShop123!`.
2. Confirm Main Menu has Customers, GraphQL, and Plugins.
3. Customers: search by name, search by phone, clear search, add/edit/delete a customer without orders, select customer with orders, verify loyalty, purchased products, and order history.
4. Settings: verify GraphQL, Plugins, and Customer Loyalty are gone; License shows the demo code guidance; LLM explains Settings input and Reports usage.
5. GraphQL: Load Sample, Execute, verify JSON output. Run a bad query and verify JSON error output.
6. Plugins: verify plugin list or no-plugin status and no crash on Refresh.
7. Reports: verify Assistant says not configured when no key is available; configure key/endpoint in Settings and refresh Reports to use the assistant.
8. Switch tabs repeatedly and verify no obvious lag.
9. Close/reopen and verify startup lands on Dashboard or another lightweight restored page.

Build commands for this pass:

```powershell
dotnet restore .\ProjectTest.csproj
dotnet build .\ProjectTest.csproj -c Debug -p:Platform=x64
```

- `dotnet restore .\ProjectTest.csproj`: passed.
- `dotnet build .\ProjectTest.csproj -c Debug -p:Platform=x64`: passed, 0 warnings, 0 errors.
- Launch smoke: started the Debug x64 `ProjectTest.exe`, waited 8 seconds, verified the process was still running and responding, then stopped it.

Remaining notes:

- `setup.exe` was not rebuilt.
- Orders customer selection still uses the existing ComboBox; customer search was implemented in Customers first and documented as a later enhancement if needed.

## Customer Management Finish Validation

Updated: 2026-05-13

Changes validated in this pass:

- Pulled `origin/main`; repository was already up to date before edits.
- Main menu Customers button exists in `Views/MainWindow.xaml` with `x:Name="CustomersButton"`, `Tag="Customers"`, `Click="NavigationButton_Click"`, text `Customers`, and glyph `&#xE77B;`.
- `Views/MainWindow.xaml.cs` now maps `CustomersButton`, registers `CustomersPage`, resolves `Customers` in `GetPageKey`, preserves `Customers` in startup normalization, shows the requested Customers subtitle, and mentions Customers in onboarding text.
- Removed `Views/CustomerNavigationSupport.cs` because it only contained the stale `CustomerMenu_Click` partial handler.
- `Services/NavigationService.cs` no longer uses `Type.GetType("ProjectTest.Views.Pages.CustomersPage")`; navigation depends on explicit registration and saves startup settings asynchronously.
- Customer page supports list, selection, add, edit, delete only when no order history exists, loyalty points, lifetime spend, order history, and purchased products. Empty customers and customers without orders are handled by the view model/repository paths.
- Products and Orders page headers were shortened to the requested text, and the long helper descriptions were removed or shortened.
- Settings header and GraphQL helper copy were shortened.
- GraphQL sample query remains a light products query; product/order GraphQL page sizes are clamped to 20; service timeout is 12 seconds; Settings execute now shows `Running...`, catches exceptions, and writes JSON error output.
- Installer was inspected only. It has Inno Setup, bootstrap scripts for .NET Desktop Runtime 8, Windows App Runtime 1.8, PostgreSQL, and a database bootstrapper that writes connection config and runs seed initialization. No `setup.exe` rebuild was performed.

Manual test checklist to run with a desktop session:

1. Open app.
2. Login with `admin / MyShop123!`.
3. Click Customers in the main menu.
4. Add a customer.
5. Edit the customer.
6. Delete a customer that has no orders.
7. Select a customer with orders and verify purchased products plus order history.
8. Open Orders and select a customer for an order.
9. Open Settings, click Execute GraphQL, and verify JSON output appears.
10. Open Products and Orders and verify the shorter titles.

Command results from this pass:

```powershell
dotnet restore .\ProjectTest.csproj
dotnet build .\ProjectTest.csproj -c Debug -p:Platform=x64
```

- `dotnet restore .\ProjectTest.csproj`: passed.
- `dotnet build .\ProjectTest.csproj -c Debug -p:Platform=x64`: passed, 0 warnings, 0 errors.
- Launch smoke: started the Debug x64 `ProjectTest.exe`, waited 8 seconds, verified the process was still running and responding, then stopped it. No long startup hang was observed.

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
- Removed the manual `WinUISDKReferences=false` override from `ProjectTest.csproj`.
- Added a Debug-only MSIX target that copies the generated build `ProjectTest.deps.json` into the loose `AppX` layout used by Visual Studio `ProjectTest (Package)`. The previous loose AppX deps file was stale and missing the `Microsoft.WinUI.dll` runtime entry even though the `.msix` package deps file was correct.
- `dotnet build-server shutdown`: passed.
- `dotnet clean ProjectTest.csproj -c Debug -p:Platform=x64`: passed.
- `dotnet restore ProjectTest.csproj`: passed.
- `dotnet build ProjectTest.csproj -c Debug -p:Platform=x64`: passed, 0 warnings.
- Verified `Microsoft.WinUI.dll` exists in the Debug x64 output folder.
- `dotnet publish ProjectTest.csproj -c Debug -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true -p:AppxPackageDir=AppPackages\ -p:AppxBundle=Never`: passed and produced the Debug MSIX package. The only warning was the existing `mspdbcmf.exe` symbols-package warning.
- `dotnet build ProjectTest.csproj -c Debug -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true -p:AppxBundle=Never`: passed; the loose `AppX\ProjectTest.deps.json` now contains `lib/net6.0-windows10.0.17763.0/Microsoft.WinUI.dll`.
- Registered and launched the loose AppX package with `shell:AppsFolder`; the app opened from `AppX\ProjectTest.exe` with title `MyShop Gaming Accessories POS`.

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
# Installer completion update - 2026-05-13

- Pulled latest `main`: already up to date.
- Added dump-first installer database flow with `scripts/export-demo-database.ps1`, `scripts/restore-demo-database.ps1`, and `installer/database/myshop_demo.dump`.
- Updated Inno installer and bootstrap to include .NET Desktop Runtime, Windows App Runtime, PostgreSQL 18, dump restore, seed fallback, app connection config, Desktop shortcut, Start Menu shortcut, and setup/restore logs.
- Built final setup at `installer/output-final/setup.exe`; previous `installer/output/setup.exe` was left untouched because an elevated test process locked it.
- Ran Debug and Release builds successfully.
- Ran `tools/VerificationRunner` successfully after fixing stale service stubs.
- Ran installer validation script; it exposed real bootstrap issues that were fixed. A later silent installer run timed out because the earlier elevated Inno process could not be killed from this shell, so the final setup was compiled to `installer/output-final`.
- Full command log and environment notes are in `codex_installer_validation.md`.
