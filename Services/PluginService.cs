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

    public async Task LoadAsync(AppServices services)
    {
        _plugins.Clear();
        var directory = Path.Combine(AppContext.BaseDirectory, "Plugins");
        Directory.CreateDirectory(directory);
        EnsureSamplePluginManifest(directory);

        foreach (var manifestPath in Directory.EnumerateFiles(directory, "plugin.json", SearchOption.AllDirectories))
        {
            try
            {
                var info = JsonSerializer.Deserialize<PluginInfo>(await File.ReadAllTextAsync(manifestPath)) ?? new PluginInfo();
                info.Status = "Loaded manifest";
                _plugins.Add(info);
            }
            catch (Exception ex)
            {
                _plugins.Add(new PluginInfo { Name = Path.GetFileName(Path.GetDirectoryName(manifestPath)) ?? "Plugin", Status = ex.Message });
            }
        }

        foreach (var assemblyPath in Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories))
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                foreach (var type in assembly.GetTypes().Where(x => typeof(IMyShopPlugin).IsAssignableFrom(x) && !x.IsAbstract))
                {
                    if (Activator.CreateInstance(type) is IMyShopPlugin plugin)
                    {
                        await plugin.InitializeAsync(services);
                        _plugins.Add(new PluginInfo { Id = plugin.Id, Name = plugin.Name, Version = plugin.Version, Status = "Loaded" });
                    }
                }
            }
            catch (Exception ex)
            {
                _plugins.Add(new PluginInfo { Name = Path.GetFileName(assemblyPath), Status = $"Error: {ex.Message}" });
            }
        }
    }

    private static void EnsureSamplePluginManifest(string pluginsDirectory)
    {
        var sampleDirectory = Path.Combine(pluginsDirectory, "SamplePlugin");
        Directory.CreateDirectory(sampleDirectory);
        var sampleManifest = Path.Combine(sampleDirectory, "plugin.json");
        if (File.Exists(sampleManifest))
        {
            return;
        }

        File.WriteAllText(sampleManifest, JsonSerializer.Serialize(new PluginInfo
        {
            Id = "sample",
            Name = "Sample MyShop Plugin",
            Version = "1.0.0",
            Status = "Manifest sample"
        }, new JsonSerializerOptions { WriteIndented = true }));
    }
}
