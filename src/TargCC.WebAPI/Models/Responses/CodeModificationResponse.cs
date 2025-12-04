// <copyright file="CodeModificationResponse.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.WebAPI.Models.Responses;

/// <summary>
/// Response model for AI-powered code modification.
/// </summary>
public sealed class CodeModificationResponse
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
    /// Gets or sets the original code.
    /// </summary>
    public string OriginalCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of changes made.
    /// </summary>
    public List<CodeChangeDto> Changes { get; set; } = new();

    /// <summary>
    /// Gets or sets the validation result.
    /// </summary>
    public ValidationResultDto Validation { get; set; } = new();

    /// <summary>
    /// Gets or sets the error message if operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the conversation ID for tracking.
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the AI's explanation of changes.
    /// </summary>
    public string? Explanation { get; set; }
}

/// <summary>
/// DTO for code change information.
/// </summary>
public sealed class CodeChangeDto
{
    /// <summary>
    /// Gets or sets the line number where the change occurred.
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the type of change (Addition, Deletion, Modification).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the change.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the old value (before change).
    /// </summary>
    public string OldValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new value (after change).
    /// </summary>
    public string NewValue { get; set; } = string.Empty;
}

/// <summary>
/// DTO for validation result.
/// </summary>
public sealed class ValidationResultDto
{
    /// <summary>
    /// Gets or sets a value indicating whether the code is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the validation errors.
    /// </summary>
    public List<ValidationErrorDto> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets the validation warnings.
    /// </summary>
    public List<ValidationWarningDto> Warnings { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether there are breaking changes.
    /// </summary>
    public bool HasBreakingChanges { get; set; }
}

/// <summary>
/// DTO for validation error.
/// </summary>
public sealed class ValidationErrorDto
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the line number where the error occurred (optional).
    /// </summary>
    public int? LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the severity (Info, Warning, Error, Critical).
    /// </summary>
    public string Severity { get; set; } = string.Empty;
}

/// <summary>
/// DTO for validation warning.
/// </summary>
public sealed class ValidationWarningDto
{
    /// <summary>
    /// Gets or sets the warning message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the line number where the warning occurred (optional).
    /// </summary>
    public int? LineNumber { get; set; }
}
