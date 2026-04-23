using ProjectTest.DataAccess;
using ProjectTest.Repositories;

namespace ProjectTest.Services;

public static class AppBootstrapper
{
    public static async Task<AppServices> BuildAsync()
    {
        var databaseOptions = DatabaseOptionsProvider.GetDefault();
        var settingsService = new SettingsService();
        await settingsService.InitializeAsync();

        var authenticationService = new AuthenticationService();
        var dbContextFactory = new MyShopDbContextFactory(databaseOptions.ConnectionString);
        var databaseInitializer = new DatabaseInitializer(dbContextFactory);
        var excelProductImportService = new ExcelProductImportService(dbContextFactory);

        var categoryRepository = new CategoryRepository(dbContextFactory);
        var productRepository = new ProductRepository(dbContextFactory);
        var orderRepository = new OrderRepository(dbContextFactory);
        var dashboardService = new DashboardService(dbContextFactory);
        var reportingService = new ReportingService(dbContextFactory);
        var navigationService = new NavigationService(settingsService);

        return new AppServices(
            databaseOptions,
            settingsService,
            authenticationService,
            navigationService,
            databaseInitializer,
            excelProductImportService,
            categoryRepository,
            productRepository,
            orderRepository,
            dashboardService,
            reportingService);
    }
}
