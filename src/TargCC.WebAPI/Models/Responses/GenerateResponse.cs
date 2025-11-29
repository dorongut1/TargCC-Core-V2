// <copyright file="GenerateResponse.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.WebAPI.Models.Responses;

/// <summary>
/// Response model for code generation operations.
/// </summary>
public sealed class GenerateResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether generation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the result message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of generated files.
    /// </summary>
    public List<string> GeneratedFiles { get; set; } = new();

    /// <summary>
    /// Gets or sets any errors that occurred.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets the generation statistics.
    /// </summary>
    public GenerationStats? Stats { get; set; }
}

/// <summary>
/// Statistics about code generation.
/// </summary>
public sealed class GenerationStats
{
    /// <summary>
    /// Gets or sets the number of tables processed.
    /// </summary>
    public int TablesProcessed { get; set; }

    /// <summary>
    /// Gets or sets the number of files generated.
    /// </summary>
    public int FilesGenerated { get; set; }

    /// <summary>
    /// Gets or sets the generation duration in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }
}
