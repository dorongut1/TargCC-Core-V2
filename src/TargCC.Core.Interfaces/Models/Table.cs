namespace TargCC.Core.Interfaces.Models;

/// <summary>
/// Represents a database table.
/// </summary>
public class Table
{
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema name.
    /// </summary>
    public string SchemaName { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets the list of columns in the table.
    /// </summary>
    public List<Column> Columns { get; set; } = new ();

    /// <summary>
    /// Gets or sets the list of indexes.
    /// </summary>
    public List<Index> Indexes { get; set; } = new ();

    /// <summary>
    /// Gets or sets the list of foreign key relationships.
    /// </summary>
    public List<Relationship> Relationships { get; set; } = new ();

    /// <summary>
    /// Gets or sets the primary key column name.
    /// </summary>
    public string? PrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the table description/comment.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this table is a system table (c_ prefix).
    /// </summary>
    public bool IsSystemTable { get; set; }

    /// <summary>
    /// Gets or sets the number of rows in the table.
    /// </summary>
    public long RowCount { get; set; }

    /// <summary>
    /// Gets the full table name (Schema.TableName).
    /// </summary>
    public string FullName => $"{SchemaName}.{Name}";

    /// <summary>
    /// Gets or sets the SQL Server object ID.
    /// </summary>
    public int ObjectId { get; set; }

    /// <summary>
    /// Gets or sets the table creation date.
    /// </summary>
    public DateTime? CreateDate { get; set; }

    /// <summary>
    /// Gets or sets the table last modification date.
    /// </summary>
    public DateTime? ModifyDate { get; set; }

    /// <summary>
    /// Gets or sets the list of primary key column names.
    /// </summary>
    public List<string> PrimaryKeyColumns { get; set; } = new ();

    /// <summary>
    /// Gets or sets the extended properties.
    /// </summary>
    public Dictionary<string, string> ExtendedProperties { get; set; } = new ();
}
