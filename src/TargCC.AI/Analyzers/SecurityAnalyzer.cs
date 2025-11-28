// <copyright file="SecurityAnalyzer.cs" company="Doron Aharoni">
// Copyright (c) Doron Aharoni. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TargCC.AI.Models;
using TargCC.Core.Interfaces.Models;

namespace TargCC.AI.Analyzers;

/// <summary>
/// Provides security analysis for database schemas, detecting vulnerabilities,
/// recommending TargCC prefixes, and suggesting encryption for sensitive data.
/// </summary>
public sealed class SecurityAnalyzer
{
    // Sensitive data patterns for detection
    private static readonly Dictionary<string, string> SensitivePatterns = new()
    {
        { "ssn", "Social Security Number" },
        { "socialsecurity", "Social Security Number" },
        { "creditcard", "Credit Card Number" },
        { "ccnumber", "Credit Card Number" },
        { "cardnumber", "Credit Card Number" },
        { "bankaccount", "Bank Account Number" },
        { "password", "Password" },
        { "pwd", "Password" },
        { "secret", "Secret Data" },
        { "taxid", "Tax ID Number" },
        { "passport", "Passport Number" },
        { "drivinglicense", "Driver's License" },
        { "dob", "Date of Birth" },
        { "birthdate", "Date of Birth" },
        { "salary", "Salary Information" },
        { "income", "Income Information" },
        { "medical", "Medical Information" },
        { "health", "Health Information" },
        { "diagnosis", "Medical Diagnosis" },
    };

    // TargCC prefix definitions
    private static readonly Dictionary<string, string> TargCCPrefixes = new()
    {
        { "eno_", "One-way encryption (SHA256) - for passwords and non-retrievable data" },
        { "ent_", "Two-way encryption (AES-256) - for retrievable sensitive data" },
        { "clc_", "Calculated column - read-only, computed from other columns" },
        { "blg_", "Business logic - server-side only, requires UpdateFriend method" },
        { "agg_", "Aggregate column - counters, requires UpdateAggregates method" },
        { "spt_", "Separate update - different permissions, dedicated update method" },
    };

    private readonly ILogger<SecurityAnalyzer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityAnalyzer"/> class.
    /// </summary>
    /// <param name="logger">The logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public SecurityAnalyzer(ILogger<SecurityAnalyzer> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Analyzes a table for security vulnerabilities, prefix recommendations, and encryption suggestions.
    /// </summary>
    /// <param name="table">The table to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A complete security analysis result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when table is null.</exception>
    public Task<SecurityAnalysisResult> AnalyzeTableSecurityAsync(
        Table table,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(table);

        this.logger.LogInformation("Starting security analysis for table: {TableName}", table.Name);

        var vulnerabilities = this.DetectVulnerabilities(table);
        var prefixRecommendations = this.GetPrefixRecommendations(table);
        var encryptionSuggestions = this.GetEncryptionSuggestions(table);
        var securityScore = this.CalculateSecurityScore(vulnerabilities, prefixRecommendations, encryptionSuggestions);

        var result = new SecurityAnalysisResult
        {
            Vulnerabilities = vulnerabilities,
            PrefixRecommendations = prefixRecommendations,
            EncryptionSuggestions = encryptionSuggestions,
            OverallScore = securityScore,
            TableName = table.Name,
            AnalyzedAt = DateTime.UtcNow,
        };

        this.logger.LogInformation(
            "Security analysis completed for {TableName}: {IssueCount} total issues found",
            table.Name,
            result.TotalIssues);

        return Task.FromResult(result);
    }

    /// <summary>
    /// Detects security vulnerabilities in a table.
    /// </summary>
    /// <param name="table">The table to analyze.</param>
    /// <returns>List of detected vulnerabilities.</returns>
    public List<SecurityVulnerability> DetectVulnerabilities(Table table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var vulnerabilities = new List<SecurityVulnerability>();

        foreach (var column in table.Columns)
        {
            // Check for plain text passwords
            if (IsPasswordColumn(column) && !HasEncryptionPrefix(column))
            {
                vulnerabilities.Add(new SecurityVulnerability
                {
                    VulnerabilityType = "PlainTextPassword",
                    Severity = SecuritySeverity.Critical,
                    Description = $"Column '{column.Name}' appears to store passwords in plain text",
                    ColumnName = column.Name,
                    TableName = table.Name,
                    Recommendation = "Use 'eno_' prefix for one-way encryption (SHA256) for passwords",
                    AdditionalContext = "Passwords should never be stored in plain text. Use SHA256 hashing with salt.",
                });
            }

            // Check for unencrypted sensitive data
            var sensitiveType = GetSensitiveDataType(column);
            if (sensitiveType != null && !HasEncryptionPrefix(column))
            {
                var severity = sensitiveType switch
                {
                    "Social Security Number" or "Credit Card Number" or "Bank Account Number" => SecuritySeverity.Critical,
                    "Password" or "Tax ID Number" or "Passport Number" => SecuritySeverity.Critical,
                    _ => SecuritySeverity.High,
                };

                vulnerabilities.Add(new SecurityVulnerability
                {
                    VulnerabilityType = "UnencryptedSensitiveData",
                    Severity = severity,
                    Description = $"Column '{column.Name}' contains sensitive data ({sensitiveType}) without encryption",
                    ColumnName = column.Name,
                    TableName = table.Name,
                    Recommendation = $"Use 'ent_' prefix for two-way encryption to protect {sensitiveType.ToLower()}",
                    AdditionalContext = "Sensitive data should be encrypted at rest using AES-256 encryption.",
                });
            }
        }

        return vulnerabilities;
    }

    /// <summary>
    /// Gets TargCC prefix recommendations for columns that need them.
    /// </summary>
    /// <param name="table">The table to analyze.</param>
    /// <returns>List of prefix recommendations.</returns>
    public List<PrefixRecommendation> GetPrefixRecommendations(Table table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var recommendations = new List<PrefixRecommendation>();

        foreach (var column in table.Columns)
        {
            // Skip columns that already have prefixes
            if (HasAnyTargCCPrefix(column))
            {
                continue;
            }

            // Recommend eno_ for passwords
            if (IsPasswordColumn(column))
            {
                recommendations.Add(new PrefixRecommendation
                {
                    CurrentColumnName = column.Name,
                    TableName = table.Name,
                    RecommendedPrefix = "eno_",
                    RecommendedColumnName = $"eno_{column.Name}",
                    Reason = "Password column should use one-way encryption (SHA256)",
                    Severity = SecuritySeverity.Critical,
                    AdditionalContext = "eno_ prefix ensures passwords are hashed, not stored in plain text.",
                });
            }

            // Recommend ent_ for retrievable sensitive data
            var sensitiveType = GetSensitiveDataType(column);
            if (sensitiveType != null && !IsPasswordColumn(column))
            {
                recommendations.Add(new PrefixRecommendation
                {
                    CurrentColumnName = column.Name,
                    TableName = table.Name,
                    RecommendedPrefix = "ent_",
                    RecommendedColumnName = $"ent_{column.Name}",
                    Reason = $"Sensitive data ({sensitiveType}) should use two-way encryption",
                    Severity = SecuritySeverity.Critical,
                    AdditionalContext = "ent_ prefix enables AES-256 encryption for data that needs to be decrypted.",
                });
            }

            // Recommend clc_ for calculated columns
            if (column.IsComputed)
            {
                recommendations.Add(new PrefixRecommendation
                {
                    CurrentColumnName = column.Name,
                    TableName = table.Name,
                    RecommendedPrefix = "clc_",
                    RecommendedColumnName = $"clc_{column.Name}",
                    Reason = "Computed columns should use 'clc_' prefix to mark them as read-only",
                    Severity = SecuritySeverity.Low,
                    AdditionalContext = "clc_ prefix prevents calculated columns from appearing in Update methods.",
                });
            }

            // Recommend blg_ for boolean flags that look like business logic
            if (IsBooleanColumn(column) && LooksLikeBusinessLogic(column))
            {
                recommendations.Add(new PrefixRecommendation
                {
                    CurrentColumnName = column.Name,
                    TableName = table.Name,
                    RecommendedPrefix = "blg_",
                    RecommendedColumnName = $"blg_{column.Name}",
                    Reason = "Business logic flag should use 'blg_' prefix for server-side updates only",
                    Severity = SecuritySeverity.Medium,
                    AdditionalContext = "blg_ prefix creates UpdateFriend method for server-side business logic updates.",
                });
            }
        }

        return recommendations;
    }

    /// <summary>
    /// Gets encryption suggestions for columns containing sensitive data.
    /// </summary>
    /// <param name="table">The table to analyze.</param>
    /// <returns>List of encryption suggestions.</returns>
    public List<EncryptionSuggestion> GetEncryptionSuggestions(Table table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var suggestions = new List<EncryptionSuggestion>();

        foreach (var column in table.Columns)
        {
            // Skip columns that already have encryption
            if (HasEncryptionPrefix(column))
            {
                continue;
            }

            // Suggest encryption for passwords
            if (IsPasswordColumn(column))
            {
                suggestions.Add(new EncryptionSuggestion
                {
                    ColumnName = column.Name,
                    TableName = table.Name,
                    SensitiveDataType = "Password",
                    RecommendedEncryptionMethod = "SHA256 One-Way Hash",
                    Reason = "Passwords should be hashed, not encrypted, so they cannot be retrieved",
                    Severity = SecuritySeverity.Critical,
                    RecommendedColumnName = $"eno_{column.Name}",
                    ImplementationNotes = "Use SHA256 with salt. TargCC will generate hashing code automatically with eno_ prefix.",
                });
            }

            // Suggest encryption for other sensitive data
            var sensitiveType = GetSensitiveDataType(column);
            if (sensitiveType != null && !IsPasswordColumn(column))
            {
                suggestions.Add(new EncryptionSuggestion
                {
                    ColumnName = column.Name,
                    TableName = table.Name,
                    SensitiveDataType = sensitiveType,
                    RecommendedEncryptionMethod = "AES-256 Two-Way Encryption",
                    Reason = $"{sensitiveType} must be protected with encryption",
                    Severity = SecuritySeverity.Critical,
                    RecommendedColumnName = $"ent_{column.Name}",
                    ImplementationNotes = "Use AES-256 encryption. TargCC will generate encryption/decryption code with ent_ prefix.",
                });
            }
        }

        return suggestions;
    }

    /// <summary>
    /// Calculates an overall security score based on findings.
    /// </summary>
    /// <param name="vulnerabilities">Detected vulnerabilities.</param>
    /// <param name="prefixRecommendations">Prefix recommendations.</param>
    /// <param name="encryptionSuggestions">Encryption suggestions.</param>
    /// <returns>An overall security score.</returns>
    private SecurityScore CalculateSecurityScore(
        List<SecurityVulnerability> vulnerabilities,
        List<PrefixRecommendation> prefixRecommendations,
        List<EncryptionSuggestion> encryptionSuggestions)
    {
        // Count issues by severity
        var criticalCount = CountBySeverity(vulnerabilities, prefixRecommendations, encryptionSuggestions, SecuritySeverity.Critical);
        var highCount = CountBySeverity(vulnerabilities, prefixRecommendations, encryptionSuggestions, SecuritySeverity.High);
        var mediumCount = CountBySeverity(vulnerabilities, prefixRecommendations, encryptionSuggestions, SecuritySeverity.Medium);
        var lowCount = CountBySeverity(vulnerabilities, prefixRecommendations, encryptionSuggestions, SecuritySeverity.Low);

        // Calculate score (100 = perfect, 0 = terrible)
        // Critical issues: -25 points each
        // High issues: -10 points each
        // Medium issues: -5 points each
        // Low issues: -2 points each
        var score = 100 - (criticalCount * 25) - (highCount * 10) - (mediumCount * 5) - (lowCount * 2);
        score = Math.Max(0, score); // Minimum score is 0

        // Determine grade
        var grade = score switch
        {
            >= 90 => "A",
            >= 80 => "B",
            >= 70 => "C",
            >= 60 => "D",
            _ => "F",
        };

        // Generate summary
        var totalIssues = criticalCount + highCount + mediumCount + lowCount;
        var summary = totalIssues == 0
            ? "Excellent security posture with no issues detected."
            : $"Found {totalIssues} security issue(s). {criticalCount} critical, {highCount} high, {mediumCount} medium, {lowCount} low.";

        return new SecurityScore
        {
            Score = score,
            Grade = grade,
            CriticalCount = criticalCount,
            HighCount = highCount,
            MediumCount = mediumCount,
            LowCount = lowCount,
            Summary = summary,
        };
    }

    /// <summary>
    /// Counts issues by severity across all issue types.
    /// </summary>
    private int CountBySeverity(
        List<SecurityVulnerability> vulnerabilities,
        List<PrefixRecommendation> prefixRecommendations,
        List<EncryptionSuggestion> encryptionSuggestions,
        SecuritySeverity severity)
    {
        var count = vulnerabilities.Count(v => v.Severity == severity);
        count += prefixRecommendations.Count(p => p.Severity == severity);
        count += encryptionSuggestions.Count(e => e.Severity == severity);
        return count;
    }

    /// <summary>
    /// Checks if a column is a password column based on its name.
    /// </summary>
    private bool IsPasswordColumn(Column column)
    {
        var name = column.Name.ToLowerInvariant();
        return name.Contains("password") || name.Contains("pwd") || name == "pass";
    }

    /// <summary>
    /// Checks if a column is a boolean column.
    /// </summary>
    private bool IsBooleanColumn(Column column)
    {
        return column.DataType.ToLowerInvariant() is "bit" or "bool" or "boolean";
    }

    /// <summary>
    /// Checks if a boolean column looks like it contains business logic.
    /// </summary>
    private bool LooksLikeBusinessLogic(Column column)
    {
        var name = column.Name.ToLowerInvariant();
        return name.Contains("approved") ||
               name.Contains("verified") ||
               name.Contains("confirmed") ||
               name.Contains("locked") ||
               name.Contains("blocked") ||
               name.Contains("suspended");
    }

    /// <summary>
    /// Gets the type of sensitive data if the column contains sensitive information.
    /// </summary>
    private string? GetSensitiveDataType(Column column)
    {
        var name = column.Name.ToLowerInvariant();

        foreach (var (pattern, dataType) in SensitivePatterns)
        {
            if (name.Contains(pattern))
            {
                return dataType;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if a column has an encryption prefix (eno_ or ent_).
    /// </summary>
    private bool HasEncryptionPrefix(Column column)
    {
        var name = column.Name.ToLowerInvariant();
        return name.StartsWith("eno_") || name.StartsWith("ent_");
    }

    /// <summary>
    /// Checks if a column has any TargCC prefix.
    /// </summary>
    private bool HasAnyTargCCPrefix(Column column)
    {
        var name = column.Name.ToLowerInvariant();
        return name.StartsWith("eno_") ||
               name.StartsWith("ent_") ||
               name.StartsWith("clc_") ||
               name.StartsWith("blg_") ||
               name.StartsWith("agg_") ||
               name.StartsWith("spt_");
    }
}
