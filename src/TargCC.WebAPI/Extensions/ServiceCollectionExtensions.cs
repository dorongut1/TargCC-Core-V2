// <copyright file="ServiceCollectionExtensions.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

using TargCC.AI.Services;
using TargCC.AI.Configuration;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Generation;
using TargCC.CLI.Services.Analysis;
using TargCC.CLI.Configuration;
using TargCC.Core.Analyzers;
using TargCC.Core.Services;
using TargCC.Core.Services.AI;
using TargCC.Core.Generators.Project;

namespace TargCC.WebAPI.Extensions;

/// <summary>
/// Extension methods for service collection configuration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds TargCC core services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddTargCCServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // HttpClient for AI services
        services.AddHttpClient();
        
        // CLI Configuration - create basic config service
        services.AddSingleton<IConfigurationService>(sp => 
        {
            var logger = sp.GetRequiredService<ILogger<ConfigurationService>>();
            return new ConfigurationService(logger);
        });
        
        // CLI Core Services
        services.AddSingleton<IOutputService, OutputService>();
        services.AddSingleton<IGenerationService, GenerationService>();
        services.AddSingleton<IErrorSuggestionService, ErrorSuggestionService>();
        services.AddSingleton<IAnalysisService, AnalysisService>();
        services.AddSingleton<CLI.Documentation.IDocumentationGenerator, CLI.Documentation.DocumentationGenerator>();
        services.AddSingleton<IProjectGenerationService, ProjectGenerationService>();

        // Generation Tracker (without schema change detection for now)
        services.AddSingleton<IGenerationTracker, GenerationTracker>();

        // Project Generators
        services.AddSingleton<ISolutionGenerator, SolutionGenerator>();
        services.AddSingleton<IProjectStructureGenerator, ProjectStructureGenerator>();
        services.AddSingleton<IProjectFileGenerator, ProjectFileGenerator>();
        services.AddSingleton<IProgramCsGenerator, ProgramCsGenerator>();
        services.AddSingleton<IAppSettingsGenerator, AppSettingsGenerator>();
        services.AddSingleton<IDependencyInjectionGenerator, DependencyInjectionGenerator>();

        // AI Services
        var aiConfig = new AIConfiguration();
        configuration.GetSection("AI").Bind(aiConfig);
        services.AddSingleton(aiConfig);
        services.AddSingleton<IAIService, ClaudeAIService>();
        services.AddSingleton<ISecurityScannerService, SecurityScannerService>();
        services.AddSingleton<ICodeQualityAnalyzer, CodeQualityAnalyzerService>();

        // AI Code Editor Service
        services.AddScoped<IAICodeEditorService>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
            var logger = sp.GetRequiredService<ILogger<AICodeEditorService>>();
            var apiKey = aiConfig.ApiKey ?? string.Empty;
            var model = aiConfig.Model;
            var maxTokens = aiConfig.MaxTokens;

            return new AICodeEditorService(httpClient, logger, apiKey, model, maxTokens);
        });

        return services;
    }
}
