using System;
using System.Collections.Generic;

namespace TargCC.Core.Interfaces.Models.Metadata;

/// <summary>
/// Metadata for a database index stored in c_Index.
/// </summary>
public class IndexMetadata
{
    /// <summary>
    /// Gets or sets the unique identifier for the index metadata record.
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// Gets or sets the foreign key reference to the parent table metadata.
    /// </summary>
    public int TableID { get; set; }

    /// <summary>
    /// Gets or sets the name of the database index.
    /// </summary>
    public string IndexName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the index enforces uniqueness.
    /// </summary>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the index is clustered.
    /// </summary>
    public bool IsClustered { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the index is a primary key.
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the type of index (e.g., BTREE, HASH).
    /// </summary>
    public string? IndexType { get; set; }

    /// <summary>
    /// Gets or sets the filter definition for filtered indexes.
    /// </summary>
    public string? FilterDefinition { get; set; }

    /// <summary>
    /// Gets or sets the comma-separated list of included columns.
    /// </summary>
    public string? IncludedColumns { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate a GetBy method for this index.
    /// </summary>
    public bool GenerateGetByMethod { get; set; } = true;

    /// <summary>
    /// Gets or sets the custom method name for the generated GetBy method.
    /// </summary>
    public string? MethodName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this index metadata is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets optional notes about this index.
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

    /// <summary>
    /// Gets or sets the collection of columns that make up this index.
    /// </summary>
    public List<IndexColumnMetadata> Columns { get; set; } = new();
}

/// <summary>
/// Metadata for columns in an index stored in c_IndexColumn.
/// </summary>
public class IndexColumnMetadata
{
    /// <summary>
    /// Gets or sets the unique identifier for the index column metadata record.
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// Gets or sets the foreign key reference to the parent index metadata.
    /// </summary>
    public int IndexID { get; set; }

    /// <summary>
    /// Gets or sets the name of the column in the index.
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ordinal position of the column in the index.
    /// </summary>
    public int KeyOrdinal { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is sorted in descending order.
    /// </summary>
    public bool IsDescending { get; set; }
}
