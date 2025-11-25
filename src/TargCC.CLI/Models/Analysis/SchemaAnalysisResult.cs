// <copyright file="SchemaAnalysisResult.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

namespace TargCC.CLI.Models.Analysis;

/// <summary>
/// Result of schema analysis.
/// </summary>
public class SchemaAnalysisResult
{
    /// <summary>
    /// Gets or sets the list of tables analyzed.
    /// </summary>
    public List<TableInfo> Tables { get; set; } = new();

    /// <summary>
    /// Gets or sets the total number of tables.
    /// </summary>
    public int TotalTables { get; set; }

    /// <summary>
    /// Gets or sets the total number of columns across all tables.
    /// </summary>
    public int TotalColumns { get; set; }
}

/// <summary>
/// Information about a table.
/// </summary>
public class TableInfo
{
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of columns.
    /// </summary>
    public List<ColumnInfo> Columns { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of indexes.
    /// </summary>
    public List<IndexInfo> Indexes { get; set; } = new();
}

/// <summary>
/// Information about a column.
/// </summary>
public class ColumnInfo
{
    /// <summary>
    /// Gets or sets the column name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data type.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the column is nullable.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is a primary key.
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the special prefix (eno_, ent_, clc_, etc.).
    /// </summary>
    public string? SpecialPrefix { get; set; }
}

/// <summary>
/// Information about an index.
/// </summary>
public class IndexInfo
{
    /// <summary>
    /// Gets or sets the index name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the columns in the index.
    /// </summary>
    public List<string> Columns { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the index is unique.
    /// </summary>
    public bool IsUnique { get; set; }
}
