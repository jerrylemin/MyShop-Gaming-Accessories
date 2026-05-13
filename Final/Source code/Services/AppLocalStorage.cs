using System.Runtime.InteropServices;
using System.Text.Json;
using Windows.Storage;

namespace ProjectTest.Services;

internal static class AppLocalStorage
{
    private static readonly object StorageLock = new();
    private static readonly string StorageDirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ProjectTest");
    private static readonly string StorageFilePath = Path.Combine(StorageDirectoryPath, "localsettings.json");

    public static bool TryGetString(string key, out string? value)
    {
        if (IsRunningPackaged())
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var rawValue) &&
                rawValue is string settingValue)
            {
                value = settingValue;
                return true;
            }

            value = null;
            return false;
        }

        lock (StorageLock)
        {
            var settings = LoadUnpackagedSettings();
            return settings.TryGetValue(key, out value);
        }
    }

    public static void SetString(string key, string value)
    {
        if (IsRunningPackaged())
        {
            ApplicationData.Current.LocalSettings.Values[key] = value;
            return;
        }

        lock (StorageLock)
        {
            var settings = LoadUnpackagedSettings();
            settings[key] = value;
            SaveUnpackagedSettings(settings);
        }
    }

    public static bool ContainsKey(string key)
    {
        if (IsRunningPackaged())
        {
            return ApplicationData.Current.LocalSettings.Values.ContainsKey(key);
        }

        lock (StorageLock)
        {
            return LoadUnpackagedSettings().ContainsKey(key);
        }
    }

    public static void Remove(string key)
    {
        if (IsRunningPackaged())
        {
            ApplicationData.Current.LocalSettings.Values.Remove(key);
            return;
        }

        lock (StorageLock)
        {
            var settings = LoadUnpackagedSettings();
            if (settings.Remove(key))
            {
                SaveUnpackagedSettings(settings);
            }
        }
    }

    private static Dictionary<string, string> LoadUnpackagedSettings()
    {
        if (!File.Exists(StorageFilePath))
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        try
        {
            var json = File.ReadAllText(StorageFilePath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ??
                   new Dictionary<string, string>(StringComparer.Ordinal);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }

    private static void SaveUnpackagedSettings(Dictionary<string, string> settings)
    {
        Directory.CreateDirectory(StorageDirectoryPath);
        var json = JsonSerializer.Serialize(settings);
        File.WriteAllText(StorageFilePath, json);
    }

    private static bool IsRunningPackaged()
    {
        var length = 0;
        var result = GetCurrentPackageFullName(ref length, null);
        return result != AppModelErrorNoPackage;
    }

    private const int AppModelErrorNoPackage = 15700;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, string? packageFullName);
}
