using Microsoft.EntityFrameworkCore;
using ProjectTest.DataAccess;
using ProjectTest.Models;

namespace ProjectTest.Repositories;

public class PromotionRepository
{
    private readonly MyShopDbContextFactory _dbContextFactory;

    public PromotionRepository(MyShopDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<Promotion>> GetAllAsync()
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Promotions.OrderByDescending(x => x.IsActive).ThenBy(x => x.Code).ToListAsync();
    }

    public async Task<List<Promotion>> GetActiveAsync()
    {
        var today = DateTime.Today;
        await using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Promotions
            .Where(x => x.IsActive && x.StartDate <= today && x.EndDate >= today)
            .OrderBy(x => x.Code)
            .ToListAsync();
    }

    public async Task<OperationResult<int>> SaveAsync(Promotion promotion)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        if (promotion.Id == 0)
        {
            dbContext.Promotions.Add(promotion);
        }
        else
        {
            var existing = await dbContext.Promotions.FirstOrDefaultAsync(x => x.Id == promotion.Id);
            if (existing is null)
            {
                return OperationResult<int>.Fail("Promotion not found.");
            }

            existing.Code = promotion.Code;
            existing.Name = promotion.Name;
            existing.DiscountType = promotion.DiscountType;
            existing.DiscountValue = promotion.DiscountValue;
            existing.StartDate = promotion.StartDate;
            existing.EndDate = promotion.EndDate;
            existing.IsActive = promotion.IsActive;
            existing.MinimumOrderTotal = promotion.MinimumOrderTotal;
        }

        await dbContext.SaveChangesAsync();
        return OperationResult<int>.Ok(promotion.Id, "Promotion saved.");
    }
}
