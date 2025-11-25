// <copyright file="AnalyzeQualityCommand.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using System.CommandLine;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Rendering;
using TargCC.CLI.Models.Analysis;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Analysis;

namespace TargCC.CLI.Commands.Analyze;

/// <summary>
/// Command to analyze schema quality.
/// </summary>
public class AnalyzeQualityCommand : Command
{
    private readonly IAnalysisService analysisService;
    private readonly IOutputService outputService;
    private readonly ILogger<AnalyzeQualityCommand> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeQualityCommand"/> class.
    /// </summary>
    /// <param name="analysisService">Analysis service.</param>
    /// <param name="outputService">Output service.</param>
    /// <param name="loggerFactory">Logger factory.</param>
    public AnalyzeQualityCommand(
        IAnalysisService analysisService,
        IOutputService outputService,
        ILoggerFactory loggerFactory)
        : base("quality", "Analyze schema quality")
    {
        this.analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
        this.outputService = outputService ?? throw new ArgumentNullException(nameof(outputService));
        ArgumentNullException.ThrowIfNull(loggerFactory);
        this.logger = loggerFactory.CreateLogger<AnalyzeQualityCommand>();

        this.SetHandler(this.HandleAsync);
    }

    private async Task<int> HandleAsync()
    {
        try
        {
            this.logger.LogInformation("Starting quality analysis");

            QualityAnalysisResult result = null!;
            await this.outputService.SpinnerAsync(
                "Analyzing quality...",
                async () =>
                {
                    result = await this.analysisService.AnalyzeQualityAsync();
                });

            // Display results
            this.DisplayResults(result);

            this.outputService.Success("Quality analysis completed");
            return 0;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error analyzing quality");
            this.outputService.Error($"Error: {ex.Message}");
            return 1;
        }
    }

    private void DisplayResults(QualityAnalysisResult result)
    {
        var panel = new Panel(this.BuildResultsMarkup(result))
        {
            Header = new PanelHeader("[bold cyan]📊 Schema Quality Report[/]"),
            Border = BoxBorder.Rounded,
        };

        AnsiConsole.Write(panel);
    }

    private IRenderable BuildResultsMarkup(QualityAnalysisResult result)
    {
        var tree = new Tree("[bold]Quality Metrics[/]");

        // Naming conventions
        var namingNode = tree.AddNode(
            $"[cyan]Naming Conventions:[/] {result.NamingConventions.Percentage}% " +
            $"({result.NamingConventions.Passed}/{result.NamingConventions.Total} tables)");

        var namingIssues = result.Issues.Where(i => i.Category == "Naming Conventions").ToList();
        if (namingIssues.Any())
        {
            var issuesNode = namingNode.AddNode($"[yellow]⚠️  Issues:[/]");
            foreach (var issue in namingIssues)
            {
                var issueNode = issuesNode.AddNode($"[dim]{issue.Description}[/]");
                issueNode.AddNode($"[dim]→ {issue.Recommendation}[/]");
            }
        }

        // Relationships
        var relNode = tree.AddNode(
            $"[cyan]Relationships:[/] {result.Relationships.Percentage}% " +
            $"({result.Relationships.Passed}/{result.Relationships.Total} detected)");

        var relIssues = result.Issues.Where(i => i.Category == "Relationships").ToList();
        if (relIssues.Any())
        {
            var issuesNode = relNode.AddNode($"[yellow]⚠️  Missing:[/]");
            foreach (var issue in relIssues)
            {
                var issueNode = issuesNode.AddNode($"[dim]{issue.Description}[/]");
                issueNode.AddNode($"[dim]→ {issue.Recommendation}[/]");
            }
        }

        // Index coverage
        var indexNode = tree.AddNode(
            $"[cyan]Index Coverage:[/] {result.IndexCoverage.Percentage}% " +
            $"({result.IndexCoverage.Passed}/{result.IndexCoverage.Total} recommended)");

        var indexIssues = result.Issues.Where(i => i.Category == "Index Coverage").ToList();
        if (indexIssues.Any())
        {
            var issuesNode = indexNode.AddNode($"[yellow]⚠️  Recommended:[/]");
            foreach (var issue in indexIssues)
            {
                var issueNode = issuesNode.AddNode($"[dim]{issue.Description}[/]");
                issueNode.AddNode($"[dim]→ {issue.Recommendation}[/]");
            }
        }

        // Overall score
        var gradeColor = result.OverallScore >= 90 ? "green" : result.OverallScore >= 80 ? "yellow" : "red";
        tree.AddNode($"[bold {gradeColor}]Overall Score: {result.OverallGrade} ({result.OverallScore}/100)[/]");

        return tree;
    }
}
