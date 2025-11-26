// <copyright file="IGenerationTracker.cs" company="Doron">
// Copyright (c) Doron. All rights reserved.
// </copyright>

using TargCC.Core.Services.Models;

namespace TargCC.Core.Services;

/// <summary>
/// Interface for tracking generated files and determining if regeneration is needed.
/// </summary>
public interface IGenerationTracker
{
    /// <summary>
    /// Tracks a generated file for future reference.
    /// </summary>
    /// <param name="fileInfo">Information about the generated file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task TrackGeneratedFileAsync(
        GeneratedFileInfo fileInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file needs to be regenerated based on schema changes.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="fileType">Type of file to check.</param>
    /// <param name="currentSchemaHash">Current hash of the table schema.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the file needs regeneration, false otherwise.</returns>
    Task<bool> NeedsRegenerationAsync(
        string tableName,
        string fileType,
        string currentSchemaHash,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the last generation time for a table.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Last generation timestamp, or null if never generated.</returns>
    Task<DateTime?> GetLastGenerationTimeAsync(
        string tableName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all tracking information for a specific table.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearTrackingAsync(
        string tableName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all generated files for a table.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of generated file information.</returns>
    Task<List<GeneratedFileInfo>> GetGeneratedFilesAsync(
        string tableName,
        CancellationToken cancellationToken = default);
}
