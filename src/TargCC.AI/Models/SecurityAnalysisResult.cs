// <copyright file="SecurityAnalysisResult.cs" company="Doron Aharoni">
// Copyright (c) Doron Aharoni. All rights reserved.
// </copyright>

namespace TargCC.AI.Models;

/// <summary>
/// Represents the complete security analysis result for a table or database.
/// </summary>
public sealed class SecurityAnalysisResult
{
    /// <summary>
    /// Gets the list of security vulnerabilities found.
    /// </summary>
    public required List<SecurityVulnerability> Vulnerabilities { get; init; }

    /// <summary>
    /// Gets the list of TargCC prefix recommendations.
    /// </summary>
    public required List<PrefixRecommendation> PrefixRecommendations { get; init; }

    /// <summary>
    /// Gets the list of encryption suggestions.
    /// </summary>
    public required List<EncryptionSuggestion> EncryptionSuggestions { get; init; }

    /// <summary>
    /// Gets the overall security score.
    /// </summary>
    public required SecurityScore OverallScore { get; init; }

    /// <summary>
    /// Gets the name of the analyzed table (if single table analysis).
    /// </summary>
    public string? TableName { get; init; }

    /// <summary>
    /// Gets the timestamp when the analysis was performed.
    /// </summary>
    public required DateTime AnalyzedAt { get; init; }

    /// <summary>
    /// Gets the total number of issues found (all severities).
    /// </summary>
    public int TotalIssues =>
        Vulnerabilities.Count +
        PrefixRecommendations.Count +
        EncryptionSuggestions.Count;

    /// <summary>
    /// Gets a value indicating whether the analysis found any critical issues.
    /// </summary>
    public bool HasCriticalIssues =>
        Vulnerabilities.Any(v => v.Severity == SecuritySeverity.Critical) ||
        PrefixRecommendations.Any(p => p.Severity == SecuritySeverity.Critical) ||
        EncryptionSuggestions.Any(e => e.Severity == SecuritySeverity.Critical);
}
