using ProjectTest.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ProjectTest.Services;

public class LlmAssistantService
{
    private static readonly Uri DefaultEndpoint = new("https://api.openai.com/v1/chat/completions");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly SettingsService _settingsService;
    private readonly HttpClient _httpClient;

    public LlmAssistantService(SettingsService settingsService, HttpClient? httpClient = null)
    {
        _settingsService = settingsService;
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
    }

    public async Task<AssistantResult> AnalyzeReportsAsync(ReportsSnapshot snapshot)
    {
        var apiKey = ResolveApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new AssistantResult
            {
                IsConfigured = false,
                Summary = "LLM assistant is not configured. Set MYSHOP_LLM_API_KEY or save a key in Settings."
            };
        }

        try
        {
            using var request = BuildRequest(apiKey, snapshot);
            using var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return new AssistantResult
                {
                    IsConfigured = true,
                    Summary = $"LLM request failed: {(int)response.StatusCode} {response.ReasonPhrase}."
                };
            }

            return new AssistantResult
            {
                IsConfigured = true,
                Summary = ExtractSummary(responseBody)
            };
        }
        catch (TaskCanceledException)
        {
            return new AssistantResult
            {
                IsConfigured = true,
                Summary = "LLM request timed out. Check the endpoint or try again later."
            };
        }
        catch (Exception ex)
        {
            return new AssistantResult
            {
                IsConfigured = true,
                Summary = $"LLM request failed safely: {ex.Message}"
            };
        }
    }

    private string ResolveApiKey()
    {
        var apiKey = Environment.GetEnvironmentVariable("MYSHOP_LLM_API_KEY");
        return string.IsNullOrWhiteSpace(apiKey)
            ? _settingsService.CurrentSettings.LlmApiKey.Trim()
            : apiKey.Trim();
    }

    private HttpRequestMessage BuildRequest(string apiKey, ReportsSnapshot snapshot)
    {
        var endpoint = Uri.TryCreate(_settingsService.CurrentSettings.LlmEndpoint, UriKind.Absolute, out var configuredEndpoint)
            ? configuredEndpoint
            : DefaultEndpoint;
        var body = new
        {
            model = "gpt-4o-mini",
            temperature = 0.2,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "You summarize POS report snapshots for a Vietnamese gaming accessories store. Keep the answer concise and actionable."
                },
                new
                {
                    role = "user",
                    content = JsonSerializer.Serialize(BuildSnapshotSummary(snapshot), JsonOptions)
                }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return request;
    }

    private static object BuildSnapshotSummary(ReportsSnapshot snapshot)
    {
        return new
        {
            snapshot.RangeLabel,
            snapshot.TotalRevenue,
            snapshot.TotalProfit,
            TopProducts = snapshot.ProductSalesByRange.Take(5).Select(x => new
            {
                x.Label,
                x.Subtitle,
                x.Value,
                x.ValueLabel
            }),
            SalesCommissions = snapshot.SalesCommissions.Take(5).Select(x => new
            {
                x.Salesperson,
                x.Role,
                x.PaidOrders,
                x.Revenue,
                x.Commission
            }),
            MlInsights = snapshot.MlInsights.Take(5).Select(x => new
            {
                x.Title,
                x.Detail,
                x.Score
            })
        };
    }

    private static string ExtractSummary(string responseBody)
    {
        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;

        if (root.TryGetProperty("choices", out var choices) &&
            choices.ValueKind == JsonValueKind.Array &&
            choices.GetArrayLength() > 0 &&
            choices[0].TryGetProperty("message", out var message) &&
            message.TryGetProperty("content", out var content))
        {
            return content.GetString() ?? "LLM returned an empty summary.";
        }

        if (root.TryGetProperty("output_text", out var outputText))
        {
            return outputText.GetString() ?? "LLM returned an empty summary.";
        }

        return "LLM response was received, but no summary field was recognized.";
    }
}
