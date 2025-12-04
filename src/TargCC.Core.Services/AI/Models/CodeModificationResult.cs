// Copyright (c) TargCC Team. All rights reserved.

namespace TargCC.Core.Services.AI.Models;

/// <summary>
/// Result of a code modification operation.
/// </summary>
public sealed class CodeModificationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the modification was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the modified code.
    /// </summary>
    public string ModifiedCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original code (before modification).
    /// </summary>
    public string OriginalCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of changes made.
    /// </summary>
    public List<CodeChange> Changes { get; set; } = new();

    /// <summary>
    /// Gets or sets the validation result.
    /// </summary>
    public ValidationResult Validation { get; set; } = ValidationResult.Success();

    /// <summary>
    /// Gets or sets the error message (if any).
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the conversation ID for tracking.
    /// </summary>
    public string ConversationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the AI's explanation of the changes.
    /// </summary>
    public string? Explanation { get; set; }

    /// <summary>
    /// Creates a successful modification result.
    /// </summary>
    /// <param name="originalCode">The original code.</param>
    /// <param name="modifiedCode">The modified code.</param>
    /// <param name="changes">The list of changes.</param>
    /// <param name="explanation">Explanation of changes.</param>
    /// <returns>A successful CodeModificationResult.</returns>
    public static CodeModificationResult CreateSuccess(
        string originalCode,
        string modifiedCode,
        List<CodeChange> changes,
        string? explanation = null)
    {
        return new CodeModificationResult
        {
            Success = true,
            OriginalCode = originalCode,
            ModifiedCode = modifiedCode,
            Changes = changes,
            Explanation = explanation,
            Validation = ValidationResult.Success(),
        };
    }

    /// <summary>
    /// Creates a failed modification result.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="originalCode">The original code.</param>
    /// <returns>A failed CodeModificationResult.</returns>
    public static CodeModificationResult CreateFailure(string errorMessage, string originalCode = "")
    {
        return new CodeModificationResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            OriginalCode = originalCode,
            Validation = ValidationResult.Failure(errorMessage),
        };
    }
}
