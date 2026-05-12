using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using Npgsql;
using ProjectTest.Services;
using ProjectTest.Views;
using System.Text;

namespace ProjectTest;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        WriteStartupMarker("App.ctor");
        InitializeComponent();
        RequestedTheme = ApplicationTheme.Light;
    }

    public static new App Current => (App)Application.Current;

    public AppServices Services { get; private set; } = null!;

    public Window? ActiveWindow => _window;

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        await InitializeAsync();
    }

    public void ShowLoginWindow()
    {
        SwitchWindow(new LoginWindow());
    }

    public void ShowMainWindow()
    {
        try
        {
            SwitchWindow(new MainWindow());
        }
        catch (Exception ex) when (ContainsXamlParseException(ex))
        {
            WriteStartupExceptionLog("MainWindow", ex);
            throw;
        }
    }

    public void ShowDatabaseSetupWindow(string errorMessage)
    {
        SwitchWindow(new DatabaseSetupWindow(errorMessage));
    }

    public async Task<Models.OperationResult> ConfigureDatabaseAsync(string connectionString)
    {
        DatabaseOptionsProvider.SaveConnectionString(connectionString);

        try
        {
            await InitializeAsync();
            return Models.OperationResult.Ok("Database connection saved.");
        }
        catch (Exception ex) when (IsDatabaseStartupException(ex))
        {
            return Models.OperationResult.Fail(BuildDatabaseErrorMessage(ex));
        }
    }

    private void SwitchWindow(Window nextWindow)
    {
        var currentWindow = _window;
        _window = nextWindow;
        nextWindow.Activate();
        currentWindow?.Close();
    }

    private async Task InitializeAsync()
    {
        try
        {
            Services = await AppBootstrapper.BuildAsync();
            await Services.DatabaseInitializer.InitializeAsync();

            if (await Services.AuthenticationService.TryRestoreSavedCredentialsAsync())
            {
                ShowMainWindow();
            }
            else
            {
                ShowLoginWindow();
            }
        }
        catch (Exception ex) when (IsDatabaseStartupException(ex))
        {
            ShowDatabaseSetupWindow(BuildDatabaseErrorMessage(ex));
        }
    }

    private static bool IsDatabaseStartupException(Exception ex)
    {
        return ex is NpgsqlException || ex.InnerException is not null && IsDatabaseStartupException(ex.InnerException);
    }

    private static string BuildDatabaseErrorMessage(Exception ex)
    {
        var baseException = ex.GetBaseException();
        return baseException.Message;
    }

    private static bool ContainsXamlParseException(Exception ex)
    {
        return ex is XamlParseException || ex.InnerException is not null && ContainsXamlParseException(ex.InnerException);
    }

    private static void WriteStartupExceptionLog(string target, Exception ex)
    {
        try
        {
            var builder = new StringBuilder()
                .AppendLine($"Timestamp: {DateTimeOffset.Now:O}")
                .AppendLine($"Target: {target}")
                .AppendLine(ex.ToString())
                .AppendLine();

            foreach (var logPath in GetStartupLogPaths())
            {
                try
                {
                    var directory = Path.GetDirectoryName(logPath);
                    if (!string.IsNullOrWhiteSpace(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.AppendAllText(logPath, builder.ToString());
                }
                catch
                {
                    // Try the next location.
                }
            }
        }
        catch
        {
            // Preserve the original startup exception if logging fails.
        }
    }

    private static IEnumerable<string> GetStartupLogPaths()
    {
        yield return Path.Combine(AppContext.BaseDirectory, "startup-error.log");
        yield return Path.Combine(Path.GetTempPath(), "ProjectTest-startup-error.log");
        yield return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ProjectTest",
            "startup-error.log");
    }

    private static void WriteStartupMarker(string stage)
    {
        try
        {
            var builder = new StringBuilder()
                .AppendLine($"Timestamp: {DateTimeOffset.Now:O}")
                .AppendLine($"Marker: {stage}")
                .AppendLine();

            foreach (var logPath in GetStartupLogPaths())
            {
                try
                {
                    var directory = Path.GetDirectoryName(logPath);
                    if (!string.IsNullOrWhiteSpace(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.AppendAllText(logPath, builder.ToString());
                }
                catch
                {
                    // Continue with next location.
                }
            }
        }
        catch
        {
            // Ignore marker failures.
        }
    }
}
