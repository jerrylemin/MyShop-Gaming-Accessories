using Microsoft.UI.Xaml.Data;

namespace ProjectTest.Helpers;

public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            decimal decimalValue => CurrencyFormatter.ToCurrency(decimalValue),
            double doubleValue => CurrencyFormatter.ToCurrency((decimal)doubleValue),
            int intValue => intValue.ToString(),
            _ => value?.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
