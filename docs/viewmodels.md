# ViewModels

## Purpose
This document maps screens to view models and describes the state and commands each one owns.

## Base Pattern
- Base class: `Helpers/ViewModelBase.cs`
- Commands:
  - `Helpers/RelayCommand.cs`
  - `Helpers/AsyncRelayCommand.cs`

## Window ViewModels

### `ViewModels/LoginViewModel.cs`
- Screen: `Views/LoginWindow.xaml`
- Purpose: login validation, busy state, version display, and bootstrap hint.
- Important members:
  - `Username`
  - `Password`
  - `ErrorMessage`
  - `VersionText`
  - `LoginCommand`

### `ViewModels/DatabaseSetupViewModel.cs`
- Screen: `Views/DatabaseSetupWindow.xaml`
- Purpose: capture a PostgreSQL connection string and retry startup.
- Important members:
  - `ConnectionString`
  - `ErrorMessage`
  - `StatusMessage`
  - `TemplateConnectionString`
  - `SaveCommand`

### `ViewModels/MainWindowViewModel.cs`
- Status: present in the repo but not used by the active shell.
- Note: `MainWindow.xaml.cs` currently owns active page title state directly.

## Page ViewModels

### `ViewModels/DashboardViewModel.cs`
- Screen: `Views/Pages/DashboardPage.xaml`
- Collections:
  - `RevenuePoints`
  - `TopLowStockProducts`
  - `TopSellingProducts`
  - `LatestOrders`
- Command:
  `RefreshCommand`

### `ViewModels/ProductsViewModel.cs`
- Screen: `Views/Pages/ProductsPage.xaml`
- Purpose: product listing, filtering, pagination, import, and edit routing.
- Important state:
  - `Keyword`
  - `MinPrice`
  - `MaxPrice`
  - `SelectedCategoryFilter`
  - `SelectedSortOption`
  - `CurrentPage`
  - `TotalPages`
  - `SelectedProduct`
- Commands:
  - `RefreshCommand`
  - `PreviousPageCommand`
  - `NextPageCommand`
  - `AddCommand`
  - `EditCommand`

### `ViewModels/ProductEditViewModel.cs`
- Screen: `Views/Pages/ProductEditPage.xaml`
- Purpose: edit one product and save it.
- Important state:
  SKU, name, manufacturer, specs, prices, stock, category, description, image paths.
- Command:
  `SaveCommand`

### `ViewModels/OrdersViewModel.cs`
- Screen: `Views/Pages/OrdersPage.xaml`
- Purpose: order history filtering and inline draft editing.
- Important state:
  - `FromDate`
  - `ToDate`
  - `SelectedOrder`
  - `DraftCreatedTime`
  - `SelectedStatus`
  - `DraftTotal`
  - `CanEditCurrentOrder`
- Commands:
  - `SaveCommand`
  - `RefreshCommand`
  - `ApplyDateFilterCommand`
  - `NewOrderCommand`
  - `AddItemCommand`

### `ViewModels/OrderLineViewModel.cs`
- Used inside `OrdersViewModel`.
- Purpose: represent a draft line item with calculated total and quantity binding.

### `ViewModels/ReportsViewModel.cs`
- Screen: `Views/Pages/ReportsPage.xaml`
- Purpose: manage date range selection and expose datasets for charts.
- Collections:
  - `RevenueByDay`
  - `RevenueByWeek`
  - `RevenueByMonth`
  - `RevenueByYear`
  - `ProductSalesByRange`
  - `ProductSalesShare`

### `ViewModels/SettingsViewModel.cs`
- Screen: `Views/Pages/SettingsPage.xaml`
- Purpose: save items-per-page and clear credentials.
- Important state:
  - `SelectedItemsPerPage`
  - `LastOpenedScreen`
  - `CredentialStatus`
  - `StatusMessage`

## Relationships
- Every active page/window has one corresponding active view model.
- View models call repositories/services directly; they do not depend on `DbContext`.
- Collections exposed to XAML use `ObservableCollection`.
