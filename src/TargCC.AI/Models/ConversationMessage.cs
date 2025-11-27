// Copyright (c) TargCC Team. All rights reserved.

namespace TargCC.AI.Models;

/// <summary>
/// Represents a single message in a conversation.
/// </summary>
public sealed class ConversationMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConversationMessage"/> class.
    /// </summary>
    /// <param name="role">The role of the message sender (user, assistant, system).</param>
    /// <param name="content">The message content.</param>
    public ConversationMessage(string role, string content)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role cannot be null or whitespace.", nameof(role));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Content cannot be null or whitespace.", nameof(content));
        }

        this.Role = role;
        this.Content = content;
        this.Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the role of the message sender.
    /// Typical values: "user", "assistant", "system".
    /// </summary>
    public string Role { get; }

    /// <summary>
    /// Gets the message content.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Gets the timestamp when this message was created.
    /// </summary>
    public DateTime Timestamp { get; }
}
