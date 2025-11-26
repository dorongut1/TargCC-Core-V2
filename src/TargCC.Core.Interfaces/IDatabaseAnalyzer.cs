// Copyright (c) TargCC Team. All rights reserved.

using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Interfaces;

/// <summary>
/// Interface for database schema analysis.
/// </summary>
public interface IDatabaseAnalyzer
{
    /// <summary>
    /// Analyzes the complete database schema.
    /// </summary>
    /// <returns>Complete database schema.</returns>
    Task<DatabaseSchema> AnalyzeAsync();

    /// <summary>
    /// Analyzes a specific database schema using the provided connection string.
    /// </summary>
    /// <param name="connectionString">Database connection string.</param>
    /// <returns>Complete database schema.</returns>
    Task<DatabaseSchema> AnalyzeDatabaseAsync(string connectionString);
}
