using System;

namespace TargCC.Core.Interfaces.Models.Metadata;

/// <summary>
/// Metadata for a database column stored in c_Column
/// </summary>
public class ColumnMetadata
{
    public int ID { get; set; }
    public int TableID { get; set; }
    public string ColumnName { get; set; } = string.Empty;

    // Type Information
    public string DataType { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }

    // Constraints
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsIdentity { get; set; }
    public bool IsComputed { get; set; }
    public string? DefaultValue { get; set; }

    // Foreign Key
    public bool IsForeignKey { get; set; }
    public string? ReferencedTable { get; set; }
    public string? ReferencedColumn { get; set; }
    public string? ForeignKeyName { get; set; }

    // Position & Order
    public int OrdinalPosition { get; set; }

    // TargCC Metadata
    public string? Prefix { get; set; }
    public string? PrefixType { get; set; }
    public string? UIControlType { get; set; }
    public string? ValidationRules { get; set; }

    // Change Detection
    public string? ColumnHash { get; set; }
    public string? ColumnHashPrevious { get; set; }

    // System
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Audit
    public DateTime AddedOn { get; set; } = DateTime.UtcNow;
    public string? AddedBy { get; set; }
    public DateTime? ChangedOn { get; set; }
    public string? ChangedBy { get; set; }
}
