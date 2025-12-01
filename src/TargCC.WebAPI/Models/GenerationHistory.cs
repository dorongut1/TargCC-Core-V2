namespace TargCC.WebAPI.Models;

/// <summary>
/// Represents a record of a code generation operation.
/// </summary>
public class GenerationHistory
{
    /// <summary>
    /// Gets or sets the unique identifier for this generation record.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the table name that was generated.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema name.
    /// </summary>
    public string SchemaName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the generation occurred.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the list of files that were generated.
    /// </summary>
    public string[] FilesGenerated { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets whether the generation was successful.
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Gets or sets any errors that occurred during generation.
    /// </summary>
    public string[] Errors { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets any warnings that occurred during generation.
    /// </summary>
    public string[] Warnings { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the generation options used.
    /// </summary>
    public GenerationOptions Options { get; set; } = new();
}

/// <summary>
/// Represents options for code generation.
/// </summary>
public class GenerationOptions
{
    /// <summary>
    /// Gets or sets whether to generate entity classes.
    /// </summary>
    public bool GenerateEntity { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate repository classes.
    /// </summary>
    public bool GenerateRepository { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate service classes.
    /// </summary>
    public bool GenerateService { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate controller classes.
    /// </summary>
    public bool GenerateController { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate test classes.
    /// </summary>
    public bool GenerateTests { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to overwrite existing files.
    /// </summary>
    public bool OverwriteExisting { get; set; } = false;
}
