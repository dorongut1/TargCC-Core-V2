// <copyright file="AnalyzeImpactCommandTests.cs" company="Doron Vaida">
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
/// Tests for <see cref="AnalyzeImpactCommand"/>.
/// </summary>
public class AnalyzeImpactCommandTests
{
    private readonly Mock<IAnalysisService> mockAnalysisService;
    private readonly Mock<IOutputService> mockOutputService;
    private readonly Mock<ILoggerFactory> mockLoggerFactory;
    private readonly Mock<ILogger<AnalyzeImpactCommand>> mockLogger;

    public AnalyzeImpactCommandTests()
    {
        this.mockAnalysisService = new Mock<IAnalysisService>();
        this.mockOutputService = new Mock<IOutputService>();
        this.mockLoggerFactory = new Mock<ILoggerFactory>();
        this.mockLogger = new Mock<ILogger<AnalyzeImpactCommand>>();

        this.mockLoggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(this.mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesCommand()
    {
        // Act
        var command = new AnalyzeImpactCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("impact");
        command.Description.Should().Be("Analyze impact of schema changes");
    }

    [Fact]
    public void Constructor_WithNullAnalysisService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnalyzeImpactCommand(
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
        var act = () => new AnalyzeImpactCommand(
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
        var act = () => new AnalyzeImpactCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("loggerFactory");
    }
}
