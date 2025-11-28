// <copyright file="PrefixRecommendation.cs" company="Doron Aharoni">
// Copyright (c) Doron Aharoni. All rights reserved.
// </copyright>

namespace TargCC.AI.Models;

/// <summary>
/// Represents a recommendation to add or change a TargCC prefix for a column.
/// </summary>
public sealed class PrefixRecommendation
{
    /// <summary>
    /// Gets the current column name.
    /// </summary>
    public required string CurrentColumnName { get; init; }

    /// <summary>
    /// Gets the name of the table containing the column.
    /// </summary>
    public required string TableName { get; init; }

    /// <summary>
    /// Gets the recommended TargCC prefix (e.g., "eno_", "clc_", "blg_").
    /// </summary>
    public required string RecommendedPrefix { get; init; }

    /// <summary>
    /// Gets the recommended new column name with prefix.
    /// </summary>
    public required string RecommendedColumnName { get; init; }

    /// <summary>
    /// Gets the reason for this recommendation.
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Gets the severity/importance of this recommendation.
    /// </summary>
    public required SecuritySeverity Severity { get; init; }

    /// <summary>
    /// Gets additional context or explanation.
    /// </summary>
    public string? AdditionalContext { get; init; }
}
