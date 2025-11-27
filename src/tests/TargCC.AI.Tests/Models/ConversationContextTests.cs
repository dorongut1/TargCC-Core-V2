// <copyright file="ConversationContextTests.cs" company="TargCC Team">
// Copyright (c) TargCC Team. All rights reserved.
// </copyright>

namespace TargCC.AI.Tests.Models;

using FluentAssertions;
using TargCC.AI.Models;

/// <summary>
/// Tests for <see cref="ConversationContext"/>.
/// </summary>
public sealed class ConversationContextTests
{
    [Fact]
    public void Constructor_WithNoParameters_ShouldCreateContextWithGuid()
    {
        // Act
        var context = new ConversationContext();

        // Assert
        context.ConversationId.Should().NotBeNullOrEmpty();
        context.Messages.Should().BeEmpty();
        context.MessageCount.Should().Be(0);
        context.HasMessages.Should().BeFalse();
        context.LastMessageAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithConversationId_ShouldUseProvidedId()
    {
        // Arrange
        var expectedId = "test-conversation-123";

        // Act
        var context = new ConversationContext(expectedId);

        // Assert
        context.ConversationId.Should().Be(expectedId);
    }

    [Fact]
    public void AddUserMessage_WithValidContent_ShouldAddMessageAndUpdateTimestamp()
    {
        // Arrange
        var context = new ConversationContext();
        var content = "Hello, AI!";

        // Act
        context.AddUserMessage(content);

        // Assert
        context.Messages.Should().HaveCount(1);
        context.MessageCount.Should().Be(1);
        context.HasMessages.Should().BeTrue();
        context.Messages[0].Role.Should().Be("user");
        context.Messages[0].Content.Should().Be(content);
        context.LastMessageAt.Should().NotBeNull();
    }

    [Fact]
    public void AddAssistantMessage_WithValidContent_ShouldAddMessageAndUpdateTimestamp()
    {
        // Arrange
        var context = new ConversationContext();
        var content = "Hello, human!";

        // Act
        context.AddAssistantMessage(content);

        // Assert
        context.Messages.Should().HaveCount(1);
        context.MessageCount.Should().Be(1);
        context.HasMessages.Should().BeTrue();
        context.Messages[0].Role.Should().Be("assistant");
        context.Messages[0].Content.Should().Be(content);
        context.LastMessageAt.Should().NotBeNull();
    }

    [Fact]
    public void AddSystemMessage_WithValidContent_ShouldAddMessageAndUpdateTimestamp()
    {
        // Arrange
        var context = new ConversationContext();
        var content = "System initialization complete.";

        // Act
        context.AddSystemMessage(content);

        // Assert
        context.Messages.Should().HaveCount(1);
        context.MessageCount.Should().Be(1);
        context.HasMessages.Should().BeTrue();
        context.Messages[0].Role.Should().Be("system");
        context.Messages[0].Content.Should().Be(content);
        context.LastMessageAt.Should().NotBeNull();
    }

    [Fact]
    public void AddUserMessage_WithNullContent_ShouldThrowArgumentException()
    {
        // Arrange
        var context = new ConversationContext();

        // Act
        var act = () => context.AddUserMessage(null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("content");
    }

    [Fact]
    public void AddUserMessage_WithWhitespaceContent_ShouldThrowArgumentException()
    {
        // Arrange
        var context = new ConversationContext();

        // Act
        var act = () => context.AddUserMessage("   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("content");
    }

    [Fact]
    public void Clear_WithMessages_ShouldRemoveAllMessagesAndResetTimestamp()
    {
        // Arrange
        var context = new ConversationContext();
        context.AddUserMessage("Message 1");
        context.AddAssistantMessage("Message 2");
        context.AddUserMessage("Message 3");

        // Act
        context.Clear();

        // Assert
        context.Messages.Should().BeEmpty();
        context.MessageCount.Should().Be(0);
        context.HasMessages.Should().BeFalse();
        context.LastMessageAt.Should().BeNull();
    }

    [Fact]
    public void Messages_ShouldBeReadOnly()
    {
        // Arrange
        var context = new ConversationContext();
        context.AddUserMessage("Test message");

        // Act & Assert
        context.Messages.Should().BeAssignableTo<IReadOnlyList<ConversationMessage>>();
    }
}
