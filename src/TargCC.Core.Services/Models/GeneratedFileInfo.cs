// <copyright file="GeneratedFileInfo.cs" company="Doron">
// Copyright (c) Doron. All rights reserved.
// </copyright>

namespace TargCC.Core.Services.Models;

/// <summary>
/// Represents information about a generated file for tracking purposes.
/// </summary>
public class GeneratedFileInfo
{
    /// <summary>
    /// Gets or sets the name of the table this file was generated from.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full path to the generated file.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of file (Entity, Repository, SQL, CQRS, API, etc.).
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when this file was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Gets or sets the hash of the table schema at the time of generation.
    /// Used to detect if schema has changed since generation.
    /// </summary>
    public string SchemaHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this is a manual code file (.prt).
    /// Manual files should never be auto-regenerated.
    /// </summary>
    public bool IsManual { get; set; }
}
