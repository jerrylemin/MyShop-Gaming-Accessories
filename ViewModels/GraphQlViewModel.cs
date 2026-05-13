using ProjectTest.Helpers;
using ProjectTest.Services;
using System.Text.Json;

namespace ProjectTest.ViewModels;

public class GraphQlViewModel : ViewModelBase
{
    private readonly GraphQlPosService _graphQlPosService;
    private string _graphQlQuery = string.Empty;
    private string _graphQlResult = string.Empty;
    private string _statusMessage = string.Empty;

    public GraphQlViewModel(GraphQlPosService graphQlPosService)
    {
        _graphQlPosService = graphQlPosService;
        ExecuteGraphQlCommand = new AsyncRelayCommand(ExecuteGraphQlAsync);
        LoadSampleGraphQlCommand = new RelayCommand(LoadSampleGraphQl);
        LoadSchemaCommand = new RelayCommand(LoadSchemaQuery);
        LoadOrderDetailCommand = new RelayCommand(LoadOrderDetailQuery);
        LoadMutationSampleCommand = new RelayCommand(LoadMutationSample);
        LoadSampleGraphQl();
    }

    public AsyncRelayCommand ExecuteGraphQlCommand { get; }

    public RelayCommand LoadSampleGraphQlCommand { get; }

    public RelayCommand LoadSchemaCommand { get; }

    public RelayCommand LoadOrderDetailCommand { get; }

    public RelayCommand LoadMutationSampleCommand { get; }

    public string GraphQlQuery
    {
        get => _graphQlQuery;
        set => SetProperty(ref _graphQlQuery, value);
    }

    public string GraphQlResult
    {
        get => _graphQlResult;
        set => SetProperty(ref _graphQlResult, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private void LoadSampleGraphQl()
    {
        GraphQlQuery = _graphQlPosService.GetSampleQuery();
        GraphQlResult = "Sample POS query loaded. Click Execute to run GraphQL.";
        StatusMessage = "Ready.";
    }

    private void LoadSchemaQuery()
    {
        GraphQlQuery = """
            query Schema {
              schemaSummary
            }
            """;
        GraphQlResult = "Schema summary query loaded.";
        StatusMessage = "Ready.";
    }

    private void LoadOrderDetailQuery()
    {
        GraphQlQuery = """
            query OrderDetail {
              orderById(id: 1) {
                id
                createdTime
                status
                customerName
                discountAmount
                items {
                  productId
                  productName
                  quantity
                  unitSalePrice
                  totalPrice
                }
              }
            }
            """;
        GraphQlResult = "Order detail query loaded. Change id if needed.";
        StatusMessage = "Ready.";
    }

    private void LoadMutationSample()
    {
        GraphQlQuery = """
            mutation SaveOrderDemo {
              saveOrder(inputJson: "{\"id\":0,\"status\":0,\"createdTime\":\"2026-01-01T10:00:00\",\"items\":[]}") {
                success
                message
                value
              }
            }
            """;
        GraphQlResult = "Mutation sample loaded. Add valid items before executing.";
        StatusMessage = "Ready.";
    }

    private async Task ExecuteGraphQlAsync()
    {
        GraphQlResult = "Running...";
        StatusMessage = "Running GraphQL query...";

        try
        {
            GraphQlResult = await _graphQlPosService.ExecuteAsync(GraphQlQuery);
            StatusMessage = "GraphQL query executed.";
        }
        catch (Exception ex)
        {
            GraphQlResult = JsonSerializer.Serialize(
                new { errors = new[] { new { message = ex.GetBaseException().Message } } },
                new JsonSerializerOptions { WriteIndented = true });
            StatusMessage = $"GraphQL query failed: {ex.GetBaseException().Message}";
        }
    }
}
