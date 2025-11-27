// <copyright file="SuggestCommand.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using System.CommandLine;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using TargCC.AI.Formatters;
using TargCC.AI.Models;
using TargCC.AI.Services;
using TargCC.CLI.Services;
using TargCC.Core.Interfaces;

namespace TargCC.CLI.Commands;

/// <summary>
/// Command to get AI-powered suggestions for database schema improvements.
/// </summary>
public class SuggestCommand : Command
{
    private readonly IAIService aiService;
    private readonly IDatabaseAnalyzer databaseAnalyzer;
    private readonly IOutputService outputService;
    private readonly ILogger<SuggestCommand> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SuggestCommand"/> class.
    /// </summary>
    /// <param name="aiService">AI service.</param>
    /// <param name="databaseAnalyzer">Database analyzer.</param>
    /// <param name="outputService">Output service.</param>
    /// <param name="loggerFactory">Logger factory.</param>
    public SuggestCommand(
        IAIService aiService,
        IDatabaseAnalyzer databaseAnalyzer,
        IOutputService outputService,
        ILoggerFactory loggerFactory)
        : base("suggest", "Get AI-powered suggestions for schema improvements")
    {
        this.aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        this.databaseAnalyzer = databaseAnalyzer ?? throw new ArgumentNullException(nameof(databaseAnalyzer));
        this.outputService = outputService ?? throw new ArgumentNullException(nameof(outputService));
        ArgumentNullException.ThrowIfNull(loggerFactory);
        this.logger = loggerFactory.CreateLogger<SuggestCommand>();

        // Table argument
        var tableArgument = new Argument<string>(
            name: "table",
            description: "Table name to get suggestions for");

        // Category option
        var categoryOption = new Option<string?>(
            aliases: ["--category", "-c"],
            description: "Filter suggestions by category (Performance, Security, DataIntegrity, Naming, BestPractices)")
        {
            IsRequired = false,
        };

        // Severity option
        var severityOption = new Option<string?>(
            aliases: ["--severity", "-s"],
            description: "Filter suggestions by severity (Critical, High, Medium, Low)")
        {
            IsRequired = false,
        };

        // Group by option
        var groupByOption = new Option<string>(
            aliases: ["--group-by", "-g"],
            description: "Group suggestions by (severity|category)",
            getDefaultValue: () => "severity");

        // No colors option
        var noColorsOption = new Option<bool>(
            aliases: ["--no-colors"],
            description: "Disable colored output",
            getDefaultValue: () => false);

        this.AddArgument(tableArgument);
        this.AddOption(categoryOption);
        this.AddOption(severityOption);
        this.AddOption(groupByOption);
        this.AddOption(noColorsOption);

        this.SetHandler(
            this.HandleAsync,
            tableArgument,
            categoryOption,
            severityOption,
            groupByOption,
            noColorsOption);
    }

    private async Task<int> HandleAsync(
        string tableName,
        string? category,
        string? severity,
        string groupBy,
        bool noColors)
    {
        try
        {
            this.logger.LogInformation("Getting suggestions for table: {TableName}", tableName);

            // Check if AI service is healthy
            var isHealthy = await this.aiService.IsHealthyAsync();
            if (!isHealthy)
            {
                this.outputService.Error("AI service is not available. Please check your configuration.");
                return 1;
            }

            // Get database schema
            var schema = await this.outputService.SpinnerAsync(
                "Loading database schema...",
                async () => await this.databaseAnalyzer.AnalyzeAsync());

            // Find the table
            var table = schema.Tables.FirstOrDefault(t =>
                t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));

            if (table == null)
            {
                this.outputService.Error($"Table '{tableName}' not found.");
                return 1;
            }

            // Get suggestions from AI
            SchemaAnalysisResult result = null!;
            await this.outputService.SpinnerAsync(
                "Analyzing schema and generating suggestions...",
                async () =>
                {
                    result = await this.aiService.GetTableSuggestionsAsync(table);
                });

            // Filter suggestions if requested
            result = this.FilterSuggestions(result, category, severity);

            // Display results
            this.DisplaySuggestions(result, groupBy, !noColors);

            this.outputService.Success($"Suggestions generated for '{tableName}'");
            return 0;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting suggestions");
            this.outputService.Error($"Error: {ex.Message}");
            return 1;
        }
    }

    private SchemaAnalysisResult FilterSuggestions(
        SchemaAnalysisResult result,
        string? category,
        string? severity)
    {
        var suggestions = result.Suggestions.AsEnumerable();

        // Filter by category if specified
        if (!string.IsNullOrWhiteSpace(category) &&
            Enum.TryParse<SuggestionCategory>(category, ignoreCase: true, out var categoryEnum))
        {
            suggestions = suggestions.Where(s => s.Category == categoryEnum);
        }

        // Filter by severity if specified
        if (!string.IsNullOrWhiteSpace(severity) &&
            Enum.TryParse<SuggestionSeverity>(severity, ignoreCase: true, out var severityEnum))
        {
            suggestions = suggestions.Where(s => s.Severity == severityEnum);
        }

        return new SchemaAnalysisResult
        {
            TableName = result.TableName,
            QualityScore = result.QualityScore,
            Summary = result.Summary,
            Suggestions = suggestions.ToList(),
        };
    }

    private void DisplaySuggestions(SchemaAnalysisResult result, string groupBy, bool useColors)
    {
        var formatter = new SuggestionFormatter();

        // Display formatted suggestions
        string formattedOutput;
        if (groupBy.Equals("category", StringComparison.OrdinalIgnoreCase))
        {
            formattedOutput = formatter.FormatByCategory(result, useColors);
        }
        else
        {
            formattedOutput = formatter.FormatSuggestions(result, useColors);
        }

        // Display in a panel
        var panel = new Panel(formattedOutput)
        {
            Header = new PanelHeader("[bold cyan]💡 AI Suggestions[/]"),
            Border = BoxBorder.Rounded,
        };

        AnsiConsole.Write(panel);

        // Display summary
        var summary = formatter.CreateSummary(result);
        AnsiConsole.MarkupLine($"[dim]{summary}[/]");
    }
}
