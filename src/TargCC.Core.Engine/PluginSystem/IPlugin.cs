namespace TargCC.Core.Engine.PluginSystem;

/// <summary>
/// Base interface for all TargCC plugins.
/// Plugins extend the core functionality of TargCC through a dynamic loading system.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Gets the unique name of the plugin.
    /// Must be unique across all loaded plugins.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the version of the plugin in SemVer format (e.g., "1.0.0").
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the description of what this plugin does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the author or organization that created this plugin.
    /// </summary>
    string Author { get; }

    /// <summary>
    /// Gets the list of plugin names this plugin depends on.
    /// Dependencies will be loaded before this plugin.
    /// </summary>
    IReadOnlyList<string> Dependencies { get; }

    /// <summary>
    /// Initializes the plugin with the service provider.
    /// This is called once when the plugin is loaded.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A task representing the initialization operation.</returns>
    Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Shuts down the plugin gracefully.
    /// Called when the application is shutting down or the plugin is being unloaded.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A task representing the shutdown operation.</returns>
    Task ShutdownAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the plugin can be loaded in the current environment.
    /// Returns null if validation succeeds, otherwise returns an error message.
    /// </summary>
    /// <returns>Null if valid, error message if invalid.</returns>
    string? Validate();
}