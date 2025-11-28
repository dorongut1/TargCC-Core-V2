// <copyright file="ISecurityScannerService.cs" company="Doron Aharoni">
// Copyright (c) Doron Aharoni. All rights reserved.
// </copyright>

using TargCC.AI.Models;
using TargCC.Core.Interfaces.Models;

namespace TargCC.AI.Services;

/// <summary>
/// Interface for security scanning operations on database schemas.
/// </summary>
public interface ISecurityScannerService
{
    /// <summary>
    /// Finds security vulnerabilities in a table schema.
    /// </summary>
    /// <param name="table">The table to scan for vulnerabilities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of security vulnerabilities found.</returns>
    Task<IReadOnlyList<SecurityVulnerability>> FindVulnerabilitiesAsync(
        Table table,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recommendations for TargCC column prefixes (eno_, ent_, clc_, etc.).
    /// </summary>
    /// <param name="table">The table to analyze for prefix recommendations.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of prefix recommendations.</returns>
    Task<IReadOnlyList<PrefixRecommendation>> GetPrefixRecommendationsAsync(
        Table table,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets suggestions for encrypting sensitive data.
    /// </summary>
    /// <param name="table">The table to analyze for encryption needs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of encryption suggestions.</returns>
    Task<IReadOnlyList<EncryptionSuggestion>> GetEncryptionSuggestionsAsync(
        Table table,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a comprehensive security report for a table.
    /// </summary>
    /// <param name="table">The table to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A security report containing all findings.</returns>
    Task<SecurityReport> GenerateSecurityReportAsync(
        Table table,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Scans multiple tables and generates a comprehensive security report.
    /// </summary>
    /// <param name="tables">The tables to scan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A comprehensive security report for all tables.</returns>
    Task<SecurityReport> ScanMultipleTablesAsync(
        IEnumerable<Table> tables,
        CancellationToken cancellationToken = default);
}
