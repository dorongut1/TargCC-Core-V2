// <copyright file="Suggestion.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.AI.Models;

/// <summary>
/// Represents a suggestion from AI analysis.
/// </summary>
public class Suggestion
{
    /// <summary>
    /// Gets or sets the severity of the suggestion.
    /// </summary>
    public SuggestionSeverity Severity { get; set; }

    /// <summary>
    /// Gets or sets the category of the suggestion.
    /// </summary>
    public SuggestionCategory Category { get; set; }

    /// <summary>
    /// Gets or sets the message describing the suggestion.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target (table/column name) this suggestion applies to.
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// Gets or sets the recommended action or fix.
    /// </summary>
    public string? RecommendedAction { get; set; }

    /// <summary>
    /// Gets or sets additional context or reasoning.
    /// </summary>
    public string? Context { get; set; }
}
