namespace ProjectTest.Models;

public class OrderDraft
{
    public int Id { get; set; }

    public DateTime CreatedTime { get; set; } = DateTime.Now;

    public OrderStatus Status { get; set; } = OrderStatus.Created;

    public int? PromotionId { get; set; }

    public int? CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public int? CreatedByUserId { get; set; }

    public string SalespersonName { get; set; } = string.Empty;

    public decimal DiscountAmount { get; set; }

    public List<OrderDraftItem> Items { get; set; } = new();
}
