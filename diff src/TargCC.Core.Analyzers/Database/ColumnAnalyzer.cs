using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Common;

         private async Task EnrichColumnAsync(Column column, string schemaName, string tableName)
         {
            // Analyze TargCC naming conventions (name-based)
            AnalyzeColumnPrefix(column);

            // Load extended properties (may override detected prefix via ccType)
            await LoadColumnExtendedPropertiesAsync(column, schemaName, tableName);

            // Map SQL type to .NET type
            column.DotNetType = MapSqlTypeToDotNet(column.DataType);
         }

         private void AnalyzeColumnPrefix(Column column)
         {
            column.Prefix = ColumnPrefixDetector.DeterminePrefix(column.Name);
            ApplyPrefixProperties(column);

            _logger.LogTrace("Column {Column} has prefix {Prefix}", column.Name, column.Prefix);
         }

         private async Task LoadColumnExtendedPropertiesAsync(Column column, string schemaName, string tableName)
         {
             try
             {

                
                ProcessExtendedProperties(column, properties);
                // After extended properties are loaded, allow ccType / ccDNA to influence column
                // ccDNA: DoNotAudit flag
                if (column.ExtendedProperties != null &&
                    column.ExtendedProperties.TryGetValue("ccDNA", out var dnaValue) &&
                    dnaValue == "1")
                {
                    column.DoNotAudit = true;
                    _logger.LogTrace("Column {Column} has DoNotAudit = {Value}", column.Name, column.DoNotAudit);
                }

                // Re-evaluate prefix allowing extended properties (ccType) to override name-based detection
                column.Prefix = ColumnPrefixDetector.DeterminePrefix(column.Name, column.ExtendedProperties);
                ApplyPrefixProperties(column);
             }
             catch (SqlException ex)
             {
                 _logger.LogWarning(
                     ex,
                     "Failed to load extended properties for column {Column} in {Schema}.{Table}",
                     column.Name,
                     schemaName,
                     tableName);
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
            column.ExtendedProperties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var prop in properties)
            {
                string propertyName = prop.PropertyName;
                string propertyValue = prop.PropertyValue;

                column.ExtendedProperties[propertyName] = propertyValue;
            }

            _logger.LogTrace(
                "Loaded {Count} extended properties for column {Column}",
                column.ExtendedProperties.Count,
                column.Name);
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
        ///     @name = 'ccDNA',        ///     @value = '1',
        ///     @level0type = 'SCHEMA', @level0name = 'dbo',
        ///     @level1type = 'TABLE', @level1name = 'Customer',
        ///     @level2type = 'COLUMN', @level2name = 'SSN';
        /// // Result: column.DoNotAudit = true
        ///
        /// // ccType - Define as business logic
        /// EXEC sp_addextendedproperty
        ///     @name = 'ccType',       ///     @value = 'blg',
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
        /// EXEC sp_addextendedproperty        ///     @name = 'ccType',        ///     @value = 'blg,clc',
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
