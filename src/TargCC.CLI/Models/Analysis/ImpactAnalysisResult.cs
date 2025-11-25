// <copyright file="ImpactAnalysisResult.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

namespace TargCC.CLI.Models.Analysis;

/// <summary>
/// Result of impact analysis.
/// </summary>
public class ImpactAnalysisResult
{
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the change type.
    /// </summary>
    public string ChangeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the affected files.
    /// </summary>
    public List<AffectedFile> AffectedFiles { get; set; } = new();

    /// <summary>
    /// Gets or sets the manual code files that need review.
    /// </summary>
    public List<string> ManualCodeFiles { get; set; } = new();

    /// <summary>
    /// Gets or sets the estimated fix time in minutes.
    /// </summary>
    public int EstimatedFixTimeMinutes { get; set; }
}

/// <summary>
/// Information about an affected file.
/// </summary>
public class AffectedFile
{
    /// <summary>
    /// Gets or sets the file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the impact description.
    /// </summary>
    public string Impact { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the file needs manual review.
    /// </summary>
    public bool NeedsManualReview { get; set; }
}
