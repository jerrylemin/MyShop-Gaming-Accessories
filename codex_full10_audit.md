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

### Fixed In This Pass

- Invoice export now writes real PDF through a FileSavePicker from Orders.
- `GraphQlPosService` now uses GraphQL.NET schema/executor with typed query and mutation fields.
- Settings now has GraphQL query, sample, execute button, and JSON result area.
- `MlInsightService` now uses a Microsoft.ML regression pipeline for 7-day revenue forecast when enough data exists, with stock velocity fallback.
- `LlmAssistantService` now calls an OpenAI-compatible HTTP endpoint using Settings or `MYSHOP_LLM_API_KEY`, with timeout and safe error handling.
- Release obfuscation config/script added through Obfuscar.
- Local UI automation smoke script added separately from headless tests.
- Functional test project aligned to `net8.0-windows10.0.19041.0`; 15 tests pass.
- Sample plugin project and build/copy script added.
- Database users are ensured, login checks DB users when available, and saved auto-login restores role.
- Logout offers a clear-saved-login path.
- Sale users do not see import price, and order queries scope sale users to their own orders.
- Settings now shows customer loyalty points and lifetime spend.
- Settings mojibake text was replaced.
- Verification tools were updated for advanced POS models.

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
