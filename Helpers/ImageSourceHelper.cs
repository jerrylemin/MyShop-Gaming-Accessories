using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;

namespace ProjectTest.Helpers;

public static class ImageSourceHelper
{
    private const string PlaceholderUri = "ms-appx:///Assets/StoreLogo.png";

    public static BitmapImage ToBitmap(string? path)
    {
        var candidate = string.IsNullOrWhiteSpace(path) ? PlaceholderUri : path;

        if (!candidate.StartsWith("ms-appx://", StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith("ms-appdata://", StringComparison.OrdinalIgnoreCase) &&
            !File.Exists(candidate))
        {
            candidate = PlaceholderUri;
        }

        return new BitmapImage(new Uri(candidate));
    }
}
