using System;
using System.Collections.Generic;

namespace ProjectTest.Models;

public class PluginInfo
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string EntryPoint { get; set; } = string.Empty;

    public string FolderPath { get; set; } = string.Empty;

    public List<string> Capabilities { get; set; } = [];

    public string CapabilitiesText => Capabilities.Count == 0 ? "No declared capabilities" : string.Join(", ", Capabilities);

    public DateTime LastLoaded { get; set; } = DateTime.Now;

    public string LastLoadedText => LastLoaded.ToString("yyyy-MM-dd HH:mm:ss");
}
