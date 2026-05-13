using ProjectTest.Helpers;
using ProjectTest.Services;

namespace ProjectTest.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly AuthenticationService _authenticationService;
    private readonly AsyncRelayCommand _loginCommand;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public LoginViewModel(AuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        VersionText = AppInfoHelper.GetDisplayVersion();
        DefaultCredentialsHint = $"Default login: {_authenticationService.DefaultUsername} / {_authenticationService.DefaultPassword}";
        _loginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
    }

    public event EventHandler? LoginSucceeded;

    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                _loginCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                _loginCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string VersionText { get; }

    public string DefaultCredentialsHint { get; }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                _loginCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public AsyncRelayCommand LoginCommand => _loginCommand;

    private bool CanLogin()
    {
        return !IsBusy &&
               !string.IsNullOrWhiteSpace(Username) &&
               !string.IsNullOrWhiteSpace(Password);
    }

    private async Task LoginAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var isValid = await _authenticationService.ValidateAsync(Username.Trim(), Password);
            if (!isValid)
            {
                ErrorMessage = "Invalid username or password.";
                return;
            }

            await _authenticationService.SaveCredentialsAsync(Username.Trim(), Password);
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
