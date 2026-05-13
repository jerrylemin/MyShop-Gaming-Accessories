namespace ProjectTest.Models;

public class Product
{
    public int Id { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public string CPU { get; set; } = string.Empty;

    public string RAM { get; set; } = string.Empty;

    public string Storage { get; set; } = string.Empty;

    public string GPU { get; set; } = string.Empty;

    public string Screen { get; set; } = string.Empty;

    public decimal ImportPrice { get; set; }

    public decimal SalePrice { get; set; }

    public int Stock { get; set; }

    public int CategoryId { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Image1 { get; set; } = string.Empty;

    public string Image2 { get; set; } = string.Empty;

    public string Image3 { get; set; } = string.Empty;

    public string Brand => Manufacturer;

    public IReadOnlyList<string> AccessorySpecs =>
        [.. new[] { CPU, RAM, Storage, GPU, Screen }.Where(x => !string.IsNullOrWhiteSpace(x))];

    public Category? Category { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
