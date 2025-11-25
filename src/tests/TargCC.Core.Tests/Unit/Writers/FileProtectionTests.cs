// <copyright file="FileProtectionTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Writers;

using TargCC.Core.Writers;
using Xunit;

/// <summary>
/// Unit tests for <see cref="FileProtection"/> class.
/// Tests the file protection logic that prevents overwriting manually edited files.
/// </summary>
public class FileProtectionTests
{
    /// <summary>
    /// Test 1: Protected VB.NET file (.prt.vb) is identified correctly.
    /// </summary>
    [Fact]
    public void IsProtected_VbPrtFile_ReturnsTrue()
    {
        // Arrange
        string filePath = "Customer.prt.vb";

        // Act
        bool result = FileProtection.IsProtected(filePath);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Test 2: Protected C# file (.prt.cs) is identified correctly.
    /// </summary>
    [Fact]
    public void IsProtected_CsPrtFile_ReturnsTrue()
    {
        // Arrange
        string filePath = "OrderUI.prt.cs";

        // Act
        bool result = FileProtection.IsProtected(filePath);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Test 3: Normal C# file is not protected.
    /// </summary>
    [Fact]
    public void IsProtected_NormalCsFile_ReturnsFalse()
    {
        // Arrange
        string filePath = "Customer.cs";

        // Act
        bool result = FileProtection.IsProtected(filePath);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Test 4: Normal VB.NET file is not protected.
    /// </summary>
    [Fact]
    public void IsProtected_NormalVbFile_ReturnsFalse()
    {
        // Arrange
        string filePath = "Customer.vb";

        // Act
        bool result = FileProtection.IsProtected(filePath);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Test 5: Full path with protected file is identified correctly.
    /// </summary>
    [Fact]
    public void IsProtected_FullPathWithPrtFile_ReturnsTrue()
    {
        // Arrange
        string filePath = @"C:\Projects\TargCC\Output\Customer.prt.cs";

        // Act
        bool result = FileProtection.IsProtected(filePath);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Test 6: Case insensitive check - uppercase extension.
    /// </summary>
    [Fact]
    public void IsProtected_UppercaseExtension_ReturnsTrue()
    {
        // Arrange
        string filePath = "Customer.PRT.CS";

        // Act
        bool result = FileProtection.IsProtected(filePath);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Test 7: Case insensitive check - mixed case.
    /// </summary>
    [Fact]
    public void IsProtected_MixedCase_ReturnsTrue()
    {
        // Arrange
        string filePath = "Customer.Prt.Vb";

        // Act
        bool result = FileProtection.IsProtected(filePath);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Test 8: Null path returns false.
    /// </summary>
    [Fact]
    public void IsProtected_NullPath_ReturnsFalse()
    {
        // Arrange
        string? filePath = null;

        // Act
        bool result = FileProtection.IsProtected(filePath!);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Test 9: Empty string returns false.
    /// </summary>
    [Fact]
    public void IsProtected_EmptyString_ReturnsFalse()
    {
        // Arrange
        string filePath = string.Empty;

        // Act
        bool result = FileProtection.IsProtected(filePath);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Test 10: Whitespace string returns false.
    /// </summary>
    [Fact]
    public void IsProtected_WhitespaceString_ReturnsFalse()
    {
        // Arrange
        string filePath = "   ";

        // Act
        bool result = FileProtection.IsProtected(filePath);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Test 11: File with .prt. in middle of name is protected.
    /// </summary>
    [Fact]
    public void IsProtected_PrtInMiddleOfName_ReturnsTrue()
    {
        // Arrange
        string filePath = "Customer.prt.Generated.cs";

        // Act
        bool result = FileProtection.IsProtected(filePath);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Test 12: EnsureNotProtected does not throw for normal file.
    /// </summary>
    [Fact]
    public void EnsureNotProtected_NormalFile_DoesNotThrow()
    {
        // Arrange
        string filePath = "Customer.cs";

        // Act & Assert
        var exception = Record.Exception(() => FileProtection.EnsureNotProtected(filePath));
        Assert.Null(exception);
    }

    /// <summary>
    /// Test 13: EnsureNotProtected throws for protected file.
    /// </summary>
    [Fact]
    public void EnsureNotProtected_ProtectedFile_ThrowsProtectedFileException()
    {
        // Arrange
        string filePath = "Customer.prt.cs";

        // Act & Assert
        var exception = Assert.Throws<ProtectedFileException>(
            () => FileProtection.EnsureNotProtected(filePath));

        Assert.Equal(filePath, exception.FilePath);
        Assert.Contains("Cannot overwrite protected file", exception.Message);
        Assert.Contains("manual code edits", exception.Message);
    }

    /// <summary>
    /// Test 14: GetProtectedPatterns returns expected patterns.
    /// </summary>
    [Fact]
    public void GetProtectedPatterns_ReturnsExpectedPatterns()
    {
        // Act
        var patterns = FileProtection.GetProtectedPatterns();

        // Assert
        Assert.NotNull(patterns);
        Assert.NotEmpty(patterns);
        Assert.Contains(".prt.vb", patterns);
        Assert.Contains(".prt.cs", patterns);
    }

    /// <summary>
    /// Test 15: GetProtectedPatterns returns a copy (not reference).
    /// </summary>
    [Fact]
    public void GetProtectedPatterns_ReturnsCopy_NotReference()
    {
        // Arrange
        var patterns1 = FileProtection.GetProtectedPatterns();
        var patterns2 = FileProtection.GetProtectedPatterns();

        // Act - modify first array
        patterns1[0] = "modified";

        // Assert - second array should be unchanged
        Assert.NotEqual(patterns1[0], patterns2[0]);
    }

    /// <summary>
    /// Test 16: Multiple .prt. in filename is still protected.
    /// </summary>
    [Fact]
    public void IsProtected_MultiplePrtInName_ReturnsTrue()
    {
        // Arrange
        string filePath = "Customer.prt.prt.cs";

        // Act
        bool result = FileProtection.IsProtected(filePath);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Test 17: Filename with prt but not as extension separator is not protected.
    /// </summary>
    [Fact]
    public void IsProtected_PrtNotAsExtension_ReturnsFalse()
    {
        // Arrange
        string filePath = "CustomerPrt.cs"; // "prt" is part of filename, not extension

        // Act
        bool result = FileProtection.IsProtected(filePath);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Test 18: Linux-style path with protected file.
    /// </summary>
    [Fact]
    public void IsProtected_LinuxPath_ReturnsTrue()
    {
        // Arrange
        string filePath = "/home/user/projects/Customer.prt.cs";

        // Act
        bool result = FileProtection.IsProtected(filePath);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Test 19: Network path with protected file.
    /// </summary>
    [Fact]
    public void IsProtected_NetworkPath_ReturnsTrue()
    {
        // Arrange
        string filePath = @"\\server\share\Customer.prt.vb";

        // Act
        bool result = FileProtection.IsProtected(filePath);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Test 20: Relative path with protected file.
    /// </summary>
    [Fact]
    public void IsProtected_RelativePath_ReturnsTrue()
    {
        // Arrange
        string filePath = @"..\..\Output\Customer.prt.cs";

        // Act
        bool result = FileProtection.IsProtected(filePath);

        // Assert
        Assert.True(result);
    }
}
