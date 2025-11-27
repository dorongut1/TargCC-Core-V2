// Copyright (c) TargCC Team. All rights reserved.

using FluentAssertions;
using TargCC.AI.Formatters;
using TargCC.AI.Models;
using Xunit;

namespace TargCC.AI.Tests.Formatters;

public class SuggestionFormatterTests
{
    private readonly SuggestionFormatter formatter;

    public SuggestionFormatterTests()
    {
        this.formatter = new SuggestionFormatter();
    }

    [Fact]
    public void FormatSuggestions_WithNullResult_ShouldThrowArgumentNullException()
    {
        var act = () => this.formatter.FormatSuggestions(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FormatSuggestions_WithEmptySuggestions_ShouldReturnSuccessMessage()
    {
        var result = new SchemaAnalysisResult
        {
            TableName = "Customer",
            QualityScore = 95,
            Summary = "Excellent table",
            Suggestions = new List<Suggestion>(),
        };

        var formatted = this.formatter.FormatSuggestions(result);

        formatted.Should().Contain("No suggestions");
        formatted.Should().Contain("schema looks good");
    }

    [Fact]
    public void FormatSuggestions_WithValidSuggestions_ShouldFormatCorrectly()
    {
        var result = new SchemaAnalysisResult
        {
            TableName = "Customer",
            QualityScore = 75,
            Summary = "Good table with minor issues",
            Suggestions = new List<Suggestion>
            {
                new()
                {
                    Severity = SuggestionSeverity.Critical,
                    Category = SuggestionCategory.Security,
                    Message = "Missing encryption on Password column",
                    Target = "Password",
                    RecommendedAction = "Add eno_ prefix",
                    Context = "Sensitive data",
                },
                new()
                {
                    Severity = SuggestionSeverity.Warning,
                    Category = SuggestionCategory.Performance,
                    Message = "Add index on Email",
                    Target = "Email",
                    RecommendedAction = "CREATE INDEX IX_Customer_Email",
                    Context = "Frequent lookups",
                },
            },
        };

        var formatted = this.formatter.FormatSuggestions(result, useColors: false);

        formatted.Should().Contain("Customer");
        formatted.Should().Contain("75/100");
        formatted.Should().Contain("2");
        formatted.Should().Contain("Missing encryption");
        formatted.Should().Contain("Add index");
    }

    [Fact]
    public void FormatSuggestion_WithNullSuggestion_ShouldThrowArgumentNullException()
    {
        var act = () => this.formatter.FormatSuggestion(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FormatSuggestion_WithValidSuggestion_ShouldFormatCorrectly()
    {
        var suggestion = new Suggestion
        {
            Severity = SuggestionSeverity.Critical,
            Category = SuggestionCategory.Security,
            Message = "Sensitive column not encrypted",
            Target = "Password",
            RecommendedAction = "Add eno_ prefix for one-way encryption",
            Context = "User authentication",
        };

        var formatted = this.formatter.FormatSuggestion(suggestion, useColors: false);

        formatted.Should().Contain("Critical");
        formatted.Should().Contain("Sensitive column not encrypted");
        formatted.Should().Contain("Target: Password");
        formatted.Should().Contain("Action: Add eno_");
        formatted.Should().Contain("Context: User authentication");
    }

    [Fact]
    public void FormatByCategory_WithNullResult_ShouldThrowArgumentNullException()
    {
        var act = () => this.formatter.FormatByCategory(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FormatByCategory_WithValidSuggestions_ShouldGroupByCategory()
    {
        var result = new SchemaAnalysisResult
        {
            TableName = "Customer",
            QualityScore = 80,
            Summary = "Good table",
            Suggestions = new List<Suggestion>
            {
                new()
                {
                    Severity = SuggestionSeverity.Critical,
                    Category = SuggestionCategory.Security,
                    Message = "Security issue 1",
                },
                new()
                {
                    Severity = SuggestionSeverity.Warning,
                    Category = SuggestionCategory.Security,
                    Message = "Security issue 2",
                },
                new()
                {
                    Severity = SuggestionSeverity.Info,
                    Category = SuggestionCategory.Performance,
                    Message = "Performance suggestion",
                },
            },
        };

        var formatted = this.formatter.FormatByCategory(result, useColors: false);

        formatted.Should().Contain("Customer");
        formatted.Should().Contain("Security");
        formatted.Should().Contain("Performance");
        formatted.Should().Contain("Security issue 1");
        formatted.Should().Contain("Security issue 2");
        formatted.Should().Contain("Performance suggestion");
    }

    [Fact]
    public void CreateSummary_WithNullResult_ShouldThrowArgumentNullException()
    {
        var act = () => this.formatter.CreateSummary(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateSummary_WithNoSuggestions_ShouldReturnSuccessMessage()
    {
        var result = new SchemaAnalysisResult
        {
            TableName = "Customer",
            QualityScore = 95,
            Summary = "Perfect",
            Suggestions = new List<Suggestion>(),
        };

        var summary = this.formatter.CreateSummary(result);

        summary.Should().Contain("Customer");
        summary.Should().Contain("No issues found");
        summary.Should().Contain("95/100");
    }

    [Fact]
    public void CreateSummary_WithMixedSeverities_ShouldIncludeAllCounts()
    {
        var result = new SchemaAnalysisResult
        {
            TableName = "Orders",
            QualityScore = 65,
            Summary = "Needs work",
            Suggestions = new List<Suggestion>
            {
                new() { Severity = SuggestionSeverity.Critical, Category = SuggestionCategory.Security, Message = "Critical 1" },
                new() { Severity = SuggestionSeverity.Critical, Category = SuggestionCategory.Security, Message = "Critical 2" },
                new() { Severity = SuggestionSeverity.Warning, Category = SuggestionCategory.Performance, Message = "Warning 1" },
                new() { Severity = SuggestionSeverity.BestPractice, Category = SuggestionCategory.Naming, Message = "BP 1" },
                new() { Severity = SuggestionSeverity.Info, Category = SuggestionCategory.Relationships, Message = "Info 1" },
            },
        };

        var summary = this.formatter.CreateSummary(result);

        summary.Should().Contain("Orders");
        summary.Should().Contain("2 critical");
        summary.Should().Contain("1 warning");
        summary.Should().Contain("1 best practice");
        summary.Should().Contain("1 info");
        summary.Should().Contain("65/100");
    }

    [Fact]
    public void FormatSuggestions_WithColorsDisabled_ShouldNotIncludeAnsiCodes()
    {
        var result = new SchemaAnalysisResult
        {
            TableName = "Customer",
            QualityScore = 75,
            Summary = "Good",
            Suggestions = new List<Suggestion>
            {
                new()
                {
                    Severity = SuggestionSeverity.Critical,
                    Category = SuggestionCategory.Security,
                    Message = "Test message",
                },
            },
        };

        var formatted = this.formatter.FormatSuggestions(result, useColors: false);

        // Should not contain ANSI escape codes
        formatted.Should().NotContain("\u001b[");
    }

    [Fact]
    public void FormatSuggestions_ShouldOrderBySeverity()
    {
        var result = new SchemaAnalysisResult
        {
            TableName = "Customer",
            QualityScore = 70,
            Summary = "Mixed",
            Suggestions = new List<Suggestion>
            {
                new()
                {
                    Severity = SuggestionSeverity.Info,
                    Category = SuggestionCategory.Performance,
                    Message = "Info message",
                },
                new()
                {
                    Severity = SuggestionSeverity.Critical,
                    Category = SuggestionCategory.Security,
                    Message = "Critical message",
                },
                new()
                {
                    Severity = SuggestionSeverity.Warning,
                    Category = SuggestionCategory.Relationships,
                    Message = "Warning message",
                },
            },
        };

        var formatted = this.formatter.FormatSuggestions(result, useColors: false);

        // Critical should appear before Warning which should appear before Info
        var criticalIndex = formatted.IndexOf("Critical message", StringComparison.Ordinal);
        var warningIndex = formatted.IndexOf("Warning message", StringComparison.Ordinal);
        var infoIndex = formatted.IndexOf("Info message", StringComparison.Ordinal);

        criticalIndex.Should().BeLessThan(warningIndex);
        warningIndex.Should().BeLessThan(infoIndex);
    }
}
