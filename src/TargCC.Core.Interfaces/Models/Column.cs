namespace TargCC.Core.Interfaces.Models;

/// <summary>
/// Represents a database column
/// </summary>
public class Column
{
    /// <summary>
    /// Gets or sets the column name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SQL data type
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the .NET data type
    /// </summary>
    public string DotNetType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the column is nullable
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Gets or sets whether this is the primary key
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets whether this is an identity column
    /// </summary>
    public bool IsIdentity { get; set; }

    /// <summary>
    /// Gets or sets the maximum length (for string types)
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the precision (for numeric types)
    /// </summary>
    public int? Precision { get; set; }

    /// <summary>
    /// Gets or sets the scale (for numeric types)
    /// </summary>
    public int? Scale { get; set; }

    /// <summary>
    /// Gets or sets the default value
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the column description/comment
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the column prefix (eno, ent, enm, lkp, etc.)
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// Gets or sets extended properties from ccType
    /// </summary>
    public Dictionary<string, string> ExtendedProperties { get; set; } = new();

    /// <summary>
    /// Gets or sets whether this is a foreign key
    /// </summary>
    public bool IsForeignKey { get; set; }

    /// <summary>
    /// Gets or sets the referenced table name (if foreign key)
    /// </summary>
    public string? ReferencedTable { get; set; }

    /// <summary>
    /// Gets or sets the ordinal position of the column
    /// </summary>
    public int OrdinalPosition { get; set; }
}
