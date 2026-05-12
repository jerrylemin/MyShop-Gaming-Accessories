using Microsoft.EntityFrameworkCore;
using ProjectTest.DataAccess;
using ProjectTest.Helpers;
using ProjectTest.Models;

namespace ProjectTest.Repositories;

public class ProductRepository
{
    private readonly MyShopDbContextFactory _dbContextFactory;

    public ProductRepository(MyShopDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<PagedResult<Product>> GetPagedAsync(ProductQueryOptions options)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var query = dbContext.Products.Include(x => x.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(options.Keyword))
        {
            var keyword = options.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(keyword) ||
                x.Manufacturer.ToLower().Contains(keyword) ||
                x.SKU.ToLower().Contains(keyword) ||
                x.CPU.ToLower().Contains(keyword) ||
                x.RAM.ToLower().Contains(keyword) ||
                x.Storage.ToLower().Contains(keyword) ||
                x.GPU.ToLower().Contains(keyword) ||
                x.Screen.ToLower().Contains(keyword) ||
                x.Description.ToLower().Contains(keyword));
        }

        if (options.MinPrice.HasValue)
        {
            query = query.Where(x => x.SalePrice >= options.MinPrice.Value);
        }

        if (options.MaxPrice.HasValue)
        {
            query = query.Where(x => x.SalePrice <= options.MaxPrice.Value);
        }

        if (options.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == options.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(options.Manufacturer))
        {
            var manufacturer = options.Manufacturer.Trim().ToLowerInvariant();
            query = query.Where(x => x.Manufacturer.ToLower().Contains(manufacturer));
        }

        if (options.MinStock.HasValue)
        {
            query = query.Where(x => x.Stock >= options.MinStock.Value);
        }

        if (options.MaxStock.HasValue)
        {
            query = query.Where(x => x.Stock <= options.MaxStock.Value);
        }

        query = options.SortOption switch
        {
            ProductSortOption.PriceLowToHigh => options.SortDescending ? query.OrderByDescending(x => x.SalePrice).ThenBy(x => x.Name) : query.OrderBy(x => x.SalePrice).ThenBy(x => x.Name),
            ProductSortOption.PriceHighToLow => options.SortDescending ? query.OrderBy(x => x.SalePrice).ThenBy(x => x.Name) : query.OrderByDescending(x => x.SalePrice).ThenBy(x => x.Name),
            ProductSortOption.StockHighToLow => options.SortDescending ? query.OrderBy(x => x.Stock).ThenBy(x => x.Name) : query.OrderByDescending(x => x.Stock).ThenBy(x => x.Name),
            _ => options.SortDescending ? query.OrderByDescending(x => x.Name).ThenBy(x => x.Manufacturer) : query.OrderBy(x => x.Name).ThenBy(x => x.Manufacturer)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((options.PageNumber - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Items = items,
            PageNumber = options.PageNumber,
            PageSize = options.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<List<ProductLookupItem>> GetLookupAsync()
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Products
            .OrderBy(x => x.Name)
            .Select(x => new ProductLookupItem
            {
                Id = x.Id,
                Name = x.Name,
                Manufacturer = x.Manufacturer,
                SalePrice = x.SalePrice,
                Stock = x.Stock,
                ImagePath = x.Image1
            })
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Products.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<OperationResult<int>> SaveAsync(Product product)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        if (product.Id == 0)
        {
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            product.Image1 = string.IsNullOrWhiteSpace(product.Image1) ? ImageSourceHelper.DefaultProductImagePath : product.Image1;
            product.Image2 = string.IsNullOrWhiteSpace(product.Image2) ? ImageSourceHelper.DefaultProductImagePath : product.Image2;
            product.Image3 = string.IsNullOrWhiteSpace(product.Image3) ? ImageSourceHelper.DefaultProductImagePath : product.Image3;
            await dbContext.SaveChangesAsync();

            return OperationResult<int>.Ok(product.Id, "Product created.");
        }

        var existing = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == product.Id);
        if (existing is null)
        {
            return OperationResult<int>.Fail("Product not found.");
        }

        existing.SKU = product.SKU;
        existing.Name = product.Name;
        existing.Manufacturer = product.Manufacturer;
        existing.CPU = product.CPU;
        existing.RAM = product.RAM;
        existing.Storage = product.Storage;
        existing.GPU = product.GPU;
        existing.Screen = product.Screen;
        existing.ImportPrice = product.ImportPrice;
        existing.SalePrice = product.SalePrice;
        existing.Stock = product.Stock;
        existing.CategoryId = product.CategoryId;
        existing.Description = product.Description;
        existing.Image1 = string.IsNullOrWhiteSpace(product.Image1) ? ImageSourceHelper.DefaultProductImagePath : product.Image1;
        existing.Image2 = string.IsNullOrWhiteSpace(product.Image2) ? ImageSourceHelper.DefaultProductImagePath : product.Image2;
        existing.Image3 = string.IsNullOrWhiteSpace(product.Image3) ? ImageSourceHelper.DefaultProductImagePath : product.Image3;

        await dbContext.SaveChangesAsync();
        return OperationResult<int>.Ok(existing.Id, "Product updated.");
    }

    public async Task<OperationResult> DeleteAsync(int productId)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var product = await dbContext.Products.Include(x => x.OrderItems).FirstOrDefaultAsync(x => x.Id == productId);
        if (product is null)
        {
            return OperationResult.Fail("Product not found.");
        }

        if (product.OrderItems.Count > 0)
        {
            return OperationResult.Fail("Products with order history cannot be deleted.");
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync();
        return OperationResult.Ok("Product deleted.");
    }
}
