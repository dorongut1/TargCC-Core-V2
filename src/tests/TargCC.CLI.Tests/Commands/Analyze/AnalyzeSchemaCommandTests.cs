// <copyright file="AnalyzeSchemaCommandTests.cs" company="Doron Vaida">
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
/// Tests for <see cref="AnalyzeSchemaCommand"/>.
/// </summary>
public class AnalyzeSchemaCommandTests
{
    private readonly Mock<IAnalysisService> mockAnalysisService;
    private readonly Mock<IOutputService> mockOutputService;
    private readonly Mock<ILoggerFactory> mockLoggerFactory;
    private readonly Mock<ILogger<AnalyzeSchemaCommand>> mockLogger;

    public AnalyzeSchemaCommandTests()
    {
        this.mockAnalysisService = new Mock<IAnalysisService>();
        this.mockOutputService = new Mock<IOutputService>();
        this.mockLoggerFactory = new Mock<ILoggerFactory>();
        this.mockLogger = new Mock<ILogger<AnalyzeSchemaCommand>>();

        this.mockLoggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(this.mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesCommand()
    {
        // Act
        var command = new AnalyzeSchemaCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("schema");
        command.Description.Should().Be("Analyze database schema structure");
    }

    [Fact]
    public void Constructor_WithNullAnalysisService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnalyzeSchemaCommand(
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
        var act = () => new AnalyzeSchemaCommand(
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
        var act = () => new AnalyzeSchemaCommand(
            this.mockAnalysisService.Object,
            this.mockOutputService.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("loggerFactory");
    }
}
