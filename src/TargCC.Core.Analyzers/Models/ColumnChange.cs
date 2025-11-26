// <copyright file="ColumnChange.cs" company="Doron">
// Copyright (c) Doron. All rights reserved.
// </copyright>

using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Analyzers.Models;

/// <summary>
/// Represents a change detected in a database column.
/// </summary>
public class ColumnChange
{
    /// <summary>
    /// Gets or sets the type of change.
    /// </summary>
    public ChangeType Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the table containing this column.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the column that changed.
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the old column metadata (null if column was added).
    /// </summary>
    public Column? OldMetadata { get; set; }

    /// <summary>
    /// Gets or sets the new column metadata (null if column was removed).
    /// </summary>
    public Column? NewMetadata { get; set; }

    /// <summary>
    /// Gets or sets a description of the change.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
