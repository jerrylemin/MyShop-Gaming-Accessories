using Microsoft.EntityFrameworkCore;
using ProjectTest.DataAccess;
using ProjectTest.Models;

namespace ProjectTest.Repositories;

public class OrderRepository
{
    private readonly MyShopDbContextFactory _dbContextFactory;
    private readonly Services.DiscountService _discountService;

    public OrderRepository(MyShopDbContextFactory dbContextFactory, Services.DiscountService? discountService = null)
    {
        _dbContextFactory = dbContextFactory;
        _discountService = discountService ?? new Services.DiscountService();
    }

    public async Task<List<OrderSummary>> GetAllAsync()
    {
        return await GetAllAsync(new OrderQueryOptions());
    }

    public async Task<List<OrderSummary>> GetAllAsync(OrderQueryOptions options)
    {
        var paged = await GetPagedAsync(options);
        return paged.Items.ToList();
    }

    public async Task<PagedResult<OrderSummary>> GetPagedAsync(OrderQueryOptions options)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var query = dbContext.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .Include(x => x.Customer)
            .Include(x => x.CreatedByUser)
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

        if (options.Status.HasValue)
        {
            query = query.Where(x => x.Status == options.Status.Value);
        }

        if (options.CustomerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == options.CustomerId.Value);
        }

        if (options.MinTotal.HasValue)
        {
            query = query.Where(x => x.FinalPrice >= options.MinTotal.Value);
        }

        if (options.MaxTotal.HasValue)
        {
            query = query.Where(x => x.FinalPrice <= options.MaxTotal.Value);
        }

        if (options.CurrentUserRole == UserRole.Sale && options.CurrentUserId.HasValue)
        {
            query = query.Where(x => x.CreatedByUserId == options.CurrentUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(options.Keyword))
        {
            var keyword = options.Keyword.Trim().ToLower();
            var parsedOrderId = int.TryParse(keyword, out var orderId);
            query = query.Where(x =>
                (parsedOrderId && x.Id == orderId) ||
                x.Status.ToString().ToLower().Contains(keyword) ||
                x.Items.Any(item =>
                    item.Product != null &&
                    (item.Product.Name.ToLower().Contains(keyword) ||
                     item.Product.Manufacturer.ToLower().Contains(keyword) ||
                     item.Product.SKU.ToLower().Contains(keyword))));
        }

        query = options.SortOption switch
        {
            OrderSortOption.OldestFirst => query.OrderBy(x => x.CreatedTime).ThenBy(x => x.Id),
            OrderSortOption.HighestValue => query.OrderByDescending(x => x.FinalPrice).ThenByDescending(x => x.CreatedTime),
            OrderSortOption.LowestValue => query.OrderBy(x => x.FinalPrice).ThenByDescending(x => x.CreatedTime),
            _ => query.OrderByDescending(x => x.CreatedTime).ThenByDescending(x => x.Id)
        };

        var totalCount = await query.CountAsync();
        var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)Math.Max(1, options.PageSize));
        var pageNumber = Math.Min(Math.Max(1, options.PageNumber), totalPages);

        var items = await query
            .Skip((pageNumber - 1) * options.PageSize)
            .Take(options.PageSize)
            .Select(x => new OrderSummary
            {
                Id = x.Id,
                CreatedTime = x.CreatedTime,
                FinalPrice = x.FinalPrice,
                Status = x.Status,
                ItemCount = x.Items.Count,
                CustomerName = x.Customer == null ? string.Empty : x.Customer.Name,
                Salesperson = x.CreatedByUser == null ? string.Empty : x.CreatedByUser.DisplayName,
                DiscountAmount = x.DiscountAmount
            })
            .ToListAsync();

        return new PagedResult<OrderSummary>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = options.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OrderDraft?> GetDraftByIdAsync(int orderId)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var order = await dbContext.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .Include(x => x.Customer)
            .Include(x => x.Promotion)
            .Include(x => x.CreatedByUser)
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
            CustomerId = order.CustomerId,
            CustomerName = order.Customer?.Name ?? string.Empty,
            PromotionId = order.PromotionId,
            CreatedByUserId = order.CreatedByUserId,
            SalespersonName = order.CreatedByUser?.DisplayName ?? string.Empty,
            DiscountAmount = order.DiscountAmount,
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
            order.CustomerId = draft.CustomerId;
            order.PromotionId = draft.PromotionId;
            order.CreatedByUserId = draft.CreatedByUserId;
            order.Subtotal = draft.Items.Sum(x => x.TotalPrice);
            var promotion = draft.PromotionId.HasValue
                ? await dbContext.Promotions.FirstOrDefaultAsync(x => x.Id == draft.PromotionId.Value)
                : null;
            order.DiscountAmount = draft.DiscountAmount > 0 ? Math.Min(draft.DiscountAmount, order.Subtotal) : _discountService.CalculateDiscount(order.Subtotal, promotion);
            order.FinalPrice = Math.Max(0m, order.Subtotal - order.DiscountAmount);

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
                    UnitCostPrice = product.ImportPrice,
                    UnitSalePrice = item.UnitSalePrice,
                    TotalPrice = item.TotalPrice
                });
            }

            await dbContext.SaveChangesAsync();
            await ApplyLoyaltyAsync(dbContext, order);
            await transaction.CommitAsync();
            return OperationResult<int>.Ok(order.Id, draft.Id == 0 ? "Order created." : "Order updated.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return OperationResult<int>.Fail(ex.Message);
        }
    }

    private static async Task ApplyLoyaltyAsync(MyShopDbContext dbContext, Order order)
    {
        if (order.CustomerId is null || order.Status != OrderStatus.Paid)
        {
            return;
        }

        var customer = await dbContext.Customers.FirstOrDefaultAsync(x => x.Id == order.CustomerId.Value);
        if (customer is null)
        {
            return;
        }

        var existingPoints = await dbContext.CustomerLoyaltyTransactions
            .Where(x => x.OrderId == order.Id)
            .SumAsync(x => (int?)x.Points) ?? 0;
        var targetPoints = (int)Math.Floor(order.FinalPrice / 100000m);
        var delta = targetPoints - existingPoints;
        if (delta == 0)
        {
            return;
        }

        customer.LoyaltyPoints += delta;
        customer.LifetimeSpend += order.FinalPrice;
        dbContext.CustomerLoyaltyTransactions.Add(new CustomerLoyaltyTransaction
        {
            CustomerId = customer.Id,
            OrderId = order.Id,
            Points = delta,
            Reason = "Paid order loyalty points",
            CreatedTime = DateTime.Now
        });

        await dbContext.SaveChangesAsync();
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
