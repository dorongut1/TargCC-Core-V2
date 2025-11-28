// <copyright file="CodeQualityAnalyzerService.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

using System.Text.Json;
using Microsoft.Extensions.Logging;
using TargCC.AI.Models;
using TargCC.AI.Models.Quality;
using TargCC.Core.Interfaces.Models;

namespace TargCC.AI.Services;

/// <summary>
/// Service for analyzing code quality and best practices.
/// </summary>
public partial class CodeQualityAnalyzerService : ICodeQualityAnalyzer
{
    private readonly IAIService _aiService;
    private readonly ILogger<CodeQualityAnalyzerService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeQualityAnalyzerService"/> class.
    /// </summary>
    /// <param name="aiService">The AI service.</param>
    /// <param name="logger">The logger.</param>
    public CodeQualityAnalyzerService(
        IAIService aiService,
        ILogger<CodeQualityAnalyzerService> logger)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NamingConventionIssue>> AnalyzeNamingConventionsAsync(
        Table table,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(table);

        LogAnalyzingNaming(table.Name);

        try
        {
            var prompt = BuildNamingConventionsPrompt(table);
            var request = new AIRequest(prompt, maxTokens: 2000, temperature: 0.3);
            var response = await _aiService.CompleteAsync(request, cancellationToken);

            if (!response.Success)
            {
                LogNamingAnalysisFailed(table.Name, response.ErrorMessage ?? "Unknown error");
                return Array.Empty<NamingConventionIssue>();
            }

            var issues = ParseNamingIssues(response.Content);
            LogNamingAnalysisComplete(table.Name, issues.Count);

            return issues;
        }
        catch (Exception ex)
        {
            LogNamingAnalysisError(table.Name, ex);
            return Array.Empty<NamingConventionIssue>();
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BestPracticeViolation>> CheckBestPracticesAsync(
        Table table,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(table);

        LogAnalyzingBestPractices(table.Name);

        try
        {
            var prompt = BuildBestPracticesPrompt(table);
            var request = new AIRequest(prompt, maxTokens: 2000, temperature: 0.3);
            var response = await _aiService.CompleteAsync(request, cancellationToken);

            if (!response.Success)
            {
                LogBestPracticesAnalysisFailed(table.Name, response.ErrorMessage ?? "Unknown error");
                return Array.Empty<BestPracticeViolation>();
            }

            var violations = ParseBestPracticeViolations(response.Content);
            LogBestPracticesAnalysisComplete(table.Name, violations.Count);

            return violations;
        }
        catch (Exception ex)
        {
            LogBestPracticesAnalysisError(table.Name, ex);
            return Array.Empty<BestPracticeViolation>();
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RelationshipIssue>> ValidateRelationshipsAsync(
        Table table,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(table);

        LogAnalyzingRelationships(table.Name);

        try
        {
            var prompt = BuildRelationshipsPrompt(table);
            var request = new AIRequest(prompt, maxTokens: 2000, temperature: 0.3);
            var response = await _aiService.CompleteAsync(request, cancellationToken);

            if (!response.Success)
            {
                LogRelationshipsAnalysisFailed(table.Name, response.ErrorMessage ?? "Unknown error");
                return Array.Empty<RelationshipIssue>();
            }

            var issues = ParseRelationshipIssues(response.Content);
            LogRelationshipsAnalysisComplete(table.Name, issues.Count);

            return issues;
        }
        catch (Exception ex)
        {
            LogRelationshipsAnalysisError(table.Name, ex);
            return Array.Empty<RelationshipIssue>();
        }
    }

    /// <inheritdoc/>
    public async Task<CodeQualityReport> GenerateQualityReportAsync(
        Table table,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(table);

        LogGeneratingQualityReport(table.Name);

        try
        {
            // Run all analyses in parallel
            var namingTask = AnalyzeNamingConventionsAsync(table, cancellationToken);
            var practicesTask = CheckBestPracticesAsync(table, cancellationToken);
            var relationshipsTask = ValidateRelationshipsAsync(table, cancellationToken);

            await Task.WhenAll(namingTask, practicesTask, relationshipsTask);

            var namingIssues = await namingTask;
            var practiceViolations = await practicesTask;
            var relationshipIssues = await relationshipsTask;

            var report = BuildQualityReport(table, namingIssues, practiceViolations, relationshipIssues);

            LogQualityReportComplete(table.Name, report.QualityScore, report.TotalIssues);

            return report;
        }
        catch (Exception ex)
        {
            LogQualityReportError(table.Name, ex);
            throw;
        }
    }

    // Private helper methods
    private static string BuildNamingConventionsPrompt(Table table)
    {
        var columnsList = string.Join(", ", table.Columns.Select(c => $"{c.Name} ({c.DataType})"));

        return $@"Analyze the following database table for naming convention issues:

Table: {table.Name}
Schema: {table.SchemaName}
Columns: {columnsList}

Check for:
1. PascalCase naming conventions
2. Appropriate use of TargCC prefixes (eno_, ent_, clc_)
3. Singular vs plural naming
4. Abbreviated names
5. Reserved keywords
6. Consistency with Clean Architecture naming

Return a JSON array of issues with this structure:
{{
  ""issues"": [
    {{
      ""elementName"": ""column_name"",
      ""elementType"": ""Column"",
      ""schemaName"": ""{table.SchemaName}"",
      ""issue"": ""Description of the issue"",
      ""recommendation"": ""How to fix it"",
      ""severity"": ""Medium"",
      ""example"": ""ColumnName""
    }}
  ]
}}

If no issues found, return: {{""issues"": []}}";
    }

    private static string BuildBestPracticesPrompt(Table table)
    {
        var columnsList = string.Join(", ", table.Columns.Select(c => $"{c.Name} ({c.DataType})"));
        var hasPrimaryKey = table.Columns.Any(c => c.IsPrimaryKey);

        return $@"Analyze the following database table for best practice violations:

Table: {table.Name}
Schema: {table.SchemaName}
Columns: {columnsList}
Has Primary Key: {hasPrimaryKey}

Check for violations in:
1. Architecture: Missing primary keys, audit columns, soft delete columns
2. Performance: Missing indexes on foreign keys, large varchar columns
3. Security: Unencrypted sensitive data, missing eno_ prefix
4. Maintainability: Too many columns, redundant data

Return a JSON array with this structure:
{{
  ""violations"": [
    {{
      ""category"": ""Architecture"",
      ""elementName"": ""{table.Name}"",
      ""violation"": ""Description"",
      ""recommendation"": ""How to fix"",
      ""severity"": ""High"",
      ""impact"": ""Impact description"",
      ""fixEffort"": ""Medium""
    }}
  ]
}}

If no violations found, return: {{""violations"": []}}";
    }

    private static string BuildRelationshipsPrompt(Table table)
    {
        var foreignKeyColumns = table.Columns.Where(c => c.IsForeignKey).Select(c => c.Name).ToList();
        var foreignKeysList = foreignKeyColumns.Any() ? string.Join(", ", foreignKeyColumns) : "None";

        return $@"Analyze the following database table for relationship issues:

Table: {table.Name}
Schema: {table.SchemaName}
Foreign Key Columns: {foreignKeysList}

Check for:
1. Missing foreign key constraints
2. Potential orphaned data
3. Circular reference risks
4. Missing indexes on foreign keys
5. Cascade delete concerns

Return a JSON array with this structure:
{{
  ""issues"": [
    {{
      ""sourceTable"": ""{table.Name}"",
      ""targetTable"": ""RelatedTable"",
      ""issueType"": ""MissingForeignKey"",
      ""description"": ""Description of the issue"",
      ""suggestion"": ""How to fix it"",
      ""severity"": ""Medium"",
      ""affectedColumns"": [""ColumnName""],
      ""isCascadeIssue"": false
    }}
  ]
}}

If no issues found, return: {{""issues"": []}}";
    }

    private static IReadOnlyList<NamingConventionIssue> ParseNamingIssues(string content)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(content);
            var issues = new List<NamingConventionIssue>();

            if (jsonDoc.RootElement.TryGetProperty("issues", out var issuesArray))
            {
                foreach (var issueElement in issuesArray.EnumerateArray())
                {
                    var issue = new NamingConventionIssue
                    {
                        ElementName = issueElement.GetProperty("elementName").GetString() ?? string.Empty,
                        ElementType = issueElement.GetProperty("elementType").GetString() ?? string.Empty,
                        SchemaName = issueElement.GetProperty("schemaName").GetString() ?? string.Empty,
                        Issue = issueElement.GetProperty("issue").GetString() ?? string.Empty,
                        Recommendation = issueElement.GetProperty("recommendation").GetString() ?? string.Empty,
                        Severity = issueElement.GetProperty("severity").GetString() ?? "Medium",
                        Example = issueElement.TryGetProperty("example", out var ex) ? ex.GetString() : null,
                    };
                    issues.Add(issue);
                }
            }

            return issues;
        }
        catch (JsonException)
        {
            return Array.Empty<NamingConventionIssue>();
        }
    }

    private static IReadOnlyList<BestPracticeViolation> ParseBestPracticeViolations(string content)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(content);
            var violations = new List<BestPracticeViolation>();

            if (jsonDoc.RootElement.TryGetProperty("violations", out var violationsArray))
            {
                foreach (var violationElement in violationsArray.EnumerateArray())
                {
                    var violation = new BestPracticeViolation
                    {
                        Category = violationElement.GetProperty("category").GetString() ?? string.Empty,
                        ElementName = violationElement.GetProperty("elementName").GetString() ?? string.Empty,
                        Violation = violationElement.GetProperty("violation").GetString() ?? string.Empty,
                        Recommendation = violationElement.GetProperty("recommendation").GetString() ?? string.Empty,
                        Severity = violationElement.GetProperty("severity").GetString() ?? "Medium",
                        Impact = violationElement.GetProperty("impact").GetString() ?? string.Empty,
                        FixEffort = violationElement.TryGetProperty("fixEffort", out var effort) ? effort.GetString() : null,
                        ReferenceUrl = violationElement.TryGetProperty("referenceUrl", out var url) ? url.GetString() : null,
                    };
                    violations.Add(violation);
                }
            }

            return violations;
        }
        catch (JsonException)
        {
            return Array.Empty<BestPracticeViolation>();
        }
    }

    private static IReadOnlyList<RelationshipIssue> ParseRelationshipIssues(string content)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(content);
            var issues = new List<RelationshipIssue>();

            if (jsonDoc.RootElement.TryGetProperty("issues", out var issuesArray))
            {
                foreach (var issueElement in issuesArray.EnumerateArray())
                {
                    var affectedColumns = new List<string>();
                    if (issueElement.TryGetProperty("affectedColumns", out var columnsArray))
                    {
                        foreach (var col in columnsArray.EnumerateArray())
                        {
                            if (col.GetString() is string colName)
                            {
                                affectedColumns.Add(colName);
                            }
                        }
                    }

                    var issue = new RelationshipIssue
                    {
                        SourceTable = issueElement.GetProperty("sourceTable").GetString() ?? string.Empty,
                        TargetTable = issueElement.TryGetProperty("targetTable", out var target) ? target.GetString() : null,
                        IssueType = issueElement.GetProperty("issueType").GetString() ?? string.Empty,
                        Description = issueElement.GetProperty("description").GetString() ?? string.Empty,
                        Suggestion = issueElement.GetProperty("suggestion").GetString() ?? string.Empty,
                        Severity = issueElement.GetProperty("severity").GetString() ?? "Medium",
                        AffectedColumns = affectedColumns.Count > 0 ? affectedColumns : null,
                        IsCascadeIssue = issueElement.TryGetProperty("isCascadeIssue", out var cascade) && cascade.GetBoolean(),
                    };
                    issues.Add(issue);
                }
            }

            return issues;
        }
        catch (JsonException)
        {
            return Array.Empty<RelationshipIssue>();
        }
    }

    private static CodeQualityReport BuildQualityReport(
        Table table,
        IReadOnlyList<NamingConventionIssue> namingIssues,
        IReadOnlyList<BestPracticeViolation> practiceViolations,
        IReadOnlyList<RelationshipIssue> relationshipIssues)
    {
        var totalIssues = namingIssues.Count + practiceViolations.Count + relationshipIssues.Count;

        // Calculate quality score (0-100)
        // Start with 100 and deduct points based on severity
        var score = 100;
        foreach (var issue in namingIssues)
        {
            score -= GetSeverityDeduction(issue.Severity);
        }

        foreach (var violation in practiceViolations)
        {
            score -= GetSeverityDeduction(violation.Severity);
        }

        foreach (var issue in relationshipIssues)
        {
            score -= GetSeverityDeduction(issue.Severity);
        }

        score = Math.Max(0, score); // Ensure score doesn't go below 0

        // Determine grade
        var grade = score switch
        {
            >= 90 => "A",
            >= 80 => "B",
            >= 70 => "C",
            >= 60 => "D",
            _ => "F",
        };

        var summary = totalIssues == 0
            ? $"Table {table.Name} has excellent code quality with no issues found."
            : $"Table {table.Name} has {totalIssues} issue(s) requiring attention.";

        return new CodeQualityReport
        {
            TableName = table.Name,
            SchemaName = table.SchemaName,
            AnalyzedAt = DateTime.UtcNow,
            QualityScore = score,
            Grade = grade,
            NamingIssues = namingIssues,
            BestPracticeViolations = practiceViolations,
            RelationshipIssues = relationshipIssues,
            Summary = summary,
        };
    }

    private static int GetSeverityDeduction(string severity)
    {
        return severity switch
        {
            "Critical" => 15,
            "High" => 10,
            "Medium" => 5,
            "Low" => 2,
            _ => 3,
        };
    }

    // Logging methods
    [LoggerMessage(
        EventId = 1801,
        Level = LogLevel.Information,
        Message = "Analyzing naming conventions for table: {TableName}")]
    private partial void LogAnalyzingNaming(string tableName);

    [LoggerMessage(
        EventId = 1802,
        Level = LogLevel.Information,
        Message = "Naming analysis complete for {TableName}: {IssueCount} issues found")]
    private partial void LogNamingAnalysisComplete(string tableName, int issueCount);

    [LoggerMessage(
        EventId = 1803,
        Level = LogLevel.Warning,
        Message = "Naming analysis failed for {TableName}: {Error}")]
    private partial void LogNamingAnalysisFailed(string tableName, string error);

    [LoggerMessage(
        EventId = 1804,
        Level = LogLevel.Error,
        Message = "Error analyzing naming for {TableName}")]
    private partial void LogNamingAnalysisError(string tableName, Exception ex);

    [LoggerMessage(
        EventId = 1805,
        Level = LogLevel.Information,
        Message = "Analyzing best practices for table: {TableName}")]
    private partial void LogAnalyzingBestPractices(string tableName);

    [LoggerMessage(
        EventId = 1806,
        Level = LogLevel.Information,
        Message = "Best practices analysis complete for {TableName}: {ViolationCount} violations found")]
    private partial void LogBestPracticesAnalysisComplete(string tableName, int violationCount);

    [LoggerMessage(
        EventId = 1807,
        Level = LogLevel.Warning,
        Message = "Best practices analysis failed for {TableName}: {Error}")]
    private partial void LogBestPracticesAnalysisFailed(string tableName, string error);

    [LoggerMessage(
        EventId = 1808,
        Level = LogLevel.Error,
        Message = "Error analyzing best practices for {TableName}")]
    private partial void LogBestPracticesAnalysisError(string tableName, Exception ex);

    [LoggerMessage(
        EventId = 1809,
        Level = LogLevel.Information,
        Message = "Analyzing relationships for table: {TableName}")]
    private partial void LogAnalyzingRelationships(string tableName);

    [LoggerMessage(
        EventId = 1810,
        Level = LogLevel.Information,
        Message = "Relationships analysis complete for {TableName}: {IssueCount} issues found")]
    private partial void LogRelationshipsAnalysisComplete(string tableName, int issueCount);

    [LoggerMessage(
        EventId = 1811,
        Level = LogLevel.Warning,
        Message = "Relationships analysis failed for {TableName}: {Error}")]
    private partial void LogRelationshipsAnalysisFailed(string tableName, string error);

    [LoggerMessage(
        EventId = 1812,
        Level = LogLevel.Error,
        Message = "Error analyzing relationships for {TableName}")]
    private partial void LogRelationshipsAnalysisError(string tableName, Exception ex);

    [LoggerMessage(
        EventId = 1813,
        Level = LogLevel.Information,
        Message = "Generating quality report for table: {TableName}")]
    private partial void LogGeneratingQualityReport(string tableName);

    [LoggerMessage(
        EventId = 1814,
        Level = LogLevel.Information,
        Message = "Quality report complete for {TableName}: Score {QualityScore}, Total issues {TotalIssues}")]
    private partial void LogQualityReportComplete(string tableName, int qualityScore, int totalIssues);

    [LoggerMessage(
        EventId = 1815,
        Level = LogLevel.Error,
        Message = "Error generating quality report for {TableName}")]
    private partial void LogQualityReportError(string tableName, Exception ex);
}
