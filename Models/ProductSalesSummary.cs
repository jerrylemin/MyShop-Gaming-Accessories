namespace ProjectTest.Models;

public class ProductSalesSummary
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public int QuantitySold { get; set; }

    public decimal Revenue { get; set; }

    public string RevenueText => Helpers.CurrencyFormatter.ToCurrency(Revenue);
}
