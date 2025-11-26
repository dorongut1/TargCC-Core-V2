// <copyright file="RelationshipChange.cs" company="Doron">
// Copyright (c) Doron. All rights reserved.
// </copyright>

using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Analyzers.Models;

/// <summary>
/// Represents a change detected in a database relationship (foreign key).
/// </summary>
public class RelationshipChange
{
    /// <summary>
    /// Gets or sets the type of change.
    /// </summary>
    public ChangeType Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the source table.
    /// </summary>
    public string SourceTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the target table.
    /// </summary>
    public string TargetTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the old relationship metadata (null if relationship was added).
    /// </summary>
    public Relationship? OldMetadata { get; set; }

    /// <summary>
    /// Gets or sets the new relationship metadata (null if relationship was removed).
    /// </summary>
    public Relationship? NewMetadata { get; set; }

    /// <summary>
    /// Gets or sets a description of the change.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
