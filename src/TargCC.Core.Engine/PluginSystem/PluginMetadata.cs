namespace TargCC.Core.Engine.PluginSystem;

/// <summary>
/// Metadata information about a loaded plugin.
/// </summary>
public sealed class PluginMetadata
{
    /// <summary>
    /// Gets the plugin instance.
    /// </summary>
    public required IPlugin Plugin { get; init; }

    /// <summary>
    /// Gets the assembly path from which the plugin was loaded.
    /// </summary>
    public required string AssemblyPath { get; init; }

    /// <summary>
    /// Gets the timestamp when the plugin was loaded.
    /// </summary>
    public DateTime LoadedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a value indicating whether the plugin is currently initialized.
    /// </summary>
    public bool IsInitialized { get; set; }

    /// <summary>
    /// Gets a value indicating whether the plugin is currently enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets any error message if the plugin failed to load or initialize.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets the full type name of the plugin.
    /// </summary>
    public string TypeName => Plugin.GetType().FullName ?? Plugin.GetType().Name;

    /// <summary>
    /// Returns a string representation of this plugin metadata.
    /// </summary>
    public override string ToString()
    {
        var status = IsInitialized ? "Initialized" : "Not Initialized";
        if (!IsEnabled)
            status = "Disabled";
        if (ErrorMessage != null)
            status = $"Error: {ErrorMessage}";

        return $"{Plugin.Name} v{Plugin.Version} ({status})";
    }
}