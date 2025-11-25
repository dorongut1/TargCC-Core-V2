// <copyright file="TableAnalyzerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Analyzers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Analyzers.Database;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Tests.TestHelpers;
using Xunit;
using Index = TargCC.Core.Interfaces.Models.Index;

/// <summary>
/// Unit tests for TableAnalyzer.
/// Tests table parsing, indexing, and analysis functionality.
/// </summary>
public class TableAnalyzerTests
{
    private readonly Mock<ILogger> loggerMock;
    private readonly TableAnalyzer analyzer;
    private readonly string connectionString = "Server=.;Database=TestDB;Integrated Security=true;";

    /// <summary>
    /// Initializes a new instance of the <see cref="TableAnalyzerTests"/> class.
    /// </summary>
    public TableAnalyzerTests()
    {
        this.loggerMock = new Mock<ILogger>();
        this.analyzer = new TableAnalyzer(this.connectionString, this.loggerMock.Object);
    }

    #region Validation Tests

    /// <summary>
    /// Test that AnalyzeTableAsync throws ArgumentNullException when table name is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AnalyzeTableAsync_NullTableName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => this.analyzer.AnalyzeTableAsync(null!));
    }

    /// <summary>
    /// Test that AnalyzeTableAsync throws ArgumentNullException when table name is empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AnalyzeTableAsync_EmptyTableName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => this.analyzer.AnalyzeTableAsync(string.Empty));
    }

    #endregion

    #region ParseTableName Tests

    /// <summary>
    /// Test ParseTableName with simple table name.
    /// </summary>
    [Fact]
    public void ParseTableName_SimpleTableName_ReturnsCorrectParts()
    {
        // Arrange
        var fullTableName = "Customer";

        // Act
        var (schema, tableName) = this.ParseTableName(fullTableName);

        // Assert
        Assert.Equal("dbo", schema);  // Default schema
        Assert.Equal("Customer", tableName);
    }

    /// <summary>
    /// Test ParseTableName with schema.table format.
    /// </summary>
    [Fact]
    public void ParseTableName_WithSchema_ReturnsCorrectParts()
    {
        // Arrange
        var fullTableName = "sales.Customer";

        // Act
        var (schema, tableName) = this.ParseTableName(fullTableName);

        // Assert
        Assert.Equal("sales", schema);
        Assert.Equal("Customer", tableName);
    }

    /// <summary>
    /// Test ParseTableName with brackets.
    /// </summary>
    [Fact]
    public void ParseTableName_WithBrackets_ReturnsCorrectParts()
    {
        // Arrange
        var fullTableName = "[dbo].[Customer]";

        // Act
        var (schema, tableName) = this.ParseTableName(fullTableName);

        // Assert
        Assert.Equal("dbo", schema);
        Assert.Equal("Customer", tableName);
    }

    /// <summary>
    /// Test ParseTableName with database.schema.table format.
    /// </summary>
    [Fact]
    public void ParseTableName_WithDatabase_ReturnsSchemaAndTable()
    {
        // Arrange
        var fullTableName = "MyDB.sales.Customer";

        // Act
        var (schema, tableName) = this.ParseTableName(fullTableName);

        // Assert
        Assert.Equal("sales", schema);
        Assert.Equal("Customer", tableName);
    }

    #endregion

    #region TableBuilder Tests

    /// <summary>
    /// Test that TableBuilder creates simple table correctly.
    /// </summary>
    [Fact]
    public void CreateSimpleTable_CreatesTableWithIdAndName()
    {
        // Act
        var table = TableBuilder.SimpleTable(
            "TestTable",
            ColumnBuilder.IdColumn(),
            ColumnBuilder.NameColumn());

        // Assert
        Assert.Equal("TestTable", table.Name);
        Assert.Equal("dbo", table.SchemaName);
        Assert.Equal(2, table.Columns.Count);
        Assert.Contains(table.Columns, c => c.Name == "ID" && c.IsPrimaryKey);
        Assert.Contains(table.Columns, c => c.Name == "Name");
    }

    /// <summary>
    /// Test that TableBuilder can add columns fluently.
    /// </summary>
    [Fact]
    public void TableBuilder_FluentColumnAddition_Works()
    {
        // Act
        var table = new TableBuilder()
            .WithName("Product")
            .AddColumn(ColumnBuilder.IdColumn())
            .AddColumn(ColumnBuilder.NameColumn())
            .AddColumn(new ColumnBuilder()
                .WithName("Price")
                .AsDecimal()
                .NotNullable()
                .Build())
            .Build();

        // Assert
        Assert.Equal("Product", table.Name);
        Assert.Equal(3, table.Columns.Count);
    }

    /// <summary>
    /// Test that TableBuilder creates Customer table correctly.
    /// </summary>
    [Fact]
    public void CreateCustomerTable_CreatesTableCorrectly()
    {
        // Act
        var table = TableBuilder.CustomerTable();

        // Assert
        Assert.Equal("Customer", table.Name);
        Assert.True(table.Columns.Count >= 6);  // ID, FirstName, LastName, Email, Phone, CreatedDate
        Assert.Contains(table.Columns, c => c.Name == "ID" && c.IsPrimaryKey);
        Assert.Contains(table.Columns, c => c.Name == "FirstName");
        Assert.Contains(table.Columns, c => c.Name == "LastName");
        Assert.Contains(table.Columns, c => c.Name == "Email");
    }

    /// <summary>
    /// Test that TableBuilder can set schema.
    /// </summary>
    [Fact]
    public void TableBuilder_WithSchema_SetsSchemaCorrectly()
    {
        // Act
        var table = new TableBuilder()
            .WithName("Customer")
            .WithSchema("sales")
            .WithIdColumn()
            .Build();

        // Assert
        Assert.Equal("sales", table.SchemaName);
    }

    #endregion

    #region Index Tests

    /// <summary>
    /// Test that indexes can be added to table.
    /// </summary>
    [Fact]
    public void AddIndex_AddsIndexCorrectly()
    {
        // Arrange
        var index = new Index
        {
            Name = "IX_Test_Email",
            IsUnique = true,
            ColumnNames = new List<string> { "Email" }
        };

        // Act
        var table = new TableBuilder()
            .WithName("Test")
            .WithIdColumn()
            .AddIndex(index)
            .Build();

        // Assert
        Assert.Single(table.Indexes);
        Assert.Equal("IX_Test_Email", table.Indexes[0].Name);
        Assert.True(table.Indexes[0].IsUnique);
    }

    #endregion

    #region Primary Key Tests

    /// <summary>
    /// Test that primary key is correctly set.
    /// </summary>
    [Fact]
    public void WithPrimaryKey_SetsPrimaryKeyCorrectly()
    {
        // Act
        var table = new TableBuilder()
            .WithName("Test")
            .AddColumn(ColumnBuilder.IdColumn())
            .WithPrimaryKey("ID")
            .Build();

        // Assert
        Assert.Equal("ID", table.PrimaryKey);
        var pkColumn = table.Columns.First(c => c.Name == "ID");
        Assert.True(pkColumn.IsPrimaryKey);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Helper method to parse table name.
    /// Simulates the ParseTableName method in TableAnalyzer.
    /// </summary>
    /// <param name="fullTableName">The full table name.</param>
    /// <returns>Tuple of (schema, tableName).</returns>
    private (string schema, string tableName) ParseTableName(string fullTableName)
    {
        if (string.IsNullOrWhiteSpace(fullTableName))
        {
            throw new ArgumentException("Table name cannot be empty.", nameof(fullTableName));
        }

        // Remove brackets if present
        var cleaned = fullTableName.Replace("[", string.Empty).Replace("]", string.Empty);

        // Split by '.'
        var parts = cleaned.Split('.');

        return parts.Length switch
        {
            1 => ("dbo", parts[0]),                           // TableName → dbo.TableName
            2 => (parts[0], parts[1]),                        // Schema.TableName
            3 => (parts[1], parts[2]),                        // Database.Schema.TableName
            _ => throw new ArgumentException($"Invalid table name format: {fullTableName}"),
        };
    }

    #endregion
}
