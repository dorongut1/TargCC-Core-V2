namespace TargCC.Core.Tests.Unit.Generators.Project;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Generators.Project;
using TargCC.Core.Generators.Project.Models;
using Xunit;

/// <summary>
/// Tests for ProjectFileGenerator to ensure correct .csproj file generation.
/// </summary>
public class ProjectFileGeneratorTests
{
    private readonly Mock<ILogger<ProjectFileGenerator>> _mockLogger;
    private readonly ProjectFileGenerator _generator;

    public ProjectFileGeneratorTests()
    {
        _mockLogger = new Mock<ILogger<ProjectFileGenerator>>();
        _generator = new ProjectFileGenerator(_mockLogger.Object);
    }

    /// <summary>
    /// Test 1: Constructor validates required dependencies.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ProjectFileGenerator(null!));

        exception.ParamName.Should().Be("logger");
    }

    /// <summary>
    /// Test 2: Generate validates null projectInfo parameter.
    /// </summary>
    [Fact]
    public void Generate_WithNullProjectInfo_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateSampleOptions();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            _generator.Generate(null!, options));

        exception.ParamName.Should().Be("projectInfo");
    }

    /// <summary>
    /// Test 3: Generate validates null options parameter.
    /// </summary>
    [Fact]
    public void Generate_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var projectInfo = CreateSampleProjectInfo();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            _generator.Generate(projectInfo, null!));

        exception.ParamName.Should().Be("options");
    }

    /// <summary>
    /// Test 4: Generates valid project file with SDK and target framework.
    /// </summary>
    [Fact]
    public void Generate_WithValidInputs_GeneratesBasicProjectStructure()
    {
        // Arrange
        var projectInfo = CreateSampleProjectInfo();
        var options = CreateSampleOptions();

        // Act
        var result = _generator.Generate(projectInfo, options);

        // Assert
        result.Should().Contain("<Project Sdk=\"Microsoft.NET.Sdk\">");
        result.Should().Contain("<TargetFramework>net8.0</TargetFramework>");
        result.Should().Contain("<Nullable>enable</Nullable>");
        result.Should().Contain("<ImplicitUsings>enable</ImplicitUsings>");
        result.Should().Contain("<LangVersion>latest</LangVersion>");
        result.Should().Contain("</Project>");
    }

    /// <summary>
    /// Test 5: Generates OutputType for API project.
    /// </summary>
    [Fact]
    public void Generate_WithApiProject_IncludesExeOutputType()
    {
        // Arrange
        var projectInfo = CreateSampleProjectInfo();
        projectInfo.Type = ProjectType.Api;
        projectInfo.OutputType = "Exe";
        var options = CreateSampleOptions();

        // Act
        var result = _generator.Generate(projectInfo, options);

        // Assert
        result.Should().Contain("<OutputType>Exe</OutputType>");
    }

    /// <summary>
    /// Test 6: Generates package references.
    /// </summary>
    [Fact]
    public void Generate_WithPackageReferences_IncludesPackageReferences()
    {
        // Arrange
        var projectInfo = CreateSampleProjectInfo();
        projectInfo.PackageReferences.Add(new PackageReference { Name = "MediatR", Version = "12.2.0" });
        projectInfo.PackageReferences.Add(new PackageReference { Name = "FluentValidation", Version = "11.9.0" });
        var options = CreateSampleOptions();

        // Act
        var result = _generator.Generate(projectInfo, options);

        // Assert
        result.Should().Contain("<ItemGroup>");
        result.Should().Contain("<PackageReference Include=\"MediatR\" Version=\"12.2.0\" />");
        result.Should().Contain("<PackageReference Include=\"FluentValidation\" Version=\"11.9.0\" />");
    }

    /// <summary>
    /// Test 7: Generates package references with PrivateAssets.
    /// </summary>
    [Fact]
    public void Generate_WithPrivateAssets_IncludesPrivateAssetsElement()
    {
        // Arrange
        var projectInfo = CreateSampleProjectInfo();
        projectInfo.PackageReferences.Add(new PackageReference
        {
            Name = "coverlet.collector",
            Version = "6.0.1",
            PrivateAssets = "all"
        });
        var options = CreateSampleOptions();

        // Act
        var result = _generator.Generate(projectInfo, options);

        // Assert
        result.Should().Contain("<PackageReference Include=\"coverlet.collector\" Version=\"6.0.1\">");
        result.Should().Contain("<PrivateAssets>all</PrivateAssets>");
    }

    /// <summary>
    /// Test 8: Generates project references.
    /// </summary>
    [Fact]
    public void Generate_WithProjectReferences_IncludesProjectReferences()
    {
        // Arrange
        var projectInfo = CreateSampleProjectInfo();
        projectInfo.ProjectReferences.Add("..\\MyApp.Domain\\MyApp.Domain.csproj");
        var options = CreateSampleOptions();

        // Act
        var result = _generator.Generate(projectInfo, options);

        // Assert
        result.Should().Contain("<ProjectReference Include=\"..\\MyApp.Domain\\MyApp.Domain.csproj\" />");
    }

    /// <summary>
    /// Test 9: Generates documentation file for non-test projects.
    /// </summary>
    [Fact]
    public void Generate_WithDomainProject_IncludesDocumentationFile()
    {
        // Arrange
        var projectInfo = CreateSampleProjectInfo();
        projectInfo.Type = ProjectType.Domain;
        var options = CreateSampleOptions();

        // Act
        var result = _generator.Generate(projectInfo, options);

        // Assert
        result.Should().Contain("<GenerateDocumentationFile>true</GenerateDocumentationFile>");
        result.Should().Contain("<NoWarn>$(NoWarn);1591</NoWarn>");
    }

    /// <summary>
    /// Test 10: Does not generate documentation file for test projects.
    /// </summary>
    [Fact]
    public void Generate_WithTestProject_DoesNotIncludeDocumentationFile()
    {
        // Arrange
        var projectInfo = CreateSampleProjectInfo();
        projectInfo.Type = ProjectType.Tests;
        var options = CreateSampleOptions();

        // Act
        var result = _generator.Generate(projectInfo, options);

        // Assert
        result.Should().NotContain("<GenerateDocumentationFile>true</GenerateDocumentationFile>");
    }

    /// <summary>
    /// Test 11: CreateProjectInfo creates correct project info for Domain.
    /// </summary>
    [Fact]
    public void CreateProjectInfo_ForDomain_CreatesCorrectInfo()
    {
        // Arrange
        var options = CreateSampleOptions();

        // Act
        var result = _generator.CreateProjectInfo("MyApp", ProjectType.Domain, options);

        // Assert
        result.Name.Should().Be("MyApp.Domain");
        result.Type.Should().Be(ProjectType.Domain);
        result.RelativePath.Should().Contain("src");
        result.OutputType.Should().Be("Library");
        result.RootNamespace.Should().Be("MyApp.Domain");
    }

    /// <summary>
    /// Test 12: CreateProjectInfo creates correct project info for Application.
    /// </summary>
    [Fact]
    public void CreateProjectInfo_ForApplication_IncludesRequiredPackages()
    {
        // Arrange
        var options = CreateSampleOptions();

        // Act
        var result = _generator.CreateProjectInfo("MyApp", ProjectType.Application, options);

        // Assert
        result.Name.Should().Be("MyApp.Application");
        result.PackageReferences.Should().Contain(p => p.Name == "MediatR");
        result.PackageReferences.Should().Contain(p => p.Name == "FluentValidation");
        result.PackageReferences.Should().Contain(p => p.Name == "AutoMapper");
        result.ProjectReferences.Should().Contain(r => r.Contains("Domain"));
    }

    /// <summary>
    /// Test 13: CreateProjectInfo creates correct project info for Infrastructure.
    /// </summary>
    [Fact]
    public void CreateProjectInfo_ForInfrastructure_IncludesDataAccessPackages()
    {
        // Arrange
        var options = CreateSampleOptions();

        // Act
        var result = _generator.CreateProjectInfo("MyApp", ProjectType.Infrastructure, options);

        // Assert
        result.Name.Should().Be("MyApp.Infrastructure");
        result.PackageReferences.Should().Contain(p => p.Name == "Microsoft.EntityFrameworkCore");
        result.PackageReferences.Should().Contain(p => p.Name == "Dapper");
        result.ProjectReferences.Should().Contain(r => r.Contains("Domain"));
        result.ProjectReferences.Should().Contain(r => r.Contains("Application"));
    }

    /// <summary>
    /// Test 14: CreateProjectInfo creates correct project info for API.
    /// </summary>
    [Fact]
    public void CreateProjectInfo_ForApi_IsExecutableWithAllReferences()
    {
        // Arrange
        var options = CreateSampleOptions();

        // Act
        var result = _generator.CreateProjectInfo("MyApp", ProjectType.Api, options);

        // Assert
        result.Name.Should().Be("MyApp.API");
        result.OutputType.Should().Be("Exe");
        result.PackageReferences.Should().Contain(p => p.Name == "Swashbuckle.AspNetCore");
        result.ProjectReferences.Should().HaveCount(3);
    }

    /// <summary>
    /// Test 15: CreateProjectInfo creates correct project info for Tests.
    /// </summary>
    [Fact]
    public void CreateProjectInfo_ForTests_IncludesTestingPackages()
    {
        // Arrange
        var options = CreateSampleOptions();

        // Act
        var result = _generator.CreateProjectInfo("MyApp", ProjectType.Tests, options);

        // Assert
        result.Name.Should().Be("MyApp.Tests");
        result.RelativePath.Should().Contain("tests");
        result.PackageReferences.Should().Contain(p => p.Name == "xunit");
        result.PackageReferences.Should().Contain(p => p.Name == "Moq");
        result.PackageReferences.Should().Contain(p => p.Name == "FluentAssertions");
        result.ProjectReferences.Should().HaveCount(4);
    }

    /// <summary>
    /// Test 16: GenerateAndSaveAsync creates file on disk.
    /// </summary>
    [Fact]
    public async Task GenerateAndSaveAsync_CreatesFileOnDisk()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"TargCC_Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var projectInfo = new ProjectInfo
            {
                Name = "TestProject.Domain",
                Type = ProjectType.Domain,
                RelativePath = "src\\TestProject.Domain"
            };
            var options = CreateSampleOptions();

            // Act
            var filePath = await _generator.GenerateAndSaveAsync(projectInfo, options, tempDir);

            // Assert
            filePath.Should().Contain("TestProject.Domain.csproj");
            File.Exists(filePath).Should().BeTrue();

            var content = await File.ReadAllTextAsync(filePath);
            content.Should().Contain("<Project Sdk=\"Microsoft.NET.Sdk\">");
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

    private static ProjectInfo CreateSampleProjectInfo()
    {
        return new ProjectInfo
        {
            Name = "MyApp.Domain",
            Type = ProjectType.Domain,
            RelativePath = "src\\MyApp.Domain"
        };
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
            LangVersion = "latest"
        };
    }
}
