// <copyright file="DatabaseSchemaDto.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.WebAPI.Services;

using TargCC.WebAPI.Models;

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
