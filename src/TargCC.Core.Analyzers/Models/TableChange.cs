// <copyright file="TableChange.cs" company="Doron">
// Copyright (c) Doron. All rights reserved.
// </copyright>

using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Analyzers.Models;

/// <summary>
/// Represents a change detected in a database table.
/// </summary>
public class TableChange
{
    /// <summary>
    /// Gets or sets the type of change.
    /// </summary>
    public ChangeType Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the table that changed.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the old table metadata (null if table was added).
    /// </summary>
    public Table? OldMetadata { get; set; }

    /// <summary>
    /// Gets or sets the new table metadata (null if table was removed).
    /// </summary>
    public Table? NewMetadata { get; set; }

    /// <summary>
    /// Gets or sets a description of the change.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
