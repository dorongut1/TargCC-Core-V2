// <copyright file="AnalyzeSecurityCommand.cs" company="Doron Vaida">
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

            SecurityAnalysisResult result = null!;
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

    private void DisplayResults(SecurityAnalysisResult result)
    {
        var panel = new Panel(this.BuildResultsMarkup(result))
        {
            Header = new PanelHeader("[bold cyan]🔒 Security Analysis[/]"),
            Border = BoxBorder.Rounded,
        };

        AnsiConsole.Write(panel);
    }

    private IRenderable BuildResultsMarkup(SecurityAnalysisResult result)
    {
        var tree = new Tree($"[bold]Issues Found: {result.Issues.Count}[/]");

        if (result.Issues.Any())
        {
            var highIssues = result.Issues.Where(i => i.Severity == SecuritySeverity.High).ToList();
            var mediumIssues = result.Issues.Where(i => i.Severity == SecuritySeverity.Medium).ToList();
            var lowIssues = result.Issues.Where(i => i.Severity == SecuritySeverity.Low).ToList();

            if (highIssues.Any())
            {
                var highNode = tree.AddNode($"[red]High Priority: {highIssues.Count}[/]");
                foreach (var issue in highIssues)
                {
                    this.AddIssueNode(highNode, issue);
                }
            }

            if (mediumIssues.Any())
            {
                var mediumNode = tree.AddNode($"[yellow]Medium Priority: {mediumIssues.Count}[/]");
                foreach (var issue in mediumIssues)
                {
                    this.AddIssueNode(mediumNode, issue);
                }
            }

            if (lowIssues.Any())
            {
                var lowNode = tree.AddNode($"[dim]Low Priority: {lowIssues.Count}[/]");
                foreach (var issue in lowIssues)
                {
                    this.AddIssueNode(lowNode, issue);
                }
            }
        }
        else
        {
            tree.AddNode("[green]✓ No security issues found[/]");
        }

        var compliancePercentage = (int)result.CompliancePercentage;
        var complianceColor = compliancePercentage >= 90 ? "green" : compliancePercentage >= 70 ? "yellow" : "red";
        tree.AddNode($"[{complianceColor}]✓ Compliant Fields: {result.CompliantFields}/{result.TotalFieldsChecked} ({compliancePercentage}%)[/]");

        return tree;
    }

    private void AddIssueNode(TreeNode parent, SecurityIssue issue)
    {
        var issueNode = parent.AddNode($"[cyan]{issue.TableName}.{issue.ColumnName}[/] - {issue.Description}");
        issueNode.AddNode($"[dim]→ {issue.Recommendation}[/]");
    }
}
