// <copyright file="AnalyzeImpactCommand.cs" company="Doron Vaida">
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
/// Command to analyze impact of schema changes.
/// </summary>
public class AnalyzeImpactCommand : Command
{
    private readonly IAnalysisService analysisService;
    private readonly IOutputService outputService;
    private readonly ILogger<AnalyzeImpactCommand> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeImpactCommand"/> class.
    /// </summary>
    /// <param name="analysisService">Analysis service.</param>
    /// <param name="outputService">Output service.</param>
    /// <param name="loggerFactory">Logger factory.</param>
    public AnalyzeImpactCommand(
        IAnalysisService analysisService,
        IOutputService outputService,
        ILoggerFactory loggerFactory)
        : base("impact", "Analyze impact of schema changes")
    {
        this.analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
        this.outputService = outputService ?? throw new ArgumentNullException(nameof(outputService));
        ArgumentNullException.ThrowIfNull(loggerFactory);
        this.logger = loggerFactory.CreateLogger<AnalyzeImpactCommand>();

        var tableArgument = new Argument<string>(
            name: "table",
            description: "Table name to analyze");

        var changeOption = new Option<string>(
            name: "--change",
            description: "Type of change (e.g., column-type, column-add, table-rename)")
        {
            IsRequired = true,
        };

        var columnOption = new Option<string?>(
            name: "--column",
            description: "Column name affected by the change",
            getDefaultValue: () => null);

        var newValueOption = new Option<string?>(
            name: "--new-value",
            description: "New value (type, length, etc.)",
            getDefaultValue: () => null);

        this.AddArgument(tableArgument);
        this.AddOption(changeOption);
        this.AddOption(columnOption);
        this.AddOption(newValueOption);

        this.SetHandler(
            this.HandleAsync,
            tableArgument,
            changeOption,
            columnOption,
            newValueOption);
    }

    private async Task<int> HandleAsync(
        string tableName,
        string changeType,
        string? columnName,
        string? newValue)
    {
        try
        {
            this.logger.LogInformation(
                "Starting impact analysis for table: {TableName}, change: {ChangeType}",
                tableName,
                changeType);

            ImpactAnalysisResult result = null!;
            await this.outputService.SpinnerAsync(
                "Analyzing impact...",
                async () =>
                {
                    result = await this.analysisService.AnalyzeImpactAsync(
                        tableName,
                        changeType,
                        columnName,
                        newValue);
                });

            // Display results
            this.DisplayResults(result);

            this.outputService.Success("Impact analysis completed");
            return 0;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error analyzing impact");
            this.outputService.Error($"Error: {ex.Message}");
            return 1;
        }
    }

    private void DisplayResults(Models.Analysis.ImpactAnalysisResult result)
    {
        var panel = new Panel(this.BuildResultsMarkup(result))
        {
            Header = new PanelHeader("[bold cyan]🔍 Impact Analysis[/]"),
            Border = BoxBorder.Rounded,
        };

        AnsiConsole.Write(panel);
    }

    private IRenderable BuildResultsMarkup(Models.Analysis.ImpactAnalysisResult result)
    {
        var tree = new Tree("[bold]Affected Files[/]");

        if (result.AffectedFiles.Any())
        {
            var filesNode = tree.AddNode($"[yellow]📁 {result.AffectedFiles.Count} files[/]");

            foreach (var file in result.AffectedFiles)
            {
                var fileNode = filesNode.AddNode($"[cyan]{file.FilePath}[/]");
                fileNode.AddNode($"[dim]{file.Impact}[/]");
            }
        }

        if (result.ManualCodeFiles.Any())
        {
            var manualNode = tree.AddNode($"[red]⚠️  Manual Code Impact: {result.ManualCodeFiles.Count} files[/]");

            foreach (var file in result.ManualCodeFiles)
            {
                manualNode.AddNode($"[yellow]{file}[/] - Check custom logic");
            }
        }

        tree.AddNode($"[green]⏱️  Estimated Fix Time: ~{result.EstimatedFixTimeMinutes} minutes[/]");

        return tree;
    }
}
