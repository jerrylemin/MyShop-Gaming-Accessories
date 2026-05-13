using ProjectTest.Helpers;
using ProjectTest.Models;
using ProjectTest.Services;
using System.Collections.ObjectModel;

namespace ProjectTest.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;
    private readonly AuthenticationService _authenticationService;
    private readonly LicenseService _licenseService;
    private readonly BackupRestoreService _backupRestoreService;
    private readonly LlmAssistantService _llmAssistantService;
    private int _selectedItemsPerPage;
    private string _lastOpenedScreen = string.Empty;
    private string _statusMessage = string.Empty;
    private string _credentialStatus = "Checking...";
    private string _licenseStatus = string.Empty;
    private string _activationCode = string.Empty;
    private string _backupPath = string.Empty;
    private string _restorePath = string.Empty;
    private string _postgresSqlToolsPath = string.Empty;
    private string _postgresSqlToolsStatus = string.Empty;
    private string _llmApiKey = string.Empty;
    private string _llmEndpoint = string.Empty;

    public SettingsViewModel(
        SettingsService settingsService,
        AuthenticationService authenticationService,
        LicenseService? licenseService = null,
        BackupRestoreService? backupRestoreService = null,
        LlmAssistantService? llmAssistantService = null)
    {
        _settingsService = settingsService;
        _authenticationService = authenticationService;
        _licenseService = licenseService ?? App.Current.Services.LicenseService;
        _backupRestoreService = backupRestoreService ?? App.Current.Services.BackupRestoreService;
        _llmAssistantService = llmAssistantService ?? App.Current.Services.LlmAssistantService;
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        ClearCredentialsCommand = new AsyncRelayCommand(ClearCredentialsAsync);
        ActivateCommand = new AsyncRelayCommand(ActivateAsync);
        BackupCommand = new AsyncRelayCommand(BackupAsync);
        RestoreCommand = new AsyncRelayCommand(RestoreAsync);
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        TestAssistantCommand = new AsyncRelayCommand(TestAssistantAsync);

        PageSizeOptions = new ObservableCollection<int>([5, 10, 15, 20]);
    }

    public ObservableCollection<int> PageSizeOptions { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public AsyncRelayCommand ClearCredentialsCommand { get; }

    public AsyncRelayCommand ActivateCommand { get; }

    public AsyncRelayCommand BackupCommand { get; }

    public AsyncRelayCommand RestoreCommand { get; }

    public AsyncRelayCommand RefreshCommand { get; }

    public AsyncRelayCommand TestAssistantCommand { get; }

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

    public string LicenseStatus
    {
        get => _licenseStatus;
        set => SetProperty(ref _licenseStatus, value);
    }

    public string ActivationCode
    {
        get => _activationCode;
        set => SetProperty(ref _activationCode, value);
    }

    public string BackupPath
    {
        get => _backupPath;
        set => SetProperty(ref _backupPath, value);
    }

    public string RestorePath
    {
        get => _restorePath;
        set => SetProperty(ref _restorePath, value);
    }

    public string PostgreSqlToolsPath
    {
        get => _postgresSqlToolsPath;
        set
        {
            if (SetProperty(ref _postgresSqlToolsPath, value))
            {
                _backupRestoreService.PostgreSqlToolsDirectory = value;
                PostgreSqlToolsStatus = BackupRestoreService.GetToolStatus(value);
            }
        }
    }

    public string PostgreSqlToolsStatus
    {
        get => _postgresSqlToolsStatus;
        set => SetProperty(ref _postgresSqlToolsStatus, value);
    }

    public string LlmApiKey
    {
        get => _llmApiKey;
        set => SetProperty(ref _llmApiKey, value);
    }

    public string LlmEndpoint
    {
        get => _llmEndpoint;
        set => SetProperty(ref _llmEndpoint, value);
    }

    public async Task LoadAsync()
    {
        var settings = _settingsService.CurrentSettings;
        SelectedItemsPerPage = PageSizeOptions.Contains(settings.ItemsPerPage) ? settings.ItemsPerPage : 10;
        LastOpenedScreen = settings.LastOpenedScreen;
        LlmApiKey = settings.LlmApiKey;
        LlmEndpoint = settings.LlmEndpoint;
        PostgreSqlToolsPath = settings.PostgreSqlToolsPath;
        PostgreSqlToolsStatus = BackupRestoreService.GetToolStatus(PostgreSqlToolsPath);

        BackupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"myshop-backup-{DateTime.Today:yyyyMMdd}.dump");
        CredentialStatus = await _authenticationService.HasSavedCredentialsAsync()
            ? "Saved credentials are present for auto login."
            : "No saved credentials found.";
        var license = await _licenseService.GetStateAsync();
        LicenseStatus = license.IsActivated
            ? "Activated"
            : license.IsTrialExpired ? "Trial expired. Activation is required." : $"Trial active. {license.TrialDaysRemaining} day(s) remaining.";
    }

    private async Task SaveAsync()
    {
        var settings = new AppSettings
        {
            ItemsPerPage = SelectedItemsPerPage,
            LastOpenedScreen = string.IsNullOrWhiteSpace(LastOpenedScreen) ? "Dashboard" : LastOpenedScreen,
            LlmApiKey = LlmApiKey.Trim(),
            LlmEndpoint = LlmEndpoint.Trim(),
            PostgreSqlToolsPath = PostgreSqlToolsPath.Trim()
        };

        await _settingsService.SaveAsync(settings);
        _backupRestoreService.PostgreSqlToolsDirectory = settings.PostgreSqlToolsPath;
        StatusMessage = "Settings saved.";
    }

    private async Task ClearCredentialsAsync()
    {
        await _authenticationService.ClearCredentialsAsync();
        CredentialStatus = "Saved credentials were cleared. The login window will appear on next launch.";
        StatusMessage = "Credentials cleared.";
    }

    private async Task ActivateAsync()
    {
        var result = await _licenseService.ActivateAsync(ActivationCode);
        StatusMessage = result.Message;
        await LoadAsync();
    }

    private async Task BackupAsync()
    {
        await SaveAsync();
        var result = await _backupRestoreService.BackupAsync(BackupPath);
        StatusMessage = result.Message;
        if (result.Success)
        {
            BackupPath = result.Value ?? BackupPath;
        }
    }

    private async Task RestoreAsync()
    {
        await SaveAsync();
        var result = await _backupRestoreService.RestoreAsync(RestorePath);
        StatusMessage = result.Message;
    }

    private async Task TestAssistantAsync()
    {
        await SaveAsync();
        StatusMessage = "Testing Assistant connection...";
        var result = await _llmAssistantService.TestConnectionAsync();
        StatusMessage = result.IsConfigured
            ? $"Assistant test finished: {result.Summary}"
            : result.Summary;
    }
}
