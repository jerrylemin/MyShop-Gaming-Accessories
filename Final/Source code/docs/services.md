# Services

## Purpose
This document summarizes the services layer, what each service owns, and how services collaborate with repositories and view models.

## Composition And Registry

### `Services/AppBootstrapper.cs`
- Builds the service graph at startup.
- Creates settings, authentication, repositories, analytics services, and navigation.

### `Services/AppServices.cs`
- Holds the constructed service instances.
- Exposed through `App.Current.Services`.

## Runtime Services

### `Services/AuthenticationService.cs`
- Encrypted credential storage and validation.
- Used by `LoginViewModel`, `SettingsViewModel`, and `App.xaml.cs`.

### `Services/DatabaseOptionsProvider.cs`
- Resolves and persists the PostgreSQL connection string.
- Used by bootstrap and the database setup window.

### `Services/DatabaseInitializer.cs`
- Detects legacy schema, runs migrations, and seeds data.
- Used by `App.xaml.cs`.

### `Services/NavigationService.cs`
- Owns frame navigation and last-screen persistence.
- Used by `MainWindow` and product-edit routing.

### `Services/SettingsService.cs`
- Reads and writes `AppSettings` in `LocalSettings`.
- Used by navigation, products paging, and settings UI.

### `Services/DashboardService.cs`
- Aggregates dashboard KPI and chart data.
- Used by `DashboardViewModel`.

### `Services/ReportingService.cs`
- Aggregates day/week/month/year report datasets.
- Used by `ReportsViewModel`.

### `Services/ExcelProductImportService.cs`
- Imports `.xlsx` rows and upserts categories/products.
- Used by `ProductsViewModel`.

## Collaboration With Repositories
- Repositories handle direct entity queries and transactional write logic.
- Services handle startup, analytics, import, navigation, settings, and authentication workflows.

## Files To Read First
- `Services/AppBootstrapper.cs`
- `Services/AppServices.cs`
- `Services/DatabaseInitializer.cs`
- `Services/NavigationService.cs`
- `Services/DashboardService.cs`
- `Services/ReportingService.cs`
- `Services/ExcelProductImportService.cs`
