using Microsoft.Data.SqlClient;
using System.Text.Json;
using TargCC.WebAPI.Models;

namespace TargCC.WebAPI.Services;

/// <summary>
/// Service for managing database connections.
/// Connections are stored in a JSON file for persistence.
/// </summary>
public class ConnectionService : IConnectionService
{
    private readonly string _storageFilePath;
    private readonly ILogger<ConnectionService> _logger;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public ConnectionService(ILogger<ConnectionService> logger)
    {
        _logger = logger;
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var targccPath = Path.Combine(appDataPath, "TargCC");
        Directory.CreateDirectory(targccPath);
        _storageFilePath = Path.Combine(targccPath, "connections.json");
    }

    /// <inheritdoc/>
    public async Task<List<DatabaseConnectionInfo>> GetConnectionsAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            var connections = await LoadConnectionsFromFileAsync();
            return connections.OrderByDescending(c => c.LastUsed).ToList();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<DatabaseConnectionInfo?> GetConnectionAsync(string id)
    {
        await _fileLock.WaitAsync();
        try
        {
            var connections = await LoadConnectionsFromFileAsync();
            return connections.FirstOrDefault(c => c.Id == id);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<DatabaseConnectionInfo> AddConnectionAsync(DatabaseConnectionInfo connection)
    {
        await _fileLock.WaitAsync();
        try
        {
            connection.Id = Guid.NewGuid().ToString();
            connection.Created = DateTime.UtcNow;
            connection.LastUsed = DateTime.UtcNow;

            // Build connection string if not provided
            if (string.IsNullOrWhiteSpace(connection.ConnectionString))
            {
                connection.ConnectionString = BuildConnectionString(connection);
            }

            var connections = await LoadConnectionsFromFileAsync();
            connections.Add(connection);
            await SaveConnectionsToFileAsync(connections);

            _logger.LogInformation("Added new connection: {Name} ({Id})", connection.Name, connection.Id);
            return connection;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task UpdateConnectionAsync(DatabaseConnectionInfo connection)
    {
        await _fileLock.WaitAsync();
        try
        {
            var connections = await LoadConnectionsFromFileAsync();
            var index = connections.FindIndex(c => c.Id == connection.Id);

            if (index >= 0)
            {
                // Rebuild connection string if connection info changed
                if (string.IsNullOrWhiteSpace(connection.ConnectionString))
                {
                    connection.ConnectionString = BuildConnectionString(connection);
                }

                connections[index] = connection;
                await SaveConnectionsToFileAsync(connections);
                _logger.LogInformation("Updated connection: {Name} ({Id})", connection.Name, connection.Id);
            }
            else
            {
                _logger.LogWarning("Connection not found for update: {Id}", connection.Id);
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task DeleteConnectionAsync(string id)
    {
        await _fileLock.WaitAsync();
        try
        {
            var connections = await LoadConnectionsFromFileAsync();
            var removed = connections.RemoveAll(c => c.Id == id);
            
            if (removed > 0)
            {
                await SaveConnectionsToFileAsync(connections);
                _logger.LogInformation("Deleted connection: {Id}", id);
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> TestConnectionAsync(string connectionString)
    {
        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            _logger.LogInformation("Connection test successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Connection test failed");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task UpdateLastUsedAsync(string id)
    {
        await _fileLock.WaitAsync();
        try
        {
            var connections = await LoadConnectionsFromFileAsync();
            var connection = connections.FirstOrDefault(c => c.Id == id);
            
            if (connection != null)
            {
                connection.LastUsed = DateTime.UtcNow;
                await SaveConnectionsToFileAsync(connections);
                _logger.LogDebug("Updated last used timestamp for connection: {Id}", id);
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// Builds a SQL Server connection string from connection info.
    /// </summary>
    /// <param name="connection">Connection information.</param>
    /// <returns>Connection string.</returns>
    private static string BuildConnectionString(DatabaseConnectionInfo connection)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = connection.Server,
            InitialCatalog = connection.Database,
            TrustServerCertificate = true
        };

        if (connection.UseIntegratedSecurity)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.IntegratedSecurity = false;
            builder.UserID = connection.Username;
            // Note: Password should be handled separately in a real production app
            // For now, we'll require it to be provided via the connection string
        }

        return builder.ConnectionString;
    }

    /// <summary>
    /// Loads connections from the JSON file.
    /// </summary>
    /// <returns>List of connections.</returns>
    private async Task<List<DatabaseConnectionInfo>> LoadConnectionsFromFileAsync()
    {
        if (!File.Exists(_storageFilePath))
        {
            return new List<DatabaseConnectionInfo>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_storageFilePath);
            var connections = JsonSerializer.Deserialize<List<DatabaseConnectionInfo>>(json);
            return connections ?? new List<DatabaseConnectionInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load connections from file");
            return new List<DatabaseConnectionInfo>();
        }
    }

    /// <summary>
    /// Saves connections to the JSON file.
    /// </summary>
    /// <param name="connections">Connections to save.</param>
    private async Task SaveConnectionsToFileAsync(List<DatabaseConnectionInfo> connections)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            var json = JsonSerializer.Serialize(connections, options);
            await File.WriteAllTextAsync(_storageFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save connections to file");
            throw;
        }
    }
}
