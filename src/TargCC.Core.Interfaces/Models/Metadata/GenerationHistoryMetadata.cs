using System;

namespace TargCC.Core.Interfaces.Models.Metadata;

/// <summary>
/// Metadata for generation history stored in c_GenerationHistory
/// </summary>
public class GenerationHistoryMetadata
{
    public int ID { get; set; }
    public int? TableID { get; set; }
    public string? TableName { get; set; }

    // Generation Information
    public string GenerationType { get; set; } = string.Empty; // Entity, Repository, Controller, etc.
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string? GeneratedBy { get; set; }

    // Execution
    public int DurationMs { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }

    // Context
    public string? SchemaHash { get; set; }
    public string? OutputPath { get; set; }
    public string? ToolVersion { get; set; }

    // Metadata
    public string? MetadataJson { get; set; }
}
