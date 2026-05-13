using ProjectTest.Helpers;
using ProjectTest.Models;
using ProjectTest.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ProjectTest.ViewModels;

public class PluginsViewModel : ViewModelBase
{
    private readonly PluginService _pluginService;
    private string _statusMessage = string.Empty;
    private string _architectureSummary = string.Empty;

    public PluginsViewModel(PluginService pluginService)
    {
        _pluginService = pluginService;
        Plugins = [];
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        CreateSamplesCommand = new AsyncRelayCommand(CreateSamplesAsync);
        OpenFolderCommand = new RelayCommand(OpenPluginsFolder);
        ArchitectureSummary = _pluginService.GetPluginArchitectureSummary();
    }

    public ObservableCollection<PluginInfo> Plugins { get; }

    public AsyncRelayCommand RefreshCommand { get; }

    public AsyncRelayCommand CreateSamplesCommand { get; }

    public RelayCommand OpenFolderCommand { get; }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string ArchitectureSummary
    {
        get => _architectureSummary;
        set => SetProperty(ref _architectureSummary, value);
    }

    public async Task LoadAsync()
    {
        try
        {
            StatusMessage = "Loading plugins...";
            await _pluginService.LoadAsync(App.Current.Services);

            Plugins.Clear();
            foreach (var plugin in _pluginService.Plugins)
            {
                Plugins.Add(plugin);
            }

            StatusMessage = Plugins.Count == 0 ? "No plugins loaded." : $"Loaded {Plugins.Count} plugin(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Plugins could not be loaded: {ex.GetBaseException().Message}";
        }
    }

    private async Task CreateSamplesAsync()
    {
        try
        {
            var count = await _pluginService.CreateSamplePluginPackAsync();
            StatusMessage = $"Created or refreshed {count} sample plugin manifest(s).";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Sample plugins could not be created: {ex.GetBaseException().Message}";
        }
    }

    private void OpenPluginsFolder()
    {
        Directory.CreateDirectory(_pluginService.PluginsDirectory);
        Process.Start(new ProcessStartInfo
        {
            FileName = _pluginService.PluginsDirectory,
            UseShellExecute = true
        });
    }
}
