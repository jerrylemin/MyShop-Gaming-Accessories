namespace ProjectTest.Models;

public class DashboardSnapshot
{
    public int TotalProducts { get; set; }

    public int LowStockProducts { get; set; }

    public int TodayOrderCount { get; set; }

    public decimal TodayRevenue { get; set; }

    public List<ChartPoint> RevenuePoints { get; set; } = new();

    public List<Product> TopLowStockProducts { get; set; } = new();

    public List<ProductSalesSummary> TopSellingProducts { get; set; } = new();

    public List<OrderSummary> LatestOrders { get; set; } = new();
}
