// <copyright file="SpDeleteTemplateTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators;

using System;
using System.Threading.Tasks;
using FluentAssertions;
using TargCC.Core.Generators.Sql.Templates;
using TargCC.Core.Tests.TestHelpers;
using Xunit;

/// <summary>
/// Unit tests for SpDeleteTemplate class.
/// Tests stored procedure generation for DELETE operations.
/// </summary>
public class SpDeleteTemplateTests
{
    [Fact]
    public async Task GenerateAsync_SimpleTable_CreatesDeleteStoredProcedure()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
            .WithColumn(c => c.WithName("Name").AsNvarchar(100).Build())
            .WithColumn(c => c.WithName("Email").AsNvarchar(100).Build())
            .Build();

        // Act
        var result = await SpDeleteTemplate.GenerateAsync(table);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("CREATE OR ALTER PROCEDURE");
        result.Should().Contain("SP_DeleteCustomer");
        result.Should().Contain("DELETE FROM");
        result.Should().Contain("[Customer]");
    }

    [Fact]
    public async Task GenerateAsync_WithPrimaryKey_IncludesPrimaryKeyParameter()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Order")
            .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
            .WithColumn(c => c.WithName("OrderDate").AsDateTime().Build())
            .Build();

        // Act
        var result = await SpDeleteTemplate.GenerateAsync(table);

        // Assert
        result.Should().Contain("@ID int");
        result.Should().Contain("WHERE [ID] = @ID");
    }

    [Fact]
    public async Task GenerateAsync_CompositePrimaryKey_IncludesAllKeyParameters()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("OrderDetail")
            .WithColumn(c => c.WithName("OrderID").AsInt().AsPrimaryKey().Build())
            .WithColumn(c => c.WithName("ProductID").AsInt().AsPrimaryKey().Build())
            .WithColumn(c => c.WithName("Quantity").AsInt().Build())
            .Build();

        // Act
        var result = await SpDeleteTemplate.GenerateAsync(table);

        // Assert
        result.Should().Contain("@OrderID int");
        result.Should().Contain("@ProductID int");
        result.Should().Contain("WHERE [OrderID] = @OrderID AND [ProductID] = @ProductID");
    }

    [Fact]
    public async Task GenerateAsync_NullTable_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Func<Task> act = async () => await SpDeleteTemplate.GenerateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
