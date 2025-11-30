namespace TargCC.WebAPI.Models;

/// <summary>
/// Represents a database connection configuration.
/// </summary>
public class DatabaseConnectionInfo
{
    /// <summary>
    /// Gets or sets the unique identifier for this connection.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the friendly name for this connection.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the server address.
    /// </summary>
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    public string Database { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this connection was last used.
    /// </summary>
    public DateTime LastUsed { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when this connection was created.
    /// </summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether this connection uses integrated security.
    /// </summary>
    public bool UseIntegratedSecurity { get; set; } = true;

    /// <summary>
    /// Gets or sets the username (if not using integrated security).
    /// </summary>
    public string? Username { get; set; }
}
