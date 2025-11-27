// <copyright file="AnalyzeQualityCommandTests.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

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
}
