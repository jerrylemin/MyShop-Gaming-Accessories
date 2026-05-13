using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ProjectTest.DataAccess;

public class MyShopDbContextFactory
{
    private readonly string _connectionString;

    public MyShopDbContextFactory(string connectionString)
    {
        _connectionString = BuildRuntimeConnectionString(connectionString);
    }

    public MyShopDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<MyShopDbContext>();
        optionsBuilder.UseNpgsql(_connectionString);
        return new MyShopDbContext(optionsBuilder.Options);
    }

    private static string BuildRuntimeConnectionString(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            KeepAlive = 30,
            Timeout = 15,
            CommandTimeout = 30
        };

        return builder.ConnectionString;
    }
}
