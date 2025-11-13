using System.Reflection;
using System.Runtime.Loader;

namespace TargCC.Core.Engine.PluginSystem;

/// <summary>
/// Custom assembly load context for loading plugins in isolation.
/// This allows plugins to have their own dependencies without conflicting with the main application.
/// </summary>
internal sealed class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private readonly string _pluginPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginLoadContext"/> class.
    /// </summary>
    /// <param name="pluginPath">The path to the plugin assembly.</param>
    /// <param name="isCollectible">Whether this load context is collectible (for unloading).</param>
    public PluginLoadContext(string pluginPath, bool isCollectible = false) 
        : base(Path.GetFileNameWithoutExtension(pluginPath), isCollectible)
    {
        _pluginPath = pluginPath;
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    /// <summary>
    /// Loads an assembly by name.
    /// </summary>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Try to resolve using the dependency resolver
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        // For shared assemblies (like TargCC.Core.Interfaces), 
        // use the default context to avoid duplication
        if (IsSharedAssembly(assemblyName))
        {
            return Default.LoadFromAssemblyName(assemblyName);
        }

        return null;
    }

    /// <summary>
    /// Loads an unmanaged library.
    /// </summary>
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Determines if an assembly should be shared across all load contexts.
    /// </summary>
    private static bool IsSharedAssembly(AssemblyName assemblyName)
    {
        var name = assemblyName.Name ?? string.Empty;
        
        // Share TargCC core assemblies
        if (name.StartsWith("TargCC.Core.", StringComparison.OrdinalIgnoreCase))
            return true;

        // Share Microsoft.Extensions assemblies
        if (name.StartsWith("Microsoft.Extensions.", StringComparison.OrdinalIgnoreCase))
            return true;

        // Share System assemblies
        if (name.StartsWith("System.", StringComparison.OrdinalIgnoreCase) || 
            name.Equals("System", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}