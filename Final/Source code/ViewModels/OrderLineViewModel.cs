using ProjectTest.Helpers;

namespace ProjectTest.ViewModels;

public class OrderLineViewModel : ViewModelBase
{
    private int _quantity = 1;

    public int ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string Manufacturer { get; init; } = string.Empty;

    public decimal UnitSalePrice { get; init; }

    public int AvailableStock { get; init; }

    public string ImagePath { get; init; } = string.Empty;

    public int Quantity
    {
        get => _quantity;
        set
        {
            var normalized = Math.Max(1, value);
            if (SetProperty(ref _quantity, normalized))
            {
                OnPropertyChanged(nameof(QuantityValue));
                OnPropertyChanged(nameof(LineTotal));
                OnPropertyChanged(nameof(LineTotalText));
                LineChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public double QuantityValue
    {
        get => Quantity;
        set => Quantity = Math.Max(1, (int)Math.Round(value));
    }

    public decimal LineTotal => UnitSalePrice * Quantity;

    public string LineTotalText => CurrencyFormatter.ToCurrency(LineTotal);

    public event EventHandler? LineChanged;
}
