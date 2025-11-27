// Copyright (c) TargCC Team. All rights reserved.

using TargCC.AI.Models;
using TargCC.Core.Interfaces.Models;

namespace TargCC.AI.Services;

/// <summary>
/// Interface for AI service operations.
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Sends a completion request to the AI service.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The AI response.</returns>
    Task<AIResponse> CompleteAsync(AIRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a chat message in an ongoing conversation.
    /// </summary>
    /// <param name="message">The user message.</param>
    /// <param name="context">The conversation context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The AI response.</returns>
    Task<AIResponse> ChatAsync(string message, ConversationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes a database schema and provides insights.
    /// </summary>
    /// <param name="schemaJson">The database schema as JSON.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The analysis response.</returns>
    Task<AIResponse> AnalyzeSchemaAsync(string schemaJson, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets suggestions for improving a database schema.
    /// </summary>
    /// <param name="schemaJson">The database schema as JSON.</param>
    /// <param name="context">Additional context for suggestions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The suggestions response.</returns>
    Task<AIResponse> GetSuggestionsAsync(string schemaJson, string? context = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes code for security vulnerabilities.
    /// </summary>
    /// <param name="code">The code to analyze.</param>
    /// <param name="language">The programming language.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The security analysis response.</returns>
    Task<AIResponse> AnalyzeSecurityAsync(string code, string language, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes a table schema and provides structured suggestions.
    /// </summary>
    /// <param name="table">The table definition to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The structured schema analysis result.</returns>
    Task<SchemaAnalysisResult> AnalyzeTableSchemaAsync(Table table, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets structured suggestions for a specific table.
    /// </summary>
    /// <param name="table">The table to get suggestions for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The structured schema analysis result with suggestions.</returns>
    Task<SchemaAnalysisResult> GetTableSuggestionsAsync(Table table, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the AI service is available and configured correctly.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if service is healthy; otherwise, false.</returns>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}
