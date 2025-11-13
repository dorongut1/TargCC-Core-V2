using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Serilog;
using TargCC.Core.Interfaces;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Analyzers.Database;

/// <summary>
/// Analyzes database structure - reads tables, columns, indexes and relationships.
/// </summary>
public class DatabaseAnalyzer : IAnalyzer
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseAnalyzer> _logger;
    private readonly TableAnalyzer _tableAnalyzer;
    private readonly RelationshipAnalyzer _relationshipAnalyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseAnalyzer"/> class.
    /// </summary>
    /// <param name="connectionString">Connection string to the database.</param>
    /// <param name="logger">Logger for tracking operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when connectionString or logger is null.</exception>
    public DatabaseAnalyzer(string connectionString, ILogger<DatabaseAnalyzer> logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _tableAnalyzer = new TableAnalyzer(connectionString, logger);
        _relationshipAnalyzer = new RelationshipAnalyzer(connectionString, logger);

        _logger.LogInformation("DatabaseAnalyzer initialized");
    }

    /// <summary>
    /// Tests connection to database.
    /// </summary>
    /// <returns>True if connection succeeded.</returns>
    public async Task<bool> ConnectAsync()
    {
        try
        {
            _logger.LogInformation("Attempting to connect to database...");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            _logger.LogInformation("Database connection successful");
            return true;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error connecting to database. Error: {ErrorMessage}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error connecting to database");
            return false;
        }
    }

    /// <summary>
    /// Reads list of all tables in the database (excluding system tables).
    /// </summary>
    /// <returns>List of table names.</returns>
    public async Task<List<string>> GetTablesAsync()
    {
        try
        {
            _logger.LogInformation("Reading table list...");

            const string query = @"
                SELECT 
                    SCHEMA_NAME(t.schema_id) + '.' + t.name AS TableName
                FROM 
                    sys.tables t
                WHERE 
                    t.is_ms_shipped = 0
                ORDER BY 
                    SCHEMA_NAME(t.schema_id), t.name";

            await using var connection = new SqlConnection(_connectionString);
            var tables = await connection.QueryAsync<string>(query);
            var tableList = tables.ToList();

            _logger.LogInformation("Found {TableCount} tables", tableList.Count);
            return tableList;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error reading table list");
            throw new InvalidOperationException("Failed to read table list from database", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error reading table list");
            throw;
        }
    }

    /// <summary>
    /// Performs full schema analysis - tables, columns, indexes and relationships.
    /// </summary>
    /// <returns>Complete DatabaseSchema.</returns>
    public async Task<DatabaseSchema> AnalyzeAsync()
    {
        try
        {
            _logger.LogInformation("Starting full database analysis...");

            var schema = await CreateDatabaseSchemaAsync();
            var tableNames = await GetTablesAsync();

            _logger.LogInformation("Found {TableCount} tables to analyze", tableNames.Count);

            await AnalyzeTablesAsync(schema, tableNames);
            await AnalyzeRelationshipsAsync(schema);

            LogAnalysisComplete(schema);
            return schema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database analysis");
            throw new InvalidOperationException("Database analysis failed", ex);
        }
    }

    /// <summary>
    /// Performs incremental analysis - only changed tables.
    /// </summary>
    /// <param name="changedTables">List of changed tables.</param>
    /// <returns>DatabaseSchema with only changed tables.</returns>
    public async Task<DatabaseSchema> AnalyzeIncrementalAsync(List<string> changedTables)
    {
        if (changedTables == null || changedTables.Count == 0)
        {
            _logger.LogWarning("No changed tables provided for incremental analysis");
            return await CreateDatabaseSchemaAsync();
        }

        try
        {
            _logger.LogInformation("Starting incremental analysis of {TableCount} tables...", changedTables.Count);

            var schema = await CreateDatabaseSchemaAsync();
            schema.IsIncrementalAnalysis = true;

            await AnalyzeTablesAsync(schema, changedTables);
            await AnalyzeRelationshipsAsync(schema);

            LogAnalysisComplete(schema);
            return schema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during incremental analysis");
            throw new InvalidOperationException("Incremental analysis failed", ex);
        }
    }

    /// <summary>
    /// Detects which tables have changed since previous analysis.
    /// </summary>
    /// <param name="previousSchema">Previous schema for comparison.</param>
    /// <returns>List of changed tables.</returns>
    public async Task<List<string>> DetectChangedTablesAsync(DatabaseSchema previousSchema)
    {
        if (previousSchema == null)
        {
            throw new ArgumentNullException(nameof(previousSchema));
        }

        try
        {
            _logger.LogInformation("Detecting changes in database structure...");

            var changedTables = new List<string>();
            var currentTables = await GetTablesAsync();

            await DetectNewTablesAsync(previousSchema, currentTables, changedTables);
            await DetectModifiedTablesAsync(previousSchema, currentTables, changedTables);

            _logger.LogInformation("Total {ChangedCount} tables changed", changedTables.Count);
            return changedTables;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting changes");
            throw new InvalidOperationException("Failed to detect table changes", ex);
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Creates a new database schema with basic information.
    /// </summary>
    private async Task<DatabaseSchema> CreateDatabaseSchemaAsync()
    {
        return new DatabaseSchema
        {
            DatabaseName = await GetDatabaseNameAsync(),
            ServerName = await GetServerNameAsync(),
            AnalysisDate = DateTime.UtcNow,
            Tables = new List<Table>(),
            Relationships = new List<Relationship>()
        };
    }

    /// <summary>
    /// Analyzes all specified tables and adds them to schema.
    /// </summary>
    private async Task AnalyzeTablesAsync(DatabaseSchema schema, List<string> tableNames)
    {
        foreach (var tableName in tableNames)
        {
            try
            {
                _logger.LogDebug("Analyzing table: {TableName}", tableName);
                var table = await _tableAnalyzer.AnalyzeTableAsync(tableName);
                schema.Tables.Add(table);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to analyze table {TableName}, skipping", tableName);
            }
        }
    }

    /// <summary>
    /// Analyzes relationships between tables.
    /// </summary>
    private async Task AnalyzeRelationshipsAsync(DatabaseSchema schema)
    {
        _logger.LogInformation("Analyzing table relationships...");
        schema.Relationships = await _relationshipAnalyzer.AnalyzeRelationshipsAsync(schema.Tables);
    }

    /// <summary>
    /// Detects new tables that don't exist in previous schema.
    /// </summary>
    private async Task DetectNewTablesAsync(
        DatabaseSchema previousSchema,
        List<string> currentTables,
        List<string> changedTables)
    {
        var previousTableNames = previousSchema.Tables.Select(t => t.FullName).ToHashSet();
        var newTables = currentTables.Where(t => !previousTableNames.Contains(t)).ToList();

        changedTables.AddRange(newTables);
        _logger.LogInformation("Found {NewTableCount} new tables", newTables.Count);
    }

    /// <summary>
    /// Detects existing tables that were modified.
    /// </summary>
    private async Task DetectModifiedTablesAsync(
        DatabaseSchema previousSchema,
        List<string> currentTables,
        List<string> changedTables)
    {
        var previousTableNames = previousSchema.Tables.Select(t => t.FullName).ToHashSet();
        var existingTables = currentTables.Where(t => previousTableNames.Contains(t));

        foreach (var tableName in existingTables)
        {
            var hasChanged = await HasTableChangedAsync(tableName, previousSchema);
            if (hasChanged)
            {
                changedTables.Add(tableName);
            }
        }
    }

    /// <summary>
    /// Logs completion of analysis with summary.
    /// </summary>
    private void LogAnalysisComplete(DatabaseSchema schema)
    {
        _logger.LogInformation(
            "Analysis complete: {TableCount} tables, {RelationshipCount} relationships",
            schema.Tables.Count,
            schema.Relationships.Count);
    }

    /// <summary>
    /// Gets the database name.
    /// </summary>
    private async Task<string> GetDatabaseNameAsync()
    {
        const string query = "SELECT DB_NAME()";
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleAsync<string>(query);
    }

    /// <summary>
    /// Gets the server name.
    /// </summary>
    private async Task<string> GetServerNameAsync()
    {
        const string query = "SELECT @@SERVERNAME";
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleAsync<string>(query);
    }

    /// <summary>
    /// Checks if a table has changed since previous analysis.
    /// </summary>
    private async Task<bool> HasTableChangedAsync(string tableName, DatabaseSchema previousSchema)
    {
        try
        {
            var previousTable = previousSchema.Tables.FirstOrDefault(t => t.FullName == tableName);
            if (previousTable == null)
            {
                return true; // New table
            }

            const string query = @"
                SELECT modify_date 
                FROM sys.tables 
                WHERE SCHEMA_NAME(schema_id) + '.' + name = @TableName";

            await using var connection = new SqlConnection(_connectionString);
            var modifyDate = await connection.QuerySingleOrDefaultAsync<DateTime?>(
                query,
                new { TableName = tableName });

            if (!modifyDate.HasValue)
            {
                return false;
            }

            return modifyDate.Value > previousSchema.AnalysisDate;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking changes for table {TableName}, assuming changed", tableName);
            return true; // In case of doubt, return that table changed
        }
    }

    #endregion

    #region IAnalyzer Implementation

    /// <inheritdoc/>
    string IAnalyzer.Name => "Database Analyzer";

    /// <inheritdoc/>
    string IAnalyzer.Version => "1.0.0";

    /// <inheritdoc/>
    async Task<object> IAnalyzer.AnalyzeAsync(object input, CancellationToken cancellationToken = default)
    {
        return await AnalyzeAsync();
    }

    #endregion
}
