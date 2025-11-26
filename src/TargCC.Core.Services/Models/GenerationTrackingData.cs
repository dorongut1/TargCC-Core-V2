// <copyright file="GenerationTrackingData.cs" company="Doron">
// Copyright (c) Doron. All rights reserved.
// </copyright>

namespace TargCC.Core.Services.Models;

/// <summary>
/// Represents the complete generation tracking data structure.
/// This is serialized to .targcc/generated.json.
/// </summary>
public class GenerationTrackingData
{
    /// <summary>
    /// Gets or sets the dictionary of tables and their generation information.
    /// Key is the table name, value is the generation info.
    /// </summary>
    public Dictionary<string, TableGenerationInfo> Tables { get; set; } = new();
}
