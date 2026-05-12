namespace ProjectTest.Models;

public class CustomerLoyaltyTransaction
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int? OrderId { get; set; }

    public int Points { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime CreatedTime { get; set; } = DateTime.Now;

    public Customer? Customer { get; set; }

    public Order? Order { get; set; }
}
