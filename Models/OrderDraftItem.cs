namespace ProjectTest.Models;

public class OrderDraftItem
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public decimal UnitSalePrice { get; set; }

    public int Quantity { get; set; } = 1;

    public int AvailableStock { get; set; }

    public string ImagePath { get; set; } = string.Empty;

    public decimal TotalPrice => UnitSalePrice * Quantity;
}
