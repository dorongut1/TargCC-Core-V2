// <copyright file="RelationshipIssue.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.AI.Models.Quality;

/// <summary>
/// Represents a relationship issue between tables.
/// </summary>
public class RelationshipIssue
{
    /// <summary>
    /// Gets the name of the source table.
    /// </summary>
    public required string SourceTable { get; init; }

    /// <summary>
    /// Gets the name of the target table (if applicable).
    /// </summary>
    public string? TargetTable { get; init; }

    /// <summary>
    /// Gets the type of issue (MissingForeignKey, OrphanedData, CircularReference, etc.).
    /// </summary>
    public required string IssueType { get; init; }

    /// <summary>
    /// Gets the description of the issue.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the suggested solution.
    /// </summary>
    public required string Suggestion { get; init; }

    /// <summary>
    /// Gets the severity level (Critical, High, Medium, Low).
    /// </summary>
    public required string Severity { get; init; }

    /// <summary>
    /// Gets the column names involved in the issue.
    /// </summary>
    public IReadOnlyList<string>? AffectedColumns { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is a potential cascade delete issue.
    /// </summary>
    public bool IsCascadeIssue { get; init; }
}
