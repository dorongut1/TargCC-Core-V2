// <copyright file="SchemaResponse.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.WebAPI.Models.Responses;

/// <summary>
/// Response model for schema operations.
/// </summary>
public sealed class SchemaResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the list of tables.
    /// </summary>
    public List<TableInfo> Tables { get; set; } = new();

    /// <summary>
    /// Gets or sets any errors that occurred.
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Information about a database table.
/// </summary>
public sealed class TableInfo
{
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the table schema.
    /// </summary>
    public string Schema { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets the number of columns.
    /// </summary>
    public int ColumnCount { get; set; }

    /// <summary>
    /// Gets or sets the list of columns.
    /// </summary>
    public List<ColumnInfo> Columns { get; set; } = new();
}

/// <summary>
/// Information about a table column.
/// </summary>
public sealed class ColumnInfo
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
    /// Gets or sets a value indicating whether column is nullable.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether column is primary key.
    /// </summary>
    public bool IsPrimaryKey { get; set; }
}
