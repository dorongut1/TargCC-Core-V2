namespace TargCC.Core.Interfaces.Models;

/// <summary>
/// Represents a foreign key relationship between two database tables.
/// </summary>
/// <remarks>
/// <para>
/// Relationships define how tables are connected through foreign keys and are critical for
/// generating navigation methods and properties in the generated code.
/// </para>
/// <para>
/// <strong>Generated Code for Relationships:</strong>
/// </para>
/// <list type="table">
/// <listheader><term>Relationship Direction</term><description>Generated Methods</description></listheader>
/// <item><term>Parent → Child (1:N)</term><description>Parent.FillChildren(), Parent.Children collection</description></item>
/// <item><term>Child → Parent (N:1)</term><description>Child.LoadParent(), Child.Parent property</description></item>
/// <item><term>One-to-One</term><description>Both directions with Load methods</description></item>
/// </list>
/// <para>
/// <strong>Cascading Actions:</strong>
/// </para>
/// <list type="bullet">
/// <item>NO ACTION - Prevents deletion/update if children exist</item>
/// <item>CASCADE - Deletes/updates children automatically</item>
/// <item>SET NULL - Sets foreign key to NULL</item>
/// <item>SET DEFAULT - Sets foreign key to default value</item>
/// </list>
/// </remarks>
/// <example>
/// <para><strong>Example 1: One-to-Many (Customer → Orders)</strong></para>
/// <code>
/// var relationship = new Relationship
/// {
///     Name = "FK_Order_Customer",
///     ParentTable = "Customer",
///     ParentColumn = "ID",
///     ChildTable = "Order",
///     ChildColumn = "CustomerID",
///     Type = RelationshipType.OneToMany,
///     DeleteAction = "NO ACTION",
///     UpdateAction = "CASCADE"
/// };
///
/// // Generates in clsCustomer:
/// //   public clsFault FillOrders()
/// //   public colOrders Orders { get; }
/// //
/// // Generates in clsOrder:
/// //   public clsFault LoadCustomer()
/// //   public clsCustomer Customer { get; }
/// </code>
/// </example>
/// <example>
/// <para><strong>Example 2: Disabled relationship (still used for code generation)</strong></para>
/// <code>
/// var disabledRelationship = new Relationship
/// {
///     Name = "FK_Order_Branch",
///     ParentTable = "Branch",
///     ChildTable = "Order",
///     IsDisabled = true,  // Constraint disabled in SQL
///     IsEnabled = true    // Still used by TargCC for code generation
/// };
///
/// // Even though SQL constraint is disabled, TargCC still generates:
/// // - clsOrder.LoadBranch() method
/// // - clsBranch.FillOrders() method
/// // This is useful when data integrity is enforced by application logic
/// </code>
/// </example>
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
