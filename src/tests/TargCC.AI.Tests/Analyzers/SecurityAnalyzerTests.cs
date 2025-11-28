// <copyright file="SecurityAnalyzerTests.cs" company="Doron Aharoni">
// Copyright (c) Doron Aharoni. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.AI.Analyzers;
using TargCC.AI.Models;
using TargCC.Core.Interfaces.Models;
using Xunit;

namespace TargCC.AI.Tests.Analyzers;

/// <summary>
/// Tests for <see cref="SecurityAnalyzer"/>.
/// </summary>
public sealed class SecurityAnalyzerTests
{
    private readonly Mock<ILogger<SecurityAnalyzer>> mockLogger;
    private readonly SecurityAnalyzer analyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityAnalyzerTests"/> class.
    /// </summary>
    public SecurityAnalyzerTests()
    {
        this.mockLogger = new Mock<ILogger<SecurityAnalyzer>>();
        this.analyzer = new SecurityAnalyzer(this.mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new SecurityAnalyzer(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithValidLogger_ShouldCreateInstance()
    {
        // Act
        var analyzer = new SecurityAnalyzer(this.mockLogger.Object);

        // Assert
        analyzer.Should().NotBeNull();
    }

    #endregion

    #region AnalyzeTableSecurityAsync Tests

    [Fact]
    public async Task AnalyzeTableSecurityAsync_WithNullTable_ShouldThrowArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await this.analyzer.AnalyzeTableSecurityAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AnalyzeTableSecurityAsync_WithEmptyTable_ShouldReturnCleanResult()
    {
        // Arrange
        var table = new Table
        {
            Name = "EmptyTable",
            Columns = new List<Column>(),
        };

        // Act
        var result = await this.analyzer.AnalyzeTableSecurityAsync(table, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TableName.Should().Be("EmptyTable");
        result.Vulnerabilities.Should().BeEmpty();
        result.PrefixRecommendations.Should().BeEmpty();
        result.EncryptionSuggestions.Should().BeEmpty();
        result.OverallScore.Score.Should().Be(100);
        result.OverallScore.Grade.Should().Be("A");
    }

    #endregion

    #region DetectVulnerabilities Tests

    [Fact]
    public void DetectVulnerabilities_WithPlainTextPassword_ShouldDetectCriticalVulnerability()
    {
        // Arrange
        var table = new Table
        {
            Name = "Users",
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "Password",
                    DataType = "nvarchar(255)",
                    IsNullable = false,
                    IsPrimaryKey = false,
                },
            },
        };

        // Act
        var vulnerabilities = this.analyzer.DetectVulnerabilities(table);

        // Assert
        // Password triggers both PlainTextPassword AND UnencryptedSensitiveData
        vulnerabilities.Should().HaveCount(2);
        vulnerabilities.Should().Contain(v => v.VulnerabilityType == "PlainTextPassword");
        vulnerabilities.Should().Contain(v => v.VulnerabilityType == "UnencryptedSensitiveData");
        vulnerabilities.Should().AllSatisfy(v =>
        {
            v.Severity.Should().Be(SecuritySeverity.Critical);
            v.ColumnName.Should().Be("Password");
            v.TableName.Should().Be("Users");
        });
    }

    [Fact]
    public void DetectVulnerabilities_WithEncryptedPassword_ShouldNotDetectVulnerability()
    {
        // Arrange
        var table = new Table
        {
            Name = "Users",
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "eno_Password",
                    DataType = "nvarchar(255)",
                    IsNullable = false,
                    IsPrimaryKey = false,
                },
            },
        };

        // Act
        var vulnerabilities = this.analyzer.DetectVulnerabilities(table);

        // Assert
        vulnerabilities.Should().BeEmpty();
    }

    [Fact]
    public void DetectVulnerabilities_WithUnencryptedSSN_ShouldDetectCriticalVulnerability()
    {
        // Arrange
        var table = new Table
        {
            Name = "Customers",
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "SSN",
                    DataType = "varchar(11)",
                    IsNullable = false,
                    IsPrimaryKey = false,
                },
            },
        };

        // Act
        var vulnerabilities = this.analyzer.DetectVulnerabilities(table);

        // Assert
        vulnerabilities.Should().HaveCount(1);
        var vuln = vulnerabilities[0];
        vuln.VulnerabilityType.Should().Be("UnencryptedSensitiveData");
        vuln.Severity.Should().Be(SecuritySeverity.Critical);
        vuln.ColumnName.Should().Be("SSN");
        vuln.Recommendation.Should().Contain("ent_");
    }

    [Fact]
    public void DetectVulnerabilities_WithUnencryptedCreditCard_ShouldDetectCriticalVulnerability()
    {
        // Arrange
        var table = new Table
        {
            Name = "PaymentMethods",
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "CreditCardNumber",
                    DataType = "varchar(16)",
                    IsNullable = false,
                    IsPrimaryKey = false,
                },
            },
        };

        // Act
        var vulnerabilities = this.analyzer.DetectVulnerabilities(table);

        // Assert
        vulnerabilities.Should().HaveCount(1);
        var vuln = vulnerabilities[0];
        vuln.Severity.Should().Be(SecuritySeverity.Critical);
    }

    [Fact]
    public void DetectVulnerabilities_WithUnencryptedSalary_ShouldDetectHighVulnerability()
    {
        // Arrange
        var table = new Table
        {
            Name = "Employees",
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "Salary",
                    DataType = "decimal(18,2)",
                    IsNullable = false,
                    IsPrimaryKey = false,
                },
            },
        };

        // Act
        var vulnerabilities = this.analyzer.DetectVulnerabilities(table);

        // Assert
        vulnerabilities.Should().HaveCount(1);
        var vuln = vulnerabilities[0];
        vuln.Severity.Should().Be(SecuritySeverity.High);
        vuln.ColumnName.Should().Be("Salary");
        vuln.VulnerabilityType.Should().Be("UnencryptedSensitiveData");
    }

    #endregion

    #region GetPrefixRecommendations Tests

    [Fact]
    public void GetPrefixRecommendations_WithPasswordColumn_ShouldRecommendEnoPrefix()
    {
        // Arrange
        var table = new Table
        {
            Name = "Users",
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "Password",
                    DataType = "nvarchar(255)",
                    IsNullable = false,
                    IsPrimaryKey = false,
                },
            },
        };

        // Act
        var recommendations = this.analyzer.GetPrefixRecommendations(table);

        // Assert
        recommendations.Should().HaveCount(1);
        var rec = recommendations[0];
        rec.CurrentColumnName.Should().Be("Password");
        rec.RecommendedPrefix.Should().Be("eno_");
        rec.RecommendedColumnName.Should().Be("eno_Password");
        rec.Severity.Should().Be(SecuritySeverity.Critical);
        rec.Reason.Should().Contain("one-way encryption");
    }

    [Fact]
    public void GetPrefixRecommendations_WithSensitiveData_ShouldRecommendEntPrefix()
    {
        // Arrange
        var table = new Table
        {
            Name = "Customers",
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "SSN",
                    DataType = "varchar(11)",
                    IsNullable = false,
                    IsPrimaryKey = false,
                },
            },
        };

        // Act
        var recommendations = this.analyzer.GetPrefixRecommendations(table);

        // Assert
        recommendations.Should().HaveCount(1);
        var rec = recommendations[0];
        rec.RecommendedPrefix.Should().Be("ent_");
        rec.RecommendedColumnName.Should().Be("ent_SSN");
        rec.Severity.Should().Be(SecuritySeverity.Critical);
        rec.Reason.Should().Contain("two-way encryption");
    }

    [Fact]
    public void GetPrefixRecommendations_WithComputedColumn_ShouldRecommendClcPrefix()
    {
        // Arrange
        var table = new Table
        {
            Name = "Orders",
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "TotalPrice",
                    DataType = "decimal(18,2)",
                    IsNullable = false,
                    IsPrimaryKey = false,
                    IsComputed = true,
                },
            },
        };

        // Act
        var recommendations = this.analyzer.GetPrefixRecommendations(table);

        // Assert
        recommendations.Should().HaveCount(1);
        var rec = recommendations[0];
        rec.RecommendedPrefix.Should().Be("clc_");
        rec.RecommendedColumnName.Should().Be("clc_TotalPrice");
        rec.Severity.Should().Be(SecuritySeverity.Low);
        rec.Reason.Should().Contain("read-only");
    }

    [Fact]
    public void GetPrefixRecommendations_WithBusinessLogicFlag_ShouldRecommendBlgPrefix()
    {
        // Arrange
        var table = new Table
        {
            Name = "Users",
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "IsApproved",
                    DataType = "bit",
                    IsNullable = false,
                    IsPrimaryKey = false,
                },
            },
        };

        // Act
        var recommendations = this.analyzer.GetPrefixRecommendations(table);

        // Assert
        recommendations.Should().HaveCount(1);
        var rec = recommendations[0];
        rec.RecommendedPrefix.Should().Be("blg_");
        rec.RecommendedColumnName.Should().Be("blg_IsApproved");
        rec.Severity.Should().Be(SecuritySeverity.Medium);
    }

    [Fact]
    public void GetPrefixRecommendations_WithExistingPrefix_ShouldNotRecommend()
    {
        // Arrange
        var table = new Table
        {
            Name = "Users",
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "eno_Password",
                    DataType = "nvarchar(255)",
                    IsNullable = false,
                    IsPrimaryKey = false,
                },
            },
        };

        // Act
        var recommendations = this.analyzer.GetPrefixRecommendations(table);

        // Assert
        recommendations.Should().BeEmpty();
    }

    #endregion

    #region GetEncryptionSuggestions Tests

    [Fact]
    public void GetEncryptionSuggestions_WithPasswordColumn_ShouldSuggestSHA256()
    {
        // Arrange
        var table = new Table
        {
            Name = "Users",
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "Password",
                    DataType = "nvarchar(255)",
                    IsNullable = false,
                    IsPrimaryKey = false,
                },
            },
        };

        // Act
        var suggestions = this.analyzer.GetEncryptionSuggestions(table);

        // Assert
        suggestions.Should().HaveCount(1);
        var sugg = suggestions[0];
        sugg.ColumnName.Should().Be("Password");
        sugg.SensitiveDataType.Should().Be("Password");
        sugg.RecommendedEncryptionMethod.Should().Be("SHA256 One-Way Hash");
        sugg.Severity.Should().Be(SecuritySeverity.Critical);
        sugg.RecommendedColumnName.Should().Be("eno_Password");
    }

    [Fact]
    public void GetEncryptionSuggestions_WithSSN_ShouldSuggestAES256()
    {
        // Arrange
        var table = new Table
        {
            Name = "Customers",
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "SSN",
                    DataType = "varchar(11)",
                    IsNullable = false,
                    IsPrimaryKey = false,
                },
            },
        };

        // Act
        var suggestions = this.analyzer.GetEncryptionSuggestions(table);

        // Assert
        suggestions.Should().HaveCount(1);
        var sugg = suggestions[0];
        sugg.SensitiveDataType.Should().Be("Social Security Number");
        sugg.RecommendedEncryptionMethod.Should().Be("AES-256 Two-Way Encryption");
        sugg.Severity.Should().Be(SecuritySeverity.Critical);
        sugg.RecommendedColumnName.Should().Be("ent_SSN");
    }

    [Fact]
    public void GetEncryptionSuggestions_WithEncryptedColumn_ShouldNotSuggest()
    {
        // Arrange
        var table = new Table
        {
            Name = "Users",
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "eno_Password",
                    DataType = "nvarchar(255)",
                    IsNullable = false,
                    IsPrimaryKey = false,
                },
            },
        };

        // Act
        var suggestions = this.analyzer.GetEncryptionSuggestions(table);

        // Assert
        suggestions.Should().BeEmpty();
    }

    #endregion

    #region Security Score Tests

    [Fact]
    public async Task AnalyzeTableSecurityAsync_WithNoIssues_ShouldHaveScoreOf100()
    {
        // Arrange
        var table = new Table
        {
            Name = "Products",
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "ProductId",
                    DataType = "int",
                    IsNullable = false,
                    IsPrimaryKey = true,
                },
                new Column
                {
                    Name = "ProductName",
                    DataType = "nvarchar(100)",
                    IsNullable = false,
                    IsPrimaryKey = false,
                },
            },
        };

        // Act
        var result = await this.analyzer.AnalyzeTableSecurityAsync(table, CancellationToken.None);

        // Assert
        result.OverallScore.Score.Should().Be(100);
        result.OverallScore.Grade.Should().Be("A");
        result.OverallScore.CriticalCount.Should().Be(0);
        result.OverallScore.Summary.Should().Contain("no issues");
    }

    [Fact]
    public async Task AnalyzeTableSecurityAsync_WithCriticalIssues_ShouldHaveLowScore()
    {
        // Arrange
        var table = new Table
        {
            Name = "Users",
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "Password",
                    DataType = "nvarchar(255)",
                    IsNullable = false,
                    IsPrimaryKey = false,
                },
                new Column
                {
                    Name = "SSN",
                    DataType = "varchar(11)",
                    IsNullable = false,
                    IsPrimaryKey = false,
                },
            },
        };

        // Act
        var result = await this.analyzer.AnalyzeTableSecurityAsync(table, CancellationToken.None);

        // Assert
        result.OverallScore.Score.Should().BeLessThan(70);
        result.OverallScore.Grade.Should().BeOneOf("D", "F");
        result.OverallScore.CriticalCount.Should().BeGreaterThan(0);
        result.HasCriticalIssues.Should().BeTrue();
    }

    #endregion
}
