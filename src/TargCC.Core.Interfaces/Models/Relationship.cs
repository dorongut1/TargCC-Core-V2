namespace TargCC.Core.Interfaces.Models;

/// <summary>
/// Represents a foreign key relationship between tables.
/// </summary>
public class Relationship
{
    /// <summary>
    /// Gets or sets the constraint name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent table name (referenced table).
    /// </summary>
    public string ParentTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent column name.
    /// </summary>
    public string ParentColumn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the child table name (referencing table).
    /// </summary>
    public string ChildTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the child column name.
    /// </summary>
    public string ChildColumn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relationship type (OneToMany, ManyToOne, etc.).
    /// </summary>
    public RelationshipType Type { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this relationship is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the constraint name.
    /// </summary>
    public string ConstraintName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the referenced table.
    /// </summary>
    public string ReferencedTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the referenced column.
    /// </summary>
    public string ReferencedColumn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the delete action.
    /// </summary>
    public string DeleteAction { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the update action.
    /// </summary>
    public string UpdateAction { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this constraint is disabled.
    /// </summary>
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Gets or sets the detailed relationship type.
    /// </summary>
    public RelationshipType RelationshipType { get; set; }
}
