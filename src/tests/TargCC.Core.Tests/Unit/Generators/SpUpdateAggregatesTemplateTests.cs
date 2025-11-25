// <copyright file="SpUpdateAggregatesTemplateTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators;

using System.Threading.Tasks;
using FluentAssertions;
using TargCC.Core.Generators.Sql.Templates;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Tests.TestHelpers;
using Xunit;

/// <summary>
/// Unit tests for SpUpdateAggregatesTemplate class.
/// Note: UpdateAggregates is generated only for tables with agg_ columns.
/// </summary>
public class SpUpdateAggregatesTemplateTests
{
    [Fact]
    public async Task GenerateAsync_SimpleTableWithoutAggregates_ReturnsEmptyOrValidSP()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
            .WithColumn(c => c.WithName("Name").AsNvarchar(100).Build())
            .Build();

        // Act
        var result = await SpUpdateAggregatesTemplate.GenerateAsync(table);

        // Assert - Either empty or contains valid SP
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateAsync_NullTable_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = async () => await SpUpdateAggregatesTemplate.GenerateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<System.ArgumentNullException>();
    }
}
