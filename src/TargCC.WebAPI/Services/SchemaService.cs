// <copyright file="SchemaService.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace TargCC.WebAPI.Services;

/// <summary>
/// Service for reading database schema information using Dapper.
/// </summary>
public class SchemaService : ISchemaService
{
    private readonly ILogger<SchemaService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public SchemaService(ILogger<SchemaService> logger)
    {
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetSchemasAsync(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        
        const string sql = @"
            SELECT DISTINCT TABLE_SCHEMA 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_SCHEMA";

        var schemas = await connection.QueryAsync<string>(sql);
        return schemas.ToList();
    }

    /// <inheritdoc/>
    public async Task<DatabaseSchemaDto> GetSchemaDetailsAsync(string connectionString, string schemaName)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var schema = new DatabaseSchemaDto();

        // Get tables
        schema.Tables = await GetTablesAsync(connection, schemaName);

        // Get columns for each table
        foreach (var table in schema.Tables)
        {
            table.Columns = await GetColumnsAsync(connection, schemaName, table.Name);
            table.HasTargCCColumns = CheckTargCCColumns(table.Columns);
        }

        // Get relationships
        schema.Relationships = await GetRelationshipsAsync(connection, schemaName);

        return schema;
    }

    private async Task<List<TableDto>> GetTablesAsync(IDbConnection connection, string schemaName)
    {
        const string sql = @"
            SELECT 
                t.TABLE_NAME as Name,
                t.TABLE_SCHEMA as [Schema],
                p.rows as [RowCount]
            FROM INFORMATION_SCHEMA.TABLES t
            LEFT JOIN sys.tables st ON t.TABLE_NAME = st.name
            LEFT JOIN sys.partitions p ON st.object_id = p.object_id AND p.index_id IN (0, 1)
            WHERE t.TABLE_TYPE = 'BASE TABLE' 
                AND t.TABLE_SCHEMA = @SchemaName
            ORDER BY t.TABLE_NAME";

        var tables = await connection.QueryAsync<TableDto>(sql, new { SchemaName = schemaName });
        return tables.ToList();
    }

    private async Task<List<ColumnDto>> GetColumnsAsync(IDbConnection connection, string schemaName, string tableName)
    {
        const string sql = @"
            SELECT 
                c.COLUMN_NAME as Name,
                c.DATA_TYPE as [Type],
                CASE WHEN c.IS_NULLABLE = 'YES' THEN 1 ELSE 0 END as Nullable,
                CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END as IsPrimaryKey,
                CASE WHEN fk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END as IsForeignKey,
                fk.REFERENCED_TABLE_NAME as ForeignKeyTable,
                fk.REFERENCED_COLUMN_NAME as ForeignKeyColumn,
                c.CHARACTER_MAXIMUM_LENGTH as MaxLength,
                c.COLUMN_DEFAULT as DefaultValue
            FROM INFORMATION_SCHEMA.COLUMNS c
            LEFT JOIN (
                SELECT ku.TABLE_SCHEMA, ku.TABLE_NAME, ku.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku 
                    ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
            ) pk ON c.TABLE_SCHEMA = pk.TABLE_SCHEMA 
                AND c.TABLE_NAME = pk.TABLE_NAME 
                AND c.COLUMN_NAME = pk.COLUMN_NAME
            LEFT JOIN (
                SELECT 
                    fk.TABLE_SCHEMA,
                    fk.TABLE_NAME,
                    fk.COLUMN_NAME,
                    pk.TABLE_NAME as REFERENCED_TABLE_NAME,
                    pk.COLUMN_NAME as REFERENCED_COLUMN_NAME
                FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE fk 
                    ON rc.CONSTRAINT_NAME = fk.CONSTRAINT_NAME
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk 
                    ON rc.UNIQUE_CONSTRAINT_NAME = pk.CONSTRAINT_NAME
            ) fk ON c.TABLE_SCHEMA = fk.TABLE_SCHEMA 
                AND c.TABLE_NAME = fk.TABLE_NAME 
                AND c.COLUMN_NAME = fk.COLUMN_NAME
            WHERE c.TABLE_SCHEMA = @SchemaName 
                AND c.TABLE_NAME = @TableName
            ORDER BY c.ORDINAL_POSITION";

        var columns = await connection.QueryAsync<ColumnDto>(sql, new { SchemaName = schemaName, TableName = tableName });
        return columns.ToList();
    }

    private async Task<List<RelationshipDto>> GetRelationshipsAsync(IDbConnection connection, string schemaName)
    {
        const string sql = @"
            SELECT 
                fk_table.name as FromTable,
                fk_col.name as FromColumn,
                pk_table.name as ToTable,
                pk_col.name as ToColumn,
                'one-to-many' as [Type]
            FROM sys.foreign_keys fk
            JOIN sys.tables fk_table ON fk.parent_object_id = fk_table.object_id
            JOIN sys.schemas fk_schema ON fk_table.schema_id = fk_schema.schema_id
            JOIN sys.tables pk_table ON fk.referenced_object_id = pk_table.object_id
            JOIN sys.foreign_key_columns fk_cols ON fk.object_id = fk_cols.constraint_object_id
            JOIN sys.columns fk_col ON fk_cols.parent_column_id = fk_col.column_id 
                AND fk_cols.parent_object_id = fk_col.object_id
            JOIN sys.columns pk_col ON fk_cols.referenced_column_id = pk_col.column_id 
                AND fk_cols.referenced_object_id = pk_col.object_id
            WHERE fk_schema.name = @SchemaName";

        var relationships = await connection.QueryAsync<RelationshipDto>(sql, new { SchemaName = schemaName });
        return relationships.ToList();
    }

    private bool CheckTargCCColumns(List<ColumnDto> columns)
    {
        // Check for TargCC special column prefixes
        var targccPrefixes = new[] { "eno_", "ent_", "clc_", "lkp_", "rel_" };
        return columns.Any(c => targccPrefixes.Any(prefix => c.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)));
    }
}
