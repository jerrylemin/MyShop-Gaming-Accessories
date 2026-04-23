using ProjectTest.Repositories;

namespace ProjectTest.Services;

public class AppServices
{
    public AppServices(
        DatabaseOptions databaseOptions,
        SettingsService settingsService,
        AuthenticationService authenticationService,
        NavigationService navigationService,
        DatabaseInitializer databaseInitializer,
        ExcelProductImportService excelProductImportService,
        CategoryRepository categoryRepository,
        ProductRepository productRepository,
        OrderRepository orderRepository,
        DashboardService dashboardService,
        ReportingService reportingService)
    {
        DatabaseOptions = databaseOptions;
        SettingsService = settingsService;
        AuthenticationService = authenticationService;
        NavigationService = navigationService;
        DatabaseInitializer = databaseInitializer;
        ExcelProductImportService = excelProductImportService;
        CategoryRepository = categoryRepository;
        ProductRepository = productRepository;
        OrderRepository = orderRepository;
        DashboardService = dashboardService;
        ReportingService = reportingService;
    }

    public DatabaseOptions DatabaseOptions { get; }

    public SettingsService SettingsService { get; }

    public AuthenticationService AuthenticationService { get; }

    public NavigationService NavigationService { get; }

    public DatabaseInitializer DatabaseInitializer { get; }

    public ExcelProductImportService ExcelProductImportService { get; }

    public CategoryRepository CategoryRepository { get; }

    public ProductRepository ProductRepository { get; }

    public OrderRepository OrderRepository { get; }

    public DashboardService DashboardService { get; }

    public ReportingService ReportingService { get; }
}
