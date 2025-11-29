// <copyright file="ChatRequest.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.WebAPI.Models.Requests;

/// <summary>
/// Request model for AI chat operations.
/// </summary>
public sealed class ChatRequest
{
    /// <summary>
    /// Gets or sets the user's message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conversation context (optional).
    /// </summary>
    public List<ConversationMessage>? Context { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include schema context.
    /// </summary>
    public bool IncludeSchemaContext { get; set; }

    /// <summary>
    /// Gets or sets the table name for schema context (optional).
    /// </summary>
    public string? TableName { get; set; }
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
