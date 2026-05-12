namespace ProjectTest.Models;

public class ProductQueryOptions
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string Keyword { get; set; } = string.Empty;

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public int? CategoryId { get; set; }

    public ProductSortOption SortOption { get; set; } = ProductSortOption.Name;

    public bool SortDescending { get; set; }

    public string Manufacturer { get; set; } = string.Empty;

    public int? MinStock { get; set; }

    public int? MaxStock { get; set; }

    public UserRole CurrentUserRole { get; set; } = UserRole.Admin;
}
