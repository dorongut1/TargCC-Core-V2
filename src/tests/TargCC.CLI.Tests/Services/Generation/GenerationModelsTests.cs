using FluentAssertions;
using TargCC.CLI.Services.Generation;
using Xunit;

namespace TargCC.CLI.Tests.Services.Generation;

/// <summary>
/// Tests for GenerationResult and GeneratedFile models.
/// </summary>
public class GenerationModelsTests
{
    [Fact]
    public void GenerationResult_DefaultConstructor_ShouldInitializeEmptyList()
    {
        // Arrange & Act
        var result = new GenerationResult();

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().BeNull();
        result.GeneratedFiles.Should().NotBeNull();
        result.GeneratedFiles.Should().BeEmpty();
        result.Duration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void GenerationResult_Success_ShouldBeSettable()
    {
        // Arrange
        var result = new GenerationResult();

        // Act
        result.Success = true;

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public void GenerationResult_ErrorMessage_ShouldBeSettable()
    {
        // Arrange
        var result = new GenerationResult();
        var errorMessage = "Test error";

        // Act
        result.ErrorMessage = errorMessage;

        // Assert
        result.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void GenerationResult_GeneratedFiles_ShouldBeAddable()
    {
        // Arrange
        var result = new GenerationResult();
        var file = new GeneratedFile
        {
            FilePath = "test.cs",
            FileType = "Entity",
            SizeBytes = 100,
            LineCount = 10
        };

        // Act
        result.GeneratedFiles.Add(file);

        // Assert
        result.GeneratedFiles.Should().HaveCount(1);
        result.GeneratedFiles[0].Should().Be(file);
    }

    [Fact]
    public void GenerationResult_Duration_ShouldBeSettable()
    {
        // Arrange
        var result = new GenerationResult();
        var duration = TimeSpan.FromSeconds(5);

        // Act
        result.Duration = duration;

        // Assert
        result.Duration.Should().Be(duration);
    }

    [Fact]
    public void GeneratedFile_Properties_ShouldBeSettable()
    {
        // Arrange
        var filePath = "C:\\Test\\Entity.cs";
        var fileType = "Entity";
        var sizeBytes = 1024L;
        var lineCount = 50;

        // Act
        var file = new GeneratedFile
        {
            FilePath = filePath,
            FileType = fileType,
            SizeBytes = sizeBytes,
            LineCount = lineCount
        };

        // Assert
        file.FilePath.Should().Be(filePath);
        file.FileType.Should().Be(fileType);
        file.SizeBytes.Should().Be(sizeBytes);
        file.LineCount.Should().Be(lineCount);
    }

    [Fact]
    public void GeneratedFile_Required_Properties_MustBeSet()
    {
        // Arrange & Act
        var file = new GeneratedFile
        {
            FilePath = "test.cs",
            FileType = "Entity"
        };

        // Assert
        file.FilePath.Should().NotBeNullOrEmpty();
        file.FileType.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerationResult_MultipleFiles_ShouldBeSupported()
    {
        // Arrange
        var result = new GenerationResult();
        var files = new[]
        {
            new GeneratedFile { FilePath = "Entity1.cs", FileType = "Entity", SizeBytes = 100, LineCount = 10 },
            new GeneratedFile { FilePath = "Entity2.cs", FileType = "Entity", SizeBytes = 200, LineCount = 20 },
            new GeneratedFile { FilePath = "Sql1.sql", FileType = "SQL", SizeBytes = 300, LineCount = 30 }
        };

        // Act
        result.GeneratedFiles.AddRange(files);

        // Assert
        result.GeneratedFiles.Should().HaveCount(3);
        result.GeneratedFiles.Should().ContainInOrder(files);
    }

    [Fact]
    public void GenerationResult_WithError_ShouldHaveErrorMessage()
    {
        // Arrange
        var result = new GenerationResult
        {
            Success = false,
            ErrorMessage = "Connection failed"
        };

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Connection failed");
        result.GeneratedFiles.Should().BeEmpty();
    }

    [Fact]
    public void GenerationResult_Successful_ShouldHaveFiles()
    {
        // Arrange
        var result = new GenerationResult
        {
            Success = true,
            Duration = TimeSpan.FromSeconds(2.5)
        };
        result.GeneratedFiles.Add(new GeneratedFile
        {
            FilePath = "Test.cs",
            FileType = "Entity",
            SizeBytes = 500,
            LineCount = 25
        });

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.GeneratedFiles.Should().HaveCount(1);
        result.Duration.Should().Be(TimeSpan.FromSeconds(2.5));
    }
}
