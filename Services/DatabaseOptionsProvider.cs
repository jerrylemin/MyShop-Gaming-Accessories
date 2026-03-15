using Windows.Storage;

namespace ProjectTest.Services;

public static class DatabaseOptionsProvider
{
    private const string ConnectionStringKey = "DatabaseConnectionString";
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=myshop_gaming_accessories;Username=postgres;Password=jelly;Include Error Detail=true";
    private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

    public static DatabaseOptions GetDefault()
    {
        var connectionString = Environment.GetEnvironmentVariable("MYSHOP_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString) &&
            LocalSettings.Values.TryGetValue(ConnectionStringKey, out var savedValue) &&
            savedValue is string savedConnectionString &&
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
        LocalSettings.Values[ConnectionStringKey] = connectionString.Trim();
    }
}
