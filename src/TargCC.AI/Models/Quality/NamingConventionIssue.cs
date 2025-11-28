// <copyright file="NamingConventionIssue.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.AI.Models.Quality;

/// <summary>
/// Represents a naming convention issue found in database schema.
/// </summary>
public class NamingConventionIssue
{
    /// <summary>
    /// Gets the name of the element with the issue.
    /// </summary>
    public required string ElementName { get; init; }

    /// <summary>
    /// Gets the type of element (Table, Column, Index, etc.).
    /// </summary>
    public required string ElementType { get; init; }

    /// <summary>
    /// Gets the schema name where the element belongs.
    /// </summary>
    public required string SchemaName { get; init; }

    /// <summary>
    /// Gets the description of the naming issue.
    /// </summary>
    public required string Issue { get; init; }

    /// <summary>
    /// Gets the recommended naming convention.
    /// </summary>
    public required string Recommendation { get; init; }

    /// <summary>
    /// Gets the severity level (Critical, High, Medium, Low).
    /// </summary>
    public required string Severity { get; init; }

    /// <summary>
    /// Gets the example of correct naming.
    /// </summary>
    public string? Example { get; init; }
}
