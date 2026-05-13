namespace ProjectTest.Models;

public class SalesCommissionSnapshot
{
    public int UserId { get; set; }

    public string Salesperson { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public decimal Revenue { get; set; }

    public decimal Commission { get; set; }

    public int PaidOrders { get; set; }

    public string RevenueText => Helpers.CurrencyFormatter.ToCurrency(Revenue);

    public string CommissionText => Helpers.CurrencyFormatter.ToCurrency(Commission);
}
