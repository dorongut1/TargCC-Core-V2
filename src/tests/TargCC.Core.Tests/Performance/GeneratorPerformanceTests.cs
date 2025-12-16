// <copyright file="GeneratorPerformanceTests.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Generators.Data;
using TargCC.Core.Generators.DI;
using TargCC.Core.Generators.Repositories;
using TargCC.Core.Tests.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace TargCC.Core.Tests.Performance;

/// <summary>
/// Performance tests for generators.
/// Ensures generators complete within acceptable time limits.
/// </summary>
public class GeneratorPerformanceTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ILogger<RepositoryInterfaceGenerator>> _repoInterfaceLoggerMock;
    private readonly Mock<ILogger<RepositoryGenerator>> _repoLoggerMock;
    private readonly Mock<ILogger<DbContextGenerator>> _dbContextLoggerMock;
    private readonly Mock<ILogger<EntityConfigurationGenerator>> _entityConfigLoggerMock;
    private readonly Mock<ILogger<DIRegistrationGenerator>> _diLoggerMock;

    public GeneratorPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
        _repoInterfaceLoggerMock = new Mock<ILogger<RepositoryInterfaceGenerator>>();
        _repoLoggerMock = new Mock<ILogger<RepositoryGenerator>>();
        _dbContextLoggerMock = new Mock<ILogger<DbContextGenerator>>();
        _entityConfigLoggerMock = new Mock<ILogger<EntityConfigurationGenerator>>();
        _diLoggerMock = new Mock<ILogger<DIRegistrationGenerator>>();
    }

    [Fact]
    public async Task RepositoryInterfaceGenerator_SimpleTable_CompletesUnderOneSecond()
    {
        // Arrange
        var generator = new RepositoryInterfaceGenerator(_repoInterfaceLoggerMock.Object);
        var table = TableBuilder.CreateSimpleTable("Customer");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await generator.GenerateAsync(table);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNullOrEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "Generation should complete in under 1 second");
        _output.WriteLine($"RepositoryInterface generation: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task RepositoryGenerator_SimpleTable_CompletesUnderOneSecond()
    {
        // Arrange
        var generator = new RepositoryGenerator(_repoLoggerMock.Object);
        var table = TableBuilder.CreateSimpleTable("Customer");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await generator.GenerateAsync(table);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNullOrEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "Generation should complete in under 1 second");
        _output.WriteLine($"Repository generation: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task DbContextGenerator_SmallSchema_CompletesUnderOneSecond()
    {
        // Arrange
        var generator = new DbContextGenerator(_dbContextLoggerMock.Object);
        var schema = new DatabaseSchemaBuilder()
            .WithTable(TableBuilder.CreateSimpleTable("Customer"))
            .WithTable(TableBuilder.CreateSimpleTable("Order"))
            .WithTable(TableBuilder.CreateSimpleTable("Product"))
            .Build();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await generator.GenerateAsync(schema, "TestNamespace");
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNullOrEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "Generation should complete in under 1 second");
        _output.WriteLine($"DbContext generation: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task EntityConfigurationGenerator_SimpleTable_CompletesUnderOneSecond()
    {
        // Arrange
        var generator = new EntityConfigurationGenerator(_entityConfigLoggerMock.Object);
        var table = TableBuilder.CreateSimpleTable("Customer");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await generator.GenerateAsync(table);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNullOrEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "Generation should complete in under 1 second");
        _output.WriteLine($"EntityConfiguration generation: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task DIRegistrationGenerator_SmallSchema_CompletesUnderOneSecond()
    {
        // Arrange
        var generator = new DIRegistrationGenerator(_diLoggerMock.Object);
        var schema = new DatabaseSchemaBuilder()
            .WithTable(TableBuilder.CreateSimpleTable("Customer"))
            .WithTable(TableBuilder.CreateSimpleTable("Order"))
            .WithTable(TableBuilder.CreateSimpleTable("Product"))
            .Build();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await generator.GenerateAsync(schema);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNullOrEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "Generation should complete in under 1 second");
        _output.WriteLine($"DI Registration generation: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task CompleteGeneration_SmallSchema_CompletesUnderFiveSeconds()
    {
        // Arrange
        var schema = new DatabaseSchemaBuilder()
            .WithTable(TableBuilder.CreateSimpleTable("Customer"))
            .WithTable(TableBuilder.CreateSimpleTable("Order"))
            .WithTable(TableBuilder.CreateSimpleTable("Product"))
            .Build();

        var repoInterfaceGen = new RepositoryInterfaceGenerator(_repoInterfaceLoggerMock.Object);
        var repoGen = new RepositoryGenerator(_repoLoggerMock.Object);
        var dbContextGen = new DbContextGenerator(_dbContextLoggerMock.Object);
        var entityConfigGen = new EntityConfigurationGenerator(_entityConfigLoggerMock.Object);
        var diGen = new DIRegistrationGenerator(_diLoggerMock.Object);

        var stopwatch = Stopwatch.StartNew();

        // Act: Generate everything
        foreach (var table in schema.Tables)
        {
            await repoInterfaceGen.GenerateAsync(table);
            await repoGen.GenerateAsync(table);
            await entityConfigGen.GenerateAsync(table);
        }
        await dbContextGen.GenerateAsync(schema, "TestNamespace");
        await diGen.GenerateAsync(schema);

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "Complete generation should finish in under 5 seconds");
        _output.WriteLine($"Complete generation for 3 tables: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task CompleteGeneration_MediumSchema_CompletesUnderTenSeconds()
    {
        // Arrange
        var schemaBuilder = new DatabaseSchemaBuilder();
        for (int i = 1; i <= 10; i++)
        {
            schemaBuilder.WithTable(TableBuilder.CreateSimpleTable($"Table{i}"));
        }
        var schema = schemaBuilder.Build();

        var repoInterfaceGen = new RepositoryInterfaceGenerator(_repoInterfaceLoggerMock.Object);
        var repoGen = new RepositoryGenerator(_repoLoggerMock.Object);
        var entityConfigGen = new EntityConfigurationGenerator(_entityConfigLoggerMock.Object);

        var stopwatch = Stopwatch.StartNew();

        // Act: Generate for all tables
        foreach (var table in schema.Tables)
        {
            await repoInterfaceGen.GenerateAsync(table);
            await repoGen.GenerateAsync(table);
            await entityConfigGen.GenerateAsync(table);
        }

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, "Generation for 10 tables should finish in under 10 seconds");
        _output.WriteLine($"Complete generation for 10 tables: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task RepositoryGenerator_ComplexTable_CompletesUnderTwoSeconds()
    {
        // Arrange
        var generator = new RepositoryGenerator(_repoLoggerMock.Object);
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("Name", "nvarchar", maxLength: 100, isNullable: false)
            .WithColumn("Email", "nvarchar", maxLength: 100, isNullable: false)
            .WithColumn("Phone", "nvarchar", maxLength: 20)
            .WithColumn("ent_CreditCard", "nvarchar", maxLength: 500)
            .WithColumn("eno_Password", "varchar", maxLength: 64)
            .WithColumn("agg_OrderCount", "int")
            .WithColumn("agg_TotalSpent", "decimal", precision: 18, scale: 2)
            .WithPrimaryKey("ID")
            .WithIndex("IX_Customer_Email", true, new[] { "Email" })
            .WithIndex("IX_Customer_Phone", false, new[] { "Phone" })
            .Build();

        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await generator.GenerateAsync(table);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNullOrEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "Complex table generation should complete in under 2 seconds");
        _output.WriteLine($"Complex table generation: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task ParallelGeneration_MultipleTables_CompletesEfficiently()
    {
        // Arrange
        var tables = new[]
        {
            TableBuilder.CreateSimpleTable("Customer"),
            TableBuilder.CreateSimpleTable("Order"),
            TableBuilder.CreateSimpleTable("Product"),
            TableBuilder.CreateSimpleTable("Category"),
            TableBuilder.CreateSimpleTable("Supplier")
        };

        var repoGen = new RepositoryGenerator(_repoLoggerMock.Object);
        var stopwatch = Stopwatch.StartNew();

        // Act: Generate in parallel
        var tasks = tables.Select(table => repoGen.GenerateAsync(table)).ToArray();
        await Task.WhenAll(tasks);

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000, "Parallel generation should be efficient");
        _output.WriteLine($"Parallel generation for 5 tables: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task MemoryUsage_LargeSchema_StaysReasonable()
    {
        // Arrange
        var schemaBuilder = new DatabaseSchemaBuilder();
        for (int i = 1; i <= 50; i++)
        {
            schemaBuilder.WithTable(TableBuilder.CreateSimpleTable($"Table{i}"));
        }
        var schema = schemaBuilder.Build();

        var generator = new DbContextGenerator(_dbContextLoggerMock.Object);

        // Measure memory before
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var memoryBefore = GC.GetTotalMemory(false);

        // Act
        var result = await generator.GenerateAsync(schema);

        // Measure memory after
        var memoryAfter = GC.GetTotalMemory(false);
        var memoryUsed = memoryAfter - memoryBefore;

        // Assert
        memoryUsed.Should().BeLessThan(10_000_000, "Memory usage should stay under 10MB");
        _output.WriteLine($"Memory used for 50 tables: {memoryUsed / 1024.0 / 1024.0:F2} MB");
    }
}
