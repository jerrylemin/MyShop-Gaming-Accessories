using Microsoft.EntityFrameworkCore;
using ProjectTest.DataAccess;
using ProjectTest.Models;

namespace ProjectTest.Repositories;

public class OrderRepository
{
    private readonly MyShopDbContextFactory _dbContextFactory;

    public OrderRepository(MyShopDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<OrderSummary>> GetAllAsync()
    {
        return await GetAllAsync(new OrderQueryOptions());
    }

    public async Task<List<OrderSummary>> GetAllAsync(OrderQueryOptions options)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var query = dbContext.Orders
            .Include(x => x.Items)
            .AsQueryable();

        if (options.FromDate.HasValue)
        {
            var fromDate = options.FromDate.Value.Date;
            query = query.Where(x => x.CreatedTime >= fromDate);
        }

        if (options.ToDate.HasValue)
        {
            var toDateExclusive = options.ToDate.Value.Date.AddDays(1);
            query = query.Where(x => x.CreatedTime < toDateExclusive);
        }

        return await query
            .OrderByDescending(x => x.CreatedTime)
            .Select(x => new OrderSummary
            {
                Id = x.Id,
                CreatedTime = x.CreatedTime,
                FinalPrice = x.FinalPrice,
                Status = x.Status,
                ItemCount = x.Items.Count
            })
            .ToListAsync();
    }

    public async Task<OrderDraft?> GetDraftByIdAsync(int orderId)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var order = await dbContext.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == orderId);

        if (order is null)
        {
            return null;
        }

        return new OrderDraft
        {
            Id = order.Id,
            CreatedTime = order.CreatedTime,
            Status = order.Status,
            Items = order.Items.Select(x => new OrderDraftItem
            {
                ProductId = x.ProductId,
                ProductName = x.Product?.Name ?? string.Empty,
                Manufacturer = x.Product?.Manufacturer ?? string.Empty,
                UnitSalePrice = x.UnitSalePrice,
                Quantity = x.Quantity,
                AvailableStock = x.Product?.Stock ?? 0,
                ImagePath = x.Product?.Image1 ?? string.Empty
            }).ToList()
        };
    }

    public async Task<OperationResult<int>> SaveAsync(OrderDraft draft)
    {
        if (draft.Items.Count == 0)
        {
            return OperationResult<int>.Fail("An order must contain at least one item.");
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            Order order;
            List<OrderItem> previousItems = [];
            OrderStatus previousStatus = OrderStatus.Created;

            if (draft.Id == 0)
            {
                order = new Order();
                dbContext.Orders.Add(order);
            }
            else
            {
                order = await dbContext.Orders
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.Id == draft.Id)
                    ?? throw new InvalidOperationException("Order not found.");

                previousItems = order.Items.ToList();
                previousStatus = order.Status;
                ValidateTransition(previousStatus, draft.Status);
            }

            if (draft.Id != 0 && previousStatus != OrderStatus.Cancelled)
            {
                foreach (var item in previousItems)
                {
                    var product = await dbContext.Products.FirstAsync(x => x.Id == item.ProductId);
                    product.Stock += item.Quantity;
                }
            }

            if (draft.Id != 0)
            {
                dbContext.OrderItems.RemoveRange(previousItems);
                order.Items.Clear();
            }

            order.CreatedTime = draft.CreatedTime;
            order.Status = draft.Status;
            order.FinalPrice = draft.Items.Sum(x => x.TotalPrice);

            foreach (var item in draft.Items)
            {
                var product = await dbContext.Products.FirstAsync(x => x.Id == item.ProductId);
                if (draft.Status != OrderStatus.Cancelled)
                {
                    if (product.Stock < item.Quantity)
                    {
                        throw new InvalidOperationException($"Insufficient stock for {product.Name}.");
                    }

                    product.Stock -= item.Quantity;
                }

                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitSalePrice = item.UnitSalePrice,
                    TotalPrice = item.TotalPrice
                });
            }

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return OperationResult<int>.Ok(order.Id, draft.Id == 0 ? "Order created." : "Order updated.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return OperationResult<int>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult> DeleteAsync(int orderId)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var order = await dbContext.Orders
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order is null)
            {
                return OperationResult.Fail("Order not found.");
            }

            if (order.Status != OrderStatus.Cancelled)
            {
                foreach (var item in order.Items)
                {
                    var product = await dbContext.Products.FirstAsync(x => x.Id == item.ProductId);
                    product.Stock += item.Quantity;
                }
            }

            dbContext.Orders.Remove(order);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return OperationResult.Ok("Order deleted.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return OperationResult.Fail(ex.Message);
        }
    }

    private static void ValidateTransition(OrderStatus previousStatus, OrderStatus nextStatus)
    {
        var isValid = previousStatus == OrderStatus.Created &&
                      (nextStatus == OrderStatus.Created || nextStatus == OrderStatus.Paid || nextStatus == OrderStatus.Cancelled);

        if (!isValid)
        {
            throw new InvalidOperationException($"Invalid order status transition: {previousStatus} -> {nextStatus}.");
        }
    }
}
