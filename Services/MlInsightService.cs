using Microsoft.EntityFrameworkCore;
using ProjectTest.DataAccess;
using ProjectTest.Models;

namespace ProjectTest.Services;

public class MlInsightService
{
    private readonly MyShopDbContextFactory _dbContextFactory;

    public MlInsightService(MyShopDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<MlInsight>> GetInsightsAsync()
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var since = DateTime.Today.AddDays(-30);
        var velocity = await dbContext.OrderItems
            .Where(x => x.Order != null && x.Order.Status == OrderStatus.Paid && x.Order.CreatedTime >= since)
            .GroupBy(x => new { x.ProductId, x.Product!.Name, x.Product.Stock })
            .Select(group => new
            {
                group.Key.Name,
                group.Key.Stock,
                Sold = group.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.Sold)
            .Take(5)
            .ToListAsync();

        var insights = velocity
            .Select(x =>
            {
                var dailySales = x.Sold / 30m;
                var daysLeft = dailySales <= 0 ? 999m : x.Stock / dailySales;
                return new MlInsight
                {
                    Title = daysLeft <= 7 ? "Restock soon" : "Stock healthy",
                    Detail = $"{x.Name}: about {daysLeft:N1} days of stock remaining based on 30-day sales.",
                    Score = decimal.Round(daysLeft, 1)
                };
            })
            .ToList();

        if (insights.Count == 0)
        {
            insights.Add(new MlInsight { Title = "Revenue forecast", Detail = "Not enough paid order history to forecast yet.", Score = 0m });
        }

        return insights;
    }
}
