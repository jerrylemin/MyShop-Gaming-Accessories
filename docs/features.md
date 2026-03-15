# Features

## Purpose
This document lists the main capability areas in the current gaming accessories POS codebase and the files that own those behaviors.

## Feature Matrix

| Feature | Status | Files implementing it |
| --- | --- | --- |
| Login system | COMPLETE | `Views/LoginWindow.xaml`, `ViewModels/LoginViewModel.cs`, `Services/AuthenticationService.cs` |
| Database configuration screen | COMPLETE | `Views/DatabaseSetupWindow.xaml`, `ViewModels/DatabaseSetupViewModel.cs`, `Services/DatabaseOptionsProvider.cs` |
| Main shell navigation | COMPLETE | `Views/MainWindow.xaml`, `Views/MainWindow.xaml.cs`, `Services/NavigationService.cs` |
| Dashboard metrics | COMPLETE | `Views/Pages/DashboardPage.xaml`, `ViewModels/DashboardViewModel.cs`, `Services/DashboardService.cs` |
| Product listing with images | COMPLETE | `Views/Pages/ProductsPage.xaml`, `ViewModels/ProductsViewModel.cs`, `Repositories/ProductRepository.cs` |
| Product search, sorting, pagination, and price filtering | COMPLETE | `ViewModels/ProductsViewModel.cs`, `Models/ProductQueryOptions.cs`, `Repositories/ProductRepository.cs` |
| Product add/edit/delete | COMPLETE | `Views/Pages/ProductEditPage.xaml`, `Views/Pages/ProductsPage.xaml.cs`, `ViewModels/ProductEditViewModel.cs`, `Repositories/ProductRepository.cs` |
| Product detail panel and gallery | COMPLETE | `Views/Pages/ProductsPage.xaml`, `Models/Product.cs`, `Helpers/ImagePathConverter.cs` |
| Excel product import | COMPLETE | `Views/Pages/ProductsPage.xaml.cs`, `Services/ExcelProductImportService.cs` |
| Orders workflow | COMPLETE | `Views/Pages/OrdersPage.xaml`, `ViewModels/OrdersViewModel.cs`, `Repositories/OrderRepository.cs` |
| Stock synchronization on orders | COMPLETE | `Repositories/OrderRepository.cs` |
| Reports by day and month | COMPLETE | `Views/Pages/ReportsPage.xaml`, `ViewModels/ReportsViewModel.cs`, `Services/ReportingService.cs` |
| Top-selling gaming accessories reports | COMPLETE | `Services/ReportingService.cs`, `Models/ReportsSnapshot.cs` |
| Settings and last-screen persistence | COMPLETE | `Views/Pages/SettingsPage.xaml`, `ViewModels/SettingsViewModel.cs`, `Services/SettingsService.cs`, `Services/NavigationService.cs` |
| Gaming accessories seed dataset | COMPLETE | `Services/DatabaseInitializer.cs`, `DataAccess/Seeding/GamingAccessorySeedGenerator.cs`, `DataAccess/Seeding/gaming_accessories_seed_data.json` |
| Product image asset pipeline | COMPLETE | `scripts/build_gaming_accessory_seed_assets.py`, `Assets/GamingProducts` |
| Database rebuild helper | COMPLETE | `scripts/rebuild-dev-db.ps1`, `tools/DatabaseRebuilder/*` |
| Dependency injection container | PARTIAL | `Services/AppBootstrapper.cs`, `Services/AppServices.cs` |
| Plugin architecture | MISSING | none |
| Backup and restore database | MISSING | none |
| External API layer | MISSING | none |

## Notes
- The products page keeps the original architecture and schema, but now shows gaming accessory brand plus generic accessory spec lines instead of the original domain-specific fields.
- Dashboard metrics still cover total products, low stock, top selling products, today revenue, and recent orders.
- Reports continue to aggregate paid-order revenue and product quantities from the same order tables.
