using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Serilog;
using TargCC.Core.Interfaces;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Analyzers.Database;

/// <summary>
/// Analyzes SQL Server database structure including tables, columns, indexes, relationships, and metadata.
/// Supports both full analysis and incremental change detection for efficient code generation.
/// </summary>
/// <remarks>
/// This analyzer is the core component for database schema introspection. It uses Dapper for efficient
/// SQL queries and provides both full and incremental analysis modes:
/// <list type="bullet">
/// <item>Full Analysis: Reads entire database schema</item>
/// <item>Incremental Analysis: Only analyzes changed tables (faster)</item>
/// <item>Change Detection: Compares current schema with previous analysis</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var analyzer = new DatabaseAnalyzer(connectionString, logger);
///
/// // Full analysis
/// var schema = await analyzer.AnalyzeAsync();
/// Console.WriteLine($"Found {schema.Tables.Count} tables");
/// 
/// // Incremental analysis
/// var changes = await analyzer.DetectChangedTablesAsync(previousSchema);
/// var incrementalSchema = await analyzer.AnalyzeIncrementalAsync(changes);
/// </code>
/// </example>
public class DatabaseAnalyzer : IAnalyzer
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseAnalyzer> _logger;
    private readonly TableAnalyzer _tableAnalyzer;
    private readonly RelationshipAnalyzer _relationshipAnalyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseAnalyzer"/> class.
    /// </summary>
    /// <param name="connectionString">SQL Server connection string. Must include database name and credentials.
    /// Example: "Server=localhost;Database=MyDb;Integrated Security=true;"</param>
    /// <param name="logger">Logger instance for tracking operations and diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionString"/> or <paramref name="logger"/> is null.</exception>
    /// <remarks>
    /// The constructor also initializes child analyzers (TableAnalyzer and RelationshipAnalyzer)
    /// that are used internally for detailed analysis.
    /// </remarks>
    public DatabaseAnalyzer(string connectionString, ILogger<DatabaseAnalyzer> logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _tableAnalyzer = new TableAnalyzer(connectionString, logger);
        _relationshipAnalyzer = new RelationshipAnalyzer(connectionString, logger);

        _logger.LogInformation("DatabaseAnalyzer initialized");
    }

    /// <summary>
    /// Tests the connection to the SQL Server database.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// <c>true</c> if the connection was successful; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is useful for validating connection strings before performing full analysis.
    /// It catches and logs both SQL-specific errors and general exceptions.
    /// No exception is thrown - errors are logged and false is returned.
    /// </remarks>
    /// <example>
    /// <code>
    /// var analyzer = new DatabaseAnalyzer(connectionString, logger);
    /// if (await analyzer.ConnectAsync())
    /// {
    ///     Console.WriteLine("Connected successfully!");
    ///     var schema = await analyzer.AnalyzeAsync();
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Connection failed - check logs");
    /// }
    /// </code>
    /// </example>
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
    /// Retrieves a list of all user tables in the database, excluding system tables.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a list of fully-qualified table names in the format "SchemaName.TableName" (e.g., "dbo.Customer").
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the query fails to execute or database is inaccessible.
    /// </exception>
    /// <remarks>
    /// <para>This method queries sys.tables to retrieve user-defined tables only (is_ms_shipped = 0).</para>
    /// <para>Table names are returned in alphabetical order by schema and then table name.</para>
    /// <para>System tables and internal SQL Server tables are automatically excluded.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var analyzer = new DatabaseAnalyzer(connectionString, logger);
    /// var tables = await analyzer.GetTablesAsync();
    /// 
    /// foreach (var table in tables)
    /// {
    ///     Console.WriteLine($"Table: {table}");
    /// }
    /// // Output:
    /// // Table: dbo.Customer
    /// // Table: dbo.Order
    /// // Table: sales.Invoice
    /// </code>
    /// </example>
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
    /// Performs a complete database schema analysis including all tables, columns, indexes, and relationships.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a complete <see cref="DatabaseSchema"/> object with all analyzed metadata.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the analysis fails due to database connection issues or query errors.
    /// </exception>
    /// <remarks>
    /// <para><strong>This is the primary method for full database analysis.</strong></para>
    /// <para>The analysis process includes:</para>
    /// <list type="number">
    /// <item>Creating database schema metadata (name, server, timestamp)</item>
    /// <item>Retrieving all user tables</item>
    /// <item>Analyzing each table's structure (columns, indexes, keys)</item>
    /// <item>Detecting relationships (foreign keys)</item>
    /// <item>Building relationship graph</item>
    /// </list>
    /// <para><strong>Performance Note:</strong> For large databases, consider using <see cref="AnalyzeIncrementalAsync"/> 
    /// for faster analysis of only changed tables.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var analyzer = new DatabaseAnalyzer(connectionString, logger);
    /// var schema = await analyzer.AnalyzeAsync();
    /// 
    /// Console.WriteLine($"Database: {schema.DatabaseName}");
    /// Console.WriteLine($"Server: {schema.ServerName}");
    /// Console.WriteLine($"Tables: {schema.Tables.Count}");
    /// Console.WriteLine($"Relationships: {schema.Relationships.Count}");
    /// 
    /// // Access specific table
    /// var customerTable = schema.Tables.FirstOrDefault(t => t.Name == "Customer");
    /// if (customerTable != null)
    /// {
    ///     Console.WriteLine($"Columns in Customer: {customerTable.Columns.Count}");
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="AnalyzeIncrementalAsync"/>
    /// <seealso cref="DetectChangedTablesAsync"/>
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
    /// Performs incremental database analysis on only the specified changed tables.
    /// This is significantly faster than full analysis for large databases.
    /// </summary>
    /// <param name="changedTables">List of fully-qualified table names that have changed (e.g., "dbo.Customer").
    /// Can be obtained from <see cref="DetectChangedTablesAsync"/>.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a <see cref="DatabaseSchema"/> with only the analyzed changed tables.
    /// The <see cref="DatabaseSchema.IsIncrementalAnalysis"/> property is set to <c>true</c>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the analysis fails due to database connection issues or query errors.
    /// </exception>
    /// <remarks>
    /// <para><strong>⚡ Performance Optimization:</strong> This method analyzes only specified tables,
    /// making it ideal for incremental code generation where only changed tables need updating.</para>
    /// <para>If <paramref name="changedTables"/> is null or empty, returns an empty schema.</para>
    /// <para><strong>Use Case:</strong> After detecting changes with <see cref="DetectChangedTablesAsync"/>,
    /// use this method to analyze only those tables instead of the entire database.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var analyzer = new DatabaseAnalyzer(connectionString, logger);
    /// 
    /// // First, detect what changed
    /// var changedTables = await analyzer.DetectChangedTablesAsync(previousSchema);
    /// Console.WriteLine($"Found {changedTables.Count} changed tables");
    /// 
    /// // Then analyze only the changes (fast!)
    /// var incrementalSchema = await analyzer.AnalyzeIncrementalAsync(changedTables);
    /// 
    /// // Generate code only for changed tables
    /// foreach (var table in incrementalSchema.Tables)
    /// {
    ///     Console.WriteLine($"Regenerating code for: {table.Name}");
    ///     // GenerateCode(table);
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="DetectChangedTablesAsync"/>
    /// <seealso cref="AnalyzeAsync"/>
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
    /// Detects which tables have been added, removed, or modified since the previous analysis.
    /// This is the core method for incremental code generation.
    /// </summary>
    /// <param name="previousSchema">The previous database schema to compare against.
    /// Must not be null and should contain the timestamp of the previous analysis.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a list of fully-qualified table names (e.g., "dbo.Customer") that have changed.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="previousSchema"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when change detection fails due to database errors.
    /// </exception>
    /// <remarks>
    /// <para><strong>🎯 This is the KEY method for the incremental generation feature!</strong></para>
    /// <para>Change detection includes:</para>
    /// <list type="bullet">
    /// <item><strong>New Tables:</strong> Tables that exist now but didn't before</item>
    /// <item><strong>Modified Tables:</strong> Tables whose modify_date is after the previous analysis</item>
    /// <item><strong>Removed Tables:</strong> Not included (handled separately by code generator)</item>
    /// </list>
    /// <para><strong>How it works:</strong></para>
    /// <list type="number">
    /// <item>Compares current table list with previous schema</item>
    /// <item>Identifies new tables not in previous schema</item>
    /// <item>For existing tables, checks SQL Server modify_date against previous analysis timestamp</item>
    /// <item>Returns combined list of new + modified tables</item>
    /// </list>
    /// <para><strong>Important:</strong> This method compares metadata timestamps, not actual data.
    /// Schema changes (columns, indexes, keys) are detected, but data-only changes are not.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var analyzer = new DatabaseAnalyzer(connectionString, logger);
    /// 
    /// // Load previous schema (from file, cache, etc.)
    /// var previousSchema = LoadPreviousSchema();
    /// Console.WriteLine($"Previous analysis: {previousSchema.AnalysisDate}");
    /// 
    /// // Detect changes
    /// var changedTables = await analyzer.DetectChangedTablesAsync(previousSchema);
    /// 
    /// if (changedTables.Count == 0)
    /// {
    ///     Console.WriteLine("No changes detected!");
    ///     return;
    /// }
    /// 
    /// Console.WriteLine($"Changed tables: {changedTables.Count}");
    /// foreach (var table in changedTables)
    /// {
    ///     Console.WriteLine($"  - {table}");
    /// }
    /// 
    /// // Now do incremental analysis
    /// var incrementalSchema = await analyzer.AnalyzeIncrementalAsync(changedTables);
    /// </code>
    /// </example>
    /// <seealso cref="AnalyzeIncrementalAsync"/>
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
