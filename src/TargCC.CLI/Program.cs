using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using TargCC.CLI.Commands;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Generation;
using TargCC.Core.Writers;

namespace TargCC.CLI;

/// <summary>
/// Main entry point for TargCC CLI application.
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Exit code.</returns>
    public static async Task<int> Main(string[] args)
    {
        // Setup Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".targcc", "logs", "targcc-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        try
        {
            // Build service provider
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            // Get root command and execute
            var rootCommand = serviceProvider.GetRequiredService<Commands.RootCommand>();
            return await rootCommand.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    /// <summary>
    /// Configure dependency injection services.
    /// </summary>
    /// <returns>Service collection.</returns>
    private static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();

        // Logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });

        // Configuration
        services.AddSingleton<IConfigurationService, ConfigurationService>();

        // Services
        services.AddSingleton<IOutputService, OutputService>();
        services.AddSingleton<IGenerationService, GenerationService>();
        services.AddSingleton<IErrorSuggestionService, ErrorSuggestionService>();
        services.AddSingleton<Services.Analysis.IAnalysisService, Services.Analysis.AnalysisService>();
        services.AddSingleton<Documentation.IDocumentationGenerator, Documentation.DocumentationGenerator>();
        services.AddSingleton<IProjectGenerationService, ProjectGenerationService>();

        // File Writer (required by ProjectGenerationService)
        services.AddSingleton<IFileWriter, FileWriter>();

        // Database Analyzer (connection string will be provided at runtime)
        services.AddSingleton<Core.Interfaces.IDatabaseAnalyzer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<Core.Analyzers.Database.DatabaseAnalyzer>>();
            return new Core.Analyzers.Database.DatabaseAnalyzer(string.Empty, logger);
        });

        // AI Service (disabled by default - no API key required)
        services.AddSingleton<AI.Services.IAIService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<AI.Services.ClaudeAIService>>();
            var httpClient = new System.Net.Http.HttpClient();
            var config = Microsoft.Extensions.Options.Options.Create(
                new AI.Configuration.AIConfiguration
                {
                    Enabled = false,  // Disabled by default - no API key needed
                    ApiKey = string.Empty
                });
            return new AI.Services.ClaudeAIService(httpClient, config, logger);
        });

        // Watch Mode Services
        services.AddSingleton<Core.Analyzers.ISchemaChangeDetector, Core.Analyzers.SchemaChangeDetector>();
        services.AddSingleton<Core.Services.IGenerationTracker, Core.Services.GenerationTracker>();

        // Project Generators
        services.AddSingleton<Core.Generators.Project.ISolutionGenerator, Core.Generators.Project.SolutionGenerator>();
        services.AddSingleton<Core.Generators.Project.IProjectStructureGenerator, Core.Generators.Project.ProjectStructureGenerator>();
        services.AddSingleton<Core.Generators.Project.IProjectFileGenerator, Core.Generators.Project.ProjectFileGenerator>();
        services.AddSingleton<Core.Generators.Project.IProgramCsGenerator, Core.Generators.Project.ProgramCsGenerator>();
        services.AddSingleton<Core.Generators.Project.IAppSettingsGenerator, Core.Generators.Project.AppSettingsGenerator>();
        services.AddSingleton<Core.Generators.Project.IDependencyInjectionGenerator, Core.Generators.Project.DependencyInjectionGenerator>();

        // Commands
        services.AddSingleton<Commands.RootCommand>();

        return services;
    }
}
