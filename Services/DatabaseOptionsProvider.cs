namespace ProjectTest.Services;

public static class DatabaseOptionsProvider
{
    private const string ConnectionStringKey = "DatabaseConnectionString";
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=myshop_gaming_accessories;Username=postgres;Password=jelly;Include Error Detail=true";

    public static DatabaseOptions GetDefault()
    {
        var connectionString = Environment.GetEnvironmentVariable("MYSHOP_CONNECTION_STRING");

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
}
