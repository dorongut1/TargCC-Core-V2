using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.CLI.Commands.Generate;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Generation;
using Xunit;

namespace TargCC.CLI.Tests.Commands.Generate;

/// <summary>
/// Tests for GenerateRepositoryCommand.
/// </summary>
public class GenerateRepositoryCommandTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IConfigurationService> _mockConfigService;
    private readonly Mock<IOutputService> _mockOutput;
    private readonly Mock<IGenerationService> _mockGenerationService;

    public GenerateRepositoryCommandTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockConfigService = new Mock<IConfigurationService>();
        _mockOutput = new Mock<IOutputService>();
        _mockGenerationService = new Mock<IGenerationService>();
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldSucceed()
    {
        // Arrange & Act
        var command = new GenerateRepositoryCommand(
            _mockLogger.Object,
            _mockConfigService.Object,
            _mockOutput.Object,
            _mockGenerationService.Object);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("repository");
        command.Description.Should().Contain("repository");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        var act = () => new GenerateRepositoryCommand(
            null!,
            _mockConfigService.Object,
            _mockOutput.Object,
            _mockGenerationService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullConfigService_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        var act = () => new GenerateRepositoryCommand(
            _mockLogger.Object,
            null!,
            _mockOutput.Object,
            _mockGenerationService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configService");
    }

    [Fact]
    public void Constructor_WithNullOutput_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        var act = () => new GenerateRepositoryCommand(
            _mockLogger.Object,
            _mockConfigService.Object,
            null!,
            _mockGenerationService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("output");
    }

    [Fact]
    public void Constructor_WithNullGenerationService_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        var act = () => new GenerateRepositoryCommand(
            _mockLogger.Object,
            _mockConfigService.Object,
            _mockOutput.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("generationService");
    }
}
