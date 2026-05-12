# MyShop Full-10 Validation Report

Updated: 2026-05-13

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
