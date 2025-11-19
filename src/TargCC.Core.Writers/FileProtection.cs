namespace TargCC.Core.Writers;

/// <summary>
/// Provides file protection logic to prevent overwriting manually edited files.
/// </summary>
/// <remarks>
/// <para>
/// This class implements TargCC's core principle: <strong>Build Errors as Safety Net</strong>.
/// It identifies protected files and prevents accidental overwrites, forcing developers
/// to manually review their code when schema changes occur.
/// </para>
/// <para>
/// <strong>Protected File Patterns:</strong>
/// </para>
/// <list type="bullet">
/// <item>*.prt.vb - VB.NET partial/manual files</item>
/// <item>*.prt.cs - C# partial/manual files</item>
/// <item>*.prt.* - Any other partial files</item>
/// </list>
/// </remarks>
public static class FileProtection
{
    /// <summary>
    /// Protected file extensions that indicate manual edits.
    /// </summary>
    private static readonly string[] ProtectedExtensions = { ".prt.vb", ".prt.cs" };

    /// <summary>
    /// Checks if a file is protected from overwriting.
    /// </summary>
    /// <param name="filePath">Path to check (can be relative or absolute).</param>
    /// <returns>True if the file is protected; otherwise, false.</returns>
    /// <remarks>
    /// <para>
    /// A file is considered protected if:
    /// </para>
    /// <list type="number">
    /// <item>Its name contains ".prt." before the extension</item>
    /// <item>It matches any of the protected extension patterns</item>
    /// </list>
    /// <para>
    /// The check is case-insensitive to work across different file systems.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isProtected1 = FileProtection.IsProtected("Customer.prt.cs");      // true
    /// bool isProtected2 = FileProtection.IsProtected("Customer.cs");          // false
    /// bool isProtected3 = FileProtection.IsProtected("OrderUI.prt.vb");       // true
    /// bool isProtected4 = FileProtection.IsProtected("C:\\Code\\Test.prt.cs"); // true
    /// </code>
    /// </example>
    public static bool IsProtected(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        // Get just the filename (without directory path)
        string fileName = Path.GetFileName(filePath);

        // Check if contains .prt. pattern (case-insensitive)
        if (fileName.Contains(".prt.", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Check against known protected extensions
        foreach (var extension in ProtectedExtensions)
        {
            if (fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates that a file is not protected before allowing an operation.
    /// </summary>
    /// <param name="filePath">Path to validate.</param>
    /// <exception cref="ProtectedFileException">If the file is protected.</exception>
    /// <remarks>
    /// <para>
    /// Use this method at the start of write operations to ensure protected
    /// files are not accidentally overwritten.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public async Task WriteFile(string path, string content)
    /// {
    ///     FileProtection.EnsureNotProtected(path); // Throws if protected
    ///     
    ///     // Safe to write
    ///     await File.WriteAllTextAsync(path, content);
    /// }
    /// </code>
    /// </example>
    public static void EnsureNotProtected(string filePath)
    {
        if (IsProtected(filePath))
        {
            throw new ProtectedFileException(
                filePath,
                $"Cannot overwrite protected file: {filePath}\n\n" +
                $"This file contains manual code edits and is protected from automatic overwrite.\n" +
                $"Protected files (*.prt.*) must be updated manually to preserve your custom logic.\n\n" +
                $"🛡️  This is a FEATURE, not a bug! It prevents silent code changes that could introduce bugs.\n\n" +
                $"What to do:\n" +
                $"1. Review the schema changes that triggered this\n" +
                $"2. Manually update '{Path.GetFileName(filePath)}' to match new schema\n" +
                $"3. Build will succeed once your manual updates are complete");
        }
    }

    /// <summary>
    /// Gets a list of all protected file patterns.
    /// </summary>
    /// <returns>Array of protected file extension patterns.</returns>
    /// <remarks>
    /// <para>
    /// Useful for displaying information to users about which files are protected.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var patterns = FileProtection.GetProtectedPatterns();
    /// Console.WriteLine("Protected file patterns:");
    /// foreach (var pattern in patterns)
    /// {
    ///     Console.WriteLine($"  - *{pattern}");
    /// }
    /// // Output:
    /// //   - *.prt.vb
    /// //   - *.prt.cs
    /// </code>
    /// </example>
    public static string[] GetProtectedPatterns()
    {
        return (string[])ProtectedExtensions.Clone();
    }
}
