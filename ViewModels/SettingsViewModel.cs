using ProjectTest.Helpers;
using ProjectTest.Models;
using ProjectTest.Services;
using System.Collections.ObjectModel;

namespace ProjectTest.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;
    private readonly AuthenticationService _authenticationService;
    private int _selectedItemsPerPage;
    private string _lastOpenedScreen = string.Empty;
    private string _statusMessage = string.Empty;
    private string _credentialStatus = "Checking...";

    public SettingsViewModel(SettingsService settingsService, AuthenticationService authenticationService)
    {
        _settingsService = settingsService;
        _authenticationService = authenticationService;
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        ClearCredentialsCommand = new AsyncRelayCommand(ClearCredentialsAsync);

        PageSizeOptions = new ObservableCollection<int>([5, 10, 15, 20]);
    }

    public ObservableCollection<int> PageSizeOptions { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public AsyncRelayCommand ClearCredentialsCommand { get; }

    public int SelectedItemsPerPage
    {
        get => _selectedItemsPerPage;
        set => SetProperty(ref _selectedItemsPerPage, value);
    }

    public string LastOpenedScreen
    {
        get => _lastOpenedScreen;
        set => SetProperty(ref _lastOpenedScreen, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string CredentialStatus
    {
        get => _credentialStatus;
        set => SetProperty(ref _credentialStatus, value);
    }

    public async Task LoadAsync()
    {
        var settings = _settingsService.CurrentSettings;
        SelectedItemsPerPage = PageSizeOptions.Contains(settings.ItemsPerPage) ? settings.ItemsPerPage : 10;
        LastOpenedScreen = settings.LastOpenedScreen;
        CredentialStatus = await _authenticationService.HasSavedCredentialsAsync()
            ? "Saved credentials are present for auto login."
            : "No saved credentials found.";
    }

    private async Task SaveAsync()
    {
        var settings = new AppSettings
        {
            ItemsPerPage = SelectedItemsPerPage,
            LastOpenedScreen = string.IsNullOrWhiteSpace(LastOpenedScreen) ? "Dashboard" : LastOpenedScreen
        };

        await _settingsService.SaveAsync(settings);
        StatusMessage = "Settings saved.";
    }

    private async Task ClearCredentialsAsync()
    {
        await _authenticationService.ClearCredentialsAsync();
        CredentialStatus = "Saved credentials were cleared. The login window will appear on next launch.";
        StatusMessage = "Credentials cleared.";
    }
}
