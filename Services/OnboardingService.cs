namespace ProjectTest.Services;

public class OnboardingService
{
    private const string CompletedKey = "OnboardingCompleted";

    public bool IsCompleted => AppLocalStorage.TryGetString(CompletedKey, out var value) && value == "true";

    public void Complete()
    {
        AppLocalStorage.SetString(CompletedKey, "true");
    }
}
