namespace TargCC.Core.Interfaces;

/// <summary>
/// Base interface for all validators in the system
/// </summary>
public interface IValidator
{
    /// <summary>
    /// Gets the name of the validator
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the version of the validator
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Validates the provided input asynchronously
    /// </summary>
    /// <param name="input">The input to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateAsync(object input, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of a validation operation
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets or sets whether the validation was successful
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the list of validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of validation warnings
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}
