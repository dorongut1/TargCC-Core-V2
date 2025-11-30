// <copyright file="DatabaseSchemaDto.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.WebAPI.Services;

/// <summary>
/// Database schema DTO matching frontend TypeScript interface.
/// </summary>
public class DatabaseSchemaDto
{
    /// <summary>
    /// Gets or sets all tables in the database.
    /// </summary>
    public List<TableDto> Tables { get; set; } = new();

    /// <summary>
    /// Gets or sets all relationships between tables.
    /// </summary>
    public List<RelationshipDto> Relationships { get; set; } = new();
}

/// <summary>
/// Database table DTO.
/// </summary>
public class TableDto
{
    /// <summary>
    /// Gets or sets table name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets schema name (dbo, etc.).
    /// </summary>
    public string Schema { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets list of columns in the table.
    /// </summary>
    public List<ColumnDto> Columns { get; set; } = new();

    /// <summary>
    /// Gets or sets number of rows in the table.
    /// </summary>
    public int? RowCount { get; set; }

    /// <summary>
    /// Gets or sets whether table contains TargCC special columns.
    /// </summary>
    public bool HasTargCCColumns { get; set; }
}

/// <summary>
/// Database column DTO.
/// </summary>
public class ColumnDto
{
    /// <summary>
    /// Gets or sets column name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets SQL data type (int, nvarchar, datetime2, etc.).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the column allows NULL values.
    /// </summary>
    public bool Nullable { get; set; }

    /// <summary>
    /// Gets or sets whether this column is a primary key.
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets whether this column is a foreign key.
    /// </summary>
    public bool IsForeignKey { get; set; }

    /// <summary>
    /// Gets or sets referenced table name if foreign key.
    /// </summary>
    public string? ForeignKeyTable { get; set; }

    /// <summary>
    /// Gets or sets referenced column name if foreign key.
    /// </summary>
    public string? ForeignKeyColumn { get; set; }

    /// <summary>
    /// Gets or sets maximum length for string types.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets default value if specified.
    /// </summary>
    public string? DefaultValue { get; set; }
}

/// <summary>
/// Database relationship DTO.
/// </summary>
public class RelationshipDto
{
    /// <summary>
    /// Gets or sets source table name.
    /// </summary>
    public string FromTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets source column name.
    /// </summary>
    public string FromColumn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets target table name.
    /// </summary>
    public string ToTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets target column name.
    /// </summary>
    public string ToColumn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets relationship type.
    /// </summary>
    public string Type { get; set; } = "one-to-many";
}
