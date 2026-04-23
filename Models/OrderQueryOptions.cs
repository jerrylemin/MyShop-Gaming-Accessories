namespace ProjectTest.Models;

public class OrderQueryOptions
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string Keyword { get; set; } = string.Empty;

    public OrderStatus? Status { get; set; }

    public OrderSortOption SortOption { get; set; } = OrderSortOption.LatestFirst;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
