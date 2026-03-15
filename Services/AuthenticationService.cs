using ProjectTest.Models;
using System.Text.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;

namespace ProjectTest.Services;

public class AuthenticationService
{
    private const string CredentialsKey = "SavedCredentials";
    private const string BootstrapUsername = "admin";
    private const string BootstrapPassword = "MyShop123!";
    private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

    public string DefaultUsername => BootstrapUsername;

    public string DefaultPassword => BootstrapPassword;

    public Task<bool> HasSavedCredentialsAsync()
    {
        return Task.FromResult(_localSettings.Values.ContainsKey(CredentialsKey));
    }

    public async Task<bool> ValidateAsync(string username, string password)
    {
        if (await HasSavedCredentialsAsync())
        {
            var saved = await GetSavedCredentialsAsync();
            if (saved is null)
            {
                return false;
            }

            var storedUsername = await UnprotectAsync(saved.ProtectedUsername);
            var storedPassword = await UnprotectAsync(saved.ProtectedPassword);
            return string.Equals(username, storedUsername, StringComparison.Ordinal) &&
                   string.Equals(password, storedPassword, StringComparison.Ordinal);
        }

        return string.Equals(username, BootstrapUsername, StringComparison.Ordinal) &&
               string.Equals(password, BootstrapPassword, StringComparison.Ordinal);
    }

    public async Task SaveCredentialsAsync(string username, string password)
    {
        var credentials = new LoginCredentials
        {
            ProtectedUsername = await ProtectAsync(username),
            ProtectedPassword = await ProtectAsync(password)
        };

        _localSettings.Values[CredentialsKey] = JsonSerializer.Serialize(credentials);
    }

    public Task ClearCredentialsAsync()
    {
        _localSettings.Values.Remove(CredentialsKey);
        return Task.CompletedTask;
    }

    private async Task<LoginCredentials?> GetSavedCredentialsAsync()
    {
        if (!_localSettings.Values.TryGetValue(CredentialsKey, out var rawValue) || rawValue is not string json)
        {
            return null;
        }

        return await Task.FromResult(JsonSerializer.Deserialize<LoginCredentials>(json));
    }

    private static async Task<string> ProtectAsync(string input)
    {
        var provider = new DataProtectionProvider("LOCAL=user");
        var buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
        var protectedBuffer = await provider.ProtectAsync(buffer);
        return CryptographicBuffer.EncodeToBase64String(protectedBuffer);
    }

    private static async Task<string> UnprotectAsync(string protectedInput)
    {
        var provider = new DataProtectionProvider();
        var buffer = CryptographicBuffer.DecodeFromBase64String(protectedInput);
        var unprotectedBuffer = await provider.UnprotectAsync(buffer);
        return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, unprotectedBuffer);
    }
}
