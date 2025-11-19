namespace TargCC.Core.Writers;

using System.Text;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides file writing operations with protection for manually edited files.
/// </summary>
/// <remarks>
/// <para>
/// This class implements safe file writing for TargCC code generation, ensuring that
/// manually edited files (*.prt files) are never accidentally overwritten.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// </para>
/// <list type="bullet">
/// <item>Automatic directory creation</item>
/// <item>Protected file detection and prevention</item>
/// <item>Automatic backups before overwriting</item>
/// <item>Comprehensive logging</item>
/// <item>UTF-8 encoding with BOM</item>
/// </list>
/// </remarks>
public class FileWriter : IFileWriter
{
    private readonly ILogger<FileWriter> _logger;
    private const string BackupExtension = ".bak";

    /// <summary>
    /// Initializes a new instance of the <see cref="FileWriter"/> class.
    /// </summary>
    /// <param name="logger">Logger for tracking file operations.</param>
    public FileWriter(ILogger<FileWriter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task WriteFileAsync(string filePath, string content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(content);

        // Check if file is protected
        FileProtection.EnsureNotProtected(filePath);

        _logger.LogDebug("Writing file: {FilePath}", filePath);

        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                await EnsureDirectoryExistsAsync(directory, cancellationToken);
            }

            // Backup existing file if it exists
            if (File.Exists(filePath))
            {
                await CreateBackupAsync(filePath, cancellationToken);
            }

            // Write file with UTF-8 encoding
            await File.WriteAllTextAsync(filePath, content, Encoding.UTF8, cancellationToken);

            _logger.LogInformation("Successfully wrote file: {FilePath} ({Size} bytes)", 
                filePath, content.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing file: {FilePath}", filePath);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool IsProtectedFile(string filePath)
    {
        return FileProtection.IsProtected(filePath);
    }

    /// <inheritdoc/>
    public async Task UpdateFileAsync(
        string filePath, 
        string oldContent, 
        string newContent, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(oldContent);
        ArgumentNullException.ThrowIfNull(newContent);

        // Check if file is protected
        FileProtection.EnsureNotProtected(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}", filePath);
        }

        _logger.LogDebug("Updating file: {FilePath}", filePath);

        try
        {
            // Read current content
            var currentContent = await File.ReadAllTextAsync(filePath, cancellationToken);

            // Check if old content exists
            if (!currentContent.Contains(oldContent))
            {
                throw new InvalidOperationException(
                    $"Old content not found in file: {filePath}\n" +
                    $"Searched for: {oldContent.Substring(0, Math.Min(100, oldContent.Length))}...");
            }

            // Check if old content appears exactly once
            var occurrences = CountOccurrences(currentContent, oldContent);
            if (occurrences != 1)
            {
                throw new InvalidOperationException(
                    $"Old content appears {occurrences} times in file (expected exactly 1): {filePath}");
            }

            // Backup before updating
            await CreateBackupAsync(filePath, cancellationToken);

            // Replace content
            var updatedContent = currentContent.Replace(oldContent, newContent);

            // Write updated content
            await File.WriteAllTextAsync(filePath, updatedContent, Encoding.UTF8, cancellationToken);

            _logger.LogInformation("Successfully updated file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating file: {FilePath}", filePath);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        
        return Task.FromResult(File.Exists(filePath));
    }

    /// <inheritdoc/>
    public Task EnsureDirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);

        if (!Directory.Exists(directoryPath))
        {
            _logger.LogDebug("Creating directory: {DirectoryPath}", directoryPath);
            Directory.CreateDirectory(directoryPath);
            _logger.LogInformation("Created directory: {DirectoryPath}", directoryPath);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a backup of a file before overwriting it.
    /// </summary>
    /// <param name="filePath">Path to the file to backup.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Path to the backup file.</returns>
    private async Task<string> CreateBackupAsync(string filePath, CancellationToken cancellationToken)
    {
        var backupPath = filePath + BackupExtension;
        
        _logger.LogDebug("Creating backup: {BackupPath}", backupPath);
        
        // If backup already exists, add timestamp
        if (File.Exists(backupPath))
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            backupPath = $"{filePath}.{timestamp}{BackupExtension}";
        }

        await Task.Run(() => File.Copy(filePath, backupPath, overwrite: true), cancellationToken);
        
        _logger.LogDebug("Backup created: {BackupPath}", backupPath);
        
        return backupPath;
    }

    /// <summary>
    /// Counts how many times a substring appears in a string.
    /// </summary>
    private static int CountOccurrences(string text, string substring)
    {
        if (string.IsNullOrEmpty(substring))
        {
            return 0;
        }

        int count = 0;
        int index = 0;

        while ((index = text.IndexOf(substring, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += substring.Length;
        }

        return count;
    }
}
