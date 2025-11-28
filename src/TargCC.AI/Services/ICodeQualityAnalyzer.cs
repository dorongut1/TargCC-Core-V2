// <copyright file="ICodeQualityAnalyzer.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

using TargCC.AI.Models.Quality;
using TargCC.Core.Interfaces.Models;

namespace TargCC.AI.Services;

/// <summary>
/// Interface for code quality analysis operations.
/// </summary>
public interface ICodeQualityAnalyzer
{
    /// <summary>
    /// Analyzes naming conventions in a table schema.
    /// </summary>
    /// <param name="table">The table to analyze for naming issues.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of naming convention issues found.</returns>
    Task<IReadOnlyList<NamingConventionIssue>> AnalyzeNamingConventionsAsync(
        Table table,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks for best practice violations in a table schema.
    /// </summary>
    /// <param name="table">The table to analyze for best practice violations.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of best practice violations found.</returns>
    Task<IReadOnlyList<BestPracticeViolation>> CheckBestPracticesAsync(
        Table table,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates relationships between tables.
    /// </summary>
    /// <param name="table">The table to analyze for relationship issues.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of relationship issues found.</returns>
    Task<IReadOnlyList<RelationshipIssue>> ValidateRelationshipsAsync(
        Table table,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a comprehensive code quality report for a table.
    /// </summary>
    /// <param name="table">The table to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A comprehensive code quality report.</returns>
    Task<CodeQualityReport> GenerateQualityReportAsync(
        Table table,
        CancellationToken cancellationToken = default);
}
