namespace TargCC.Core.Tests.Unit.Generators.Project;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Generators.Project;
using TargCC.Core.Generators.Project.Models;
using Xunit;

/// <summary>
/// Tests for SolutionGenerator to ensure correct .sln file generation.
/// </summary>
public class SolutionGeneratorTests
{
    private readonly Mock<ILogger<SolutionGenerator>> _mockLogger;
    private readonly SolutionGenerator _generator;

    public SolutionGeneratorTests()
    {
        _mockLogger = new Mock<ILogger<SolutionGenerator>>();
        _generator = new SolutionGenerator(_mockLogger.Object);
    }

    /// <summary>
    /// Test 1: Constructor validates required dependencies.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SolutionGenerator(null!));

        exception.ParamName.Should().Be("logger");
    }

    /// <summary>
    /// Test 2: Generate validates null solutionInfo parameter.
    /// </summary>
    [Fact]
    public void Generate_WithNullSolutionInfo_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            _generator.Generate(null!));

        exception.ParamName.Should().Be("solutionInfo");
    }

    /// <summary>
    /// Test 3: Generates valid solution file header.
    /// </summary>
    [Fact]
    public void Generate_WithValidSolutionInfo_GeneratesCorrectHeader()
    {
        // Arrange
        var solutionInfo = CreateSampleSolutionInfo();

        // Act
        var result = _generator.Generate(solutionInfo);

        // Assert
        result.Should().Contain("Microsoft Visual Studio Solution File, Format Version 12.00");
        result.Should().Contain("# Visual Studio Version 17");
        result.Should().Contain("VisualStudioVersion = 17.0.31903.59");
        result.Should().Contain("MinimumVisualStudioVersion = 10.0.40219.1");
    }

    /// <summary>
    /// Test 4: Generates project entries correctly.
    /// </summary>
    [Fact]
    public void Generate_WithProjects_GeneratesProjectEntries()
    {
        // Arrange
        var solutionInfo = CreateSampleSolutionInfo();
        var project = new ProjectInfo
        {
            Name = "MyApp.Domain",
            Type = ProjectType.Domain,
            RelativePath = "src\\MyApp.Domain"
        };
        solutionInfo.Projects.Add(project);

        // Act
        var result = _generator.Generate(solutionInfo);

        // Assert
        result.Should().Contain("Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\")");
        result.Should().Contain("\"MyApp.Domain\"");
        result.Should().Contain("src\\MyApp.Domain\\MyApp.Domain.csproj");
        result.Should().Contain("EndProject");
    }

    /// <summary>
    /// Test 5: Generates solution folders.
    /// </summary>
    [Fact]
    public void Generate_WithFolders_GeneratesSolutionFolders()
    {
        // Arrange
        var solutionInfo = CreateSampleSolutionInfo();
        var srcFolder = new SolutionFolder { Name = "src" };
        solutionInfo.Folders.Add(srcFolder);

        // Act
        var result = _generator.Generate(solutionInfo);

        // Assert
        result.Should().Contain("Project(\"{2150E333-8FDC-42A3-9474-1A3956D46DE8}\")");
        result.Should().Contain("\"src\"");
    }

    /// <summary>
    /// Test 6: Generates global sections.
    /// </summary>
    [Fact]
    public void Generate_GeneratesGlobalSections()
    {
        // Arrange
        var solutionInfo = CreateSampleSolutionInfo();
        var project = new ProjectInfo
        {
            Name = "MyApp.Domain",
            Type = ProjectType.Domain,
            RelativePath = "src\\MyApp.Domain"
        };
        solutionInfo.Projects.Add(project);

        // Act
        var result = _generator.Generate(solutionInfo);

        // Assert
        result.Should().Contain("Global");
        result.Should().Contain("GlobalSection(SolutionConfigurationPlatforms) = preSolution");
        result.Should().Contain("Debug|Any CPU = Debug|Any CPU");
        result.Should().Contain("Release|Any CPU = Release|Any CPU");
        result.Should().Contain("EndGlobalSection");
        result.Should().Contain("GlobalSection(ProjectConfigurationPlatforms) = postSolution");
        result.Should().Contain("GlobalSection(SolutionProperties) = preSolution");
        result.Should().Contain("HideSolutionNode = FALSE");
        result.Should().Contain("EndGlobal");
    }

    /// <summary>
    /// Test 7: Generates nested projects section when folders have projects.
    /// </summary>
    [Fact]
    public void Generate_WithNestedProjects_GeneratesNestedProjectsSection()
    {
        // Arrange
        var solutionInfo = CreateSampleSolutionInfo();
        var project = new ProjectInfo
        {
            Name = "MyApp.Domain",
            Type = ProjectType.Domain,
            RelativePath = "src\\MyApp.Domain"
        };
        solutionInfo.Projects.Add(project);

        var srcFolder = new SolutionFolder { Name = "src" };
        srcFolder.ProjectGuids.Add(project.ProjectGuid);
        solutionInfo.Folders.Add(srcFolder);

        // Act
        var result = _generator.Generate(solutionInfo);

        // Assert
        result.Should().Contain("GlobalSection(NestedProjects) = preSolution");
    }

    /// <summary>
    /// Test 8: GenerateAndSaveAsync creates file on disk.
    /// </summary>
    [Fact]
    public async Task GenerateAndSaveAsync_CreatesFileOnDisk()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"TargCC_Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var solutionInfo = new SolutionInfo
            {
                Name = "TestSolution",
                OutputDirectory = tempDir
            };

            // Act
            var filePath = await _generator.GenerateAndSaveAsync(solutionInfo);

            // Assert
            filePath.Should().Be(Path.Combine(tempDir, "TestSolution.sln"));
            File.Exists(filePath).Should().BeTrue();

            var content = await File.ReadAllTextAsync(filePath);
            content.Should().Contain("Microsoft Visual Studio Solution File");
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

    private static SolutionInfo CreateSampleSolutionInfo()
    {
        return new SolutionInfo
        {
            Name = "MyApp",
            OutputDirectory = "C:\\Projects\\MyApp"
        };
    }
}
