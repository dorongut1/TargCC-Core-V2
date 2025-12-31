namespace TargCC.Core.Interfaces.Models;

/// <summary>
/// Represents a database VIEW with metadata for code generation.
/// </summary>
public class ViewInfo
{
    /// <summary>
    /// Gets or sets the view name.
    /// </summary>
    public string ViewName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema name.
    /// </summary>
    public string SchemaName { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets the view type based on naming convention.
    /// </summary>
    public ViewType Type { get; set; }

    /// <summary>
    /// Gets or sets the collection of view columns.
    /// </summary>
    public List<ViewColumn> Columns { get; set; } = new ();

    /// <summary>
    /// Gets the fully qualified view name.
    /// </summary>
    public string FullName => $"{SchemaName}.{ViewName}";
}

/// <summary>
/// Represents a column in a database view.
/// </summary>
public class ViewColumn
{
    /// <summary>
    /// Gets or sets the column name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SQL data type.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum length for string types.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column allows NULL.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Gets or sets the ordinal position of the column.
    /// </summary>
    public int OrdinalPosition { get; set; }
}

/// <summary>
/// Defines the type of view based on naming convention.
/// </summary>
public enum ViewType
{
    /// <summary>
    /// Manual view created by user (starts with "MN" or "mn").
    /// Should generate read-only report screens.
    /// </summary>
    Manual,

    /// <summary>
    /// Auto-generated ComboList view (starts with "ccvwComboList_").
    /// Used for dropdowns, no UI screens needed.
    /// </summary>
    ComboList,

    /// <summary>
    /// Other view that doesn't match known patterns.
    /// Skip for now.
    /// </summary>
    Other
}
