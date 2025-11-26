// <copyright file="IndexChange.cs" company="Doron">
// Copyright (c) Doron. All rights reserved.
// </copyright>

using TargCC.Core.Interfaces.Models;
using IndexMetadata = TargCC.Core.Interfaces.Models.Index;

namespace TargCC.Core.Analyzers.Models;

/// <summary>
/// Represents a change detected in a database index.
/// </summary>
public class IndexChange
{
    /// <summary>
    /// Gets or sets the type of change.
    /// </summary>
    public ChangeType Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the table containing this index.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the index that changed.
    /// </summary>
    public string IndexName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the old index metadata (null if index was added).
    /// </summary>
    public IndexMetadata? OldMetadata { get; set; }

    /// <summary>
    /// Gets or sets the new index metadata (null if index was removed).
    /// </summary>
    public IndexMetadata? NewMetadata { get; set; }

    /// <summary>
    /// Gets or sets a description of the change.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
