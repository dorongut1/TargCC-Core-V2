// Copyright (c) TargCC Team. All rights reserved.

namespace TargCC.AI.Models;

/// <summary>
/// Represents the context of an ongoing conversation with the AI.
/// </summary>
public sealed class ConversationContext
{
    private readonly List<ConversationMessage> messages;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConversationContext"/> class.
    /// </summary>
    /// <param name="conversationId">Optional conversation ID.</param>
    public ConversationContext(string? conversationId = null)
    {
        this.ConversationId = conversationId ?? Guid.NewGuid().ToString();
        this.messages = new List<ConversationMessage>();
        this.CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the unique identifier for this conversation.
    /// </summary>
    public string ConversationId { get; }

    /// <summary>
    /// Gets the messages in this conversation.
    /// </summary>
    public IReadOnlyList<ConversationMessage> Messages => this.messages.AsReadOnly();

    /// <summary>
    /// Gets the timestamp when this conversation was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Gets the timestamp of the last message.
    /// </summary>
    public DateTime? LastMessageAt { get; private set; }

    /// <summary>
    /// Gets the total number of messages in this conversation.
    /// </summary>
    public int MessageCount => this.messages.Count;

    /// <summary>
    /// Gets a value indicating whether this conversation has any messages.
    /// </summary>
    public bool HasMessages => this.messages.Count > 0;

    /// <summary>
    /// Adds a user message to the conversation.
    /// </summary>
    /// <param name="content">The message content.</param>
    public void AddUserMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content cannot be null or whitespace.", nameof(content));
        }

        this.messages.Add(new ConversationMessage("user", content));
        this.LastMessageAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds an assistant (AI) message to the conversation.
    /// </summary>
    /// <param name="content">The message content.</param>
    public void AddAssistantMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content cannot be null or whitespace.", nameof(content));
        }

        this.messages.Add(new ConversationMessage("assistant", content));
        this.LastMessageAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a system message to the conversation.
    /// </summary>
    /// <param name="content">The message content.</param>
    public void AddSystemMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content cannot be null or whitespace.", nameof(content));
        }

        this.messages.Add(new ConversationMessage("system", content));
        this.LastMessageAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Clears all messages from the conversation.
    /// </summary>
    public void Clear()
    {
        this.messages.Clear();
        this.LastMessageAt = null;
    }
}
