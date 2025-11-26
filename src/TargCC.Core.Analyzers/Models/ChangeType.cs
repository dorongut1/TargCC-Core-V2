// <copyright file="ChangeType.cs" company="Doron">
// Copyright (c) Doron. All rights reserved.
// </copyright>

namespace TargCC.Core.Analyzers.Models;

/// <summary>
/// Represents the type of change detected in database schema.
/// </summary>
public enum ChangeType
{
    /// <summary>
    /// An item was added (new table, column, index, etc.).
    /// </summary>
    Added,

    /// <summary>
    /// An item was removed (deleted table, column, index, etc.).
    /// </summary>
    Removed,

    /// <summary>
    /// An existing item was modified (column type changed, etc.).
    /// </summary>
    Modified,
}
