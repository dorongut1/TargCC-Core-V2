// <copyright file="AnalyzeSecurityCommand.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using System.CommandLine;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Rendering;
using TargCC.AI.Models;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Analysis;


namespace TargCC.CLI.Commands.Analyze;

/// <summary>
/// Command to analyze security issues in schema.
/// </summary>
public class AnalyzeSecurityCommand : Command
{
    private readonly IAnalysisService analysisService;
    private readonly IOutputService outputService;
    private readonly ILogger<AnalyzeSecurityCommand> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeSecurityCommand"/> class.
    /// </summary>
    /// <param name="analysisService">Analysis service.</param>
    /// <param name="outputService">Output service.</param>
    /// <param name="loggerFactory">Logger factory.</param>
    public AnalyzeSecurityCommand(
        IAnalysisService analysisService,
        IOutputService outputService,
        ILoggerFactory loggerFactory)
        : base("security", "Analyze security issues in schema")
    {
        this.analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
        this.outputService = outputService ?? throw new ArgumentNullException(nameof(outputService));
        ArgumentNullException.ThrowIfNull(loggerFactory);
        this.logger = loggerFactory.CreateLogger<AnalyzeSecurityCommand>();

        this.SetHandler(this.HandleAsync);
    }

    private async Task<int> HandleAsync()
    {
        try
        {
            this.logger.LogInformation("Starting security analysis");

            AI.Models.SecurityAnalysisResult result = null!;
            await this.outputService.SpinnerAsync(
                "Analyzing security...",
                async () =>
                {
                    result = await this.analysisService.AnalyzeSecurityAsync();
                });

            // Display results
            this.DisplayResults(result);

            this.outputService.Success("Security analysis completed");
            return 0;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error analyzing security");
            this.outputService.Error($"Error: {ex.Message}");
            return 1;
        }
    }

    private void DisplayResults(AI.Models.SecurityAnalysisResult result)
    {
        var panel = new Panel(this.BuildResultsMarkup(result))
        {
            Header = new PanelHeader("[bold cyan]🔒 Security Analysis[/]"),
            Border = BoxBorder.Rounded,
        };

        AnsiConsole.Write(panel);
    }

    private IRenderable BuildResultsMarkup(AI.Models.SecurityAnalysisResult result)
    {
        var tree = new Tree($"[bold]Security Score: {result.OverallScore.Score}/100 (Grade: {result.OverallScore.Grade})[/]");

        // Add score summary
        var scoreColor = result.OverallScore.Grade switch
        {
            "A" => "green",
            "B" => "blue",
            "C" => "yellow",
            "D" => "orange",
            _ => "red",
        };
        tree.AddNode($"[{scoreColor}]{result.OverallScore.Summary}[/]");

        // Add vulnerabilities section
        if (result.Vulnerabilities.Any())
        {
            var vulnNode = tree.AddNode($"[red]⚠️  Vulnerabilities: {result.Vulnerabilities.Count}[/]");

            var criticalVulns = result.Vulnerabilities.Where(v => v.Severity == AI.Models.SecuritySeverity.Critical).ToList();
            var highVulns = result.Vulnerabilities.Where(v => v.Severity == AI.Models.SecuritySeverity.High).ToList();
            var mediumVulns = result.Vulnerabilities.Where(v => v.Severity == AI.Models.SecuritySeverity.Medium).ToList();
            var lowVulns = result.Vulnerabilities.Where(v => v.Severity == AI.Models.SecuritySeverity.Low).ToList();

            if (criticalVulns.Any())
            {
                var criticalNode = vulnNode.AddNode($"[red bold]🔴 Critical: {criticalVulns.Count}[/]");
                foreach (var vuln in criticalVulns)
                {
                    this.AddVulnerabilityNode(criticalNode, vuln);
                }
            }

            if (highVulns.Any())
            {
                var highNode = vulnNode.AddNode($"[red]High: {highVulns.Count}[/]");
                foreach (var vuln in highVulns)
                {
                    this.AddVulnerabilityNode(highNode, vuln);
                }
            }

            if (mediumVulns.Any())
            {
                var mediumNode = vulnNode.AddNode($"[yellow]Medium: {mediumVulns.Count}[/]");
                foreach (var vuln in mediumVulns)
                {
                    this.AddVulnerabilityNode(mediumNode, vuln);
                }
            }

            if (lowVulns.Any())
            {
                var lowNode = vulnNode.AddNode($"[dim]Low: {lowVulns.Count}[/]");
                foreach (var vuln in lowVulns)
                {
                    this.AddVulnerabilityNode(lowNode, vuln);
                }
            }
        }

        // Add prefix recommendations section
        if (result.PrefixRecommendations.Any())
        {
            var prefixNode = tree.AddNode($"[yellow]📝 Prefix Recommendations: {result.PrefixRecommendations.Count}[/]");

            foreach (var recommendation in result.PrefixRecommendations.Take(10))
            {
                var recNode = prefixNode.AddNode(
                    $"[cyan]{recommendation.TableName}.{recommendation.CurrentColumnName}[/] → [green]{recommendation.RecommendedColumnName}[/]");
                recNode.AddNode($"[dim]Reason: {recommendation.Reason}[/]");
            }

            if (result.PrefixRecommendations.Count > 10)
            {
                prefixNode.AddNode($"[dim]... and {result.PrefixRecommendations.Count - 10} more[/]");
            }
        }

        // Add encryption suggestions section
        if (result.EncryptionSuggestions.Any())
        {
            var encryptNode = tree.AddNode($"[orange]🔐 Encryption Suggestions: {result.EncryptionSuggestions.Count}[/]");

            foreach (var suggestion in result.EncryptionSuggestions.Take(10))
            {
                var suggNode = encryptNode.AddNode(
                    $"[cyan]{suggestion.TableName}.{suggestion.ColumnName}[/] ({suggestion.SensitiveDataType})");
                suggNode.AddNode($"[dim]→ {suggestion.RecommendedEncryptionMethod}[/]");
                suggNode.AddNode($"[dim]{suggestion.Reason}[/]");
            }

            if (result.EncryptionSuggestions.Count > 10)
            {
                encryptNode.AddNode($"[dim]... and {result.EncryptionSuggestions.Count - 10} more[/]");
            }
        }

        // If no issues found
        if (result.TotalIssues == 0)
        {
            tree.AddNode("[green]✓ No security issues found - Excellent![/]");
        }

        return tree;
    }

    private void AddVulnerabilityNode(TreeNode parent, SecurityVulnerability vulnerability)
    {
        var vulnNode = parent.AddNode(
            $"[cyan]{vulnerability.TableName}.{vulnerability.ColumnName}[/] - {vulnerability.Description}");
        vulnNode.AddNode($"[dim]→ {vulnerability.Recommendation}[/]");

        if (!string.IsNullOrWhiteSpace(vulnerability.AdditionalContext))
        {
            vulnNode.AddNode($"[dim italic]{vulnerability.AdditionalContext}[/]");
        }
    }
}
