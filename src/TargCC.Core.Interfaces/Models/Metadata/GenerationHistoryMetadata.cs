using System;

namespace TargCC.Core.Interfaces.Models.Metadata;

/// <summary>
/// Metadata for generation history stored in c_GenerationHistory.
/// </summary>
public class GenerationHistoryMetadata
{
    /// <summary>
    /// Gets or sets the unique identifier for the generation history record.
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// Gets or sets the foreign key reference to the table metadata.
    /// </summary>
    public int? TableID { get; set; }

    /// <summary>
    /// Gets or sets the name of the table for which code was generated.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the type of code generation (Entity, Repository, Controller, etc.).
    /// </summary>
    public string GenerationType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the code was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the user who triggered the code generation.
    /// </summary>
    public string? GeneratedBy { get; set; }

    /// <summary>
    /// Gets or sets the duration of the generation process in milliseconds.
    /// </summary>
    public int DurationMs { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the generation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message if the generation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the stack trace if an exception occurred during generation.
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Gets or sets the SHA256 hash of the schema at the time of generation.
    /// </summary>
    public string? SchemaHash { get; set; }

    /// <summary>
    /// Gets or sets the output path where the generated files were saved.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Gets or sets the version of the TargCC tool used for generation.
    /// </summary>
    public string? ToolVersion { get; set; }

    /// <summary>
    /// Gets or sets additional metadata in JSON format.
    /// </summary>
    public string? MetadataJson { get; set; }
}
