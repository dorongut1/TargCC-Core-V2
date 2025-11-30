// <copyright file="ISchemaService.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

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
}
