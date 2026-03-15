using Microsoft.EntityFrameworkCore;
using ProjectTest.DataAccess;
using ProjectTest.Models;

namespace ProjectTest.Repositories;

public class CategoryRepository
{
    private readonly MyShopDbContextFactory _dbContextFactory;

    public CategoryRepository(MyShopDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Categories.OrderBy(x => x.Name).ToListAsync();
    }
}
