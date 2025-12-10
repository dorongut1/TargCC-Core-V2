using System;

namespace TargCC.Core.Interfaces.Models.Metadata;

/// <summary>
/// Metadata for table relationships stored in c_Relationship
/// </summary>
public class RelationshipMetadata
{
    public int ID { get; set; }
    public string ForeignKeyName { get; set; } = string.Empty;

    // Parent (Primary Key side)
    public string ParentTable { get; set; } = string.Empty;
    public string ParentColumn { get; set; } = string.Empty;
    public int? ParentTableID { get; set; }

    // Child (Foreign Key side)
    public string ChildTable { get; set; } = string.Empty;
    public string ChildColumn { get; set; } = string.Empty;
    public int? ChildTableID { get; set; }

    // Relationship Type
    public string RelationshipType { get; set; } = "OneToMany"; // OneToMany, OneToOne, ManyToMany
    public bool IsNullable { get; set; }

    // Cascade Rules
    public string OnDelete { get; set; } = "NoAction"; // NoAction, Cascade, SetNull, SetDefault
    public string OnUpdate { get; set; } = "NoAction";

    // Navigation Properties
    public string? ParentNavigationProperty { get; set; }
    public string? ChildNavigationProperty { get; set; }

    // Master-Detail Support
    public bool IsMasterDetail { get; set; }
    public bool GenerateMasterDetailSP { get; set; } = true;

    // System
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Audit
    public DateTime AddedOn { get; set; } = DateTime.UtcNow;
    public string? AddedBy { get; set; }
    public DateTime? ChangedOn { get; set; }
    public string? ChangedBy { get; set; }
}
