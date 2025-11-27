// <copyright file="SchemaAnalysisResult.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.AI.Models;

/// <summary>
/// Result of AI schema analysis.
/// </summary>
public class SchemaAnalysisResult
{
    /// <summary>
    /// Gets or sets the name of the analyzed table.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the overall assessment summary.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of suggestions.
    /// </summary>
    public List<Suggestion> Suggestions { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of strengths found in the schema.
    /// </summary>
    public List<string> Strengths { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of issues found in the schema.
    /// </summary>
    public List<string> Issues { get; set; } = new();

    /// <summary>
    /// Gets or sets the overall quality score (0-100).
    /// </summary>
    public int QualityScore { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the schema follows TargCC conventions.
    /// </summary>
    public bool FollowsTargCCConventions { get; set; }

    /// <summary>
    /// Gets a value indicating whether there are critical issues.
    /// </summary>
    public bool HasCriticalIssues => Suggestions.Any(s => s.Severity == SuggestionSeverity.Critical);

    /// <summary>
    /// Gets a value indicating whether there are warnings.
    /// </summary>
    public bool HasWarnings => Suggestions.Any(s => s.Severity == SuggestionSeverity.Warning);
}
