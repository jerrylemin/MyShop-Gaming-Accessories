using Microsoft.EntityFrameworkCore;
using ProjectTest.DataAccess;
using ProjectTest.Helpers;
using ProjectTest.Models;
using System.Globalization;

namespace ProjectTest.Services;

public class ReportingService
{
    private const int MaxLinePoints = 90;
    private const int MaxBarItems = 12;
    private const int MaxTopProducts = 8;
    private const int MaxPieItems = 6;

    private readonly MyShopDbContextFactory _dbContextFactory;
    private readonly MlInsightService? _mlInsightService;
    private readonly LlmAssistantService? _llmAssistantService;

    public ReportingService(MyShopDbContextFactory dbContextFactory, MlInsightService? mlInsightService = null, LlmAssistantService? llmAssistantService = null)
    {
        _dbContextFactory = dbContextFactory;
        _mlInsightService = mlInsightService;
        _llmAssistantService = llmAssistantService;
    }

    public async Task<ReportsSnapshot> GetSnapshotAsync(ReportQueryOptions? options = null, CancellationToken cancellationToken = default)
    {
        var snapshot = await GetCoreSnapshotAsync(options, cancellationToken).ConfigureAwait(false);
        var insights = await GetReportInsightsAsync(snapshot, cancellationToken).ConfigureAwait(false);
        snapshot.MlInsights = insights.MlInsights;
        snapshot.AssistantResult = insights.AssistantResult;
        return snapshot;
    }

    public async Task<ReportsSnapshot> GetCoreSnapshotAsync(ReportQueryOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new ReportQueryOptions();
        cancellationToken.ThrowIfCancellationRequested();

        await using var dbContext = _dbContextFactory.CreateDbContext();
        var normalizedFromDate = options.FromDate.Date;
        var normalizedToDate = options.ToDate.Date < normalizedFromDate ? normalizedFromDate : options.ToDate.Date;
        var rangeEndExclusive = normalizedToDate.AddDays(1);

        var paidItemsInRange = await dbContext.OrderItems
            .AsNoTracking()
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
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var revenueByDay = BuildLinePoints(normalizedFromDate, normalizedToDate, paidItemsInRange, snapshot => snapshot.Revenue, cancellationToken);
        var profitByDay = BuildLinePoints(normalizedFromDate, normalizedToDate, paidItemsInRange, snapshot => snapshot.Profit, cancellationToken);
        var revenueByWeek = LimitBars(BuildWeeklyBars(normalizedFromDate, normalizedToDate, paidItemsInRange, snapshot => snapshot.Revenue, cancellationToken));
        var profitByWeek = LimitBars(BuildWeeklyBars(normalizedFromDate, normalizedToDate, paidItemsInRange, snapshot => snapshot.Profit, cancellationToken));
        var revenueByMonth = LimitBars(BuildMonthlyBars(normalizedFromDate, normalizedToDate, paidItemsInRange, snapshot => snapshot.Revenue, cancellationToken));
        var profitByMonth = LimitBars(BuildMonthlyBars(normalizedFromDate, normalizedToDate, paidItemsInRange, snapshot => snapshot.Profit, cancellationToken));
        var revenueByYear = LimitBars(BuildYearlyBars(normalizedFromDate, normalizedToDate, paidItemsInRange, snapshot => snapshot.Revenue, "Revenue", cancellationToken));
        var profitByYear = LimitBars(BuildYearlyBars(normalizedFromDate, normalizedToDate, paidItemsInRange, snapshot => snapshot.Profit, "Profit", cancellationToken));

        var productSalesByRange = paidItemsInRange
            .GroupBy(x => new { x.ProductName, x.Manufacturer })
            .Select(group =>
            {
                var quantity = group.Sum(x => x.Quantity);
                return new BarChartItem
                {
                    Label = group.Key.ProductName,
                    Subtitle = group.Key.Manufacturer,
                    Value = quantity,
                    ValueLabel = quantity.ToString(CultureInfo.CurrentCulture)
                };
            })
            .OrderByDescending(x => x.Value)
            .Take(MaxTopProducts)
            .ToList();

        var productSalesShare = productSalesByRange
            .Take(MaxPieItems)
            .Select(x => new PieChartItem
            {
                Label = x.Label,
                Subtitle = x.Subtitle,
                Value = x.Value,
                ValueLabel = x.ValueLabel
            })
            .ToList();

        var commissions = await dbContext.Orders
            .AsNoTracking()
            .Where(x => x.Status == OrderStatus.Paid && x.CreatedTime >= normalizedFromDate && x.CreatedTime < rangeEndExclusive)
            .GroupBy(x => new
            {
                x.CreatedByUserId,
                DisplayName = x.CreatedByUser == null ? "Unassigned" : x.CreatedByUser.DisplayName,
                Role = x.CreatedByUser == null ? UserRole.Sale : x.CreatedByUser.Role
            })
            .Select(group => new SalesCommissionSnapshot
            {
                UserId = group.Key.CreatedByUserId ?? 0,
                Salesperson = group.Key.DisplayName,
                Role = group.Key.Role,
                Revenue = group.Sum(x => x.FinalPrice),
                PaidOrders = group.Count(),
                Commission = group.Sum(x => x.FinalPrice) * 0.03m
            })
            .OrderByDescending(x => x.Revenue)
            .Take(MaxBarItems)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

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
            ProductSalesShare = productSalesShare,
            SalesCommissions = commissions,
            AssistantResult = new AssistantResult { Summary = "Assistant summary is loading after the core report." }
        };
    }

    public async Task<(List<MlInsight> MlInsights, AssistantResult AssistantResult)> GetReportInsightsAsync(
        ReportsSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        List<MlInsight> mlInsights = _mlInsightService is null
            ? []
            : await _mlInsightService.GetInsightsAsync(cancellationToken).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
        snapshot.MlInsights = mlInsights;

        var assistantResult = _llmAssistantService is null
            ? new AssistantResult { Summary = "LLM assistant is not configured." }
            : await _llmAssistantService.AnalyzeReportsAsync(snapshot, cancellationToken).ConfigureAwait(false);

        return (mlInsights, assistantResult);
    }

    private static List<ChartPoint> BuildLinePoints(
        DateTime fromDate,
        DateTime toDate,
        IReadOnlyCollection<PaidOrderItemSnapshot> items,
        Func<PaidOrderItemSnapshot, decimal> selector,
        CancellationToken cancellationToken)
    {
        var dayCount = (toDate.Date - fromDate.Date).Days + 1;
        if (dayCount <= MaxLinePoints)
        {
            return BuildDailyLinePoints(fromDate, toDate, items, selector, cancellationToken);
        }

        if (dayCount <= 366)
        {
            return BuildWeeklyLinePoints(fromDate, toDate, items, selector, cancellationToken);
        }

        return BuildMonthlyLinePoints(fromDate, toDate, items, selector, cancellationToken)
            .TakeLast(MaxLinePoints)
            .ToList();
    }

    private static List<ChartPoint> BuildDailyLinePoints(
        DateTime fromDate,
        DateTime toDate,
        IReadOnlyCollection<PaidOrderItemSnapshot> items,
        Func<PaidOrderItemSnapshot, decimal> selector,
        CancellationToken cancellationToken)
    {
        var valuesByDay = items
            .GroupBy(x => x.CreatedTime.Date)
            .ToDictionary(group => group.Key, group => group.Sum(selector));

        var points = new List<ChartPoint>();
        foreach (var day in EachDate(fromDate, toDate))
        {
            cancellationToken.ThrowIfCancellationRequested();
            valuesByDay.TryGetValue(day.Date, out var value);
            points.Add(ToChartPoint(day.ToString("MM/dd"), value));
        }

        return points;
    }

    private static List<ChartPoint> BuildWeeklyLinePoints(
        DateTime fromDate,
        DateTime toDate,
        IReadOnlyCollection<PaidOrderItemSnapshot> items,
        Func<PaidOrderItemSnapshot, decimal> selector,
        CancellationToken cancellationToken)
    {
        var points = new List<ChartPoint>();
        var weekStart = StartOfWeek(fromDate);
        var finalWeekStart = StartOfWeek(toDate);

        for (var current = weekStart; current <= finalWeekStart; current = current.AddDays(7))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var nextWeek = current.AddDays(7);
            var value = items
                .Where(x => x.CreatedTime >= current && x.CreatedTime < nextWeek)
                .Sum(selector);
            points.Add(ToChartPoint($"W{ISOWeek.GetWeekOfYear(current)}", value));
        }

        return points;
    }

    private static List<ChartPoint> BuildMonthlyLinePoints(
        DateTime fromDate,
        DateTime toDate,
        IReadOnlyCollection<PaidOrderItemSnapshot> items,
        Func<PaidOrderItemSnapshot, decimal> selector,
        CancellationToken cancellationToken)
    {
        var points = new List<ChartPoint>();
        var monthStart = new DateTime(fromDate.Year, fromDate.Month, 1);
        var lastMonthStart = new DateTime(toDate.Year, toDate.Month, 1);

        for (var current = monthStart; current <= lastMonthStart; current = current.AddMonths(1))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var nextMonth = current.AddMonths(1);
            var value = items
                .Where(x => x.CreatedTime >= current && x.CreatedTime < nextMonth)
                .Sum(selector);
            points.Add(ToChartPoint($"{current:MMM yyyy}", value));
        }

        return points;
    }

    private static ChartPoint ToChartPoint(string label, decimal value)
    {
        return new ChartPoint
        {
            Label = label,
            Value = (double)value,
            ValueLabel = CurrencyFormatter.ToCurrency(value)
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

    private static List<BarChartItem> LimitBars(List<BarChartItem> bars)
    {
        return bars.Count <= MaxBarItems
            ? bars
            : bars.TakeLast(MaxBarItems).ToList();
    }

    private static List<BarChartItem> BuildWeeklyBars(
        DateTime fromDate,
        DateTime toDate,
        IReadOnlyCollection<PaidOrderItemSnapshot> items,
        Func<PaidOrderItemSnapshot, decimal> selector,
        CancellationToken cancellationToken)
    {
        var weekStart = StartOfWeek(fromDate);
        var finalWeekStart = StartOfWeek(toDate);
        var bars = new List<BarChartItem>();

        for (var current = weekStart; current <= finalWeekStart; current = current.AddDays(7))
        {
            cancellationToken.ThrowIfCancellationRequested();
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
        Func<PaidOrderItemSnapshot, decimal> selector,
        CancellationToken cancellationToken)
    {
        var monthStart = new DateTime(fromDate.Year, fromDate.Month, 1);
        var lastMonthStart = new DateTime(toDate.Year, toDate.Month, 1);
        var bars = new List<BarChartItem>();

        for (var current = monthStart; current <= lastMonthStart; current = current.AddMonths(1))
        {
            cancellationToken.ThrowIfCancellationRequested();
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
        string subtitle,
        CancellationToken cancellationToken)
    {
        var yearStart = new DateTime(fromDate.Year, 1, 1);
        var finalYearStart = new DateTime(toDate.Year, 1, 1);
        var bars = new List<BarChartItem>();

        for (var current = yearStart; current <= finalYearStart; current = current.AddYears(1))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var nextYear = current.AddYears(1);
            var value = items
                .Where(x => x.CreatedTime >= current && x.CreatedTime < nextYear)
                .Sum(selector);

            bars.Add(new BarChartItem
            {
                Label = current.Year.ToString(CultureInfo.CurrentCulture),
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
