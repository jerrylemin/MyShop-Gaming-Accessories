namespace ProjectTest.Models;

public class OrderDraft
{
    public int Id { get; set; }

    public DateTime CreatedTime { get; set; } = DateTime.Now;

    public OrderStatus Status { get; set; } = OrderStatus.Created;

    public List<OrderDraftItem> Items { get; set; } = new();
}
