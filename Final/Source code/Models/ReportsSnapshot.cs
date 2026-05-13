namespace ProjectTest.Models;

public class ReportsSnapshot
{
    public string RangeLabel { get; set; } = string.Empty;

    public decimal TotalRevenue { get; set; }

    public decimal TotalProfit { get; set; }

    public List<ChartPoint> RevenueByDay { get; set; } = new();

    public List<ChartPoint> ProfitByDay { get; set; } = new();

    public List<BarChartItem> RevenueByWeek { get; set; } = new();

    public List<BarChartItem> ProfitByWeek { get; set; } = new();

    public List<BarChartItem> RevenueByMonth { get; set; } = new();

    public List<BarChartItem> ProfitByMonth { get; set; } = new();

    public List<BarChartItem> RevenueByYear { get; set; } = new();

    public List<BarChartItem> ProfitByYear { get; set; } = new();

    public List<BarChartItem> ProductSalesByRange { get; set; } = new();

    public List<PieChartItem> ProductSalesShare { get; set; } = new();

    public List<SalesCommissionSnapshot> SalesCommissions { get; set; } = new();

    public List<MlInsight> MlInsights { get; set; } = new();

    public AssistantResult AssistantResult { get; set; } = new();
}
