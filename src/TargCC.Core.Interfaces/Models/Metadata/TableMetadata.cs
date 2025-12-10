using System;

namespace TargCC.Core.Interfaces.Models.Metadata;

/// <summary>
/// Metadata for a database table stored in c_Table
/// </summary>
public class TableMetadata
{
    public int ID { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = "dbo";

    // Tracking & Change Detection
    public DateTime? LastGenerated { get; set; }
    public DateTime? LastModifiedInDB { get; set; }
    public string? SchemaHash { get; set; }
    public string? SchemaHashPrevious { get; set; }

    // Metadata (Legacy compatible)
    public int CcAuditLevel { get; set; }
    public bool CcUICreateMenu { get; set; } = true;
    public bool CcUICreateEntity { get; set; } = true;
    public bool CcUICreateCollection { get; set; } = true;
    public bool CcIsSingleRow { get; set; }
    public bool CcUsedForIdentity { get; set; }

    // Generation Options
    public bool GenerateEntity { get; set; } = true;
    public bool GenerateRepository { get; set; } = true;
    public bool GenerateController { get; set; } = true;
    public bool GenerateReactUI { get; set; }
    public bool GenerateStoredProcedures { get; set; } = true;
    public bool GenerateCQRS { get; set; } = true;

    // System
    public bool IsSystemTable { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Audit
    public DateTime AddedOn { get; set; } = DateTime.UtcNow;
    public string? AddedBy { get; set; }
    public DateTime? ChangedOn { get; set; }
    public string? ChangedBy { get; set; }
}
