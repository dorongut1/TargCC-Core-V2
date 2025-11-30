namespace TargCC.WebAPI.Models;

/// <summary>
/// Data transfer object for table preview information.
/// </summary>
public class TablePreviewDto
{
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the column names.
    /// </summary>
    public List<string> Columns { get; set; } = new();

    /// <summary>
    /// Gets or sets the preview data rows.
    /// </summary>
    public List<Dictionary<string, object?>> Data { get; set; } = new();

    /// <summary>
    /// Gets or sets the total row count in the table.
    /// </summary>
    public int TotalRowCount { get; set; }
}
