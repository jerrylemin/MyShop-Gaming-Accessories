namespace ProjectTest.Services;

public static class DatabaseOptionsProvider
{
    private const string ConnectionStringKey = "DatabaseConnectionString";
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=myshop_gaming_accessories;Username=postgres;Password=jelly;Include Error Detail=true";
    private const string InstallerDatabaseConfigFileName = "myshop.database.json";

    public static DatabaseOptions GetDefault()
    {
        var connectionString = Environment.GetEnvironmentVariable("MYSHOP_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString) &&
            TryReadInstallerConnectionString(out var installerConnectionString))
        {
            connectionString = installerConnectionString;
        }

        if (string.IsNullOrWhiteSpace(connectionString) &&
            AppLocalStorage.TryGetString(ConnectionStringKey, out var savedConnectionString) &&
            !string.IsNullOrWhiteSpace(savedConnectionString))
        {
            connectionString = savedConnectionString;
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = DefaultConnectionString;
        }

        return new DatabaseOptions
        {
            ConnectionString = connectionString
        };
    }

    public static string GetConfiguredConnectionString()
    {
        return GetDefault().ConnectionString;
    }

    public static string GetTemplateConnectionString()
    {
        return DefaultConnectionString;
    }

    public static void SaveConnectionString(string connectionString)
    {
        AppLocalStorage.SetString(ConnectionStringKey, connectionString.Trim());
    }

    private static bool TryReadInstallerConnectionString(out string? connectionString)
    {
        connectionString = null;
        var configPath = Path.Combine(AppContext.BaseDirectory, InstallerDatabaseConfigFileName);
        if (!File.Exists(configPath))
        {
            return false;
        }

        try
        {
            var json = File.ReadAllText(configPath);
            var config = System.Text.Json.JsonSerializer.Deserialize<InstallerDatabaseConfig>(json, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            connectionString = config?.ConnectionString;
            return !string.IsNullOrWhiteSpace(connectionString);
        }
        catch
        {
            connectionString = null;
            return false;
        }
    }

    private sealed class InstallerDatabaseConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
    }
}
