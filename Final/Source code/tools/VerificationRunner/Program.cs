using ProjectTest.DataAccess;
using ProjectTest.Models;
using ProjectTest.Repositories;
using ProjectTest.Services;

var connectionString = args.Length > 0
    ? args[0]
    : "Host=localhost;Port=5432;Database=myshop_gaming_accessories;Username=postgres;Password=jelly;Include Error Detail=true";

var dbContextFactory = new MyShopDbContextFactory(connectionString);
var productRepository = new ProductRepository(dbContextFactory);
var orderRepository = new OrderRepository(dbContextFactory);
var dashboardService = new DashboardService(dbContextFactory);
var reportingService = new ReportingService(dbContextFactory);

var availableProducts = await productRepository.GetLookupAsync();
var product = availableProducts.First(x => x.Stock > 0);
var stockBefore = product.Stock;

var saveResult = await orderRepository.SaveAsync(new OrderDraft
{
    CreatedTime = DateTime.Now,
    Status = OrderStatus.Paid,
    Items =
    [
        new OrderDraftItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Manufacturer = product.Manufacturer,
            UnitSalePrice = product.SalePrice,
            Quantity = 1,
            AvailableStock = product.Stock,
            ImagePath = product.ImagePath
        }
    ]
});

if (!saveResult.Success)
{
    throw new InvalidOperationException(saveResult.Message);
}

var createdOrder = await orderRepository.GetDraftByIdAsync(saveResult.Value)
    ?? throw new InvalidOperationException("The created order could not be reloaded.");
var updatedProduct = await productRepository.GetByIdAsync(product.Id)
    ?? throw new InvalidOperationException("The ordered product could not be reloaded.");
var dashboard = await dashboardService.GetSnapshotAsync();
var reports = await reportingService.GetSnapshotAsync(new ReportQueryOptions
{
    FromDate = DateTime.Today.AddDays(-7),
    ToDate = DateTime.Today
});

Console.WriteLine($"CreatedOrderId={saveResult.Value}");
Console.WriteLine($"OrderItemCount={createdOrder.Items.Count}");
Console.WriteLine($"StockBefore={stockBefore}");
Console.WriteLine($"StockAfter={updatedProduct.Stock}");
Console.WriteLine($"DashboardTotalProducts={dashboard.TotalProducts}");
Console.WriteLine($"DashboardLowStockProducts={dashboard.LowStockProducts}");
Console.WriteLine($"DashboardTodayRevenue={dashboard.TodayRevenue}");
Console.WriteLine($"DashboardRecentOrders={dashboard.LatestOrders.Count}");
Console.WriteLine($"ReportRevenueByDayPoints={reports.RevenueByDay.Count}");
Console.WriteLine($"ReportProfitByDayPoints={reports.ProfitByDay.Count}");
Console.WriteLine($"ReportRevenueByMonthBars={reports.RevenueByMonth.Count}");
Console.WriteLine($"ReportProfitByMonthBars={reports.ProfitByMonth.Count}");
Console.WriteLine($"ReportTotalProfit={reports.TotalProfit}");
Console.WriteLine($"ReportTopProducts={reports.ProductSalesByRange.Count}");
