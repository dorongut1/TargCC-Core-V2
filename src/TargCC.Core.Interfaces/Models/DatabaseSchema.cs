namespace TargCC.Core.Interfaces.Models;

/// <summary>
/// Represents a database schema.
/// </summary>
public class DatabaseSchema
{
    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database server.
    /// </summary>
    public string ServerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of tables in the database.
    /// </summary>
    public List<Table> Tables { get; set; } = new ();

    /// <summary>
    /// Gets or sets the database version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date when the schema was analyzed.
    /// </summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date when the schema was analyzed (alias for AnalyzedAt).
    /// </summary>
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the list of relationships between tables.
    /// </summary>
    public List<Relationship> Relationships { get; set; } = new ();

    /// <summary>
    /// Gets or sets a value indicating whether this is an incremental analysis.
    /// </summary>
    public bool IsIncrementalAnalysis { get; set; } = false;
}
