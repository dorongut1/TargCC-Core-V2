// <copyright file="CodeModificationRequest.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.WebAPI.Models.Requests;

/// <summary>
/// Request model for AI-powered code modification.
/// </summary>
public sealed class CodeModificationRequest
{
    /// <summary>
    /// Gets or sets the original code to modify.
    /// </summary>
    public string OriginalCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the natural language instruction for modification.
    /// </summary>
    public string Instruction { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the table name for context.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema name for context (default: dbo).
    /// </summary>
    public string Schema { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets the list of related table names for FK context.
    /// </summary>
    public List<string>? RelatedTables { get; set; }

    /// <summary>
    /// Gets or sets the conversation ID for tracking (optional).
    /// </summary>
    public string? ConversationId { get; set; }

    /// <summary>
    /// Gets or sets user preferences for code generation (optional).
    /// </summary>
    public Dictionary<string, string>? UserPreferences { get; set; }
}
