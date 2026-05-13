using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace ProjectTest.DataAccess;

public class DesignTimeMyShopDbContextFactory : IDesignTimeDbContextFactory<MyShopDbContext>
{
    private const string FallbackConnectionString =
        "Host=localhost;Port=5432;Database=myshop_gaming_accessories;Username=postgres;Password=jelly;Include Error Detail=true";

    public MyShopDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("MYSHOP_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = FallbackConnectionString;
        }

        var optionsBuilder = new DbContextOptionsBuilder<MyShopDbContext>();
        optionsBuilder.UseNpgsql(BuildRuntimeConnectionString(connectionString));
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
