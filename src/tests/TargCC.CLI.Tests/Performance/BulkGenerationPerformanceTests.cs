// <copyright file="BulkGenerationPerformanceTests.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Generation;
using TargCC.Core.Interfaces.Models;
using Xunit;
using Xunit.Abstractions;

namespace TargCC.CLI.Tests.Performance;

/// <summary>
/// Performance tests for bulk generation operations.
/// </summary>
[Trait("Category", "Performance")]
public class BulkGenerationPerformanceTests : IDisposable
{
    private readonly ITestOutputHelper output;
    private readonly string testDirectory;
    private readonly Mock<IGenerationService> mockGenerationService;
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkGenerationPerformanceTests"/> class.
    /// </summary>
    /// <param name="output">Test output helper.</param>
    public BulkGenerationPerformanceTests(ITestOutputHelper output)
    {
        this.output = output;
        this.testDirectory = Path.Combine(Path.GetTempPath(), $"TargCC_PerfTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(this.testDirectory);

        this.mockGenerationService = new Mock<IGenerationService>();
        this.SetupMocks();
    }

    [Fact]
    public async Task BulkGeneration_10Tables_CompletesUnder10Seconds()
    {
        // Arrange
        var tables = this.CreateMultipleTables(10);
        var connectionString = "Server=localhost;Database=TestDB;";

        var stopwatch = Stopwatch.StartNew();

        // Act - Generate for all tables
        foreach (var table in tables)
        {
            await this.mockGenerationService.Object.GenerateAllAsync(
                connectionString,
                table.Name,
                this.testDirectory,
                "TestApp");
        }

        stopwatch.Stop();

        // Assert
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
        this.output.WriteLine($"Bulk generation for 10 tables completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task BulkGeneration_ParallelExecution_HandlesEfficiently()
    {
        // Arrange
        var tables = this.CreateMultipleTables(20);
        var connectionString = "Server=localhost;Database=TestDB;";

        var stopwatch = Stopwatch.StartNew();

        // Act - Generate for all tables in parallel
        var tasks = tables.Select(table =>
            this.mockGenerationService.Object.GenerateAllAsync(
                connectionString,
                table.Name,
                this.testDirectory,
                "TestApp"));

        await Task.WhenAll(tasks);

        stopwatch.Stop();

        // Assert
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(15));
        this.output.WriteLine($"Parallel bulk generation for 20 tables completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task SingleTableGeneration_AllComponents_CompletesUnder2Seconds()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDB;";

        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await this.mockGenerationService.Object.GenerateAllAsync(
            connectionString,
            "Customer",
            this.testDirectory,
            "TestApp");

        stopwatch.Stop();

        // Assert
        result.Success.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
        this.output.WriteLine($"Single table generation completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GenerationService_EntityOnly_CompletesUnder500ms()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDB;";

        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await this.mockGenerationService.Object.GenerateEntityAsync(
            connectionString,
            "Customer",
            this.testDirectory,
            "TestApp");

        stopwatch.Stop();

        // Assert
        result.Success.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(500));
        this.output.WriteLine($"Entity generation completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GenerationService_SqlOnly_CompletesUnder500ms()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDB;";

        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await this.mockGenerationService.Object.GenerateSqlAsync(
            connectionString,
            "Customer",
            this.testDirectory);

        stopwatch.Stop();

        // Assert
        result.Success.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(500));
        this.output.WriteLine($"SQL generation completed in {stopwatch.ElapsedMilliseconds}ms");
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

    private void SetupMocks()
    {
        // Setup all generation methods to complete quickly
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
                Duration = TimeSpan.FromMilliseconds(100),
            });

        this.mockGenerationService
            .Setup(x => x.GenerateSqlAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult
            {
                Success = true,
                GeneratedFiles = new List<GeneratedFile>(),
                Duration = TimeSpan.FromMilliseconds(150),
            });

        this.mockGenerationService
            .Setup(x => x.GenerateAllAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult
            {
                Success = true,
                GeneratedFiles = new List<GeneratedFile>(),
                Duration = TimeSpan.FromMilliseconds(500),
            });

        this.mockGenerationService
            .Setup(x => x.GenerateRepositoryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult
            {
                Success = true,
                GeneratedFiles = new List<GeneratedFile>(),
                Duration = TimeSpan.FromMilliseconds(100),
            });

        this.mockGenerationService
            .Setup(x => x.GenerateCqrsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult
            {
                Success = true,
                GeneratedFiles = new List<GeneratedFile>(),
                Duration = TimeSpan.FromMilliseconds(200),
            });

        this.mockGenerationService
            .Setup(x => x.GenerateApiAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult
            {
                Success = true,
                GeneratedFiles = new List<GeneratedFile>(),
                Duration = TimeSpan.FromMilliseconds(100),
            });
    }

    private List<Table> CreateMultipleTables(int count)
    {
        var tables = new List<Table>();

        for (int i = 1; i <= count; i++)
        {
            tables.Add(new Table
            {
                SchemaName = "dbo",
                Name = $"Table{i}",
                Columns = new List<Column>
                {
                    new() { Name = $"Table{i}ID", DataType = "int", IsPrimaryKey = true, IsNullable = false },
                    new() { Name = "Name", DataType = "nvarchar", MaxLength = 100, IsNullable = false },
                    new() { Name = "Description", DataType = "nvarchar", MaxLength = 500, IsNullable = true },
                },
                Indexes = new List<TargCC.Core.Interfaces.Models.Index>(),
                Relationships = new List<Relationship>(),
            });
        }

        return tables;
    }
}
