using System;
using System.Collections.Generic;

namespace TargCC.Core.Interfaces.Models.Metadata;

/// <summary>
/// Metadata for a database index stored in c_Index
/// </summary>
public class IndexMetadata
{
    public int ID { get; set; }
    public int TableID { get; set; }
    public string IndexName { get; set; } = string.Empty;

    // Index Type
    public bool IsUnique { get; set; }
    public bool IsClustered { get; set; }
    public bool IsPrimaryKey { get; set; }
    public string? IndexType { get; set; }

    // Filter & Include
    public string? FilterDefinition { get; set; }
    public string? IncludedColumns { get; set; }

    // Generation Options
    public bool GenerateGetByMethod { get; set; } = true;
    public string? MethodName { get; set; }

    // System
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Audit
    public DateTime AddedOn { get; set; } = DateTime.UtcNow;
    public string? AddedBy { get; set; }
    public DateTime? ChangedOn { get; set; }
    public string? ChangedBy { get; set; }

    // Navigation
    public List<IndexColumnMetadata> Columns { get; set; } = new();
}

/// <summary>
/// Metadata for columns in an index stored in c_IndexColumn
/// </summary>
public class IndexColumnMetadata
{
    public int ID { get; set; }
    public int IndexID { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public int KeyOrdinal { get; set; }
    public bool IsDescending { get; set; }
}
