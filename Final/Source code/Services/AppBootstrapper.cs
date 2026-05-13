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

        var dbContextFactory = new MyShopDbContextFactory(databaseOptions.ConnectionString);
        var currentUserService = new CurrentUserService();
        var authenticationService = new AuthenticationService(currentUserService, dbContextFactory);
        var databaseInitializer = new DatabaseInitializer(dbContextFactory);
        var excelProductImportService = new ExcelProductImportService(dbContextFactory);
        var licenseService = new LicenseService();
        var discountService = new DiscountService();
        var backupRestoreService = new BackupRestoreService(databaseOptions);
        var pluginService = new PluginService();

        var categoryRepository = new CategoryRepository(dbContextFactory);
        var customerRepository = new CustomerRepository(dbContextFactory);
        var promotionRepository = new PromotionRepository(dbContextFactory);
        var productRepository = new ProductRepository(dbContextFactory);
        var orderRepository = new OrderRepository(dbContextFactory, discountService);
        var dashboardService = new DashboardService(dbContextFactory);
        var mlInsightService = new MlInsightService(dbContextFactory);
        var llmAssistantService = new LlmAssistantService(settingsService);
        var reportingService = new ReportingService(dbContextFactory, mlInsightService, llmAssistantService);
        var invoiceExportService = new InvoiceExportService(orderRepository);
        var graphQlPosService = new GraphQlPosService(productRepository, orderRepository, reportingService);
        var onboardingService = new OnboardingService();
        var navigationService = new NavigationService(settingsService);

        var services = new AppServices(
            databaseOptions,
            settingsService,
            authenticationService,
            navigationService,
            databaseInitializer,
            excelProductImportService,
            currentUserService,
            licenseService,
            discountService,
            backupRestoreService,
            pluginService,
            invoiceExportService,
            mlInsightService,
            llmAssistantService,
            graphQlPosService,
            onboardingService,
            categoryRepository,
            customerRepository,
            promotionRepository,
            productRepository,
            orderRepository,
            dashboardService,
            reportingService);

        return services;
    }
}
