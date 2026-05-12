using ProjectTest.Models;
using System.Text.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;

namespace ProjectTest.Services;

public class AuthenticationService
{
    private const string CredentialsKey = "SavedCredentials";
    private const string BootstrapUsername = "admin";
    private const string BootstrapPassword = "MyShop123!";
    private readonly CurrentUserService _currentUserService;

    public AuthenticationService(CurrentUserService? currentUserService = null)
    {
        _currentUserService = currentUserService ?? new CurrentUserService();
    }

    public string DefaultUsername => BootstrapUsername;

    public string DefaultPassword => BootstrapPassword;

    public Task<bool> HasSavedCredentialsAsync()
    {
        return Task.FromResult(AppLocalStorage.ContainsKey(CredentialsKey));
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
            var savedValid = string.Equals(username, storedUsername, StringComparison.Ordinal) &&
                             string.Equals(password, storedPassword, StringComparison.Ordinal);
            if (savedValid)
            {
                _currentUserService.SetCurrentUser(BuildUser(username));
            }

            return savedValid;
        }

        var isValid = IsBootstrapCredential(username, password);
        if (isValid)
        {
            _currentUserService.SetCurrentUser(BuildUser(username));
        }

        return isValid;
    }

    public async Task SaveCredentialsAsync(string username, string password)
    {
        var credentials = new LoginCredentials
        {
            ProtectedUsername = await ProtectAsync(username),
            ProtectedPassword = await ProtectAsync(password)
        };

        AppLocalStorage.SetString(CredentialsKey, JsonSerializer.Serialize(credentials));
    }

    public Task ClearCredentialsAsync()
    {
        AppLocalStorage.Remove(CredentialsKey);
        return Task.CompletedTask;
    }

    private async Task<LoginCredentials?> GetSavedCredentialsAsync()
    {
        if (!AppLocalStorage.TryGetString(CredentialsKey, out var json) || string.IsNullOrWhiteSpace(json))
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

    private static bool IsBootstrapCredential(string username, string password)
    {
        if (!string.Equals(password, BootstrapPassword, StringComparison.Ordinal))
        {
            return false;
        }

        return string.Equals(username, BootstrapUsername, StringComparison.Ordinal) ||
               string.Equals(username, "moderator", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(username, "sale", StringComparison.OrdinalIgnoreCase);
    }

    private static AppUser BuildUser(string username)
    {
        var role = username.Equals("sale", StringComparison.OrdinalIgnoreCase)
            ? UserRole.Sale
            : username.Equals("moderator", StringComparison.OrdinalIgnoreCase)
                ? UserRole.Moderator
                : UserRole.Admin;

        return new AppUser
        {
            Id = role == UserRole.Admin ? 1 : role == UserRole.Moderator ? 2 : 3,
            Username = username,
            DisplayName = role.ToString(),
            Role = role
        };
    }
}
