// Copyright (c) TargCC Team. All rights reserved.

namespace TargCC.Core.Services.AI.Models;

/// <summary>
/// Represents a single code change made by AI.
/// </summary>
public sealed class CodeChange
{
    /// <summary>
    /// Gets or sets the line number where the change occurred.
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the type of change.
    /// </summary>
    public CodeChangeType Type { get; set; }

    /// <summary>
    /// Gets or sets a description of the change.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the old value (before change).
    /// </summary>
    public string OldValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new value (after change).
    /// </summary>
    public string NewValue { get; set; } = string.Empty;
}

/// <summary>
/// Types of code changes.
/// </summary>
public enum CodeChangeType
{
    /// <summary>
    /// A line was added.
    /// </summary>
    Addition,

    /// <summary>
    /// A line was deleted.
    /// </summary>
    Deletion,

    /// <summary>
    /// A line was modified.
    /// </summary>
    Modification,
}
