namespace TargCC.Core.Tests.Unit.Generators.Project;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Generators.Project;
using TargCC.Core.Generators.Project.Models;
using Xunit;

/// <summary>
/// Tests for ProjectStructureGenerator to ensure correct project structure generation.
/// </summary>
public class ProjectStructureGeneratorTests
{
    private readonly Mock<ILogger<ProjectStructureGenerator>> _mockLogger;
    private readonly Mock<ISolutionGenerator> _mockSolutionGenerator;
    private readonly Mock<IProjectFileGenerator> _mockProjectFileGenerator;
    private readonly ProjectStructureGenerator _generator;

    public ProjectStructureGeneratorTests()
    {
        _mockLogger = new Mock<ILogger<ProjectStructureGenerator>>();
        _mockSolutionGenerator = new Mock<ISolutionGenerator>();
        _mockProjectFileGenerator = new Mock<IProjectFileGenerator>();
        _generator = new ProjectStructureGenerator(
            _mockLogger.Object,
            _mockSolutionGenerator.Object,
            _mockProjectFileGenerator.Object);
    }

    /// <summary>
    /// Test 1: Constructor validates required dependencies - logger.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ProjectStructureGenerator(
                null!,
                _mockSolutionGenerator.Object,
                _mockProjectFileGenerator.Object));

        exception.ParamName.Should().Be("logger");
    }

    /// <summary>
    /// Test 2: Constructor validates required dependencies - solution generator.
    /// </summary>
    [Fact]
    public void Constructor_WithNullSolutionGenerator_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ProjectStructureGenerator(
                _mockLogger.Object,
                null!,
                _mockProjectFileGenerator.Object));

        exception.ParamName.Should().Be("solutionGenerator");
    }

    /// <summary>
    /// Test 3: Constructor validates required dependencies - project file generator.
    /// </summary>
    [Fact]
    public void Constructor_WithNullProjectFileGenerator_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ProjectStructureGenerator(
                _mockLogger.Object,
                _mockSolutionGenerator.Object,
                null!));

        exception.ParamName.Should().Be("projectFileGenerator");
    }

    /// <summary>
    /// Test 4: GetProjectsForArchitecture returns correct projects for Clean Architecture.
    /// </summary>
    [Fact]
    public void GetProjectsForArchitecture_ForCleanArchitecture_Returns5Projects()
    {
        // Arrange
        var options = CreateSampleOptions();
        options.Architecture = ArchitectureType.CleanArchitecture;
        options.IncludeTests = true;

        SetupProjectFileGeneratorMock();

        // Act
        var result = _generator.GetProjectsForArchitecture(options);

        // Assert
        result.Should().HaveCount(5);
        result.Should().Contain(p => p.Type == ProjectType.Domain);
        result.Should().Contain(p => p.Type == ProjectType.Application);
        result.Should().Contain(p => p.Type == ProjectType.Infrastructure);
        result.Should().Contain(p => p.Type == ProjectType.Api);
        result.Should().Contain(p => p.Type == ProjectType.Tests);
    }

    /// <summary>
    /// Test 5: GetProjectsForArchitecture returns correct projects for Clean Architecture without tests.
    /// </summary>
    [Fact]
    public void GetProjectsForArchitecture_ForCleanArchitectureWithoutTests_Returns4Projects()
    {
        // Arrange
        var options = CreateSampleOptions();
        options.Architecture = ArchitectureType.CleanArchitecture;
        options.IncludeTests = false;

        SetupProjectFileGeneratorMock();

        // Act
        var result = _generator.GetProjectsForArchitecture(options);

        // Assert
        result.Should().HaveCount(4);
        result.Should().NotContain(p => p.Type == ProjectType.Tests);
    }

    /// <summary>
    /// Test 6: GetProjectsForArchitecture returns correct projects for Minimal API.
    /// </summary>
    [Fact]
    public void GetProjectsForArchitecture_ForMinimalApi_ReturnsMinimalProjects()
    {
        // Arrange
        var options = CreateSampleOptions();
        options.Architecture = ArchitectureType.MinimalApi;
        options.IncludeTests = true;

        SetupProjectFileGeneratorMock();

        // Act
        var result = _generator.GetProjectsForArchitecture(options);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Type == ProjectType.Api);
        result.Should().Contain(p => p.Type == ProjectType.Tests);
    }

    /// <summary>
    /// Test 7: GetProjectsForArchitecture returns correct projects for Three Tier.
    /// </summary>
    [Fact]
    public void GetProjectsForArchitecture_ForThreeTier_Returns4Projects()
    {
        // Arrange
        var options = CreateSampleOptions();
        options.Architecture = ArchitectureType.ThreeTier;
        options.IncludeTests = true;

        SetupProjectFileGeneratorMock();

        // Act
        var result = _generator.GetProjectsForArchitecture(options);

        // Assert
        result.Should().HaveCount(4);
        result.Should().Contain(p => p.Type == ProjectType.Domain);
        result.Should().NotContain(p => p.Type == ProjectType.Application);
        result.Should().Contain(p => p.Type == ProjectType.Infrastructure);
        result.Should().Contain(p => p.Type == ProjectType.Api);
        result.Should().Contain(p => p.Type == ProjectType.Tests);
    }

    /// <summary>
    /// Test 8: CreateFolderStructureAsync creates src and tests directories.
    /// </summary>
    [Fact]
    public async Task CreateFolderStructureAsync_CreatesRequiredDirectories()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"TargCC_Test_{Guid.NewGuid():N}");

        try
        {
            var options = CreateSampleOptions();
            options.OutputDirectory = tempDir;
            options.IncludeTests = true;

            // Act
            var result = await _generator.CreateFolderStructureAsync(options);

            // Assert
            result.Should().Contain(tempDir);
            result.Should().Contain(Path.Combine(tempDir, "src"));
            result.Should().Contain(Path.Combine(tempDir, "tests"));

            Directory.Exists(tempDir).Should().BeTrue();
            Directory.Exists(Path.Combine(tempDir, "src")).Should().BeTrue();
            Directory.Exists(Path.Combine(tempDir, "tests")).Should().BeTrue();
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    /// <summary>
    /// Test 9: GenerateStructureAsync returns success result.
    /// </summary>
    [Fact]
    public async Task GenerateStructureAsync_WithValidOptions_ReturnsSuccessResult()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"TargCC_Test_{Guid.NewGuid():N}");

        try
        {
            var options = CreateSampleOptions();
            options.OutputDirectory = tempDir;

            SetupProjectFileGeneratorMock();

            _mockSolutionGenerator
                .Setup(x => x.GenerateAndSaveAsync(It.IsAny<SolutionInfo>()))
                .ReturnsAsync(Path.Combine(tempDir, "MyApp.sln"));

            _mockProjectFileGenerator
                .Setup(x => x.GenerateAndSaveAsync(
                    It.IsAny<ProjectInfo>(),
                    It.IsAny<ProjectGenerationOptions>(),
                    It.IsAny<string>()))
                .ReturnsAsync((ProjectInfo p, ProjectGenerationOptions o, string d) =>
                    Path.Combine(d, p.CsprojPath));

            // Act
            var result = await _generator.GenerateStructureAsync(options);

            // Assert
            result.Success.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
            result.GeneratedFiles.Should().NotBeEmpty();
            result.SolutionInfo.Should().NotBeNull();
            result.SolutionInfo!.Name.Should().Be("MyApp");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    /// <summary>
    /// Test 10: GenerateStructureAsync handles exceptions gracefully.
    /// </summary>
    [Fact]
    public async Task GenerateStructureAsync_OnException_ReturnsFailureResult()
    {
        // Arrange
        var options = CreateSampleOptions();

        SetupProjectFileGeneratorMock();

        _mockSolutionGenerator
            .Setup(x => x.GenerateAndSaveAsync(It.IsAny<SolutionInfo>()))
            .ThrowsAsync(new IOException("Test error"));

        // Act
        var result = await _generator.GenerateStructureAsync(options);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Test error");
    }

    /// <summary>
    /// Test 11: GenerateStructureAsync validates null options.
    /// </summary>
    [Fact]
    public async Task GenerateStructureAsync_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _generator.GenerateStructureAsync(null!));

        exception.ParamName.Should().Be("options");
    }

    private void SetupProjectFileGeneratorMock()
    {
        _mockProjectFileGenerator
            .Setup(x => x.CreateProjectInfo(
                It.IsAny<string>(),
                It.IsAny<ProjectType>(),
                It.IsAny<ProjectGenerationOptions>()))
            .Returns((string name, ProjectType type, ProjectGenerationOptions opts) =>
                new ProjectInfo
                {
                    Name = $"{name}.{type}",
                    Type = type,
                    RelativePath = type == ProjectType.Tests
                        ? $"tests\\{name}.{type}"
                        : $"src\\{name}.{type}"
                });
    }

    private static ProjectGenerationOptions CreateSampleOptions()
    {
        return new ProjectGenerationOptions
        {
            ProjectName = "MyApp",
            OutputDirectory = "C:\\Projects\\MyApp",
            ConnectionString = "Server=.;Database=Test;Trusted_Connection=True;",
            TargetFramework = "net8.0",
            EnableNullable = true,
            ImplicitUsings = true,
            LangVersion = "latest",
            Architecture = ArchitectureType.CleanArchitecture,
            IncludeTests = true
        };
    }
}
