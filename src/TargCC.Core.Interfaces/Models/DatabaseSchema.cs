namespace TargCC.Core.Interfaces.Models;

/// <summary>
/// Represents the complete database schema with all tables, relationships, and metadata.
/// </summary>
/// <remarks>
/// <para>
/// DatabaseSchema is the root model that contains the entire analyzed database structure.
/// It serves as the input for code generation, containing all tables, columns, indexes,
/// and relationships discovered during database analysis.
/// </para>
/// <para>
/// <strong>Key Components:</strong>
/// </para>
/// <list type="bullet">
/// <item><term>Tables</term><description>All database tables with columns, indexes, and metadata</description></item>
/// <item><term>Relationships</term><description>All foreign key relationships between tables</description></item>
/// <item><term>Metadata</term><description>Database name, version, server, analysis timestamp</description></item>
/// </list>
/// <para>
/// <strong>Usage in TargCC:</strong>
/// </para>
/// <list type="number">
/// <item>DatabaseAnalyzer analyzes SQL Server database → creates DatabaseSchema</item>
/// <item>DatabaseSchema passed to code generators</item>
/// <item>Generators create C# classes, stored procedures, UI forms based on schema</item>
/// <item>Incremental mode: only changed tables are regenerated</item>
/// </list>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Complete database schema</strong></para>
/// <code>
/// var schema = new DatabaseSchema
/// {
///     DatabaseName = "MyAppDB",
///     ServerName = "localhost",
///     Version = "SQL Server 2022",
///     AnalyzedAt = DateTime.UtcNow,
///     Tables = new List&lt;Table&gt;
///     {
///         new() { Name = "Customer", Columns = ... },
///         new() { Name = "Order", Columns = ... },
///         new() { Name = "OrderLine", Columns = ... }
///     },
///     Relationships = new List&lt;Relationship&gt;
///     {
///         new() { ParentTable = "Customer", ChildTable = "Order" },
///         new() { ParentTable = "Order", ChildTable = "OrderLine" }
///     }
/// };
///
/// Console.WriteLine($"Database: {schema.DatabaseName} on {schema.ServerName}");
/// Console.WriteLine($"Tables: {schema.Tables.Count}");
/// Console.WriteLine($"Relationships: {schema.Relationships.Count}");
/// </code>
/// </example>
/// <example>
/// <para><strong>Example 2: Incremental analysis</strong></para>
/// <code>
/// // Initial full analysis
/// var fullSchema = await analyzer.AnalyzeAsync(connectionString);
/// // fullSchema.IsIncrementalAnalysis = false
/// // fullSchema.Tables contains all tables
///
/// // Later: incremental analysis (only Customer table changed)
/// var incrementalSchema = await analyzer.AnalyzeIncrementalAsync(
///     connectionString,
///     changedTables: new[] { "Customer" }
/// );
/// // incrementalSchema.IsIncrementalAnalysis = true
/// // incrementalSchema.Tables contains only Customer table
///
/// // Code generation: only Customer.cs and related files regenerated
/// </code>
/// </example>
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
