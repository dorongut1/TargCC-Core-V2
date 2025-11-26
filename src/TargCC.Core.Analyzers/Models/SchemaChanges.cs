// <copyright file="SchemaChanges.cs" company="Doron">
// Copyright (c) Doron. All rights reserved.
// </copyright>

namespace TargCC.Core.Analyzers.Models;

/// <summary>
/// Represents all changes detected in a database schema.
/// </summary>
public class SchemaChanges
{
    /// <summary>
    /// Gets or sets the list of table changes.
    /// </summary>
    public List<TableChange> TableChanges { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of column changes.
    /// </summary>
    public List<ColumnChange> ColumnChanges { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of index changes.
    /// </summary>
    public List<IndexChange> IndexChanges { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of relationship changes.
    /// </summary>
    public List<RelationshipChange> RelationshipChanges { get; set; } = new();

    /// <summary>
    /// Gets a value indicating whether any changes were detected.
    /// </summary>
    public bool HasChanges =>
        TableChanges.Count > 0 ||
        ColumnChanges.Count > 0 ||
        IndexChanges.Count > 0 ||
        RelationshipChanges.Count > 0;

    /// <summary>
    /// Gets the total number of changes detected.
    /// </summary>
    public int TotalChanges =>
        TableChanges.Count +
        ColumnChanges.Count +
        IndexChanges.Count +
        RelationshipChanges.Count;
}
