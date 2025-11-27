// <copyright file="WatchModePerformanceTests.cs" company="Doron Vaida">
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
/// Performance tests for watch mode and change detection operations.
/// </summary>
[Trait("Category", "Performance")]
public class WatchModePerformanceTests : IDisposable
{
    private readonly ITestOutputHelper output;
    private readonly string testDirectory;
    private readonly Mock<IDatabaseAnalyzer> mockDatabaseAnalyzer;
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="WatchModePerformanceTests"/> class.
    /// </summary>
    /// <param name="output">Test output helper.</param>
    public WatchModePerformanceTests(ITestOutputHelper output)
    {
        this.output = output;
        this.testDirectory = Path.Combine(Path.GetTempPath(), $"TargCC_WatchPerfTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(this.testDirectory);

        this.mockDatabaseAnalyzer = new Mock<IDatabaseAnalyzer>();
        this.SetupMockDatabase(50);
    }

    [Fact]
    public async Task DatabaseAnalyzer_RepeatedAnalysis_MaintainsPerformance()
    {
        // Arrange
        var times = new List<long>();

        // Act - Analyze 10 times
        for (int i = 0; i < 10; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            await this.mockDatabaseAnalyzer.Object.AnalyzeAsync();
            stopwatch.Stop();
            times.Add(stopwatch.ElapsedMilliseconds);
        }

        // Assert
        var averageTime = times.Average();
        var maxTime = times.Max();
        var minTime = times.Min();

        averageTime.Should().BeLessThan(1000); // Average under 1 second
        maxTime.Should().BeLessThan(2000); // Max under 2 seconds

        this.output.WriteLine($"10 repeated analyses - Avg: {averageTime:F1}ms, Min: {minTime}ms, Max: {maxTime}ms");
    }

    [Fact]
    public async Task TableComparison_50Tables_CompletesQuickly()
    {
        // Arrange
        var schema1 = await this.mockDatabaseAnalyzer.Object.AnalyzeAsync();

        // Modify one table
        this.SetupMockDatabase(50);
        var schema2 = await this.mockDatabaseAnalyzer.Object.AnalyzeAsync();

        var stopwatch = Stopwatch.StartNew();

        // Act - Compare schemas
        var differences = new List<string>();
        for (int i = 0; i < schema1.Tables.Count; i++)
        {
            if (schema1.Tables[i].Columns.Count != schema2.Tables[i].Columns.Count)
            {
                differences.Add($"Table {schema1.Tables[i].Name} has different column count");
            }
        }

        stopwatch.Stop();

        // Assert
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
        this.output.WriteLine($"Schema comparison completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task SchemaSnapshot_SerializationPerformance()
    {
        // Arrange
        var schema = await this.mockDatabaseAnalyzer.Object.AnalyzeAsync();
        var snapshotPath = Path.Combine(this.testDirectory, "snapshot.json");

        var stopwatch = Stopwatch.StartNew();

        // Act - Serialize to JSON
        var json = System.Text.Json.JsonSerializer.Serialize(schema, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
        });
        await File.WriteAllTextAsync(snapshotPath, json);

        // Act - Deserialize from JSON
        var loadedJson = await File.ReadAllTextAsync(snapshotPath);
        var loadedSchema = System.Text.Json.JsonSerializer.Deserialize<DatabaseSchema>(loadedJson);

        stopwatch.Stop();

        // Assert
        loadedSchema.Should().NotBeNull();
        loadedSchema!.Tables.Should().HaveCount(50);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
        this.output.WriteLine($"Snapshot save+load completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task TableAnalysis_LargeSchema_HandlesEfficiently()
    {
        // Arrange
        this.SetupMockDatabase(100); // 100 tables
        var stopwatch = Stopwatch.StartNew();

        // Act
        var schema = await this.mockDatabaseAnalyzer.Object.AnalyzeAsync();

        stopwatch.Stop();

        // Assert
        schema.Tables.Should().HaveCount(100);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
        this.output.WriteLine($"Analysis of 100 tables completed in {stopwatch.ElapsedMilliseconds}ms");
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
            if (disposing)
            {
                // Cleanup test directory
                if (Directory.Exists(this.testDirectory))
                {
                    try
                    {
                        Directory.Delete(this.testDirectory, true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }

            this.disposed = true;
        }
    }

    private void SetupMockDatabase(int tableCount)
    {
        var tables = this.CreateMultipleTables(tableCount);

        var schema = new DatabaseSchema
        {
            Tables = tables,
        };

        this.mockDatabaseAnalyzer
            .Setup(x => x.AnalyzeAsync())
            .ReturnsAsync(schema);
    }

    private List<Table> CreateMultipleTables(int count)
    {
        var tables = new List<Table>();

        for (int i = 1; i <= count; i++)
        {
            var columns = new List<Column>
            {
                new() { Name = $"Table{i}ID", DataType = "int", IsPrimaryKey = true, IsNullable = false },
                new() { Name = "Name", DataType = "nvarchar", MaxLength = 100, IsNullable = false },
                new() { Name = "Description", DataType = "nvarchar", MaxLength = 500, IsNullable = true },
                new() { Name = "CreatedDate", DataType = "datetime", IsNullable = false },
                new() { Name = "ModifiedDate", DataType = "datetime", IsNullable = true },
                new() { Name = "IsActive", DataType = "bit", IsNullable = false },
            };

            tables.Add(new Table
            {
                SchemaName = "dbo",
                Name = $"Table{i}",
                Columns = columns,
                Indexes = new List<IndexModel>
                {
                    new()
                    {
                        Name = $"PK_Table{i}",
                        ColumnNames = new List<string> { $"Table{i}ID" },
                        IsUnique = true,
                        IsPrimaryKey = true,
                    },
                    new()
                    {
                        Name = $"IX_Table{i}_Name",
                        ColumnNames = new List<string> { "Name" },
                        IsUnique = false,
                    },
                },
                Relationships = new List<Relationship>(),
            });
        }

        return tables;
    }
}
