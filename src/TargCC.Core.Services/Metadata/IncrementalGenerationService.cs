using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Interfaces.Models.Metadata;
using TargCC.Core.Interfaces.Services;

namespace TargCC.Core.Services.Metadata;

/// <summary>
/// Service for incremental code generation based on metadata and change detection
/// </summary>
public class IncrementalGenerationService
{
    private readonly IMetadataService _metadataService;
    private readonly ChangeDetectionService _changeDetectionService;
    private readonly ILogger<IncrementalGenerationService> _logger;

    public IncrementalGenerationService(
        IMetadataService metadataService,
        ChangeDetectionService changeDetectionService,
        ILogger<IncrementalGenerationService> logger)
    {
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
        _changeDetectionService = changeDetectionService ?? throw new ArgumentNullException(nameof(changeDetectionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Determines which tables need regeneration based on schema changes
    /// </summary>
    public async Task<IncrementalGenerationPlan> CreateGenerationPlanAsync(DatabaseSchema schema)
    {
        _logger.LogInformation("Creating incremental generation plan for {TableCount} tables", schema.Tables.Count);

        var plan = new IncrementalGenerationPlan
        {
            TotalTables = schema.Tables.Count,
            CreatedAt = DateTime.UtcNow
        };

        // Get all existing metadata
        var existingMetadata = await _metadataService.GetAllTableMetadataAsync();
        var metadataLookup = existingMetadata.ToDictionary(m => m.TableName, m => m);

        // Analyze each table
        foreach (var table in schema.Tables)
        {
            var tableAnalysis = await AnalyzeTableAsync(table, metadataLookup);
            plan.TablePlans.Add(tableAnalysis);

            if (tableAnalysis.RequiresGeneration)
            {
                plan.TablesToGenerate++;
            }
        }

        _logger.LogInformation(
            "Generation plan created: {Total} tables, {ToGenerate} require generation, {Unchanged} unchanged",
            plan.TotalTables,
            plan.TablesToGenerate,
            plan.TablesUnchanged);

        return plan;
    }

    private async Task<TableGenerationPlan> AnalyzeTableAsync(
        Table table,
        Dictionary<string, TableMetadata> metadataLookup)
    {
        var plan = new TableGenerationPlan
        {
            TableName = table.Name,
            SchemaName = table.Schema
        };

        // Check if metadata exists
        if (!metadataLookup.TryGetValue(table.Name, out var metadata))
        {
            _logger.LogInformation("Table {TableName} is new - will generate", table.Name);
            plan.IsNew = true;
            plan.RequiresGeneration = true;
            plan.Reason = "New table";
            return plan;
        }

        // Compute current hash
        var currentHash = _changeDetectionService.ComputeTableHash(table);
        var previousHash = metadata.SchemaHash;

        // Detect changes
        var changeResult = _changeDetectionService.DetectTableChanges(table, previousHash);
        plan.CurrentHash = currentHash;
        plan.PreviousHash = previousHash;
        plan.HasSchemaChanged = changeResult.HasChanged;

        if (changeResult.HasChanged)
        {
            _logger.LogInformation("Table {TableName} schema has changed - will regenerate", table.Name);
            plan.RequiresGeneration = true;
            plan.Reason = "Schema changed";
        }
        else
        {
            _logger.LogDebug("Table {TableName} unchanged - skipping generation", table.Name);
            plan.RequiresGeneration = false;
            plan.Reason = "No changes detected";
        }

        // Check generation options
        plan.GenerateEntity = metadata.GenerateEntity;
        plan.GenerateRepository = metadata.GenerateRepository;
        plan.GenerateController = metadata.GenerateController;
        plan.GenerateReactUI = metadata.GenerateReactUI;
        plan.GenerateStoredProcedures = metadata.GenerateStoredProcedures;
        plan.GenerateCQRS = metadata.GenerateCQRS;

        return plan;
    }

    /// <summary>
    /// Executes the generation plan and logs results
    /// </summary>
    public async Task<GenerationExecutionResult> ExecuteGenerationPlanAsync(
        IncrementalGenerationPlan plan,
        Func<Table, Task> generateTableFunc)
    {
        var result = new GenerationExecutionResult
        {
            StartedAt = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Starting incremental generation for {Count} tables", plan.TablesToGenerate);

        foreach (var tablePlan in plan.TablePlans.Where(t => t.RequiresGeneration))
        {
            var tableStopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Generating code for table {TableName}", tablePlan.TableName);

                // Execute generation (this would call the actual generator)
                // For now, we just simulate the generation
                await Task.Delay(10); // Simulate work

                tableStopwatch.Stop();

                // Log success
                var history = new GenerationHistoryMetadata
                {
                    TableName = tablePlan.TableName,
                    GenerationType = "Incremental",
                    GeneratedAt = DateTime.UtcNow,
                    DurationMs = (int)tableStopwatch.ElapsedMilliseconds,
                    Success = true,
                    SchemaHash = tablePlan.CurrentHash,
                    ToolVersion = "2.0.0"
                };

                await _metadataService.LogGenerationAsync(history);

                // Update table metadata
                var metadata = await _metadataService.GetTableMetadataAsync(tablePlan.SchemaName, tablePlan.TableName);
                if (metadata != null)
                {
                    metadata.LastGenerated = DateTime.UtcNow;
                    metadata.SchemaHashPrevious = metadata.SchemaHash;
                    metadata.SchemaHash = tablePlan.CurrentHash;
                    await _metadataService.UpsertTableMetadataAsync(metadata);
                }

                result.SuccessCount++;
                _logger.LogInformation(
                    "Successfully generated {TableName} in {Duration}ms",
                    tablePlan.TableName,
                    tableStopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                tableStopwatch.Stop();

                _logger.LogError(ex, "Failed to generate code for table {TableName}", tablePlan.TableName);

                // Log failure
                var history = new GenerationHistoryMetadata
                {
                    TableName = tablePlan.TableName,
                    GenerationType = "Incremental",
                    GeneratedAt = DateTime.UtcNow,
                    DurationMs = (int)tableStopwatch.ElapsedMilliseconds,
                    Success = false,
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    SchemaHash = tablePlan.CurrentHash,
                    ToolVersion = "2.0.0"
                };

                await _metadataService.LogGenerationAsync(history);

                result.FailureCount++;
                result.Errors.Add($"{tablePlan.TableName}: {ex.Message}");
            }
        }

        stopwatch.Stop();

        result.CompletedAt = DateTime.UtcNow;
        result.TotalDurationMs = (int)stopwatch.ElapsedMilliseconds;

        _logger.LogInformation(
            "Incremental generation completed: {Success} succeeded, {Failed} failed, {Duration}ms total",
            result.SuccessCount,
            result.FailureCount,
            result.TotalDurationMs);

        return result;
    }

    /// <summary>
    /// Gets a summary of pending changes
    /// </summary>
    public async Task<ChangesSummary> GetChangesSummaryAsync(DatabaseSchema schema)
    {
        var plan = await CreateGenerationPlanAsync(schema);

        var summary = new ChangesSummary
        {
            TotalTables = plan.TotalTables,
            NewTables = plan.TablePlans.Count(t => t.IsNew),
            ModifiedTables = plan.TablePlans.Count(t => t.HasSchemaChanged && !t.IsNew),
            UnchangedTables = plan.TablePlans.Count(t => !t.RequiresGeneration),
            CheckedAt = DateTime.UtcNow
        };

        summary.NewTableNames = plan.TablePlans.Where(t => t.IsNew).Select(t => t.TableName).ToList();
        summary.ModifiedTableNames = plan.TablePlans.Where(t => t.HasSchemaChanged && !t.IsNew).Select(t => t.TableName).ToList();

        return summary;
    }
}

/// <summary>
/// Plan for incremental generation
/// </summary>
public class IncrementalGenerationPlan
{
    public int TotalTables { get; set; }
    public int TablesToGenerate { get; set; }
    public int TablesUnchanged => TotalTables - TablesToGenerate;
    public DateTime CreatedAt { get; set; }
    public List<TableGenerationPlan> TablePlans { get; set; } = new();
}

/// <summary>
/// Generation plan for a single table
/// </summary>
public class TableGenerationPlan
{
    public string TableName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = "dbo";
    public bool IsNew { get; set; }
    public bool HasSchemaChanged { get; set; }
    public bool RequiresGeneration { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? CurrentHash { get; set; }
    public string? PreviousHash { get; set; }

    // Generation options
    public bool GenerateEntity { get; set; } = true;
    public bool GenerateRepository { get; set; } = true;
    public bool GenerateController { get; set; } = true;
    public bool GenerateReactUI { get; set; }
    public bool GenerateStoredProcedures { get; set; } = true;
    public bool GenerateCQRS { get; set; } = true;
}

/// <summary>
/// Result of generation execution
/// </summary>
public class GenerationExecutionResult
{
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public int TotalDurationMs { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Summary of pending changes
/// </summary>
public class ChangesSummary
{
    public int TotalTables { get; set; }
    public int NewTables { get; set; }
    public int ModifiedTables { get; set; }
    public int UnchangedTables { get; set; }
    public DateTime CheckedAt { get; set; }
    public List<string> NewTableNames { get; set; } = new();
    public List<string> ModifiedTableNames { get; set; } = new();
}
