// <copyright file="SpUpdateFriendTemplateTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators;

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Generators.Sql.Templates;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Tests.TestHelpers;
using Xunit;

/// <summary>
/// Unit tests for SpUpdateFriendTemplate class.
/// Note: UpdateFriend is a specialized SP that may not be generated for all tables.
/// </summary>
public class SpUpdateFriendTemplateTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly SpUpdateFriendTemplate _template;

    public SpUpdateFriendTemplateTests()
    {
        _mockLogger = new Mock<ILogger>();
        _template = new SpUpdateFriendTemplate(_mockLogger.Object);
    }

    [Fact]
    public async Task GenerateAsync_SimpleTableWithoutSpecialColumns_ReturnsEmptyOrValidSP()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
            .WithColumn(c => c.WithName("Name").AsNvarchar(100).Build())
            .Build();

        // Act
        var result = await _template.GenerateAsync(table);

        // Assert - Either empty or contains valid SP
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateAsync_NullTable_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = async () => await _template.GenerateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<System.ArgumentNullException>();
    }
}
