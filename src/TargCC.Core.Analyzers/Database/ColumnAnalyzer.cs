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
    /// Analyzes table columns including data types, nullability, defaults, and extended properties.
    /// Implements TargCC naming convention detection for specialized column behaviors.
    /// </summary>
    /// <remarks>
    /// <para>
    /// ColumnAnalyzer is responsible for detecting and interpreting TargCC column prefixes
    /// which define special behaviors like encryption, localization, and business logic.
    /// </para>
    /// <para>
    /// <strong>Supported Prefixes:</strong>
    /// <list type="bullet">
    /// <item><term>eno</term><description>One-way encryption (SHA256 hashing)</description></item>
    /// <item><term>ent</term><description>Two-way encryption (AES-256)</description></item>
    /// <item><term>enm</term><description>Enumeration field (links to c_Enumeration table)</description></item>
    /// <item><term>lkp</term><description>Lookup field (links to c_Lookup table)</description></item>
    /// <item><term>loc</term><description>Localizable field (supports multiple languages)</description></item>
    /// <item><term>clc_</term><description>Calculated field (read-only, computed)</description></item>
    /// <item><term>blg_</term><description>Business logic field (server-side only)</description></item>
    /// <item><term>agg_</term><description>Aggregate field (counters, sums)</description></item>
    /// <item><term>spt_</term><description>Separately updated field (different permissions)</description></item>
    /// <item><term>spl_</term><description>Separate list field (NewLine delimited)</description></item>
    /// <item><term>upl_</term><description>Upload field (document management)</description></item>
    /// <item><term>fui_</term><description>Fake unique index (computed column for indexing)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Extended Properties:</strong>
    /// Supports SQL Server extended properties for additional metadata:
    /// <list type="bullet">
    /// <item><term>ccType</term><description>Comma-separated list: blg,clc,spt,agg</description></item>
    /// <item><term>ccDNA</term><description>Do Not Audit flag (1 = skip auditing)</description></item>
    /// <item><term>ccUpdateXXXX</term><description>Partial update group definition</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var analyzer = new ColumnAnalyzer(connectionString, logger);
    /// var columns = await analyzer.AnalyzeColumnsAsync("dbo", "Customer");
    /// 
    /// foreach (var column in columns)
    /// {
    ///     Console.WriteLine($"{column.Name} ({column.DataType})");
    ///     Console.WriteLine($"  Prefix: {column.Prefix}");
    ///     Console.WriteLine($"  .NET Type: {column.DotNetType}");
    ///     Console.WriteLine($"  Encrypted: {column.IsEncrypted}");
    ///     Console.WriteLine($"  Read-Only: {column.IsReadOnly}");
    /// }
    /// </code>
    /// </example>
    public class ColumnAnalyzer
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnAnalyzer"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string to the database.</param>
        /// <param name="logger">Logger for tracking operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when connectionString or logger is null.</exception>
        public ColumnAnalyzer(string connectionString, ILogger logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Analyzes all columns in a table.
        /// </summary>
        /// <param name="schemaName">Schema name of the table.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>List of analyzed columns with complete metadata including prefixes, types, and extended properties.</returns>
        /// <exception cref="ArgumentNullException">Thrown when schemaName or tableName is null.</exception>
        /// <exception cref="SqlException">Thrown when database operation fails.</exception>
        /// <example>
        /// <code>
        /// var analyzer = new ColumnAnalyzer(connectionString, logger);
        /// 
        /// // Analyze all columns in Customer table
        /// var columns = await analyzer.AnalyzeColumnsAsync("dbo", "Customer");
        /// 
        /// // Find password column (one-way encrypted)
        /// var passwordColumn = columns.Find(c => c.Prefix == ColumnPrefix.OneWayEncryption);
        /// if (passwordColumn != null)
        /// {
        ///     Console.WriteLine($"Password field: {passwordColumn.Name}");
        ///     Console.WriteLine($"Is encrypted: {passwordColumn.IsEncrypted}");
        /// }
        /// 
        /// // Find all business logic columns
        /// var businessLogicColumns = columns.Where(c => c.Prefix == ColumnPrefix.BusinessLogic);
        /// Console.WriteLine($"Found {businessLogicColumns.Count()} business logic columns");
        /// </code>
        /// </example>
        public async Task<List<Column>> AnalyzeColumnsAsync(string schemaName, string tableName)
        {
            ValidateParameters(schemaName, tableName);

            try
            {
                _logger.LogDebug("Starting column analysis for {Schema}.{Table}", schemaName, tableName);

                var columnData = await FetchColumnDataAsync(schemaName, tableName);
                var columns = await ProcessColumnDataAsync(columnData, schemaName, tableName);

                LogAnalysisComplete(schemaName, tableName, columns.Count);
                return columns;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error analyzing columns for {Schema}.{Table}", schemaName, tableName);
                throw new InvalidOperationException($"Failed to analyze columns for table '{schemaName}.{tableName}'", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error analyzing columns for {Schema}.{Table}", schemaName, tableName);
                throw;
            }
        }

        /// <summary>
        /// Validates input parameters.
        /// </summary>
        /// <param name="schemaName">Schema name to validate.</param>
        /// <param name="tableName">Table name to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null or whitespace.</exception>
        private static void ValidateParameters(string schemaName, string tableName)
        {
            if (string.IsNullOrWhiteSpace(schemaName))
            {
                throw new ArgumentNullException(nameof(schemaName));
            }

            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }
        }

        /// <summary>
        /// Fetches raw column data from the database.
        /// </summary>
        /// <param name="schemaName">Schema name.</param>
        /// <param name="tableName">Table name.</param>
        /// <returns>Dynamic result set from database.</returns>
        private async Task<IEnumerable<dynamic>> FetchColumnDataAsync(string schemaName, string tableName)
        {
            const string query = @"
                SELECT 
                    c.column_id AS ColumnId,
                    c.name AS Name,
                    TYPE_NAME(c.user_type_id) AS DataType,
                    c.max_length AS MaxLength,
                    c.precision AS Precision,
                    c.scale AS Scale,
                    c.is_nullable AS IsNullable,
                    c.is_identity AS IsIdentity,
                    c.is_computed AS IsComputed,
                    CAST(dc.definition AS NVARCHAR(4000)) AS DefaultValue,
                    CAST(cc.definition AS NVARCHAR(4000)) AS ComputedDefinition,
                    CAST(ep.value AS NVARCHAR(4000)) AS Description
                FROM 
                    sys.columns c
                    LEFT JOIN sys.default_constraints dc 
                        ON dc.parent_object_id = c.object_id 
                        AND dc.parent_column_id = c.column_id
                    LEFT JOIN sys.computed_columns cc 
                        ON cc.object_id = c.object_id 
                        AND cc.column_id = c.column_id
                    LEFT JOIN sys.extended_properties ep 
                        ON ep.major_id = c.object_id 
                        AND ep.minor_id = c.column_id 
                        AND ep.name = 'MS_Description'
                WHERE 
                    c.object_id = OBJECT_ID(@FullTableName)
                ORDER BY 
                    c.column_id";

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<dynamic>(
                query,
                new { FullTableName = $"{schemaName}.{tableName}" });
        }

        /// <summary>
        /// Processes raw column data into Column objects.
        /// </summary>
        /// <param name="columnData">Raw column data from database.</param>
        /// <param name="schemaName">Schema name.</param>
        /// <param name="tableName">Table name.</param>
        /// <returns>List of processed Column objects.</returns>
        private async Task<List<Column>> ProcessColumnDataAsync(
            IEnumerable<dynamic> columnData, 
            string schemaName, 
            string tableName)
        {
            var columns = new List<Column>();

            foreach (var col in columnData)
            {
                var column = CreateColumnFromData(col);
                await EnrichColumnAsync(column, schemaName, tableName);
                columns.Add(column);
            }

            return columns;
        }

        /// <summary>
        /// Creates a Column object from dynamic database result.
        /// </summary>
        /// <param name="data">Dynamic data from database.</param>
        /// <returns>Column object with basic properties.</returns>
        private static Column CreateColumnFromData(dynamic data)
        {
            return new Column
            {
                ColumnId = data.ColumnId,
                Name = data.Name,
                DataType = data.DataType,
                MaxLength = data.MaxLength,
                //Precision = data.Precision,
               // Scale = data.Scale,
                IsNullable = data.IsNullable,
                IsIdentity = data.IsIdentity,
                IsComputed = data.IsComputed,
                DefaultValue = data.DefaultValue,
                ComputedDefinition = data.ComputedDefinition,
                Description = data.Description
            };
        }

        /// <summary>
        /// Enriches column with additional analysis and properties.
        /// </summary>
        /// <param name="column">Column to enrich.</param>
        /// <param name="schemaName">Schema name.</param>
        /// <param name="tableName">Table name.</param>
        private async Task EnrichColumnAsync(Column column, string schemaName, string tableName)
        {
            // Analyze TargCC naming conventions
            AnalyzeColumnPrefix(column);

            // Load extended properties
            await LoadColumnExtendedPropertiesAsync(column, schemaName, tableName);

            // Map SQL type to .NET type
            column.DotNetType = MapSqlTypeToDotNet(column.DataType);

            _logger.LogTrace("Processed column {Column} with type {Type} and prefix {Prefix}", 
                column.Name, column.DataType, column.Prefix);
        }

        /// <summary>
        /// Analyzes column name prefix for TargCC conventions.
        /// </summary>
        /// <param name="column">Column to analyze.</param>
        private void AnalyzeColumnPrefix(Column column)
        {
            var name = column.Name.ToLower();

            column.Prefix = DetermineColumnPrefix(name);
            ApplyPrefixProperties(column);

            _logger.LogTrace("Column {Column} has prefix {Prefix}", column.Name, column.Prefix);
        }

        /// <summary>
        /// Determines the column prefix based on TargCC naming conventions.
        /// </summary>
        /// <param name="columnName">Column name in lowercase.</param>
        /// <returns>Column prefix enum value indicating the column's special behavior.</returns>
        /// <remarks>
        /// <para>
        /// This is the core method for detecting TargCC column conventions. The prefix determines
        /// how the column behaves in generated code, including encryption, read-only status,
        /// and UI rendering.
        /// </para>
        /// <para>
        /// Prefix detection is performed in order, with earlier prefixes taking precedence.
        /// For example, 'enoprivate' will be detected as OneWayEncryption, not Enumeration.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // One-way encryption (passwords)
        /// var prefix1 = DetermineColumnPrefix("enopassword");
        /// // Result: ColumnPrefix.OneWayEncryption
        /// 
        /// // Two-way encryption (credit cards)
        /// var prefix2 = DetermineColumnPrefix("entcreditcard");
        /// // Result: ColumnPrefix.TwoWayEncryption
        /// 
        /// // Enumeration (status fields)
        /// var prefix3 = DetermineColumnPrefix("enmstatus");
        /// // Result: ColumnPrefix.Enumeration
        /// 
        /// // Lookup (dropdown lists)
        /// var prefix4 = DetermineColumnPrefix("lkpcountry");
        /// // Result: ColumnPrefix.Lookup
        /// 
        /// // Calculated field (read-only)
        /// var prefix5 = DetermineColumnPrefix("clc_totalamount");
        /// // Result: ColumnPrefix.Calculated
        /// 
        /// // Business logic (server-side only)
        /// var prefix6 = DetermineColumnPrefix("blg_creditlimit");
        /// // Result: ColumnPrefix.BusinessLogic
        /// 
        /// // Regular column
        /// var prefix7 = DetermineColumnPrefix("firstname");
        /// // Result: ColumnPrefix.None
        /// </code>
        /// </example>
        private static ColumnPrefix DetermineColumnPrefix(string columnName)
        {
            return columnName switch
            {
                _ when columnName.StartsWith("eno") => ColumnPrefix.OneWayEncryption,
                _ when columnName.StartsWith("ent") => ColumnPrefix.TwoWayEncryption,
                _ when columnName.StartsWith("enm") => ColumnPrefix.Enumeration,
                _ when columnName.StartsWith("lkp") => ColumnPrefix.Lookup,
                _ when columnName.StartsWith("loc") => ColumnPrefix.Localization,
                _ when columnName.StartsWith("clc_") => ColumnPrefix.Calculated,
                _ when columnName.StartsWith("blg_") => ColumnPrefix.BusinessLogic,
                _ when columnName.StartsWith("agg_") => ColumnPrefix.Aggregate,
                _ when columnName.StartsWith("spt_") => ColumnPrefix.SeparateUpdate,
                _ when columnName.StartsWith("spl_") => ColumnPrefix.SeparateList,
                _ when columnName.StartsWith("upl_") => ColumnPrefix.Upload,
                _ when columnName.StartsWith("fui_") => ColumnPrefix.FakeUniqueIndex,
                _ => ColumnPrefix.None
            };
        }

        /// <summary>
        /// Applies behavioral properties based on detected column prefix.
        /// </summary>
        /// <param name="column">Column to apply properties to.</param>
        /// <remarks>
        /// <para>
        /// Different prefixes trigger different column behaviors:
        /// </para>
        /// <para>
        /// <strong>Encryption Prefixes (eno, ent):</strong>
        /// Sets <c>IsEncrypted = true</c>, which tells code generators to:
        /// - Hash before storing (eno = one-way)
        /// - Encrypt/decrypt on save/load (ent = two-way)
        /// - Use password textbox in UI (eno)
        /// </para>
        /// <para>
        /// <strong>Read-Only Prefixes (clc_, blg_, agg_):</strong>
        /// Sets <c>IsReadOnly = true</c>, which tells code generators to:
        /// - Exclude from Update methods
        /// - Use read-only controls in UI
        /// - Server-side updates only (business logic)
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var column1 = new Column { Name = "enoPassword", Prefix = ColumnPrefix.OneWayEncryption };
        /// ApplyPrefixProperties(column1);
        /// // Result: column1.IsEncrypted = true
        /// 
        /// var column2 = new Column { Name = "blg_CreditScore", Prefix = ColumnPrefix.BusinessLogic };
        /// ApplyPrefixProperties(column2);
        /// // Result: column2.IsReadOnly = true
        /// 
        /// var column3 = new Column { Name = "FirstName", Prefix = ColumnPrefix.None };
        /// ApplyPrefixProperties(column3);
        /// // Result: No special properties set
        /// </code>
        /// </example>
        private static void ApplyPrefixProperties(Column column)
        {
            switch (column.Prefix)
            {
                case ColumnPrefix.OneWayEncryption:
                case ColumnPrefix.TwoWayEncryption:
                    column.IsEncrypted = true;
                    break;

                case ColumnPrefix.Calculated:
                case ColumnPrefix.BusinessLogic:
                case ColumnPrefix.Aggregate:
                    column.IsReadOnly = true;
                    break;
            }
        }

        /// <summary>
        /// Loads extended properties for a column.
        /// </summary>
        /// <param name="column">Column to load properties for.</param>
        /// <param name="schemaName">Schema name.</param>
        /// <param name="tableName">Table name.</param>
        private async Task LoadColumnExtendedPropertiesAsync(Column column, string schemaName, string tableName)
        {
            const string query = @"
                SELECT 
                    ep.name AS PropertyName,
                    CAST(ep.value AS NVARCHAR(4000)) AS PropertyValue
                FROM 
                    sys.extended_properties ep
                    INNER JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id
                WHERE 
                    c.object_id = OBJECT_ID(@FullTableName)
                    AND c.name = @ColumnName
                    AND ep.name != 'MS_Description'";

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                var properties = await connection.QueryAsync<dynamic>(
                    query,
                    new
                    {
                        FullTableName = $"{schemaName}.{tableName}",
                        ColumnName = column.Name
                    });

                ProcessExtendedProperties(column, properties);
            }
            catch (SqlException ex)
            {
                _logger.LogWarning(ex, "Failed to load extended properties for column {Column} in {Schema}.{Table}",
                    column.Name, schemaName, tableName);
                column.ExtendedProperties = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Processes extended properties into column object.
        /// </summary>
        /// <param name="column">Column to process properties for.</param>
        /// <param name="properties">Raw properties from database.</param>
        private void ProcessExtendedProperties(Column column, IEnumerable<dynamic> properties)
        {
            column.ExtendedProperties = new Dictionary<string, string>();

            foreach (var prop in properties)
            {
                string propertyName = prop.PropertyName;
                string propertyValue = prop.PropertyValue;

                column.ExtendedProperties[propertyName] = propertyValue;

                // Handle special properties
                HandleSpecialProperty(column, propertyName, propertyValue);
            }

            _logger.LogTrace("Loaded {Count} extended properties for column {Column}",
                column.ExtendedProperties.Count, column.Name);
        }

        /// <summary>
        /// Handles special TargCC extended properties like ccType and ccDNA.
        /// </summary>
        /// <param name="column">Column to apply properties to.</param>
        /// <param name="propertyName">Extended property name (e.g., 'ccType', 'ccDNA').</param>
        /// <param name="propertyValue">Extended property value.</param>
        /// <remarks>
        /// <para>
        /// TargCC uses SQL Server extended properties to store metadata that affects
        /// code generation and behavior. These properties are prefixed with 'cc' (Code Creator).
        /// </para>
        /// <para>
        /// <strong>Recognized Properties:</strong>
        /// <list type="bullet">
        /// <item><term>ccType</term><description>Defines column behavior (blg, clc, spt, agg)</description></item>
        /// <item><term>ccDNA</term><description>Do Not Audit flag - when set to '1', skips audit logging</description></item>
        /// <item><term>ccUpdateXXXX</term><description>Partial update group definition</description></item>
        /// <item><term>ccUsedForTableCleanup</term><description>Marks date field for automated cleanup</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // ccDNA - Skip audit logging for sensitive data
        /// EXEC sp_addextendedproperty 
        ///     @name = 'ccDNA', 
        ///     @value = '1',
        ///     @level0type = 'SCHEMA', @level0name = 'dbo',
        ///     @level1type = 'TABLE', @level1name = 'Customer',
        ///     @level2type = 'COLUMN', @level2name = 'SSN';
        /// // Result: column.DoNotAudit = true
        /// 
        /// // ccType - Define as business logic
        /// EXEC sp_addextendedproperty 
        ///     @name = 'ccType', 
        ///     @value = 'blg',
        ///     @level0type = 'SCHEMA', @level0name = 'dbo',
        ///     @level1type = 'TABLE', @level1name = 'Order',
        ///     @level2type = 'COLUMN', @level2name = 'TotalAmount';
        /// // Result: column.Prefix = ColumnPrefix.BusinessLogic, column.IsReadOnly = true
        /// </code>
        /// </example>
        private void HandleSpecialProperty(Column column, string propertyName, string propertyValue)
        {
            if (propertyName.Equals("ccType", StringComparison.OrdinalIgnoreCase))
            {
                ParseCcType(column, propertyValue);
            }
            else if (propertyName.Equals("ccDNA", StringComparison.OrdinalIgnoreCase))
            {
                column.DoNotAudit = propertyValue == "1";
                _logger.LogTrace("Column {Column} has DoNotAudit = {Value}", column.Name, column.DoNotAudit);
            }
        }

        /// <summary>
        /// Parses the ccType extended property and applies appropriate settings.
        /// </summary>
        /// <param name="column">Column to apply settings to.</param>
        /// <param name="ccType">ccType value (comma-separated list of type identifiers).</param>
        /// <remarks>
        /// <para>
        /// The ccType extended property allows defining column behavior without changing
        /// the column name. This is useful when you cannot modify the schema but need
        /// TargCC-specific behaviors.
        /// </para>
        /// <para>
        /// Multiple types can be combined using commas: "blg,clc" means both
        /// business logic AND calculated.
        /// </para>
        /// <para>
        /// <strong>Supported ccType values:</strong>
        /// <list type="bullet">
        /// <item><term>blg</term><description>Business logic - read-only, server-side updates only</description></item>
        /// <item><term>clc</term><description>Calculated - computed field, read-only</description></item>
        /// <item><term>spt</term><description>Separate update - different permissions required</description></item>
        /// <item><term>agg</term><description>Aggregate - counter/sum field, read-only</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // In SQL Server:
        /// EXEC sp_addextendedproperty 
        ///     @name = 'ccType', 
        ///     @value = 'blg,clc',
        ///     @level0type = 'SCHEMA', @level0name = 'dbo',
        ///     @level1type = 'TABLE', @level1name = 'Customer',
        ///     @level2type = 'COLUMN', @level2name = 'TotalOrderAmount';
        /// 
        /// // The analyzer will detect this and set:
        /// // column.Prefix = ColumnPrefix.BusinessLogic
        /// // column.IsReadOnly = true
        /// </code>
        /// </example>
        private void ParseCcType(Column column, string ccType)
        {
            if (string.IsNullOrWhiteSpace(ccType))
            {
                return;
            }

            var types = ccType.Split(',')
                .Select(t => t.Trim().ToLower())
                .ToList();

            ApplyCcTypeSettings(column, types);

            _logger.LogTrace("Column {Column} has ccType: {CcType}", column.Name, ccType);
        }

        /// <summary>
        /// Applies settings based on ccType values.
        /// </summary>
        /// <param name="column">Column to apply settings to.</param>
        /// <param name="types">List of type identifiers.</param>
        private static void ApplyCcTypeSettings(Column column, List<string> types)
        {
            if (types.Contains("blg"))
            {
                column.Prefix = ColumnPrefix.BusinessLogic;
                column.IsReadOnly = true;
            }

            if (types.Contains("clc"))
            {
                column.Prefix = ColumnPrefix.Calculated;
                column.IsReadOnly = true;
            }

            if (types.Contains("spt"))
            {
                column.Prefix = ColumnPrefix.SeparateUpdate;
            }

            if (types.Contains("agg"))
            {
                column.Prefix = ColumnPrefix.Aggregate;
                column.IsReadOnly = true;
            }
        }

        /// <summary>
        /// Maps SQL Server data type to .NET type string for code generation.
        /// </summary>
        /// <param name="sqlType">SQL Server type name (e.g., 'int', 'nvarchar', 'datetime2').</param>
        /// <returns>.NET type name as string (e.g., 'int', 'string', 'DateTime').</returns>
        /// <remarks>
        /// <para>
        /// This mapping is used by code generators to create properly-typed .NET properties.
        /// The mapping follows C# conventions and uses built-in type aliases where available.
        /// </para>
        /// <para>
        /// For unknown types, returns 'object' as a safe fallback.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var mapper = new ColumnAnalyzer(connectionString, logger);
        /// 
        /// // Integer types
        /// var type1 = mapper.MapSqlTypeToDotNet("int");        // Returns: "int"
        /// var type2 = mapper.MapSqlTypeToDotNet("bigint");     // Returns: "long"
        /// var type3 = mapper.MapSqlTypeToDotNet("tinyint");    // Returns: "byte"
        /// 
        /// // Text types
        /// var type4 = mapper.MapSqlTypeToDotNet("nvarchar");   // Returns: "string"
        /// var type5 = mapper.MapSqlTypeToDotNet("varchar");    // Returns: "string"
        /// 
        /// // Date/Time types
        /// var type6 = mapper.MapSqlTypeToDotNet("datetime2");  // Returns: "DateTime"
        /// var type7 = mapper.MapSqlTypeToDotNet("time");       // Returns: "TimeSpan"
        /// 
        /// // Decimal types
        /// var type8 = mapper.MapSqlTypeToDotNet("decimal");    // Returns: "decimal"
        /// var type9 = mapper.MapSqlTypeToDotNet("money");      // Returns: "decimal"
        /// 
        /// // Binary types
        /// var type10 = mapper.MapSqlTypeToDotNet("varbinary"); // Returns: "byte[]"
        /// 
        /// // Boolean
        /// var type11 = mapper.MapSqlTypeToDotNet("bit");       // Returns: "bool"
        /// 
        /// // Unknown type
        /// var type12 = mapper.MapSqlTypeToDotNet("unknown");   // Returns: "object"
        /// </code>
        /// </example>
        private string MapSqlTypeToDotNet(string sqlType)
        {
            var dotNetType = sqlType.ToLower() switch
            {
                "bigint" => "long",
                "int" => "int",
                "smallint" => "short",
                "tinyint" => "byte",
                "bit" => "bool",
                "decimal" or "numeric" or "money" or "smallmoney" => "decimal",
                "float" => "double",
                "real" => "float",
                "date" or "datetime" or "datetime2" or "smalldatetime" => "DateTime",
                "time" => "TimeSpan",
                "datetimeoffset" => "DateTimeOffset",
                "char" or "varchar" or "text" or "nchar" or "nvarchar" or "ntext" => "string",
                "uniqueidentifier" => "Guid",
                "binary" or "varbinary" or "image" => "byte[]",
                "xml" => "string",
                _ => "object"
            };

            _logger.LogTrace("Mapped SQL type {SqlType} to .NET type {DotNetType}", sqlType, dotNetType);
            return dotNetType;
        }

        /// <summary>
        /// Logs analysis completion summary.
        /// </summary>
        /// <param name="schemaName">Schema name.</param>
        /// <param name="tableName">Table name.</param>
        /// <param name="columnCount">Number of columns analyzed.</param>
        private void LogAnalysisComplete(string schemaName, string tableName, int columnCount)
        {
            _logger.LogDebug("Column analysis complete for {Schema}.{Table}: {Count} columns analyzed",
                schemaName, tableName, columnCount);
        }
    }
}
