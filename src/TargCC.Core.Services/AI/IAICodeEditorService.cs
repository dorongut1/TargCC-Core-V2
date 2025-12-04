// Copyright (c) TargCC Team. All rights reserved.

using TargCC.Core.Services.AI.Models;

namespace TargCC.Core.Services.AI;

/// <summary>
/// Interface for AI-powered code editing service.
/// </summary>
public interface IAICodeEditorService
{
    /// <summary>
    /// Modifies code based on natural language instructions using AI.
    /// </summary>
    /// <param name="originalCode">The original code to modify.</param>
    /// <param name="instruction">Natural language instruction for the modification.</param>
    /// <param name="context">Context information for the modification.</param>
    /// <param name="conversationId">Optional conversation ID for tracking.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the code modification operation.</returns>
    Task<CodeModificationResult> ModifyCodeAsync(
        string originalCode,
        string instruction,
        ModificationContext context,
        string? conversationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that modified code is syntactically correct and doesn't break existing functionality.
    /// </summary>
    /// <param name="originalCode">The original code.</param>
    /// <param name="modifiedCode">The modified code to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with any errors or warnings.</returns>
    Task<ValidationResult> ValidateModificationAsync(
        string originalCode,
        string modifiedCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a diff between original and modified code.
    /// </summary>
    /// <param name="originalCode">The original code.</param>
    /// <param name="modifiedCode">The modified code.</param>
    /// <returns>List of code changes.</returns>
    List<CodeChange> GenerateDiff(string originalCode, string modifiedCode);

    /// <summary>
    /// Builds context information for code modification.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="schema">Database schema name.</param>
    /// <param name="relatedTables">Names of related tables.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Modification context with schema and relationship information.</returns>
    Task<ModificationContext> BuildCodeContextAsync(
        string tableName,
        string schema = "dbo",
        List<string>? relatedTables = null,
        CancellationToken cancellationToken = default);
}
