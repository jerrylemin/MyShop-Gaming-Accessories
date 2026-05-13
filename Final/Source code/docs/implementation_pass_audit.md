# Implementation Pass Audit

## Current flow scan

- Startup flow:
  `Program.cs` -> `App.xaml.cs` -> `AppBootstrapper.BuildAsync()` -> `DatabaseInitializer.InitializeAsync()` -> `LoginWindow` or `MainWindow`
- Main shell:
  `Views/MainWindow.xaml` + `Views/MainWindow.xaml.cs` + `Services/NavigationService.cs`
- Products flow:
  `ProductsPage` -> `ProductsViewModel` -> `ProductRepository` and `CategoryRepository`
- Product edit flow:
  `ProductEditPage` -> `ProductEditViewModel` -> `ProductRepository`
- Orders flow:
  `OrdersPage` -> `OrdersViewModel` -> `OrderRepository`
- Reports flow:
  `ReportsPage` -> `ReportsViewModel` -> `ReportingService`
- Dashboard flow:
  `DashboardPage` -> `DashboardViewModel` -> `DashboardService`
- Database and seed flow:
  `DatabaseInitializer` -> `GamingAccessorySeedGenerator` -> embedded dataset JSON -> packaged assets

## Gaps confirmed before implementation

- Category management has no CRUD UI or safe delete flow.
- Orders list has filtering but no paging model or repository paging query.
- Profit reporting is missing from schema, aggregation logic, and report UI.
- Seed catalog is fixed at 50 products and must be expanded to at least 22 products per category.
- Product image pipeline still maps category-level placeholder sets instead of product-correct galleries.
- Rebuild and verification scripts need to stay aligned with the new schema and seed pipeline.

## Guardrails for the implementation pass

- Keep MVVM boundaries intact and avoid moving business logic into code-behind.
- Preserve startup migration flow so the app can self-heal older databases.
- Keep every major phase buildable and ready for an immediate push to `origin/main`.
