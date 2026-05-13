using ProjectTest.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ProjectTest.Services;

public class LicenseService
{
    private const string LicenseKey = "LicenseState";
    public const string DemoOneMonthCode = "MYSHOP-1MONTH-2026";
    public const string DemoOneYearCode = "MYSHOP-1YEAR-2026";
    public const string DemoLifetimeCode = "MYSHOP-LIFETIME-2026";
    private static readonly TimeSpan TrialLength = TimeSpan.FromDays(15);

    public async Task<LicenseState> GetStateAsync()
    {
        var state = LoadState();
        if (state is null)
        {
            state = new LicenseState { TrialStartedUtc = DateTime.UtcNow };
            SaveState(state);
        }

        if (state.IsActivated && state.ExpiresUtc.HasValue && DateTime.UtcNow > state.ExpiresUtc.Value)
        {
            state.IsActivated = false;
        }

        state.IsTrialExpired = !state.CanUseActivatedPlan && DateTime.UtcNow - state.TrialStartedUtc > TrialLength;
        state.TrialDaysRemaining = Math.Max(0, (int)Math.Ceiling((TrialLength - (DateTime.UtcNow - state.TrialStartedUtc)).TotalDays));
        return await Task.FromResult(state);
    }

    public async Task<OperationResult> ActivateAsync(string activationCode)
    {
        var plan = GetPlan(activationCode);
        if (plan is null)
        {
            return await Task.FromResult(OperationResult.Fail("Activation code is invalid."));
        }

        var state = await GetStateAsync();
        var activatedUtc = DateTime.UtcNow;
        state.IsActivated = true;
        state.ActivatedUtc = activatedUtc;
        state.ExpiresUtc = plan.Duration is null ? null : activatedUtc.Add(plan.Duration.Value);
        state.PlanCode = plan.Code;
        state.PlanName = plan.Name;
        state.ProtectedActivationHash = Hash(activationCode.Trim());
        SaveState(state);
        return OperationResult.Ok($"License activated: {plan.Name}.");
    }

    public static bool IsValidActivationCode(string activationCode)
    {
        if (string.IsNullOrWhiteSpace(activationCode))
        {
            return false;
        }

        return GetPlan(activationCode) is not null;
    }

    private static LicensePlan? GetPlan(string activationCode)
    {
        if (string.IsNullOrWhiteSpace(activationCode))
        {
            return null;
        }

        return activationCode.Trim().ToUpperInvariant() switch
        {
            DemoOneMonthCode => new LicensePlan("1 Month", DemoOneMonthCode, TimeSpan.FromDays(30)),
            DemoOneYearCode => new LicensePlan("1 Year", DemoOneYearCode, TimeSpan.FromDays(365)),
            DemoLifetimeCode => new LicensePlan("Lifetime", DemoLifetimeCode, null),
            "MYSHOP-DEMO-2026" => new LicensePlan("Lifetime", "MYSHOP-DEMO-2026", null),
            _ => null
        };
    }

    private static LicenseState? LoadState()
    {
        if (!AppLocalStorage.TryGetString(LicenseKey, out var json) || string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<LicenseState>(json);
        }
        catch
        {
            return null;
        }
    }

    private static void SaveState(LicenseState state)
    {
        AppLocalStorage.SetString(LicenseKey, JsonSerializer.Serialize(state));
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}

internal sealed record LicensePlan(string Name, string Code, TimeSpan? Duration);

public class LicenseState
{
    public DateTime TrialStartedUtc { get; set; }

    public bool IsActivated { get; set; }

    public bool IsTrialExpired { get; set; }

    public int TrialDaysRemaining { get; set; }

    public DateTime? ActivatedUtc { get; set; }

    public DateTime? ExpiresUtc { get; set; }

    public string PlanName { get; set; } = string.Empty;

    public string PlanCode { get; set; } = string.Empty;

    public string ProtectedActivationHash { get; set; } = string.Empty;

    public bool CanUseActivatedPlan => IsActivated && (!ExpiresUtc.HasValue || DateTime.UtcNow <= ExpiresUtc.Value);

    public bool CanUseFullApp => CanUseActivatedPlan || !IsTrialExpired;

    public int ActivatedDaysRemaining => !ExpiresUtc.HasValue
        ? int.MaxValue
        : Math.Max(0, (int)Math.Ceiling((ExpiresUtc.Value - DateTime.UtcNow).TotalDays));
}
