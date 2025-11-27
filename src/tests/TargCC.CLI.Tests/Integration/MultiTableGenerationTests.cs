// <copyright file="MultiTableGenerationTests.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Generation;
using TargCC.Core.Interfaces;
using TargCC.Core.Interfaces.Models;
using Xunit;

namespace TargCC.CLI.Tests.Integration;

/// <summary>
/// Integration tests for multi-table generation scenarios.
/// </summary>
public class MultiTableGenerationTests : IntegrationTestBase
{
    private readonly Mock<IGenerationService> mockGenerationService;
    private readonly IOutputService outputService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiTableGenerationTests"/> class.
    /// </summary>
    public MultiTableGenerationTests()
    {
        this.mockGenerationService = new Mock<IGenerationService>();
        this.outputService = this.ServiceProvider.GetRequiredService<IOutputService>();
    }

    [Fact]
    public async Task GenerationService_GenerateMultipleTables_CompletesSuccessfully()
    {
        // Arrange
        var config = this.CreateTestConfig();
        var tables = new[] { "Customer", "Order", "Product" };

        // Setup mock to succeed for all tables
        this.mockGenerationService
            .Setup(x => x.GenerateEntityAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult
            {
                Success = true,
                GeneratedFiles = new List<GeneratedFile>
                {
                    new GeneratedFile
                    {
                        FilePath = "test.cs",
                        FileType = "Entity",
                        LineCount = 50,
                    },
                },
            });

        // Act
        var results = new List<GenerationResult>();
        foreach (var tableName in tables)
        {
            var result = await this.mockGenerationService.Object.GenerateEntityAsync(
                config.ConnectionString!,
                tableName,
                config.OutputDirectory!,
                config.DefaultNamespace!);
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r.Success);
        this.mockGenerationService.Verify(
            x => x.GenerateEntityAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task GenerationService_GenerateAllForTable_GeneratesAllComponents()
    {
        // Arrange
        var config = this.CreateTestConfig();
        var tableName = "Customer";

        // Setup mock to return successful generation
        this.mockGenerationService
            .Setup(x => x.GenerateAllAsync(
                config.ConnectionString!,
                tableName,
                config.OutputDirectory!,
                config.DefaultNamespace!))
            .ReturnsAsync(new GenerationResult
            {
                Success = true,
                GeneratedFiles = new List<GeneratedFile>
                {
                    new GeneratedFile { FilePath = "Customer.cs", FileType = "Entity", LineCount = 50 },
                    new GeneratedFile { FilePath = "Customer_Insert.sql", FileType = "SQL", LineCount = 20 },
                    new GeneratedFile { FilePath = "CustomerRepository.cs", FileType = "Repository", LineCount = 100 },
                    new GeneratedFile { FilePath = "GetCustomerByIdQuery.cs", FileType = "CQRS", LineCount = 30 },
                    new GeneratedFile { FilePath = "CustomerController.cs", FileType = "API", LineCount = 80 },
                },
                Duration = TimeSpan.FromSeconds(2),
            });

        // Act
        var result = await this.mockGenerationService.Object.GenerateAllAsync(
            config.ConnectionString!,
            tableName,
            config.OutputDirectory!,
            config.DefaultNamespace!);

        // Assert
        result.Success.Should().BeTrue();
        result.GeneratedFiles.Should().HaveCount(5);
        result.GeneratedFiles.Should().Contain(f => f.FileType == "Entity");
        result.GeneratedFiles.Should().Contain(f => f.FileType == "SQL");
        result.GeneratedFiles.Should().Contain(f => f.FileType == "Repository");
        result.GeneratedFiles.Should().Contain(f => f.FileType == "CQRS");
        result.GeneratedFiles.Should().Contain(f => f.FileType == "API");
    }

    [Fact]
    public async Task GenerationService_GenerateMultipleTablesWithOneFailure_HandlesGracefully()
    {
        // Arrange
        var config = this.CreateTestConfig();

        // Setup Customer to succeed
        this.mockGenerationService
            .Setup(x => x.GenerateEntityAsync(
                config.ConnectionString!,
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
                config.ConnectionString!,
                "Order",
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult
            {
                Success = false,
                ErrorMessage = "Table 'Order' not found",
                GeneratedFiles = new List<GeneratedFile>(),
            });

        // Setup Product to succeed
        this.mockGenerationService
            .Setup(x => x.GenerateEntityAsync(
                config.ConnectionString!,
                "Product",
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult
            {
                Success = true,
                GeneratedFiles = new List<GeneratedFile>(),
            });

        // Act
        var results = new Dictionary<string, GenerationResult>();
        foreach (var tableName in new[] { "Customer", "Order", "Product" })
        {
            var result = await this.mockGenerationService.Object.GenerateEntityAsync(
                config.ConnectionString!,
                tableName,
                config.OutputDirectory!,
                config.DefaultNamespace!);
            results[tableName] = result;
        }

        // Assert
        results["Customer"].Success.Should().BeTrue();
        results["Order"].Success.Should().BeFalse();
        results["Order"].ErrorMessage.Should().Contain("not found");
        results["Product"].Success.Should().BeTrue();
    }

    [Fact]
    public async Task GenerationService_GenerateAllComponentsForMultipleTables_CompletesAllOperations()
    {
        // Arrange
        var config = this.CreateTestConfig();
        var tables = new[] { "Customer", "Order" };

        // Setup successful generation for all tables
        foreach (var tableName in tables)
        {
            this.mockGenerationService
                .Setup(x => x.GenerateEntityAsync(
                    config.ConnectionString!,
                    tableName,
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new GenerationResult { Success = true, GeneratedFiles = new List<GeneratedFile>() });

            this.mockGenerationService
                .Setup(x => x.GenerateSqlAsync(
                    config.ConnectionString!,
                    tableName,
                    It.IsAny<string>()))
                .ReturnsAsync(new GenerationResult { Success = true, GeneratedFiles = new List<GeneratedFile>() });

            this.mockGenerationService
                .Setup(x => x.GenerateRepositoryAsync(
                    config.ConnectionString!,
                    tableName,
                    It.IsAny<string>()))
                .ReturnsAsync(new GenerationResult { Success = true, GeneratedFiles = new List<GeneratedFile>() });
        }

        // Act
        var allSuccessful = true;
        foreach (var tableName in tables)
        {
            var entityResult = await this.mockGenerationService.Object.GenerateEntityAsync(
                config.ConnectionString!, tableName, config.OutputDirectory!, config.DefaultNamespace!);
            var sqlResult = await this.mockGenerationService.Object.GenerateSqlAsync(
                config.ConnectionString!, tableName, config.OutputDirectory!);
            var repoResult = await this.mockGenerationService.Object.GenerateRepositoryAsync(
                config.ConnectionString!, tableName, config.OutputDirectory!);

            allSuccessful = allSuccessful && entityResult.Success && sqlResult.Success && repoResult.Success;
        }

        // Assert
        allSuccessful.Should().BeTrue();
        this.mockGenerationService.Verify(
            x => x.GenerateEntityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Exactly(2));
        this.mockGenerationService.Verify(
            x => x.GenerateSqlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Exactly(2));
        this.mockGenerationService.Verify(
            x => x.GenerateRepositoryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Exactly(2));
    }

}
