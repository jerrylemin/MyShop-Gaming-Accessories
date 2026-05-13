using ProjectTest.Models;
using System.Text.Json;

namespace ProjectTest.Services;

public class SettingsService
{
    private const string SettingsKey = "AppSettings";

    public AppSettings CurrentSettings { get; private set; } = new();

    public event EventHandler<AppSettings>? SettingsChanged;

    public Task InitializeAsync()
    {
        if (AppLocalStorage.TryGetString(SettingsKey, out var json) &&
            !string.IsNullOrWhiteSpace(json))
        {
            CurrentSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        else
        {
            CurrentSettings = new AppSettings();
            AppLocalStorage.SetString(SettingsKey, JsonSerializer.Serialize(CurrentSettings));
        }

        return Task.CompletedTask;
    }

    public Task SaveAsync(AppSettings settings)
    {
        CurrentSettings = settings;
        AppLocalStorage.SetString(SettingsKey, JsonSerializer.Serialize(CurrentSettings));
        SettingsChanged?.Invoke(this, CurrentSettings);
        return Task.CompletedTask;
    }
}
