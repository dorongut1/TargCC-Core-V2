// <copyright file="FileWriterTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Writers;

using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Writers;
using Xunit;

/// <summary>
/// Unit tests for <see cref="FileWriter"/> class.
/// Tests file writing operations with protection for manually edited files.
/// </summary>
public class FileWriterTests : IDisposable
{
    private readonly Mock<ILogger<FileWriter>> _mockLogger;
    private readonly FileWriter _fileWriter;
    private readonly string _testDirectory;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileWriterTests"/> class.
    /// </summary>
    public FileWriterTests()
    {
        _mockLogger = new Mock<ILogger<FileWriter>>();
        _fileWriter = new FileWriter(_mockLogger.Object);

        // Create unique test directory for each test run
        _testDirectory = Path.Combine(Path.GetTempPath(), "TargCC_Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    /// Test 1: WriteFileAsync creates a new file successfully.
    /// </summary>
    [Fact]
    public async Task WriteFileAsync_NewFile_CreatesFile()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "Customer.cs");
        string content = "public class Customer { }";

        // Act
        await _fileWriter.WriteFileAsync(filePath, content);

        // Assert
        Assert.True(File.Exists(filePath));
        string writtenContent = await File.ReadAllTextAsync(filePath);
        Assert.Equal(content, writtenContent);
    }

    /// <summary>
    /// Test 2: WriteFileAsync creates parent directories if they don't exist.
    /// </summary>
    [Fact]
    public async Task WriteFileAsync_NonExistentDirectory_CreatesDirectory()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "Nested", "Deep", "Customer.cs");
        string content = "public class Customer { }";

        // Act
        await _fileWriter.WriteFileAsync(filePath, content);

        // Assert
        Assert.True(File.Exists(filePath));
        Assert.True(Directory.Exists(Path.GetDirectoryName(filePath)));
    }

    /// <summary>
    /// Test 3: WriteFileAsync throws for protected file (.prt.cs).
    /// </summary>
    [Fact]
    public async Task WriteFileAsync_ProtectedFile_ThrowsProtectedFileException()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "Customer.prt.cs");
        string content = "manual code";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProtectedFileException>(
            () => _fileWriter.WriteFileAsync(filePath, content));

        Assert.Equal(filePath, exception.FilePath);
        Assert.False(File.Exists(filePath)); // File should not be created
    }

    /// <summary>
    /// Test 4: WriteFileAsync throws for protected file (.prt.vb).
    /// </summary>
    [Fact]
    public async Task WriteFileAsync_ProtectedVbFile_ThrowsProtectedFileException()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "OrderUI.prt.vb");
        string content = "manual code";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProtectedFileException>(
            () => _fileWriter.WriteFileAsync(filePath, content));

        Assert.Equal(filePath, exception.FilePath);
    }

    /// <summary>
    /// Test 5: WriteFileAsync creates backup when overwriting existing file.
    /// </summary>
    [Fact]
    public async Task WriteFileAsync_ExistingFile_CreatesBackup()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "Customer.cs");
        string originalContent = "original content";
        string newContent = "new content";

        await File.WriteAllTextAsync(filePath, originalContent);

        // Act
        await _fileWriter.WriteFileAsync(filePath, newContent);

        // Assert
        string backupPath = filePath + ".bak";
        Assert.True(File.Exists(backupPath));
        string backupContent = await File.ReadAllTextAsync(backupPath);
        Assert.Equal(originalContent, backupContent);

        string currentContent = await File.ReadAllTextAsync(filePath);
        Assert.Equal(newContent, currentContent);
    }

    /// <summary>
    /// Test 6: WriteFileAsync throws ArgumentNullException for null file path.
    /// </summary>
    [Fact]
    public async Task WriteFileAsync_NullFilePath_ThrowsArgumentNullException()
    {
        // Arrange
        string? filePath = null;
        string content = "content";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _fileWriter.WriteFileAsync(filePath!, content));
    }

    /// <summary>
    /// Test 7: WriteFileAsync throws ArgumentNullException for null content.
    /// </summary>
    [Fact]
    public async Task WriteFileAsync_NullContent_ThrowsArgumentNullException()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "Customer.cs");
        string? content = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _fileWriter.WriteFileAsync(filePath, content!));
    }

    /// <summary>
    /// Test 8: IsProtectedFile correctly identifies protected files.
    /// </summary>
    [Fact]
    public void IsProtectedFile_ProtectedFile_ReturnsTrue()
    {
        // Arrange
        string filePath = "Customer.prt.cs";

        // Act
        bool result = _fileWriter.IsProtectedFile(filePath);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Test 9: IsProtectedFile correctly identifies normal files.
    /// </summary>
    [Fact]
    public void IsProtectedFile_NormalFile_ReturnsFalse()
    {
        // Arrange
        string filePath = "Customer.cs";

        // Act
        bool result = _fileWriter.IsProtectedFile(filePath);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Test 10: UpdateFileAsync replaces content successfully.
    /// </summary>
    [Fact]
    public async Task UpdateFileAsync_ValidContent_ReplacesContent()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "Customer.cs");
        string originalContent = "public class Customer { public string FirstName { get; set; } }";
        string oldContent = "FirstName";
        string newContent = "FullName";

        await File.WriteAllTextAsync(filePath, originalContent);

        // Act
        await _fileWriter.UpdateFileAsync(filePath, oldContent, newContent);

        // Assert
        string updatedContent = await File.ReadAllTextAsync(filePath);
        Assert.Contains("FullName", updatedContent);
        Assert.DoesNotContain("FirstName", updatedContent);
    }

    /// <summary>
    /// Test 11: UpdateFileAsync throws when file doesn't exist.
    /// </summary>
    [Fact]
    public async Task UpdateFileAsync_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "NonExistent.cs");
        string oldContent = "old";
        string newContent = "new";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _fileWriter.UpdateFileAsync(filePath, oldContent, newContent));
    }

    /// <summary>
    /// Test 12: UpdateFileAsync throws when old content not found.
    /// </summary>
    [Fact]
    public async Task UpdateFileAsync_OldContentNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "Customer.cs");
        string originalContent = "public class Customer { }";
        string oldContent = "NotFound";
        string newContent = "new";

        await File.WriteAllTextAsync(filePath, originalContent);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _fileWriter.UpdateFileAsync(filePath, oldContent, newContent));

        Assert.Contains("Old content not found", exception.Message);
    }

    /// <summary>
    /// Test 13: UpdateFileAsync throws when old content appears multiple times.
    /// </summary>
    [Fact]
    public async Task UpdateFileAsync_OldContentAppearsMultipleTimes_ThrowsInvalidOperationException()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "Customer.cs");
        string originalContent = "Name Name Name";
        string oldContent = "Name";
        string newContent = "FullName";

        await File.WriteAllTextAsync(filePath, originalContent);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _fileWriter.UpdateFileAsync(filePath, oldContent, newContent));

        Assert.Contains("appears 3 times", exception.Message);
    }

    /// <summary>
    /// Test 14: UpdateFileAsync throws for protected file.
    /// </summary>
    [Fact]
    public async Task UpdateFileAsync_ProtectedFile_ThrowsProtectedFileException()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "Customer.prt.cs");
        string originalContent = "manual code";
        string oldContent = "manual";
        string newContent = "updated";

        // Create the file first (bypassing protection for setup)
        await File.WriteAllTextAsync(filePath, originalContent);

        // Act & Assert
        await Assert.ThrowsAsync<ProtectedFileException>(
            () => _fileWriter.UpdateFileAsync(filePath, oldContent, newContent));
    }

    /// <summary>
    /// Test 15: UpdateFileAsync creates backup before updating.
    /// </summary>
    [Fact]
    public async Task UpdateFileAsync_ExistingFile_CreatesBackup()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "Customer.cs");
        string originalContent = "original Name content";
        string oldContent = "Name";
        string newContent = "FullName";

        await File.WriteAllTextAsync(filePath, originalContent);

        // Act
        await _fileWriter.UpdateFileAsync(filePath, oldContent, newContent);

        // Assert
        string backupPath = filePath + ".bak";
        Assert.True(File.Exists(backupPath));
        string backupContent = await File.ReadAllTextAsync(backupPath);
        Assert.Equal(originalContent, backupContent);
    }

    /// <summary>
    /// Test 16: FileExistsAsync returns true for existing file.
    /// </summary>
    [Fact]
    public async Task FileExistsAsync_ExistingFile_ReturnsTrue()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "Customer.cs");
        await File.WriteAllTextAsync(filePath, "content");

        // Act
        bool result = await _fileWriter.FileExistsAsync(filePath);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Test 17: FileExistsAsync returns false for non-existent file.
    /// </summary>
    [Fact]
    public async Task FileExistsAsync_NonExistentFile_ReturnsFalse()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "NonExistent.cs");

        // Act
        bool result = await _fileWriter.FileExistsAsync(filePath);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Test 18: EnsureDirectoryExistsAsync creates directory.
    /// </summary>
    [Fact]
    public async Task EnsureDirectoryExistsAsync_NonExistentDirectory_CreatesDirectory()
    {
        // Arrange
        string directoryPath = Path.Combine(_testDirectory, "NewDirectory");

        // Act
        await _fileWriter.EnsureDirectoryExistsAsync(directoryPath);

        // Assert
        Assert.True(Directory.Exists(directoryPath));
    }

    /// <summary>
    /// Test 19: EnsureDirectoryExistsAsync is idempotent (can be called multiple times).
    /// </summary>
    [Fact]
    public async Task EnsureDirectoryExistsAsync_ExistingDirectory_DoesNotThrow()
    {
        // Arrange
        string directoryPath = Path.Combine(_testDirectory, "ExistingDirectory");
        Directory.CreateDirectory(directoryPath);

        // Act & Assert
        var exception = await Record.ExceptionAsync(
            () => _fileWriter.EnsureDirectoryExistsAsync(directoryPath));

        Assert.Null(exception);
    }

    /// <summary>
    /// Test 20: WriteFileAsync uses UTF-8 encoding.
    /// </summary>
    [Fact]
    public async Task WriteFileAsync_WithUnicodeContent_PreservesEncoding()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "Unicode.cs");
        string content = "// שלום עולם - Hebrew\n// 你好世界 - Chinese\npublic class Test { }";

        // Act
        await _fileWriter.WriteFileAsync(filePath, content);

        // Assert
        string readContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        Assert.Equal(content, readContent);
    }

    /// <summary>
    /// Test 21: Constructor throws ArgumentNullException for null logger.
    /// </summary>
    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        ILogger<FileWriter>? logger = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileWriter(logger!));
    }

    /// <summary>
    /// Test 22: WriteFileAsync logs information message.
    /// </summary>
    [Fact]
    public async Task WriteFileAsync_Success_LogsInformation()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "Customer.cs");
        string content = "public class Customer { }";

        // Act
        await _fileWriter.WriteFileAsync(filePath, content);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully wrote file")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Test 23: Multiple backups create timestamped files.
    /// </summary>
    [Fact]
    public async Task WriteFileAsync_MultipleWrites_CreatesTimestampedBackups()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "Customer.cs");
        await File.WriteAllTextAsync(filePath, "version 1");

        // Act - Write multiple times
        await _fileWriter.WriteFileAsync(filePath, "version 2");
        await Task.Delay(1000); // Ensure different timestamp
        await _fileWriter.WriteFileAsync(filePath, "version 3");

        // Assert
        var backupFiles = Directory.GetFiles(_testDirectory, "Customer.cs*.bak");
        Assert.True(backupFiles.Length >= 2); // At least 2 backups
    }

    /// <summary>
    /// Cleanup test directory after each test.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected dispose pattern.
    /// </summary>
    /// <param name="disposing">Whether disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Cleanup test directory
                if (Directory.Exists(_testDirectory))
                {
                    try
                    {
                        Directory.Delete(_testDirectory, recursive: true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }

            _disposed = true;
        }
    }
}
