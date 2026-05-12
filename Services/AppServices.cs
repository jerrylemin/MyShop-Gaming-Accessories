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
        CurrentUserService currentUserService,
        LicenseService licenseService,
        DiscountService discountService,
        BackupRestoreService backupRestoreService,
        PluginService pluginService,
        InvoiceExportService invoiceExportService,
        MlInsightService mlInsightService,
        LlmAssistantService llmAssistantService,
        GraphQlPosService graphQlPosService,
        OnboardingService onboardingService,
        CategoryRepository categoryRepository,
        CustomerRepository customerRepository,
        PromotionRepository promotionRepository,
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
        CurrentUserService = currentUserService;
        LicenseService = licenseService;
        DiscountService = discountService;
        BackupRestoreService = backupRestoreService;
        PluginService = pluginService;
        InvoiceExportService = invoiceExportService;
        MlInsightService = mlInsightService;
        LlmAssistantService = llmAssistantService;
        GraphQlPosService = graphQlPosService;
        OnboardingService = onboardingService;
        CategoryRepository = categoryRepository;
        CustomerRepository = customerRepository;
        PromotionRepository = promotionRepository;
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

    public CurrentUserService CurrentUserService { get; }

    public LicenseService LicenseService { get; }

    public DiscountService DiscountService { get; }

    public BackupRestoreService BackupRestoreService { get; }

    public PluginService PluginService { get; }

    public InvoiceExportService InvoiceExportService { get; }

    public MlInsightService MlInsightService { get; }

    public LlmAssistantService LlmAssistantService { get; }

    public GraphQlPosService GraphQlPosService { get; }

    public OnboardingService OnboardingService { get; }

    public CategoryRepository CategoryRepository { get; }

    public CustomerRepository CustomerRepository { get; }

    public PromotionRepository PromotionRepository { get; }

    public ProductRepository ProductRepository { get; }

    public OrderRepository OrderRepository { get; }

    public DashboardService DashboardService { get; }

    public ReportingService ReportingService { get; }
}
