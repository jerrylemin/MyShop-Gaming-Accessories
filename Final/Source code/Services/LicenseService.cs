using ProjectTest.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ProjectTest.Services;

public class LicenseService
{
    private const string LicenseKey = "LicenseState";
    private static readonly TimeSpan TrialLength = TimeSpan.FromDays(15);

    public async Task<LicenseState> GetStateAsync()
    {
        var state = LoadState();
        if (state is null)
        {
            state = new LicenseState { TrialStartedUtc = DateTime.UtcNow };
            SaveState(state);
        }

        state.IsTrialExpired = !state.IsActivated && DateTime.UtcNow - state.TrialStartedUtc > TrialLength;
        state.TrialDaysRemaining = Math.Max(0, (int)Math.Ceiling((TrialLength - (DateTime.UtcNow - state.TrialStartedUtc)).TotalDays));
        return await Task.FromResult(state);
    }

    public async Task<OperationResult> ActivateAsync(string activationCode)
    {
        if (!IsValidActivationCode(activationCode))
        {
            return await Task.FromResult(OperationResult.Fail("Activation code is invalid."));
        }

        var state = await GetStateAsync();
        state.IsActivated = true;
        state.ActivatedUtc = DateTime.UtcNow;
        state.ProtectedActivationHash = Hash(activationCode.Trim());
        SaveState(state);
        return OperationResult.Ok("License activated.");
    }

    public static bool IsValidActivationCode(string activationCode)
    {
        if (string.IsNullOrWhiteSpace(activationCode))
        {
            return false;
        }

        var normalized = activationCode.Trim().ToUpperInvariant();
        return normalized.StartsWith("MYSHOP-", StringComparison.Ordinal) && normalized.Length >= 16;
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

public class LicenseState
{
    public DateTime TrialStartedUtc { get; set; }

    public bool IsActivated { get; set; }

    public bool IsTrialExpired { get; set; }

    public int TrialDaysRemaining { get; set; }

    public DateTime? ActivatedUtc { get; set; }

    public string ProtectedActivationHash { get; set; } = string.Empty;

    public bool CanUseFullApp => IsActivated || !IsTrialExpired;
}
