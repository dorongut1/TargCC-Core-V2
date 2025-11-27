// <copyright file="SchemaAnalysisPerformanceTests.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using System.Diagnostics;
using FluentAssertions;
using Moq;
using TargCC.Core.Interfaces;
using TargCC.Core.Interfaces.Models;
using Xunit;
using Xunit.Abstractions;
using IndexModel = TargCC.Core.Interfaces.Models.Index;

namespace TargCC.CLI.Tests.Performance;

/// <summary>
/// Performance tests for schema analysis operations.
/// </summary>
[Trait("Category", "Performance")]
public class SchemaAnalysisPerformanceTests : IDisposable
{
    private readonly ITestOutputHelper output;
    private readonly Mock<IDatabaseAnalyzer> mockDatabaseAnalyzer;
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaAnalysisPerformanceTests"/> class.
    /// </summary>
    /// <param name="output">Test output helper.</param>
    public SchemaAnalysisPerformanceTests(ITestOutputHelper output)
    {
        this.output = output;
        this.mockDatabaseAnalyzer = new Mock<IDatabaseAnalyzer>();
    }

    [Fact]
    public async Task DatabaseAnalyzer_With50Tables_CompletesUnder5Seconds()
    {
        // Arrange
        this.SetupLargeSchemaDatabase(50);
        var stopwatch = Stopwatch.StartNew();

        // Act
        var schema = await this.mockDatabaseAnalyzer.Object.AnalyzeAsync();

        stopwatch.Stop();

        // Assert
        schema.Tables.Should().HaveCount(50);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
        this.output.WriteLine($"Analysis of 50 tables completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task DatabaseAnalyzer_WithComplexRelationships_CompletesUnder3Seconds()
    {
        // Arrange
        this.SetupComplexRelationshipsDatabase();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var schema = await this.mockDatabaseAnalyzer.Object.AnalyzeAsync();

        stopwatch.Stop();

        // Assert
        schema.Tables.Should().HaveCountGreaterThan(0);
        var totalRelationships = schema.Tables.Sum(t => t.Relationships.Count);
        totalRelationships.Should().BeGreaterThan(10);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
        this.output.WriteLine($"Complex analysis with {totalRelationships} relationships completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task DatabaseAnalyzer_WithWideTable100Columns_HandlesEfficiently()
    {
        // Arrange
        this.SetupWideTableDatabase(100);
        var stopwatch = Stopwatch.StartNew();

        // Act
        var schema = await this.mockDatabaseAnalyzer.Object.AnalyzeAsync();

        stopwatch.Stop();

        // Assert
        var wideTable = schema.Tables.First();
        wideTable.Columns.Should().HaveCount(100);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
        this.output.WriteLine($"Wide table with 100 columns analyzed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task DatabaseAnalyzer_MultipleSequentialCalls_MaintainsPerformance()
    {
        // Arrange
        this.SetupLargeSchemaDatabase(20);
        var times = new List<long>();

        // Act - Call 5 times
        for (int i = 0; i < 5; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            await this.mockDatabaseAnalyzer.Object.AnalyzeAsync();
            stopwatch.Stop();
            times.Add(stopwatch.ElapsedMilliseconds);
        }

        // Assert
        var averageTime = times.Average();
        var maxTime = times.Max();

        averageTime.Should().BeLessThan(1000); // Average under 1 second
        maxTime.Should().BeLessThan(2000); // Max under 2 seconds
        this.output.WriteLine($"5 sequential calls - Avg: {averageTime}ms, Max: {maxTime}ms");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    /// <param name="disposing">Whether disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            this.disposed = true;
        }
    }

    private void SetupLargeSchemaDatabase(int tableCount)
    {
        var tables = new List<Table>();

        // Create tables with realistic structure
        for (int i = 1; i <= tableCount; i++)
        {
            var columns = new List<Column>
            {
                new() { Name = $"Table{i}ID", DataType = "int", IsPrimaryKey = true, IsNullable = false },
                new() { Name = "Name", DataType = "nvarchar", MaxLength = 100, IsNullable = false },
                new() { Name = "Description", DataType = "nvarchar", MaxLength = 500, IsNullable = true },
                new() { Name = "CreatedDate", DataType = "datetime", IsNullable = false },
                new() { Name = "ModifiedDate", DataType = "datetime", IsNullable = true },
            };

            tables.Add(new Table
            {
                SchemaName = "dbo",
                Name = $"Table{i}",
                Columns = columns,
                Indexes = new List<IndexModel>
                {
                    new() { Name = $"PK_Table{i}", ColumnNames = new List<string> { $"Table{i}ID" }, IsUnique = true, IsPrimaryKey = true },
                    new() { Name = $"IX_Table{i}_Name", ColumnNames = new List<string> { "Name" }, IsUnique = false },
                },
                Relationships = new List<Relationship>(),
            });
        }

        var schema = new DatabaseSchema
        {
            Tables = tables,
        };

        this.mockDatabaseAnalyzer
            .Setup(x => x.AnalyzeAsync())
            .ReturnsAsync(schema);
    }

    private void SetupComplexRelationshipsDatabase()
    {
        var tables = new List<Table>();

        // Create Customer table
        var customerTable = new Table
        {
            SchemaName = "dbo",
            Name = "Customer",
            Columns = new List<Column>
            {
                new() { Name = "CustomerID", DataType = "int", IsPrimaryKey = true, IsNullable = false },
                new() { Name = "Name", DataType = "nvarchar", MaxLength = 100, IsNullable = false },
            },
            Indexes = new List<IndexModel>
            {
                new() { Name = "PK_Customer", ColumnNames = new List<string> { "CustomerID" }, IsUnique = true, IsPrimaryKey = true },
            },
            Relationships = new List<Relationship>(),
        };

        // Create Order table with FK to Customer
        var orderTable = new Table
        {
            SchemaName = "dbo",
            Name = "Order",
            Columns = new List<Column>
            {
                new() { Name = "OrderID", DataType = "int", IsPrimaryKey = true, IsNullable = false },
                new() { Name = "CustomerID", DataType = "int", IsForeignKey = true, IsNullable = false },
            },
            Indexes = new List<IndexModel>
            {
                new() { Name = "PK_Order", ColumnNames = new List<string> { "OrderID" }, IsUnique = true, IsPrimaryKey = true },
            },
            Relationships = new List<Relationship>
            {
                new()
                {
                    Name = "FK_Order_Customer",
                    ChildTable = "Order",
                    ChildColumn = "CustomerID",
                    ParentTable = "Customer",
                    ParentColumn = "CustomerID",
                },
            },
        };

        tables.Add(customerTable);
        tables.Add(orderTable);

        // Add 18 more interconnected tables
        for (int i = 3; i <= 20; i++)
        {
            tables.Add(new Table
            {
                SchemaName = "dbo",
                Name = $"Table{i}",
                Columns = new List<Column>
                {
                    new() { Name = $"Table{i}ID", DataType = "int", IsPrimaryKey = true, IsNullable = false },
                    new() { Name = "CustomerID", DataType = "int", IsForeignKey = true, IsNullable = false },
                },
                Indexes = new List<IndexModel>
                {
                    new() { Name = $"PK_Table{i}", ColumnNames = new List<string> { $"Table{i}ID" }, IsUnique = true, IsPrimaryKey = true },
                },
                Relationships = new List<Relationship>
                {
                    new()
                    {
                        Name = $"FK_Table{i}_Customer",
                        ChildTable = $"Table{i}",
                        ChildColumn = "CustomerID",
                        ParentTable = "Customer",
                        ParentColumn = "CustomerID",
                    },
                },
            });
        }

        var schema = new DatabaseSchema
        {
            Tables = tables,
        };

        this.mockDatabaseAnalyzer
            .Setup(x => x.AnalyzeAsync())
            .ReturnsAsync(schema);
    }

    private void SetupWideTableDatabase(int columnCount)
    {
        var columns = new List<Column>
        {
            new() { Name = "ID", DataType = "int", IsPrimaryKey = true, IsNullable = false },
        };

        // Create many columns
        for (int i = 1; i < columnCount; i++)
        {
            columns.Add(new Column
            {
                Name = $"Column{i}",
                DataType = i % 2 == 0 ? "nvarchar" : "int",
                MaxLength = i % 2 == 0 ? 100 : null,
                IsNullable = i % 3 == 0,
            });
        }

        var wideTable = new Table
        {
            SchemaName = "dbo",
            Name = "WideTable",
            Columns = columns,
            Indexes = new List<IndexModel>
            {
                new() { Name = "PK_WideTable", ColumnNames = new List<string> { "ID" }, IsUnique = true, IsPrimaryKey = true },
            },
            Relationships = new List<Relationship>(),
        };

        var schema = new DatabaseSchema
        {
            Tables = new List<Table> { wideTable },
        };

        this.mockDatabaseAnalyzer
            .Setup(x => x.AnalyzeAsync())
            .ReturnsAsync(schema);
    }
}
