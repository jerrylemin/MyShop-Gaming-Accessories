namespace ProjectTest.Models;

public class AppSettings
{
    public int ItemsPerPage { get; set; } = 10;

    public string LastOpenedScreen { get; set; } = "Dashboard";

    public string LlmApiKey { get; set; } = string.Empty;

    public string LlmEndpoint { get; set; } = string.Empty;
}
