namespace ProjectTest.Models;

public class CategoryListItem
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int ProductCount { get; set; }

    public string ProductCountText => $"{ProductCount} products";

    public bool CanDelete => ProductCount == 0;
}
