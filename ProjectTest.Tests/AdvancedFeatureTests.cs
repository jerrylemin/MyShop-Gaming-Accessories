using ProjectTest.Models;
using ProjectTest.Repositories;
using ProjectTest.Services;
using ProjectTest.ViewModels;
using System.Reflection;
using System.Text;

namespace ProjectTest.Tests;

public class AdvancedFeatureTests
{
    [Fact]
    public void DiscountService_AppliesPercentagePromotion()
    {
        var service = new DiscountService();
        var discount = service.CalculateDiscount(1_000_000m, new Promotion
        {
            IsActive = true,
            DiscountType = DiscountType.Percentage,
            DiscountValue = 10m,
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today.AddDays(1)
        });

        Assert.Equal(100_000m, discount);
    }

    [Fact]
    public void DiscountService_CapsFixedPromotionAtSubtotal()
    {
        var service = new DiscountService();
        var discount = service.CalculateDiscount(50_000m, new Promotion
        {
            IsActive = true,
            DiscountType = DiscountType.Amount,
            DiscountValue = 100_000m,
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today.AddDays(1)
        });

        Assert.Equal(50_000m, discount);
    }

    [Theory]
    [InlineData("MYSHOP-123456789", true)]
    [InlineData("bad-code", false)]
    public void LicenseService_ValidatesActivationCodeShape(string code, bool expected)
    {
        Assert.Equal(expected, LicenseService.IsValidActivationCode(code));
    }

    [Fact]
    public void RoleService_RestrictsSaleImportPriceAndOrderScope()
    {
        var service = new CurrentUserService();
        service.SetCurrentUser(new AppUser { Id = 3, Username = "sale", Role = UserRole.Sale });

        Assert.False(service.CanViewImportPrice);
        Assert.False(service.CanViewAllOrders);
    }

    [Fact]
    public void MainWindowViewModel_ExposesDefaultNavigationTitle()
    {
        var viewModel = new MainWindowViewModel();

        Assert.Equal("Dashboard", viewModel.CurrentScreenTitle);
        Assert.Equal("MyShop Gaming Accessories POS", viewModel.WindowTitle);
    }

    [Fact]
    public void OrderDraft_TotalSupportsDiscountInputs()
    {
        var draft = new OrderDraft
        {
            DiscountAmount = 15_000m,
            Items =
            [
                new OrderDraftItem { ProductId = 1, ProductName = "Mouse", Quantity = 2, UnitSalePrice = 100_000m }
            ]
        };

        Assert.Equal(200_000m, draft.Items.Sum(x => x.TotalPrice));
        Assert.Equal(15_000m, draft.DiscountAmount);
    }

    [Fact]
    public void ProductQueryOptions_DefaultsAreStableForPagingAndSort()
    {
        var options = new ProductQueryOptions();

        Assert.Equal(1, options.PageNumber);
        Assert.Equal(10, options.PageSize);
        Assert.Equal(ProductSortOption.Name, options.SortOption);
        Assert.Equal(UserRole.Admin, options.CurrentUserRole);
    }

    [Fact]
    public void OrderRepository_AllowsCreatedToPaidOnlyForOpenOrders()
    {
        var method = typeof(OrderRepository).GetMethod("ValidateTransition", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException("ValidateTransition was not found.");

        method.Invoke(null, [OrderStatus.Created, OrderStatus.Paid]);
        var exception = Assert.Throws<TargetInvocationException>(() => method.Invoke(null, [OrderStatus.Paid, OrderStatus.Cancelled]));
        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    [Fact]
    public void InvoiceExportService_BuildsPdfBytes()
    {
        var method = typeof(InvoiceExportService).GetMethod("BuildPdf", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException("BuildPdf was not found.");
        var draft = new OrderDraft
        {
            Id = 12,
            CustomerName = "Demo customer",
            SalespersonName = "Administrator",
            Status = OrderStatus.Paid,
            Items =
            [
                new OrderDraftItem { ProductId = 1, ProductName = "Gaming Mouse", Quantity = 2, UnitSalePrice = 500_000m }
            ]
        };

        var bytes = Assert.IsType<byte[]>(method.Invoke(null, [draft]));
        var header = Encoding.ASCII.GetString(bytes.Take(8).ToArray());

        Assert.StartsWith("%PDF-1.", header);
        Assert.Contains("MyShop Invoice", Encoding.ASCII.GetString(bytes));
    }

    [Fact]
    public async Task LlmAssistant_ReturnsNotConfiguredWithoutKey()
    {
        Environment.SetEnvironmentVariable("MYSHOP_LLM_API_KEY", null);
        var service = new LlmAssistantService(new SettingsService());

        var result = await service.AnalyzeReportsAsync(new ReportsSnapshot());

        Assert.False(result.IsConfigured);
        Assert.Contains("not configured", result.Summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BackupService_ReturnsClearErrorWhenToolMissingOrPathInvalid()
    {
        var service = new BackupRestoreService(new DatabaseOptions { ConnectionString = "Host=localhost;Database=myshop;Username=u;Password=p" });
        var result = await service.BackupAsync(Path.Combine(Path.GetTempPath(), "myshop-test-backup.dump"));

        Assert.False(result.Success);
        Assert.Contains("pg_dump", result.Message);
    }
}
