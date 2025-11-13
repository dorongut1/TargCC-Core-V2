using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Analyzers.Database
{
    /// <summary>
    /// Analyzes individual table structure including columns, indexes, and keys.
    /// </summary>
    public class TableAnalyzer
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;
        private readonly ColumnAnalyzer _columnAnalyzer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableAnalyzer"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string to the database.</param>
        /// <param name="logger">Logger for tracking operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when connectionString or logger is null.</exception>
        public TableAnalyzer(string connectionString, ILogger logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _columnAnalyzer = new ColumnAnalyzer(connectionString, logger);
        }

        /// <summary>
        /// Analyzes a complete table structure including columns, primary key, indexes, and extended properties.
        /// </summary>
        /// <param name="tableName">Fully qualified table name (schema.table or just table name).</param>
        /// <returns>A <see cref="Table"/> object with complete structure information.</returns>
        /// <exception cref="ArgumentNullException">Thrown when tableName is null.</exception>
        /// <exception cref="SqlException">Thrown when database operation fails.</exception>
        public async Task<Table> AnalyzeTableAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            try
            {
                _logger.LogDebug("Starting table analysis for {TableName}", tableName);

                var table = await CreateTableStructureAsync(tableName);
                await PopulateTableDataAsync(table);
                
                LogAnalysisComplete(table);
                return table;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error analyzing table {TableName}", tableName);
                throw new InvalidOperationException($"Failed to analyze table '{tableName}'", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error analyzing table {TableName}", tableName);
                throw;
            }
        }

        /// <summary>
        /// Creates the initial table structure from table name.
        /// </summary>
        /// <param name="tableName">Fully qualified or simple table name.</param>
        /// <returns>Table object with schema and name set.</returns>
        private async Task<Table> CreateTableStructureAsync(string tableName)
        {
            var (schemaName, tableNameOnly) = ParseTableName(tableName);

            var table = new Table
            {
                SchemaName = schemaName,
                Name = tableNameOnly,
                Columns = new List<Column>(),
                Indexes = new List<TargCC.Core.Interfaces.Models.Index>()
            };

            await LoadTableInfoAsync(table);
            return table;
        }

        /// <summary>
        /// Populates all table data including columns, keys, and indexes.
        /// </summary>
        /// <param name="table">Table to populate.</param>
        private async Task PopulateTableDataAsync(Table table)
        {
            // Load columns
            table.Columns = await _columnAnalyzer.AnalyzeColumnsAsync(table.SchemaName, table.Name);

            // Load primary key and mark columns
            await LoadPrimaryKeyAsync(table);

            // Load all indexes
            await LoadIndexesAsync(table);

            // Load extended properties
            await LoadExtendedPropertiesAsync(table);
        }

        /// <summary>
        /// Parses a table name into schema and table components.
        /// </summary>
        /// <param name="tableName">Fully qualified or simple table name.</param>
        /// <returns>Tuple of (schemaName, tableName).</returns>
        private static (string schemaName, string tableName) ParseTableName(string tableName)
        {
            var parts = tableName.Split('.');
            var schemaName = parts.Length > 1 ? parts[0] : "dbo";
            var tableNameOnly = parts.Length > 1 ? parts[1] : parts[0];
            
            return (schemaName, tableNameOnly);
        }

        /// <summary>
        /// Loads basic table information including object ID and description.
        /// </summary>
        /// <param name="table">Table to load information into.</param>
        private async Task LoadTableInfoAsync(Table table)
        {
            const string query = @"
                SELECT 
                    t.object_id AS ObjectId,
                    t.create_date AS CreateDate,
                    t.modify_date AS ModifyDate,
                    CAST(ep.value AS NVARCHAR(4000)) AS Description
                FROM 
                    sys.tables t
                    LEFT JOIN sys.extended_properties ep 
                        ON ep.major_id = t.object_id 
                        AND ep.minor_id = 0 
                        AND ep.name = 'MS_Description'
                WHERE 
                    SCHEMA_NAME(t.schema_id) = @SchemaName
                    AND t.name = @TableName";

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                var info = await connection.QuerySingleOrDefaultAsync<dynamic>(
                    query,
                    new { SchemaName = table.SchemaName, TableName = table.Name });

                if (info != null)
                {
                    table.ObjectId = info.ObjectId;
                    table.CreateDate = info.CreateDate;
                    table.ModifyDate = info.ModifyDate;
                    table.Description = info.Description;
                    
                    _logger.LogDebug("Loaded table info for {Schema}.{Table}, ObjectId: {ObjectId}", 
                        table.SchemaName, table.Name, table.ObjectId);
                }
                else
                {
                    _logger.LogWarning("Table {Schema}.{Table} not found in database", 
                        table.SchemaName, table.Name);
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Failed to load table info for {Schema}.{Table}", 
                    table.SchemaName, table.Name);
                throw;
            }
        }

        /// <summary>
        /// Identifies and loads the primary key columns of the table.
        /// </summary>
        /// <param name="table">Table to load primary key into.</param>
        private async Task LoadPrimaryKeyAsync(Table table)
        {
            const string query = @"
                SELECT 
                    c.name AS ColumnName
                FROM 
                    sys.indexes i
                    INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                    INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                WHERE 
                    i.object_id = @ObjectId
                    AND i.is_primary_key = 1
                ORDER BY 
                    ic.key_ordinal";

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                var pkColumns = await connection.QueryAsync<string>(
                    query,
                    new { ObjectId = table.ObjectId });

                table.PrimaryKeyColumns = pkColumns.ToList();
                
                MarkPrimaryKeyColumns(table);
                
                _logger.LogDebug("Found {Count} primary key columns for {Schema}.{Table}", 
                    table.PrimaryKeyColumns.Count, table.SchemaName, table.Name);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Failed to load primary key for {Schema}.{Table}", 
                    table.SchemaName, table.Name);
                throw;
            }
        }

        /// <summary>
        /// Marks columns that are part of the primary key.
        /// </summary>
        /// <param name="table">Table with columns and primary key list.</param>
        private static void MarkPrimaryKeyColumns(Table table)
        {
            foreach (var pkColumn in table.PrimaryKeyColumns)
            {
                var column = table.Columns.FirstOrDefault(c => c.Name == pkColumn);
                if (column != null)
                {
                    column.IsPrimaryKey = true;
                }
            }
        }

        /// <summary>
        /// Loads all indexes defined on the table.
        /// </summary>
        /// <param name="table">Table to load indexes into.</param>
        private async Task LoadIndexesAsync(Table table)
        {
            const string query = @"
                SELECT 
                    i.name AS IndexName,
                    i.is_unique AS IsUnique,
                    i.is_primary_key AS IsPrimaryKey,
                    i.type_desc AS TypeDescription,
                    STRING_AGG(c.name, ',') WITHIN GROUP (ORDER BY ic.key_ordinal) AS ColumnNames
                FROM 
                    sys.indexes i
                    INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                    INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                WHERE 
                    i.object_id = @ObjectId
                    AND i.type > 0  -- Exclude heap
                GROUP BY
                    i.name, i.is_unique, i.is_primary_key, i.type_desc
                ORDER BY
                    i.is_primary_key DESC, i.is_unique DESC, i.name";

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                var indexData = await connection.QueryAsync<dynamic>(
                    query,
                    new { ObjectId = table.ObjectId });

                foreach (var idx in indexData)
                {
                    var index = CreateIndexFromData(idx);
                    table.Indexes.Add(index);
                }

                _logger.LogDebug("Found {Count} indexes for {Schema}.{Table}", 
                    table.Indexes.Count, table.SchemaName, table.Name);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Failed to load indexes for {Schema}.{Table}", 
                    table.SchemaName, table.Name);
                throw;
            }
        }

        /// <summary>
        /// Creates an Index object from dynamic database result.
        /// </summary>
        /// <param name="data">Dynamic object from database query.</param>
        /// <returns>Index object.</returns>
        private static TargCC.Core.Interfaces.Models.Index CreateIndexFromData(dynamic data)
        {
            return new TargCC.Core.Interfaces.Models.Index
            {
                Name = data.IndexName,
                IsUnique = data.IsUnique,
                IsPrimaryKey = data.IsPrimaryKey,
                TypeDescription = data.TypeDescription,
                ColumnNames = ((string)data.ColumnNames).Split(',').ToList()
            };
        }

        /// <summary>
        /// Loads extended properties defined on the table.
        /// </summary>
        /// <param name="table">Table to load properties into.</param>
        private async Task LoadExtendedPropertiesAsync(Table table)
        {
            const string query = @"
                SELECT 
                    ep.name AS PropertyName,
                    CAST(ep.value AS NVARCHAR(4000)) AS PropertyValue
                FROM 
                    sys.extended_properties ep
                WHERE 
                    ep.major_id = @ObjectId
                    AND ep.minor_id = 0
                    AND ep.name != 'MS_Description'";

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                var properties = await connection.QueryAsync<dynamic>(
                    query,
                    new { ObjectId = table.ObjectId });

                table.ExtendedProperties = new Dictionary<string, string>();
                foreach (var prop in properties)
                {
                    table.ExtendedProperties[prop.PropertyName] = prop.PropertyValue;
                }

                _logger.LogDebug("Found {Count} extended properties for {Schema}.{Table}", 
                    table.ExtendedProperties.Count, table.SchemaName, table.Name);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Failed to load extended properties for {Schema}.{Table}", 
                    table.SchemaName, table.Name);
                throw;
            }
        }

        /// <summary>
        /// Logs analysis completion summary.
        /// </summary>
        /// <param name="table">Analyzed table.</param>
        private void LogAnalysisComplete(Table table)
        {
            _logger.LogDebug(
                "Table analysis complete for {Schema}.{Table}: {ColumnCount} columns, {IndexCount} indexes, {PKCount} PK columns",
                table.SchemaName, 
                table.Name, 
                table.Columns.Count, 
                table.Indexes.Count, 
                table.PrimaryKeyColumns.Count);
        }
    }
}
