// <copyright file="SchemaAnalysisParser.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

using System.Text.Json;
using System.Text.RegularExpressions;
using TargCC.AI.Models;

namespace TargCC.AI.Parsers;

/// <summary>
/// Parses schema analysis responses from AI.
/// </summary>
public class SchemaAnalysisParser : IResponseParser<SchemaAnalysisResult>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <inheritdoc/>
    public SchemaAnalysisResult Parse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            throw new ArgumentException("Response cannot be null or empty.", nameof(response));
        }

        if (!this.TryParse(response, out var result))
        {
            throw new InvalidOperationException("Failed to parse schema analysis response.");
        }

        return result!;
    }

    /// <inheritdoc/>
    public bool TryParse(string response, out SchemaAnalysisResult? result)
    {
        result = null;

        try
        {
            // Extract JSON from markdown code blocks if present
            var json = this.ExtractJson(response);

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            // Parse JSON
            var jsonDoc = JsonDocument.Parse(json);
            var root = jsonDoc.RootElement;

            result = new SchemaAnalysisResult
            {
                TableName = root.GetProperty("tableName").GetString() ?? string.Empty,
                Summary = root.GetProperty("summary").GetString() ?? string.Empty,
                QualityScore = root.GetProperty("qualityScore").GetInt32(),
                FollowsTargCCConventions = root.GetProperty("followsTargCCConventions").GetBoolean(),
                Strengths = this.ParseStringArray(root, "strengths"),
                Issues = this.ParseStringArray(root, "issues"),
                Suggestions = this.ParseSuggestions(root),
            };

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string ExtractJson(string response)
    {
        // Try to extract JSON from markdown code blocks
        var jsonBlockPattern = @"```(?:json)?\s*([\s\S]*?)\s*```";
        var match = Regex.Match(response, jsonBlockPattern);

        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // If no code block, try to find JSON directly
        var jsonPattern = @"\{[\s\S]*\}";
        match = Regex.Match(response, jsonPattern);

        if (match.Success)
        {
            return match.Value.Trim();
        }

        return response.Trim();
    }

    private List<string> ParseStringArray(JsonElement root, string propertyName)
    {
        var list = new List<string>();

        if (!root.TryGetProperty(propertyName, out var arrayElement))
        {
            return list;
        }

        foreach (var item in arrayElement.EnumerateArray())
        {
            var value = item.GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                list.Add(value);
            }
        }

        return list;
    }

    private List<Suggestion> ParseSuggestions(JsonElement root)
    {
        var suggestions = new List<Suggestion>();

        if (!root.TryGetProperty("suggestions", out var suggestionsElement))
        {
            return suggestions;
        }

        foreach (var item in suggestionsElement.EnumerateArray())
        {
            var suggestion = new Suggestion
            {
                Severity = this.ParseSeverity(item.GetProperty("severity").GetString()),
                Category = this.ParseCategory(item.GetProperty("category").GetString()),
                Message = item.GetProperty("message").GetString() ?? string.Empty,
            };

            // Optional properties
            if (item.TryGetProperty("target", out var targetElement))
            {
                suggestion.Target = targetElement.GetString();
            }

            if (item.TryGetProperty("recommendedAction", out var actionElement))
            {
                suggestion.RecommendedAction = actionElement.GetString();
            }

            if (item.TryGetProperty("context", out var contextElement))
            {
                suggestion.Context = contextElement.GetString();
            }

            suggestions.Add(suggestion);
        }

        return suggestions;
    }

    private SuggestionSeverity ParseSeverity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return SuggestionSeverity.Info;
        }

        return value.ToLowerInvariant() switch
        {
            "info" => SuggestionSeverity.Info,
            "bestpractice" => SuggestionSeverity.BestPractice,
            "warning" => SuggestionSeverity.Warning,
            "critical" => SuggestionSeverity.Critical,
            _ => SuggestionSeverity.Info,
        };
    }

    private SuggestionCategory ParseCategory(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return SuggestionCategory.General;
        }

        return value.ToLowerInvariant() switch
        {
            "general" => SuggestionCategory.General,
            "security" => SuggestionCategory.Security,
            "performance" => SuggestionCategory.Performance,
            "naming" => SuggestionCategory.Naming,
            "relationships" => SuggestionCategory.Relationships,
            "indexing" => SuggestionCategory.Indexing,
            "datatype" => SuggestionCategory.DataType,
            "targccconventions" => SuggestionCategory.TargCCConventions,
            _ => SuggestionCategory.General,
        };
    }
}
