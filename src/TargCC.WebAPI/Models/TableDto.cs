namespace TargCC.WebAPI.Models;

/// <summary>
/// Represents a database table with metadata.
/// </summary>
public class TableDto
{
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the table schema.
    /// </summary>
    public string Schema { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets the row count.
    /// </summary>
    public int RowCount { get; set; }

    /// <summary>
    /// Gets or sets the list of columns.
    /// </summary>
    public List<ColumnDto> Columns { get; set; } = new();

    /// <summary>
    /// Gets or sets whether this table has TargCC columns.
    /// </summary>
    public bool HasTargCCColumns { get; set; }

    /// <summary>
    /// Gets or sets the generation status.
    /// </summary>
    public string GenerationStatus { get; set; } = "Not Generated";

    /// <summary>
    /// Gets or sets when this table was last generated.
    /// </summary>
    public DateTime? LastGenerated { get; set; }
}
