using Microsoft.EntityFrameworkCore;
using ProjectTest.DataAccess;
using ProjectTest.Services;

const string fallbackConnectionString =
    "Host=localhost;Port=5432;Database=myshop_gaming_accessories;Username=postgres;Password=jelly;Include Error Detail=true";

var connectionString = args.Length > 0
    ? args[0]
    : Environment.GetEnvironmentVariable("MYSHOP_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = fallbackConnectionString;
}

var dbContextFactory = new MyShopDbContextFactory(connectionString);
var initializer = new DatabaseInitializer(dbContextFactory);

await initializer.InitializeAsync();

await using var dbContext = dbContextFactory.CreateDbContext();

var categoryCount = await dbContext.Categories.CountAsync();
var productCount = await dbContext.Products.CountAsync();
var orderCount = await dbContext.Orders.CountAsync();
var orderItemCount = await dbContext.OrderItems.CountAsync();

Console.WriteLine($"ConnectionString={connectionString}");
Console.WriteLine($"Categories={categoryCount}");
Console.WriteLine($"Products={productCount}");
Console.WriteLine($"Orders={orderCount}");
Console.WriteLine($"OrderItems={orderItemCount}");
