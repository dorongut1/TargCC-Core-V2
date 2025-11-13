using Microsoft.Extensions.Logging;
using TargCC.Core.Engine.PluginSystem;

namespace TargCC.Core.Engine.Examples;

/// <summary>
/// Example plugin demonstrating basic plugin implementation.
/// This can be used as a template for creating new plugins.
/// </summary>
public sealed class ExamplePlugin : IPlugin
{
    private ILogger? _logger;
    private bool _isInitialized;

    /// <inheritdoc/>
    public string Name => "ExamplePlugin";

    /// <inheritdoc/>
    public string Version => "1.0.0";

    /// <inheritdoc/>
    public string Description => "An example plugin demonstrating the plugin system.";

    /// <inheritdoc/>
    public string Author => "TargCC Core Team";

    /// <inheritdoc/>
    public IReadOnlyList<string> Dependencies => Array.Empty<string>();

    /// <inheritdoc/>
    public Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
            throw new InvalidOperationException("Plugin already initialized.");

        // Get services from DI container
        _logger = serviceProvider.GetService(typeof(ILogger<ExamplePlugin>)) as ILogger<ExamplePlugin>;
        
        _logger?.LogInformation("ExamplePlugin initialized successfully");
        _isInitialized = true;

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("ExamplePlugin shutting down");
        _isInitialized = false;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public string? Validate()
    {
        // Example validation: check if running on Windows
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
        {
            return "This plugin requires Windows or Linux operating system.";
        }

        return null; // Validation passed
    }

    /// <summary>
    /// Example method that can be called after initialization.
    /// </summary>
    public void DoSomething()
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Plugin not initialized.");

        _logger?.LogInformation("ExamplePlugin doing something...");
    }
}