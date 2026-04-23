using Microsoft.EntityFrameworkCore;
using ProjectTest.DataAccess;
using ProjectTest.DataAccess.Seeding;
using Npgsql;

namespace ProjectTest.Services;

public class DatabaseInitializer
{
    private readonly MyShopDbContextFactory _dbContextFactory;

    public DatabaseInitializer(MyShopDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task InitializeAsync()
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await EnsureSchemaAsync(dbContext);

        if (await dbContext.Categories.AnyAsync())
        {
            return;
        }

        var categories = GamingAccessorySeedGenerator.BuildCategories();
        dbContext.Categories.AddRange(categories);
        await dbContext.SaveChangesAsync();

        var persistedCategories = await dbContext.Categories.OrderBy(x => x.Id).ToListAsync();
        var products = GamingAccessorySeedGenerator.BuildProducts(persistedCategories);
        dbContext.Products.AddRange(products);
        await dbContext.SaveChangesAsync();

        foreach (var product in products)
        {
            product.Image1 = GamingAccessorySeedGenerator.BuildImagePath(product.Id, 1);
            product.Image2 = GamingAccessorySeedGenerator.BuildImagePath(product.Id, 2);
            product.Image3 = GamingAccessorySeedGenerator.BuildImagePath(product.Id, 3);
        }

        await dbContext.SaveChangesAsync();

        var orders = GamingAccessorySeedGenerator.BuildOrders(products);
        dbContext.Orders.AddRange(orders);
        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureSchemaAsync(MyShopDbContext dbContext)
    {
        if (await HasLegacySchemaAsync(dbContext))
        {
            await TryUpgradeLegacySchemaAsync(dbContext);
        }

        try
        {
            await dbContext.Database.MigrateAsync();
        }
        catch (Exception ex) when (ex is PostgresException or InvalidOperationException)
        {
            if (await TryUpgradeLegacySchemaAsync(dbContext))
            {
                await dbContext.Database.MigrateAsync();
                return;
            }

            throw;
        }
    }

    private static async Task<bool> HasLegacySchemaAsync(MyShopDbContext dbContext)
    {
        if (!await dbContext.Database.CanConnectAsync())
        {
            return false;
        }

        await using var command = dbContext.Database.GetDbConnection().CreateCommand();
        if (command.Connection?.State != System.Data.ConnectionState.Open)
        {
            await dbContext.Database.OpenConnectionAsync();
        }

        command.CommandText = """
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.columns
                WHERE
                    (table_name = 'categories' AND column_name IN ('Id', 'Name', 'Description')) OR
                    (table_name = 'products' AND column_name IN ('Id', 'SKU', 'Name', 'Manufacturer', 'CPU', 'RAM', 'Storage', 'GPU', 'Screen', 'ImportPrice', 'SalePrice', 'Stock', 'CategoryId', 'Description', 'Image1', 'Image2', 'Image3')) OR
                    (table_name = 'orders' AND column_name IN ('Id', 'CreatedTime', 'FinalPrice', 'Status')) OR
                    (table_name = 'order_items' AND column_name IN ('Id', 'OrderId', 'ProductId', 'Quantity', 'UnitSalePrice', 'TotalPrice'))
            );
            """;

        var result = await command.ExecuteScalarAsync();
        await dbContext.Database.CloseConnectionAsync();
        return result is true || result is bool boolResult && boolResult;
    }

    private static async Task<bool> TryUpgradeLegacySchemaAsync(MyShopDbContext dbContext)
    {
        if (!await dbContext.Database.CanConnectAsync())
        {
            return false;
        }

        try
        {
            if (!await HasLegacySchemaAsync(dbContext))
            {
                return false;
            }

            // Earlier builds used EnsureCreated with PascalCase column names. Normalize them in place
            // and mark the baseline migration as applied so future startups can use MigrateAsync().
            var legacyUpgradeScript = """
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'categories' AND column_name = 'Id') THEN
                        ALTER TABLE categories RENAME COLUMN "Id" TO category_id;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'categories' AND column_name = 'Name') THEN
                        ALTER TABLE categories RENAME COLUMN "Name" TO name;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'categories' AND column_name = 'Description') THEN
                        ALTER TABLE categories RENAME COLUMN "Description" TO description;
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'Id') THEN
                        ALTER TABLE products RENAME COLUMN "Id" TO product_id;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'SKU') THEN
                        ALTER TABLE products RENAME COLUMN "SKU" TO sku;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'Name') THEN
                        ALTER TABLE products RENAME COLUMN "Name" TO name;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'Manufacturer') THEN
                        ALTER TABLE products RENAME COLUMN "Manufacturer" TO manufacturer;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'CPU') THEN
                        ALTER TABLE products RENAME COLUMN "CPU" TO cpu;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'RAM') THEN
                        ALTER TABLE products RENAME COLUMN "RAM" TO ram;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'Storage') THEN
                        ALTER TABLE products RENAME COLUMN "Storage" TO storage;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'GPU') THEN
                        ALTER TABLE products RENAME COLUMN "GPU" TO gpu;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'Screen') THEN
                        ALTER TABLE products RENAME COLUMN "Screen" TO screen;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'ImportPrice') THEN
                        ALTER TABLE products RENAME COLUMN "ImportPrice" TO import_price;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'SalePrice') THEN
                        ALTER TABLE products RENAME COLUMN "SalePrice" TO sale_price;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'Stock') THEN
                        ALTER TABLE products RENAME COLUMN "Stock" TO count;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'CategoryId') THEN
                        ALTER TABLE products RENAME COLUMN "CategoryId" TO category_id;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'Description') THEN
                        ALTER TABLE products RENAME COLUMN "Description" TO description;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'Image1') THEN
                        ALTER TABLE products RENAME COLUMN "Image1" TO image1;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'Image2') THEN
                        ALTER TABLE products RENAME COLUMN "Image2" TO image2;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'Image3') THEN
                        ALTER TABLE products RENAME COLUMN "Image3" TO image3;
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'orders' AND column_name = 'Id') THEN
                        ALTER TABLE orders RENAME COLUMN "Id" TO order_id;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'orders' AND column_name = 'CreatedTime') THEN
                        ALTER TABLE orders RENAME COLUMN "CreatedTime" TO created_time;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'orders' AND column_name = 'FinalPrice') THEN
                        ALTER TABLE orders RENAME COLUMN "FinalPrice" TO final_price;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'orders' AND column_name = 'Status') THEN
                        ALTER TABLE orders RENAME COLUMN "Status" TO status;
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'order_items' AND column_name = 'Id') THEN
                        ALTER TABLE order_items RENAME COLUMN "Id" TO order_item_id;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'order_items' AND column_name = 'OrderId') THEN
                        ALTER TABLE order_items RENAME COLUMN "OrderId" TO order_id;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'order_items' AND column_name = 'ProductId') THEN
                        ALTER TABLE order_items RENAME COLUMN "ProductId" TO product_id;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'order_items' AND column_name = 'Quantity') THEN
                        ALTER TABLE order_items RENAME COLUMN "Quantity" TO quantity;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'order_items' AND column_name = 'UnitSalePrice') THEN
                        ALTER TABLE order_items RENAME COLUMN "UnitSalePrice" TO unit_sale_price;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'order_items' AND column_name = 'TotalPrice') THEN
                        ALTER TABLE order_items RENAME COLUMN "TotalPrice" TO total_price;
                    END IF;
                END
                $$;

                CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                    "MigrationId" character varying(150) NOT NULL,
                    "ProductVersion" character varying(32) NOT NULL,
                    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
                );

                INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                SELECT '20260309120000_InitialCreate', '8.0.22'
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "__EFMigrationsHistory"
                    WHERE "MigrationId" = '20260309120000_InitialCreate'
                );
                """;

            await dbContext.Database.ExecuteSqlRawAsync(legacyUpgradeScript);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
