namespace ProjectTest.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitSalePrice { get; set; }

    public decimal UnitCostPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public decimal TotalCost => UnitCostPrice * Quantity;

    public decimal Profit => TotalPrice - TotalCost;

    public Order? Order { get; set; }

    public Product? Product { get; set; }
}
