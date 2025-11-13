using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TargCC.Core.Configuration;
using TargCC.Core.Engine.PluginSystem;

namespace TargCC.Core.Engine.Examples;

/// <summary>
/// Complete example demonstrating TargCC Engine usage.
/// Shows how to:
/// 1. Setup dependency injection
/// 2. Load configuration
/// 3. Initialize plugin system
/// 4. Load and manage plugins.
/// </summary>
public static class UsageExample
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("TargCC Core Engine - Usage Example");
        Console.WriteLine("===================================\n");

        // Step 1: Setup Dependency Injection
        Console.WriteLine("1. Setting up services...");
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Add Engine services
        services.AddSingleton<ConfigurationManager>();
        services.AddSingleton<PluginLoader>();
        services.AddSingleton<PluginManager>();

        var serviceProvider = services.BuildServiceProvider();
        Console.WriteLine("   ✓ Services configured\n");

        // Step 2: Load Configuration
        Console.WriteLine("2. Loading configuration...");
        var configManager = serviceProvider.GetRequiredService<ConfigurationManager>();

        try
        {
            var config = configManager.LoadConfiguration("appsettings.json", optional: true);
            Console.WriteLine($"   ✓ Configuration loaded");
            Console.WriteLine($"   - Database: {config.Database.Provider}");
            Console.WriteLine($"   - Plugin Directory: {config.Plugins.PluginDirectory}");
            Console.WriteLine($"   - Encryption: {(config.Security.EnableEncryption ? "Enabled" : "Disabled")}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✗ Configuration load failed: {ex.Message}");
            Console.WriteLine("   Using default configuration\n");
        }

        // Step 3: Initialize Plugin System
        Console.WriteLine("3. Initializing plugin system...");
        var pluginManager = serviceProvider.GetRequiredService<PluginManager>();
        Console.WriteLine("   ✓ Plugin manager ready\n");

        // Step 4: Load Plugins
        Console.WriteLine("4. Loading plugins...");
        var config2 = configManager.Configuration;
        var pluginCount = await pluginManager.LoadAndInitializePluginsAsync(
            config2.Plugins.PluginDirectory);

        Console.WriteLine($"   ✓ Loaded {pluginCount} plugin(s)\n");

        // Step 5: List Loaded Plugins
        if (pluginManager.Plugins.Any())
        {
            Console.WriteLine("5. Loaded plugins:");
            foreach (var (name, metadata) in pluginManager.Plugins)
            {
                var status = metadata.IsInitialized ? "✓" : "✗";
                Console.WriteLine($"   {status} {metadata.Plugin.Name} v{metadata.Plugin.Version}");
                Console.WriteLine($"      {metadata.Plugin.Description}");
                Console.WriteLine($"      Author: {metadata.Plugin.Author}");

                if (metadata.Plugin.Dependencies.Any())
                {
                    Console.WriteLine($"      Dependencies: {string.Join(", ", metadata.Plugin.Dependencies)}");
                }

                if (!string.IsNullOrEmpty(metadata.ErrorMessage))
                {
                    Console.WriteLine($"      ⚠ Error: {metadata.ErrorMessage}");
                }

                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine("5. No plugins loaded");
            Console.WriteLine("   Tip: Place plugin DLLs in the 'plugins' directory\n");
        }

        // Step 6: Demonstrate Plugin Operations
        if (pluginManager.Plugins.Any())
        {
            Console.WriteLine("6. Plugin operations:");
            var firstPlugin = pluginManager.Plugins.First().Value;

            // Disable plugin
            Console.WriteLine($"   Disabling: {firstPlugin.Plugin.Name}");
            await pluginManager.DisablePluginAsync(firstPlugin.Plugin.Name);
            Console.WriteLine($"   ✓ Status: {(firstPlugin.IsEnabled ? "Enabled" : "Disabled")}");

            // Enable plugin
            Console.WriteLine($"   Enabling: {firstPlugin.Plugin.Name}");
            await pluginManager.EnablePluginAsync(firstPlugin.Plugin.Name);
            Console.WriteLine($"   ✓ Status: {(firstPlugin.IsEnabled ? "Enabled" : "Disabled")}\n");
        }

        // Step 7: Configuration Operations
        Console.WriteLine("7. Configuration operations:");

        // Get a value
        var dbProvider = configManager.GetValue("database:provider");
        Console.WriteLine($"   Database Provider: {dbProvider ?? "Not set"}");

        // Set a value
        configManager.SetValue("database:commandTimeoutSeconds", "60");
        Console.WriteLine($"   Updated Command Timeout: {configManager.Configuration.Database.CommandTimeoutSeconds}s");

        // Save configuration (optional)
        // configManager.SaveConfiguration("appsettings.backup.json", encryptSensitiveData: true);
        Console.WriteLine();

        // Step 8: Cleanup
        Console.WriteLine("8. Shutting down...");
        await pluginManager.ShutdownAllAsync();
        Console.WriteLine("   ✓ All plugins shut down");

        pluginManager.Dispose();
        Console.WriteLine("   ✓ Resources released\n");

        Console.WriteLine("===================================");
        Console.WriteLine("Example completed successfully!");
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
