using Microsoft.EntityFrameworkCore;
using ProjectTest.DataAccess;
using ProjectTest.Models;

namespace ProjectTest.Repositories;

public class CustomerRepository
{
    private readonly MyShopDbContextFactory _dbContextFactory;

    public CustomerRepository(MyShopDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Customers
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<List<Customer>> SearchAsync(string keyword)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var query = dbContext.Customers
            .AsNoTracking()
            .AsQueryable();

        var search = keyword.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Name, pattern) ||
                EF.Functions.ILike(x.Phone, pattern) ||
                EF.Functions.ILike(x.Email, pattern));
        }

        return await query
            .OrderBy(x => x.Name)
            .Take(200)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int customerId)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == customerId);
    }

    public async Task<Customer?> GetByPhoneAsync(string phone)
    {
        var normalizedPhone = phone.Trim();
        if (string.IsNullOrWhiteSpace(normalizedPhone))
        {
            return null;
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Phone == normalizedPhone);
    }

    public async Task<CustomerProfile?> GetProfileAsync(int customerId)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var customer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == customerId);

        if (customer is null)
        {
            return null;
        }

        var orders = await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedTime)
            .Take(50)
            .ToListAsync();

        var products = orders
            .SelectMany(x => x.Items)
            .GroupBy(x => new
            {
                x.ProductId,
                Name = x.Product == null ? "Unknown product" : x.Product.Name,
                Brand = x.Product == null ? string.Empty : x.Product.Brand
            })
            .Select(group => new CustomerPurchasedProduct
            {
                ProductId = group.Key.ProductId,
                ProductName = group.Key.Name,
                Brand = group.Key.Brand,
                Quantity = group.Sum(x => x.Quantity),
                TotalSpend = group.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.TotalSpend)
            .ThenByDescending(x => x.Quantity)
            .ToList();

        return new CustomerProfile
        {
            Customer = customer,
            Orders = orders.Select(x => new CustomerOrderHistory
            {
                OrderId = x.Id,
                CreatedTime = x.CreatedTime,
                Status = x.Status,
                ItemCount = x.Items.Count,
                FinalPrice = x.FinalPrice,
                DiscountAmount = x.DiscountAmount
            }).ToList(),
            PurchasedProducts = products,
            PaidOrderCount = orders.Count(x => x.Status == OrderStatus.Paid),
            TotalOrderCount = orders.Count,
            TotalSpend = orders.Where(x => x.Status == OrderStatus.Paid).Sum(x => x.FinalPrice)
        };
    }

    public async Task<OperationResult<int>> SaveAsync(Customer customer)
    {
        if (string.IsNullOrWhiteSpace(customer.Name))
        {
            return OperationResult<int>.Fail("Customer name is required.");
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();
        if (customer.Id == 0)
        {
            customer.Name = customer.Name.Trim();
            customer.Phone = customer.Phone.Trim();
            customer.Email = customer.Email.Trim();
            dbContext.Customers.Add(customer);
        }
        else
        {
            var existing = await dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customer.Id);
            if (existing is null)
            {
                return OperationResult<int>.Fail("Customer not found.");
            }

            existing.Name = customer.Name.Trim();
            existing.Phone = customer.Phone.Trim();
            existing.Email = customer.Email.Trim();
        }

        await dbContext.SaveChangesAsync();
        return OperationResult<int>.Ok(customer.Id, "Customer saved.");
    }

    public async Task<OperationResult> DeleteAsync(int customerId)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var customer = await dbContext.Customers
            .Include(x => x.Orders)
            .FirstOrDefaultAsync(x => x.Id == customerId);

        if (customer is null)
        {
            return OperationResult.Fail("Customer not found.");
        }

        if (customer.Orders.Count > 0)
        {
            return OperationResult.Fail("Customers with order history cannot be deleted. Edit the info instead.");
        }

        dbContext.Customers.Remove(customer);
        await dbContext.SaveChangesAsync();
        return OperationResult.Ok("Customer deleted.");
    }
}

public class CustomerProfile
{
    public Customer Customer { get; set; } = new();

    public List<CustomerOrderHistory> Orders { get; set; } = [];

    public List<CustomerPurchasedProduct> PurchasedProducts { get; set; } = [];

    public int TotalOrderCount { get; set; }

    public int PaidOrderCount { get; set; }

    public decimal TotalSpend { get; set; }

    public string TotalSpendText => Helpers.CurrencyFormatter.ToCurrency(TotalSpend);
}

public class CustomerOrderHistory
{
    public int OrderId { get; set; }

    public DateTime CreatedTime { get; set; }

    public OrderStatus Status { get; set; }

    public int ItemCount { get; set; }

    public decimal FinalPrice { get; set; }

    public decimal DiscountAmount { get; set; }

    public string OrderLabel => $"Order #{OrderId}";

    public string CreatedDisplay => CreatedTime.ToString("dd/MM/yyyy HH:mm");

    public string StatusLabel => Status.ToString();

    public string FinalPriceText => Helpers.CurrencyFormatter.ToCurrency(FinalPrice);
}

public class CustomerPurchasedProduct
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal TotalSpend { get; set; }

    public string TotalSpendText => Helpers.CurrencyFormatter.ToCurrency(TotalSpend);

    public string QuantityText => $"{Quantity} item(s)";
}
