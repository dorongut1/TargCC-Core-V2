// <copyright file="SecurityScannerService.cs" company="Doron Aharoni">
// Copyright (c) Doron Aharoni. All rights reserved.
// </copyright>

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TargCC.AI.Models;
using TargCC.Core.Interfaces.Models;

namespace TargCC.AI.Services;

/// <summary>
/// Service for security scanning operations on database schemas.
/// </summary>
public sealed class SecurityScannerService : ISecurityScannerService
{
    private readonly IAIService aiService;
    private readonly ILogger<SecurityScannerService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityScannerService"/> class.
    /// </summary>
    /// <param name="aiService">The AI service.</param>
    /// <param name="logger">The logger.</param>
    public SecurityScannerService(
        IAIService aiService,
        ILogger<SecurityScannerService> logger)
    {
        this.aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SecurityVulnerability>> FindVulnerabilitiesAsync(
        Table table,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(table);

        this.logger.LogInformation("Finding security vulnerabilities for table {TableName}", table.Name);

        var prompt = this.BuildVulnerabilityPrompt(table);
        var request = new AIRequest(prompt, maxTokens: 2000, temperature: 0.3);
        var response = await this.aiService.CompleteAsync(request, cancellationToken);

        var vulnerabilities = this.ParseVulnerabilities(response.Content, table.Name);

        this.logger.LogInformation(
            "Found {Count} vulnerabilities for table {TableName}",
            vulnerabilities.Count,
            table.Name);

        return vulnerabilities;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PrefixRecommendation>> GetPrefixRecommendationsAsync(
        Table table,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(table);

        this.logger.LogInformation("Getting prefix recommendations for table {TableName}", table.Name);

        var prompt = this.BuildPrefixPrompt(table);
        var request = new AIRequest(prompt, maxTokens: 2000, temperature: 0.3);
        var response = await this.aiService.CompleteAsync(request, cancellationToken);

        var recommendations = this.ParsePrefixRecommendations(response.Content, table.Name);

        this.logger.LogInformation(
            "Found {Count} prefix recommendations for table {TableName}",
            recommendations.Count,
            table.Name);

        return recommendations;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EncryptionSuggestion>> GetEncryptionSuggestionsAsync(
        Table table,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(table);

        this.logger.LogInformation("Getting encryption suggestions for table {TableName}", table.Name);

        var prompt = this.BuildEncryptionPrompt(table);
        var request = new AIRequest(prompt, maxTokens: 2000, temperature: 0.3);
        var response = await this.aiService.CompleteAsync(request, cancellationToken);

        var suggestions = this.ParseEncryptionSuggestions(response.Content, table.Name);

        this.logger.LogInformation(
            "Found {Count} encryption suggestions for table {TableName}",
            suggestions.Count,
            table.Name);

        return suggestions;
    }

    /// <inheritdoc/>
    public async Task<SecurityReport> GenerateSecurityReportAsync(
        Table table,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(table);

        this.logger.LogInformation("Generating security report for table {TableName}", table.Name);

        // Run all analyses in parallel
        var vulnerabilitiesTask = this.FindVulnerabilitiesAsync(table, cancellationToken);
        var recommendationsTask = this.GetPrefixRecommendationsAsync(table, cancellationToken);
        var suggestionsTask = this.GetEncryptionSuggestionsAsync(table, cancellationToken);

        await Task.WhenAll(vulnerabilitiesTask, recommendationsTask, suggestionsTask);

        var vulnerabilities = await vulnerabilitiesTask;
        var recommendations = await recommendationsTask;
        var suggestions = await suggestionsTask;

        var report = new SecurityReport
        {
            Scope = table.Name,
            GeneratedAt = DateTime.UtcNow,
            Vulnerabilities = vulnerabilities,
            PrefixRecommendations = recommendations,
            EncryptionSuggestions = suggestions,
            SecurityScore = this.CalculateSecurityScore(vulnerabilities, recommendations, suggestions),
            Summary = this.BuildSummary(vulnerabilities, recommendations, suggestions),
        };

        this.logger.LogInformation(
            "Generated security report for table {TableName}: Score={Score}, Issues={Total}",
            table.Name,
            report.SecurityScore,
            report.TotalIssuesCount);

        return report;
    }

    /// <inheritdoc/>
    public async Task<SecurityReport> ScanMultipleTablesAsync(
        IEnumerable<Table> tables,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        var tableList = tables.ToList();
        if (tableList.Count == 0)
        {
            throw new ArgumentException("At least one table must be provided", nameof(tables));
        }

        this.logger.LogInformation("Scanning {Count} tables for security issues", tableList.Count);

        // Generate reports for all tables in parallel
        var reportTasks = tableList.Select(t => this.GenerateSecurityReportAsync(t, cancellationToken));
        var reports = await Task.WhenAll(reportTasks);

        // Combine all reports
        var allVulnerabilities = reports.SelectMany(r => r.Vulnerabilities).ToList();
        var allRecommendations = reports.SelectMany(r => r.PrefixRecommendations).ToList();
        var allSuggestions = reports.SelectMany(r => r.EncryptionSuggestions).ToList();

        var combinedReport = new SecurityReport
        {
            Scope = $"{tableList.Count} tables",
            GeneratedAt = DateTime.UtcNow,
            Vulnerabilities = allVulnerabilities,
            PrefixRecommendations = allRecommendations,
            EncryptionSuggestions = allSuggestions,
            SecurityScore = this.CalculateSecurityScore(allVulnerabilities, allRecommendations, allSuggestions),
            Summary = this.BuildSummary(allVulnerabilities, allRecommendations, allSuggestions),
        };

        this.logger.LogInformation(
            "Completed scan of {Count} tables: Score={Score}, Total Issues={Total}",
            tableList.Count,
            combinedReport.SecurityScore,
            combinedReport.TotalIssuesCount);

        return combinedReport;
    }

    private string BuildVulnerabilityPrompt(Table table)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Analyze the following database table for security vulnerabilities:");
        sb.AppendLine();
        sb.AppendLine($"Table: {table.Name}");
        sb.AppendLine("Columns:");

        foreach (var column in table.Columns)
        {
            sb.AppendLine($"  - {column.Name} ({column.DataType}){(column.IsNullable ? " NULL" : " NOT NULL")}");
        }

        sb.AppendLine();
        sb.AppendLine("Focus on:");
        sb.AppendLine("1. Columns storing passwords or credentials without encryption");
        sb.AppendLine("2. Sensitive data (SSN, credit cards, etc.) stored in plain text");
        sb.AppendLine("3. Personal information without protection");
        sb.AppendLine("4. Weak or outdated encryption methods");
        sb.AppendLine();
        sb.AppendLine("Return findings in JSON format:");
        sb.AppendLine("[{");
        sb.AppendLine("  \"vulnerabilityType\": \"type\",");
        sb.AppendLine("  \"severity\": \"Critical|High|Medium|Low\",");
        sb.AppendLine("  \"description\": \"description\",");
        sb.AppendLine("  \"columnName\": \"column\",");
        sb.AppendLine("  \"recommendation\": \"how to fix\"");
        sb.AppendLine("}]");

        return sb.ToString();
    }

    private string BuildPrefixPrompt(Table table)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Analyze the following table for TargCC column prefix recommendations:");
        sb.AppendLine();
        sb.AppendLine($"Table: {table.Name}");
        sb.AppendLine("Columns:");

        foreach (var column in table.Columns)
        {
            sb.AppendLine($"  - {column.Name} ({column.DataType})");
        }

        sb.AppendLine();
        sb.AppendLine("TargCC prefix conventions:");
        sb.AppendLine("- eno_: Encrypted columns (sensitive data)");
        sb.AppendLine("- ent_: Temporal columns (date ranges, history tracking)");
        sb.AppendLine("- clc_: Calculated columns (computed values)");
        sb.AppendLine("- idx_: Indexed columns (for performance)");
        sb.AppendLine();
        sb.AppendLine("Return recommendations in JSON format:");
        sb.AppendLine("[{");
        sb.AppendLine("  \"currentColumnName\": \"current name\",");
        sb.AppendLine("  \"recommendedPrefix\": \"prefix\",");
        sb.AppendLine("  \"recommendedColumnName\": \"new name with prefix\",");
        sb.AppendLine("  \"reason\": \"why this prefix\",");
        sb.AppendLine("  \"severity\": \"High|Medium|Low\"");
        sb.AppendLine("}]");

        return sb.ToString();
    }

    private string BuildEncryptionPrompt(Table table)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Analyze the following table for encryption requirements:");
        sb.AppendLine();
        sb.AppendLine($"Table: {table.Name}");
        sb.AppendLine("Columns:");

        foreach (var column in table.Columns)
        {
            sb.AppendLine($"  - {column.Name} ({column.DataType})");
        }

        sb.AppendLine();
        sb.AppendLine("Identify columns that should be encrypted and suggest methods:");
        sb.AppendLine("Return suggestions in JSON format:");
        sb.AppendLine("[{");
        sb.AppendLine("  \"columnName\": \"column\",");
        sb.AppendLine("  \"sensitiveDataType\": \"type (SSN, CreditCard, etc.)\",");
        sb.AppendLine("  \"recommendedEncryptionMethod\": \"method (AES256, etc.)\",");
        sb.AppendLine("  \"reason\": \"why encrypt\",");
        sb.AppendLine("  \"severity\": \"High|Medium|Low\",");
        sb.AppendLine("  \"recommendedColumnName\": \"eno_columnName (optional)\"");
        sb.AppendLine("}]");

        return sb.ToString();
    }

    private IReadOnlyList<SecurityVulnerability> ParseVulnerabilities(string content, string tableName)
    {
        try
        {
            // Try to extract JSON array from response
            var jsonStart = content.IndexOf('[');
            var jsonEnd = content.LastIndexOf(']');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var items = JsonSerializer.Deserialize<List<VulnerabilityDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });

                if (items != null)
                {
                    return items.Select(dto => new SecurityVulnerability
                    {
                        VulnerabilityType = dto.VulnerabilityType ?? "Unknown",
                        Severity = this.ParseSeverity(dto.Severity),
                        Description = dto.Description ?? string.Empty,
                        ColumnName = dto.ColumnName ?? string.Empty,
                        TableName = tableName,
                        Recommendation = dto.Recommendation ?? string.Empty,
                    }).ToList();
                }
            }
        }
        catch (JsonException ex)
        {
            this.logger.LogWarning(ex, "Failed to parse vulnerabilities JSON, returning empty list");
        }

        return Array.Empty<SecurityVulnerability>();
    }

    private IReadOnlyList<PrefixRecommendation> ParsePrefixRecommendations(string content, string tableName)
    {
        try
        {
            var jsonStart = content.IndexOf('[');
            var jsonEnd = content.LastIndexOf(']');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var items = JsonSerializer.Deserialize<List<PrefixDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });

                if (items != null)
                {
                    return items.Select(dto => new PrefixRecommendation
                    {
                        CurrentColumnName = dto.CurrentColumnName ?? string.Empty,
                        TableName = tableName,
                        RecommendedPrefix = dto.RecommendedPrefix ?? string.Empty,
                        RecommendedColumnName = dto.RecommendedColumnName ?? string.Empty,
                        Reason = dto.Reason ?? string.Empty,
                        Severity = this.ParseSeverity(dto.Severity),
                    }).ToList();
                }
            }
        }
        catch (JsonException ex)
        {
            this.logger.LogWarning(ex, "Failed to parse prefix recommendations JSON, returning empty list");
        }

        return Array.Empty<PrefixRecommendation>();
    }

    private IReadOnlyList<EncryptionSuggestion> ParseEncryptionSuggestions(string content, string tableName)
    {
        try
        {
            var jsonStart = content.IndexOf('[');
            var jsonEnd = content.LastIndexOf(']');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var items = JsonSerializer.Deserialize<List<EncryptionDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });

                if (items != null)
                {
                    return items.Select(dto => new EncryptionSuggestion
                    {
                        ColumnName = dto.ColumnName ?? string.Empty,
                        TableName = tableName,
                        SensitiveDataType = dto.SensitiveDataType ?? string.Empty,
                        RecommendedEncryptionMethod = dto.RecommendedEncryptionMethod ?? string.Empty,
                        Reason = dto.Reason ?? string.Empty,
                        Severity = this.ParseSeverity(dto.Severity),
                        RecommendedColumnName = dto.RecommendedColumnName,
                    }).ToList();
                }
            }
        }
        catch (JsonException ex)
        {
            this.logger.LogWarning(ex, "Failed to parse encryption suggestions JSON, returning empty list");
        }

        return Array.Empty<EncryptionSuggestion>();
    }

    private SecuritySeverity ParseSeverity(string? severity)
    {
        return severity?.ToUpperInvariant() switch
        {
            "CRITICAL" => SecuritySeverity.Critical,
            "HIGH" => SecuritySeverity.High,
            "MEDIUM" => SecuritySeverity.Medium,
            "LOW" => SecuritySeverity.Low,
            _ => SecuritySeverity.Low,
        };
    }

    private int CalculateSecurityScore(
        IReadOnlyList<SecurityVulnerability> vulnerabilities,
        IReadOnlyList<PrefixRecommendation> recommendations,
        IReadOnlyList<EncryptionSuggestion> suggestions)
    {
        // Start with perfect score
        var score = 100;

        // Deduct points for each issue based on severity
        score -= vulnerabilities.Count(v => v.Severity == SecuritySeverity.Critical) * 20;
        score -= vulnerabilities.Count(v => v.Severity == SecuritySeverity.High) * 10;
        score -= vulnerabilities.Count(v => v.Severity == SecuritySeverity.Medium) * 5;
        score -= vulnerabilities.Count(v => v.Severity == SecuritySeverity.Low) * 2;

        score -= recommendations.Count(r => r.Severity == SecuritySeverity.High) * 8;
        score -= recommendations.Count(r => r.Severity == SecuritySeverity.Medium) * 4;
        score -= recommendations.Count(r => r.Severity == SecuritySeverity.Low) * 1;

        score -= suggestions.Count(s => s.Severity == SecuritySeverity.High) * 8;
        score -= suggestions.Count(s => s.Severity == SecuritySeverity.Medium) * 4;
        score -= suggestions.Count(s => s.Severity == SecuritySeverity.Low) * 1;

        return Math.Max(0, score); // Never go below 0
    }

    private string BuildSummary(
        IReadOnlyList<SecurityVulnerability> vulnerabilities,
        IReadOnlyList<PrefixRecommendation> recommendations,
        IReadOnlyList<EncryptionSuggestion> suggestions)
    {
        var sb = new StringBuilder();

        var criticalCount = vulnerabilities.Count(v => v.Severity == SecuritySeverity.Critical);
        var highCount = vulnerabilities.Count(v => v.Severity == SecuritySeverity.High) +
                       recommendations.Count(r => r.Severity == SecuritySeverity.High) +
                       suggestions.Count(s => s.Severity == SecuritySeverity.High);

        if (criticalCount > 0)
        {
            sb.AppendLine($"⚠️  {criticalCount} CRITICAL vulnerabilities found!");
        }

        if (highCount > 0)
        {
            sb.AppendLine($"🔴 {highCount} HIGH severity issues");
        }

        sb.AppendLine($"Found {vulnerabilities.Count} vulnerabilities, {recommendations.Count} prefix recommendations, and {suggestions.Count} encryption suggestions.");

        return sb.ToString().Trim();
    }

    // DTOs for JSON deserialization
    private sealed class VulnerabilityDto
    {
        public string? VulnerabilityType { get; set; }

        public string? Severity { get; set; }

        public string? Description { get; set; }

        public string? ColumnName { get; set; }

        public string? Recommendation { get; set; }
    }

    private sealed class PrefixDto
    {
        public string? CurrentColumnName { get; set; }

        public string? RecommendedPrefix { get; set; }

        public string? RecommendedColumnName { get; set; }

        public string? Reason { get; set; }

        public string? Severity { get; set; }
    }

    private sealed class EncryptionDto
    {
        public string? ColumnName { get; set; }

        public string? SensitiveDataType { get; set; }

        public string? RecommendedEncryptionMethod { get; set; }

        public string? Reason { get; set; }

        public string? Severity { get; set; }

        public string? RecommendedColumnName { get; set; }
    }
}
