// <copyright file="IntegrationTestBase.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using Xunit;

namespace TargCC.CLI.Tests.Integration;

/// <summary>
/// Base class for integration tests with common setup.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    private readonly string testDirectory;
    private readonly string configFilePath;
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTestBase"/> class.
    /// </summary>
    protected IntegrationTestBase()
    {
        // Create a unique test directory
        this.testDirectory = Path.Combine(Path.GetTempPath(), $"TargCC_Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(this.testDirectory);

        this.configFilePath = Path.Combine(this.testDirectory, "targcc.json");

        // Setup service provider
        var services = new ServiceCollection();
        this.ConfigureServices(services);
        this.ServiceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    protected ServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the test directory path.
    /// </summary>
    protected string TestDirectory => this.testDirectory;

    /// <summary>
    /// Gets the config file path.
    /// </summary>
    protected string ConfigFilePath => this.configFilePath;

    /// <summary>
    /// Configures services for testing.
    /// </summary>
    /// <param name="services">The service collection.</param>
    protected virtual void ConfigureServices(ServiceCollection services)
    {
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IOutputService, OutputService>();
    }

    /// <summary>
    /// Creates a test configuration file.
    /// </summary>
    /// <param name="connectionString">The database connection string.</param>
    /// <returns>The created configuration.</returns>
    protected CliConfiguration CreateTestConfig(string connectionString = "Server=localhost;Database=TestDB;Integrated Security=true;")
    {
        var config = new CliConfiguration
        {
            ConnectionString = connectionString,
            OutputDirectory = this.testDirectory,
            DefaultNamespace = "TestApp",
            IsInitialized = true,
            InitializationDate = DateTime.UtcNow,
        };

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(this.configFilePath, json);

        return config;
    }

    /// <summary>
    /// Loads configuration from file.
    /// </summary>
    /// <returns>The loaded configuration.</returns>
    protected CliConfiguration LoadConfig()
    {
        var json = File.ReadAllText(this.configFilePath);
        return JsonSerializer.Deserialize<CliConfiguration>(json) ?? new CliConfiguration();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    /// <param name="disposing">Whether disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                this.ServiceProvider.Dispose();

                // Cleanup test directory
                if (Directory.Exists(this.testDirectory))
                {
                    try
                    {
                        Directory.Delete(this.testDirectory, true);
                    }
                    catch
                    {
                        // Ignore cleanup errors in tests
                    }
                }
            }

            this.disposed = true;
        }
    }
}
