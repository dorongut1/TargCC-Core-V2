// <copyright file="GenerateRequest.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.WebAPI.Models.Requests;

/// <summary>
/// Request model for code generation operations.
/// </summary>
public sealed class GenerateRequest
{
    /// <summary>
    /// Gets or sets the table names to generate code for.
    /// </summary>
    public List<string> TableNames { get; set; } = new();

    /// <summary>
    /// Gets or sets the project path where code will be generated.
    /// </summary>
    public string? ProjectPath { get; set; }

    /// <summary>
    /// Gets or sets the connection string to the database.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to force regeneration.
    /// </summary>
    public bool Force { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include stored procedures.
    /// </summary>
    public bool IncludeStoredProcedures { get; set; } = true;
}
