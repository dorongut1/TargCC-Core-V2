using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Services.Metadata;
using Xunit;

namespace TargCC.Core.Tests.Unit.Services;

/// <summary>
/// Unit tests for ChangeDetectionService.
/// </summary>
public sealed class ChangeDetectionServiceTests
{
    private readonly Mock<ILogger<ChangeDetectionService>> _mockLogger;
    private readonly ChangeDetectionService _service;

    public ChangeDetectionServiceTests()
    {
        _mockLogger = new Mock<ILogger<ChangeDetectionService>>();
        _service = new ChangeDetectionService(_mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ChangeDetectionService(null!));
    }

    [Fact]
    public void ComputeHash_WithValidString_ReturnsConsistentHash()
    {
        // Arrange
        var input = "test string";

        // Act
        var hash1 = _service.ComputeHash(input);
        var hash2 = _service.ComputeHash(input);

        // Assert
        Assert.NotNull(hash1);
        Assert.NotEmpty(hash1);
        Assert.Equal(hash1, hash2); // Same input should produce same hash
        Assert.Equal(64, hash1.Length); // SHA256 produces 64 hex characters
    }

    [Fact]
    public void ComputeHash_WithDifferentStrings_ReturnsDifferentHashes()
    {
        // Arrange
        var input1 = "string one";
        var input2 = "string two";

        // Act
        var hash1 = _service.ComputeHash(input1);
        var hash2 = _service.ComputeHash(input2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_WithEmptyString_ReturnsEmptyString()
    {
        // Act
        var hash = _service.ComputeHash(string.Empty);

        // Assert
        Assert.Equal(string.Empty, hash);
    }

    [Fact]
    public void ComputeHash_WithNullString_ReturnsEmptyString()
    {
        // Act
        var hash = _service.ComputeHash(null!);

        // Assert
        Assert.Equal(string.Empty, hash);
    }

    [Fact]
    public void ComputeTableHash_WithSameTable_ReturnsConsistentHash()
    {
        // Arrange
        var table = CreateTestTable("Customer");

        // Act
        var hash1 = _service.ComputeTableHash(table);
        var hash2 = _service.ComputeTableHash(table);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeTableHash_WithDifferentTables_ReturnsDifferentHashes()
    {
        // Arrange
        var table1 = CreateTestTable("Customer");
        var table2 = CreateTestTable("Order");

        // Act
        var hash1 = _service.ComputeTableHash(table1);
        var hash2 = _service.ComputeTableHash(table2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeTableHash_WithModifiedColumn_ReturnsDifferentHash()
    {
        // Arrange
        var table1 = CreateTestTable("Customer");
        var table2 = CreateTestTable("Customer");
        table2.Columns[0].DataType = "varchar"; // Modify data type

        // Act
        var hash1 = _service.ComputeTableHash(table1);
        var hash2 = _service.ComputeTableHash(table2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HasChanged_WithNullPreviousHash_ReturnsTrue()
    {
        // Arrange
        var currentHash = "abc123";

        // Act
        var result = _service.HasChanged(currentHash, null);

        // Assert
        Assert.True(result); // First time, no previous hash
    }

    [Fact]
    public void HasChanged_WithEmptyPreviousHash_ReturnsTrue()
    {
        // Arrange
        var currentHash = "abc123";

        // Act
        var result = _service.HasChanged(currentHash, string.Empty);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasChanged_WithIdenticalHashes_ReturnsFalse()
    {
        // Arrange
        var hash = "abc123def456";

        // Act
        var result = _service.HasChanged(hash, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasChanged_WithDifferentHashes_ReturnsTrue()
    {
        // Arrange
        var currentHash = "abc123";
        var previousHash = "def456";

        // Act
        var result = _service.HasChanged(currentHash, previousHash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasChanged_IsCaseInsensitive()
    {
        // Arrange
        var currentHash = "ABC123";
        var previousHash = "abc123";

        // Act
        var result = _service.HasChanged(currentHash, previousHash);

        // Assert
        Assert.False(result); // Should be case-insensitive
    }

    [Fact]
    public void DetectTableChanges_WithNoPreviousHash_ReturnsNewChangeType()
    {
        // Arrange
        var table = CreateTestTable("Customer");

        // Act
        var result = _service.DetectTableChanges(table, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Customer", result.TableName);
        Assert.True(result.HasChanged);
        Assert.Equal("New", result.ChangeType);
        Assert.NotNull(result.CurrentHash);
        Assert.NotEmpty(result.CurrentHash);
    }

    [Fact]
    public void DetectTableChanges_WithSameHash_ReturnsUnchanged()
    {
        // Arrange
        var table = CreateTestTable("Customer");
        var previousHash = _service.ComputeTableHash(table);

        // Act
        var result = _service.DetectTableChanges(table, previousHash);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Customer", result.TableName);
        Assert.False(result.HasChanged);
        Assert.Equal("Unchanged", result.ChangeType);
        Assert.Equal(previousHash, result.CurrentHash);
    }

    [Fact]
    public void DetectTableChanges_WithDifferentHash_ReturnsModified()
    {
        // Arrange
        var table = CreateTestTable("Customer");
        var previousHash = "old_hash_value";

        // Act
        var result = _service.DetectTableChanges(table, previousHash);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Customer", result.TableName);
        Assert.True(result.HasChanged);
        Assert.Equal("Modified", result.ChangeType);
        Assert.NotEqual(previousHash, result.CurrentHash);
    }

    [Fact]
    public void DetectSchemaChanges_WithMultipleTables_ReturnsAllResults()
    {
        // Arrange
        var tables = new List<Table>
        {
            CreateTestTable("Customer"),
            CreateTestTable("Order"),
            CreateTestTable("Product")
        };

        var previousHashes = new Dictionary<string, string>
        {
            { "Customer", _service.ComputeTableHash(tables[0]) }, // Unchanged
            { "Order", "old_hash" } // Modified
            // Product is new (no previous hash)
        };

        // Act
        var results = _service.DetectSchemaChanges(tables, previousHashes).ToList();

        // Assert
        Assert.Equal(3, results.Count);

        // Customer - Unchanged
        Assert.Equal("Customer", results[0].TableName);
        Assert.False(results[0].HasChanged);
        Assert.Equal("Unchanged", results[0].ChangeType);

        // Order - Modified
        Assert.Equal("Order", results[1].TableName);
        Assert.True(results[1].HasChanged);
        Assert.Equal("Modified", results[1].ChangeType);

        // Product - New
        Assert.Equal("Product", results[2].TableName);
        Assert.True(results[2].HasChanged);
        Assert.Equal("New", results[2].ChangeType);
    }

    [Fact]
    public void DetectSchemaChanges_WithEmptyTables_ReturnsEmptyResults()
    {
        // Arrange
        var tables = new List<Table>();
        var previousHashes = new Dictionary<string, string>();

        // Act
        var results = _service.DetectSchemaChanges(tables, previousHashes).ToList();

        // Assert
        Assert.Empty(results);
    }

    /// <summary>
    /// Helper method to create a test table with columns.
    /// </summary>
    private static Table CreateTestTable(string tableName)
    {
        var table = new Table
        {
            Name = tableName,
            Schema = "dbo",
            Columns = new List<Column>
            {
                new Column
                {
                    Name = "ID",
                    DataType = "int",
                    IsNullable = false,
                    IsPrimaryKey = true,
                    IsIdentity = true,
                    OrdinalPosition = 1
                },
                new Column
                {
                    Name = "Name",
                    DataType = "nvarchar",
                    MaxLength = 100,
                    IsNullable = false,
                    OrdinalPosition = 2
                },
                new Column
                {
                    Name = "Email",
                    DataType = "nvarchar",
                    MaxLength = 255,
                    IsNullable = true,
                    OrdinalPosition = 3
                }
            }
        };

        return table;
    }
}
