using ProjectTest.Helpers;

namespace ProjectTest.Models;

public class ProductLookupItem
{
    public int Id { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public decimal SalePrice { get; set; }

    public int Stock { get; set; }

    public string ImagePath { get; set; } = string.Empty;

    public string DisplayName => $"{SKU} | {Manufacturer} {Name} | {CurrencyFormatter.ToCurrency(SalePrice)} | Stock {Stock}";

    public string SearchText => $"{SKU} {Manufacturer} {Name} {SalePrice} {Stock}".ToLowerInvariant();
}
