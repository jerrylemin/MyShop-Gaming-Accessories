using System.Reflection;
using Windows.ApplicationModel;

namespace ProjectTest.Helpers;

public static class AppInfoHelper
{
    public static string GetDisplayVersion()
    {
        try
        {
            var version = Package.Current.Id.Version;
            return $"v{version.Major}.{version.Minor}.{version.Build}";
        }
        catch
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            return assemblyVersion is null
                ? "v1.0.0"
                : $"v{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
        }
    }
}
