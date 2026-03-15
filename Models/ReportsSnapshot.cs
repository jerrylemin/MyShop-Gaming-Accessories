namespace ProjectTest.Models;

public class ReportsSnapshot
{
    public string RangeLabel { get; set; } = string.Empty;

    public List<ChartPoint> RevenueByDay { get; set; } = new();

    public List<BarChartItem> RevenueByWeek { get; set; } = new();

    public List<BarChartItem> RevenueByMonth { get; set; } = new();

    public List<BarChartItem> RevenueByYear { get; set; } = new();

    public List<BarChartItem> ProductSalesByRange { get; set; } = new();

    public List<PieChartItem> ProductSalesShare { get; set; } = new();
}
