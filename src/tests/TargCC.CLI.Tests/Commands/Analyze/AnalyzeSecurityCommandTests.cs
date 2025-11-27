// <copyright file="AnalyzeSecurityCommandTests.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using TargCC.CLI.Commands.Analyze;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Analysis;


namespace TargCC.CLI.Tests.Commands.Analyze;

/// <summary>
/// Tests for <see cref="AnalyzeSecurityCommand"/>.
/// </summary>
public class AnalyzeSecurityCommandTests
{
    private readonly Mock<IAnalysisService> mockAnalysisService;
    private readonly Mock<IOutputService> mockOutputService;
    private readonly Mock<ILoggerFactory> mockLoggerFactory;
    private readonly Mock<ILogger<AnalyzeSecurityCommand>> mockLogger;

    public AnalyzeSecurityCommandTests()
    {
        this.mockAnalysisService = new Mock<IAnalysisService>();
        this.mockOutputService = new Mock<IOutputService>();
        this.mockLoggerFactory = new Mock<ILoggerFactory>();
        this.mockLogger = new Mock<ILogger<AnalyzeSecurityCommand>>();

        this.mockLoggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(this.mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesCommand()
    {
        // Act
        var command = new AnalyzeSecurityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("security");
        command.Description.Should().Be("Analyze security issues in schema");
    }

    [Fact]
    public void Constructor_WithNullAnalysisService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnalyzeSecurityCommand(
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
        var act = () => new AnalyzeSecurityCommand(
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
        var act = () => new AnalyzeSecurityCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("loggerFactory");
    }
}
