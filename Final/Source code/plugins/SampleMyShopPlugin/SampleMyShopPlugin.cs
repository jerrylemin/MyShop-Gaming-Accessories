using ProjectTest.Services;

namespace SampleMyShopPlugin;

public sealed class SampleMyShopPlugin : IMyShopPlugin
{
    public string Id => "sample-myshop-plugin";

    public string Name => "Sample MyShop Plugin";

    public string Version => "1.0.0";

    public Task InitializeAsync(AppServices services)
    {
        return Task.CompletedTask;
    }
}
