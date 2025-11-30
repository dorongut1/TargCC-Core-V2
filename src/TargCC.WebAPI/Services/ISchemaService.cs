// <copyright file="ISchemaService.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

using TargCC.WebAPI.Models;

namespace TargCC.WebAPI.Services;

/// <summary>
/// Service for reading database schema information.
/// </summary>
public interface ISchemaService
{
    /// <summary>
    /// Get list of available schemas.
    /// </summary>
    /// <param name="connectionString">Database connection string.</param>
    /// <returns>List of schema names.</returns>
    Task<List<string>> GetSchemasAsync(string connectionString);

    /// <summary>
    /// Get detailed schema information.
    /// </summary>
    /// <param name="connectionString">Database connection string.</param>
    /// <param name="schemaName">Name of the schema.</param>
    /// <returns>Schema details including tables, columns, and relationships.</returns>
    Task<DatabaseSchemaDto> GetSchemaDetailsAsync(string connectionString, string schemaName);

    /// <summary>
    /// Get preview data for a specific table.
    /// </summary>
    /// <param name="connectionString">Database connection string.</param>
    /// <param name="schemaName">Name of the schema.</param>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="rowCount">Number of rows to retrieve (default 10).</param>
    /// <returns>Table preview data with columns and rows.</returns>
    Task<TablePreviewDto> GetTablePreviewAsync(string connectionString, string schemaName, string tableName, int rowCount = 10);
}
