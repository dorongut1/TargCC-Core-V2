// <copyright file="AnalyzeSchemaCommand.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using System.CommandLine;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using TargCC.CLI.Models.Analysis;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Analysis;


namespace TargCC.CLI.Commands.Analyze;

/// <summary>
/// Command to analyze database schema.
/// </summary>
public class AnalyzeSchemaCommand : Command
{
    private readonly IAnalysisService analysisService;
    private readonly IOutputService outputService;
    private readonly ILogger<AnalyzeSchemaCommand> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeSchemaCommand"/> class.
    /// </summary>
    /// <param name="analysisService">Analysis service.</param>
    /// <param name="outputService">Output service.</param>
    /// <param name="loggerFactory">Logger factory.</param>
    public AnalyzeSchemaCommand(
        IAnalysisService analysisService,
        IOutputService outputService,
        ILoggerFactory loggerFactory)
        : base("schema", "Analyze database schema structure")
    {
        this.analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
        this.outputService = outputService ?? throw new ArgumentNullException(nameof(outputService));
        ArgumentNullException.ThrowIfNull(loggerFactory);
        this.logger = loggerFactory.CreateLogger<AnalyzeSchemaCommand>();

        var tableArgument = new Argument<string?>(
            name: "table",
            description: "Optional table name to analyze. If not specified, all tables will be analyzed.",
            getDefaultValue: () => null);

        this.AddArgument(tableArgument);

        this.SetHandler(this.HandleAsync, tableArgument);
    }

    private async Task<int> HandleAsync(string? tableName)
    {
        try
        {
            this.logger.LogInformation("Starting schema analysis for table: {TableName}", tableName ?? "all");

            SchemaAnalysisResult result = null!;
            await this.outputService.SpinnerAsync(
                "Analyzing schema...",
                async () =>
                {
                    result = await this.analysisService.AnalyzeSchemaAsync(tableName);
                });

            // Display results
            this.DisplayResults(result, tableName);

            this.outputService.Success("Schema analysis completed");
            return 0;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error analyzing schema");
            this.outputService.Error($"Error: {ex.Message}");
            return 1;
        }
    }

    private void DisplayResults(SchemaAnalysisResult result, string? tableName)
    {
        var panel = new Panel(this.BuildResultsTable(result, tableName))
        {
            Header = new PanelHeader("[bold cyan]📊 Database Schema Analysis[/]"),
            Border = BoxBorder.Rounded,
        };

        AnsiConsole.Write(panel);
    }

    private Table BuildResultsTable(SchemaAnalysisResult result, string? tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            // Show all tables summary
            var table = new Table();
            table.AddColumn("[bold]Table Name[/]");
            table.AddColumn("[bold]Columns[/]");
            table.AddColumn("[bold]Indexes[/]");

            foreach (var tableInfo in result.Tables)
            {
                table.AddRow(
                    $"[cyan]{tableInfo.Name}[/]",
                    tableInfo.Columns.Count.ToString(),
                    tableInfo.Indexes.Count.ToString());
            }

            table.Caption = new TableTitle($"[dim]Total: {result.TotalTables} tables, {result.TotalColumns} columns[/]");

            return table;
        }
        else
        {
            // Show detailed table info
            var tableInfo = result.Tables.First();
            var table = new Table();
            table.AddColumn("[bold]Column Name[/]");
            table.AddColumn("[bold]Data Type[/]");
            table.AddColumn("[bold]Nullable[/]");
            table.AddColumn("[bold]Primary Key[/]");
            table.AddColumn("[bold]Special[/]");

            foreach (var column in tableInfo.Columns)
            {
                table.AddRow(
                    $"[cyan]{column.Name}[/]",
                    column.DataType,
                    column.IsNullable ? "[yellow]Yes[/]" : "[green]No[/]",
                    column.IsPrimaryKey ? "[green]✓[/]" : string.Empty,
                    column.SpecialPrefix ?? string.Empty);
            }

            // Add indexes section
            if (tableInfo.Indexes.Any())
            {
                table.Caption = new TableTitle($"[dim]Indexes: {string.Join(", ", tableInfo.Indexes.Select(i => i.Name))}[/]");
            }

            return table;
        }
    }
}
