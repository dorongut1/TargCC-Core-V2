// <copyright file="ConnectionServiceTests.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.WebAPI.Models;
using TargCC.WebAPI.Services;
using Xunit;

namespace TargCC.WebAPI.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ConnectionService"/>.
/// </summary>
public class ConnectionServiceTests : IDisposable
{
    private readonly Mock<ILogger<ConnectionService>> _mockLogger;
    private readonly ConnectionService _service;
    private readonly string _testFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionServiceTests"/> class.
    /// </summary>
    public ConnectionServiceTests()
    {
        _mockLogger = new Mock<ILogger<ConnectionService>>();
        _service = new ConnectionService(_mockLogger.Object);
        
        // Get the actual file path for cleanup
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _testFilePath = Path.Combine(appDataPath, "TargCC", "connections.json");
    }

    /// <summary>
    /// Disposes test resources.
    /// </summary>
    public void Dispose()
    {
        // Clean up test file after each test
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    /// <summary>
    /// Tests that GetConnectionsAsync returns empty list when no connections exist.
    /// </summary>
    [Fact]
    public async Task GetConnectionsAsync_WhenNoConnections_ReturnsEmptyList()
    {
        // Arrange
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }

        // Act
        var result = await _service.GetConnectionsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that AddConnectionAsync successfully adds a new connection.
    /// </summary>
    [Fact]
    public async Task AddConnectionAsync_AddsConnectionSuccessfully()
    {
        // Arrange
        var connection = new DatabaseConnectionInfo
        {
            Name = "Test Connection",
            Server = "localhost",
            Database = "TestDB",
            ConnectionString = "Server=localhost;Database=TestDB;",
            UseIntegratedSecurity = true
        };

        // Act
        var result = await _service.AddConnectionAsync(connection);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Name.Should().Be("Test Connection");
        result.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.LastUsed.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Tests that GetConnectionsAsync returns connections ordered by last used.
    /// </summary>
    [Fact]
    public async Task GetConnectionsAsync_ReturnsConnectionsOrderedByLastUsed()
    {
        // Arrange
        var connection1 = new DatabaseConnectionInfo { Name = "Connection 1", Server = "server1", Database = "db1", ConnectionString = "cs1" };
        var connection2 = new DatabaseConnectionInfo { Name = "Connection 2", Server = "server2", Database = "db2", ConnectionString = "cs2" };
        
        await _service.AddConnectionAsync(connection1);
        await Task.Delay(100); // Ensure different timestamps
        var added2 = await _service.AddConnectionAsync(connection2);

        // Act
        var result = await _service.GetConnectionsAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(added2.Id); // Most recently added should be first
    }

    /// <summary>
    /// Tests that GetConnectionAsync returns correct connection by ID.
    /// </summary>
    [Fact]
    public async Task GetConnectionAsync_WithValidId_ReturnsConnection()
    {
        // Arrange
        var connection = new DatabaseConnectionInfo { Name = "Test", Server = "server", Database = "db", ConnectionString = "cs" };
        var added = await _service.AddConnectionAsync(connection);

        // Act
        var result = await _service.GetConnectionAsync(added.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(added.Id);
        result.Name.Should().Be("Test");
    }

    /// <summary>
    /// Tests that GetConnectionAsync returns null for invalid ID.
    /// </summary>
    [Fact]
    public async Task GetConnectionAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _service.GetConnectionAsync("non-existent-id");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that UpdateConnectionAsync updates existing connection.
    /// </summary>
    [Fact]
    public async Task UpdateConnectionAsync_UpdatesExistingConnection()
    {
        // Arrange
        var connection = new DatabaseConnectionInfo { Name = "Original", Server = "server", Database = "db", ConnectionString = "cs" };
        var added = await _service.AddConnectionAsync(connection);

        // Modify the connection
        added.Name = "Updated";
        added.Server = "newserver";

        // Act
        await _service.UpdateConnectionAsync(added);
        var result = await _service.GetConnectionAsync(added.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated");
        result.Server.Should().Be("newserver");
    }

    /// <summary>
    /// Tests that DeleteConnectionAsync removes connection.
    /// </summary>
    [Fact]
    public async Task DeleteConnectionAsync_RemovesConnection()
    {
        // Arrange
        var connection = new DatabaseConnectionInfo { Name = "ToDelete", Server = "server", Database = "db", ConnectionString = "cs" };
        var added = await _service.AddConnectionAsync(connection);

        // Act
        await _service.DeleteConnectionAsync(added.Id);
        var result = await _service.GetConnectionAsync(added.Id);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that UpdateLastUsedAsync updates timestamp.
    /// </summary>
    [Fact]
    public async Task UpdateLastUsedAsync_UpdatesTimestamp()
    {
        // Arrange
        var connection = new DatabaseConnectionInfo { Name = "Test", Server = "server", Database = "db", ConnectionString = "cs" };
        var added = await _service.AddConnectionAsync(connection);
        var originalLastUsed = added.LastUsed;
        
        await Task.Delay(100); // Ensure different timestamp

        // Act
        await _service.UpdateLastUsedAsync(added.Id);
        var result = await _service.GetConnectionAsync(added.Id);

        // Assert
        result.Should().NotBeNull();
        result!.LastUsed.Should().BeAfter(originalLastUsed);
    }

    /// <summary>
    /// Tests that TestConnectionAsync returns false for invalid connection string.
    /// </summary>
    [Fact]
    public async Task TestConnectionAsync_WithInvalidConnectionString_ReturnsFalse()
    {
        // Arrange
        var invalidConnectionString = "Invalid Connection String";

        // Act
        var result = await _service.TestConnectionAsync(invalidConnectionString);

        // Assert
        result.Should().BeFalse();
    }
}
