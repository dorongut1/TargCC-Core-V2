namespace TargCC.WebAPI.Models;

/// <summary>
/// Represents a relationship between tables.
/// </summary>
public class RelationshipDto
{
    /// <summary>
    /// Gets or sets the source table name.
    /// </summary>
    public string FromTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source column name.
    /// </summary>
    public string FromColumn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target table name.
    /// </summary>
    public string ToTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target column name.
    /// </summary>
    public string ToColumn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relationship type.
    /// </summary>
    public string Type { get; set; } = "one-to-many";
}
