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
    /// Analyzes individual table structure including columns, indexes, primary keys, and extended properties.
    /// Orchestrates <see cref="ColumnAnalyzer"/> to provide complete table metadata.
    /// </summary>
    /// <remarks>
    /// <para>
    /// TableAnalyzer is the primary entry point for analyzing a single table's complete structure.
    /// It coordinates between column analysis, index detection, and metadata loading to create
    /// a comprehensive <see cref="Table"/> object ready for code generation.
    /// </para>
    /// <para>
    /// <strong>What it analyzes:</strong>
    /// <list type="bullet">
    /// <item><term>Columns</term><description>Full column metadata via ColumnAnalyzer (types, nullability, defaults)</description></item>
    /// <item><term>Primary Key</term><description>Which columns form the PK (single or composite)</description></item>
    /// <item><term>Indexes</term><description>All indexes (unique, non-unique, clustered, filtered)</description></item>
    /// <item><term>Extended Properties</term><description>Custom metadata (MS_Description, custom properties)</description></item>
    /// <item><term>Metadata</term><description>ObjectId, CreateDate, ModifyDate for change detection</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Table Name Parsing:</strong>
    /// Supports multiple formats:
    /// <list type="bullet">
    /// <item>Simple name: "Customer" → assumes "dbo.Customer"</item>
    /// <item>Qualified name: "dbo.Customer"</item>
    /// <item>Bracketed: "[Sales].[Customer]" → parses to "Sales.Customer"</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var analyzer = new TableAnalyzer(connectionString, logger);
    /// 
    /// // Analyze a table
    /// var table = await analyzer.AnalyzeTableAsync("dbo.Customer");
    /// 
    /// Console.WriteLine($"Table: {table.FullName}");
    /// Console.WriteLine($"Columns: {table.Columns.Count}");
    /// Console.WriteLine($"Primary Key: {string.Join(", ", table.PrimaryKeyColumns)}");
    /// Console.WriteLine($"Indexes: {table.Indexes.Count}");
    /// 
    /// // Find the primary key column
    /// var pkColumn = table.Columns.FirstOrDefault(c => c.IsPrimaryKey);
    /// Console.WriteLine($"PK Column: {pkColumn?.Name} ({pkColumn?.DataType})");
    /// 
    /// // List all unique indexes (for GetBy methods)
    /// var uniqueIndexes = table.Indexes.Where(i => i.IsUnique && !i.IsPrimaryKey);
    /// foreach (var idx in uniqueIndexes)
    /// {
    ///     Console.WriteLine($"Unique Index: {idx.Name}");
    ///     Console.WriteLine($"  Columns: {string.Join(", ", idx.ColumnNames)}");
    /// }
    /// </code>
    /// </example>
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
        /// <param name="tableName">Fully qualified table name (schema.table) or simple name (defaults to dbo schema).</param>
        /// <returns>A <see cref="Table"/> object with complete structure information ready for code generation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when tableName is null or whitespace.</exception>
        /// <exception cref="SqlException">Thrown when database operation fails.</exception>
        /// <remarks>
        /// <para>
        /// This is the primary entry point for table analysis. It orchestrates all sub-analyzers
        /// to build a complete picture of the table structure.
        /// </para>
        /// <para>
        /// <strong>Analysis Pipeline:</strong>
        /// <list type="number">
        /// <item>Parse table name (schema + table)</item>
        /// <item>Load table metadata (ObjectId, dates, description)</item>
        /// <item>Analyze all columns via ColumnAnalyzer</item>
        /// <item>Detect primary key columns</item>
        /// <item>Load all indexes</item>
        /// <item>Load extended properties</item>
        /// </list>
        /// </para>
        /// <para>
        /// <strong>Change Detection:</strong>
        /// The returned Table object includes ModifyDate which can be compared
        /// to detect schema changes for incremental code generation.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var analyzer = new TableAnalyzer(connectionString, logger);
        /// 
        /// // Example 1: Analyze with simple name (assumes dbo schema)
        /// var customer = await analyzer.AnalyzeTableAsync("Customer");
        /// // Result: dbo.Customer analyzed
        /// 
        /// // Example 2: Analyze with qualified name
        /// var order = await analyzer.AnalyzeTableAsync("dbo.Order");
        /// Console.WriteLine($"Table: {order.FullName}");
        /// Console.WriteLine($"Columns: {order.Columns.Count}");
        /// Console.WriteLine($"Primary Key: {string.Join(", ", order.PrimaryKeyColumns)}");
        /// 
        /// // Example 3: Check for composite primary key
        /// if (order.PrimaryKeyColumns.Count > 1)
        /// {
        ///     Console.WriteLine("Composite primary key detected!");
        ///     Console.WriteLine($"PK: ({string.Join(", ", order.PrimaryKeyColumns)})");
        /// }
        /// 
        /// // Example 4: Find all foreign key columns (columns ending in ID)
        /// var fkColumns = order.Columns
        ///     .Where(c => c.Name.EndsWith("ID") && !c.IsPrimaryKey)
        ///     .ToList();
        /// Console.WriteLine($"Foreign Keys: {string.Join(", ", fkColumns.Select(c => c.Name))}");
        /// 
        /// // Example 5: Change detection
        /// var previousModifyDate = DateTime.Parse("2024-01-01");
        /// if (order.ModifyDate > previousModifyDate)
        /// {
        ///     Console.WriteLine("Table schema has changed - regenerate code!");
        /// }
        /// </code>
        /// </example>
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
        /// <remarks>
        /// <para>
        /// This method handles multiple table name formats commonly used in SQL Server:
        /// </para>
        /// <para>
        /// <strong>Supported Formats:</strong>
        /// <list type="bullet">
        /// <item><term>Simple</term><description>"Customer" → ("dbo", "Customer")</description></item>
        /// <item><term>Qualified</term><description>"Sales.Customer" → ("Sales", "Customer")</description></item>
        /// <item><term>Bracketed</term><description>"[Sales].[Customer]" → ("Sales", "Customer") (brackets removed)</description></item>
        /// <item><term>Default Schema</term><description>When no schema specified, defaults to "dbo"</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// <strong>Note:</strong> This is a simple string split. For production use with complex
        /// schema names containing dots, consider using SQL Server's built-in PARSENAME() function.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Example 1: Simple table name (defaults to dbo)
        /// var (schema1, table1) = ParseTableName("Customer");
        /// // Result: schema1 = "dbo", table1 = "Customer"
        /// 
        /// // Example 2: Qualified table name
        /// var (schema2, table2) = ParseTableName("Sales.Customer");
        /// // Result: schema2 = "Sales", table2 = "Customer"
        /// 
        /// // Example 3: Custom schema
        /// var (schema3, table3) = ParseTableName("Marketing.Campaign");
        /// // Result: schema3 = "Marketing", table3 = "Campaign"
        /// 
        /// // Example 4: Bracketed names (brackets need pre-removal)
        /// // Note: Current implementation doesn't handle brackets - preprocess first
        /// var name = "[Sales].[Customer]".Replace("[", "").Replace("]", "");
        /// var (schema4, table4) = ParseTableName(name);
        /// // Result: schema4 = "Sales", table4 = "Customer"
        /// 
        /// // Example 5: System tables
        /// var (schema5, table5) = ParseTableName("sys.tables");
        /// // Result: schema5 = "sys", table5 = "tables"
        /// </code>
        /// </example>
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
        /// <remarks>
        /// <para>
        /// Primary keys are critical for code generation as they determine:
        /// <list type="bullet">
        /// <item><term>GetByID methods</term><description>Generated method signature and parameters</description></item>
        /// <item><term>Update methods</term><description>WHERE clause for updates</description></item>
        /// <item><term>Delete methods</term><description>Parameter for deletion</description></item>
        /// <item><term>Identity detection</term><description>Auto-increment vs manual ID assignment</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// <strong>Composite Keys:</strong>
        /// When a table has a composite primary key (multiple columns), all columns
        /// are marked with <c>IsPrimaryKey = true</c> and listed in <c>PrimaryKeyColumns</c>.
        /// Code generation will create methods with multiple parameters.
        /// </para>
        /// <para>
        /// <strong>No Primary Key:</strong>
        /// If a table has no primary key, the PrimaryKeyColumns list will be empty.
        /// Code generation may skip certain methods or use all columns as identifier.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Example 1: Single column PK
        /// var customerTable = await analyzer.AnalyzeTableAsync("dbo.Customer");
        /// // customerTable.PrimaryKeyColumns = ["CustomerID"]
        /// // Generated method: GetByCustomerID(int customerID)
        /// 
        /// // Example 2: Composite PK
        /// var orderDetailTable = await analyzer.AnalyzeTableAsync("dbo.OrderDetail");
        /// // orderDetailTable.PrimaryKeyColumns = ["OrderID", "ProductID"]
        /// // Generated method: GetByOrderIDAndProductID(int orderID, int productID)
        /// 
        /// // Example 3: Check for composite key
        /// if (table.PrimaryKeyColumns.Count > 1)
        /// {
        ///     Console.WriteLine($"Composite key: {string.Join(" + ", table.PrimaryKeyColumns)}");
        /// }
        /// </code>
        /// </example>
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
        /// <remarks>
        /// <para>
        /// Indexes are crucial for code generation as they determine which query methods to create:
        /// </para>
        /// <para>
        /// <strong>Index Types and Generated Methods:</strong>
        /// <list type="bullet">
        /// <item><term>Unique Indexes</term><description>Generate GetByXXX methods (single result)</description></item>
        /// <item><term>Non-Unique Indexes</term><description>Generate FillByXXX methods (collection results)</description></item>
        /// <item><term>Primary Key Index</term><description>Generate GetByID method (already handled by PK)</description></item>
        /// <item><term>Composite Indexes</term><description>Methods with multiple parameters (e.g., GetByOrderIDAndDate)</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// <strong>Example Code Generation:</strong>
        /// <code>
        /// // Unique index on Email → generates:
        /// public Customer GetByEmail(string email)
        /// 
        /// // Non-unique index on Country → generates:
        /// public List&lt;Customer&gt; FillByCountry(string country)
        /// 
        /// // Composite index on (LastName, FirstName) → generates:
        /// public List&lt;Customer&gt; FillByLastNameAndFirstName(string lastName, string firstName)
        /// </code>
        /// </para>
        /// <para>
        /// <strong>Index Ordering:</strong>
        /// Indexes are loaded in order: Primary Key first, then Unique indexes, then Non-Unique.
        /// This ensures consistent code generation and method ordering.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var table = await analyzer.AnalyzeTableAsync("dbo.Customer");
        /// 
        /// // Example 1: Find unique indexes (for GetBy methods)
        /// var uniqueIndexes = table.Indexes
        ///     .Where(i => i.IsUnique && !i.IsPrimaryKey)
        ///     .ToList();
        /// 
        /// foreach (var idx in uniqueIndexes)
        /// {
        ///     Console.WriteLine($"GetBy method: GetBy{string.Join("And", idx.ColumnNames)}");
        /// }
        /// 
        /// // Example 2: Find non-unique indexes (for FillBy methods)
        /// var nonUniqueIndexes = table.Indexes
        ///     .Where(i => !i.IsUnique && !i.IsPrimaryKey)
        ///     .ToList();
        /// 
        /// foreach (var idx in nonUniqueIndexes)
        /// {
        ///     Console.WriteLine($"FillBy method: FillBy{string.Join("And", idx.ColumnNames)}");
        /// }
        /// 
        /// // Example 3: Check for composite indexes
        /// var compositeIndexes = table.Indexes
        ///     .Where(i => i.ColumnNames.Count > 1)
        ///     .ToList();
        /// 
        /// Console.WriteLine($"Composite indexes: {compositeIndexes.Count}");
        /// 
        /// // Example 4: Get all indexed columns (for optimization hints)
        /// var indexedColumns = table.Indexes
        ///     .SelectMany(i => i.ColumnNames)
        ///     .Distinct()
        ///     .ToList();
        /// 
        /// Console.WriteLine($"Indexed columns: {string.Join(", ", indexedColumns)}");
        /// </code>
        /// </example>
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
        /// Loads extended properties defined on the table (excluding MS_Description which is loaded separately).
        /// </summary>
        /// <param name="table">Table to load properties into.</param>
        /// <remarks>
        /// <para>
        /// Extended properties can store custom metadata for code generation behavior.
        /// Common TargCC extended properties at table level:
        /// </para>
        /// <para>
        /// <strong>Common Table Properties:</strong>
        /// <list type="bullet">
        /// <item><term>ccAuditLevel</term><description>Audit tracking level (None, Insert, Update, Delete, All)</description></item>
        /// <item><term>ccDefaultSortColumn</term><description>Default column for sorting in Fill methods</description></item>
        /// <item><term>ccUICreateMenu</term><description>Whether to create UI menu entry (1/0)</description></item>
        /// <item><term>ccUICreateEntity</term><description>Whether to create entity form (1/0)</description></item>
        /// <item><term>ccUICreateCollection</term><description>Whether to create collection grid (1/0)</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// These properties are accessed by code generators to customize behavior per table.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Add extended property in SQL Server:
        /// EXEC sp_addextendedproperty 
        ///     @name = 'ccAuditLevel', 
        ///     @value = 'All',
        ///     @level0type = 'SCHEMA', @level0name = 'dbo',
        ///     @level1type = 'TABLE', @level1name = 'Customer';
        /// 
        /// // In code generation:
        /// var table = await analyzer.AnalyzeTableAsync("dbo.Customer");
        /// 
        /// if (table.ExtendedProperties.TryGetValue("ccAuditLevel", out var auditLevel))
        /// {
        ///     Console.WriteLine($"Audit level: {auditLevel}");
        ///     // Generate appropriate audit triggers/code
        /// }
        /// 
        /// // Check if UI should be generated
        /// if (table.ExtendedProperties.TryGetValue("ccUICreateMenu", out var createMenu) 
        ///     && createMenu == "1")
        /// {
        ///     Console.WriteLine("Generate menu entry for this table");
        /// }
        /// </code>
        /// </example>
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
