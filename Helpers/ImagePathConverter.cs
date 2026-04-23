using Microsoft.UI.Xaml.Data;

namespace ProjectTest.Helpers;

public class ImagePathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return ImageSourceHelper.ToBitmap(value as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
