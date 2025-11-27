// <copyright file="ProjectGenerationTests.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using Xunit;

namespace TargCC.CLI.Tests.Integration;

/// <summary>
/// Integration tests for project generation workflows.
/// </summary>
public class ProjectGenerationTests : IntegrationTestBase
{
    private readonly Mock<ILogger> mockLogger;
    private readonly Mock<IConfigurationService> mockConfigService;
    private readonly Mock<IOutputService> mockOutputService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectGenerationTests"/> class.
    /// </summary>
    public ProjectGenerationTests()
    {
        this.mockLogger = new Mock<ILogger>();
        this.mockConfigService = new Mock<IConfigurationService>();
        this.mockOutputService = new Mock<IOutputService>();
    }

    [Fact]
    public void CreateTestConfig_CreatesValidConfiguration()
    {
        // Act
        var config = this.CreateTestConfig();

        // Assert
        config.Should().NotBeNull();
        config.ConnectionString.Should().NotBeNullOrEmpty();
        config.OutputDirectory.Should().Be(this.TestDirectory);
        config.DefaultNamespace.Should().Be("TestApp");
        config.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public void CreateTestConfig_WithCustomConnectionString_UsesProvidedValue()
    {
        // Arrange
        var customConnectionString = "Server=myserver;Database=MyDB;";

        // Act
        var config = this.CreateTestConfig(customConnectionString);

        // Assert
        config.ConnectionString.Should().Be(customConnectionString);
    }

    [Fact]
    public void TestDirectory_IsCreated()
    {
        // Assert
        Directory.Exists(this.TestDirectory).Should().BeTrue();
    }

    [Fact]
    public void ConfigFilePath_PointsToCorrectLocation()
    {
        // Assert
        this.ConfigFilePath.Should().EndWith("targcc.json");
        Path.GetDirectoryName(this.ConfigFilePath).Should().Be(this.TestDirectory);
    }

    [Fact]
    public void LoadConfig_AfterCreate_ReturnsOriginalConfig()
    {
        // Arrange
        var originalConfig = this.CreateTestConfig();

        // Act
        var loadedConfig = this.LoadConfig();

        // Assert
        loadedConfig.ConnectionString.Should().Be(originalConfig.ConnectionString);
        loadedConfig.OutputDirectory.Should().Be(originalConfig.OutputDirectory);
        loadedConfig.DefaultNamespace.Should().Be(originalConfig.DefaultNamespace);
    }

    [Fact]
    public void ServiceProvider_IsNotNull()
    {
        // Assert
        this.ServiceProvider.Should().NotBeNull();
    }

    [Fact]
    public void ServiceProvider_CanResolveOutputService()
    {
        // Act
        var outputService = this.ServiceProvider.GetService(typeof(IOutputService));

        // Assert
        outputService.Should().NotBeNull();
        outputService.Should().BeAssignableTo<IOutputService>();
    }

    [Fact]
    public void ServiceProvider_CanResolveLoggerFactory()
    {
        // Act
        var loggerFactory = this.ServiceProvider.GetService(typeof(ILoggerFactory));

        // Assert
        loggerFactory.Should().NotBeNull();
        loggerFactory.Should().BeAssignableTo<ILoggerFactory>();
    }
}
