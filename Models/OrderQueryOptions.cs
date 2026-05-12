namespace ProjectTest.Models;

public class OrderQueryOptions
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string Keyword { get; set; } = string.Empty;

    public OrderStatus? Status { get; set; }

    public OrderSortOption SortOption { get; set; } = OrderSortOption.LatestFirst;

    public bool SortDescending { get; set; }

    public decimal? MinTotal { get; set; }

    public decimal? MaxTotal { get; set; }

    public int? CustomerId { get; set; }

    public int? CurrentUserId { get; set; }

    public UserRole CurrentUserRole { get; set; } = UserRole.Admin;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
