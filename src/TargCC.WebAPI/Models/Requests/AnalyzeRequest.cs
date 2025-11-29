// <copyright file="AnalyzeRequest.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.WebAPI.Models.Requests;

/// <summary>
/// Request model for schema analysis operations.
/// </summary>
public sealed class AnalyzeRequest
{
    /// <summary>
    /// Gets or sets the table name to analyze.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the connection string to the database.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the analysis type (Security, Quality, Schema).
    /// </summary>
    public string AnalysisType { get; set; } = "Security";
}
