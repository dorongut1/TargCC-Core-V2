// <copyright file="SchemaRequest.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.WebAPI.Models.Requests;

/// <summary>
/// Request model for schema operations.
/// </summary>
public sealed class SchemaRequest
{
    /// <summary>
    /// Gets or sets the connection string to the database.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include system tables.
    /// </summary>
    public bool IncludeSystemTables { get; set; }
}
