// <copyright file="BestPracticeViolation.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.AI.Models.Quality;

/// <summary>
/// Represents a best practice violation in the generated code or schema.
/// </summary>
public class BestPracticeViolation
{
    /// <summary>
    /// Gets the category of the violation (Architecture, Performance, Security, Maintainability).
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Gets the name of the table or component with the violation.
    /// </summary>
    public required string ElementName { get; init; }

    /// <summary>
    /// Gets the description of the violation.
    /// </summary>
    public required string Violation { get; init; }

    /// <summary>
    /// Gets the recommended fix or improvement.
    /// </summary>
    public required string Recommendation { get; init; }

    /// <summary>
    /// Gets the severity level (Critical, High, Medium, Low).
    /// </summary>
    public required string Severity { get; init; }

    /// <summary>
    /// Gets the impact of the violation.
    /// </summary>
    public required string Impact { get; init; }

    /// <summary>
    /// Gets the estimated effort to fix (Low, Medium, High).
    /// </summary>
    public string? FixEffort { get; init; }

    /// <summary>
    /// Gets the reference documentation URL.
    /// </summary>
    public string? ReferenceUrl { get; init; }
}
