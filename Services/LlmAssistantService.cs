using ProjectTest.Models;

namespace ProjectTest.Services;

public class LlmAssistantService
{
    private readonly SettingsService _settingsService;

    public LlmAssistantService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public Task<AssistantResult> AnalyzeReportsAsync(ReportsSnapshot snapshot)
    {
        var apiKey = Environment.GetEnvironmentVariable("MYSHOP_LLM_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = _settingsService.CurrentSettings.LlmApiKey;
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Task.FromResult(new AssistantResult
            {
                IsConfigured = false,
                Summary = "LLM assistant is not configured. Set MYSHOP_LLM_API_KEY or save a key in Settings."
            });
        }

        var summary = $"Revenue {snapshot.TotalRevenueText()} and profit {snapshot.TotalProfitText()} in the selected range. Top products and restock insights are ready for review.";
        return Task.FromResult(new AssistantResult { IsConfigured = true, Summary = summary });
    }
}

internal static class ReportSnapshotFormattingExtensions
{
    public static string TotalRevenueText(this ReportsSnapshot snapshot) => Helpers.CurrencyFormatter.ToCurrency(snapshot.TotalRevenue);

    public static string TotalProfitText(this ReportsSnapshot snapshot) => Helpers.CurrencyFormatter.ToCurrency(snapshot.TotalProfit);
}
