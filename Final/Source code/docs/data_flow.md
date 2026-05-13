# Data Flow

## Purpose
This document explains how data moves through the application from startup, storage, repositories, services, and view models to the UI.

## Startup Flow
1. `App.OnLaunched` calls `InitializeAsync`.
2. `AppBootstrapper.BuildAsync` creates shared services and repositories.
3. `DatabaseInitializer.InitializeAsync`:
   - resolves the connection string
   - checks for legacy schema
   - runs migrations
   - seeds categories/products/orders if needed
4. Authentication decides whether to open login or main shell.

## Product Data Flow
1. `ProductsPage` creates `ProductsViewModel`.
2. `ProductsViewModel.LoadAsync` builds `ProductQueryOptions`.
3. `ProductRepository.GetPagedAsync` executes the EF query.
4. Results populate `Products`, `CategoryFilters`, and pagination state.
5. Selected product data is shown in the detail panel.

### Product Save Flow
1. `ProductsViewModel` raises `EditRequested`.
2. `MainWindow` navigates to `ProductEditPage`.
3. `ProductEditViewModel.LoadAsync` loads categories and product data.
4. `ProductEditViewModel.SaveAsync` validates and calls `ProductRepository.SaveAsync`.

### Excel Import Flow
1. `ProductsPage` launches a file picker.
2. `ProductsViewModel.ImportFromExcelAsync` calls `ExcelProductImportService.ImportAsync`.
3. The import service reads `.xlsx`, creates missing categories, and upserts products by SKU.

## Order Data Flow
1. `OrdersViewModel.LoadAsync` requests filtered orders and product lookup items.
2. Selecting an order calls `OrderRepository.GetDraftByIdAsync`.
3. Draft lines are converted into `OrderLineViewModel`.
4. Saving builds `OrderDraft` and calls `OrderRepository.SaveAsync`.
5. The repository uses a transaction and synchronizes stock.

## Dashboard Data Flow
1. `DashboardViewModel.LoadAsync` calls `DashboardService.GetSnapshotAsync`.
2. `DashboardService` queries counts, revenue, low-stock products, top sellers, and latest orders.
3. Snapshot collections populate the page.

## Reporting Data Flow
1. `ReportsViewModel.LoadAsync` builds `ReportQueryOptions`.
2. `ReportingService.GetSnapshotAsync` aggregates chart data.
3. The view model replaces the bound chart collections.

## Settings And Session Data Flow
1. `SettingsService.InitializeAsync` loads `AppSettings`.
2. `NavigationService` updates `LastOpenedScreen`.
3. `ProductsViewModel` listens for settings changes and reloads page data.
4. `SettingsViewModel` can also clear saved login credentials through `AuthenticationService`.

## Authentication Data Flow
1. `LoginViewModel.LoginAsync` sends credentials to `AuthenticationService`.
2. The service validates against saved or bootstrap credentials.
3. On success, encrypted credentials are persisted for future launches.

## Reference Files
- Startup: `App.xaml.cs`, `Services/AppBootstrapper.cs`, `Services/DatabaseInitializer.cs`
- Products: `ViewModels/ProductsViewModel.cs`, `Repositories/ProductRepository.cs`
- Orders: `ViewModels/OrdersViewModel.cs`, `Repositories/OrderRepository.cs`
- Reports: `ViewModels/ReportsViewModel.cs`, `Services/ReportingService.cs`
- Settings: `ViewModels/SettingsViewModel.cs`, `Services/SettingsService.cs`
