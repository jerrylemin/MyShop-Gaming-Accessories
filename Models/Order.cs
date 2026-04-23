namespace ProjectTest.Models;

public class Order
{
    public int Id { get; set; }

    public DateTime CreatedTime { get; set; }

    public decimal FinalPrice { get; set; }

    public OrderStatus Status { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
