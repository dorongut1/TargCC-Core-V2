// <copyright file="SuggestionCategory.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.AI.Models;

/// <summary>
/// Category for suggestions.
/// </summary>
public enum SuggestionCategory
{
    /// <summary>
    /// General suggestions.
    /// </summary>
    General = 0,

    /// <summary>
    /// Security-related suggestions.
    /// </summary>
    Security = 1,

    /// <summary>
    /// Performance-related suggestions.
    /// </summary>
    Performance = 2,

    /// <summary>
    /// Naming convention suggestions.
    /// </summary>
    Naming = 3,

    /// <summary>
    /// Relationship and foreign key suggestions.
    /// </summary>
    Relationships = 4,

    /// <summary>
    /// Indexing suggestions.
    /// </summary>
    Indexing = 5,

    /// <summary>
    /// Data type suggestions.
    /// </summary>
    DataType = 6,

    /// <summary>
    /// TargCC-specific prefix suggestions (eno_, ent_, etc.).
    /// </summary>
    TargCCConventions = 7,
}
