namespace ProjectTest.Tests;

public class AutomatedUiSmokeTests
{
    private static readonly string RepoRoot = FindRepoRoot();

    [Fact]
    public void MainWindow_HasNavigationTargets()
    {
        var xaml = File.ReadAllText(Path.Combine(RepoRoot, "Views", "MainWindow.xaml"));

        Assert.Contains("Dashboard", xaml);
        Assert.Contains("Products", xaml);
        Assert.Contains("Orders", xaml);
        Assert.Contains("Reports", xaml);
        Assert.Contains("Settings", xaml);
    }

    [Fact]
    public void ProductAndOrderPages_HaveResponsiveWorkspaceHooks()
    {
        var products = File.ReadAllText(Path.Combine(RepoRoot, "Views", "Pages", "ProductsPage.xaml"));
        var orders = File.ReadAllText(Path.Combine(RepoRoot, "Views", "Pages", "OrdersPage.xaml"));

        Assert.Contains("ProductsWorkspaceGrid", products);
        Assert.Contains("OrdersWorkspaceGrid", orders);
    }

    [Fact]
    public void OrderPage_HasRoleAndSalesWorkflowControls()
    {
        var xaml = File.ReadAllText(Path.Combine(RepoRoot, "Views", "Pages", "OrdersPage.xaml"));

        Assert.Contains("SelectedCustomer", xaml);
        Assert.Contains("SelectedPromotion", xaml);
        Assert.Contains("PrintCommand", xaml);
        Assert.Contains("AutoSaveStatus", xaml);
    }

    private static string FindRepoRoot()
    {
        var directory = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(directory))
        {
            if (File.Exists(Path.Combine(directory, "ProjectTest.csproj")))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate ProjectTest.csproj.");
    }
}
