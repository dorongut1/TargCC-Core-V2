namespace TargCC.Core.Interfaces.Models;

/// <summary>
/// Column prefix types in TargCC.
/// </summary>
public enum ColumnPrefix
{
    /// <summary>
    /// No special prefix.
    /// </summary>
    None = 0,

    /// <summary>
    /// eno - One-way encryption (SHA256 hashing).
    /// </summary>
    OneWayEncryption = 1,

    /// <summary>
    /// ent - Two-way encryption.
    /// </summary>
    TwoWayEncryption = 2,

    /// <summary>
    /// enm - Enumeration (from c_Enumeration table).
    /// </summary>
    Enumeration = 3,

    /// <summary>
    /// lkp - Lookup (from c_Lookup table).
    /// </summary>
    Lookup = 4,

    /// <summary>
    /// loc - Localization (from c_ObjectToTranslate table).
    /// </summary>
    Localization = 5,

    /// <summary>
    /// clc_ - Calculated field (read-only).
    /// </summary>
    Calculated = 6,

    /// <summary>
    /// blg_ - Business logic field (server-side only).
    /// </summary>
    BusinessLogic = 7,

    /// <summary>
    /// agg_ - Aggregate field (updated via UpdateAggregates).
    /// </summary>
    Aggregate = 8,

    /// <summary>
    /// spt_ - Separately updated field (different permissions).
    /// </summary>
    SeparateUpdate = 9,

    /// <summary>
    /// spl_ - Separate list (NewLine delimited).
    /// </summary>
    SeparateList = 10,

    /// <summary>
    /// upl_ - Upload field (file upload to server).
    /// </summary>
    Upload = 11,

    /// <summary>
    /// fui_ - Fake Unique Index (for NULLable unique fields).
    /// </summary>
    FakeUniqueIndex = 12
}

/// <summary>
/// Relationship types between tables.
/// </summary>
public enum RelationshipType
{
    /// <summary>
    /// One-to-Many (1:N) - most common type.
    /// </summary>
    OneToMany = 1,

    /// <summary>
    /// One-to-One (1:1) - FK with Unique Index.
    /// </summary>
    OneToOne = 2,

    /// <summary>
    /// Many-to-Many (N:M) - via junction table.
    /// </summary>
    ManyToMany = 3,

    /// <summary>
    /// Many-to-one (N:1) - via junction table.
    /// </summary>
    ManyToOne = 4
}
