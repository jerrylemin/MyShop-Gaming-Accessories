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
        return await dbContext.Customers.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<OperationResult<int>> SaveAsync(Customer customer)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        if (customer.Id == 0)
        {
            dbContext.Customers.Add(customer);
        }
        else
        {
            var existing = await dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customer.Id);
            if (existing is null)
            {
                return OperationResult<int>.Fail("Customer not found.");
            }

            existing.Name = customer.Name;
            existing.Phone = customer.Phone;
            existing.Email = customer.Email;
        }

        await dbContext.SaveChangesAsync();
        return OperationResult<int>.Ok(customer.Id, "Customer saved.");
    }
}
