namespace TargCC.Core.Interfaces;

/// <summary>
/// Base interface for all analyzers in the system
/// </summary>
public interface IAnalyzer
{
    /// <summary>
    /// Gets the name of the analyzer
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the version of the analyzer
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Analyzes the provided input asynchronously
    /// </summary>
    /// <param name="input">The input to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analysis result</returns>
    Task<object> AnalyzeAsync(object input, CancellationToken cancellationToken = default);
}
