// <copyright file="TableGenerationInfo.cs" company="Doron">
// Copyright (c) Doron. All rights reserved.
// </copyright>

namespace TargCC.Core.Services.Models;

/// <summary>
/// Represents generation tracking information for a specific table.
/// </summary>
public class TableGenerationInfo
{
    /// <summary>
    /// Gets or sets the timestamp of the last generation for this table.
    /// </summary>
    public DateTime LastGenerated { get; set; }

    /// <summary>
    /// Gets or sets the hash of the table schema at the last generation.
    /// </summary>
    public string SchemaHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of files generated for this table.
    /// </summary>
    public List<GeneratedFileInfo> Files { get; set; } = new();
}
