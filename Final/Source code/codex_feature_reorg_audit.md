# Feature Reorganization Audit

Updated: 2026-05-13

## Old Flow Problems

- Settings mixed unrelated modules: GraphQL console, plugin list, customer loyalty, license, backup/restore, saved-login cleanup, and LLM configuration.
- Customer Management did not have a search workflow, so demos with many customers required scanning the full list.
- Customer loyalty data was visible in Settings instead of the customer workflow where it belongs.
- GraphQL lived in Settings, which made Settings heavier and hid an API demo behind app preferences.
- Plugins lived in Settings and were loaded during bootstrap, which could make startup do extension work before the user asked for it.
- License activation existed but the UI did not explain the 15-day trial or accepted demo activation format.
- LLM API settings did not clearly explain that Reports > Assistant is the place where the configured API is used.

## New Flow

- Main Menu order is now Dashboard, Products, Orders, Customers, Reports, GraphQL, Plugins, Settings, Log out.
- Customers owns customer search, editing, loyalty points, lifetime spend, paid order count, purchased products, and order history.
- Settings is limited to General, Credentials, License, Backup / Restore, and LLM / SLM Assistant configuration.
- GraphQL has its own page with sample query, Execute, query editor, result output, and status.
- Plugins has its own page with plugin list and refresh/reload.
- Reports remains the place where the LLM / SLM Assistant summary is used.
- Startup does not restore Reports, GraphQL, or Plugins; heavy modules are opened only when selected.

## Files Reviewed

- `Views/MainWindow.xaml`
- `Views/MainWindow.xaml.cs`
- `Services/NavigationService.cs`
- `Views/Pages/SettingsPage.xaml`
- `ViewModels/SettingsViewModel.cs`
- `Views/Pages/CustomersPage.xaml`
- `Views/Pages/CustomersPage.xaml.cs`
- `ViewModels/CustomersViewModel.cs`
- `Repositories/CustomerRepository.cs`
- `Views/Pages/OrdersPage.xaml`
- `ViewModels/OrdersViewModel.cs`
- `Services/GraphQlPosService.cs`
- `Services/PluginService.cs`
- `Services/LlmAssistantService.cs`
- `Services/LicenseService.cs`
- `Models/AppSettings.cs`
- `Services/SettingsService.cs`
- `Services/AppServices.cs`
- `Services/AppBootstrapper.cs`
- `README.md`

## Files Changed

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

## Demo Notes

- Customers: search by name/phone/email, add/edit/delete a customer without orders, select a customer with orders, then inspect loyalty, purchased products, and order history.
- GraphQL: open Main Menu > GraphQL, Load Sample, Execute, verify JSON output.
- Plugins: open Main Menu > Plugins, verify plugin list or no-plugin status, click Refresh.
- Settings: verify GraphQL, Plugins, and Customer Loyalty are gone; License and LLM guidance are visible.
- Reports: Assistant shows a not-configured message without an API key, or a summary/error after Settings is configured.

## Remaining Notes

- Orders customer selection still uses the existing ComboBox. Customer search was prioritized in Customers; add an order-customer picker/search later if the customer list becomes too long during demos.
- `setup.exe` was not rebuilt.
