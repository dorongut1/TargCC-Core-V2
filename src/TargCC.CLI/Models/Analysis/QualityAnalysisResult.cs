// <copyright file="QualityAnalysisResult.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

namespace TargCC.CLI.Models.Analysis;

/// <summary>
/// Result of quality analysis.
/// </summary>
public class QualityAnalysisResult
{
    /// <summary>
    /// Gets or sets the naming conventions score.
    /// </summary>
    public QualityScore NamingConventions { get; set; } = new();

    /// <summary>
    /// Gets or sets the relationships score.
    /// </summary>
    public QualityScore Relationships { get; set; } = new();

    /// <summary>
    /// Gets or sets the index coverage score.
    /// </summary>
    public QualityScore IndexCoverage { get; set; } = new();

    /// <summary>
    /// Gets or sets the overall score (0-100).
    /// </summary>
    public int OverallScore { get; set; }

    /// <summary>
    /// Gets or sets the overall grade (A+, A, B+, etc.).
    /// </summary>
    public string OverallGrade { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of issues found.
    /// </summary>
    public List<QualityIssue> Issues { get; set; } = new();
}

/// <summary>
/// Quality score for a specific category.
/// </summary>
public class QualityScore
{
    /// <summary>
    /// Gets or sets the score percentage.
    /// </summary>
    public int Percentage { get; set; }

    /// <summary>
    /// Gets or sets the passed count.
    /// </summary>
    public int Passed { get; set; }

    /// <summary>
    /// Gets or sets the total count.
    /// </summary>
    public int Total { get; set; }
}

/// <summary>
/// Quality issue found during analysis.
/// </summary>
public class QualityIssue
{
    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recommendation.
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;
}
