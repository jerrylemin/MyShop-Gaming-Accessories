using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;

namespace ProjectTest.Helpers;

public static class ImageSourceHelper
{
    public const string DefaultProductImagePath = "ms-appx:///Assets/StoreLogo.png";

    public static BitmapImage ToBitmap(string? path)
    {
        var candidate = string.IsNullOrWhiteSpace(path) ? DefaultProductImagePath : path;

        if (candidate.StartsWith("ms-appx://", StringComparison.OrdinalIgnoreCase) ||
            candidate.StartsWith("ms-appdata://", StringComparison.OrdinalIgnoreCase) ||
            candidate.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            return new BitmapImage(new Uri(candidate));
        }

        if (File.Exists(candidate))
        {
            return new BitmapImage(new Uri(Path.GetFullPath(candidate)));
        }

        return new BitmapImage(new Uri(DefaultProductImagePath));
    }
}
