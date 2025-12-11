using System;

namespace TargCC.Core.Interfaces.Models.Metadata;

/// <summary>
/// Metadata for table relationships stored in c_Relationship.
/// </summary>
public class RelationshipMetadata
{
    /// <summary>
    /// Gets or sets the unique identifier for the relationship metadata record.
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// Gets or sets the name of the foreign key constraint.
    /// </summary>
    public string ForeignKeyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the parent table (primary key side).
    /// </summary>
    public string ParentTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the parent column (primary key).
    /// </summary>
    public string ParentColumn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the foreign key reference to the parent table metadata.
    /// </summary>
    public int? ParentTableID { get; set; }

    /// <summary>
    /// Gets or sets the name of the child table (foreign key side).
    /// </summary>
    public string ChildTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the child column (foreign key).
    /// </summary>
    public string ChildColumn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the foreign key reference to the child table metadata.
    /// </summary>
    public int? ChildTableID { get; set; }

    /// <summary>
    /// Gets or sets the type of relationship (OneToMany, OneToOne, ManyToMany).
    /// </summary>
    public string RelationshipType { get; set; } = "OneToMany";

    /// <summary>
    /// Gets or sets a value indicating whether the foreign key allows NULL values.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Gets or sets the cascade rule on delete (NoAction, Cascade, SetNull, SetDefault).
    /// </summary>
    public string OnDelete { get; set; } = "NoAction";

    /// <summary>
    /// Gets or sets the cascade rule on update (NoAction, Cascade, SetNull, SetDefault).
    /// </summary>
    public string OnUpdate { get; set; } = "NoAction";

    /// <summary>
    /// Gets or sets the navigation property name on the parent entity.
    /// </summary>
    public string? ParentNavigationProperty { get; set; }

    /// <summary>
    /// Gets or sets the navigation property name on the child entity.
    /// </summary>
    public string? ChildNavigationProperty { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a master-detail relationship.
    /// </summary>
    public bool IsMasterDetail { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate master-detail stored procedures.
    /// </summary>
    public bool GenerateMasterDetailSP { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this relationship metadata is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets optional notes about this relationship.
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
