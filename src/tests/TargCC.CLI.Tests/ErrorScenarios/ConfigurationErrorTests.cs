// <copyright file="ConfigurationErrorTests.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using System.CommandLine;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.CLI.Commands.Generate;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Generation;
using Xunit;

namespace TargCC.CLI.Tests.ErrorScenarios;

/// <summary>
/// Tests for configuration-related error scenarios.
/// </summary>
public class ConfigurationErrorTests : IDisposable
{
    private readonly string testDirectory;
    private readonly Mock<ILogger> mockLogger;
    private readonly Mock<IConfigurationService> mockConfigService;
    private readonly Mock<IOutputService> mockOutputService;
    private readonly Mock<IGenerationService> mockGenerationService;
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationErrorTests"/> class.
    /// </summary>
    public ConfigurationErrorTests()
    {
        this.testDirectory = Path.Combine(Path.GetTempPath(), $"TargCC_ErrorTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(this.testDirectory);

        this.mockLogger = new Mock<ILogger>();
        this.mockConfigService = new Mock<IConfigurationService>();
        this.mockOutputService = new Mock<IOutputService>();
        this.mockGenerationService = new Mock<IGenerationService>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GenerateEntityCommand(
            null!,
            this.mockConfigService.Object,
            this.mockOutputService.Object,
            this.mockGenerationService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullConfigService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GenerateEntityCommand(
            this.mockLogger.Object,
            null!,
            this.mockOutputService.Object,
            this.mockGenerationService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configService");
    }

    [Fact]
    public void Constructor_WithNullOutputService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GenerateEntityCommand(
            this.mockLogger.Object,
            this.mockConfigService.Object,
            null!,
            this.mockGenerationService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("output");
    }

    [Fact]
    public void Constructor_WithNullGenerationService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GenerateEntityCommand(
            this.mockLogger.Object,
            this.mockConfigService.Object,
            this.mockOutputService.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("generationService");
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesCommand()
    {
        // Act
        var command = new GenerateEntityCommand(
            this.mockLogger.Object,
            this.mockConfigService.Object,
            this.mockOutputService.Object,
            this.mockGenerationService.Object);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("entity");
        command.Description.Should().Be("Generate entity class for a table");
    }

    [Fact]
    public async Task ConfigService_LoadAsync_IsCalledDuringExecution()
    {
        // Arrange
        var config = new CliConfiguration
        {
            ConnectionString = "Server=localhost;Database=TestDB;",
            OutputDirectory = this.testDirectory,
            DefaultNamespace = "TestNamespace",
        };

        this.mockConfigService
            .Setup(x => x.LoadAsync())
            .ReturnsAsync(config);

        this.mockGenerationService
            .Setup(x => x.GenerateEntityAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult
            {
                Success = true,
                GeneratedFiles = new List<GeneratedFile>(),
                Duration = TimeSpan.FromSeconds(1),
            });

        var command = new GenerateEntityCommand(
            this.mockLogger.Object,
            this.mockConfigService.Object,
            this.mockOutputService.Object,
            this.mockGenerationService.Object);

        // Act
        var result = await command.InvokeAsync("Customer");

        // Assert
        this.mockConfigService.Verify(x => x.LoadAsync(), Times.Once);
    }

    [Fact]
    public async Task ConfigService_WithMissingConnectionString_ShowsErrorMessage()
    {
        // Arrange
        var config = new CliConfiguration
        {
            ConnectionString = null, // Missing connection string
            OutputDirectory = this.testDirectory,
            DefaultNamespace = "TestNamespace",
        };

        this.mockConfigService
            .Setup(x => x.LoadAsync())
            .ReturnsAsync(config);

        var command = new GenerateEntityCommand(
            this.mockLogger.Object,
            this.mockConfigService.Object,
            this.mockOutputService.Object,
            this.mockGenerationService.Object);

        // Act
        var result = await command.InvokeAsync("Customer");

        // Assert
        result.Should().Be(1); // Error exit code
        this.mockOutputService.Verify(
            x => x.Error(It.Is<string>(s => s.Contains("Connection string"))),
            Times.Once);
    }

    [Fact]
    public void Configuration_WithInvalidJsonFormat_ThrowsException()
    {
        // Arrange
        var configPath = Path.Combine(this.testDirectory, "invalid.json");
        File.WriteAllText(configPath, "{ invalid json content }");

        // Act
        var act = () =>
        {
            var json = File.ReadAllText(configPath);
            return System.Text.Json.JsonSerializer.Deserialize<CliConfiguration>(json);
        };

        // Assert
        act.Should().Throw<System.Text.Json.JsonException>();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    /// <param name="disposing">Whether disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                // Cleanup test directory
                if (Directory.Exists(this.testDirectory))
                {
                    try
                    {
                        Directory.Delete(this.testDirectory, true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }

            this.disposed = true;
        }
    }
}
