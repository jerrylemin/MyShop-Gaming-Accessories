using Npgsql;
using ProjectTest.DataAccess;
using ProjectTest.Services;
using System.Text.Json;

var options = BootstrapOptions.Parse(args);
Directory.CreateDirectory(Path.GetDirectoryName(options.LogPath)!);

try
{
    Log(options.LogPath, "Starting database bootstrap.");

    var appConnectionString = options.ConnectionString;
    if (string.IsNullOrWhiteSpace(appConnectionString))
    {
        await EnsureDatabaseAsync(options);
        appConnectionString = BuildConnectionString(
            options.Host,
            options.Port,
            options.Database,
            options.AppUser,
            options.AppPassword,
            options.IncludeErrorDetail);
    }
    else
    {
        Log(options.LogPath, "Using provided app connection string.");
    }

    var initializer = new DatabaseInitializer(new MyShopDbContextFactory(appConnectionString));
    await initializer.InitializeAsync();
    Log(options.LogPath, "Schema migration and seed completed.");

    Directory.CreateDirectory(options.AppDirectory);
    var configPath = Path.Combine(options.AppDirectory, "myshop.database.json");
    var configJson = JsonSerializer.Serialize(new { ConnectionString = appConnectionString }, new JsonSerializerOptions { WriteIndented = true });
    await File.WriteAllTextAsync(configPath, configJson);
    Log(options.LogPath, $"Wrote app database config: {configPath}");

    return 0;
}
catch (Exception ex)
{
    Log(options.LogPath, ex.ToString());
    Console.Error.WriteLine(ex.Message);
    return 1;
}

static async Task EnsureDatabaseAsync(BootstrapOptions options)
{
    await using var adminConnection = new NpgsqlConnection(BuildConnectionString(
        options.Host,
        options.Port,
        "postgres",
        options.AdminUser,
        options.AdminPassword,
        options.IncludeErrorDetail));
    await adminConnection.OpenAsync();

    if (!await RoleExistsAsync(adminConnection, options.AppUser))
    {
        await using var createRoleCommand = new NpgsqlCommand($"""
            CREATE ROLE {QuoteIdentifier(options.AppUser)}
            WITH LOGIN PASSWORD @password;
            """, adminConnection);
        createRoleCommand.Parameters.AddWithValue("password", options.AppPassword);
        await createRoleCommand.ExecuteNonQueryAsync();
    }
    else
    {
        await using var alterRoleCommand = new NpgsqlCommand($"""
            ALTER ROLE {QuoteIdentifier(options.AppUser)}
            WITH LOGIN PASSWORD @password;
            """, adminConnection);
        alterRoleCommand.Parameters.AddWithValue("password", options.AppPassword);
        await alterRoleCommand.ExecuteNonQueryAsync();
    }

    if (!await DatabaseExistsAsync(adminConnection, options.Database))
    {
        await using var createDatabaseCommand = new NpgsqlCommand($"""
            CREATE DATABASE {QuoteIdentifier(options.Database)}
            OWNER {QuoteIdentifier(options.AppUser)}
            ENCODING 'UTF8';
            """, adminConnection);
        await createDatabaseCommand.ExecuteNonQueryAsync();
    }

    await using var grantCommand = new NpgsqlCommand($"""
        GRANT ALL PRIVILEGES ON DATABASE {QuoteIdentifier(options.Database)} TO {QuoteIdentifier(options.AppUser)};
        """, adminConnection);
    await grantCommand.ExecuteNonQueryAsync();
}

static async Task<bool> RoleExistsAsync(NpgsqlConnection connection, string roleName)
{
    await using var command = new NpgsqlCommand("SELECT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = @name);", connection);
    command.Parameters.AddWithValue("name", roleName);
    return await command.ExecuteScalarAsync() is true;
}

static async Task<bool> DatabaseExistsAsync(NpgsqlConnection connection, string databaseName)
{
    await using var command = new NpgsqlCommand("SELECT EXISTS (SELECT 1 FROM pg_database WHERE datname = @name);", connection);
    command.Parameters.AddWithValue("name", databaseName);
    return await command.ExecuteScalarAsync() is true;
}

static string BuildConnectionString(string host, int port, string database, string username, string password, bool includeErrorDetail)
{
    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = host,
        Port = port,
        Database = database,
        Username = username,
        Password = password,
        IncludeErrorDetail = includeErrorDetail,
        Pooling = true
    };
    return builder.ConnectionString;
}

static string QuoteIdentifier(string value)
{
    return "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}

static void Log(string path, string message)
{
    File.AppendAllText(path, $"[{DateTimeOffset.Now:O}] {message}{Environment.NewLine}");
}

internal sealed class BootstrapOptions
{
    public string Host { get; private init; } = "localhost";
    public int Port { get; private init; } = 5432;
    public string AdminUser { get; private init; } = "postgres";
    public string AdminPassword { get; private init; } = "MyShopAdmin#2026";
    public string AppUser { get; private init; } = "myshop_app";
    public string AppPassword { get; private init; } = "MyShopApp#2026";
    public string Database { get; private init; } = "myshop_gaming_accessories";
    public string ConnectionString { get; private init; } = string.Empty;
    public string AppDirectory { get; private init; } = AppContext.BaseDirectory;
    public string LogPath { get; private init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "MyShop POS",
        "Logs",
        "database-bootstrap.log");
    public bool IncludeErrorDetail { get; private init; } = true;

    public static BootstrapOptions Parse(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Length; index++)
        {
            if (!args[index].StartsWith("--", StringComparison.Ordinal) || index + 1 >= args.Length)
            {
                continue;
            }

            values[args[index][2..]] = args[++index];
        }

        return new BootstrapOptions
        {
            Host = Get(values, "host", "localhost"),
            Port = int.TryParse(Get(values, "port", "5432"), out var port) ? port : 5432,
            AdminUser = Get(values, "admin-user", "postgres"),
            AdminPassword = Get(values, "admin-password", "MyShopAdmin#2026"),
            AppUser = Get(values, "app-user", "myshop_app"),
            AppPassword = Get(values, "app-password", "MyShopApp#2026"),
            Database = Get(values, "database", "myshop_gaming_accessories"),
            ConnectionString = Get(values, "connection-string", string.Empty),
            AppDirectory = Get(values, "app-dir", AppContext.BaseDirectory),
            LogPath = Get(
                values,
                "log",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MyShop POS", "Logs", "database-bootstrap.log"))
        };
    }

    private static string Get(IReadOnlyDictionary<string, string> values, string key, string fallback)
    {
        return values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }
}
