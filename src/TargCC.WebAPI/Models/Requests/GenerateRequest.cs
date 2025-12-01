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
    /// Gets or sets a value indicating whether to force regeneration (overwrite existing files).
    /// </summary>
    public bool Force { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate entity classes.
    /// </summary>
    public bool GenerateEntity { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate repository interfaces and implementations.
    /// </summary>
    public bool GenerateRepository { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate service layer.
    /// </summary>
    public bool GenerateService { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to generate API controllers.
    /// </summary>
    public bool GenerateController { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate unit tests.
    /// </summary>
    public bool GenerateTests { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to include stored procedures.
    /// </summary>
    public bool IncludeStoredProcedures { get; set; } = true;
}
