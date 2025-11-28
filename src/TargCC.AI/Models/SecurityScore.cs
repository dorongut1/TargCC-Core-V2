// <copyright file="SecurityScore.cs" company="Doron Aharoni">
// Copyright (c) Doron Aharoni. All rights reserved.
// </copyright>

namespace TargCC.AI.Models;

/// <summary>
/// Represents an overall security score for a database schema.
/// </summary>
public sealed class SecurityScore
{
    /// <summary>
    /// Gets the overall score (0-100, where 100 is perfect security).
    /// </summary>
    public required int Score { get; init; }

    /// <summary>
    /// Gets the security grade (A, B, C, D, F).
    /// </summary>
    public required string Grade { get; init; }

    /// <summary>
    /// Gets the number of critical vulnerabilities found.
    /// </summary>
    public required int CriticalCount { get; init; }

    /// <summary>
    /// Gets the number of high severity issues found.
    /// </summary>
    public required int HighCount { get; init; }

    /// <summary>
    /// Gets the number of medium severity issues found.
    /// </summary>
    public required int MediumCount { get; init; }

    /// <summary>
    /// Gets the number of low severity issues found.
    /// </summary>
    public required int LowCount { get; init; }

    /// <summary>
    /// Gets the total number of issues found.
    /// </summary>
    public int TotalIssues => CriticalCount + HighCount + MediumCount + LowCount;

    /// <summary>
    /// Gets a summary message about the security posture.
    /// </summary>
    public string? Summary { get; init; }
}
