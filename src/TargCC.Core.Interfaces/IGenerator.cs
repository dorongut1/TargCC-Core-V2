namespace TargCC.Core.Interfaces;

/// <summary>
/// Base interface for all code generators in the system.
/// </summary>
public interface IGenerator
{
    /// <summary>
    /// Gets the name of the generator.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the version of the generator.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Generates code based on the provided input asynchronously.
    /// </summary>
    /// <param name="input">The input for code generation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated code.</returns>
    Task<string> GenerateAsync(object input, CancellationToken cancellationToken = default);
}
