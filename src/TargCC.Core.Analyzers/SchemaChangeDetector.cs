// <copyright file="SchemaChangeDetector.cs" company="Doron">
// Copyright (c) Doron. All rights reserved.
// </copyright>

using System.Text.Json;
using Microsoft.Extensions.Logging;
using TargCC.Core.Analyzers.Models;
using TargCC.Core.Interfaces;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Analyzers;

/// <summary>
/// Implementation of schema change detection service.
/// </summary>
public class SchemaChangeDetector : ISchemaChangeDetector
{
    private readonly IDatabaseAnalyzer _databaseAnalyzer;
    private readonly ILogger<SchemaChangeDetector> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaChangeDetector"/> class.
    /// </summary>
    /// <param name="databaseAnalyzer">Database analyzer.</param>
    /// <param name="logger">Logger instance.</param>
    public SchemaChangeDetector(
        IDatabaseAnalyzer databaseAnalyzer,
        ILogger<SchemaChangeDetector> logger)
    {
        _databaseAnalyzer = databaseAnalyzer ?? throw new ArgumentNullException(nameof(databaseAnalyzer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<SchemaChanges> DetectChangesAsync(
        string connectionString,
        string snapshotPath,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Detecting schema changes from snapshot: {Path}", snapshotPath);

        var oldSchema = await LoadSchemaSnapshotAsync(snapshotPath, cancellationToken);
        if (oldSchema == null)
        {
            _logger.LogWarning("No snapshot found, treating all as new");
            return new SchemaChanges();
        }

        var newSchema = await GetCurrentSchemaAsync(connectionString, cancellationToken);

        var changes = CompareSchemas(oldSchema, newSchema);

        _logger.LogInformation(
            "Detected {Count} changes ({Tables} tables, {Columns} columns, {Indexes} indexes, {Relationships} relationships)",
            changes.TotalChanges,
            changes.TableChanges.Count,
            changes.ColumnChanges.Count,
            changes.IndexChanges.Count,
            changes.RelationshipChanges.Count);

        return changes;
    }

    /// <inheritdoc/>
    public async Task SaveSchemaSnapshotAsync(
        string connectionString,
        string snapshotPath,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Saving schema snapshot to: {Path}", snapshotPath);

        var schema = await GetCurrentSchemaAsync(connectionString, cancellationToken);

        var directory = Path.GetDirectoryName(snapshotPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        var json = JsonSerializer.Serialize(schema, options);
        await File.WriteAllTextAsync(snapshotPath, json, cancellationToken);

        _logger.LogInformation("Schema snapshot saved successfully");
    }

    /// <inheritdoc/>
    public async Task<Interfaces.Models.DatabaseSchema?> LoadSchemaSnapshotAsync(
        string snapshotPath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(snapshotPath))
        {
            _logger.LogWarning("Snapshot file not found: {Path}", snapshotPath);
            return null;
        }

        _logger.LogInformation("Loading schema snapshot from: {Path}", snapshotPath);

        var json = await File.ReadAllTextAsync(snapshotPath, cancellationToken);
        var schema = JsonSerializer.Deserialize<Interfaces.Models.DatabaseSchema>(json);

        _logger.LogInformation("Schema snapshot loaded: {TableCount} tables", schema?.Tables.Count ?? 0);

        return schema;
    }

    /// <inheritdoc/>
    public async Task<DatabaseSchema> GetCurrentSchemaAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing current database schema");

        var database = await _databaseAnalyzer.AnalyzeDatabaseAsync(connectionString);

        var schema = new Interfaces.Models.DatabaseSchema
        {
            AnalyzedAt = DateTime.UtcNow,
            DatabaseName = database.DatabaseName,
            ServerName = database.ServerName,
            Version = database.Version,
            Tables = database.Tables,
            Relationships = database.Relationships,
        };

        _logger.LogInformation("Current schema: {TableCount} tables, {RelCount} relationships", 
            schema.Tables.Count, 
            schema.Relationships.Count);

        return schema;
    }

    private SchemaChanges CompareSchemas(Interfaces.Models.DatabaseSchema oldSchema, Interfaces.Models.DatabaseSchema newSchema)
    {
        var changes = new SchemaChanges();

        // Detect table changes
        DetectTableChanges(oldSchema, newSchema, changes);

        // Detect column changes
        DetectColumnChanges(oldSchema, newSchema, changes);

        // Detect index changes
        DetectIndexChanges(oldSchema, newSchema, changes);

        // Detect relationship changes
        DetectRelationshipChanges(oldSchema, newSchema, changes);

        return changes;
    }

    private void DetectTableChanges(
        Interfaces.Models.DatabaseSchema oldSchema,
        Interfaces.Models.DatabaseSchema newSchema,
        SchemaChanges changes)
    {
        var oldTables = oldSchema.Tables.ToDictionary(t => t.Name);
        var newTables = newSchema.Tables.ToDictionary(t => t.Name);

        // Find added tables
        foreach (var tableName in newTables.Keys.Except(oldTables.Keys))
        {
            changes.TableChanges.Add(new TableChange
            {
                Type = ChangeType.Added,
                TableName = tableName,
                NewMetadata = newTables[tableName],
                Description = $"Table '{tableName}' was added",
            });
        }

        // Find removed tables
        foreach (var tableName in oldTables.Keys.Except(newTables.Keys))
        {
            changes.TableChanges.Add(new TableChange
            {
                Type = ChangeType.Removed,
                TableName = tableName,
                OldMetadata = oldTables[tableName],
                Description = $"Table '{tableName}' was removed",
            });
        }
    }

    private void DetectColumnChanges(
        Interfaces.Models.DatabaseSchema oldSchema,
        Interfaces.Models.DatabaseSchema newSchema,
        SchemaChanges changes)
    {
        var oldTables = oldSchema.Tables.ToDictionary(t => t.Name);
        var newTables = newSchema.Tables.ToDictionary(t => t.Name);

        // Only check tables that exist in both schemas
        foreach (var tableName in oldTables.Keys.Intersect(newTables.Keys))
        {
            var oldColumns = oldTables[tableName].Columns.ToDictionary(c => c.Name);
            var newColumns = newTables[tableName].Columns.ToDictionary(c => c.Name);

            // Find added columns
            foreach (var columnName in newColumns.Keys.Except(oldColumns.Keys))
            {
                changes.ColumnChanges.Add(new ColumnChange
                {
                    Type = ChangeType.Added,
                    TableName = tableName,
                    ColumnName = columnName,
                    NewMetadata = newColumns[columnName],
                    Description = $"Column '{tableName}.{columnName}' was added",
                });
            }

            // Find removed columns
            foreach (var columnName in oldColumns.Keys.Except(newColumns.Keys))
            {
                changes.ColumnChanges.Add(new ColumnChange
                {
                    Type = ChangeType.Removed,
                    TableName = tableName,
                    ColumnName = columnName,
                    OldMetadata = oldColumns[columnName],
                    Description = $"Column '{tableName}.{columnName}' was removed",
                });
            }

            // Find modified columns
            foreach (var columnName in oldColumns.Keys.Intersect(newColumns.Keys))
            {
                var oldColumn = oldColumns[columnName];
                var newColumn = newColumns[columnName];

                if (IsColumnModified(oldColumn, newColumn))
                {
                    changes.ColumnChanges.Add(new ColumnChange
                    {
                        Type = ChangeType.Modified,
                        TableName = tableName,
                        ColumnName = columnName,
                        OldMetadata = oldColumn,
                        NewMetadata = newColumn,
                        Description = GetColumnModificationDescription(oldColumn, newColumn, tableName),
                    });
                }
            }
        }
    }

    private void DetectIndexChanges(
        Interfaces.Models.DatabaseSchema oldSchema,
        Interfaces.Models.DatabaseSchema newSchema,
        SchemaChanges changes)
    {
        var oldTables = oldSchema.Tables.ToDictionary(t => t.Name);
        var newTables = newSchema.Tables.ToDictionary(t => t.Name);

        // Only check tables that exist in both schemas
        foreach (var tableName in oldTables.Keys.Intersect(newTables.Keys))
        {
            var oldIndexes = oldTables[tableName].Indexes.ToDictionary(i => i.Name);
            var newIndexes = newTables[tableName].Indexes.ToDictionary(i => i.Name);

            // Find added indexes
            foreach (var indexName in newIndexes.Keys.Except(oldIndexes.Keys))
            {
                changes.IndexChanges.Add(new IndexChange
                {
                    Type = ChangeType.Added,
                    TableName = tableName,
                    IndexName = indexName,
                    NewMetadata = newIndexes[indexName],
                    Description = $"Index '{indexName}' was added on '{tableName}'",
                });
            }

            // Find removed indexes
            foreach (var indexName in oldIndexes.Keys.Except(newIndexes.Keys))
            {
                changes.IndexChanges.Add(new IndexChange
                {
                    Type = ChangeType.Removed,
                    TableName = tableName,
                    IndexName = indexName,
                    OldMetadata = oldIndexes[indexName],
                    Description = $"Index '{indexName}' was removed from '{tableName}'",
                });
            }
        }
    }

    private void DetectRelationshipChanges(
        Interfaces.Models.DatabaseSchema oldSchema,
        Interfaces.Models.DatabaseSchema newSchema,
        SchemaChanges changes)
    {
        var oldRelationships = oldSchema.Relationships.ToDictionary(r =>
            $"{r.ParentTable}.{r.ParentColumn}->{r.ChildTable}.{r.ChildColumn}");
        var newRelationships = newSchema.Relationships.ToDictionary(r =>
            $"{r.ParentTable}.{r.ParentColumn}->{r.ChildTable}.{r.ChildColumn}");

        // Find added relationships
        foreach (var key in newRelationships.Keys.Except(oldRelationships.Keys))
        {
            var rel = newRelationships[key];
            changes.RelationshipChanges.Add(new RelationshipChange
            {
                Type = ChangeType.Added,
                SourceTable = rel.ParentTable,
                TargetTable = rel.ChildTable,
                NewMetadata = rel,
                Description = $"Relationship added: {rel.ParentTable}.{rel.ParentColumn} -> {rel.ChildTable}.{rel.ChildColumn}",
            });
        }

        // Find removed relationships
        foreach (var key in oldRelationships.Keys.Except(newRelationships.Keys))
        {
            var rel = oldRelationships[key];
            changes.RelationshipChanges.Add(new RelationshipChange
            {
                Type = ChangeType.Removed,
                SourceTable = rel.ParentTable,
                TargetTable = rel.ChildTable,
                OldMetadata = rel,
                Description = $"Relationship removed: {rel.ParentTable}.{rel.ParentColumn} -> {rel.ChildTable}.{rel.ChildColumn}",
            });
        }
    }

    private bool IsColumnModified(Column oldColumn, Column newColumn)
    {
        return oldColumn.DataType != newColumn.DataType ||
               oldColumn.MaxLength != newColumn.MaxLength ||
               oldColumn.IsNullable != newColumn.IsNullable ||
               oldColumn.IsPrimaryKey != newColumn.IsPrimaryKey ||
               oldColumn.IsForeignKey != newColumn.IsForeignKey;
    }

    private string GetColumnModificationDescription(Column oldColumn, Column newColumn, string tableName)
    {
        var changes = new List<string>();

        if (oldColumn.DataType != newColumn.DataType)
        {
            changes.Add($"type changed: {oldColumn.DataType} → {newColumn.DataType}");
        }

        if (oldColumn.MaxLength != newColumn.MaxLength)
        {
            changes.Add($"length changed: {oldColumn.MaxLength} → {newColumn.MaxLength}");
        }

        if (oldColumn.IsNullable != newColumn.IsNullable)
        {
            changes.Add($"nullability changed: {(oldColumn.IsNullable ? "NULL" : "NOT NULL")} → {(newColumn.IsNullable ? "NULL" : "NOT NULL")}");
        }

        return $"Column '{tableName}.{newColumn.Name}' modified: {string.Join(", ", changes)}";
    }
}
