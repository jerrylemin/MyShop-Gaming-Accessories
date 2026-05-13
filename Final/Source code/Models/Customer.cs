namespace ProjectTest.Models;

public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int LoyaltyPoints { get; set; }

    public decimal LifetimeSpend { get; set; }

    public string LoyaltySummary => $"{LoyaltyPoints} points";

    public string LifetimeSpendText => Helpers.CurrencyFormatter.ToCurrency(LifetimeSpend);

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
