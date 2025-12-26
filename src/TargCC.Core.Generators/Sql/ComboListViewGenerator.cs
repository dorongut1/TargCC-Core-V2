// <copyright file="ComboListViewGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Generates ccvwComboList Views for dropdown/autocomplete lookups.
    /// These are indexed views with SCHEMABINDING for optimal performance.
    /// </summary>
    public class ComboListViewGenerator
    {
        private static readonly Action<ILogger, string, Exception?> LogGeneratingView =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1, nameof(LogGeneratingView)),
                "Generating ccvwComboList view for table: {TableName}");

        private static readonly Action<ILogger, string, string, Exception?> LogFoundTextColumn =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(2, nameof(LogFoundTextColumn)),
                "Found text column '{ColumnName}' for table {TableName}");

        private static readonly Action<ILogger, string, Exception?> LogSkippingTable =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(3, nameof(LogSkippingTable)),
                "Skipping ccvwComboList generation for table {TableName} - no suitable text column found");

        private static readonly Action<ILogger, int, Exception?> LogGeneratedViewCount =
            LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(4, nameof(LogGeneratedViewCount)),
                "Generated {Count} ccvwComboList views");

        /// <summary>
        /// Common text column name patterns to search for.
        /// </summary>
        private static readonly string[] TextColumnCandidates = new[]
        {
            "Name",
            "Title",
            "Description",
            "Text",
            "DisplayName",
            "FullName",
            "Label",
        };

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComboListViewGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public ComboListViewGenerator(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates a Fill stored procedure for a ccvwComboList view.
        /// This SP supports search, paging, and optional parent filtering.
        /// </summary>
        /// <param name="table">Table the view is based on.</param>
        /// <param name="parentIdColumn">Optional parent ID column for filtered lookups.</param>
        /// <returns>SQL script for the fill procedure.</returns>
        public static Task<string?> GenerateFillProcedureAsync(Table table, Column? parentIdColumn = null)
        {
            ArgumentNullException.ThrowIfNull(table);

            var viewName = $"ccvwComboList_{table.Name}";
            var spName = $"SP_Fill{viewName}";

            var sb = new StringBuilder();

            sb.AppendLine(CultureInfo.InvariantCulture, $"-- Fill procedure for {viewName}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"IF EXISTS (SELECT * FROM sys.procedures WHERE name = '{spName}')");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    DROP PROCEDURE [{spName}]");
            sb.AppendLine("GO");
            sb.AppendLine();

            sb.AppendLine(CultureInfo.InvariantCulture, $"CREATE PROCEDURE [{spName}]");
            sb.AppendLine("    @Search NVARCHAR(100) = NULL,");
            sb.AppendLine("    @Page INT = 1,");
            sb.AppendLine("    @PageSize INT = 20");

            if (parentIdColumn != null)
            {
                var parentType = GetSqlParameterType(parentIdColumn);
                sb.AppendLine(CultureInfo.InvariantCulture, $"    ,@ParentID {parentType} = NULL");
            }

            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();
            sb.AppendLine("    DECLARE @Offset INT = (@Page - 1) * @PageSize;");
            sb.AppendLine();
            sb.AppendLine("    SELECT ID, Text, TextNS");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    FROM [{viewName}]");
            sb.AppendLine("    WHERE (@Search IS NULL OR Text LIKE '%' + @Search + '%')");

            if (parentIdColumn != null)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        AND (@ParentID IS NULL OR {parentIdColumn.Name} = @ParentID)");
            }

            sb.AppendLine("    ORDER BY Text");
            sb.AppendLine("    OFFSET @Offset ROWS");
            sb.AppendLine("    FETCH NEXT @PageSize ROWS ONLY;");
            sb.AppendLine("END");

            return Task.FromResult<string?>(sb.ToString());
        }

        /// <summary>
        /// Generates a ccvwComboList view for a single table.
        /// </summary>
        /// <param name="table">Table to generate view for.</param>
        /// <returns>SQL script for creating the view, or null if table is not suitable.</returns>
        public Task<string?> GenerateComboListViewAsync(Table table)
        {
            ArgumentNullException.ThrowIfNull(table);

            // Skip tables that already have a ComboList view name
            if (table.Name.StartsWith("ccvwComboList_", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<string?>(null);
            }

            // Skip system tables
            if (table.Name.StartsWith("c_", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<string?>(null);
            }

            // Skip views - they may not support SCHEMABINDING
            if (table.IsView)
            {
                return Task.FromResult<string?>(null);
            }

            LogGeneratingView(_logger, table.Name, null);

            var textColumn = FindTextColumn(table);
            if (textColumn == null)
            {
                LogSkippingTable(_logger, table.Name, null);
                return Task.FromResult<string?>(null);
            }

            LogFoundTextColumn(_logger, textColumn.Name, table.Name, null);

            var pkColumn = table.Columns.Find(c => c.IsPrimaryKey);
            if (pkColumn == null)
            {
                return Task.FromResult<string?>(null);
            }

            var sql = GenerateViewScript(table, pkColumn, textColumn);
            return Task.FromResult<string?>(sql);
        }

        /// <summary>
        /// Generates ccvwComboList views for all suitable tables in a schema.
        /// </summary>
        /// <param name="schema">Database schema.</param>
        /// <returns>Combined SQL script for all views.</returns>
        public async Task<string> GenerateAllComboListViewsAsync(DatabaseSchema schema)
        {
            ArgumentNullException.ThrowIfNull(schema);

            var sb = new StringBuilder();
            sb.AppendLine("-- =========================================");
            sb.AppendLine("-- ccvwComboList Views for Dropdown Lookups");
            sb.AppendLine(CultureInfo.InvariantCulture, $"-- Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine("-- =========================================");
            sb.AppendLine();

            var generatedCount = 0;

            foreach (var table in schema.Tables.OrderBy(t => t.Name))
            {
                var viewSql = await GenerateComboListViewAsync(table).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(viewSql))
                {
                    sb.AppendLine(viewSql);
                    sb.AppendLine("GO");
                    sb.AppendLine();
                    generatedCount++;
                }
            }

            LogGeneratedViewCount(_logger, generatedCount, null);

            return sb.ToString();
        }

        /// <summary>
        /// Finds the best text column to use for display in the ComboList view.
        /// </summary>
        private static Column? FindTextColumn(Table table)
        {
            // First, try to find columns with common text names
            foreach (var candidate in TextColumnCandidates)
            {
                var column = table.Columns.Find(c =>
                    c.Name.Equals(candidate, StringComparison.OrdinalIgnoreCase));

                if (column != null)
                {
                    return column;
                }
            }

            // Try {TableName}Name pattern
            var tableNameColumn = table.Columns.Find(c =>
                c.Name.Equals($"{table.Name}Name", StringComparison.OrdinalIgnoreCase));

            if (tableNameColumn != null)
            {
                return tableNameColumn;
            }

            // Try columns ending with "Name" or "Title"
            var namedColumn = table.Columns.Find(c =>
                c.Name.EndsWith("Name", StringComparison.OrdinalIgnoreCase) ||
                c.Name.EndsWith("Title", StringComparison.OrdinalIgnoreCase));

            if (namedColumn != null)
            {
                return namedColumn;
            }

            // Fallback: find first string/varchar column that's not the primary key
            var stringColumn = table.Columns.Find(c =>
                !c.IsPrimaryKey &&
                IsStringType(c.DataType));

            return stringColumn;
        }

        /// <summary>
        /// Checks if a SQL data type is a string type.
        /// </summary>
        private static bool IsStringType(string dataType)
        {
            var upper = dataType.ToUpperInvariant();
            return upper.Contains("VARCHAR", StringComparison.Ordinal) ||
                   upper.Contains("CHAR", StringComparison.Ordinal) ||
                   upper.Contains("TEXT", StringComparison.Ordinal) ||
                   upper.Contains("NVARCHAR", StringComparison.Ordinal) ||
                   upper.Contains("NCHAR", StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets the SQL parameter type for a column.
        /// </summary>
        private static string GetSqlParameterType(Column column)
        {
            var upper = column.DataType.ToUpperInvariant();

            if (upper.Contains("INT", StringComparison.Ordinal))
            {
                return "INT";
            }

            if (upper.Contains("BIGINT", StringComparison.Ordinal))
            {
                return "BIGINT";
            }

            if (upper.Contains("UNIQUEIDENTIFIER", StringComparison.Ordinal))
            {
                return "UNIQUEIDENTIFIER";
            }

            return "INT";
        }

        /// <summary>
        /// Generates the CREATE VIEW script.
        /// </summary>
        private static string GenerateViewScript(Table table, Column pkColumn, Column textColumn)
        {
            var viewName = $"ccvwComboList_{table.Name}";
            var schemaName = string.IsNullOrEmpty(table.SchemaName) ? "dbo" : table.SchemaName;

            var sb = new StringBuilder();

            sb.AppendLine(CultureInfo.InvariantCulture, $"-- ccvwComboList view for {table.Name}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"IF EXISTS (SELECT * FROM sys.views WHERE name = '{viewName}')");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    DROP VIEW [{viewName}]");
            sb.AppendLine("GO");
            sb.AppendLine();

            sb.AppendLine(CultureInfo.InvariantCulture, $"CREATE VIEW [{viewName}]");
            sb.AppendLine("WITH SCHEMABINDING");
            sb.AppendLine("AS");
            sb.AppendLine("SELECT");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    [{pkColumn.Name}] AS ID,");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    [{textColumn.Name}] AS Text,");

            // TextNS is the text without special characters for search optimization
            sb.AppendLine(CultureInfo.InvariantCulture, $"    REPLACE(REPLACE(REPLACE([{textColumn.Name}], ' ', ''), '-', ''), '''', '') AS TextNS");

            sb.AppendLine(CultureInfo.InvariantCulture, $"FROM [{schemaName}].[{table.Name}]");

            return sb.ToString();
        }
    }
}
