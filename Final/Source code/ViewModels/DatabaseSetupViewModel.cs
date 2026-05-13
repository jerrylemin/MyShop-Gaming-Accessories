using ProjectTest.Helpers;
using ProjectTest.Models;

namespace ProjectTest.ViewModels;

public class DatabaseSetupViewModel : ViewModelBase
{
    private readonly Func<string, Task<OperationResult>> _saveConnectionAsync;
    private readonly AsyncRelayCommand _saveCommand;
    private string _connectionString;
    private string _errorMessage;
    private string _statusMessage = string.Empty;

    public DatabaseSetupViewModel(
        string connectionString,
        string errorMessage,
        Func<string, Task<OperationResult>> saveConnectionAsync)
    {
        _connectionString = connectionString;
        _errorMessage = errorMessage;
        _saveConnectionAsync = saveConnectionAsync;
        _saveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        TemplateConnectionString = Services.DatabaseOptionsProvider.GetTemplateConnectionString();
    }

    public event EventHandler? ConnectionSucceeded;

    public string ConnectionString
    {
        get => _connectionString;
        set
        {
            if (SetProperty(ref _connectionString, value))
            {
                _saveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string TemplateConnectionString { get; }

    public AsyncRelayCommand SaveCommand => _saveCommand;

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(ConnectionString);
    }

    private async Task SaveAsync()
    {
        var trimmedConnectionString = ConnectionString.Trim();
        if (string.IsNullOrWhiteSpace(trimmedConnectionString))
        {
            StatusMessage = "Enter a PostgreSQL connection string.";
            return;
        }

        var result = await _saveConnectionAsync(trimmedConnectionString);
        StatusMessage = result.Message;

        if (result.Success)
        {
            ConnectionSucceeded?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            ErrorMessage = result.Message;
        }
    }
}
