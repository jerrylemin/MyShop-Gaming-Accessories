using System.Globalization;

namespace ProjectTest.Helpers;

public static class CurrencyFormatter
{
    private static readonly CultureInfo VndCulture = new("vi-VN");

    public static string ToCurrency(decimal value)
    {
        return value.ToString("C0", VndCulture);
    }
}
