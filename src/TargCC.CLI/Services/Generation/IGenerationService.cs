using TargCC.Core.Interfaces.Models;

namespace TargCC.CLI.Services.Generation;

/// <summary>
/// Service for orchestrating code generation operations.
/// </summary>
public interface IGenerationService
{
    /// <summary>
    /// Generates entity class for a table.
    /// </summary>
    /// <param name="connectionString">Database connection string.</param>
    /// <param name="tableName">Table name.</param>
    /// <param name="outputDirectory">Output directory.</param>
    /// <param name="namespace">Namespace for entity.</param>
    /// <returns>Generation result.</returns>
    Task<GenerationResult> GenerateEntityAsync(
        string connectionString,
        string tableName,
        string outputDirectory,
        string @namespace);

    /// <summary>
    /// Generates SQL stored procedures for a table.
    /// </summary>
    /// <param name="connectionString">Database connection string.</param>
    /// <param name="tableName">Table name.</param>
    /// <param name="outputDirectory">Output directory.</param>
    /// <returns>Generation result.</returns>
    Task<GenerationResult> GenerateSqlAsync(
        string connectionString,
        string tableName,
        string outputDirectory);

    /// <summary>
    /// Generates all code (entity, SQL, repositories, etc.) for a table.
    /// </summary>
    /// <param name="connectionString">Database connection string.</param>
    /// <param name="tableName">Table name.</param>
    /// <param name="outputDirectory">Output directory.</param>
    /// <param name="namespace">Namespace.</param>
    /// <returns>Generation result.</returns>
    Task<GenerationResult> GenerateAllAsync(
        string connectionString,
        string tableName,
        string outputDirectory,
        string @namespace);

    /// <summary>
    /// Generates repository implementation for a table.
    /// </summary>
    /// <param name="connectionString">Database connection string.</param>
    /// <param name="tableName">Table name.</param>
    /// <param name="outputDirectory">Output directory.</param>
    /// <returns>Generation result.</returns>
    Task<GenerationResult> GenerateRepositoryAsync(
        string connectionString,
        string tableName,
        string outputDirectory);

    /// <summary>
    /// Generates CQRS handlers (Commands and Queries) for a table.
    /// </summary>
    /// <param name="connectionString">Database connection string.</param>
    /// <param name="tableName">Table name.</param>
    /// <param name="outputDirectory">Output directory.</param>
    /// <returns>Generation result.</returns>
    Task<GenerationResult> GenerateCqrsAsync(
        string connectionString,
        string tableName,
        string outputDirectory);

    /// <summary>
    /// Generates REST API controller for a table.
    /// </summary>
    /// <param name="connectionString">Database connection string.</param>
    /// <param name="tableName">Table name.</param>
    /// <param name="outputDirectory">Output directory.</param>
    /// <returns>Generation result.</returns>
    Task<GenerationResult> GenerateApiAsync(
        string connectionString,
        string tableName,
        string outputDirectory);
}

/// <summary>
/// Result of a generation operation.
/// </summary>
public class GenerationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether generation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets error message if generation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets list of generated files.
    /// </summary>
    public List<GeneratedFile> GeneratedFiles { get; set; } = new();

    /// <summary>
    /// Gets or sets generation duration.
    /// </summary>
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Represents a generated file.
/// </summary>
public class GeneratedFile
{
    /// <summary>
    /// Gets or sets file path.
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// Gets or sets file type (Entity, SQL, Repository, etc.).
    /// </summary>
    public required string FileType { get; set; }

    /// <summary>
    /// Gets or sets file size in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Gets or sets number of lines.
    /// </summary>
    public int LineCount { get; set; }
}
