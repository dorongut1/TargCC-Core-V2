// <copyright file="CodeQualityAnalyzerServiceTests.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.AI.Models;
using TargCC.AI.Models.Quality;
using TargCC.AI.Services;
using TargCC.Core.Interfaces.Models;
using Xunit;

namespace TargCC.AI.Tests.Services;

/// <summary>
/// Unit tests for <see cref="CodeQualityAnalyzerService"/>.
/// </summary>
public class CodeQualityAnalyzerServiceTests
{
    private readonly Mock<IAIService> _mockAIService;
    private readonly Mock<ILogger<CodeQualityAnalyzerService>> _mockLogger;
    private readonly CodeQualityAnalyzerService _service;

    public CodeQualityAnalyzerServiceTests()
    {
        _mockAIService = new Mock<IAIService>();
        _mockLogger = new Mock<ILogger<CodeQualityAnalyzerService>>();
        _service = new CodeQualityAnalyzerService(_mockAIService.Object, _mockLogger.Object);
    }

    #region AnalyzeNamingConventionsAsync Tests

    [Fact]
    public async Task AnalyzeNamingConventionsAsync_WithValidTable_ReturnsNamingIssues()
    {
        // Arrange
        var table = CreateTestTable("Users");
        var jsonResponse = @"{
            ""issues"": [
                {
                    ""elementName"": ""user_id"",
                    ""elementType"": ""Column"",
                    ""schemaName"": ""dbo"",
                    ""issue"": ""Uses snake_case instead of PascalCase"",
                    ""recommendation"": ""Use UserId instead"",
                    ""severity"": ""Medium"",
                    ""example"": ""UserId""
                }
            ]
        }";

        _mockAIService.Setup(x => x.CompleteAsync(
                It.IsAny<AIRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(AIResponse.CreateSuccess(jsonResponse));

        // Act
        var result = await _service.AnalyzeNamingConventionsAsync(table);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result[0].ElementName.Should().Be("user_id");
        result[0].ElementType.Should().Be("Column");
        result[0].Issue.Should().Contain("snake_case");
        result[0].Severity.Should().Be("Medium");
    }

    [Fact]
    public async Task AnalyzeNamingConventionsAsync_WithNoIssues_ReturnsEmptyList()
    {
        // Arrange
        var table = CreateTestTable("Users");
        var jsonResponse = @"{""issues"": []}";

        _mockAIService.Setup(x => x.CompleteAsync(
                It.IsAny<AIRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(AIResponse.CreateSuccess(jsonResponse));

        // Act
        var result = await _service.AnalyzeNamingConventionsAsync(table);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AnalyzeNamingConventionsAsync_WithAIFailure_ReturnsEmptyList()
    {
        // Arrange
        var table = CreateTestTable("Users");

        _mockAIService.Setup(x => x.CompleteAsync(
                It.IsAny<AIRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(AIResponse.CreateFailure("AI service unavailable"));

        // Act
        var result = await _service.AnalyzeNamingConventionsAsync(table);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AnalyzeNamingConventionsAsync_WithInvalidJson_ReturnsEmptyList()
    {
        // Arrange
        var table = CreateTestTable("Users");
        var invalidJson = "{ invalid json }";

        _mockAIService.Setup(x => x.CompleteAsync(
                It.IsAny<AIRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(AIResponse.CreateSuccess(invalidJson));

        // Act
        var result = await _service.AnalyzeNamingConventionsAsync(table);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CheckBestPracticesAsync Tests

    [Fact]
    public async Task CheckBestPracticesAsync_WithViolations_ReturnsViolationsList()
    {
        // Arrange
        var table = CreateTestTable("Users");
        var jsonResponse = @"{
            ""violations"": [
                {
                    ""category"": ""Architecture"",
                    ""elementName"": ""Users"",
                    ""violation"": ""Missing primary key"",
                    ""recommendation"": ""Add Id column as primary key"",
                    ""severity"": ""High"",
                    ""impact"": ""Data integrity issues"",
                    ""fixEffort"": ""Medium""
                }
            ]
        }";

        _mockAIService.Setup(x => x.CompleteAsync(
                It.IsAny<AIRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(AIResponse.CreateSuccess(jsonResponse));

        // Act
        var result = await _service.CheckBestPracticesAsync(table);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result[0].Category.Should().Be("Architecture");
        result[0].ElementName.Should().Be("Users");
        result[0].Severity.Should().Be("High");
        result[0].FixEffort.Should().Be("Medium");
    }

    [Fact]
    public async Task CheckBestPracticesAsync_WithNoViolations_ReturnsEmptyList()
    {
        // Arrange
        var table = CreateTestTable("Users");
        var jsonResponse = @"{""violations"": []}";

        _mockAIService.Setup(x => x.CompleteAsync(
                It.IsAny<AIRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(AIResponse.CreateSuccess(jsonResponse));

        // Act
        var result = await _service.CheckBestPracticesAsync(table);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckBestPracticesAsync_WithAIFailure_ReturnsEmptyList()
    {
        // Arrange
        var table = CreateTestTable("Users");

        _mockAIService.Setup(x => x.CompleteAsync(
                It.IsAny<AIRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(AIResponse.CreateFailure("Service error"));

        // Act
        var result = await _service.CheckBestPracticesAsync(table);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckBestPracticesAsync_WithMultipleCategories_GroupsCorrectly()
    {
        // Arrange
        var table = CreateTestTable("Users");
        var jsonResponse = @"{
            ""violations"": [
                {
                    ""category"": ""Performance"",
                    ""elementName"": ""Email"",
                    ""violation"": ""Missing index"",
                    ""recommendation"": ""Add index on Email column"",
                    ""severity"": ""Medium"",
                    ""impact"": ""Slow queries""
                },
                {
                    ""category"": ""Security"",
                    ""elementName"": ""Password"",
                    ""violation"": ""Not encrypted"",
                    ""recommendation"": ""Use eno_ prefix"",
                    ""severity"": ""High"",
                    ""impact"": ""Security vulnerability""
                }
            ]
        }";

        _mockAIService.Setup(x => x.CompleteAsync(
                It.IsAny<AIRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(AIResponse.CreateSuccess(jsonResponse));

        // Act
        var result = await _service.CheckBestPracticesAsync(table);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(v => v.Category == "Performance");
        result.Should().Contain(v => v.Category == "Security");
    }

    #endregion

    #region ValidateRelationshipsAsync Tests

    [Fact]
    public async Task ValidateRelationshipsAsync_WithIssues_ReturnsRelationshipIssues()
    {
        // Arrange
        var table = CreateTestTable("Orders");
        var jsonResponse = @"{
            ""issues"": [
                {
                    ""sourceTable"": ""Orders"",
                    ""targetTable"": ""Customers"",
                    ""issueType"": ""MissingForeignKey"",
                    ""description"": ""CustomerId lacks foreign key constraint"",
                    ""suggestion"": ""Add FK constraint to Customers table"",
                    ""severity"": ""High"",
                    ""affectedColumns"": [""CustomerId""],
                    ""isCascadeIssue"": false
                }
            ]
        }";

        _mockAIService.Setup(x => x.CompleteAsync(
                It.IsAny<AIRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(AIResponse.CreateSuccess(jsonResponse));

        // Act
        var result = await _service.ValidateRelationshipsAsync(table);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result[0].SourceTable.Should().Be("Orders");
        result[0].TargetTable.Should().Be("Customers");
        result[0].IssueType.Should().Be("MissingForeignKey");
        result[0].AffectedColumns.Should().Contain("CustomerId");
        result[0].IsCascadeIssue.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateRelationshipsAsync_WithNoIssues_ReturnsEmptyList()
    {
        // Arrange
        var table = CreateTestTable("Orders");
        var jsonResponse = @"{""issues"": []}";

        _mockAIService.Setup(x => x.CompleteAsync(
                It.IsAny<AIRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(AIResponse.CreateSuccess(jsonResponse));

        // Act
        var result = await _service.ValidateRelationshipsAsync(table);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateRelationshipsAsync_WithCascadeIssue_FlagsCorrectly()
    {
        // Arrange
        var table = CreateTestTable("Orders");
        var jsonResponse = @"{
            ""issues"": [
                {
                    ""sourceTable"": ""Orders"",
                    ""targetTable"": ""Customers"",
                    ""issueType"": ""CascadeDelete"",
                    ""description"": ""Deleting customer will cascade delete orders"",
                    ""suggestion"": ""Consider soft delete instead"",
                    ""severity"": ""Critical"",
                    ""affectedColumns"": [""CustomerId""],
                    ""isCascadeIssue"": true
                }
            ]
        }";

        _mockAIService.Setup(x => x.CompleteAsync(
                It.IsAny<AIRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(AIResponse.CreateSuccess(jsonResponse));

        // Act
        var result = await _service.ValidateRelationshipsAsync(table);

        // Assert
        result.Should().HaveCount(1);
        result[0].IsCascadeIssue.Should().BeTrue();
        result[0].Severity.Should().Be("Critical");
    }

    [Fact]
    public async Task ValidateRelationshipsAsync_WithAIFailure_ReturnsEmptyList()
    {
        // Arrange
        var table = CreateTestTable("Orders");

        _mockAIService.Setup(x => x.CompleteAsync(
                It.IsAny<AIRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(AIResponse.CreateFailure("Connection timeout"));

        // Act
        var result = await _service.ValidateRelationshipsAsync(table);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GenerateQualityReportAsync Tests

    [Fact]
    public async Task GenerateQualityReportAsync_WithNoIssues_ReturnsHighScore()
    {
        // Arrange
        var table = CreateTestTable("Users");
        SetupEmptyResponses();

        // Act
        var report = await _service.GenerateQualityReportAsync(table);

        // Assert
        report.Should().NotBeNull();
        report.TableName.Should().Be("Users");
        report.SchemaName.Should().Be("dbo");
        report.QualityScore.Should().Be(100);
        report.Grade.Should().Be("A");
        report.TotalIssues.Should().Be(0);
        report.NamingIssues.Should().BeEmpty();
        report.BestPracticeViolations.Should().BeEmpty();
        report.RelationshipIssues.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateQualityReportAsync_WithMixedIssues_CalculatesScoreCorrectly()
    {
        // Arrange
        var table = CreateTestTable("Orders");
        SetupMixedIssueResponses();

        // Act
        var report = await _service.GenerateQualityReportAsync(table);

        // Assert
        report.Should().NotBeNull();
        report.TotalIssues.Should().BeGreaterThan(0);
        report.QualityScore.Should().BeLessThan(100);
        report.Grade.Should().NotBe("A");
        report.NamingIssues.Should().NotBeEmpty();
        report.BestPracticeViolations.Should().NotBeEmpty();
        report.RelationshipIssues.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateQualityReportAsync_WithCriticalIssues_AssignsLowScore()
    {
        // Arrange
        var table = CreateTestTable("Users");
        SetupCriticalIssueResponses();

        // Act
        var report = await _service.GenerateQualityReportAsync(table);

        // Assert
        report.Should().NotBeNull();
        report.QualityScore.Should().BeLessThan(70);
        report.Grade.Should().BeOneOf("D", "F");
        report.CriticalIssues.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GenerateQualityReportAsync_SetsTimestamp_Correctly()
    {
        // Arrange
        var table = CreateTestTable("Users");
        SetupEmptyResponses();
        var beforeCall = DateTime.UtcNow;

        // Act
        var report = await _service.GenerateQualityReportAsync(table);

        // Assert
        var afterCall = DateTime.UtcNow;
        report.AnalyzedAt.Should().BeOnOrAfter(beforeCall);
        report.AnalyzedAt.Should().BeOnOrBefore(afterCall);
    }

    [Fact]
    public async Task GenerateQualityReportAsync_WithNullTable_ThrowsArgumentNullException()
    {
        // Arrange
        Table? table = null;

        // Act
        Func<Task> act = async () => await _service.GenerateQualityReportAsync(table!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Helper Methods

    private static Table CreateTestTable(string tableName)
    {
        return new Table
        {
            Name = tableName,
            SchemaName = "dbo",
            Columns = new List<Column>
            {
                new()
                {
                    Name = "Id",
                    DataType = "int",
                    IsPrimaryKey = true,
                    IsNullable = false,
                    IsIdentity = true,
                },
                new()
                {
                    Name = "Name",
                    DataType = "nvarchar",
                    MaxLength = 100,
                    IsNullable = false,
                },
            },
        };
    }

    private void SetupEmptyResponses()
    {
        _mockAIService.Setup(x => x.CompleteAsync(
                It.IsAny<AIRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AIRequest req, CancellationToken ct) =>
            {
                // Return empty results for all analysis types
                if (req.Prompt.Contains("naming"))
                {
                    return AIResponse.CreateSuccess(@"{""issues"": []}");
                }

                if (req.Prompt.Contains("best practice"))
                {
                    return AIResponse.CreateSuccess(@"{""violations"": []}");
                }

                if (req.Prompt.Contains("relationship"))
                {
                    return AIResponse.CreateSuccess(@"{""issues"": []}");
                }

                return AIResponse.CreateSuccess(@"{}");
            });
    }

    private void SetupMixedIssueResponses()
    {
        _mockAIService.Setup(x => x.CompleteAsync(
                It.IsAny<AIRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AIRequest req, CancellationToken ct) =>
            {
                if (req.Prompt.Contains("naming"))
                {
                    return AIResponse.CreateSuccess(@"{
                        ""issues"": [{
                            ""elementName"": ""user_id"",
                            ""elementType"": ""Column"",
                            ""schemaName"": ""dbo"",
                            ""issue"": ""Snake case"",
                            ""recommendation"": ""Use PascalCase"",
                            ""severity"": ""Medium""
                        }]
                    }");
                }

                if (req.Prompt.Contains("best practice"))
                {
                    return AIResponse.CreateSuccess(@"{
                        ""violations"": [{
                            ""category"": ""Architecture"",
                            ""elementName"": ""Orders"",
                            ""violation"": ""Missing audit columns"",
                            ""recommendation"": ""Add CreatedAt, UpdatedAt"",
                            ""severity"": ""Low"",
                            ""impact"": ""Limited tracking""
                        }]
                    }");
                }

                if (req.Prompt.Contains("relationship"))
                {
                    return AIResponse.CreateSuccess(@"{
                        ""issues"": [{
                            ""sourceTable"": ""Orders"",
                            ""targetTable"": ""Customers"",
                            ""issueType"": ""MissingIndex"",
                            ""description"": ""No index on CustomerId"",
                            ""suggestion"": ""Add index"",
                            ""severity"": ""Medium"",
                            ""affectedColumns"": [""CustomerId""],
                            ""isCascadeIssue"": false
                        }]
                    }");
                }

                return AIResponse.CreateSuccess(@"{}");
            });
    }

    private void SetupCriticalIssueResponses()
    {
        _mockAIService.Setup(x => x.CompleteAsync(
                It.IsAny<AIRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AIRequest req, CancellationToken ct) =>
            {
                if (req.Prompt.Contains("naming"))
                {
                    return AIResponse.CreateSuccess(@"{
                        ""issues"": [{
                            ""elementName"": ""tbl_user"",
                            ""elementType"": ""Table"",
                            ""schemaName"": ""dbo"",
                            ""issue"": ""Uses Hungarian notation"",
                            ""recommendation"": ""Remove tbl_ prefix"",
                            ""severity"": ""Critical""
                        }]
                    }");
                }

                if (req.Prompt.Contains("best practice"))
                {
                    return AIResponse.CreateSuccess(@"{
                        ""violations"": [{
                            ""category"": ""Security"",
                            ""elementName"": ""Password"",
                            ""violation"": ""Sensitive data not encrypted"",
                            ""recommendation"": ""Use eno_ prefix and encryption"",
                            ""severity"": ""Critical"",
                            ""impact"": ""Major security vulnerability""
                        }]
                    }");
                }

                if (req.Prompt.Contains("relationship"))
                {
                    return AIResponse.CreateSuccess(@"{
                        ""issues"": [{
                            ""sourceTable"": ""Users"",
                            ""targetTable"": ""Orders"",
                            ""issueType"": ""CascadeDelete"",
                            ""description"": ""Cascade delete will remove all orders"",
                            ""suggestion"": ""Implement soft delete"",
                            ""severity"": ""Critical"",
                            ""affectedColumns"": [""UserId""],
                            ""isCascadeIssue"": true
                        }]
                    }");
                }

                return AIResponse.CreateSuccess(@"{}");
            });
    }

    #endregion
}
