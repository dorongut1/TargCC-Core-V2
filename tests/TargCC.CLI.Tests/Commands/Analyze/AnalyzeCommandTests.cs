// <copyright file="AnalyzeCommandTests.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using FluentAssertions;
using Moq;
using TargCC.CLI.Commands.Analyze;
using Xunit;

namespace TargCC.CLI.Tests.Commands.Analyze;

/// <summary>
/// Tests for <see cref="AnalyzeCommand"/>.
/// </summary>
public class AnalyzeCommandTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesCommand()
    {
        // Arrange
        var mockSchemaCommand = new Mock<AnalyzeSchemaCommand>(
            Mock.Of<Services.Analysis.IAnalysisService>(),
            Mock.Of<Services.Output.IOutputService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());

        var mockImpactCommand = new Mock<AnalyzeImpactCommand>(
            Mock.Of<Services.Analysis.IAnalysisService>(),
            Mock.Of<Services.Output.IOutputService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());

        var mockSecurityCommand = new Mock<AnalyzeSecurityCommand>(
            Mock.Of<Services.Analysis.IAnalysisService>(),
            Mock.Of<Services.Output.IOutputService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());

        var mockQualityCommand = new Mock<AnalyzeQualityCommand>(
            Mock.Of<Services.Analysis.IAnalysisService>(),
            Mock.Of<Services.Output.IOutputService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());

        // Act
        var command = new AnalyzeCommand(
            mockSchemaCommand.Object,
            mockImpactCommand.Object,
            mockSecurityCommand.Object,
            mockQualityCommand.Object);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("analyze");
        command.Description.Should().Be("Analyze database schema and code impact");
    }

    [Fact]
    public void Constructor_WithNullSchemaCommand_ThrowsArgumentNullException()
    {
        // Arrange
        var mockImpactCommand = new Mock<AnalyzeImpactCommand>(
            Mock.Of<Services.Analysis.IAnalysisService>(),
            Mock.Of<Services.Output.IOutputService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());

        var mockSecurityCommand = new Mock<AnalyzeSecurityCommand>(
            Mock.Of<Services.Analysis.IAnalysisService>(),
            Mock.Of<Services.Output.IOutputService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());

        var mockQualityCommand = new Mock<AnalyzeQualityCommand>(
            Mock.Of<Services.Analysis.IAnalysisService>(),
            Mock.Of<Services.Output.IOutputService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());

        // Act
        var act = () => new AnalyzeCommand(
            null!,
            mockImpactCommand.Object,
            mockSecurityCommand.Object,
            mockQualityCommand.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("schemaCommand");
    }

    [Fact]
    public void Constructor_WithNullImpactCommand_ThrowsArgumentNullException()
    {
        // Arrange
        var mockSchemaCommand = new Mock<AnalyzeSchemaCommand>(
            Mock.Of<Services.Analysis.IAnalysisService>(),
            Mock.Of<Services.Output.IOutputService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());

        var mockSecurityCommand = new Mock<AnalyzeSecurityCommand>(
            Mock.Of<Services.Analysis.IAnalysisService>(),
            Mock.Of<Services.Output.IOutputService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());

        var mockQualityCommand = new Mock<AnalyzeQualityCommand>(
            Mock.Of<Services.Analysis.IAnalysisService>(),
            Mock.Of<Services.Output.IOutputService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());

        // Act
        var act = () => new AnalyzeCommand(
            mockSchemaCommand.Object,
            null!,
            mockSecurityCommand.Object,
            mockQualityCommand.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("impactCommand");
    }
}
