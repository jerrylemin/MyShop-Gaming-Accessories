# Navigation

## Purpose
This document describes the actual screen flow, the shell structure, and the code files that control navigation.

## Shell
- Shell view: `Views/MainWindow.xaml`
- Shell logic: `Views/MainWindow.xaml.cs`
- Navigation helper: `Services/NavigationService.cs`

The app uses:
- `NavigationView` for primary sections
- `Frame` for page hosting
- `NavigationView.Header` for the current page title

## Startup Flow
1. `App.OnLaunched` in `App.xaml.cs`
2. `AppBootstrapper.BuildAsync`
3. `DatabaseInitializer.InitializeAsync`
4. If database setup fails, show `DatabaseSetupWindow`
5. If saved credentials exist, show `MainWindow`
6. Otherwise show `LoginWindow`

## Primary Screens

### LoginScreen
- Files:
  - `Views/LoginWindow.xaml`
  - `Views/LoginWindow.xaml.cs`
  - `ViewModels/LoginViewModel.cs`

### DashboardPage
- Files:
  - `Views/Pages/DashboardPage.xaml`
  - `Views/Pages/DashboardPage.xaml.cs`
  - `ViewModels/DashboardViewModel.cs`

### ProductsPage
- Files:
  - `Views/Pages/ProductsPage.xaml`
  - `Views/Pages/ProductsPage.xaml.cs`
  - `ViewModels/ProductsViewModel.cs`

### OrdersPage
- Files:
  - `Views/Pages/OrdersPage.xaml`
  - `Views/Pages/OrdersPage.xaml.cs`
  - `ViewModels/OrdersViewModel.cs`

### ReportsPage
- Files:
  - `Views/Pages/ReportsPage.xaml`
  - `Views/Pages/ReportsPage.xaml.cs`
  - `ViewModels/ReportsViewModel.cs`

### SettingsPage
- Files:
  - `Views/Pages/SettingsPage.xaml`
  - `Views/Pages/SettingsPage.xaml.cs`
  - `ViewModels/SettingsViewModel.cs`

## Additional Navigation Nodes
- `Views/DatabaseSetupWindow.xaml`
- `Views/Pages/ProductEditPage.xaml`

## Registered Routes
Configured in `Views/MainWindow.xaml.cs`:
- `Dashboard`
- `Products`
- `ProductEdit`
- `Orders`
- `Reports`
- `Settings`

## Navigation Behavior
- `NavigationService.Navigate(key, parameter, persist)` performs frame navigation.
- `LastOpenedScreen` is persisted through `SettingsService`.
- `ProductEdit` uses `persist: false`.
- `CurrentPageTitle` in `MainWindow.xaml.cs` drives the shell title.

## Important User Journeys
- Login -> Dashboard
- Login -> Products -> Product Edit -> Products
- Login -> Orders -> select order -> inline editor
- Login -> Reports -> apply date range
- Login -> Settings -> save preferences

## Notes For Future Sessions
- Main page titles are not duplicated inside page content; the shell header is authoritative.
- `MainWindowViewModel.cs` is currently unused and not part of the active navigation pipeline.
