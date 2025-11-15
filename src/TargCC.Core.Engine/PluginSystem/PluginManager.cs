using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TargCC.Core.Engine.PluginSystem;

/// <summary>
/// Manages the lifecycle of all loaded plugins including initialization, dependencies, and shutdown.
/// </summary>
public sealed class PluginManager : IDisposable
{
    private readonly PluginLoader _loader;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PluginManager> _logger;
    private readonly Dictionary<string, PluginMetadata> _plugins = new ();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginManager"/> class.
    /// </summary>
    /// <param name="loader">Plugin loader for loading assemblies.</param>
    /// <param name="serviceProvider">Service provider for dependency injection.</param>
    /// <param name="logger">Logger for diagnostic information.</param>
    public PluginManager(
        PluginLoader loader,
        IServiceProvider serviceProvider,
        ILogger<PluginManager> logger)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all loaded plugins.
    /// </summary>
    public IReadOnlyDictionary<string, PluginMetadata> Plugins => _plugins;

    /// <summary>
    /// Loads and initializes all plugins from the specified directory.
    /// </summary>
    /// <param name="pluginDirectory">Directory containing plugin assemblies.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of successfully loaded plugins.</returns>
    public async Task<int> LoadAndInitializePluginsAsync(
        string pluginDirectory, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Loading plugins from directory: {Directory}", pluginDirectory);

        // Load all plugins
        var loadedPlugins = _loader.LoadAllPlugins(pluginDirectory, useIsolation: true);

        // Create metadata for each plugin
        foreach (var (assemblyPath, plugin) in loadedPlugins)
        {
            var metadata = new PluginMetadata
            {
                Plugin = plugin,
                AssemblyPath = assemblyPath,
                IsInitialized = false,
                IsEnabled = true
            };

            // Validate plugin
            var validationError = plugin.Validate();
            if (validationError != null)
            {
                _logger.LogError("Plugin validation failed: {Name} - {Error}", plugin.Name, validationError);
                metadata.IsEnabled = false;
                metadata.ErrorMessage = validationError;
            }

            _plugins[plugin.Name] = metadata;
        }

        // Initialize plugins in dependency order
        var initCount = await InitializePluginsAsync(cancellationToken);

        _logger.LogInformation("Loaded and initialized {Count} plugins", initCount);
        return initCount;
    }

    /// <summary>
    /// Initializes all enabled plugins, respecting dependency order.
    /// </summary>
    private async Task<int> InitializePluginsAsync(CancellationToken cancellationToken)
    {
        var initializedCount = 0;
        var toInitialize = _plugins.Values
            .Where(m => m.IsEnabled && !m.IsInitialized)
            .ToList();

        // Sort by dependencies using topological sort
        var sorted = TopologicalSort(toInitialize);

        foreach (var metadata in sorted)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                _logger.LogDebug("Initializing plugin: {Name}", metadata.Plugin.Name);

                await metadata.Plugin.InitializeAsync(_serviceProvider, cancellationToken);

                metadata.IsInitialized = true;
                initializedCount++;

                _logger.LogInformation(
                    "Plugin initialized: {Name} v{Version}",
                    metadata.Plugin.Name,
                    metadata.Plugin.Version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize plugin: {Name}", metadata.Plugin.Name);
                metadata.IsEnabled = false;
                metadata.ErrorMessage = ex.Message;
            }
        }

        return initializedCount;
    }

    /// <summary>
    /// Sorts plugins by their dependencies using topological sort.
    /// Plugins with no dependencies come first.
    /// </summary>
    private List<PluginMetadata> TopologicalSort(List<PluginMetadata> plugins)
    {
        var sorted = new List<PluginMetadata>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>();

        void Visit(PluginMetadata metadata)
        {
            var name = metadata.Plugin.Name;

            if (visited.Contains(name))
                return;

            if (visiting.Contains(name))
            {
                _logger.LogWarning("Circular dependency detected for plugin: {Name}", name);
                return;
            }

            visiting.Add(name);

            // Visit dependencies first
            foreach (var depName in metadata.Plugin.Dependencies)
            {
                if (_plugins.TryGetValue(depName, out var depMetadata))
                {
                    Visit(depMetadata);
                }
                else
                {
                    _logger.LogWarning("Dependency not found: {Dependency} for plugin {Plugin}",
                        depName, name);
                }
            }

            visiting.Remove(name);
            visited.Add(name);
            sorted.Add(metadata);
        }

        foreach (var metadata in plugins)
        {
            Visit(metadata);
        }

        return sorted;
    }

    /// <summary>
    /// Gets a plugin by name.
    /// </summary>
    /// <param name="name">Plugin name.</param>
    /// <returns>Plugin metadata, or null if not found.</returns>
    public PluginMetadata? GetPlugin(string name)
    {
        return _plugins.TryGetValue(name, out var metadata) ? metadata : null;
    }

    /// <summary>
    /// Gets a plugin of a specific type.
    /// </summary>
    /// <typeparam name="T">Plugin type.</typeparam>
    /// <returns>Plugin instance, or null if not found.</returns>
    public T? GetPlugin<T>() where T : class, IPlugin
    {
        return _plugins.Values
            .Select(m => m.Plugin)
            .OfType<T>()
            .FirstOrDefault();
    }

    /// <summary>
    /// Enables a disabled plugin and initializes it.
    /// </summary>
    public async Task<bool> EnablePluginAsync(string name, CancellationToken cancellationToken = default)
    {
        if (!_plugins.TryGetValue(name, out var metadata))
        {
            _logger.LogWarning("Plugin not found: {Name}", name);
            return false;
        }

        if (metadata.IsEnabled)
        {
            _logger.LogInformation("Plugin already enabled: {Name}", name);
            return true;
        }

        metadata.IsEnabled = true;
        metadata.ErrorMessage = null;

        try
        {
            await metadata.Plugin.InitializeAsync(_serviceProvider, cancellationToken);
            metadata.IsInitialized = true;
            _logger.LogInformation("Plugin enabled: {Name}", name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enable plugin: {Name}", name);
            metadata.IsEnabled = false;
            metadata.ErrorMessage = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Disables a plugin and shuts it down.
    /// </summary>
    public async Task<bool> DisablePluginAsync(string name, CancellationToken cancellationToken = default)
    {
        if (!_plugins.TryGetValue(name, out var metadata))
        {
            _logger.LogWarning("Plugin not found: {Name}", name);
            return false;
        }

        if (!metadata.IsEnabled)
        {
            _logger.LogInformation("Plugin already disabled: {Name}", name);
            return true;
        }

        try
        {
            await metadata.Plugin.ShutdownAsync(cancellationToken);
            metadata.IsEnabled = false;
            metadata.IsInitialized = false;
            _logger.LogInformation("Plugin disabled: {Name}", name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling plugin: {Name}", name);
            return false;
        }
    }

    /// <summary>
    /// Shuts down all plugins gracefully.
    /// </summary>
    public async Task ShutdownAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Shutting down all plugins");

        // Shutdown in reverse order
        var pluginsToShutdown = _plugins.Values
            .Where(m => m.IsInitialized)
            .Reverse()
            .ToList();

        foreach (var metadata in pluginsToShutdown)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                _logger.LogDebug("Shutting down plugin: {Name}", metadata.Plugin.Name);
                await metadata.Plugin.ShutdownAsync(cancellationToken);
                metadata.IsInitialized = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shutting down plugin: {Name}", metadata.Plugin.Name);
            }
        }

        _loader.UnloadAll();
        _plugins.Clear();

        _logger.LogInformation("All plugins shut down");
    }

    /// <summary>
    /// Disposes the plugin manager and shuts down all plugins.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        ShutdownAllAsync().GetAwaiter().GetResult();
        _disposed = true;
    }
}
