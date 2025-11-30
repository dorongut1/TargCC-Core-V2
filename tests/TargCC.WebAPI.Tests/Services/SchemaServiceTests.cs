// <copyright file="SchemaServiceTests.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.WebAPI.Services;
using Xunit;

namespace TargCC.WebAPI.Tests.Services;

/// <summary>
/// Unit tests for <see cref="SchemaService"/> - GetTablePreviewAsync method.
/// </summary>
public class SchemaServiceTests
{
    private const string TestConnectionString = "Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
    private readonly Mock<ILogger<SchemaService>> _mockLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaServiceTests"/> class.
    /// </summary>
    public SchemaServiceTests()
    {
        _mockLogger = new Mock<ILogger<SchemaService>>();
    }

    /// <summary>
    /// Tests that GetTablePreviewAsync returns preview data for valid table.
    /// </summary>
    [Fact(Skip = "Requires SQL Server connection")]
    public async Task GetTablePreviewAsync_WithValidTable_ReturnsPreviewData()
    {
        // Arrange
        var service = new SchemaService(_mockLogger.Object);
        var schemaName = "sys";
        var tableName = "databases";
        var rowCount = 5;

        // Act
        var result = await service.GetTablePreviewAsync(TestConnectionString, schemaName, tableName, rowCount);

        // Assert
        result.Should().NotBeNull();
        result.TableName.Should().Be(tableName);
        result.Columns.Should().NotBeEmpty();
        result.Data.Should().NotBeEmpty();
        result.Data.Should().HaveCountLessThanOrEqualTo(rowCount);
        result.TotalRowCount.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests that GetTablePreviewAsync throws for invalid table.
    /// </summary>
    [Fact(Skip = "Requires SQL Server connection")]
    public async Task GetTablePreviewAsync_WithInvalidTable_ThrowsException()
    {
        // Arrange
        var service = new SchemaService(_mockLogger.Object);
        var schemaName = "dbo";
        var tableName = "NonExistentTable";

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            service.GetTablePreviewAsync(TestConnectionString, schemaName, tableName, 10));
    }

    /// <summary>
    /// Tests that GetTablePreviewAsync respects row count limit.
    /// </summary>
    [Fact(Skip = "Requires SQL Server connection")]
    public async Task GetTablePreviewAsync_RespectsRowCountLimit()
    {
        // Arrange
        var service = new SchemaService(_mockLogger.Object);
        var schemaName = "sys";
        var tableName = "databases";
        var rowCount = 3;

        // Act
        var result = await service.GetTablePreviewAsync(TestConnectionString, schemaName, tableName, rowCount);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCountLessThanOrEqualTo(rowCount);
    }
}
