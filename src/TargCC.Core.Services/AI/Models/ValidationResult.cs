// Copyright (c) TargCC Team. All rights reserved.

namespace TargCC.Core.Services.AI.Models;

/// <summary>
/// Result of code validation.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the code is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the validation errors.
    /// </summary>
    public List<ValidationError> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets the validation warnings.
    /// </summary>
    public List<ValidationWarning> Warnings { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether there are breaking changes.
    /// </summary>
    public bool HasBreakingChanges { get; set; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A valid ValidationResult.</returns>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with an error.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>An invalid ValidationResult.</returns>
    public static ValidationResult Failure(string errorMessage) => new()
    {
        IsValid = false,
        Errors = new List<ValidationError>
        {
            new() { Message = errorMessage, Severity = ErrorSeverity.Error },
        },
    };
}

/// <summary>
/// Represents a validation error.
/// </summary>
public sealed class ValidationError
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the line number where the error occurred.
    /// </summary>
    public int? LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the severity of the error.
    /// </summary>
    public ErrorSeverity Severity { get; set; }
}

/// <summary>
/// Represents a validation warning.
/// </summary>
public sealed class ValidationWarning
{
    /// <summary>
    /// Gets or sets the warning message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the line number where the warning occurred.
    /// </summary>
    public int? LineNumber { get; set; }
}

/// <summary>
/// Severity levels for validation errors.
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Informational message.
    /// </summary>
    Info,

    /// <summary>
    /// Warning message.
    /// </summary>
    Warning,

    /// <summary>
    /// Error message.
    /// </summary>
    Error,

    /// <summary>
    /// Critical error.
    /// </summary>
    Critical,
}
