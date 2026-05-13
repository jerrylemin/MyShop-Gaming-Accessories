using ProjectTest.Helpers;
using ProjectTest.Models;
using ProjectTest.Services;
using System.Collections.ObjectModel;

namespace ProjectTest.ViewModels;

public class PluginsViewModel : ViewModelBase
{
    private readonly PluginService _pluginService;
    private string _statusMessage = string.Empty;

    public PluginsViewModel(PluginService pluginService)
    {
        _pluginService = pluginService;
        Plugins = [];
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
    }

    public ObservableCollection<PluginInfo> Plugins { get; }

    public AsyncRelayCommand RefreshCommand { get; }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
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
            StatusMessage = $"Plugins could not be loaded: {ex.Message}";
        }
    }
}
