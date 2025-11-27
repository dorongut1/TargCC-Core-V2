// <copyright file="DatabaseErrorTests.cs" company="Doron Vaida">
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
/// Tests for database-related error scenarios.
/// </summary>
public class DatabaseErrorTests : IDisposable
{
    private readonly string testDirectory;
    private readonly Mock<ILogger> mockLogger;
    private readonly Mock<IConfigurationService> mockConfigService;
    private readonly Mock<IOutputService> mockOutputService;
    private readonly Mock<IGenerationService> mockGenerationService;
    private readonly Mock<IDatabaseAnalyzer> mockDatabaseAnalyzer;
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseErrorTests"/> class.
    /// </summary>
    public DatabaseErrorTests()
    {
        this.testDirectory = Path.Combine(Path.GetTempPath(), $"TargCC_DbErrorTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(this.testDirectory);

        this.mockLogger = new Mock<ILogger>();
        this.mockConfigService = new Mock<IConfigurationService>();
        this.mockOutputService = new Mock<IOutputService>();
        this.mockGenerationService = new Mock<IGenerationService>();
        this.mockDatabaseAnalyzer = new Mock<IDatabaseAnalyzer>();
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
    public async Task GenerationService_WithInvalidConnectionString_ReturnsFailureResult()
    {
        // Arrange
        var invalidConnectionString = "Server=nonexistent;Database=FakeDB;";

        // Setup generation service to fail with connection error
        this.mockGenerationService
            .Setup(x => x.GenerateEntityAsync(
                invalidConnectionString,
                "Customer",
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult
            {
                Success = false,
                ErrorMessage = "Failed to connect to database",
                GeneratedFiles = new List<GeneratedFile>(),
            });

        // Act
        var result = await this.mockGenerationService.Object.GenerateEntityAsync(
            invalidConnectionString,
            "Customer",
            this.testDirectory,
            "TestApp");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("connect");
    }

    [Fact]
    public async Task GenerationService_WithNonExistentTable_ReturnsFailureResult()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDB;";

        // Setup generation service to fail - table not found
        this.mockGenerationService
            .Setup(x => x.GenerateEntityAsync(
                connectionString,
                "NonExistentTable",
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult
            {
                Success = false,
                ErrorMessage = "Table 'NonExistentTable' not found",
                GeneratedFiles = new List<GeneratedFile>(),
            });

        // Act
        var result = await this.mockGenerationService.Object.GenerateEntityAsync(
            connectionString,
            "NonExistentTable",
            this.testDirectory,
            "TestApp");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task GenerationService_WithTimeout_ThrowsTimeoutException()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDB;Connection Timeout=1;";

        // Setup generation service to timeout
        this.mockGenerationService
            .Setup(x => x.GenerateEntityAsync(
                connectionString,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(new TimeoutException("Database operation timed out"));

        // Act & Assert
        var act = async () => await this.mockGenerationService.Object.GenerateEntityAsync(
            connectionString,
            "Customer",
            this.testDirectory,
            "TestApp");

        await act.Should().ThrowAsync<TimeoutException>()
            .WithMessage("*timed out*");
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
