using System;

namespace TargCC.Core.Interfaces.Models.Metadata;

/// <summary>
/// Metadata for a database column stored in c_Column.
/// </summary>
public class ColumnMetadata
{
    /// <summary>
    /// Gets or sets the unique identifier for the column metadata record.
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// Gets or sets the foreign key reference to the parent table metadata.
    /// </summary>
    public int TableID { get; set; }

    /// <summary>
    /// Gets or sets the name of the database column.
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SQL data type of the column.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum length for string or binary columns.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the precision for decimal or numeric columns.
    /// </summary>
    public int? Precision { get; set; }

    /// <summary>
    /// Gets or sets the scale for decimal or numeric columns.
    /// </summary>
    public int? Scale { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column allows NULL values.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is part of the primary key.
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is an identity column.
    /// </summary>
    public bool IsIdentity { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is computed.
    /// </summary>
    public bool IsComputed { get; set; }

    /// <summary>
    /// Gets or sets the default value expression for the column.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is a foreign key.
    /// </summary>
    public bool IsForeignKey { get; set; }

    /// <summary>
    /// Gets or sets the name of the referenced table if this is a foreign key.
    /// </summary>
    public string? ReferencedTable { get; set; }

    /// <summary>
    /// Gets or sets the name of the referenced column if this is a foreign key.
    /// </summary>
    public string? ReferencedColumn { get; set; }

    /// <summary>
    /// Gets or sets the name of the foreign key constraint.
    /// </summary>
    public string? ForeignKeyName { get; set; }

    /// <summary>
    /// Gets or sets the ordinal position of the column in the table.
    /// </summary>
    public int OrdinalPosition { get; set; }

    /// <summary>
    /// Gets or sets the TargCC prefix (eno_, ent_, lkp_, enm_, etc.).
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// Gets or sets the type of prefix for generation behavior.
    /// </summary>
    public string? PrefixType { get; set; }

    /// <summary>
    /// Gets or sets the UI control type for this column.
    /// </summary>
    public string? UIControlType { get; set; }

    /// <summary>
    /// Gets or sets the validation rules for this column.
    /// </summary>
    public string? ValidationRules { get; set; }

    /// <summary>
    /// Gets or sets the current SHA256 hash of the column definition.
    /// </summary>
    public string? ColumnHash { get; set; }

    /// <summary>
    /// Gets or sets the previous SHA256 hash for change detection.
    /// </summary>
    public string? ColumnHashPrevious { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this column metadata is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets optional notes about this column.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this metadata record was created.
    /// </summary>
    public DateTime AddedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the user who created this metadata record.
    /// </summary>
    public string? AddedBy { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this metadata record was last modified.
    /// </summary>
    public DateTime? ChangedOn { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified this metadata record.
    /// </summary>
    public string? ChangedBy { get; set; }
}
