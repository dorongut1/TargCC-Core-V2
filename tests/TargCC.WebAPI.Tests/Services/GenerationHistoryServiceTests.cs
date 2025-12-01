// <copyright file="GenerationHistoryServiceTests.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.WebAPI.Tests.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.WebAPI.Models;
using TargCC.WebAPI.Services;
using Xunit;

/// <summary>
/// Tests for <see cref="GenerationHistoryService"/>.
/// </summary>
public class GenerationHistoryServiceTests : IDisposable
{
    private readonly Mock<ILogger<GenerationHistoryService>> loggerMock;
    private readonly GenerationHistoryService service;
    private readonly string testHistoryFile;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerationHistoryServiceTests"/> class.
    /// </summary>
    public GenerationHistoryServiceTests()
    {
        this.loggerMock = new Mock<ILogger<GenerationHistoryService>>();
        
        // Use a test-specific file path
        this.testHistoryFile = Path.Combine(Path.GetTempPath(), $"test-history-{Guid.NewGuid()}.json");
        
        this.service = new GenerationHistoryService(this.loggerMock.Object, this.testHistoryFile);
    }

    /// <summary>
    /// Verifies that GetHistoryAsync returns empty list when no history exists.
    /// </summary>
    [Fact]
    public async Task GetHistoryAsync_NoHistory_ReturnsEmptyList()
    {
        // Act
        var result = await this.service.GetHistoryAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that AddHistoryAsync stores history correctly.
    /// </summary>
    [Fact]
    public async Task AddHistoryAsync_ValidEntry_StoresSuccessfully()
    {
        // Arrange
        var entry = new GenerationHistory
        {
            TableName = "Users",
            SchemaName = "dbo",
            Success = true,
            FilesGenerated = new[] { "User.cs", "UserRepository.cs" },
            Errors = Array.Empty<string>(),
            Warnings = Array.Empty<string>(),
            Options = new GenerationOptions
            {
                GenerateEntity = true,
                GenerateRepository = true,
                GenerateService = false,
                GenerateController = false,
                GenerateTests = false,
                OverwriteExisting = true,
            },
        };

        // Act
        await this.service.AddHistoryAsync(entry);
        var result = await this.service.GetHistoryAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().TableName.Should().Be("Users");
        result.First().Success.Should().BeTrue();
        result.First().FilesGenerated.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies that GetHistoryAsync filters by table name correctly.
    /// </summary>
    [Fact]
    public async Task GetHistoryAsync_WithTableName_FiltersCorrectly()
    {
        // Arrange
        await this.service.AddHistoryAsync(new GenerationHistory
        {
            TableName = "Users",
            SchemaName = "dbo",
            Success = true,
        });

        await this.service.AddHistoryAsync(new GenerationHistory
        {
            TableName = "Products",
            SchemaName = "dbo",
            Success = true,
        });

        // Act
        var result = await this.service.GetHistoryAsync("Users");

        // Assert
        result.Should().HaveCount(1);
        result.First().TableName.Should().Be("Users");
    }

    /// <summary>
    /// Verifies that GetLastGenerationAsync returns most recent entry.
    /// </summary>
    [Fact]
    public async Task GetLastGenerationAsync_MultipleEntries_ReturnsMostRecent()
    {
        // Arrange
        await this.service.AddHistoryAsync(new GenerationHistory
        {
            TableName = "Users",
            SchemaName = "dbo",
            Success = true,
            GeneratedAt = DateTime.UtcNow.AddHours(-2),
        });

        await Task.Delay(100); // Ensure different timestamps

        await this.service.AddHistoryAsync(new GenerationHistory
        {
            TableName = "Users",
            SchemaName = "dbo",
            Success = false,
            GeneratedAt = DateTime.UtcNow.AddHours(-1),
        });

        // Act
        var result = await this.service.GetLastGenerationAsync("Users");

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetLastGenerationAsync returns null when no history exists.
    /// </summary>
    [Fact]
    public async Task GetLastGenerationAsync_NoHistory_ReturnsNull()
    {
        // Act
        var result = await this.service.GetLastGenerationAsync("NonExistent");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that ClearHistoryAsync removes all entries.
    /// </summary>
    [Fact]
    public async Task ClearHistoryAsync_WithEntries_RemovesAll()
    {
        // Arrange
        await this.service.AddHistoryAsync(new GenerationHistory
        {
            TableName = "Users",
            SchemaName = "dbo",
            Success = true,
        });

        await this.service.AddHistoryAsync(new GenerationHistory
        {
            TableName = "Products",
            SchemaName = "dbo",
            Success = true,
        });

        // Act
        await this.service.ClearHistoryAsync();
        var result = await this.service.GetHistoryAsync();

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that automatic cleanup keeps only last 100 entries per table.
    /// </summary>
    [Fact]
    public async Task AddHistoryAsync_MoreThan100Entries_KeepsOnlyLast100()
    {
        // Arrange - Add 105 entries for same table
        for (int i = 0; i < 105; i++)
        {
            await this.service.AddHistoryAsync(new GenerationHistory
            {
                TableName = "Users",
                SchemaName = "dbo",
                Success = true,
                GeneratedAt = DateTime.UtcNow.AddMinutes(-i),
            });
        }

        // Act
        var result = await this.service.GetHistoryAsync("Users");

        // Assert
        result.Should().HaveCount(100);
    }

    /// <summary>
    /// Verifies that GetGenerationStatusAsync returns "Not Generated" when no history.
    /// </summary>
    [Fact]
    public async Task GetGenerationStatusAsync_NoHistory_ReturnsNotGenerated()
    {
        // Act
        var result = await this.service.GetGenerationStatusAsync("Users");

        // Assert
        result.Should().Be("Not Generated");
    }

    /// <summary>
    /// Verifies that GetGenerationStatusAsync returns "Generated" when files exist.
    /// </summary>
    [Fact]
    public async Task GetGenerationStatusAsync_FilesExist_ReturnsGenerated()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}.cs");
        await File.WriteAllTextAsync(tempFile, "// Test");

        await this.service.AddHistoryAsync(new GenerationHistory
        {
            TableName = "Users",
            SchemaName = "dbo",
            Success = true,
            FilesGenerated = new[] { tempFile },
        });

        try
        {
            // Act
            var result = await this.service.GetGenerationStatusAsync("Users");

            // Assert
            result.Should().Be("Generated");
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    /// <summary>
    /// Verifies that GetGenerationStatusAsync returns "Modified" when files missing.
    /// </summary>
    [Fact]
    public async Task GetGenerationStatusAsync_FilesMissing_ReturnsModified()
    {
        // Arrange
        await this.service.AddHistoryAsync(new GenerationHistory
        {
            TableName = "Users",
            SchemaName = "dbo",
            Success = true,
            FilesGenerated = new[] { "NonExistent.cs" },
        });

        // Act
        var result = await this.service.GetGenerationStatusAsync("Users");

        // Assert
        result.Should().Be("Modified");
    }

    /// <summary>
    /// Verifies that GetGenerationStatusAsync returns "Error" when last generation failed.
    /// </summary>
    [Fact]
    public async Task GetGenerationStatusAsync_LastGenerationFailed_ReturnsError()
    {
        // Arrange
        await this.service.AddHistoryAsync(new GenerationHistory
        {
            TableName = "Users",
            SchemaName = "dbo",
            Success = false,
            Errors = new[] { "Generation failed" },
        });

        // Act
        var result = await this.service.GetGenerationStatusAsync("Users");

        // Assert
        result.Should().Be("Error");
    }

    /// <summary>
    /// Verifies thread safety of concurrent operations.
    /// </summary>
    [Fact]
    public async Task ConcurrentOperations_ThreadSafe()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act - Run 10 concurrent additions
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                await this.service.AddHistoryAsync(new GenerationHistory
                {
                    TableName = $"Table{index}",
                    SchemaName = "dbo",
                    Success = true,
                });
            }));
        }

        await Task.WhenAll(tasks);

        var result = await this.service.GetHistoryAsync();

        // Assert
        result.Should().HaveCount(10);
    }

    /// <summary>
    /// Cleanup test resources.
    /// </summary>
    public void Dispose()
    {
        if (File.Exists(this.testHistoryFile))
        {
            File.Delete(this.testHistoryFile);
        }

        GC.SuppressFinalize(this);
    }
}
