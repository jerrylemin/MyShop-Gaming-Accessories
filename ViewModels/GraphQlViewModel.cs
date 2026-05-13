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
        LoadSampleGraphQl();
    }

    public AsyncRelayCommand ExecuteGraphQlCommand { get; }

    public RelayCommand LoadSampleGraphQlCommand { get; }

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
        GraphQlResult = "Sample query loaded. Click Execute GraphQL to run it.";
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
                new { errors = new[] { new { message = ex.Message } } },
                new JsonSerializerOptions { WriteIndented = true });
            StatusMessage = $"GraphQL query failed: {ex.Message}";
        }
    }
}
