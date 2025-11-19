namespace TargCC.Core.Writers;

/// <summary>
/// Interface for file writing operations with protection for manually edited files.
/// </summary>
/// <remarks>
/// <para>
/// This interface defines the contract for writing generated code to disk while
/// respecting TargCC's core principle: <strong>Build Errors as Safety Net</strong>.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// </para>
/// <list type="bullet">
/// <item>Write generated code files to disk</item>
/// <item>Protect manually edited files (*.prt files) from overwrite</item>
/// <item>Automatic backup before writing</item>
/// <item>Directory creation support</item>
/// </list>
/// <para>
/// <strong>Protected Files (*.prt):</strong>
/// </para>
/// <para>
/// Files ending with .prt.vb, .prt.cs, or .prt.* are considered manually edited
/// and must never be overwritten. Attempting to write to these files will throw
/// a <see cref="ProtectedFileException"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Basic usage
/// var writer = new FileWriter(logger);
/// await writer.WriteFileAsync("Customer.cs", generatedCode);
/// 
/// // Protected file - will throw exception
/// await writer.WriteFileAsync("Customer.prt.cs", code); // ❌ Throws!
/// 
/// // Check if protected
/// if (writer.IsProtectedFile("Customer.prt.cs"))
/// {
///     Console.WriteLine("Cannot overwrite protected file!");
/// }
/// </code>
/// </example>
public interface IFileWriter
{
    /// <summary>
    /// Writes content to a file, creating directories if needed.
    /// </summary>
    /// <param name="filePath">Full path to the file to write.</param>
    /// <param name="content">Content to write to the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">If filePath or content is null.</exception>
    /// <exception cref="ProtectedFileException">If the file is protected (*.prt file).</exception>
    /// <exception cref="IOException">If an I/O error occurs.</exception>
    /// <remarks>
    /// <para>
    /// This method will:
    /// </para>
    /// <list type="number">
    /// <item>Check if the file is protected (*.prt)</item>
    /// <item>Create parent directories if they don't exist</item>
    /// <item>Create a backup if the file already exists</item>
    /// <item>Write the content to the file</item>
    /// <item>Log the operation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Write a generated entity class
    /// string code = entityGenerator.Generate(table);
    /// await writer.WriteFileAsync("C:\\Output\\Customer.cs", code);
    /// </code>
    /// </example>
    Task WriteFileAsync(string filePath, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file is protected from overwriting (*.prt file).
    /// </summary>
    /// <param name="filePath">Path to check.</param>
    /// <returns>True if the file is protected; otherwise, false.</returns>
    /// <remarks>
    /// <para>
    /// Protected files are those ending with:
    /// </para>
    /// <list type="bullet">
    /// <item>.prt.vb (VB.NET partial files)</item>
    /// <item>.prt.cs (C# partial files)</item>
    /// <item>.prt.* (any other partial files)</item>
    /// </list>
    /// <para>
    /// These files contain manual code edits and must never be overwritten.
    /// When the database schema changes, protected files will intentionally
    /// cause build errors, forcing developers to manually review and update
    /// their custom logic.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Check before attempting to write
    /// string path = "Customer.prt.cs";
    /// if (writer.IsProtectedFile(path))
    /// {
    ///     Console.WriteLine("This file contains manual edits - skipping!");
    /// }
    /// else
    /// {
    ///     await writer.WriteFileAsync(path, content);
    /// }
    /// </code>
    /// </example>
    bool IsProtectedFile(string filePath);

    /// <summary>
    /// Updates an existing file by replacing old content with new content.
    /// </summary>
    /// <param name="filePath">Full path to the file to update.</param>
    /// <param name="oldContent">Content to search for and replace.</param>
    /// <param name="newContent">New content to replace with.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">If any parameter is null.</exception>
    /// <exception cref="ProtectedFileException">If the file is protected (*.prt file).</exception>
    /// <exception cref="FileNotFoundException">If the file doesn't exist.</exception>
    /// <exception cref="InvalidOperationException">If old content is not found in the file.</exception>
    /// <remarks>
    /// <para>
    /// This method performs a string replacement operation similar to str_replace.
    /// It's useful for making targeted updates to generated files without regenerating
    /// the entire file.
    /// </para>
    /// <para>
    /// The old content must match exactly (including whitespace) and must appear
    /// exactly once in the file.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Update a method in a generated class
    /// await writer.UpdateFileAsync(
    ///     "Customer.cs",
    ///     oldContent: "public string Name { get; set; }",
    ///     newContent: "public string FullName { get; set; }"
    /// );
    /// </code>
    /// </example>
    Task UpdateFileAsync(string filePath, string oldContent, string newContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    /// <param name="filePath">Path to check.</param>
    /// <returns>True if the file exists; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// if (await writer.FileExistsAsync("Customer.cs"))
    /// {
    ///     Console.WriteLine("File already exists - will be backed up");
    /// }
    /// </code>
    /// </example>
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a directory and all parent directories if they don't exist.
    /// </summary>
    /// <param name="directoryPath">Path to the directory to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">If directoryPath is null.</exception>
    /// <remarks>
    /// <para>
    /// This method is idempotent - calling it multiple times with the same path
    /// has no additional effect if the directory already exists.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create output directory structure
    /// await writer.EnsureDirectoryExistsAsync("C:\\Output\\Entities");
    /// await writer.EnsureDirectoryExistsAsync("C:\\Output\\Repositories");
    /// </code>
    /// </example>
    Task EnsureDirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default);
}
