// <copyright file="AnalysisServiceTests.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.CLI.Services.Analysis;
using TargCC.CLI.Configuration;
using Xunit;

namespace TargCC.CLI.Tests.Services.Analysis;

/// <summary>
/// Tests for <see cref="AnalysisService"/>.
/// </summary>
public class AnalysisServiceTests
{
    private readonly Mock<IConfigurationService> mockConfigurationService;
    private readonly Mock<ILoggerFactory> mockLoggerFactory;
    private readonly Mock<ILogger<AnalysisService>> mockLogger;

    public AnalysisServiceTests()
    {
        this.mockConfigurationService = new Mock<IConfigurationService>();
        this.mockLoggerFactory = new Mock<ILoggerFactory>();
        this.mockLogger = new Mock<ILogger<AnalysisService>>();

        this.mockLoggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(this.mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var service = new AnalysisService(
            this.mockConfigurationService.Object,
            this.mockLoggerFactory.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullConfigService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnalysisService(
            null!,
            this.mockLoggerFactory.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configurationService");
    }

    [Fact]
    public void Constructor_WithNullLoggerFactory_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnalysisService(
            this.mockConfigurationService.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("loggerFactory");
    }
}
