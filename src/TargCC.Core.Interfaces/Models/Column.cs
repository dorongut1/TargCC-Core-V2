namespace TargCC.Core.Interfaces.Models;

/// <summary>
/// Represents a database column.
/// </summary>
public class Column
{
    /// <summary>
    /// Gets or sets the column name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SQL data type.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the .NET data type.
    /// </summary>
    public string DotNetType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the column is nullable.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the primary key.
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is an identity column.
    /// </summary>
    public bool IsIdentity { get; set; }

    /// <summary>
    /// Gets or sets the maximum length (for string types).
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the precision (for numeric types).
    /// </summary>
    public int? Precision { get; set; }

    /// <summary>
    /// Gets or sets the scale (for numeric types).
    /// </summary>
    public int? Scale { get; set; }

    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the column description/comment.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the column prefix (eno, ent, enm, lkp, etc.).
    /// </summary>
    public ColumnPrefix Prefix { get; set; } = ColumnPrefix.None;

    /// <summary>
    /// Gets or sets extended properties from ccType.
    /// </summary>
    public Dictionary<string, string> ExtendedProperties { get; set; } = new ();

    /// <summary>
    /// Gets or sets a value indicating whether this is a foreign key.
    /// </summary>
    public bool IsForeignKey { get; set; }

    /// <summary>
    /// Gets or sets the referenced table name (if foreign key).
    /// </summary>
    public string? ReferencedTable { get; set; }

    /// <summary>
    /// Gets or sets the ordinal position of the column.
    /// </summary>
    public int OrdinalPosition { get; set; }

    /// <summary>
    /// Gets or sets the column ID.
    /// </summary>
    public int ColumnId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a computed column.
    /// </summary>
    public bool IsComputed { get; set; }

    /// <summary>
    /// Gets or sets the computed column definition.
    /// </summary>
    public string? ComputedDefinition { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this column is encrypted.
    /// </summary>
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this column is read-only.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to exclude this column from audit.
    /// </summary>
    public bool DoNotAudit { get; set; }
}
