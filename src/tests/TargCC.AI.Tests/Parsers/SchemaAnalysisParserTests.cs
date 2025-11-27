// <copyright file="SchemaAnalysisParserTests.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

using FluentAssertions;
using TargCC.AI.Models;
using TargCC.AI.Parsers;
using Xunit;

namespace TargCC.AI.Tests.Parsers;

public class SchemaAnalysisParserTests
{
    [Fact]
    public void Parse_WithValidJSON_ShouldReturnSchemaAnalysisResult()
    {
        var parser = new SchemaAnalysisParser();
        var json = @"{
            ""tableName"": ""Customer"",
            ""summary"": ""Well-structured table"",
            ""qualityScore"": 85,
            ""followsTargCCConventions"": true,
            ""strengths"": [""Good naming"", ""Proper indexes""],
            ""issues"": [""Missing ent_CreatedDate""],
            ""suggestions"": [
                {
                    ""severity"": ""Warning"",
                    ""category"": ""TargCCConventions"",
                    ""message"": ""Add temporal column"",
                    ""target"": ""Customer"",
                    ""recommendedAction"": ""Add ent_CreatedDate"",
                    ""context"": ""Tracking creation time""
                }
            ]
        }";

        var result = parser.Parse(json);

        result.Should().NotBeNull();
        result.TableName.Should().Be("Customer");
        result.Summary.Should().Be("Well-structured table");
        result.QualityScore.Should().Be(85);
        result.FollowsTargCCConventions.Should().BeTrue();
        result.Strengths.Should().HaveCount(2);
        result.Issues.Should().HaveCount(1);
        result.Suggestions.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_WithJSONWrappedInCodeBlock_ShouldExtractAndParse()
    {
        var parser = new SchemaAnalysisParser();
        var json = @"Here's the analysis:
        
```json
{
    ""tableName"": ""Order"",
    ""summary"": ""Good table design"",
    ""qualityScore"": 90,
    ""followsTargCCConventions"": true,
    ""strengths"": [],
    ""issues"": [],
    ""suggestions"": []
}
```

Hope this helps!";

        var result = parser.Parse(json);

        result.Should().NotBeNull();
        result.TableName.Should().Be("Order");
        result.QualityScore.Should().Be(90);
    }

    [Fact]
    public void Parse_WithInvalidJSON_ShouldThrowInvalidOperationException()
    {
        var parser = new SchemaAnalysisParser();
        var invalidJson = "{ invalid json }";

        var act = () => parser.Parse(invalidJson);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Failed to parse schema analysis response.");
    }

    [Fact]
    public void Parse_WithEmptyString_ShouldThrowArgumentException()
    {
        var parser = new SchemaAnalysisParser();

        var act = () => parser.Parse(string.Empty);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Response cannot be null or empty. (Parameter 'response')");
    }

    [Fact]
    public void Parse_WithMultipleSuggestions_ShouldParseAll()
    {
        var parser = new SchemaAnalysisParser();
        var json = @"{
            ""tableName"": ""Product"",
            ""summary"": ""Needs improvements"",
            ""qualityScore"": 60,
            ""followsTargCCConventions"": false,
            ""strengths"": [],
            ""issues"": [],
            ""suggestions"": [
                {
                    ""severity"": ""Critical"",
                    ""category"": ""Security"",
                    ""message"": ""Encrypt sensitive data"",
                    ""target"": ""SSN"",
                    ""recommendedAction"": ""Rename to eno_SSN"",
                    ""context"": ""Social Security Number""
                },
                {
                    ""severity"": ""Warning"",
                    ""category"": ""Performance"",
                    ""message"": ""Add index"",
                    ""target"": ""Email"",
                    ""recommendedAction"": ""CREATE INDEX IX_Email"",
                    ""context"": ""Frequently queried""
                },
                {
                    ""severity"": ""BestPractice"",
                    ""category"": ""Naming"",
                    ""message"": ""Use singular name"",
                    ""target"": ""Products"",
                    ""recommendedAction"": ""Rename to Product"",
                    ""context"": ""TargCC convention""
                }
            ]
        }";

        var result = parser.Parse(json);

        result.Suggestions.Should().HaveCount(3);
        result.Suggestions[0].Severity.Should().Be(SuggestionSeverity.Critical);
        result.Suggestions[0].Category.Should().Be(SuggestionCategory.Security);
        result.Suggestions[1].Severity.Should().Be(SuggestionSeverity.Warning);
        result.Suggestions[1].Category.Should().Be(SuggestionCategory.Performance);
        result.Suggestions[2].Severity.Should().Be(SuggestionSeverity.BestPractice);
        result.Suggestions[2].Category.Should().Be(SuggestionCategory.Naming);
    }

    [Fact]
    public void Parse_WithMissingOptionalFields_ShouldUseDefaults()
    {
        var parser = new SchemaAnalysisParser();
        var json = @"{
            ""tableName"": ""Test"",
            ""summary"": ""Test table"",
            ""qualityScore"": 50,
            ""followsTargCCConventions"": false
        }";

        var result = parser.Parse(json);

        result.TableName.Should().Be("Test");
        result.Strengths.Should().BeEmpty();
        result.Issues.Should().BeEmpty();
        result.Suggestions.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WithAllSeverityLevels_ShouldParseCorrectly()
    {
        var parser = new SchemaAnalysisParser();
        var json = @"{
            ""tableName"": ""Test"",
            ""summary"": ""Test"",
            ""qualityScore"": 50,
            ""followsTargCCConventions"": false,
            ""strengths"": [],
            ""issues"": [],
            ""suggestions"": [
                { ""severity"": ""Info"", ""category"": ""General"", ""message"": ""Info"", ""target"": ""x"", ""recommendedAction"": ""x"", ""context"": ""x"" },
                { ""severity"": ""BestPractice"", ""category"": ""General"", ""message"": ""BP"", ""target"": ""x"", ""recommendedAction"": ""x"", ""context"": ""x"" },
                { ""severity"": ""Warning"", ""category"": ""General"", ""message"": ""Warn"", ""target"": ""x"", ""recommendedAction"": ""x"", ""context"": ""x"" },
                { ""severity"": ""Critical"", ""category"": ""General"", ""message"": ""Crit"", ""target"": ""x"", ""recommendedAction"": ""x"", ""context"": ""x"" }
            ]
        }";

        var result = parser.Parse(json);

        result.Suggestions[0].Severity.Should().Be(SuggestionSeverity.Info);
        result.Suggestions[1].Severity.Should().Be(SuggestionSeverity.BestPractice);
        result.Suggestions[2].Severity.Should().Be(SuggestionSeverity.Warning);
        result.Suggestions[3].Severity.Should().Be(SuggestionSeverity.Critical);
    }

    [Fact]
    public void Parse_WithAllCategories_ShouldParseCorrectly()
    {
        var parser = new SchemaAnalysisParser();
        var json = @"{
            ""tableName"": ""Test"",
            ""summary"": ""Test"",
            ""qualityScore"": 50,
            ""followsTargCCConventions"": false,
            ""strengths"": [],
            ""issues"": [],
            ""suggestions"": [
                { ""severity"": ""Info"", ""category"": ""General"", ""message"": ""1"", ""target"": ""x"", ""recommendedAction"": ""x"", ""context"": ""x"" },
                { ""severity"": ""Info"", ""category"": ""Security"", ""message"": ""2"", ""target"": ""x"", ""recommendedAction"": ""x"", ""context"": ""x"" },
                { ""severity"": ""Info"", ""category"": ""Performance"", ""message"": ""3"", ""target"": ""x"", ""recommendedAction"": ""x"", ""context"": ""x"" },
                { ""severity"": ""Info"", ""category"": ""Naming"", ""message"": ""4"", ""target"": ""x"", ""recommendedAction"": ""x"", ""context"": ""x"" },
                { ""severity"": ""Info"", ""category"": ""Relationships"", ""message"": ""5"", ""target"": ""x"", ""recommendedAction"": ""x"", ""context"": ""x"" },
                { ""severity"": ""Info"", ""category"": ""Indexing"", ""message"": ""6"", ""target"": ""x"", ""recommendedAction"": ""x"", ""context"": ""x"" },
                { ""severity"": ""Info"", ""category"": ""DataType"", ""message"": ""7"", ""target"": ""x"", ""recommendedAction"": ""x"", ""context"": ""x"" },
                { ""severity"": ""Info"", ""category"": ""TargCCConventions"", ""message"": ""8"", ""target"": ""x"", ""recommendedAction"": ""x"", ""context"": ""x"" }
            ]
        }";

        var result = parser.Parse(json);

        result.Suggestions[0].Category.Should().Be(SuggestionCategory.General);
        result.Suggestions[1].Category.Should().Be(SuggestionCategory.Security);
        result.Suggestions[2].Category.Should().Be(SuggestionCategory.Performance);
        result.Suggestions[3].Category.Should().Be(SuggestionCategory.Naming);
        result.Suggestions[4].Category.Should().Be(SuggestionCategory.Relationships);
        result.Suggestions[5].Category.Should().Be(SuggestionCategory.Indexing);
        result.Suggestions[6].Category.Should().Be(SuggestionCategory.DataType);
        result.Suggestions[7].Category.Should().Be(SuggestionCategory.TargCCConventions);
    }
}
