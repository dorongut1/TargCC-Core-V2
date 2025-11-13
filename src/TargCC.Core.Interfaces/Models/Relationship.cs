namespace TargCC.Core.Interfaces.Models;

/// <summary>
/// Represents a foreign key relationship between tables
/// </summary>
public class Relationship
{
    /// <summary>
    /// Gets or sets the constraint name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent table name (referenced table)
    /// </summary>
    public string ParentTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent column name
    /// </summary>
    public string ParentColumn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the child table name (referencing table)
    /// </summary>
    public string ChildTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the child column name
    /// </summary>
    public string ChildColumn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relationship type (OneToMany, ManyToOne, etc.)
    /// </summary>
    public RelationshipType Type { get; set; }

    /// <summary>
    /// Gets or sets whether this relationship is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Defines the types of relationships between tables
/// </summary>
public enum RelationshipType
{
    /// <summary>
    /// One-to-many relationship
    /// </summary>
    OneToMany,

    /// <summary>
    /// Many-to-one relationship
    /// </summary>
    ManyToOne,

    /// <summary>
    /// One-to-one relationship
    /// </summary>
    OneToOne,

    /// <summary>
    /// Many-to-many relationship
    /// </summary>
    ManyToMany
}
