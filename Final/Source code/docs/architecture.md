# Architecture

## Purpose
This document explains how the current codebase is organized and how responsibilities are split between MVVM, repositories, and services.

## Architectural Style
- Desktop client application using WinUI 3
- MVVM for UI separation
- Repository pattern over EF Core
- Service layer for startup, navigation, analytics, import, settings, and authentication
- Manual dependency composition in `Services/AppBootstrapper.cs`

## MVVM Pattern

### Base Infrastructure
- `Helpers/ViewModelBase.cs`
  Implements `INotifyPropertyChanged`.
- `Helpers/RelayCommand.cs`
  Synchronous command helper.
- `Helpers/AsyncRelayCommand.cs`
  Async command helper with re-entry protection.

### View Responsibilities
Views stay thin and mostly handle UI-only tasks:
- launching pickers
- showing confirmation dialogs
- forwarding `PasswordBox` changes
- handling page load events
- page-to-frame navigation fallback

Examples:
- `Views/Pages/ProductsPage.xaml.cs`
- `Views/Pages/OrdersPage.xaml.cs`
- `Views/LoginWindow.xaml.cs`

### ViewModel Responsibilities
View models own:
- screen state
- filters and paging state
- `ObservableCollection` instances
- command wiring
- calls into repositories and services

Examples:
- `ViewModels/ProductsViewModel.cs`
- `ViewModels/OrdersViewModel.cs`
- `ViewModels/ReportsViewModel.cs`
- `ViewModels/SettingsViewModel.cs`

## Dependency Composition

### Current Implementation
The project does not use `Microsoft.Extensions.DependencyInjection`. Instead it uses:
- `Services/AppBootstrapper.cs`
- `Services/AppServices.cs`

`AppBootstrapper` constructs the service graph once at startup, then `App.Current.Services` is used by windows and pages to access shared services.

### Practical Assessment
- Status: PARTIAL dependency injection
- Strength: centralized composition exists
- Limitation: runtime code uses a service locator pattern rather than constructor injection from a container

## Repository Usage
- `Repositories/CategoryRepository.cs`
- `Repositories/ProductRepository.cs`
- `Repositories/OrderRepository.cs`

Repositories isolate EF query logic from view models and keep UI logic out of `DbContext`.

## Services Layer
- `Services/DatabaseInitializer.cs`
- `Services/AuthenticationService.cs`
- `Services/NavigationService.cs`
- `Services/SettingsService.cs`
- `Services/DashboardService.cs`
- `Services/ReportingService.cs`
- `Services/ExcelProductImportService.cs`
- `Services/DatabaseOptionsProvider.cs`

## Data Access Layer
- `DataAccess/MyShopDbContext.cs`
- `DataAccess/MyShopDbContextFactory.cs`
- `DataAccess/DesignTimeMyShopDbContextFactory.cs`
- `DataAccess/Migrations/*`
- `DataAccess/Seeding/GamingAccessorySeedGenerator.cs`

## Navigation Architecture
- Shell: `Views/MainWindow.xaml`
- Navigation controller: `Services/NavigationService.cs`
- Header control: `Views/MainWindow.xaml.cs`

The shell registers page keys and persists the last opened main page. `NavigationView.Header` is the single source of main page titles.

## Current Gaps
- No DI container package
- No plugin system
- No backup/restore module
- No dedicated API layer between client and database
