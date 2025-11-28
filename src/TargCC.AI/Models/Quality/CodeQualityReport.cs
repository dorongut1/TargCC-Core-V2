// <copyright file="CodeQualityReport.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.AI.Models.Quality;

/// <summary>
/// Represents a comprehensive code quality report for a table.
/// </summary>
public class CodeQualityReport
{
    /// <summary>
    /// Gets the name of the analyzed table.
    /// </summary>
    public required string TableName { get; init; }

    /// <summary>
    /// Gets the schema name.
    /// </summary>
    public required string SchemaName { get; init; }

    /// <summary>
    /// Gets the timestamp when the analysis was performed.
    /// </summary>
    public required DateTime AnalyzedAt { get; init; }

    /// <summary>
    /// Gets the overall quality score (0-100).
    /// </summary>
    public required int QualityScore { get; init; }

    /// <summary>
    /// Gets the grade (A, B, C, D, F).
    /// </summary>
    public required string Grade { get; init; }

    /// <summary>
    /// Gets the naming convention issues.
    /// </summary>
    public required IReadOnlyList<NamingConventionIssue> NamingIssues { get; init; }

    /// <summary>
    /// Gets the best practice violations.
    /// </summary>
    public required IReadOnlyList<BestPracticeViolation> BestPracticeViolations { get; init; }

    /// <summary>
    /// Gets the relationship issues.
    /// </summary>
    public required IReadOnlyList<RelationshipIssue> RelationshipIssues { get; init; }

    /// <summary>
    /// Gets the summary of findings.
    /// </summary>
    public string? Summary { get; init; }

    /// <summary>
    /// Gets the total count of issues.
    /// </summary>
    public int TotalIssues => NamingIssues.Count + BestPracticeViolations.Count + RelationshipIssues.Count;

    /// <summary>
    /// Gets the count of critical issues.
    /// </summary>
    public int CriticalIssues => CountBySeverity("Critical");

    /// <summary>
    /// Counts issues by severity level.
    /// </summary>
    /// <param name="severity">The severity level to count.</param>
    /// <returns>Count of issues with the specified severity.</returns>
    private int CountBySeverity(string severity)
    {
        var namingCount = NamingIssues.Count(i => i.Severity == severity);
        var practiceCount = BestPracticeViolations.Count(v => v.Severity == severity);
        var relationshipCount = RelationshipIssues.Count(r => r.Severity == severity);
        return namingCount + practiceCount + relationshipCount;
    }
}
