// <copyright file="IAnalysisService.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using TargCC.CLI.Models.Analysis;

namespace TargCC.CLI.Services.Analysis;

/// <summary>
/// Service for analyzing database schemas and code impact.
/// </summary>
public interface IAnalysisService
{
    /// <summary>
    /// Analyzes the database schema.
    /// </summary>
    /// <param name="tableName">Optional table name to analyze. If null, analyzes all tables.</param>
    /// <returns>Schema analysis result.</returns>
    Task<SchemaAnalysisResult> AnalyzeSchemaAsync(string? tableName = null);

    /// <summary>
    /// Analyzes the impact of schema changes.
    /// </summary>
    /// <param name="tableName">Table name.</param>
    /// <param name="changeType">Type of change.</param>
    /// <param name="columnName">Column name affected.</param>
    /// <param name="newValue">New value (type, length, etc.).</param>
    /// <returns>Impact analysis result.</returns>
    Task<ImpactAnalysisResult> AnalyzeImpactAsync(
        string tableName,
        string changeType,
        string? columnName = null,
        string? newValue = null);

    /// <summary>
    /// Analyzes security issues in the schema.
    /// </summary>
    /// <returns>Security analysis result.</returns>
    Task<SecurityAnalysisResult> AnalyzeSecurityAsync();

    /// <summary>
    /// Analyzes schema quality.
    /// </summary>
    /// <returns>Quality analysis result.</returns>
    Task<QualityAnalysisResult> AnalyzeQualityAsync();
}
