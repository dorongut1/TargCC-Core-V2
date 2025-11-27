// <copyright file="AnalyzeCommandTests.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.CLI.Commands.Analyze;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Analysis;
using Xunit;

namespace TargCC.CLI.Tests.Commands.Analyze;

/// <summary>
/// Tests for <see cref="AnalyzeCommand"/>.
/// </summary>
public class AnalyzeCommandTests
{
    private readonly Mock<ILoggerFactory> mockLoggerFactory;
    private readonly Mock<IConfigurationService> mockConfigService;
    private readonly Mock<IOutputService> mockOutputService;
    private readonly Mock<IAnalysisService> mockAnalysisService;
    private readonly Mock<ILogger<AnalyzeCommand>> mockLogger;

    public AnalyzeCommandTests()
    {
        this.mockLoggerFactory = new Mock<ILoggerFactory>();
        this.mockConfigService = new Mock<IConfigurationService>();
        this.mockOutputService = new Mock<IOutputService>();
        this.mockAnalysisService = new Mock<IAnalysisService>();
        this.mockLogger = new Mock<ILogger<AnalyzeCommand>>();

        this.mockLoggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(this.mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesCommand()
    {
        // Act
        var command = new AnalyzeCommand(
            this.mockLoggerFactory.Object,
            this.mockConfigService.Object,
            this.mockOutputService.Object,
            this.mockAnalysisService.Object);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("analyze");
        command.Description.Should().Be("Analyze database schema and code impact");
        command.Subcommands.Should().HaveCount(4); // schema, impact, security, quality
    }

    [Fact]
    public void Constructor_WithNullLoggerFactory_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnalyzeCommand(
            null!,
            this.mockConfigService.Object,
            this.mockOutputService.Object,
            this.mockAnalysisService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("loggerFactory");
    }

    [Fact]
    public void Constructor_WithNullConfigService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnalyzeCommand(
            this.mockLoggerFactory.Object,
            null!,
            this.mockOutputService.Object,
            this.mockAnalysisService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configService");
    }

    [Fact]
    public void Constructor_WithNullOutputService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnalyzeCommand(
            this.mockLoggerFactory.Object,
            this.mockConfigService.Object,
            null!,
            this.mockAnalysisService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("output");
    }

    [Fact]
    public void Constructor_WithNullAnalysisService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnalyzeCommand(
            this.mockLoggerFactory.Object,
            this.mockConfigService.Object,
            this.mockOutputService.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("analysisService");
    }

    [Fact]
    public void Constructor_CreatesAllSubcommands()
    {
        // Act
        var command = new AnalyzeCommand(
            this.mockLoggerFactory.Object,
            this.mockConfigService.Object,
            this.mockOutputService.Object,
            this.mockAnalysisService.Object);

        // Assert
        var subcommandNames = command.Subcommands.Select(c => c.Name).ToList();
        subcommandNames.Should().Contain("schema");
        subcommandNames.Should().Contain("impact");
        subcommandNames.Should().Contain("security");
        subcommandNames.Should().Contain("quality");
    }
}
