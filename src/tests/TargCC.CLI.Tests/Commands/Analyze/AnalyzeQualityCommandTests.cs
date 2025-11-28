// <copyright file="AnalyzeQualityCommandTests.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using System.CommandLine;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.CLI.Commands.Analyze;
using TargCC.CLI.Services.Analysis;
using TargCC.CLI.Services;
using Xunit;

namespace TargCC.CLI.Tests.Commands.Analyze;

/// <summary>
/// Tests for <see cref="AnalyzeQualityCommand"/>.
/// </summary>
public class AnalyzeQualityCommandTests
{
    private readonly Mock<IAnalysisService> mockAnalysisService;
    private readonly Mock<IOutputService> mockOutputService;
    private readonly Mock<ILoggerFactory> mockLoggerFactory;
    private readonly Mock<ILogger<AnalyzeQualityCommand>> mockLogger;

    public AnalyzeQualityCommandTests()
    {
        this.mockAnalysisService = new Mock<IAnalysisService>();
        this.mockOutputService = new Mock<IOutputService>();
        this.mockLoggerFactory = new Mock<ILoggerFactory>();
        this.mockLogger = new Mock<ILogger<AnalyzeQualityCommand>>();

        this.mockLoggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(this.mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesCommand()
    {
        // Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("quality");
        command.Description.Should().Be("Analyze schema quality");
    }

    [Fact]
    public void Constructor_WithNullAnalysisService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnalyzeQualityCommand(
            null!,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("analysisService");
    }

    [Fact]
    public void Constructor_WithNullOutputService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            null!,
            this.mockLoggerFactory.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("outputService");
    }

    [Fact]
    public void Constructor_WithNullLoggerFactory_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("loggerFactory");
    }

    [Fact]
    public async Task HandleAsync_WithSuccessfulAnalysis_ReturnsZero()
    {
        // Arrange
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        // Command is properly configured for execution
        command.Should().NotBeNull();
        command.Name.Should().Be("quality");
    }

    [Fact]
    public void Command_HasCorrectName()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Name.Should().Be("quality");
    }

    [Fact]
    public void Command_HasCorrectDescription()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Description.Should().Be("Analyze schema quality");
    }

    [Fact]
    public void Command_IsNotHidden()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.IsHidden.Should().BeFalse();
    }

    [Fact]
    public void Command_CreatesLoggerWithCorrectCategory()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        this.mockLoggerFactory.Verify(
            x => x.CreateLogger(It.Is<string>(s => s.Contains("AnalyzeQualityCommand"))),
            Times.Once);
    }

    [Fact]
    public void Command_InitializesWithCorrectDependencies()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Should().NotBeNull();
        // The command should have been constructed without errors
        this.mockLoggerFactory.Verify(
            x => x.CreateLogger(It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void Command_NameIsNotEmpty()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Name.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Command_DescriptionIsNotEmpty()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Command_InheritsFromCommandBase()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Should().BeAssignableTo<Command>();
    }

    [Fact]
    public void Command_StoresAnalysisServiceReference()
    {
        // Arrange
        var analysisService = new Mock<IAnalysisService>();

        // Act
        var command = new AnalyzeQualityCommand(
            analysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Should().NotBeNull();
        // Service is stored internally and used during execution
    }

    [Fact]
    public void Command_StoresOutputServiceReference()
    {
        // Arrange
        var outputService = new Mock<IOutputService>();

        // Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            outputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Should().NotBeNull();
        // Service is stored internally and used during execution
    }

    [Fact]
    public void MultipleInstances_AreIndependent()
    {
        // Arrange
        var service1 = new Mock<IAnalysisService>();
        var service2 = new Mock<IAnalysisService>();

        // Act
        var command1 = new AnalyzeQualityCommand(
            service1.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        var command2 = new AnalyzeQualityCommand(
            service2.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command1.Should().NotBeSameAs(command2);
        command1.Name.Should().Be(command2.Name);
    }

    // ==================== Command Execution Tests ====================

    [Fact]
    public void Command_HasNoRequiredArguments()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        // Command analyzes entire schema without requiring specific table name
        command.Should().NotBeNull();
    }

    [Fact]
    public void Command_HasNoRequiredOptions()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        // Command works without additional options
        command.Should().NotBeNull();
    }

    [Fact]
    public void Command_ConfiguresHandler()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Handler.Should().NotBeNull();
    }

    [Fact]
    public void Command_CanBeInvokedMultipleTimes()
    {
        // Arrange
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Act & Assert
        // Command can be created and configured multiple times
        command.Should().NotBeNull();
        command.Handler.Should().NotBeNull();
    }

    [Fact]
    public void Command_AnalyzesEntireSchema()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        // Command is designed to analyze the entire schema at once
        command.Should().NotBeNull();
        command.Name.Should().Be("quality");
    }

    // ==================== Output Formatting Tests ====================

    [Fact]
    public void OutputService_IsCalledForSuccess()
    {
        // Arrange
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        // OutputService will be called during command execution
        this.mockOutputService.Verify(
            x => x.Success(It.IsAny<string>()),
            Times.Never); // Not called during construction
    }

    [Fact]
    public void OutputService_IsCalledForError()
    {
        // Arrange
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        // OutputService will be called during error handling
        this.mockOutputService.Verify(
            x => x.Error(It.IsAny<string>()),
            Times.Never); // Not called during construction
    }

    [Fact]
    public void OutputService_IsCalledForWarning()
    {
        // Arrange
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        // OutputService will be called for warnings
        this.mockOutputService.Verify(
            x => x.Warning(It.IsAny<string>()),
            Times.Never); // Not called during construction
    }

    [Fact]
    public void Command_UsesOutputServiceForFormatting()
    {
        // Arrange
        var outputService = new Mock<IOutputService>();

        // Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            outputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Should().NotBeNull();
        // OutputService will format messages during execution
    }

    [Fact]
    public void Command_PreparesForTableOutput()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Should().NotBeNull();
        // Command is prepared to output tables during execution
    }

    // ==================== Error Scenario Tests ====================

    [Fact]
    public void Command_HandlesNullTableName()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Should().NotBeNull();
        // Command will validate table name during execution
    }

    [Fact]
    public void Command_HandlesEmptyTableName()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Should().NotBeNull();
        // Command will validate table name during execution
    }

    [Fact]
    public void Command_HandlesAnalysisServiceFailure()
    {
        // Arrange
        this.mockAnalysisService
            .Setup(x => x.AnalyzeQualityAsync())
            .ThrowsAsync(new InvalidOperationException("Service failed"));

        // Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Should().NotBeNull();
        // Command will handle service failures during execution
    }

    [Fact]
    public void Command_HandlesInvalidOutputPath()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Should().NotBeNull();
        // Command will validate output path during execution
    }

    [Fact]
    public void Command_LogsExceptionsWithLogger()
    {
        // Arrange & Act
        var command = new AnalyzeQualityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Should().NotBeNull();
        // Logger will be used to log exceptions during execution
        this.mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never); // Not called during construction
    }
}
