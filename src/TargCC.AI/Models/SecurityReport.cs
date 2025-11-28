// <copyright file="SecurityReport.cs" company="Doron Aharoni">
// Copyright (c) Doron Aharoni. All rights reserved.
// </copyright>

namespace TargCC.AI.Models;

/// <summary>
/// Represents a comprehensive security report for database schema analysis.
/// </summary>
public sealed class SecurityReport
{
    /// <summary>
    /// Gets the name of the table or scope that was analyzed.
    /// </summary>
    public required string Scope { get; init; }

    /// <summary>
    /// Gets the timestamp when the report was generated.
    /// </summary>
    public required DateTime GeneratedAt { get; init; }

    /// <summary>
    /// Gets the list of security vulnerabilities found.
    /// </summary>
    public required IReadOnlyList<SecurityVulnerability> Vulnerabilities { get; init; }

    /// <summary>
    /// Gets the list of TargCC prefix recommendations.
    /// </summary>
    public required IReadOnlyList<PrefixRecommendation> PrefixRecommendations { get; init; }

    /// <summary>
    /// Gets the list of encryption suggestions.
    /// </summary>
    public required IReadOnlyList<EncryptionSuggestion> EncryptionSuggestions { get; init; }

    /// <summary>
    /// Gets the overall security score (0-100, where 100 is most secure).
    /// </summary>
    public required int SecurityScore { get; init; }

    /// <summary>
    /// Gets a summary of the report findings.
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// Gets the total count of critical issues.
    /// </summary>
    public int CriticalIssuesCount => Vulnerabilities.Count(v => v.Severity == SecuritySeverity.Critical);

    /// <summary>
    /// Gets the total count of high severity issues.
    /// </summary>
    public int HighSeverityCount =>
        Vulnerabilities.Count(v => v.Severity == SecuritySeverity.High) +
        PrefixRecommendations.Count(p => p.Severity == SecuritySeverity.High) +
        EncryptionSuggestions.Count(e => e.Severity == SecuritySeverity.High);

    /// <summary>
    /// Gets the total count of medium severity issues.
    /// </summary>
    public int MediumSeverityCount =>
        Vulnerabilities.Count(v => v.Severity == SecuritySeverity.Medium) +
        PrefixRecommendations.Count(p => p.Severity == SecuritySeverity.Medium) +
        EncryptionSuggestions.Count(e => e.Severity == SecuritySeverity.Medium);

    /// <summary>
    /// Gets the total count of low severity issues.
    /// </summary>
    public int LowSeverityCount =>
        Vulnerabilities.Count(v => v.Severity == SecuritySeverity.Low) +
        PrefixRecommendations.Count(p => p.Severity == SecuritySeverity.Low) +
        EncryptionSuggestions.Count(e => e.Severity == SecuritySeverity.Low);

    /// <summary>
    /// Gets the total count of all issues.
    /// </summary>
    public int TotalIssuesCount =>
        Vulnerabilities.Count + PrefixRecommendations.Count + EncryptionSuggestions.Count;
}
