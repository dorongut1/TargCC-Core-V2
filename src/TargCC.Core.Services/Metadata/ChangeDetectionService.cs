using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Services.Metadata;

/// <summary>
/// Service for detecting schema changes using SHA256 hashing
/// </summary>
public class ChangeDetectionService
{
    private readonly ILogger _logger;

    public ChangeDetectionService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Computes SHA256 hash for a table schema
    /// </summary>
    public string ComputeSchemaHash(string schemaName, string tableName, IEnumerable<dynamic> columns)
    {
        var schemaData = new
        {
            SchemaName = schemaName,
            TableName = tableName,
            Columns = columns.Select(c => new
            {
                ColumnName = (string)c.ColumnName,
                DataType = (string)c.DataType,
                MaxLength = c.MaxLength,
                Precision = c.Precision,
                Scale = c.Scale,
                IsNullable = (string)c.IsNullable == "YES",
                DefaultValue = c.DefaultValue,
                OrdinalPosition = (int)c.OrdinalPosition,
                IsIdentity = c.IsIdentity == 1,
                IsComputed = c.IsComputed == 1
            }).OrderBy(c => c.OrdinalPosition)
        };

        var json = JsonSerializer.Serialize(schemaData, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return ComputeHash(json);
    }

    /// <summary>
    /// Computes SHA256 hash for a Table object
    /// </summary>
    public string ComputeTableHash(Table table)
    {
        var tableData = new
        {
            TableName = table.Name,
            SchemaName = table.SchemaName,
            Columns = table.Columns.Select(c => new
            {
                c.Name,
                c.DataType,
                c.MaxLength,
                c.Precision,
                c.Scale,
                c.IsNullable,
                c.DefaultValue,
                c.OrdinalPosition,
                c.IsIdentity,
                c.IsComputed
            }).OrderBy(c => c.OrdinalPosition)
        };

        var json = JsonSerializer.Serialize(tableData, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return ComputeHash(json);
    }

    /// <summary>
    /// Computes SHA256 hash for a string
    /// </summary>
    public string ComputeHash(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);

        return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
    }

    /// <summary>
    /// Compares two hashes to detect changes
    /// </summary>
    public bool HasChanged(string? currentHash, string? previousHash)
    {
        if (string.IsNullOrEmpty(previousHash))
        {
            return true; // First time, consider as changed
        }

        return !string.Equals(currentHash, previousHash, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Detects changes in a table by comparing hashes
    /// </summary>
    public ChangeDetectionResult DetectTableChanges(Table currentTable, string? previousHash)
    {
        var currentHash = ComputeTableHash(currentTable);
        var hasChanged = HasChanged(currentHash, previousHash);

        var result = new ChangeDetectionResult
        {
            TableName = currentTable.Name,
            CurrentHash = currentHash,
            PreviousHash = previousHash,
            HasChanged = hasChanged,
            DetectedAt = DateTime.UtcNow
        };

        if (hasChanged)
        {
            _logger.LogInformation("Schema change detected for table {TableName}", currentTable.Name);
            result.ChangeType = string.IsNullOrEmpty(previousHash) ? "New" : "Modified";
        }
        else
        {
            _logger.LogDebug("No changes detected for table {TableName}", currentTable.Name);
            result.ChangeType = "Unchanged";
        }

        return result;
    }

    /// <summary>
    /// Detects changes across multiple tables
    /// </summary>
    public IEnumerable<ChangeDetectionResult> DetectSchemaChanges(
        IEnumerable<Table> currentTables,
        Dictionary<string, string> previousHashes)
    {
        var results = new List<ChangeDetectionResult>();

        foreach (var table in currentTables)
        {
            previousHashes.TryGetValue(table.Name, out var previousHash);
            var result = DetectTableChanges(table, previousHash);
            results.Add(result);
        }

        return results;
    }
}

/// <summary>
/// Result of change detection for a table
/// </summary>
public class ChangeDetectionResult
{
    public string TableName { get; set; } = string.Empty;
    public string? CurrentHash { get; set; }
    public string? PreviousHash { get; set; }
    public bool HasChanged { get; set; }
    public string ChangeType { get; set; } = string.Empty; // New, Modified, Unchanged
    public DateTime DetectedAt { get; set; }
    public List<string> Changes { get; set; } = new();
}
