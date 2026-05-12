using ProjectTest.Models;
using ProjectTest.Repositories;
using System.Text.Json;

namespace ProjectTest.Services;

public class GraphQlPosService
{
    private readonly ProductRepository _productRepository;
    private readonly OrderRepository _orderRepository;
    private readonly ReportingService _reportingService;

    public GraphQlPosService(ProductRepository productRepository, OrderRepository orderRepository, ReportingService reportingService)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _reportingService = reportingService;
    }

    public async Task<string> ExecuteAsync(string operation, object? variables = null)
    {
        var normalized = operation.Trim().ToLowerInvariant();
        object result = normalized switch
        {
            "query products" => await _productRepository.GetPagedAsync(Read<ProductQueryOptions>(variables) ?? new ProductQueryOptions()),
            "query orders" => await _orderRepository.GetPagedAsync(Read<OrderQueryOptions>(variables) ?? new OrderQueryOptions()),
            "query reports" => await _reportingService.GetSnapshotAsync(Read<ReportQueryOptions>(variables) ?? new ReportQueryOptions()),
            "mutation saveorder" => await _orderRepository.SaveAsync(Read<OrderDraft>(variables) ?? new OrderDraft()),
            "mutation saveproduct" => await _productRepository.SaveAsync(Read<Product>(variables) ?? new Product()),
            _ => OperationResult.Fail($"Unsupported GraphQL operation: {operation}")
        };

        return JsonSerializer.Serialize(result);
    }

    private static T? Read<T>(object? variables)
    {
        if (variables is null)
        {
            return default;
        }

        return variables is T value
            ? value
            : JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(variables));
    }
}
