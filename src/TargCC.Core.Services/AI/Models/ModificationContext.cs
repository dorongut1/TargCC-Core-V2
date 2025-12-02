// Copyright (c) TargCC Team. All rights reserved.

namespace TargCC.Core.Services.AI.Models;

/// <summary>
/// Provides context for AI code modification operations.
/// </summary>
public sealed class ModificationContext
{
    /// <summary>
    /// Gets or sets the name of the table being worked with.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database schema name.
    /// </summary>
    public string Schema { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets the names of related tables (for foreign keys).
    /// </summary>
    public List<string> RelatedTables { get; set; } = new();

    /// <summary>
    /// Gets or sets user preferences for code generation.
    /// </summary>
    public Dictionary<string, string> UserPreferences { get; set; } = new();

    /// <summary>
    /// Gets or sets the full table schema information (JSON).
    /// </summary>
    public string? TableSchemaJson { get; set; }

    /// <summary>
    /// Gets or sets the project conventions (coding style, patterns).
    /// </summary>
    public Dictionary<string, string> ProjectConventions { get; set; } = new();
}
