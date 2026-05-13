using ProjectTest.Models;
using System.Reflection;
using System.Text.Json;

namespace ProjectTest.DataAccess.Seeding;

public static class GamingAccessorySeedGenerator
{
    private const string SeedResourceSuffix = "DataAccess.Seeding.gaming_accessories_seed_data.json";
    private const int MinimumProductsPerCategory = 22;
    private static readonly Lazy<IReadOnlyList<SeedProductRecord>> SeedCatalog = new(LoadSeedCatalog);

    public static List<Category> BuildCategories()
    {
        return
        [
            new Category
            {
                Name = "Gaming Keyboard",
                Description = "Mechanical and magnetic-switch keyboards for competitive gaming, creators, and RGB desk setups."
            },
            new Category
            {
                Name = "Gaming Mouse",
                Description = "High-precision wired and wireless gaming mice for esports, FPS, and all-day desktop play."
            },
            new Category
            {
                Name = "Gaming Headset",
                Description = "Gaming headsets with immersive audio, clear microphones, and low-latency connectivity."
            },
            new Category
            {
                Name = "Mousepad",
                Description = "Control, speed, and RGB mousepads sized for esports, battlestations, and streaming desks."
            },
            new Category
            {
                Name = "Streaming Gear",
                Description = "Webcams and microphones for live streaming, meetings, content creation, and studio-ready setups."
            }
        ];
    }

    public static List<Product> BuildProducts(IReadOnlyList<Category> categories)
    {
        var random = new Random(20260315);
        var categoryLookup = categories.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        var products = new List<Product>(SeedCatalog.Value.Count);

        foreach (var record in SeedCatalog.Value)
        {
            if (!categoryLookup.TryGetValue(record.Category, out var category))
            {
                throw new InvalidOperationException($"Missing category '{record.Category}' required by the seed dataset.");
            }

            var salePrice = record.PriceVnd;
            var importRatio = 0.74m + (decimal)(random.NextDouble() * 0.11);
            var importPrice = Math.Round(salePrice * importRatio, 0, MidpointRounding.AwayFromZero);
            var specs = record.Specs.Take(5).Concat(Enumerable.Repeat(string.Empty, 5)).Take(5).ToArray();

            products.Add(new Product
            {
                SKU = TrimToLength(record.Sku.Trim(), 40),
                Name = TrimToLength(record.Name.Trim(), 200),
                Manufacturer = TrimToLength(record.Brand.Trim(), 80),
                CPU = TrimToLength(specs[0], 120),
                RAM = TrimToLength(specs[1], 40),
                Storage = TrimToLength(specs[2], 80),
                GPU = TrimToLength(specs[3], 120),
                Screen = TrimToLength(specs[4], 80),
                ImportPrice = importPrice,
                SalePrice = salePrice,
                Stock = random.Next(5, 51),
                CategoryId = category.Id,
                Description = TrimToLength(record.ShortDescription.Trim(), 1200)
            });
        }

        return products;
    }

    public static List<Order> BuildOrders(IReadOnlyList<Product> products, int count = 0)
    {
        count = count <= 0 ? Math.Max(360, products.Count * 4) : count;
        var random = new Random(20260316);
        var inventory = products.ToDictionary(x => x.Id, x => x);
        var orders = new List<Order>(count);
        var currentMonthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var monthOptions = Enumerable.Range(0, 6)
            .Select(offset => currentMonthStart.AddMonths(-offset))
            .ToArray();

        for (var index = 0; index < count; index++)
        {
            var selectedProducts = inventory.Values
                .OrderBy(_ => random.Next())
                .Take(random.Next(1, 4))
                .ToList();

            var forceToday = index < 8;
            var createdTime = forceToday
                ? DateTime.Today
                    .AddHours(9 + index)
                    .AddMinutes(random.Next(0, 60))
                : BuildRandomOrderTime(monthOptions[random.Next(monthOptions.Length)], random);

            var status = forceToday
                ? index < 5 ? OrderStatus.Paid : OrderStatus.Created
                : RollStatus(random);

            var order = new Order
            {
                CreatedTime = createdTime,
                Status = status
            };

            foreach (var product in selectedProducts)
            {
                var allowedQuantity = Math.Max(1, Math.Min(3, product.Stock));
                var quantity = product.Stock == 0 ? 1 : random.Next(1, allowedQuantity + 1);
                var totalPrice = product.SalePrice * quantity;

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = quantity,
                    UnitSalePrice = product.SalePrice,
                    TotalPrice = totalPrice
                });

                if (status != OrderStatus.Cancelled)
                {
                    product.Stock = Math.Max(0, product.Stock - quantity);
                }
            }

            order.FinalPrice = order.Items.Sum(x => x.TotalPrice);
            orders.Add(order);
        }

        return orders;
    }

    public static string BuildImagePath(int productId, int imageNumber)
    {
        return $"ms-appx:///Assets/GamingProducts/{productId}_{imageNumber}.jpg";
    }

    private static DateTime BuildRandomOrderTime(DateTime monthStart, Random random)
    {
        var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
        var maxDay = monthStart.Year == DateTime.Today.Year && monthStart.Month == DateTime.Today.Month
            ? DateTime.Today.Day
            : daysInMonth;
        return monthStart
            .AddDays(random.Next(0, Math.Max(1, maxDay)))
            .AddHours(random.Next(9, 22))
            .AddMinutes(random.Next(0, 60));
    }

    private static OrderStatus RollStatus(Random random)
    {
        var value = random.Next(100);
        return value switch
        {
            < 68 => OrderStatus.Paid,
            < 92 => OrderStatus.Created,
            _ => OrderStatus.Cancelled
        };
    }

    private static string TrimToLength(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
        {
            return value;
        }

        return maxLength <= 3
            ? value[..maxLength]
            : $"{value[..(maxLength - 3)].TrimEnd()}...";
    }

    private static IReadOnlyList<SeedProductRecord> LoadSeedCatalog()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(SeedResourceSuffix, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Embedded resource matching '{SeedResourceSuffix}' was not found.");
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' could not be opened.");
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        var records = JsonSerializer.Deserialize<List<SeedProductRecord>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? [];

        var categoryCounts = records
            .GroupBy(x => x.Category, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        var invalidCategory = BuildCategories()
            .Select(x => x.Name)
            .FirstOrDefault(categoryName => !categoryCounts.TryGetValue(categoryName, out var count) || count < MinimumProductsPerCategory);

        if (invalidCategory is not null)
        {
            throw new InvalidOperationException(
                $"The gaming accessories dataset must contain at least {MinimumProductsPerCategory} products per category. Category failing validation: {invalidCategory}.");
        }

        return records;
    }

    private sealed class SeedProductRecord
    {
        public string Sku { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string ShortDescription { get; set; } = string.Empty;

        public decimal PriceVnd { get; set; }

        public List<string> Specs { get; set; } = [];
    }
}
