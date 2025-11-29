// <copyright file="AnalyzeResponse.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.WebAPI.Models.Responses;

/// <summary>
/// Response model for schema analysis operations.
/// </summary>
public sealed class AnalyzeResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether analysis was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the analysis type.
    /// </summary>
    public string AnalysisType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the analysis results.
    /// </summary>
    public object? Results { get; set; }

    /// <summary>
    /// Gets or sets any errors that occurred.
    /// </summary>
    public List<string> Errors { get; set; } = new();
}
