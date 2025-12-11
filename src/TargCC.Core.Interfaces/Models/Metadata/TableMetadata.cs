using System;

namespace TargCC.Core.Interfaces.Models.Metadata;

/// <summary>
/// Metadata for a database table stored in c_Table.
/// </summary>
public class TableMetadata
{
    /// <summary>
    /// Gets or sets the unique identifier for the table metadata record.
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// Gets or sets the name of the database table.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema name of the table (default: dbo).
    /// </summary>
    public string SchemaName { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets the timestamp when code was last generated for this table.
    /// </summary>
    public DateTime? LastGenerated { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the table was last modified in the database.
    /// </summary>
    public DateTime? LastModifiedInDB { get; set; }

    /// <summary>
    /// Gets or sets the current SHA256 hash of the table schema.
    /// </summary>
    public string? SchemaHash { get; set; }

    /// <summary>
    /// Gets or sets the previous SHA256 hash for change detection.
    /// </summary>
    public string? SchemaHashPrevious { get; set; }

    /// <summary>
    /// Gets or sets the audit level for this table (Legacy TARGCC compatible).
    /// </summary>
    public int CcAuditLevel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to create UI menu for this table.
    /// </summary>
    public bool CcUICreateMenu { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to create entity UI for this table.
    /// </summary>
    public bool CcUICreateEntity { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to create collection UI for this table.
    /// </summary>
    public bool CcUICreateCollection { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this table contains only a single row.
    /// </summary>
    public bool CcIsSingleRow { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this table is used for identity/authentication.
    /// </summary>
    public bool CcUsedForIdentity { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate entity class for this table.
    /// </summary>
    public bool GenerateEntity { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate repository for this table.
    /// </summary>
    public bool GenerateRepository { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate API controller for this table.
    /// </summary>
    public bool GenerateController { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate React UI components for this table.
    /// </summary>
    public bool GenerateReactUI { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate stored procedures for this table.
    /// </summary>
    public bool GenerateStoredProcedures { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate CQRS handlers for this table.
    /// </summary>
    public bool GenerateCQRS { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this is a system table.
    /// </summary>
    public bool IsSystemTable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this table metadata is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets optional notes about this table.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this metadata record was created.
    /// </summary>
    public DateTime AddedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the user who created this metadata record.
    /// </summary>
    public string? AddedBy { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this metadata record was last modified.
    /// </summary>
    public DateTime? ChangedOn { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified this metadata record.
    /// </summary>
    public string? ChangedBy { get; set; }
}
