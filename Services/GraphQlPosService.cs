using GraphQL;
using GraphQL.Types;
using ProjectTest.Models;
using ProjectTest.Repositories;
using System.Text.Json;

namespace ProjectTest.Services;

#pragma warning disable CS0618, GQL004
public class GraphQlPosService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly ProductRepository _productRepository;
    private readonly OrderRepository _orderRepository;
    private readonly ReportingService _reportingService;
    private readonly ISchema _schema;

    public GraphQlPosService(ProductRepository productRepository, OrderRepository orderRepository, ReportingService reportingService)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _reportingService = reportingService;
        _schema = BuildSchema();
    }

    public async Task<string> ExecuteAsync(string query, object? variables = null)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return JsonSerializer.Serialize(new
            {
                errors = new[] { new { message = "GraphQL query is required." } }
            }, JsonOptions);
        }

        var inputs = variables is null
            ? null
            : JsonSerializer.Deserialize<Inputs>(JsonSerializer.Serialize(variables, JsonOptions));

        var result = await new DocumentExecuter().ExecuteAsync(options =>
        {
            options.Schema = _schema;
            options.Query = query;
            options.Variables = inputs;
        });

        object payload = result.Errors?.Count > 0
            ? new
            {
                data = result.Data,
                errors = result.Errors.Select(error => new { message = error.Message })
            }
            : new { data = result.Data };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    public string GetSampleQuery()
    {
        return """
            query Demo {
              products(pageNumber: 1, pageSize: 5, keyword: "Logitech") {
                pageNumber
                totalCount
                items {
                  id
                  sku
                  name
                  brand
                  salePrice
                  stock
                }
              }
              reports {
                rangeLabel
                totalRevenue
                totalProfit
                mlInsights {
                  title
                  detail
                  score
                }
              }
            }
            """;
    }

    private ISchema BuildSchema()
    {
        var query = new ObjectGraphType { Name = "MyShopQuery" };
        query.FieldAsync<ProductPagedResultGraphType>(
            "products",
            "Products with paging, keyword/category/price filters, and sort options.",
            new QueryArguments(
                new QueryArgument<IntGraphType> { Name = "pageNumber", DefaultValue = 1 },
                new QueryArgument<IntGraphType> { Name = "pageSize", DefaultValue = 10 },
                new QueryArgument<StringGraphType> { Name = "keyword" },
                new QueryArgument<IntGraphType> { Name = "categoryId" },
                new QueryArgument<DecimalGraphType> { Name = "minPrice" },
                new QueryArgument<DecimalGraphType> { Name = "maxPrice" },
                new QueryArgument<ProductSortOptionGraphType> { Name = "sort" }),
            async context => await _productRepository.GetPagedAsync(new ProductQueryOptions
            {
                PageNumber = context.GetArgument("pageNumber", 1),
                PageSize = context.GetArgument("pageSize", 10),
                Keyword = context.GetArgument("keyword", string.Empty),
                CategoryId = context.GetArgument<int?>("categoryId"),
                MinPrice = context.GetArgument<decimal?>("minPrice"),
                MaxPrice = context.GetArgument<decimal?>("maxPrice"),
                SortOption = context.GetArgument("sort", ProductSortOption.Name)
            }));

        query.FieldAsync<OrderPagedResultGraphType>(
            "orders",
            "Orders with paging, date range, status, keyword, customer, and total filters.",
            new QueryArguments(
                new QueryArgument<IntGraphType> { Name = "pageNumber", DefaultValue = 1 },
                new QueryArgument<IntGraphType> { Name = "pageSize", DefaultValue = 10 },
                new QueryArgument<StringGraphType> { Name = "keyword" },
                new QueryArgument<DateTimeGraphType> { Name = "fromDate" },
                new QueryArgument<DateTimeGraphType> { Name = "toDate" },
                new QueryArgument<OrderStatusGraphType> { Name = "status" },
                new QueryArgument<IntGraphType> { Name = "customerId" }),
            async context => await _orderRepository.GetPagedAsync(new OrderQueryOptions
            {
                PageNumber = context.GetArgument("pageNumber", 1),
                PageSize = context.GetArgument("pageSize", 10),
                Keyword = context.GetArgument("keyword", string.Empty),
                FromDate = context.GetArgument<DateTime?>("fromDate"),
                ToDate = context.GetArgument<DateTime?>("toDate"),
                Status = context.GetArgument<OrderStatus?>("status"),
                CustomerId = context.GetArgument<int?>("customerId")
            }));

        query.FieldAsync<ReportsSnapshotGraphType>(
            "reports",
            "Reporting snapshot for the selected date range.",
            new QueryArguments(
                new QueryArgument<DateTimeGraphType> { Name = "fromDate" },
                new QueryArgument<DateTimeGraphType> { Name = "toDate" }),
            async context => await _reportingService.GetSnapshotAsync(new ReportQueryOptions
            {
                FromDate = context.GetArgument("fromDate", DateTime.Today.AddDays(-30)),
                ToDate = context.GetArgument("toDate", DateTime.Today)
            }));

        var mutation = new ObjectGraphType { Name = "MyShopMutation" };
        mutation.FieldAsync<OperationResultIntGraphType>(
            "saveProduct",
            "Creates or updates a product. Pass the Product JSON object as inputJson.",
            new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "inputJson" }),
            async context =>
            {
                var product = JsonSerializer.Deserialize<Product>(context.GetArgument<string>("inputJson"), JsonOptions)
                    ?? new Product();
                return await _productRepository.SaveAsync(product);
            });

        mutation.FieldAsync<OperationResultIntGraphType>(
            "saveOrder",
            "Creates or updates an order. Pass the OrderDraft JSON object as inputJson.",
            new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "inputJson" }),
            async context =>
            {
                var draft = JsonSerializer.Deserialize<OrderDraft>(context.GetArgument<string>("inputJson"), JsonOptions)
                    ?? new OrderDraft();
                return await _orderRepository.SaveAsync(draft);
            });

        return new Schema
        {
            Query = query,
            Mutation = mutation
        };
    }
}

public sealed class ProductSortOptionGraphType : EnumerationGraphType<ProductSortOption>
{
}

public sealed class OrderStatusGraphType : EnumerationGraphType<OrderStatus>
{
}

public sealed class ProductGraphType : ObjectGraphType<Product>
{
    public ProductGraphType()
    {
        Name = "Product";
        Field<IntGraphType>("id").Resolve(context => context.Source.Id);
        Field<StringGraphType>("sku").Resolve(context => context.Source.SKU);
        Field<StringGraphType>("name").Resolve(context => context.Source.Name);
        Field<StringGraphType>("brand").Resolve(context => context.Source.Brand);
        Field<StringGraphType>("category").Resolve(context => context.Source.Category?.Name ?? string.Empty);
        Field<DecimalGraphType>("importPrice").Resolve(context => context.Source.ImportPrice);
        Field<DecimalGraphType>("salePrice").Resolve(context => context.Source.SalePrice);
        Field<IntGraphType>("stock").Resolve(context => context.Source.Stock);
        Field<ListGraphType<StringGraphType>>("accessorySpecs").Resolve(context => context.Source.AccessorySpecs);
    }
}

public sealed class ProductPagedResultGraphType : ObjectGraphType<PagedResult<Product>>
{
    public ProductPagedResultGraphType()
    {
        Name = "ProductPagedResult";
        Field<IntGraphType>("pageNumber").Resolve(context => context.Source.PageNumber);
        Field<IntGraphType>("pageSize").Resolve(context => context.Source.PageSize);
        Field<IntGraphType>("totalCount").Resolve(context => context.Source.TotalCount);
        Field<IntGraphType>("totalPages").Resolve(context => context.Source.TotalPages);
        Field<ListGraphType<ProductGraphType>>("items").Resolve(context => context.Source.Items);
    }
}

public sealed class OrderSummaryGraphType : ObjectGraphType<OrderSummary>
{
    public OrderSummaryGraphType()
    {
        Name = "OrderSummary";
        Field<IntGraphType>("id").Resolve(context => context.Source.Id);
        Field<DateTimeGraphType>("createdTime").Resolve(context => context.Source.CreatedTime);
        Field<DecimalGraphType>("finalPrice").Resolve(context => context.Source.FinalPrice);
        Field<StringGraphType>("status").Resolve(context => context.Source.Status.ToString());
        Field<IntGraphType>("itemCount").Resolve(context => context.Source.ItemCount);
        Field<StringGraphType>("customerName").Resolve(context => context.Source.CustomerName);
        Field<StringGraphType>("salesperson").Resolve(context => context.Source.Salesperson);
        Field<DecimalGraphType>("discountAmount").Resolve(context => context.Source.DiscountAmount);
    }
}

public sealed class OrderPagedResultGraphType : ObjectGraphType<PagedResult<OrderSummary>>
{
    public OrderPagedResultGraphType()
    {
        Name = "OrderPagedResult";
        Field<IntGraphType>("pageNumber").Resolve(context => context.Source.PageNumber);
        Field<IntGraphType>("pageSize").Resolve(context => context.Source.PageSize);
        Field<IntGraphType>("totalCount").Resolve(context => context.Source.TotalCount);
        Field<IntGraphType>("totalPages").Resolve(context => context.Source.TotalPages);
        Field<ListGraphType<OrderSummaryGraphType>>("items").Resolve(context => context.Source.Items);
    }
}

public sealed class MlInsightGraphType : ObjectGraphType<MlInsight>
{
    public MlInsightGraphType()
    {
        Name = "MlInsight";
        Field<StringGraphType>("title").Resolve(context => context.Source.Title);
        Field<StringGraphType>("detail").Resolve(context => context.Source.Detail);
        Field<DecimalGraphType>("score").Resolve(context => context.Source.Score);
    }
}

public sealed class ReportsSnapshotGraphType : ObjectGraphType<ReportsSnapshot>
{
    public ReportsSnapshotGraphType()
    {
        Name = "ReportsSnapshot";
        Field<StringGraphType>("rangeLabel").Resolve(context => context.Source.RangeLabel);
        Field<DecimalGraphType>("totalRevenue").Resolve(context => context.Source.TotalRevenue);
        Field<DecimalGraphType>("totalProfit").Resolve(context => context.Source.TotalProfit);
        Field<ListGraphType<MlInsightGraphType>>("mlInsights").Resolve(context => context.Source.MlInsights);
    }
}

public sealed class OperationResultIntGraphType : ObjectGraphType<OperationResult<int>>
{
    public OperationResultIntGraphType()
    {
        Name = "OperationResultInt";
        Field<BooleanGraphType>("success").Resolve(context => context.Source.Success);
        Field<StringGraphType>("message").Resolve(context => context.Source.Message);
        Field<IntGraphType>("value").Resolve(context => context.Source.Value);
    }
}
#pragma warning restore CS0618, GQL004
