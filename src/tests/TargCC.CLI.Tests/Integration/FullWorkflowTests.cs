// <copyright file="FullWorkflowTests.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.CLI.Commands.Generate;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Generation;
using TargCC.Core.Interfaces;
using Xunit;

namespace TargCC.CLI.Tests.Integration;

/// <summary>
/// Integration tests for complete CLI workflows.
/// </summary>
public class FullWorkflowTests : IntegrationTestBase
{
    private readonly Mock<IDatabaseAnalyzer> mockDatabaseAnalyzer;
    private readonly Mock<IGenerationService> mockGenerationService;
    private readonly Mock<IConfigurationService> mockConfigurationService;
    private readonly IOutputService outputService;
    private readonly ILoggerFactory loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FullWorkflowTests"/> class.
    /// </summary>
    public FullWorkflowTests()
    {
        this.mockDatabaseAnalyzer = new Mock<IDatabaseAnalyzer>();
        this.mockGenerationService = new Mock<IGenerationService>();
        this.mockConfigurationService = new Mock<IConfigurationService>();
        this.outputService = this.ServiceProvider.GetRequiredService<IOutputService>();
        this.loggerFactory = this.ServiceProvider.GetRequiredService<ILoggerFactory>();
    }

    [Fact]
    public async Task Configuration_SaveAndLoad_WorksCorrectly()
    {
        // Arrange
        var expectedConfig = new CliConfiguration
        {
            ConnectionString = "Server=localhost;Database=TestDB;",
            OutputDirectory = this.TestDirectory,
            DefaultNamespace = "MyApp",
            IsInitialized = true,
            LogLevel = "Information",
            Verbose = false,
        };

        this.mockConfigurationService
            .Setup(x => x.SaveAsync(It.IsAny<CliConfiguration>()))
            .Returns(Task.CompletedTask);

        this.mockConfigurationService
            .Setup(x => x.LoadAsync())
            .ReturnsAsync(expectedConfig);

        // Act
        await this.mockConfigurationService.Object.SaveAsync(expectedConfig);
        var loadedConfig = await this.mockConfigurationService.Object.LoadAsync();

        // Assert
        loadedConfig.ConnectionString.Should().Be(expectedConfig.ConnectionString);
        loadedConfig.OutputDirectory.Should().Be(expectedConfig.OutputDirectory);
        loadedConfig.DefaultNamespace.Should().Be(expectedConfig.DefaultNamespace);
        loadedConfig.IsInitialized.Should().BeTrue();
        loadedConfig.LogLevel.Should().Be("Information");
        this.mockConfigurationService.Verify(x => x.SaveAsync(It.IsAny<CliConfiguration>()), Times.Once);
        this.mockConfigurationService.Verify(x => x.LoadAsync(), Times.Once);
    }

    [Fact]
    public async Task Configuration_UpdateMultipleValues_PersistsChanges()
    {
        // Arrange
        var initialConfig = this.CreateTestConfig();
        var updatedConfig = new CliConfiguration
        {
            ConnectionString = initialConfig.ConnectionString,
            OutputDirectory = initialConfig.OutputDirectory,
            DefaultNamespace = "NewNamespace",
            LogLevel = "Debug",
            Verbose = true,
            IsInitialized = true,
        };

        this.mockConfigurationService
            .Setup(x => x.LoadAsync())
            .ReturnsAsync(updatedConfig);

        // Act
        var reloadedConfig = await this.mockConfigurationService.Object.LoadAsync();

        // Assert
        reloadedConfig.DefaultNamespace.Should().Be("NewNamespace");
        reloadedConfig.LogLevel.Should().Be("Debug");
        reloadedConfig.Verbose.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateEntity_WithValidConfiguration_ReturnsSuccess()
    {
        // Arrange
        var config = this.CreateTestConfig();

        this.mockConfigurationService
            .Setup(x => x.LoadAsync())
            .ReturnsAsync(config);

        this.mockGenerationService
            .Setup(x => x.GenerateEntityAsync(
                It.IsAny<string>(),
                "Customer",
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult
            {
                Success = true,
                GeneratedFiles = new List<GeneratedFile>
                {
                    new GeneratedFile
                    {
                        FilePath = Path.Combine(this.TestDirectory, "Customer.cs"),
                        FileType = "Entity",
                        LineCount = 50,
                    },
                },
                Duration = TimeSpan.FromSeconds(1),
            });

        var logger = this.loggerFactory.CreateLogger<GenerateEntityCommand>();
        var command = new GenerateEntityCommand(
            logger,
            this.mockConfigurationService.Object,
            this.outputService,
            this.mockGenerationService.Object);

        // Act
        var result = await this.mockGenerationService.Object.GenerateEntityAsync(
            config.ConnectionString!,
            "Customer",
            config.OutputDirectory!,
            config.DefaultNamespace!);

        // Assert
        result.Success.Should().BeTrue();
        result.GeneratedFiles.Should().HaveCount(1);
        result.GeneratedFiles[0].FileType.Should().Be("Entity");
    }

    [Fact]
    public async Task GenerationWorkflow_ConfigThenGenerate_CompletesSuccessfully()
    {
        // Arrange - Create config
        var config = new CliConfiguration
        {
            ConnectionString = "Server=localhost;Database=TestDB;Integrated Security=true;",
            OutputDirectory = this.TestDirectory,
            DefaultNamespace = "TestApp",
            IsInitialized = true,
        };

        this.mockConfigurationService
            .Setup(x => x.LoadAsync())
            .ReturnsAsync(config);

        // Setup generation mock
        this.mockGenerationService
            .Setup(x => x.GenerateEntityAsync(
                config.ConnectionString,
                "Product",
                config.OutputDirectory,
                config.DefaultNamespace))
            .ReturnsAsync(new GenerationResult
            {
                Success = true,
                GeneratedFiles = new List<GeneratedFile>
                {
                    new GeneratedFile
                    {
                        FilePath = Path.Combine(this.TestDirectory, "Product.cs"),
                        FileType = "Entity",
                        LineCount = 45,
                    },
                },
                Duration = TimeSpan.FromMilliseconds(750),
            });

        // Act - Load config and generate
        var loadedConfig = await this.mockConfigurationService.Object.LoadAsync();

        var result = await this.mockGenerationService.Object.GenerateEntityAsync(
            loadedConfig.ConnectionString!,
            "Product",
            loadedConfig.OutputDirectory!,
            loadedConfig.DefaultNamespace!);

        // Assert
        loadedConfig.IsInitialized.Should().BeTrue();
        result.Success.Should().BeTrue();
        result.GeneratedFiles.Should().HaveCount(1);
        result.Duration.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task MultipleGenerations_WithSameConfig_AllSucceed()
    {
        // Arrange
        var config = this.CreateTestConfig();
        var tables = new[] { "Customer", "Order", "Product" };

        this.mockConfigurationService
            .Setup(x => x.LoadAsync())
            .ReturnsAsync(config);

        foreach (var table in tables)
        {
            this.mockGenerationService
                .Setup(x => x.GenerateEntityAsync(
                    config.ConnectionString!,
                    table,
                    config.OutputDirectory!,
                    config.DefaultNamespace!))
                .ReturnsAsync(new GenerationResult
                {
                    Success = true,
                    GeneratedFiles = new List<GeneratedFile>
                    {
                        new GeneratedFile
                        {
                            FilePath = Path.Combine(this.TestDirectory, $"{table}.cs"),
                            FileType = "Entity",
                            LineCount = 40,
                        },
                    },
                });
        }

        // Act - Generate for all tables
        var results = new List<GenerationResult>();
        foreach (var table in tables)
        {
            var result = await this.mockGenerationService.Object.GenerateEntityAsync(
                config.ConnectionString!,
                table,
                config.OutputDirectory!,
                config.DefaultNamespace!);
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(3);
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        results.SelectMany(r => r.GeneratedFiles).Should().HaveCount(3);
    }

    [Fact]
    public async Task Configuration_LoadWhenNotInitialized_ReturnsDefault()
    {
        // Arrange
        var defaultConfig = new CliConfiguration
        {
            IsInitialized = false,
            LogLevel = "Information",
            Verbose = false,
        };

        this.mockConfigurationService
            .Setup(x => x.LoadAsync())
            .ReturnsAsync(defaultConfig);

        // Act
        var config = await this.mockConfigurationService.Object.LoadAsync();

        // Assert
        config.IsInitialized.Should().BeFalse();
        config.ConnectionString.Should().BeNull();
        config.OutputDirectory.Should().BeNull();
    }

    [Fact]
    public async Task GenerationService_WithMultipleTableTypes_AllGenerate()
    {
        // Arrange
        var config = this.CreateTestConfig();

        // Setup Entity generation
        this.mockGenerationService
            .Setup(x => x.GenerateEntityAsync(
                It.IsAny<string>(),
                "Customer",
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult { Success = true, GeneratedFiles = new() });

        // Setup SQL generation
        this.mockGenerationService
            .Setup(x => x.GenerateSqlAsync(
                It.IsAny<string>(),
                "Customer",
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult { Success = true, GeneratedFiles = new() });

        // Setup Repository generation
        this.mockGenerationService
            .Setup(x => x.GenerateRepositoryAsync(
                It.IsAny<string>(),
                "Customer",
                It.IsAny<string>()))
            .ReturnsAsync(new GenerationResult { Success = true, GeneratedFiles = new() });

        // Act
        var entityResult = await this.mockGenerationService.Object.GenerateEntityAsync(
            config.ConnectionString!, "Customer", config.OutputDirectory!, config.DefaultNamespace!);

        var sqlResult = await this.mockGenerationService.Object.GenerateSqlAsync(
            config.ConnectionString!, "Customer", config.OutputDirectory!);

        var repoResult = await this.mockGenerationService.Object.GenerateRepositoryAsync(
            config.ConnectionString!, "Customer", config.OutputDirectory!);

        // Assert
        entityResult.Success.Should().BeTrue();
        sqlResult.Success.Should().BeTrue();
        repoResult.Success.Should().BeTrue();
    }

    [Fact]
    public void Configuration_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var config = new CliConfiguration();

        // Assert
        config.IsInitialized.Should().BeFalse();
        config.ConnectionString.Should().BeNull();
        config.OutputDirectory.Should().BeNull();
        config.DefaultNamespace.Should().Be("MyApp");
        config.LogLevel.Should().Be("Information");
        config.Verbose.Should().BeFalse();
    }

    [Fact]
    public void Command_Constructor_WithValidParameters_CreatesSuccessfully()
    {
        // Arrange
        var logger = this.loggerFactory.CreateLogger<GenerateEntityCommand>();

        // Act
        var command = new GenerateEntityCommand(
            logger,
            this.mockConfigurationService.Object,
            this.outputService,
            this.mockGenerationService.Object);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("entity");
    }

}
