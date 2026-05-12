# MyShop Full-10 Audit

Updated: 2026-05-12

## Repository State

- Pulled `origin main`: already up to date.
- Working tree before implementation: clean.
- App target: `net8.0-windows10.0.19041.0`.
- Test project currently targets `net10.0-windows10.0.19041.0`, which must be aligned to net8.
- Installer files exist under `installer/`; setup must not be rebuilt, deleted, renamed, or replaced.

## Current Findings Before Fixes

### Already Substantially Present

- WinUI 3 app with Dashboard, Products, Orders, Reports, Settings, login, database setup, repository/service layering.
- PostgreSQL EF Core model, migrations, bootstrap initializer, and seed generator.
- Seed generator enforces 5 gaming accessory categories and at least 22 products per category.
- Packaged product gallery exists under `Assets/GamingProducts`.
- Order repository already supports customer, promotion, salesperson, discount, order status, and sale-only order filtering.
- Customer loyalty tables and repository exist.
- Settings stores `LlmApiKey` and `LlmEndpoint`.
- Plugin loader exists and scans `Plugins` for manifests and DLLs.
- Reports page already shows KPI, charts, sales commissions, assistant summary, and restock insight area.

### Missing Or Weak Against Checklist

- Invoice export writes plain `.txt` to Documents from `OrdersViewModel.PrintAsync`; no FileSavePicker and no real PDF/XPS output.
- `GraphQlPosService` uses string switch operations, not a real GraphQL schema/executor.
- Settings has no GraphQL demo query textbox, execute button, sample query, or JSON result area.
- `MlInsightService` calculates manual sales velocity only; no Microsoft.ML pipeline/package.
- `LlmAssistantService` returns a local string summary and does not call a configured endpoint.
- No Release obfuscator config/script.
- UI smoke tests are static file assertions only; no separate local WinAppDriver/Appium/UI Automation demo script.
- Functional tests need target framework alignment and more explicit coverage.
- Sample plugin is a generated manifest only; no sample plugin project/DLL build path.
- Authentication is local/bootstrap based; database users exist in seed but login does not query DB users.
- Logout immediately returns to login but does not clear saved credentials or stop auto-login by choice.
- Customer loyalty UI is present only through customer selection in Orders; no clear customer loyalty list.
- Settings page contains mojibake Vietnamese text.
- Product import price visibility is available in view model but Product page still shows import price unconditionally.

## Files Expected To Change

- `ProjectTest.csproj`
- `ProjectTest.Tests/ProjectTest.Tests.csproj`
- `Services/InvoiceExportService.cs`
- `Services/GraphQlPosService.cs`
- `Services/MlInsightService.cs`
- `Services/LlmAssistantService.cs`
- `Services/AuthenticationService.cs`
- `Services/AppBootstrapper.cs`
- `Services/AppServices.cs`
- `ViewModels/OrdersViewModel.cs`
- `ViewModels/ReportsViewModel.cs`
- `ViewModels/SettingsViewModel.cs`
- `Views/Pages/OrdersPage.xaml`
- `Views/Pages/SettingsPage.xaml`
- `Views/Pages/ReportsPage.xaml`
- `Views/Pages/ProductsPage.xaml`
- `Views/MainWindow.xaml.cs`
- `README.md`
- root Codex handoff markdown files
- Obfuscator config/script
- sample plugin project/docs
- UI automation demo script/docs
- tests

## Demo Checklist To Validate After Fixes

- Build Debug x64.
- Run logic tests with `dotnet test ProjectTest.slnx`.
- Open app with PostgreSQL available, login as `admin`, `moderator`, and `sale`.
- Products: verify paging/filtering, images, gaming accessory spec labels, and sale import-price restriction.
- Orders: create order, choose customer, mark Paid, export invoice via save picker, open generated PDF/XPS.
- Reports: verify ML.Net insights, revenue forecast/restock note, and LLM not-configured/configured behavior.
- Settings: execute GraphQL sample query and inspect JSON result; verify plugin list and customer loyalty list.
- Plugin: build sample plugin, copy DLL to `Plugins`, restart app, verify status `Loaded`.
