using Microsoft.EntityFrameworkCore;
using ProjectTest.DataAccess;
using ProjectTest.Helpers;
using ProjectTest.Models;
using System.Globalization;

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
        var normalizedFromDate = options.FromDate.Date;
        var normalizedToDate = options.ToDate.Date < normalizedFromDate ? normalizedFromDate : options.ToDate.Date;
        var rangeEndExclusive = normalizedToDate.AddDays(1);

        var paidItemsInRange = await dbContext.OrderItems
            .Where(x => x.Order != null && x.Order.Status == OrderStatus.Paid && x.Order.CreatedTime >= normalizedFromDate && x.Order.CreatedTime < rangeEndExclusive)
            .Select(x => new PaidOrderItemSnapshot
            {
                CreatedTime = x.Order!.CreatedTime,
                Revenue = x.TotalPrice,
                Cost = x.UnitCostPrice * x.Quantity,
                Quantity = x.Quantity,
                ProductName = x.Product!.Name,
                Manufacturer = x.Product.Manufacturer
            })
            .ToListAsync();

        var revenueByDay = new List<ChartPoint>();
        var profitByDay = new List<ChartPoint>();
        foreach (var day in EachDate(normalizedFromDate, normalizedToDate))
        {
            var nextDay = day.AddDays(1);
            var revenue = paidItemsInRange
                .Where(x => x.CreatedTime >= day && x.CreatedTime < nextDay)
                .Sum(x => x.Revenue);
            var profit = paidItemsInRange
                .Where(x => x.CreatedTime >= day && x.CreatedTime < nextDay)
                .Sum(x => x.Profit);

            revenueByDay.Add(new ChartPoint
            {
                Label = day.ToString("MM/dd"),
                Value = (double)revenue,
                ValueLabel = CurrencyFormatter.ToCurrency(revenue)
            });

            profitByDay.Add(new ChartPoint
            {
                Label = day.ToString("MM/dd"),
                Value = (double)profit,
                ValueLabel = CurrencyFormatter.ToCurrency(profit)
            });
        }

        var revenueByWeek = BuildWeeklyBars(normalizedFromDate, normalizedToDate, paidItemsInRange, snapshot => snapshot.Revenue);
        var profitByWeek = BuildWeeklyBars(normalizedFromDate, normalizedToDate, paidItemsInRange, snapshot => snapshot.Profit);
        var revenueByMonth = BuildMonthlyBars(normalizedFromDate, normalizedToDate, paidItemsInRange, snapshot => snapshot.Revenue);
        var profitByMonth = BuildMonthlyBars(normalizedFromDate, normalizedToDate, paidItemsInRange, snapshot => snapshot.Profit);
        var revenueByYear = BuildYearlyBars(normalizedFromDate, normalizedToDate, paidItemsInRange, snapshot => snapshot.Revenue, "Revenue");
        var profitByYear = BuildYearlyBars(normalizedFromDate, normalizedToDate, paidItemsInRange, snapshot => snapshot.Profit, "Profit");

        var productSalesByRange = paidItemsInRange
            .GroupBy(x => new { x.ProductName, x.Manufacturer })
            .Select(group => new BarChartItem
            {
                Label = group.Key.ProductName,
                Subtitle = group.Key.Manufacturer,
                Value = group.Sum(x => x.Quantity),
                ValueLabel = group.Sum(x => x.Quantity).ToString()
            })
            .OrderByDescending(x => x.Value)
            .Take(8)
            .ToList();

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
            TotalRevenue = paidItemsInRange.Sum(x => x.Revenue),
            TotalProfit = paidItemsInRange.Sum(x => x.Profit),
            RevenueByDay = revenueByDay,
            ProfitByDay = profitByDay,
            RevenueByWeek = revenueByWeek,
            ProfitByWeek = profitByWeek,
            RevenueByMonth = revenueByMonth,
            ProfitByMonth = profitByMonth,
            RevenueByYear = revenueByYear,
            ProfitByYear = profitByYear,
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

    private static List<BarChartItem> BuildWeeklyBars(
        DateTime fromDate,
        DateTime toDate,
        IReadOnlyCollection<PaidOrderItemSnapshot> items,
        Func<PaidOrderItemSnapshot, decimal> selector)
    {
        var weekStart = StartOfWeek(fromDate);
        var finalWeekStart = StartOfWeek(toDate);
        var bars = new List<BarChartItem>();

        for (var current = weekStart; current <= finalWeekStart; current = current.AddDays(7))
        {
            var nextWeek = current.AddDays(7);
            var value = items
                .Where(x => x.CreatedTime >= current && x.CreatedTime < nextWeek)
                .Sum(selector);

            bars.Add(new BarChartItem
            {
                Label = $"W{ISOWeek.GetWeekOfYear(current)}",
                Subtitle = $"{current:dd MMM}",
                Value = (double)value,
                ValueLabel = CurrencyFormatter.ToCurrency(value)
            });
        }

        return bars;
    }

    private static List<BarChartItem> BuildMonthlyBars(
        DateTime fromDate,
        DateTime toDate,
        IReadOnlyCollection<PaidOrderItemSnapshot> items,
        Func<PaidOrderItemSnapshot, decimal> selector)
    {
        var monthStart = new DateTime(fromDate.Year, fromDate.Month, 1);
        var lastMonthStart = new DateTime(toDate.Year, toDate.Month, 1);
        var bars = new List<BarChartItem>();

        for (var current = monthStart; current <= lastMonthStart; current = current.AddMonths(1))
        {
            var nextMonth = current.AddMonths(1);
            var value = items
                .Where(x => x.CreatedTime >= current && x.CreatedTime < nextMonth)
                .Sum(selector);

            bars.Add(new BarChartItem
            {
                Label = current.ToString("MMM"),
                Subtitle = current.ToString("yyyy"),
                Value = (double)value,
                ValueLabel = CurrencyFormatter.ToCurrency(value)
            });
        }

        return bars;
    }

    private static List<BarChartItem> BuildYearlyBars(
        DateTime fromDate,
        DateTime toDate,
        IReadOnlyCollection<PaidOrderItemSnapshot> items,
        Func<PaidOrderItemSnapshot, decimal> selector,
        string subtitle)
    {
        var yearStart = new DateTime(fromDate.Year, 1, 1);
        var finalYearStart = new DateTime(toDate.Year, 1, 1);
        var bars = new List<BarChartItem>();

        for (var current = yearStart; current <= finalYearStart; current = current.AddYears(1))
        {
            var nextYear = current.AddYears(1);
            var value = items
                .Where(x => x.CreatedTime >= current && x.CreatedTime < nextYear)
                .Sum(selector);

            bars.Add(new BarChartItem
            {
                Label = current.Year.ToString(),
                Subtitle = subtitle,
                Value = (double)value,
                ValueLabel = CurrencyFormatter.ToCurrency(value)
            });
        }

        return bars;
    }

    private sealed class PaidOrderItemSnapshot
    {
        public DateTime CreatedTime { get; set; }

        public decimal Revenue { get; set; }

        public decimal Cost { get; set; }

        public decimal Profit => Revenue - Cost;

        public int Quantity { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string Manufacturer { get; set; } = string.Empty;
    }
}
