using System.Reflection;
using Microsoft.Extensions.Logging;

namespace TargCC.Core.Engine.PluginSystem;

/// <summary>
/// Responsible for discovering and loading plugin assemblies from the file system.
/// Supports dynamic loading with dependency resolution and isolation.
/// </summary>
public sealed class PluginLoader
{
    private readonly ILogger<PluginLoader> _logger;
    private readonly Dictionary<string, PluginLoadContext> _loadContexts = new ();

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginLoader"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic information.</param>
    public PluginLoader(ILogger<PluginLoader> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Discovers all plugin assemblies in the specified directory.
    /// </summary>
    /// <param name="pluginDirectory">Directory to scan for plugin DLLs.</param>
    /// <param name="searchPattern">File pattern to match (default: "*.dll").</param>
    /// <returns>List of discovered plugin assembly paths.</returns>
    public IReadOnlyList<string> DiscoverPlugins(string pluginDirectory, string searchPattern = "*.dll")
    {
        if (string.IsNullOrWhiteSpace(pluginDirectory))
            throw new ArgumentException("Plugin directory cannot be empty.", nameof(pluginDirectory));

        if (!Directory.Exists(pluginDirectory))
        {
            _logger.LogWarning("Plugin directory does not exist: {Directory}", pluginDirectory);
            return Array.Empty<string>();
        }

        try
        {
            var assemblies = Directory.GetFiles(pluginDirectory, searchPattern, SearchOption.AllDirectories);
            _logger.LogInformation("Discovered {Count} potential plugin assemblies in {Directory}"
                , 
                assemblies.Length, pluginDirectory);
            return assemblies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering plugins in directory: {Directory}", pluginDirectory);
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Loads a plugin from the specified assembly path.
    /// </summary>
    /// <param name="assemblyPath">Path to the plugin assembly.</param>
    /// <param name="useIsolation">Whether to load the plugin in an isolated context.</param>
    /// <returns>Loaded plugin instance, or null if loading failed.</returns>
    public IPlugin? LoadPlugin(string assemblyPath, bool useIsolation = true)
    {
        if (string.IsNullOrWhiteSpace(assemblyPath))
            throw new ArgumentException("Assembly path cannot be empty.", nameof(assemblyPath));

        if (!File.Exists(assemblyPath))
        {
            _logger.LogError("Plugin assembly not found: {Path}", assemblyPath);
            return null;
        }

        try
        {
            _logger.LogDebug("Loading plugin from: {Path}", assemblyPath);

            Assembly assembly;
            
            if (useIsolation)
            {
                // Load in isolated context
                var loadContext = new PluginLoadContext(assemblyPath, isCollectible: true);
                _loadContexts[assemblyPath] = loadContext;
                assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
            }
            else
            {
                // Load in default context
                assembly = Assembly.LoadFrom(assemblyPath);
            }

            // Find types implementing IPlugin
            var pluginTypes = assembly.GetTypes()
                .Where(t => typeof(IPlugin).IsAssignableFrom(t) && 
                           !t.IsInterface && 
                           !t.IsAbstract)
                .ToList();

            if (pluginTypes.Count == 0)
            {
                _logger.LogWarning("No plugin types found in assembly: {Path}", assemblyPath);
                return null;
            }

            if (pluginTypes.Count > 1)
            {
                _logger.LogWarning("Multiple plugin types found in assembly: {Path}. Using first one.", assemblyPath);
            }

            // Create plugin instance
            var pluginType = pluginTypes[0];
            var plugin = Activator.CreateInstance(pluginType) as IPlugin;

            if (plugin == null)
            {
                _logger.LogError("Failed to create instance of plugin type: {Type}", pluginType.FullName);
                return null;
            }

            _logger.LogInformation(
                "Successfully loaded plugin: {Name} v{Version} from {Path}", 
                plugin.Name, plugin.Version, assemblyPath);

            return plugin;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading plugin from: {Path}", assemblyPath);
            return null;
        }
    }

    /// <summary>
    /// Loads all plugins from the specified directory.
    /// </summary>
    /// <param name="pluginDirectory">Directory containing plugin assemblies.</param>
    /// <param name="useIsolation">Whether to load plugins in isolated contexts.</param>
    /// <returns>Dictionary of assembly path to loaded plugin.</returns>
    public IReadOnlyDictionary<string, IPlugin> LoadAllPlugins(string pluginDirectory, bool useIsolation = true)
    {
        var assemblyPaths = DiscoverPlugins(pluginDirectory);
        var loadedPlugins = new Dictionary<string, IPlugin>();

        foreach (var assemblyPath in assemblyPaths)
        {
            var plugin = LoadPlugin(assemblyPath, useIsolation);
            if (plugin != null)
            {
                loadedPlugins[assemblyPath] = plugin;
            }
        }

        _logger.LogInformation(
            "Loaded {Count} plugins from directory: {Directory}",
            loadedPlugins.Count,
            pluginDirectory);

        return loadedPlugins;
    }

    /// <summary>
    /// Unloads a plugin and its load context.
    /// Only works for plugins loaded with isolation.
    /// </summary>
    /// <param name="assemblyPath">Path to the plugin assembly.</param>
    public void UnloadPlugin(string assemblyPath)
    {
        if (_loadContexts.TryGetValue(assemblyPath, out var loadContext))
        {
            _logger.LogInformation("Unloading plugin from: {Path}", assemblyPath);
            loadContext.Unload();
            _loadContexts.Remove(assemblyPath);
        }
    }

    /// <summary>
    /// Unloads all plugins.
    /// </summary>
    public void UnloadAll()
    {
        _logger.LogInformation("Unloading all plugins");
        
        foreach (var kvp in _loadContexts)
        {
            kvp.Value.Unload();
        }
        
        _loadContexts.Clear();
    }
}
