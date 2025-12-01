namespace TargCC.WebAPI.Models;

/// <summary>
/// Represents a table column with metadata.
/// </summary>
public class ColumnDto
{
    /// <summary>
    /// Gets or sets the column name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the column is nullable.
    /// </summary>
    public bool Nullable { get; set; }

    /// <summary>
    /// Gets or sets whether the column is a primary key.
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets whether the column is a foreign key.
    /// </summary>
    public bool IsForeignKey { get; set; }

    /// <summary>
    /// Gets or sets the foreign key table name.
    /// </summary>
    public string? ForeignKeyTable { get; set; }

    /// <summary>
    /// Gets or sets the foreign key column name.
    /// </summary>
    public string? ForeignKeyColumn { get; set; }

    /// <summary>
    /// Gets or sets the maximum length.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    public string? DefaultValue { get; set; }
}
