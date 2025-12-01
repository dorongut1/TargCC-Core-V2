namespace TargCC.WebAPI.Models;

/// <summary>
/// Represents a complete database schema.
/// </summary>
public class DatabaseSchemaDto
{
    /// <summary>
    /// Gets or sets the list of tables.
    /// </summary>
    public List<TableDto> Tables { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of relationships.
    /// </summary>
    public List<RelationshipDto> Relationships { get; set; } = new();
}
