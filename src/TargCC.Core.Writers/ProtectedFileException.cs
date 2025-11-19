namespace TargCC.Core.Writers;

/// <summary>
/// Exception thrown when attempting to overwrite a protected file (*.prt file).
/// </summary>
/// <remarks>
/// <para>
/// This exception is a core part of TargCC's <strong>Build Errors as Safety Net</strong> principle.
/// Protected files contain manual code edits that must never be overwritten automatically.
/// </para>
/// <para>
/// When this exception is thrown, it indicates that the developer needs to manually
/// review and update their custom code to accommodate schema changes.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// try
/// {
///     await writer.WriteFileAsync("Customer.prt.cs", newCode);
/// }
/// catch (ProtectedFileException ex)
/// {
///     Console.WriteLine($"Cannot overwrite: {ex.Message}");
///     Console.WriteLine($"Protected file: {ex.FilePath}");
///     // Developer must manually update the file
/// }
/// </code>
/// </example>
public class ProtectedFileException : InvalidOperationException
{
    /// <summary>
    /// Gets the path to the protected file that was attempted to be overwritten.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtectedFileException"/> class.
    /// </summary>
    /// <param name="filePath">The path to the protected file.</param>
    public ProtectedFileException(string filePath)
        : base($"Cannot overwrite protected file: {filePath}. This file contains manual code edits and must be updated manually.")
    {
        FilePath = filePath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtectedFileException"/> class with a custom message.
    /// </summary>
    /// <param name="filePath">The path to the protected file.</param>
    /// <param name="message">Custom error message.</param>
    public ProtectedFileException(string filePath, string message)
        : base(message)
    {
        FilePath = filePath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtectedFileException"/> class with an inner exception.
    /// </summary>
    /// <param name="filePath">The path to the protected file.</param>
    /// <param name="message">Custom error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ProtectedFileException(string filePath, string message, Exception innerException)
        : base(message, innerException)
    {
        FilePath = filePath;
    }
}
