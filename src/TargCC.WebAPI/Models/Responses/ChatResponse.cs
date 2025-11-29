// <copyright file="ChatResponse.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.WebAPI.Models.Responses;

/// <summary>
/// Response model for AI chat operations.
/// </summary>
public sealed class ChatResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether chat was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the AI's response message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the updated conversation context.
    /// </summary>
    public List<ConversationMessage> Context { get; set; } = new();

    /// <summary>
    /// Gets or sets any errors that occurred.
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Represents a conversation message.
/// </summary>
public sealed class ConversationMessage
{
    /// <summary>
    /// Gets or sets the role (user or assistant).
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message content.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
