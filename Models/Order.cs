namespace ProjectTest.Models;

public class Order
{
    public int Id { get; set; }

    public DateTime CreatedTime { get; set; }

    public decimal FinalPrice { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public int? PromotionId { get; set; }

    public int? CustomerId { get; set; }

    public int? CreatedByUserId { get; set; }

    public OrderStatus Status { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public Promotion? Promotion { get; set; }

    public Customer? Customer { get; set; }

    public AppUser? CreatedByUser { get; set; }
}
