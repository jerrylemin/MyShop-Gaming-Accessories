using Microsoft.EntityFrameworkCore;
using ProjectTest.DataAccess;
using ProjectTest.Helpers;
using ProjectTest.Models;

namespace ProjectTest.Services;

public class DashboardService
{
    private readonly MyShopDbContextFactory _dbContextFactory;

    public DashboardService(MyShopDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<DashboardSnapshot> GetSnapshotAsync()
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        var revenueDates = Enumerable.Range(0, daysInMonth).Select(offset => monthStart.AddDays(offset)).ToList();

        var revenuePoints = new List<ChartPoint>();
        foreach (var day in revenueDates)
        {
            var nextDay = day.AddDays(1);
            var revenue = await dbContext.Orders
                .Where(x => x.Status == OrderStatus.Paid && x.CreatedTime >= day && x.CreatedTime < nextDay)
                .SumAsync(x => (decimal?)x.FinalPrice) ?? 0m;

            revenuePoints.Add(new ChartPoint
            {
                Label = day.ToString("MM/dd"),
                Value = (double)revenue,
                ValueLabel = CurrencyFormatter.ToCurrency(revenue)
            });
        }

        return new DashboardSnapshot
        {
            TotalProducts = await dbContext.Products.CountAsync(),
            LowStockProducts = await dbContext.Products.CountAsync(x => x.Stock <= 5),
            TodayOrderCount = await dbContext.Orders.CountAsync(x => x.CreatedTime >= today && x.CreatedTime < today.AddDays(1)),
            TodayRevenue = await dbContext.Orders
                .Where(x => x.Status == OrderStatus.Paid && x.CreatedTime >= today && x.CreatedTime < today.AddDays(1))
                .SumAsync(x => (decimal?)x.FinalPrice) ?? 0m,
            RevenuePoints = revenuePoints,
            TopLowStockProducts = await dbContext.Products
                .Include(x => x.Category)
                .OrderBy(x => x.Stock)
                .ThenBy(x => x.Name)
                .Take(5)
                .ToListAsync(),
            LatestOrders = await dbContext.Orders
                .Include(x => x.Items)
                .OrderByDescending(x => x.CreatedTime)
                .Take(3)
                .Select(x => new OrderSummary
                {
                    Id = x.Id,
                    CreatedTime = x.CreatedTime,
                    FinalPrice = x.FinalPrice,
                    Status = x.Status,
                    ItemCount = x.Items.Count
                })
                .ToListAsync(),
            TopSellingProducts = await dbContext.OrderItems
                .Where(x => x.Order != null && x.Order.Status == OrderStatus.Paid)
                .GroupBy(x => new { x.ProductId, x.Product!.Name, x.Product.Manufacturer })
                .Select(group => new ProductSalesSummary
                {
                    ProductId = group.Key.ProductId,
                    ProductName = group.Key.Name,
                    Manufacturer = group.Key.Manufacturer,
                    QuantitySold = group.Sum(x => x.Quantity),
                    Revenue = group.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5)
                .ToListAsync()
        };
    }
}
