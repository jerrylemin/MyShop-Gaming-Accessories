using Microsoft.EntityFrameworkCore;
using ProjectTest.DataAccess;
using ProjectTest.Helpers;
using ProjectTest.Models;

namespace ProjectTest.Services;

public class ReportingService
{
    private readonly MyShopDbContextFactory _dbContextFactory;

    public ReportingService(MyShopDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<ReportsSnapshot> GetSnapshotAsync(ReportQueryOptions? options = null)
    {
        options ??= new ReportQueryOptions();

        await using var dbContext = _dbContextFactory.CreateDbContext();
        var today = DateTime.Today;
        var normalizedFromDate = options.FromDate.Date;
        var normalizedToDate = options.ToDate.Date < normalizedFromDate ? normalizedFromDate : options.ToDate.Date;
        var rangeEndExclusive = normalizedToDate.AddDays(1);

        var paidOrdersInRange = await dbContext.Orders
            .Where(x => x.Status == OrderStatus.Paid && x.CreatedTime >= normalizedFromDate && x.CreatedTime < rangeEndExclusive)
            .ToListAsync();

        var revenueByDay = new List<ChartPoint>();
        foreach (var day in EachDate(normalizedFromDate, normalizedToDate))
        {
            var nextDay = day.AddDays(1);
            var revenue = paidOrdersInRange
                .Where(x => x.CreatedTime >= day && x.CreatedTime < nextDay)
                .Sum(x => x.FinalPrice);

            revenueByDay.Add(new ChartPoint
            {
                Label = day.ToString("MM/dd"),
                Value = (double)revenue,
                ValueLabel = CurrencyFormatter.ToCurrency(revenue)
            });
        }

        var revenueByWeek = Enumerable.Range(0, 8)
            .Select(offset => StartOfWeek(today).AddDays(-(7 * (7 - offset))))
            .Select(weekStart =>
            {
                var weekEnd = weekStart.AddDays(7);
                var revenue = paidOrdersInRange
                    .Where(x => x.CreatedTime >= weekStart && x.CreatedTime < weekEnd)
                    .Sum(x => x.FinalPrice);

                return new BarChartItem
                {
                    Label = $"W{System.Globalization.ISOWeek.GetWeekOfYear(weekStart)}",
                    Subtitle = weekStart.ToString("dd MMM"),
                    Value = (double)revenue,
                    ValueLabel = CurrencyFormatter.ToCurrency(revenue)
                };
            })
            .ToList();

        var revenueByMonth = new List<BarChartItem>();
        for (var offset = 11; offset >= 0; offset--)
        {
            var monthStart = new DateTime(today.Year, today.Month, 1).AddMonths(-offset);
            var monthEnd = monthStart.AddMonths(1);
            var revenue = await dbContext.Orders
                .Where(x => x.Status == OrderStatus.Paid && x.CreatedTime >= monthStart && x.CreatedTime < monthEnd)
                .SumAsync(x => (decimal?)x.FinalPrice) ?? 0m;

            revenueByMonth.Add(new BarChartItem
            {
                Label = monthStart.ToString("MMM"),
                Subtitle = monthStart.ToString("yyyy"),
                Value = (double)revenue,
                ValueLabel = CurrencyFormatter.ToCurrency(revenue)
            });
        }

        var revenueByYear = new List<BarChartItem>();
        for (var offset = 4; offset >= 0; offset--)
        {
            var year = today.Year - offset;
            var yearStart = new DateTime(year, 1, 1);
            var yearEnd = yearStart.AddYears(1);
            var revenue = await dbContext.Orders
                .Where(x => x.Status == OrderStatus.Paid && x.CreatedTime >= yearStart && x.CreatedTime < yearEnd)
                .SumAsync(x => (decimal?)x.FinalPrice) ?? 0m;

            revenueByYear.Add(new BarChartItem
            {
                Label = year.ToString(),
                Subtitle = "Revenue",
                Value = (double)revenue,
                ValueLabel = CurrencyFormatter.ToCurrency(revenue)
            });
        }

        var productSalesByRange = await dbContext.OrderItems
            .Where(x => x.Order != null &&
                        x.Order.Status == OrderStatus.Paid &&
                        x.Order.CreatedTime >= normalizedFromDate &&
                        x.Order.CreatedTime < rangeEndExclusive)
            .GroupBy(x => new { x.ProductId, x.Product!.Name, x.Product.Manufacturer })
            .Select(group => new BarChartItem
            {
                Label = group.Key.Name,
                Subtitle = group.Key.Manufacturer,
                Value = group.Sum(x => x.Quantity),
                ValueLabel = group.Sum(x => x.Quantity).ToString()
            })
            .OrderByDescending(x => x.Value)
            .Take(8)
            .ToListAsync();

        var productSalesShare = productSalesByRange
            .Take(6)
            .Select(x => new PieChartItem
            {
                Label = x.Label,
                Subtitle = x.Subtitle,
                Value = x.Value,
                ValueLabel = x.ValueLabel
            })
            .ToList();

        return new ReportsSnapshot
        {
            RangeLabel = $"{normalizedFromDate:dd MMM yyyy} - {normalizedToDate:dd MMM yyyy}",
            RevenueByDay = revenueByDay,
            RevenueByWeek = revenueByWeek,
            RevenueByMonth = revenueByMonth,
            RevenueByYear = revenueByYear,
            ProductSalesByRange = productSalesByRange,
            ProductSalesShare = productSalesShare
        };
    }

    private static IEnumerable<DateTime> EachDate(DateTime startDate, DateTime endDate)
    {
        for (var day = startDate.Date; day <= endDate.Date; day = day.AddDays(1))
        {
            yield return day;
        }
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-offset);
    }
}
