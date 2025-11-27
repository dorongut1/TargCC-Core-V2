// <copyright file="ConversationMessageTests.cs" company="TargCC Team">
// Copyright (c) TargCC Team. All rights reserved.
// </copyright>

namespace TargCC.AI.Tests.Models;

using FluentAssertions;
using TargCC.AI.Models;

/// <summary>
/// Tests for <see cref="ConversationMessage"/>.
/// </summary>
public sealed class ConversationMessageTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var role = "user";
        var content = "Hello, AI!";

        // Act
        var message = new ConversationMessage(role, content);

        // Assert
        message.Role.Should().Be(role);
        message.Content.Should().Be(content);
        message.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithNullRole_ShouldThrowArgumentException()
    {
        // Arrange
        var content = "Test content";

        // Act
        var act = () => new ConversationMessage(null!, content);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("role");
    }

    [Fact]
    public void Constructor_WithWhitespaceRole_ShouldThrowArgumentException()
    {
        // Arrange
        var content = "Test content";

        // Act
        var act = () => new ConversationMessage("   ", content);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("role");
    }

    [Fact]
    public void Constructor_WithNullContent_ShouldThrowArgumentException()
    {
        // Arrange
        var role = "user";

        // Act
        var act = () => new ConversationMessage(role, null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("content");
    }

    [Fact]
    public void Constructor_WithWhitespaceContent_ShouldThrowArgumentException()
    {
        // Arrange
        var role = "user";

        // Act
        var act = () => new ConversationMessage(role, "   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("content");
    }
}
