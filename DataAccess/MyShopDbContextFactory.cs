using Microsoft.EntityFrameworkCore;

namespace ProjectTest.DataAccess;

public class MyShopDbContextFactory
{
    private readonly string _connectionString;

    public MyShopDbContextFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public MyShopDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<MyShopDbContext>();
        optionsBuilder.UseNpgsql(_connectionString);
        return new MyShopDbContext(optionsBuilder.Options);
    }
}
