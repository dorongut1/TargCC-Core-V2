// <copyright file="WatchCommand.cs" company="Doron">
// Copyright (c) Doron. All rights reserved.
// </copyright>

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using TargCC.CLI.Configuration;
using TargCC.CLI.Constants;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Generation;
using TargCC.Core.Analyzers;
using TargCC.Core.Services;

namespace TargCC.CLI.Commands;

/// <summary>
/// Command to watch database for schema changes and auto-regenerate affected files.
/// </summary>
public class WatchCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WatchCommand"/> class.
    /// </summary>
    /// <param name="configurationService">Configuration service.</param>
    /// <param name="schemaChangeDetector">Schema change detector.</param>
    /// <param name="generationService">Generation service.</param>
    /// <param name="generationTracker">Generation tracker.</param>
    /// <param name="outputService">Output service.</param>
    /// <param name="loggerFactory">Logger factory.</param>
    public WatchCommand(
        IConfigurationService configurationService,
        ISchemaChangeDetector schemaChangeDetector,
        IGenerationService generationService,
        IGenerationTracker generationTracker,
        IOutputService outputService,
        ILoggerFactory loggerFactory)
        : base("watch", "Watch database for schema changes and auto-regenerate affected files")
    {
        var intervalOption = new Option<int>(
            aliases: new[] { "--interval", "-i" },
            description: "Check interval in seconds",
            getDefaultValue: () => 5);

        var noAutoGenerateOption = new Option<bool>(
            aliases: new[] { "--no-auto-generate" },
            description: "Only detect changes, don't auto-regenerate files",
            getDefaultValue: () => false);

        var tablesOption = new Option<string[]>(
            aliases: new[] { "--tables", "-t" },
            description: "Watch only specific tables (comma-separated)");

        AddOption(intervalOption);
        AddOption(noAutoGenerateOption);
        AddOption(tablesOption);

        this.SetHandler(
            ExecuteAsync,
            intervalOption,
            noAutoGenerateOption,
            tablesOption,
            new InvocationContextBinder(configurationService, schemaChangeDetector, generationService, generationTracker, outputService, loggerFactory));
    }

    private class InvocationContextBinder : System.CommandLine.Binding.BinderBase<CommandContext>
    {
        private readonly IConfigurationService _configurationService;
        private readonly ISchemaChangeDetector _schemaChangeDetector;
        private readonly IGenerationService _generationService;
        private readonly IGenerationTracker _generationTracker;
        private readonly IOutputService _outputService;
        private readonly ILoggerFactory _loggerFactory;

        public InvocationContextBinder(
            IConfigurationService configurationService,
            ISchemaChangeDetector schemaChangeDetector,
            IGenerationService generationService,
            IGenerationTracker generationTracker,
            IOutputService outputService,
            ILoggerFactory loggerFactory)
        {
            _configurationService = configurationService;
            _schemaChangeDetector = schemaChangeDetector;
            _generationService = generationService;
            _generationTracker = generationTracker;
            _outputService = outputService;
            _loggerFactory = loggerFactory;
        }

        protected override CommandContext GetBoundValue(System.CommandLine.Binding.BindingContext bindingContext)
        {
            return new CommandContext(
                _configurationService,
                _schemaChangeDetector,
                _generationService,
                _generationTracker,
                _outputService,
                _loggerFactory);
        }
    }

    private record CommandContext(
        IConfigurationService ConfigurationService,
        ISchemaChangeDetector SchemaChangeDetector,
        IGenerationService GenerationService,
        IGenerationTracker GenerationTracker,
        IOutputService OutputService,
        ILoggerFactory LoggerFactory);

    private static async Task<int> ExecuteAsync(
        int interval,
        bool noAutoGenerate,
        string[]? tables,
        CommandContext context)
    {
        var logger = context.LoggerFactory.CreateLogger<WatchCommand>();

        try
        {
            // Load configuration
            var config = await context.ConfigurationService.LoadAsync();
            if (config == null)
            {
                context.OutputService.Error("Configuration not found. Run 'targcc init' first.");
                return ExitCodes.GeneralError;
            }

            var connectionString = config.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                context.OutputService.Error("Connection string not configured.");
                return ExitCodes.GeneralError;
            }

            var snapshotPath = Path.Combine(".targcc", "schema.json");

            // Create initial snapshot if it doesn't exist
            if (!File.Exists(snapshotPath))
            {
                context.OutputService.Info("Creating initial schema snapshot...");
                await context.SchemaChangeDetector.SaveSchemaSnapshotAsync(
                    connectionString,
                    snapshotPath);
                context.OutputService.Success("Initial snapshot created.");
            }

            // Start watching
            context.OutputService.Heading("🔍 Watching database for changes...");
            AnsiConsole.MarkupLine($"   Connected to: [cyan]{config.LastUsedDatabase ?? "database"}[/]");
            AnsiConsole.MarkupLine($"   Snapshot: [cyan]{snapshotPath}[/]");
            AnsiConsole.MarkupLine($"   Checking every [cyan]{interval}[/] seconds");
            
            if (tables != null && tables.Length > 0)
            {
                AnsiConsole.MarkupLine($"   Watching tables: [cyan]{string.Join(", ", tables)}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("   Watching: [cyan]all tables[/]");
            }

            AnsiConsole.MarkupLine("   Press [red]Ctrl+C[/] to stop\n");

            // Setup cancellation
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                AnsiConsole.MarkupLine("\n[yellow]⏹ Stopping watch mode...[/]");
            };

            // Watch loop
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    var timestamp = DateTime.Now.ToString("HH:mm:ss");
                    AnsiConsole.MarkupLine($"[dim]{timestamp}[/] 🔍 Checking for changes...");

                    var changes = await context.SchemaChangeDetector.DetectChangesAsync(
                        connectionString,
                        snapshotPath,
                        cts.Token);

                    if (changes.HasChanges)
                    {
                        await HandleChangesAsync(
                            changes,
                            tables,
                            noAutoGenerate,
                            connectionString,
                            snapshotPath,
                            context,
                            config,
                            cts.Token);
                    }

                    // Wait for next check
                    await Task.Delay(TimeSpan.FromSeconds(interval), cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during watch cycle");
                    context.OutputService.Error($"Error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(interval), cts.Token);
                }
            }

            context.OutputService.Success("✅ Watch mode stopped");
            return ExitCodes.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in watch command");
            context.OutputService.Error($"Error: {ex.Message}");
            return ExitCodes.GeneralError;
        }
    }

    private static async Task HandleChangesAsync(
        Core.Analyzers.Models.SchemaChanges changes,
        string[]? watchTables,
        bool noAutoGenerate,
        string connectionString,
        string snapshotPath,
        CommandContext context,
        CliConfiguration config,
        CancellationToken cancellationToken)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        AnsiConsole.MarkupLine($"\n[yellow]{timestamp}[/] ⚡ [yellow]Change detected![/]\n");

        // Display changes
        DisplayChanges(changes, context);

        // Filter changes by watched tables if specified
        var affectedTables = GetAffectedTables(changes, watchTables);

        if (affectedTables.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No watched tables affected[/]\n");
            return;
        }

        if (noAutoGenerate)
        {
            AnsiConsole.MarkupLine("[yellow]Auto-generation disabled. Skipping regeneration.[/]\n");
            return;
        }

        // Auto-regenerate affected files
        AnsiConsole.MarkupLine($"[cyan]📦 Auto-regenerating affected files...[/]");

        foreach (var tableName in affectedTables)
        {
            try
            {
                // Generate all files for the affected table
                var result = await context.GenerationService.GenerateAllAsync(
                    connectionString,
                    tableName,
                    config.OutputDirectory ?? "./output",
                    config.DefaultNamespace);

                if (result.Success)
                {
                    foreach (var file in result.GeneratedFiles)
                    {
                        AnsiConsole.MarkupLine($"   [green]✓[/] {file.FilePath}");
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"   [red]✗[/] Failed: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"   [red]✗[/] Error regenerating {tableName}: {ex.Message}");
            }
        }

        // Update snapshot with new schema
        await context.SchemaChangeDetector.SaveSchemaSnapshotAsync(
            connectionString,
            snapshotPath,
            cancellationToken);

        AnsiConsole.MarkupLine($"\n[green]✅ Generation complete[/]\n");
    }

    private static void DisplayChanges(Core.Analyzers.Models.SchemaChanges changes, CommandContext context)
    {
        if (changes.TableChanges.Count > 0)
        {
            AnsiConsole.MarkupLine("[yellow]Tables:[/]");
            foreach (var change in changes.TableChanges)
            {
                var icon = change.Type switch
                {
                    Core.Analyzers.Models.ChangeType.Added => "[green]+[/]",
                    Core.Analyzers.Models.ChangeType.Removed => "[red]-[/]",
                    _ => "[yellow]~[/]",
                };
                AnsiConsole.MarkupLine($"   {icon} {change.Description}");
            }
        }

        if (changes.ColumnChanges.Count > 0)
        {
            AnsiConsole.MarkupLine("[yellow]Columns:[/]");
            foreach (var change in changes.ColumnChanges)
            {
                var icon = change.Type switch
                {
                    Core.Analyzers.Models.ChangeType.Added => "[green]+[/]",
                    Core.Analyzers.Models.ChangeType.Removed => "[red]-[/]",
                    _ => "[yellow]~[/]",
                };
                AnsiConsole.MarkupLine($"   {icon} {change.Description}");
            }
        }

        if (changes.IndexChanges.Count > 0)
        {
            AnsiConsole.MarkupLine("[yellow]Indexes:[/]");
            foreach (var change in changes.IndexChanges)
            {
                var icon = change.Type switch
                {
                    Core.Analyzers.Models.ChangeType.Added => "[green]+[/]",
                    Core.Analyzers.Models.ChangeType.Removed => "[red]-[/]",
                    _ => "[yellow]~[/]",
                };
                AnsiConsole.MarkupLine($"   {icon} {change.Description}");
            }
        }

        if (changes.RelationshipChanges.Count > 0)
        {
            AnsiConsole.MarkupLine("[yellow]Relationships:[/]");
            foreach (var change in changes.RelationshipChanges)
            {
                var icon = change.Type switch
                {
                    Core.Analyzers.Models.ChangeType.Added => "[green]+[/]",
                    Core.Analyzers.Models.ChangeType.Removed => "[red]-[/]",
                    _ => "[yellow]~[/]",
                };
                AnsiConsole.MarkupLine($"   {icon} {change.Description}");
            }
        }

        AnsiConsole.WriteLine();
    }

    private static List<string> GetAffectedTables(
        Core.Analyzers.Models.SchemaChanges changes,
        string[]? watchTables)
    {
        var affected = new HashSet<string>();

        foreach (var change in changes.TableChanges)
        {
            affected.Add(change.TableName);
        }

        foreach (var change in changes.ColumnChanges)
        {
            affected.Add(change.TableName);
        }

        foreach (var change in changes.IndexChanges)
        {
            affected.Add(change.TableName);
        }

        // Filter by watched tables if specified
        if (watchTables != null && watchTables.Length > 0)
        {
            affected.IntersectWith(watchTables);
        }

        return affected.ToList();
    }
}
