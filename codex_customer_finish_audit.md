# Customer Finish Audit

Updated: 2026-05-13

## Files Reviewed

- `Views/MainWindow.xaml`
- `Views/MainWindow.xaml.cs`
- `Views/CustomerNavigationSupport.cs`
- `Views/Pages/CustomersPage.xaml`
- `Views/Pages/CustomersPage.xaml.cs`
- `ViewModels/CustomersViewModel.cs`
- `Repositories/CustomerRepository.cs`
- `Services/NavigationService.cs`
- `Services/AppServices.cs`
- `Services/AppBootstrapper.cs`
- `Views/Pages/ProductsPage.xaml`
- `Views/Pages/OrdersPage.xaml`
- `Views/Pages/SettingsPage.xaml`
- `ViewModels/SettingsViewModel.cs`
- `Services/GraphQlPosService.cs`
- `ProjectTest.csproj`

## Findings

- `Views/MainWindow.xaml` already had a `CustomersButton` with `Tag="Customers"`, `Click="NavigationButton_Click"`, text `Customers`, and glyph `&#xE77B;`.
- `Views/MainWindow.xaml.cs` was missing Customers in the navigation button map, page registration, page-key mapping, startup normalization, subtitle text, and onboarding copy.
- `Views/CustomerNavigationSupport.cs` only contained the stale `CustomerMenu_Click` partial handler and was removed.
- `Services/NavigationService.cs` had a Customers fallback based on `Type.GetType(...)`; it was removed because pages are registered explicitly from `MainWindow`.
- `NavigationService.Navigate` persists settings asynchronously through `_ = SaveSettingsSafelyAsync(settings)` and does not block the UI thread.
- `CustomersPage.xaml.cs` unregisters its `Loaded` handler before calling `LoadAsync`, so the page does not repeatedly load from the same page instance.
- `CustomerRepository.GetProfileAsync` limits customer order history to the 50 newest orders and tolerates customers with no orders.
- `GraphQlPosService.GetSampleQuery()` uses a light products query with page size 5.
- GraphQL page-size values are clamped to a maximum of 20 for products and orders.
- Settings load initializes the sample GraphQL query only; it does not execute GraphQL automatically.

## Installer Check

- Installer scripts exist under `installer/`.
- `installer/setup.iss` defines an Inno Setup `setup.exe` output.
- `installer/install-bootstrap.ps1` checks/installs .NET Desktop Runtime 8, Windows App Runtime 1.8, and PostgreSQL 16.
- `installer/database/Program.cs` creates/updates the PostgreSQL database, writes `myshop.database.json`, and runs `DatabaseInitializer.InitializeAsync()` for migrations/seed.
- No `installer/output/setup.exe` file is present in this workspace snapshot. Per request, setup was not rebuilt.
