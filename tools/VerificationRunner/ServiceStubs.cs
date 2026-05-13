using ProjectTest.Models;

namespace ProjectTest.Services;

public sealed class MlInsightService
{
    public Task<List<MlInsight>> GetInsightsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<MlInsight>());
    }
}

public sealed class LlmAssistantService
{
    public Task<AssistantResult> AnalyzeReportsAsync(ReportsSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AssistantResult
        {
            IsConfigured = false,
            Summary = "LLM assistant is not configured in VerificationRunner."
        });
    }
}
