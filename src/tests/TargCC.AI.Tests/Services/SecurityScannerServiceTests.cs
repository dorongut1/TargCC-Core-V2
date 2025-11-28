// <copyright file="SecurityScannerServiceTests.cs" company="Doron Aharoni">
// Copyright (c) Doron Aharoni. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.AI.Models;
using TargCC.AI.Services;
using TargCC.Core.Interfaces.Models;

namespace TargCC.AI.Tests.Services;

/// <summary>
/// Tests for <see cref="SecurityScannerService"/>.
/// </summary>
public sealed class SecurityScannerServiceTests
{
    private readonly Mock<IAIService> mockAIService;
    private readonly Mock<ILogger<SecurityScannerService>> mockLogger;
    private readonly SecurityScannerService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityScannerServiceTests"/> class.
    /// </summary>
    public SecurityScannerServiceTests()
    {
        this.mockAIService = new Mock<IAIService>();
        this.mockLogger = new Mock<ILogger<SecurityScannerService>>();
        this.service = new SecurityScannerService(this.mockAIService.Object, this.mockLogger.Object);
    }

    [Fact]
    public async Task FindVulnerabilitiesAsync_WithPasswordColumn_ReturnsVulnerability()
    {
        // Arrange
        var table = new Table
        {
            Name = "Users",
            SchemaName = "dbo",
            Columns = new List<Column>
            {
                new Column { Name = "UserId", DataType = "INT", IsNullable = false, IsPrimaryKey = true },
                new Column { Name = "Password", DataType = "NVARCHAR(100)", IsNullable = false, IsPrimaryKey = false },
            },
        };

        var aiResponse = AIResponse.CreateSuccess(
            @"[{
                ""vulnerabilityType"": ""PlainTextPassword"",
                ""severity"": ""Critical"",
                ""description"": ""Password stored in plain text"",
                ""columnName"": ""Password"",
                ""recommendation"": ""Use eno_Password with encryption""
            }]");

        this.mockAIService
            .Setup(x => x.CompleteAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await this.service.FindVulnerabilitiesAsync(table);

        // Assert
        result.Should().HaveCount(1);
        result[0].VulnerabilityType.Should().Be("PlainTextPassword");
        result[0].Severity.Should().Be(SecuritySeverity.Critical);
        result[0].ColumnName.Should().Be("Password");
        result[0].TableName.Should().Be("Users");
    }

    [Fact]
    public async Task FindVulnerabilitiesAsync_WithNoIssues_ReturnsEmptyList()
    {
        // Arrange
        var table = new Table
        {
            Name = "Users",
            SchemaName = "dbo",
            Columns = new List<Column>
            {
                new Column { Name = "UserId", DataType = "INT", IsNullable = false, IsPrimaryKey = true },
            },
        };

        var aiResponse = AIResponse.CreateSuccess("[]");

        this.mockAIService
            .Setup(x => x.CompleteAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await this.service.FindVulnerabilitiesAsync(table);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindVulnerabilitiesAsync_WithInvalidJson_ReturnsEmptyList()
    {
        // Arrange
        var table = new Table
        {
            Name = "Users",
            SchemaName = "dbo",
            Columns = new List<Column>(),
        };

        var aiResponse = AIResponse.CreateSuccess("Invalid JSON content");

        this.mockAIService
            .Setup(x => x.CompleteAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await this.service.FindVulnerabilitiesAsync(table);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindVulnerabilitiesAsync_WithNullTable_ThrowsArgumentNullException()
    {
        // Act
        var act = async () => await this.service.FindVulnerabilitiesAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("table");
    }

    [Fact]
    public async Task GetPrefixRecommendationsAsync_WithSensitiveColumn_ReturnsRecommendation()
    {
        // Arrange
        var table = new Table
        {
            Name = "Users",
            SchemaName = "dbo",
            Columns = new List<Column>
            {
                new Column { Name = "SSN", DataType = "NVARCHAR(11)", IsNullable = false, IsPrimaryKey = false },
            },
        };

        var aiResponse = AIResponse.CreateSuccess(
            @"[{
                ""currentColumnName"": ""SSN"",
                ""recommendedPrefix"": ""eno_"",
                ""recommendedColumnName"": ""eno_SSN"",
                ""reason"": ""Social Security Number should be encrypted"",
                ""severity"": ""High""
            }]");

        this.mockAIService
            .Setup(x => x.CompleteAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await this.service.GetPrefixRecommendationsAsync(table);

        // Assert
        result.Should().HaveCount(1);
        result[0].CurrentColumnName.Should().Be("SSN");
        result[0].RecommendedPrefix.Should().Be("eno_");
        result[0].RecommendedColumnName.Should().Be("eno_SSN");
        result[0].Severity.Should().Be(SecuritySeverity.High);
    }

    [Fact]
    public async Task GetPrefixRecommendationsAsync_WithTemporalColumn_ReturnsEntPrefix()
    {
        // Arrange
        var table = new Table
        {
            Name = "Orders",
            SchemaName = "dbo",
            Columns = new List<Column>
            {
                new Column { Name = "ValidFrom", DataType = "DATETIME", IsNullable = false, IsPrimaryKey = false },
                new Column { Name = "ValidTo", DataType = "DATETIME", IsNullable = false, IsPrimaryKey = false },
            },
        };

        var aiResponse = AIResponse.CreateSuccess(
            @"[{
                ""currentColumnName"": ""ValidFrom"",
                ""recommendedPrefix"": ""ent_"",
                ""recommendedColumnName"": ""ent_ValidFrom"",
                ""reason"": ""Temporal tracking column"",
                ""severity"": ""Medium""
            }]");

        this.mockAIService
            .Setup(x => x.CompleteAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await this.service.GetPrefixRecommendationsAsync(table);

        // Assert
        result.Should().HaveCount(1);
        result[0].RecommendedPrefix.Should().Be("ent_");
    }

    [Fact]
    public async Task GetPrefixRecommendationsAsync_WithNullTable_ThrowsArgumentNullException()
    {
        // Act
        var act = async () => await this.service.GetPrefixRecommendationsAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("table");
    }

    [Fact]
    public async Task GetEncryptionSuggestionsAsync_WithCreditCardColumn_ReturnsSuggestion()
    {
        // Arrange
        var table = new Table
        {
            Name = "Payments",
            SchemaName = "dbo",
            Columns = new List<Column>
            {
                new Column { Name = "CardNumber", DataType = "NVARCHAR(16)", IsNullable = false, IsPrimaryKey = false },
            },
        };

        var aiResponse = AIResponse.CreateSuccess(
            @"[{
                ""columnName"": ""CardNumber"",
                ""sensitiveDataType"": ""CreditCard"",
                ""recommendedEncryptionMethod"": ""AES-256"",
                ""reason"": ""PCI-DSS compliance requirement"",
                ""severity"": ""High"",
                ""recommendedColumnName"": ""eno_CardNumber""
            }]");

        this.mockAIService
            .Setup(x => x.CompleteAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await this.service.GetEncryptionSuggestionsAsync(table);

        // Assert
        result.Should().HaveCount(1);
        result[0].ColumnName.Should().Be("CardNumber");
        result[0].SensitiveDataType.Should().Be("CreditCard");
        result[0].RecommendedEncryptionMethod.Should().Be("AES-256");
        result[0].RecommendedColumnName.Should().Be("eno_CardNumber");
    }

    [Fact]
    public async Task GetEncryptionSuggestionsAsync_WithNullTable_ThrowsArgumentNullException()
    {
        // Act
        var act = async () => await this.service.GetEncryptionSuggestionsAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("table");
    }

    [Fact]
    public async Task GenerateSecurityReportAsync_WithMultipleIssues_ReturnsComprehensiveReport()
    {
        // Arrange
        var table = new Table
        {
            Name = "Users",
            SchemaName = "dbo",
            Columns = new List<Column>
            {
                new Column { Name = "Password", DataType = "NVARCHAR(100)", IsNullable = false, IsPrimaryKey = false },
                new Column { Name = "SSN", DataType = "NVARCHAR(11)", IsNullable = false, IsPrimaryKey = false },
            },
        };

        var vulnerabilityResponse = AIResponse.CreateSuccess(
            @"[{
                ""vulnerabilityType"": ""PlainTextPassword"",
                ""severity"": ""Critical"",
                ""description"": ""Password stored in plain text"",
                ""columnName"": ""Password"",
                ""recommendation"": ""Use encryption""
            }]");

        var prefixResponse = AIResponse.CreateSuccess(
            @"[{
                ""currentColumnName"": ""SSN"",
                ""recommendedPrefix"": ""eno_"",
                ""recommendedColumnName"": ""eno_SSN"",
                ""reason"": ""Sensitive data"",
                ""severity"": ""High""
            }]");

        var encryptionResponse = AIResponse.CreateSuccess(
            @"[{
                ""columnName"": ""SSN"",
                ""sensitiveDataType"": ""SSN"",
                ""recommendedEncryptionMethod"": ""AES-256"",
                ""reason"": ""PII data"",
                ""severity"": ""High""
            }]");

        this.mockAIService
            .SetupSequence(x => x.CompleteAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vulnerabilityResponse)
            .ReturnsAsync(prefixResponse)
            .ReturnsAsync(encryptionResponse);

        // Act
        var result = await this.service.GenerateSecurityReportAsync(table);

        // Assert
        result.Scope.Should().Be("Users");
        result.Vulnerabilities.Should().HaveCount(1);
        result.PrefixRecommendations.Should().HaveCount(1);
        result.EncryptionSuggestions.Should().HaveCount(1);
        result.TotalIssuesCount.Should().Be(3);
        result.CriticalIssuesCount.Should().Be(1);
        result.HighSeverityCount.Should().Be(2);
        result.SecurityScore.Should().BeLessThan(100);
        result.Summary.Should().Contain("CRITICAL");
    }

    [Fact]
    public async Task GenerateSecurityReportAsync_WithNoIssues_ReturnsCleanReport()
    {
        // Arrange
        var table = new Table
        {
            Name = "Users",
            SchemaName = "dbo",
            Columns = new List<Column>
            {
                new Column { Name = "UserId", DataType = "INT", IsNullable = false, IsPrimaryKey = true },
            },
        };

        var emptyResponse = AIResponse.CreateSuccess("[]");

        this.mockAIService
            .Setup(x => x.CompleteAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResponse);

        // Act
        var result = await this.service.GenerateSecurityReportAsync(table);

        // Assert
        result.TotalIssuesCount.Should().Be(0);
        result.SecurityScore.Should().Be(100);
        result.Summary.Should().NotContain("CRITICAL");
    }

    [Fact]
    public async Task GenerateSecurityReportAsync_WithNullTable_ThrowsArgumentNullException()
    {
        // Act
        var act = async () => await this.service.GenerateSecurityReportAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("table");
    }

    [Fact]
    public async Task ScanMultipleTablesAsync_WithTwoTables_ReturnsCombinedReport()
    {
        // Arrange
        var tables = new List<Table>
        {
            new Table
            {
                Name = "Users",
                SchemaName = "dbo",
                Columns = new List<Column> { new Column { Name = "Password", DataType = "NVARCHAR", IsNullable = false, IsPrimaryKey = false } },
            },
            new Table
            {
                Name = "Orders",
                SchemaName = "dbo",
                Columns = new List<Column> { new Column { Name = "CardNumber", DataType = "NVARCHAR", IsNullable = false, IsPrimaryKey = false } },
            },
        };

        var vulnerabilityResponse = AIResponse.CreateSuccess(
            @"[{
                ""vulnerabilityType"": ""PlainTextPassword"",
                ""severity"": ""Critical"",
                ""description"": ""Password issue"",
                ""columnName"": ""Password"",
                ""recommendation"": ""Encrypt""
            }]");

        var emptyResponse = AIResponse.CreateSuccess("[]");

        this.mockAIService
            .Setup(x => x.CompleteAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AIRequest req, CancellationToken ct) =>
                req.Prompt.Contains("Password") ? vulnerabilityResponse : emptyResponse);

        // Act
        var result = await this.service.ScanMultipleTablesAsync(tables);

        // Assert
        result.Scope.Should().Be("2 tables");
        result.CriticalIssuesCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ScanMultipleTablesAsync_WithEmptyList_ThrowsArgumentException()
    {
        // Act
        var act = async () => await this.service.ScanMultipleTablesAsync(Array.Empty<Table>());

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*At least one table*");
    }

    [Fact]
    public async Task ScanMultipleTablesAsync_WithNullTables_ThrowsArgumentNullException()
    {
        // Act
        var act = async () => await this.service.ScanMultipleTablesAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("tables");
    }

    [Theory]
    [InlineData("CRITICAL", SecuritySeverity.Critical)]
    [InlineData("HIGH", SecuritySeverity.High)]
    [InlineData("MEDIUM", SecuritySeverity.Medium)]
    [InlineData("LOW", SecuritySeverity.Low)]
    [InlineData("unknown", SecuritySeverity.Low)]
    [InlineData(null, SecuritySeverity.Low)]
    public async Task FindVulnerabilitiesAsync_ParsesSeverityCorrectly(string severity, SecuritySeverity expected)
    {
        // Arrange
        var table = new Table { Name = "Test", SchemaName = "dbo", Columns = new List<Column>() };

        var aiResponse = AIResponse.CreateSuccess(
            $@"[{{
                ""vulnerabilityType"": ""Test"",
                ""severity"": ""{severity}"",
                ""description"": ""Test"",
                ""columnName"": ""Test"",
                ""recommendation"": ""Test""
            }}]");

        this.mockAIService
            .Setup(x => x.CompleteAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await this.service.FindVulnerabilitiesAsync(table);

        // Assert
        result.Should().HaveCount(1);
        result[0].Severity.Should().Be(expected);
    }

    [Fact]
    public void SecurityReport_CalculatesCountsCorrectly()
    {
        // Arrange
        var report = new SecurityReport
        {
            Scope = "Test",
            GeneratedAt = DateTime.UtcNow,
            SecurityScore = 50,
            Summary = "Test",
            Vulnerabilities = new List<SecurityVulnerability>
            {
                new SecurityVulnerability
                {
                    VulnerabilityType = "Test1",
                    Severity = SecuritySeverity.Critical,
                    Description = "Test",
                    ColumnName = "Col1",
                    TableName = "Table1",
                    Recommendation = "Fix",
                },
                new SecurityVulnerability
                {
                    VulnerabilityType = "Test2",
                    Severity = SecuritySeverity.High,
                    Description = "Test",
                    ColumnName = "Col2",
                    TableName = "Table1",
                    Recommendation = "Fix",
                },
            },
            PrefixRecommendations = new List<PrefixRecommendation>
            {
                new PrefixRecommendation
                {
                    CurrentColumnName = "Test",
                    TableName = "Table1",
                    RecommendedPrefix = "eno_",
                    RecommendedColumnName = "eno_Test",
                    Reason = "Encrypt",
                    Severity = SecuritySeverity.Medium,
                },
            },
            EncryptionSuggestions = new List<EncryptionSuggestion>
            {
                new EncryptionSuggestion
                {
                    ColumnName = "Test",
                    TableName = "Table1",
                    SensitiveDataType = "SSN",
                    RecommendedEncryptionMethod = "AES",
                    Reason = "PII",
                    Severity = SecuritySeverity.Low,
                },
            },
        };

        // Assert
        report.CriticalIssuesCount.Should().Be(1);
        report.HighSeverityCount.Should().Be(1);
        report.MediumSeverityCount.Should().Be(1);
        report.LowSeverityCount.Should().Be(1);
        report.TotalIssuesCount.Should().Be(4);
    }
}
