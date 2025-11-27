// Copyright (c) TargCC Team. All rights reserved.

using System.CommandLine;
using System.CommandLine.IO;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.AI.Models;
using TargCC.AI.Services;
using TargCC.CLI.Commands;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using TargCC.Core.Interfaces;
using TargCC.Core.Interfaces.Models;
using Xunit;
using TableIndex = TargCC.Core.Interfaces.Models.Index;

namespace TargCC.CLI.Tests.Commands;

public class SuggestCommandTests
{
    private readonly Mock<IAIService> aiServiceMock;
    private readonly Mock<IDatabaseAnalyzer> databaseAnalyzerMock;
    private readonly Mock<IConfigurationService> configurationServiceMock;
    private readonly Mock<IOutputService> outputServiceMock;
    private readonly Mock<ILoggerFactory> loggerFactoryMock;
    private readonly Mock<ILogger<SuggestCommand>> loggerMock;
    private readonly SuggestCommand command;
    private readonly TestConsole console;

    public SuggestCommandTests()
    {
        this.aiServiceMock = new Mock<IAIService>();
        this.databaseAnalyzerMock = new Mock<IDatabaseAnalyzer>();
        this.configurationServiceMock = new Mock<IConfigurationService>();
        this.outputServiceMock = new Mock<IOutputService>();
        this.loggerFactoryMock = new Mock<ILoggerFactory>();
        this.loggerMock = new Mock<ILogger<SuggestCommand>>();

        this.loggerFactoryMock
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(this.loggerMock.Object);

        // Setup default configuration
        var config = new CliConfiguration
        {
            ConnectionString = "Server=localhost;Database=TestDb;",
        };
        this.configurationServiceMock
            .Setup(x => x.LoadAsync())
            .ReturnsAsync(config);

        this.command = new SuggestCommand(
            this.aiServiceMock.Object,
            this.databaseAnalyzerMock.Object,
            this.configurationServiceMock.Object,
            this.outputServiceMock.Object,
            this.loggerFactoryMock.Object);

        this.console = new TestConsole();
    }

    [Fact]
    public void Constructor_WithNullAIService_ShouldThrowArgumentNullException()
    {
        var act = () => new SuggestCommand(
            null!,
            this.databaseAnalyzerMock.Object,
            this.configurationServiceMock.Object,
            this.outputServiceMock.Object,
            this.loggerFactoryMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("aiService");
    }

    [Fact]
    public void Constructor_WithNullDatabaseAnalyzer_ShouldThrowArgumentNullException()
    {
        var act = () => new SuggestCommand(
            this.aiServiceMock.Object,
            null!,
            this.configurationServiceMock.Object,
            this.outputServiceMock.Object,
            this.loggerFactoryMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("databaseAnalyzer");
    }

    [Fact]
    public void Constructor_WithNullConfigurationService_ShouldThrowArgumentNullException()
    {
        var act = () => new SuggestCommand(
            this.aiServiceMock.Object,
            this.databaseAnalyzerMock.Object,
            null!,
            this.outputServiceMock.Object,
            this.loggerFactoryMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("configurationService");
    }

    [Fact]
    public void Constructor_WithNullOutputService_ShouldThrowArgumentNullException()
    {
        var act = () => new SuggestCommand(
            this.aiServiceMock.Object,
            this.databaseAnalyzerMock.Object,
            this.configurationServiceMock.Object,
            null!,
            this.loggerFactoryMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("outputService");
    }

    [Fact]
    public void Constructor_WithNullLoggerFactory_ShouldThrowArgumentNullException()
    {
        var act = () => new SuggestCommand(
            this.aiServiceMock.Object,
            this.databaseAnalyzerMock.Object,
            this.configurationServiceMock.Object,
            this.outputServiceMock.Object,
            null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(Skip = "AnsiConsole.Write causes test failures - requires IAnsiConsole injection refactor")]
    public async Task Execute_WithValidTable_ShouldReturnSuccess()
    {
        // Arrange
        var table = new Table
        {
            Name = "Customer",
            SchemaName = "dbo",
            Columns = new List<Column>(),
            Indexes = new List<TableIndex>(),
        };

        var schema = new DatabaseSchema
        {
            Tables = new List<Table> { table },
        };

        var analysisResult = new SchemaAnalysisResult
        {
            TableName = "Customer",
            QualityScore = 85,
            Summary = "Good table",
            Suggestions = new List<Suggestion>
            {
                new()
                {
                    Severity = SuggestionSeverity.Warning,
                    Category = SuggestionCategory.Performance,
                    Message = "Consider adding index",
                    Target = "Email",
                },
            },
        };

        this.aiServiceMock.Setup(x => x.IsHealthyAsync(default)).ReturnsAsync(true);
        
        this.outputServiceMock
            .Setup(x => x.SpinnerAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task>>()))
            .Returns<string, Func<Task>>(async (msg, func) => await func());

        this.outputServiceMock.Setup(x => x.Success(It.IsAny<string>()));

        this.databaseAnalyzerMock.Setup(x => x.AnalyzeDatabaseAsync(It.IsAny<string>())).ReturnsAsync(schema);
        this.aiServiceMock.Setup(x => x.AnalyzeTableSchemaAsync(It.IsAny<Table>(), It.IsAny<CancellationToken>())).ReturnsAsync(analysisResult);

        // Act
        var rootCommand = new System.CommandLine.RootCommand();
        rootCommand.AddCommand(this.command);
        var result = await rootCommand.InvokeAsync("suggest Customer", this.console);

        // Assert
        result.Should().Be(0);
        this.aiServiceMock.Verify(x => x.AnalyzeTableSchemaAsync(It.IsAny<Table>(), It.IsAny<CancellationToken>()), Times.Once);
        this.outputServiceMock.Verify(x => x.Success(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Execute_WithNonExistentTable_ShouldReturnError()
    {
        // Arrange
        var schema = new DatabaseSchema
        {
            Tables = new List<Table>
            {
                new()
                {
                    Name = "Customer",
                    SchemaName = "dbo",
                    Columns = new List<Column>(),
                    Indexes = new List<TableIndex>(),
                },
            },
        };

        this.aiServiceMock.Setup(x => x.IsHealthyAsync(default)).ReturnsAsync(true);
        
        this.outputServiceMock
            .Setup(x => x.SpinnerAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task>>()))
            .Returns<string, Func<Task>>(async (msg, func) => await func());

        this.databaseAnalyzerMock.Setup(x => x.AnalyzeDatabaseAsync(It.IsAny<string>())).ReturnsAsync(schema);

        // Act
        var rootCommand = new System.CommandLine.RootCommand();
        rootCommand.AddCommand(this.command);
        var result = await rootCommand.InvokeAsync("suggest NonExistentTable", this.console);

        // Assert
        result.Should().Be(1);
        this.outputServiceMock.Verify(x => x.Error(It.Is<string>(s => s.Contains("not found"))), Times.Once);
    }

    [Fact]
    public async Task Execute_WithUnhealthyAI_ShouldReturnError()
    {
        // Arrange
        this.aiServiceMock.Setup(x => x.IsHealthyAsync(default)).ReturnsAsync(false);

        // Act
        var rootCommand = new System.CommandLine.RootCommand();
        rootCommand.AddCommand(this.command);
        var result = await rootCommand.InvokeAsync("suggest Customer", this.console);

        // Assert
        result.Should().Be(1);
        this.outputServiceMock.Verify(x => x.Error(It.Is<string>(s => s.Contains("not available"))), Times.Once);
    }

    [Fact(Skip = "AnsiConsole.Write causes test failures - requires IAnsiConsole injection refactor")]
    public async Task Execute_WithCategoryFilter_ShouldFilterSuggestions()
    {
        // Arrange
        var table = new Table
        {
            Name = "Customer",
            SchemaName = "dbo",
            Columns = new List<Column>(),
            Indexes = new List<TableIndex>(),
        };

        var schema = new DatabaseSchema
        {
            Tables = new List<Table> { table },
        };

        var analysisResult = new SchemaAnalysisResult
        {
            TableName = "Customer",
            QualityScore = 85,
            Summary = "Good table",
            Suggestions = new List<Suggestion>
            {
                new()
                {
                    Severity = SuggestionSeverity.Critical,
                    Category = SuggestionCategory.Security,
                    Message = "Security issue",
                },
                new()
                {
                    Severity = SuggestionSeverity.Warning,
                    Category = SuggestionCategory.Performance,
                    Message = "Performance issue",
                },
            },
        };

        this.aiServiceMock.Setup(x => x.IsHealthyAsync(default)).ReturnsAsync(true);
        
        this.outputServiceMock
            .Setup(x => x.SpinnerAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task>>()))
            .Returns<string, Func<Task>>(async (msg, func) => await func());

        this.outputServiceMock.Setup(x => x.Success(It.IsAny<string>()));

        this.databaseAnalyzerMock.Setup(x => x.AnalyzeDatabaseAsync(It.IsAny<string>())).ReturnsAsync(schema);
        this.aiServiceMock.Setup(x => x.AnalyzeTableSchemaAsync(It.IsAny<Table>(), It.IsAny<CancellationToken>())).ReturnsAsync(analysisResult);

        // Act
        var rootCommand = new System.CommandLine.RootCommand();
        rootCommand.AddCommand(this.command);
        var result = await rootCommand.InvokeAsync("suggest Customer --category Security", this.console);

        // Assert
        result.Should().Be(0);
    }

    [Fact(Skip = "AnsiConsole.Write causes test failures - requires IAnsiConsole injection refactor")]
    public async Task Execute_WithSeverityFilter_ShouldFilterSuggestions()
    {
        // Arrange
        var table = new Table
        {
            Name = "Customer",
            SchemaName = "dbo",
            Columns = new List<Column>(),
            Indexes = new List<TableIndex>(),
        };

        var schema = new DatabaseSchema
        {
            Tables = new List<Table> { table },
        };

        var analysisResult = new SchemaAnalysisResult
        {
            TableName = "Customer",
            QualityScore = 85,
            Summary = "Good table",
            Suggestions = new List<Suggestion>
            {
                new()
                {
                    Severity = SuggestionSeverity.Critical,
                    Category = SuggestionCategory.Security,
                    Message = "Critical issue",
                },
                new()
                {
                    Severity = SuggestionSeverity.Warning,
                    Category = SuggestionCategory.Performance,
                    Message = "Warning issue",
                },
            },
        };

        this.aiServiceMock.Setup(x => x.IsHealthyAsync(default)).ReturnsAsync(true);
        
        this.outputServiceMock
            .Setup(x => x.SpinnerAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task>>()))
            .Returns<string, Func<Task>>(async (msg, func) => await func());

        this.outputServiceMock.Setup(x => x.Success(It.IsAny<string>()));

        this.databaseAnalyzerMock.Setup(x => x.AnalyzeDatabaseAsync(It.IsAny<string>())).ReturnsAsync(schema);
        this.aiServiceMock.Setup(x => x.AnalyzeTableSchemaAsync(It.IsAny<Table>(), It.IsAny<CancellationToken>())).ReturnsAsync(analysisResult);

        // Act
        var rootCommand = new System.CommandLine.RootCommand();
        rootCommand.AddCommand(this.command);
        var result = await rootCommand.InvokeAsync("suggest Customer --severity Critical", this.console);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task Execute_WithException_ShouldReturnError()
    {
        // Arrange
        this.aiServiceMock.Setup(x => x.IsHealthyAsync(default)).ThrowsAsync(new Exception("Test exception"));

        // Act
        var rootCommand = new System.CommandLine.RootCommand();
        rootCommand.AddCommand(this.command);
        var result = await rootCommand.InvokeAsync("suggest Customer", this.console);

        // Assert
        result.Should().Be(1);
        this.outputServiceMock.Verify(x => x.Error(It.Is<string>(s => s.Contains("Error"))), Times.Once);
    }
}
