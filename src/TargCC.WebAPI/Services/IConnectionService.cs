using TargCC.WebAPI.Models;

namespace TargCC.WebAPI.Services;

/// <summary>
/// Service for managing database connections.
/// </summary>
public interface IConnectionService
{
    /// <summary>
    /// Gets all saved connections.
    /// </summary>
    /// <returns>List of connection information.</returns>
    Task<List<DatabaseConnectionInfo>> GetConnectionsAsync();

    /// <summary>
    /// Gets a specific connection by ID.
    /// </summary>
    /// <param name="id">Connection ID.</param>
    /// <returns>Connection information or null if not found.</returns>
    Task<DatabaseConnectionInfo?> GetConnectionAsync(string id);

    /// <summary>
    /// Adds a new connection.
    /// </summary>
    /// <param name="connection">Connection to add.</param>
    /// <returns>The added connection with generated ID.</returns>
    Task<DatabaseConnectionInfo> AddConnectionAsync(DatabaseConnectionInfo connection);

    /// <summary>
    /// Updates an existing connection.
    /// </summary>
    /// <param name="connection">Connection to update.</param>
    Task UpdateConnectionAsync(DatabaseConnectionInfo connection);

    /// <summary>
    /// Deletes a connection.
    /// </summary>
    /// <param name="id">Connection ID to delete.</param>
    Task DeleteConnectionAsync(string id);

    /// <summary>
    /// Tests if a connection string is valid.
    /// </summary>
    /// <param name="connectionString">Connection string to test.</param>
    /// <returns>True if connection successful, false otherwise.</returns>
    Task<bool> TestConnectionAsync(string connectionString);

    /// <summary>
    /// Updates the last used timestamp for a connection.
    /// </summary>
    /// <param name="id">Connection ID.</param>
    Task UpdateLastUsedAsync(string id);
}
