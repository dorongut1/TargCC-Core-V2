namespace TargCC.Core.Interfaces.Models;

/// <summary>
/// Represents a database index
/// </summary>
public class Index
{
    /// <summary>
    /// Gets or sets the index name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is a unique index
    /// </summary>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Gets or sets whether this is a clustered index
    /// </summary>
    public bool IsClustered { get; set; }

    /// <summary>
    /// Gets or sets whether this is a primary key
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the list of column names in the index
    /// </summary>
    public List<string> ColumnNames { get; set; } = new();

    /// <summary>
    /// Gets or sets the table name
    /// </summary>
    public string TableName { get; set; } = string.Empty;
}
