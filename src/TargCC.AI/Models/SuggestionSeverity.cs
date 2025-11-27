// <copyright file="SuggestionSeverity.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.AI.Models;

/// <summary>
/// Severity level for suggestions.
/// </summary>
public enum SuggestionSeverity
{
    /// <summary>
    /// Informational suggestion.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Best practice suggestion.
    /// </summary>
    BestPractice = 1,

    /// <summary>
    /// Warning about potential issues.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Critical issue that should be addressed.
    /// </summary>
    Critical = 3,
}
