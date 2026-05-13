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

    public async Task<List<CategoryListItem>> GetListItemsAsync()
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Categories
            .OrderBy(x => x.Name)
            .Select(x => new CategoryListItem
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ProductCount = x.Products.Count
            })
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<OperationResult<int>> SaveAsync(Category category)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        if (category.Id == 0)
        {
            var exists = await dbContext.Categories.AnyAsync(x => x.Name.ToLower() == category.Name.ToLower());
            if (exists)
            {
                return OperationResult<int>.Fail("Category name already exists.");
            }

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();
            return OperationResult<int>.Ok(category.Id, "Category created.");
        }

        var existing = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == category.Id);
        if (existing is null)
        {
            return OperationResult<int>.Fail("Category not found.");
        }

        var duplicate = await dbContext.Categories.AnyAsync(x => x.Id != category.Id && x.Name.ToLower() == category.Name.ToLower());
        if (duplicate)
        {
            return OperationResult<int>.Fail("Category name already exists.");
        }

        existing.Name = category.Name.Trim();
        existing.Description = category.Description.Trim();
        await dbContext.SaveChangesAsync();
        return OperationResult<int>.Ok(existing.Id, "Category updated.");
    }

    public async Task<OperationResult> DeleteAsync(int categoryId)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var category = await dbContext.Categories
            .Include(x => x.Products)
            .FirstOrDefaultAsync(x => x.Id == categoryId);

        if (category is null)
        {
            return OperationResult.Fail("Category not found.");
        }

        if (category.Products.Count > 0)
        {
            return OperationResult.Fail("This category is still used by products. Move those products first before deleting.");
        }

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync();
        return OperationResult.Ok("Category deleted.");
    }
}
