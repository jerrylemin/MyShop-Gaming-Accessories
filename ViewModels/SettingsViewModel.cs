using ProjectTest.Helpers;
using ProjectTest.Models;
using ProjectTest.Repositories;
using ProjectTest.Services;
using System.Collections.ObjectModel;

namespace ProjectTest.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;
    private readonly AuthenticationService _authenticationService;
    private readonly LicenseService _licenseService;
    private readonly BackupRestoreService _backupRestoreService;
    private readonly PluginService _pluginService;
    private readonly GraphQlPosService _graphQlPosService;
    private readonly CustomerRepository _customerRepository;
    private int _selectedItemsPerPage;
    private string _lastOpenedScreen = string.Empty;
    private string _statusMessage = string.Empty;
    private string _credentialStatus = "Checking...";
    private string _licenseStatus = string.Empty;
    private string _activationCode = string.Empty;
    private string _backupPath = string.Empty;
    private string _restorePath = string.Empty;
    private string _llmApiKey = string.Empty;
    private string _llmEndpoint = string.Empty;
    private string _graphQlQuery = string.Empty;
    private string _graphQlResult = string.Empty;

    public SettingsViewModel(
        SettingsService settingsService,
        AuthenticationService authenticationService,
        LicenseService? licenseService = null,
        BackupRestoreService? backupRestoreService = null,
        PluginService? pluginService = null,
        GraphQlPosService? graphQlPosService = null,
        CustomerRepository? customerRepository = null)
    {
        _settingsService = settingsService;
        _authenticationService = authenticationService;
        _licenseService = licenseService ?? App.Current.Services.LicenseService;
        _backupRestoreService = backupRestoreService ?? App.Current.Services.BackupRestoreService;
        _pluginService = pluginService ?? App.Current.Services.PluginService;
        _graphQlPosService = graphQlPosService ?? App.Current.Services.GraphQlPosService;
        _customerRepository = customerRepository ?? App.Current.Services.CustomerRepository;
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        ClearCredentialsCommand = new AsyncRelayCommand(ClearCredentialsAsync);
        ActivateCommand = new AsyncRelayCommand(ActivateAsync);
        BackupCommand = new AsyncRelayCommand(BackupAsync);
        RestoreCommand = new AsyncRelayCommand(RestoreAsync);
        ExecuteGraphQlCommand = new AsyncRelayCommand(ExecuteGraphQlAsync);
        LoadSampleGraphQlCommand = new RelayCommand(LoadSampleGraphQl);

        PageSizeOptions = new ObservableCollection<int>([5, 10, 15, 20]);
        Plugins = new ObservableCollection<PluginInfo>();
        Customers = new ObservableCollection<Customer>();
    }

    public ObservableCollection<int> PageSizeOptions { get; }

    public ObservableCollection<PluginInfo> Plugins { get; }

    public ObservableCollection<Customer> Customers { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public AsyncRelayCommand ClearCredentialsCommand { get; }

    public AsyncRelayCommand ActivateCommand { get; }

    public AsyncRelayCommand BackupCommand { get; }

    public AsyncRelayCommand RestoreCommand { get; }

    public AsyncRelayCommand ExecuteGraphQlCommand { get; }

    public RelayCommand LoadSampleGraphQlCommand { get; }

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

    public string GraphQlQuery
    {
        get => _graphQlQuery;
        set => SetProperty(ref _graphQlQuery, value);
    }

    public string GraphQlResult
    {
        get => _graphQlResult;
        set => SetProperty(ref _graphQlResult, value);
    }

    public async Task LoadAsync()
    {
        var settings = _settingsService.CurrentSettings;
        SelectedItemsPerPage = PageSizeOptions.Contains(settings.ItemsPerPage) ? settings.ItemsPerPage : 10;
        LastOpenedScreen = settings.LastOpenedScreen;
        LlmApiKey = settings.LlmApiKey;
        LlmEndpoint = settings.LlmEndpoint;
        if (string.IsNullOrWhiteSpace(GraphQlQuery))
        {
            LoadSampleGraphQl();
        }

        BackupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"myshop-backup-{DateTime.Today:yyyyMMdd}.dump");
        CredentialStatus = await _authenticationService.HasSavedCredentialsAsync()
            ? "Saved credentials are present for auto login."
            : "No saved credentials found.";
        var license = await _licenseService.GetStateAsync();
        LicenseStatus = license.IsActivated
            ? "Activated"
            : license.IsTrialExpired ? "Trial expired. Activation is required." : $"Trial active. {license.TrialDaysRemaining} day(s) remaining.";
        Plugins.Clear();
        foreach (var plugin in _pluginService.Plugins)
        {
            Plugins.Add(plugin);
        }

        Customers.Clear();
        foreach (var customer in await _customerRepository.GetAllAsync())
        {
            Customers.Add(customer);
        }
    }

    private async Task SaveAsync()
    {
        var settings = new AppSettings
        {
            ItemsPerPage = SelectedItemsPerPage,
            LastOpenedScreen = string.IsNullOrWhiteSpace(LastOpenedScreen) ? "Dashboard" : LastOpenedScreen,
            LlmApiKey = LlmApiKey.Trim(),
            LlmEndpoint = LlmEndpoint.Trim()
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

    private async Task ActivateAsync()
    {
        var result = await _licenseService.ActivateAsync(ActivationCode);
        StatusMessage = result.Message;
        await LoadAsync();
    }

    private async Task BackupAsync()
    {
        var result = await _backupRestoreService.BackupAsync(BackupPath);
        StatusMessage = result.Message;
        if (result.Success)
        {
            BackupPath = result.Value ?? BackupPath;
        }
    }

    private async Task RestoreAsync()
    {
        var result = await _backupRestoreService.RestoreAsync(RestorePath);
        StatusMessage = result.Message;
    }

    private void LoadSampleGraphQl()
    {
        GraphQlQuery = _graphQlPosService.GetSampleQuery();
        GraphQlResult = "Sample query loaded. Click Execute GraphQL to run it.";
    }

    private async Task ExecuteGraphQlAsync()
    {
        GraphQlResult = "Running...";
        StatusMessage = "Running GraphQL query...";

        try
        {
            GraphQlResult = await _graphQlPosService.ExecuteAsync(GraphQlQuery);
            StatusMessage = "GraphQL query executed.";
        }
        catch (Exception ex)
        {
            GraphQlResult = System.Text.Json.JsonSerializer.Serialize(
                new { errors = new[] { new { message = ex.Message } } },
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            StatusMessage = $"GraphQL query failed: {ex.Message}";
        }
    }
}
