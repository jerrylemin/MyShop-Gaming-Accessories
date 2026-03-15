using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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
        optionsBuilder.UseNpgsql(connectionString);
        return new MyShopDbContext(optionsBuilder.Options);
    }
}
