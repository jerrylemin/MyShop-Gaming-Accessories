using ProjectTest.Models;
using System.Text.Json;
using Windows.Storage;

namespace ProjectTest.Services;

public class SettingsService
{
    private const string SettingsKey = "AppSettings";
    private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

    public AppSettings CurrentSettings { get; private set; } = new();

    public event EventHandler<AppSettings>? SettingsChanged;

    public Task InitializeAsync()
    {
        if (_localSettings.Values.TryGetValue(SettingsKey, out var rawValue) &&
            rawValue is string json &&
            !string.IsNullOrWhiteSpace(json))
        {
            CurrentSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        else
        {
            CurrentSettings = new AppSettings();
            _localSettings.Values[SettingsKey] = JsonSerializer.Serialize(CurrentSettings);
        }

        return Task.CompletedTask;
    }

    public Task SaveAsync(AppSettings settings)
    {
        CurrentSettings = settings;
        _localSettings.Values[SettingsKey] = JsonSerializer.Serialize(CurrentSettings);
        SettingsChanged?.Invoke(this, CurrentSettings);
        return Task.CompletedTask;
    }
}
