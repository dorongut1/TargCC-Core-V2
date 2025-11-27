// <copyright file="GenerationTracker.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TargCC.Core.Services.Models;

namespace TargCC.Core.Services;

/// <summary>
/// Implementation of generation tracking service.
/// </summary>
public class GenerationTracker : IGenerationTracker
{
    private readonly ILogger<GenerationTracker> _logger;
    private readonly string _trackingFilePath;
    private GenerationTrackingData? _cachedData;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerationTracker"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="trackingDirectory">Directory where tracking file is stored (default: .targcc).</param>
    public GenerationTracker(
        ILogger<GenerationTracker> logger,
        string? trackingDirectory = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var directory = trackingDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), ".targcc");
        _trackingFilePath = Path.Combine(directory, "generated.json");
    }

    /// <inheritdoc/>
    public async Task TrackGeneratedFileAsync(
        GeneratedFileInfo fileInfo,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Tracking generated file: {FilePath}", fileInfo.FilePath);

        var data = await LoadTrackingDataAsync(cancellationToken);

        if (!data.Tables.ContainsKey(fileInfo.TableName))
        {
            data.Tables[fileInfo.TableName] = new TableGenerationInfo
            {
                LastGenerated = fileInfo.GeneratedAt,
                SchemaHash = fileInfo.SchemaHash,
                Files = new List<GeneratedFileInfo>(),
            };
        }

        var tableInfo = data.Tables[fileInfo.TableName];
        tableInfo.LastGenerated = fileInfo.GeneratedAt;
        tableInfo.SchemaHash = fileInfo.SchemaHash;

        // Remove existing entry for this file if it exists
        tableInfo.Files.RemoveAll(f => f.FilePath == fileInfo.FilePath);

        // Add the new entry
        tableInfo.Files.Add(fileInfo);

        await SaveTrackingDataAsync(data, cancellationToken);

        _logger.LogInformation(
            "Tracked file: {FilePath} for table {TableName}",
            fileInfo.FilePath,
            fileInfo.TableName);
    }

    /// <inheritdoc/>
    public async Task<bool> NeedsRegenerationAsync(
        string tableName,
        string fileType,
        string currentSchemaHash,
        CancellationToken cancellationToken = default)
    {
        var data = await LoadTrackingDataAsync(cancellationToken);

        // If table never generated, needs generation
        if (!data.Tables.ContainsKey(tableName))
        {
            _logger.LogDebug("Table {TableName} never generated - needs generation", tableName);
            return true;
        }

        var tableInfo = data.Tables[tableName];

        // If schema hash changed, needs regeneration
        if (tableInfo.SchemaHash != currentSchemaHash)
        {
            _logger.LogDebug("Schema changed for {TableName} - needs regeneration", tableName);
            return true;
        }

        // Check if file of this type exists
        var fileInfo = tableInfo.Files.FirstOrDefault(f => f.FileType == fileType);
        if (fileInfo == null)
        {
            _logger.LogDebug(
                "File type {FileType} not found for {TableName} - needs generation",
                fileType,
                tableName);
            return true;
        }

        // Check if file still exists on disk
        if (!File.Exists(fileInfo.FilePath))
        {
            _logger.LogDebug("File {FilePath} not found on disk - needs regeneration", fileInfo.FilePath);
            return true;
        }

        // Manual files should never be regenerated
        if (fileInfo.IsManual)
        {
            _logger.LogDebug("File {FilePath} is manual (.prt) - skipping regeneration", fileInfo.FilePath);
            return false;
        }

        _logger.LogDebug("File {FilePath} is up to date - no regeneration needed", fileInfo.FilePath);
        return false;
    }

    /// <inheritdoc/>
    public async Task<DateTime?> GetLastGenerationTimeAsync(
        string tableName,
        CancellationToken cancellationToken = default)
    {
        var data = await LoadTrackingDataAsync(cancellationToken);

        if (data.Tables.TryGetValue(tableName, out var tableInfo))
        {
            return tableInfo.LastGenerated;
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task ClearTrackingAsync(
        string tableName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Clearing tracking for table: {TableName}", tableName);

        var data = await LoadTrackingDataAsync(cancellationToken);
        data.Tables.Remove(tableName);

        await SaveTrackingDataAsync(data, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<GeneratedFileInfo>> GetGeneratedFilesAsync(
        string tableName,
        CancellationToken cancellationToken = default)
    {
        var data = await LoadTrackingDataAsync(cancellationToken);

        if (data.Tables.TryGetValue(tableName, out var tableInfo))
        {
            return tableInfo.Files;
        }

        return new List<GeneratedFileInfo>();
    }

    private async Task<GenerationTrackingData> LoadTrackingDataAsync(CancellationToken cancellationToken)
    {
        if (_cachedData != null)
        {
            return _cachedData;
        }

        if (!File.Exists(_trackingFilePath))
        {
            _logger.LogDebug("Tracking file not found, creating new: {Path}", _trackingFilePath);
            _cachedData = new GenerationTrackingData();
            return _cachedData;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_trackingFilePath, cancellationToken);
            _cachedData = JsonSerializer.Deserialize<GenerationTrackingData>(json)
                ?? new GenerationTrackingData();

            _logger.LogDebug("Loaded tracking data: {TableCount} tables", _cachedData.Tables.Count);
            return _cachedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tracking data from {Path}", _trackingFilePath);
            _cachedData = new GenerationTrackingData();
            return _cachedData;
        }
    }

    private async Task SaveTrackingDataAsync(
        GenerationTrackingData data,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_trackingFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        var json = JsonSerializer.Serialize(data, options);
        await File.WriteAllTextAsync(_trackingFilePath, json, cancellationToken);

        _cachedData = data;

        _logger.LogDebug("Saved tracking data to {Path}", _trackingFilePath);
    }
}
