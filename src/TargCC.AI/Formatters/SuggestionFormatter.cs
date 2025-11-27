// Copyright (c) TargCC Team. All rights reserved.

using System.Text;
using TargCC.AI.Models;

namespace TargCC.AI.Formatters;

/// <summary>
/// Formats suggestions for display in the CLI.
/// </summary>
public class SuggestionFormatter
{
    private const string CriticalColor = "\u001b[31m"; // Red
    private const string WarningColor = "\u001b[33m";     // Yellow
    private const string BestPracticeColor = "\u001b[36m";   // Cyan
    private const string InfoColor = "\u001b[32m";      // Green
    private const string ResetColor = "\u001b[0m";     // Reset

    private const string CriticalIcon = "🔴";
    private const string WarningIcon = "🟡";
    private const string BestPracticeIcon = "🔵";
    private const string InfoIcon = "🟢";

    /// <summary>
    /// Formats a list of suggestions for CLI display.
    /// </summary>
    /// <param name="result">The schema analysis result containing suggestions.</param>
    /// <param name="useColors">Whether to use ANSI colors (default: true).</param>
    /// <returns>A formatted string ready for console output.</returns>
    public string FormatSuggestions(SchemaAnalysisResult result, bool useColors = true)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (result.Suggestions.Count == 0)
        {
            return "✅ No suggestions - schema looks good!";
        }

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine($"📋 Schema Analysis Results for '{result.TableName}'");
        sb.AppendLine();
        sb.AppendLine($"Quality Score: {result.QualityScore}/100");
        sb.AppendLine($"Total Suggestions: {result.Suggestions.Count}");
        sb.AppendLine();

        // Group by severity
        var groupedBySeverity = result.Suggestions
            .GroupBy(s => s.Severity)
            .OrderByDescending(g => g.Key);

        foreach (var severityGroup in groupedBySeverity)
        {
            var severity = severityGroup.Key;
            var icon = this.GetIcon(severity);
            var color = useColors ? this.GetColor(severity) : string.Empty;
            var reset = useColors ? ResetColor : string.Empty;

            sb.AppendLine($"{color}{icon} {severity.ToString().ToUpperInvariant()}:{reset}");

            foreach (var suggestion in severityGroup)
            {
                sb.AppendLine($"  • {suggestion.Message}");

                if (!string.IsNullOrWhiteSpace(suggestion.Target))
                {
                    sb.AppendLine($"    Target: {suggestion.Target}");
                }

                if (!string.IsNullOrWhiteSpace(suggestion.RecommendedAction))
                {
                    sb.AppendLine($"    Action: {suggestion.RecommendedAction}");
                }

                if (!string.IsNullOrWhiteSpace(suggestion.Context))
                {
                    sb.AppendLine($"    Context: {suggestion.Context}");
                }

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats a single suggestion for display.
    /// </summary>
    /// <param name="suggestion">The suggestion to format.</param>
    /// <param name="useColors">Whether to use ANSI colors (default: true).</param>
    /// <returns>A formatted string ready for console output.</returns>
    public string FormatSuggestion(Suggestion suggestion, bool useColors = true)
    {
        if (suggestion == null)
        {
            throw new ArgumentNullException(nameof(suggestion));
        }

        var sb = new StringBuilder();
        var icon = this.GetIcon(suggestion.Severity);
        var color = useColors ? this.GetColor(suggestion.Severity) : string.Empty;
        var reset = useColors ? ResetColor : string.Empty;

        sb.AppendLine($"{color}{icon} [{suggestion.Severity}] {suggestion.Message}{reset}");

        if (!string.IsNullOrWhiteSpace(suggestion.Target))
        {
            sb.AppendLine($"  Target: {suggestion.Target}");
        }

        if (!string.IsNullOrWhiteSpace(suggestion.RecommendedAction))
        {
            sb.AppendLine($"  Action: {suggestion.RecommendedAction}");
        }

        if (!string.IsNullOrWhiteSpace(suggestion.Context))
        {
            sb.AppendLine($"  Context: {suggestion.Context}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats suggestions grouped by category.
    /// </summary>
    /// <param name="result">The schema analysis result containing suggestions.</param>
    /// <param name="useColors">Whether to use ANSI colors (default: true).</param>
    /// <returns>A formatted string grouped by category.</returns>
    public string FormatByCategory(SchemaAnalysisResult result, bool useColors = true)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (result.Suggestions.Count == 0)
        {
            return "✅ No suggestions - schema looks good!";
        }

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine($"📋 Schema Analysis Results for '{result.TableName}' (Grouped by Category)");
        sb.AppendLine();

        // Group by category
        var groupedByCategory = result.Suggestions
            .GroupBy(s => s.Category)
            .OrderBy(g => g.Key.ToString());

        foreach (var categoryGroup in groupedByCategory)
        {
            sb.AppendLine($"📁 {categoryGroup.Key}:");
            sb.AppendLine();

            foreach (var suggestion in categoryGroup.OrderByDescending(s => s.Severity))
            {
                var icon = this.GetIcon(suggestion.Severity);
                var color = useColors ? this.GetColor(suggestion.Severity) : string.Empty;
                var reset = useColors ? ResetColor : string.Empty;

                sb.AppendLine($"  {color}{icon} {suggestion.Message}{reset}");

                if (!string.IsNullOrWhiteSpace(suggestion.RecommendedAction))
                {
                    sb.AppendLine($"     → {suggestion.RecommendedAction}");
                }

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Creates a compact summary of suggestions.
    /// </summary>
    /// <param name="result">The schema analysis result containing suggestions.</param>
    /// <returns>A compact summary string.</returns>
    public string CreateSummary(SchemaAnalysisResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (result.Suggestions.Count == 0)
        {
            return $"✅ {result.TableName}: No issues found (Quality: {result.QualityScore}/100)";
        }

        var criticalCount = result.Suggestions.Count(s => s.Severity == SuggestionSeverity.Critical);
        var warningCount = result.Suggestions.Count(s => s.Severity == SuggestionSeverity.Warning);
        var bestPracticeCount = result.Suggestions.Count(s => s.Severity == SuggestionSeverity.BestPractice);
        var infoCount = result.Suggestions.Count(s => s.Severity == SuggestionSeverity.Info);

        var parts = new List<string>();
        if (criticalCount > 0)
        {
            parts.Add($"{CriticalIcon} {criticalCount} critical");
        }

        if (warningCount > 0)
        {
            parts.Add($"{WarningIcon} {warningCount} warning");
        }

        if (bestPracticeCount > 0)
        {
            parts.Add($"{BestPracticeIcon} {bestPracticeCount} best practice");
        }

        if (infoCount > 0)
        {
            parts.Add($"{InfoIcon} {infoCount} info");
        }

        return $"📊 {result.TableName}: {string.Join(", ", parts)} (Quality: {result.QualityScore}/100)";
    }

    private string GetIcon(SuggestionSeverity severity)
    {
        return severity switch
        {
            SuggestionSeverity.Critical => CriticalIcon,
            SuggestionSeverity.Warning => WarningIcon,
            SuggestionSeverity.BestPractice => BestPracticeIcon,
            SuggestionSeverity.Info => InfoIcon,
            _ => "⚪",
        };
    }

    private string GetColor(SuggestionSeverity severity)
    {
        return severity switch
        {
            SuggestionSeverity.Critical => CriticalColor,
            SuggestionSeverity.Warning => WarningColor,
            SuggestionSeverity.BestPractice => BestPracticeColor,
            SuggestionSeverity.Info => InfoColor,
            _ => ResetColor,
        };
    }
}
