using ProjectTest.Models;
using System.Reflection;
using System.Text.Json;

namespace ProjectTest.Services;

public interface IMyShopPlugin
{
    string Id { get; }

    string Name { get; }

    string Version { get; }

    Task InitializeAsync(AppServices services);
}

public class PluginService
{
    private readonly List<PluginInfo> _plugins = [];

    public IReadOnlyList<PluginInfo> Plugins => _plugins;

    public string PluginsDirectory => Path.Combine(AppContext.BaseDirectory, "Plugins");

    public async Task LoadAsync(AppServices services)
    {
        _plugins.Clear();
        Directory.CreateDirectory(PluginsDirectory);
        await EnsureSamplePluginPackAsync();

        foreach (var manifestPath in Directory.EnumerateFiles(PluginsDirectory, "plugin.json", SearchOption.AllDirectories))
        {
            try
            {
                var info = JsonSerializer.Deserialize<PluginInfo>(
                    await File.ReadAllTextAsync(manifestPath),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PluginInfo();

                info.FolderPath = Path.GetDirectoryName(manifestPath) ?? PluginsDirectory;
                info.Type = string.IsNullOrWhiteSpace(info.Type) ? "Manifest" : info.Type;
                info.EntryPoint = string.IsNullOrWhiteSpace(info.EntryPoint) ? "plugin.json" : info.EntryPoint;
                info.LastLoaded = DateTime.Now;
                info.Status = "Manifest loaded";
                _plugins.Add(info);
            }
            catch (Exception ex)
            {
                _plugins.Add(new PluginInfo
                {
                    Name = Path.GetFileName(Path.GetDirectoryName(manifestPath)) ?? "Plugin",
                    Type = "Manifest",
                    Status = $"Manifest error: {ex.GetBaseException().Message}",
                    FolderPath = Path.GetDirectoryName(manifestPath) ?? PluginsDirectory,
                    LastLoaded = DateTime.Now
                });
            }
        }

        foreach (var assemblyPath in Directory.EnumerateFiles(PluginsDirectory, "*.dll", SearchOption.AllDirectories))
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                foreach (var type in assembly.GetTypes().Where(x => typeof(IMyShopPlugin).IsAssignableFrom(x) && !x.IsAbstract))
                {
                    if (Activator.CreateInstance(type) is IMyShopPlugin plugin)
                    {
                        await plugin.InitializeAsync(services);
                        _plugins.Add(new PluginInfo
                        {
                            Id = plugin.Id,
                            Name = plugin.Name,
                            Version = plugin.Version,
                            Description = "Runtime .dll plugin loaded and initialized.",
                            Status = "DLL plugin loaded",
                            Type = "Runtime DLL",
                            EntryPoint = type.FullName ?? type.Name,
                            FolderPath = Path.GetDirectoryName(assemblyPath) ?? PluginsDirectory,
                            Capabilities = ["Runtime initialize", "Access AppServices"],
                            LastLoaded = DateTime.Now
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _plugins.Add(new PluginInfo
                {
                    Name = Path.GetFileName(assemblyPath),
                    Type = "Runtime DLL",
                    Status = $"DLL error: {ex.GetBaseException().Message}",
                    FolderPath = Path.GetDirectoryName(assemblyPath) ?? PluginsDirectory,
                    LastLoaded = DateTime.Now
                });
            }
        }
    }

    public async Task<int> CreateSamplePluginPackAsync()
    {
        Directory.CreateDirectory(PluginsDirectory);

        var samples = new[]
        {
            new PluginInfo
            {
                Id = "low-stock-alert",
                Name = "Low Stock Alert Plugin",
                Version = "1.0.0",
                Description = "Example extension that declares an inventory alert capability for low-stock products.",
                Status = "Sample manifest",
                Type = "Manifest",
                EntryPoint = "plugin.json",
                Capabilities = ["Inventory audit", "Low stock notification", "Dashboard card"]
            },
            new PluginInfo
            {
                Id = "daily-sales-export",
                Name = "Daily Sales Export Plugin",
                Version = "1.0.0",
                Description = "Example extension that declares export actions for daily sales data.",
                Status = "Sample manifest",
                Type = "Manifest",
                EntryPoint = "plugin.json",
                Capabilities = ["CSV export", "Order summary", "End-of-day report"]
            },
            new PluginInfo
            {
                Id = "customer-loyalty-booster",
                Name = "Customer Loyalty Booster Plugin",
                Version = "1.0.0",
                Description = "Example extension that declares customer loyalty insights.",
                Status = "Sample manifest",
                Type = "Manifest",
                EntryPoint = "plugin.json",
                Capabilities = ["Customer segmentation", "Loyalty suggestions", "Retention insight"]
            }
        };

        var count = 0;
        foreach (var sample in samples)
        {
            var folder = Path.Combine(PluginsDirectory, sample.Id);
            Directory.CreateDirectory(folder);
            var manifest = Path.Combine(folder, "plugin.json");
            await File.WriteAllTextAsync(manifest, JsonSerializer.Serialize(sample, new JsonSerializerOptions { WriteIndented = true }));

            var readme = Path.Combine(folder, "README.md");
            await File.WriteAllTextAsync(readme,
$"""
# {sample.Name}

This folder demonstrates the dynamic plugin architecture.

A plugin can be discovered by manifest:
- plugin.json

A runtime plugin can be added later by placing a .dll here.
The .dll should implement ProjectTest.Services.IMyShopPlugin.

Declared capabilities:
{string.Join(Environment.NewLine, sample.Capabilities.Select(x => "- " + x))}
""");
            count++;
        }

        return count;
    }

    public string GetPluginArchitectureSummary()
    {
        return "Plugin discovery supports plugin.json manifests and runtime .dll plugins implementing IMyShopPlugin. Drop plugin folders under the Plugins directory and press Refresh.";
    }

    private async Task EnsureSamplePluginPackAsync()
    {
        if (Directory.EnumerateFiles(PluginsDirectory, "plugin.json", SearchOption.AllDirectories).Any())
        {
            return;
        }

        await CreateSamplePluginPackAsync();
    }
}
