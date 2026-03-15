namespace ProjectTest.Models;

public class OrderSummary
{
    public int Id { get; set; }

    public DateTime CreatedTime { get; set; }

    public decimal FinalPrice { get; set; }

    public OrderStatus Status { get; set; }

    public int ItemCount { get; set; }

    public string OrderLabel => $"Order #{Id}";

    public string CreatedDisplay => CreatedTime.ToString("dd MMM yyyy HH:mm");

    public string FinalPriceText => Helpers.CurrencyFormatter.ToCurrency(FinalPrice);

    public string StatusLabel => Status.ToString();
}
