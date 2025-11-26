// <copyright file="ISchemaChangeDetector.cs" company="Doron">
// Copyright (c) Doron. All rights reserved.
// </copyright>

using TargCC.Core.Analyzers.Models;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Analyzers;

/// <summary>
/// Interface for detecting changes in database schema.
/// </summary>
public interface ISchemaChangeDetector
{
    /// <summary>
    /// Detects changes between current database schema and saved snapshot.
    /// </summary>
    /// <param name="connectionString">Database connection string.</param>
    /// <param name="snapshotPath">Path to the schema snapshot file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Detected schema changes.</returns>
    Task<SchemaChanges> DetectChangesAsync(
        string connectionString,
        string snapshotPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves current database schema as a snapshot.
    /// </summary>
    /// <param name="connectionString">Database connection string.</param>
    /// <param name="snapshotPath">Path where to save the snapshot file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveSchemaSnapshotAsync(
        string connectionString,
        string snapshotPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a database schema snapshot from file.
    /// </summary>
    /// <param name="snapshotPath">Path to the snapshot file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded database schema.</returns>
    Task<DatabaseSchema?> LoadSchemaSnapshotAsync(
        string snapshotPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current database schema.
    /// </summary>
    /// <param name="connectionString">Database connection string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current database schema.</returns>
    Task<DatabaseSchema> GetCurrentSchemaAsync(
        string connectionString,
        CancellationToken cancellationToken = default);
}
