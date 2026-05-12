using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using ProjectTest.DataAccess;
using ProjectTest.Helpers;
using ProjectTest.Models;

namespace ProjectTest.Services;

public class MlInsightService
{
    private const int MinimumTrainingDays = 14;
    private readonly MyShopDbContextFactory _dbContextFactory;
    private readonly MLContext _mlContext = new(seed: 20260512);

    public MlInsightService(MyShopDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<MlInsight>> GetInsightsAsync()
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var insights = new List<MlInsight>();

        var revenueTrainingRows = await BuildRevenueTrainingRowsAsync(dbContext);
        if (revenueTrainingRows.Count >= MinimumTrainingDays)
        {
            insights.Add(BuildRevenueForecastInsight(revenueTrainingRows));
            insights.Add(new MlInsight
            {
                Title = "ML.Net data confidence",
                Detail = $"Regression trained with {revenueTrainingRows.Count} paid-sales days from the database. Treat as directional for demo data.",
                Score = Math.Min(100m, revenueTrainingRows.Count * 4m)
            });
        }
        else
        {
            insights.Add(new MlInsight
            {
                Title = "ML.Net revenue forecast",
                Detail = $"Only {revenueTrainingRows.Count} paid-sales day(s) found. Need at least {MinimumTrainingDays} days before training the ML.Net regression model; showing stock velocity fallback below.",
                Score = revenueTrainingRows.Count
            });
        }

        insights.AddRange(await BuildRestockVelocityInsightsAsync(dbContext));
        return insights;
    }

    private async Task<List<RevenueTrainingRow>> BuildRevenueTrainingRowsAsync(MyShopDbContext dbContext)
    {
        var since = DateTime.Today.AddDays(-120);
        var dailyRevenue = await dbContext.Orders
            .Where(x => x.Status == OrderStatus.Paid && x.CreatedTime >= since)
            .GroupBy(x => x.CreatedTime.Date)
            .Select(group => new
            {
                Date = group.Key,
                Revenue = group.Sum(x => x.FinalPrice)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        if (dailyRevenue.Count == 0)
        {
            return [];
        }

        var firstDate = dailyRevenue[0].Date;
        return dailyRevenue
            .Select(x => new RevenueTrainingRow
            {
                DayIndex = (float)(x.Date - firstDate).TotalDays,
                DayOfWeek = (float)x.Date.DayOfWeek,
                Revenue = (float)x.Revenue
            })
            .ToList();
    }

    private MlInsight BuildRevenueForecastInsight(IReadOnlyList<RevenueTrainingRow> rows)
    {
        var data = _mlContext.Data.LoadFromEnumerable(rows);
        var pipeline = _mlContext.Transforms.Concatenate("Features", nameof(RevenueTrainingRow.DayIndex), nameof(RevenueTrainingRow.DayOfWeek))
            .Append(_mlContext.Regression.Trainers.Sdca(labelColumnName: nameof(RevenueTrainingRow.Revenue), featureColumnName: "Features"));

        var model = pipeline.Fit(data);
        var engine = _mlContext.Model.CreatePredictionEngine<RevenueTrainingRow, RevenuePrediction>(model);
        var lastDayIndex = rows.Max(x => x.DayIndex);
        var forecasts = Enumerable.Range(1, 7)
            .Select(offset => Math.Max(0f, engine.Predict(new RevenueTrainingRow
            {
                DayIndex = lastDayIndex + offset,
                DayOfWeek = (float)DateTime.Today.AddDays(offset).DayOfWeek
            }).Score))
            .ToList();

        var totalForecast = forecasts.Sum(x => (decimal)x);
        var averageForecast = forecasts.Count == 0 ? 0m : totalForecast / forecasts.Count;
        return new MlInsight
        {
            Title = "ML.Net 7-day revenue forecast",
            Detail = $"Next 7 days forecast: {CurrencyFormatter.ToCurrency(totalForecast)} total, about {CurrencyFormatter.ToCurrency(averageForecast)} per day.",
            Score = decimal.Round(totalForecast, 0)
        };
    }

    private static async Task<List<MlInsight>> BuildRestockVelocityInsightsAsync(MyShopDbContext dbContext)
    {
        var since = DateTime.Today.AddDays(-30);
        var velocity = await dbContext.OrderItems
            .Where(x => x.Order != null && x.Order.Status == OrderStatus.Paid && x.Order.CreatedTime >= since)
            .GroupBy(x => new
            {
                x.ProductId,
                ProductName = x.Product!.Name,
                x.Product.Stock,
                CategoryName = x.Product.Category!.Name,
                x.Product.SalePrice
            })
            .Select(group => new
            {
                Name = group.Key.ProductName,
                group.Key.Stock,
                group.Key.CategoryName,
                group.Key.SalePrice,
                Sold = group.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.Sold)
            .Take(5)
            .ToListAsync();

        if (velocity.Count == 0)
        {
            return
            [
                new MlInsight
                {
                    Title = "Restock insight",
                    Detail = "No paid order history in the last 30 days, so restock recommendations are not available yet.",
                    Score = 0m
                }
            ];
        }

        return velocity
            .Select(x =>
            {
                var dailySales = x.Sold / 30m;
                var daysLeft = dailySales <= 0 ? 999m : x.Stock / dailySales;
                var urgency = daysLeft <= 7 ? "Restock soon" : "Stock healthy";
                return new MlInsight
                {
                    Title = urgency,
                    Detail = $"{x.Name}: {x.Stock} in stock, {x.Sold} sold in 30 days, about {daysLeft:N1} days remaining.",
                    Score = decimal.Round(daysLeft, 1)
                };
            })
            .ToList();
    }

    private sealed class RevenueTrainingRow
    {
        public float DayIndex { get; set; }

        public float DayOfWeek { get; set; }

        public float Revenue { get; set; }
    }

    private sealed class RevenuePrediction
    {
        public float Score { get; set; }
    }
}
