// <copyright file="GenerationErrorTests.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.CLI.Commands.Generate;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Generation;
using TargCC.Core.Interfaces;
using TargCC.Core.Interfaces.Models;
using Xunit;

namespace TargCC.CLI.Tests.ErrorScenarios;

/// <summary>
/// Tests for code generation error scenarios.
/// </summary>
public class GenerationErrorTests : IDisposable
{
    private readonly string testDirectory;
    private readonly Mock<ILogger> mockLogger;
    private readonly Mock<IConfigurationService> mockConfigService;
    private readonly Mock<IOutputService> mockOutputService;
    private readonly Mock<IGenerationService> mockGenerationService;
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerationErrorTests"/> class.
    /// </summary>
    public GenerationErrorTests()
    {
        this.testDirectory = Path.Combine(Path.GetTempPath(), $"TargCC_GenErrorTest_{Guid.NewGuid():N}");
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
    }

    [Fact]
    public async Task GenerationService_WhenExceptionThrown_ReturnsFailureResult()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDB;";
        var tableName = "Customer";

        // Setup generation service to throw exception
        this.mockGenerationService
            .Setup(x => x.GenerateEntityAsync(
                connectionString,
                tableName,
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Generation failed"));

        // Act & Assert
        var act = async () => await this.mockGenerationService.Object.GenerateEntityAsync(
            connectionString,
            tableName,
            this.testDirectory,
            "TestApp");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Generation failed");
    }

    [Fact]
    public async Task GenerationService_WithIOException_ThrowsIOException()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDB;";
        var tableName = "Customer";

        // Setup generation to throw IO exception
        this.mockGenerationService
            .Setup(x => x.GenerateEntityAsync(
                connectionString,
                tableName,
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(new IOException("Access denied to output directory"));

        // Act & Assert
        var act = async () => await this.mockGenerationService.Object.GenerateEntityAsync(
            connectionString,
            tableName,
            this.testDirectory,
            "TestApp");

        await act.Should().ThrowAsync<IOException>()
            .WithMessage("*Access denied*");
    }

    [Fact]
    public async Task GenerationService_WithSuccessfulGeneration_ReturnsSuccessResult()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDB;";
        var tableName = "Customer";

        this.mockGenerationService
            .Setup(x => x.GenerateEntityAsync(
                connectionString,
                tableName,
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult
            {
                Success = true,
                GeneratedFiles = new List<GeneratedFile>
                {
                    new GeneratedFile
                    {
                        FilePath = Path.Combine(this.testDirectory, "Customer.cs"),
                        FileType = "Entity",
                        LineCount = 50,
                    },
                },
                Duration = TimeSpan.FromSeconds(1),
            });

        // Act
        var result = await this.mockGenerationService.Object.GenerateEntityAsync(
            connectionString,
            tableName,
            this.testDirectory,
            "TestApp");

        // Assert
        result.Success.Should().BeTrue();
        result.GeneratedFiles.Should().HaveCount(1);
        result.GeneratedFiles[0].FileType.Should().Be("Entity");
    }

    [Fact]
    public async Task GenerationService_WithMultipleFailures_ReturnsIndividualErrors()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDB;";

        // Setup Customer to succeed
        this.mockGenerationService
            .Setup(x => x.GenerateEntityAsync(
                connectionString,
                "Customer",
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult
            {
                Success = true,
                GeneratedFiles = new List<GeneratedFile>(),
            });

        // Setup Order to fail
        this.mockGenerationService
            .Setup(x => x.GenerateEntityAsync(
                connectionString,
                "Order",
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult
            {
                Success = false,
                ErrorMessage = "Table 'Order' not found",
                GeneratedFiles = new List<GeneratedFile>(),
            });

        // Act
        var customerResult = await this.mockGenerationService.Object.GenerateEntityAsync(
            connectionString,
            "Customer",
            this.testDirectory,
            "TestApp");

        var orderResult = await this.mockGenerationService.Object.GenerateEntityAsync(
            connectionString,
            "Order",
            this.testDirectory,
            "TestApp");

        // Assert
        customerResult.Success.Should().BeTrue();
        orderResult.Success.Should().BeFalse();
        orderResult.ErrorMessage.Should().Contain("not found");
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
