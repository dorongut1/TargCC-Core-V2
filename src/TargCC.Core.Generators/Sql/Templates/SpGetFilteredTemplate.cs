// <copyright file="SpGetFilteredTemplate.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.Sql.Templates
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Template for generating SP_GetFiltered stored procedure with index-based filters.
    /// </summary>
    public static class SpGetFilteredTemplate
    {
        /// <summary>
        /// Generates the SP_GetFiltered stored procedure for a table based on its indexes.
        /// </summary>
        /// <param name="table">The table to generate the procedure for.</param>
        /// <returns>The generated SQL stored procedure.</returns>
        /// <exception cref="ArgumentNullException">Thrown when table is null.</exception>
        public static Task<string> GenerateAsync(Table table)
        {
            ArgumentNullException.ThrowIfNull(table);

            // Only generate if there are non-PK indexes
            var filterableIndexes = table.Indexes?
                .Where(i => !i.IsPrimaryKey && i.ColumnNames != null && i.ColumnNames.Count > 0)
                .ToList();

            if (filterableIndexes == null || filterableIndexes.Count == 0)
            {
                return Task.FromResult(string.Empty);
            }

            var sb = new StringBuilder();
            var fkColumns = GetForeignKeyColumns(table);

            GenerateProcedureHeader(sb, table, filterableIndexes);

            if (fkColumns.Count > 0)
            {
                GenerateWithParentTextBranch(sb, table, filterableIndexes, fkColumns);
            }
            else
            {
                GenerateSimpleSelectAndWhere(sb, table, filterableIndexes, "    ");
                GenerateOrderByAndPagination(sb, table, null, "    ");
            }

            sb.AppendLine("END");

            return Task.FromResult(sb.ToString());
        }

        private static List<Column> GetForeignKeyColumns(Table table)
        {
            return table.Columns
                .Where(c => c.IsForeignKey && !string.IsNullOrEmpty(c.ReferencedTable))
                .ToList();
        }

        private static void GenerateProcedureHeader(
            StringBuilder sb,
            Table table,
            List<TargCC.Core.Interfaces.Models.Index> filterableIndexes)
        {
            var entityName = API.BaseApiGenerator.GetClassName(table.Name);

            sb.AppendLine(CultureInfo.InvariantCulture, $"CREATE OR ALTER PROCEDURE [dbo].[SP_GetFiltered{entityName}s]");

            var parameters = new List<string>();
            foreach (var index in filterableIndexes)
            {
                AddIndexColumnParameters(sb, table, index, parameters);
            }

            sb.AppendLine("    @Skip INT = NULL,");
            sb.AppendLine("    @Take INT = NULL,");
            sb.AppendLine("    @WithParentText BIT = 1");
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();
        }

        private static void AddIndexColumnParameters(
            StringBuilder sb,
            Table table,
            TargCC.Core.Interfaces.Models.Index index,
            List<string> parameters)
        {
            foreach (var columnName in index.ColumnNames)
            {
                var column = table.Columns.Find(c => c.Name == columnName);
                if (column == null)
                {
                    continue;
                }

                var paramName = $"@{columnName}";
                if (parameters.Contains(paramName))
                {
                    continue;
                }

                parameters.Add(paramName);
                var sqlType = GetSqlType(column.DataType, column.MaxLength);
                sb.AppendLine(CultureInfo.InvariantCulture, $"    {paramName} {sqlType} = NULL,");
            }
        }

        private static void GenerateWithParentTextBranch(
            StringBuilder sb,
            Table table,
            List<TargCC.Core.Interfaces.Models.Index> filterableIndexes,
            List<Column> fkColumns)
        {
            sb.AppendLine("    IF @WithParentText = 1");
            sb.AppendLine("    BEGIN");

            GenerateSelectWithJoins(sb, table, filterableIndexes, fkColumns, "        ");
            GenerateOrderByAndPagination(sb, table, "t", "        ");

            sb.AppendLine("    END");
            sb.AppendLine("    ELSE");
            sb.AppendLine("    BEGIN");

            GenerateSimpleSelectAndWhere(sb, table, filterableIndexes, "        ");
            GenerateOrderByAndPagination(sb, table, null, "        ");

            sb.AppendLine("    END");
        }

        private static void GenerateSelectWithJoins(
            StringBuilder sb,
            Table table,
            List<TargCC.Core.Interfaces.Models.Index> filterableIndexes,
            List<Column> fkColumns,
            string indent)
        {
            const string tableAlias = "t";

            sb.AppendLine(CultureInfo.InvariantCulture, $"{indent}SELECT");

            GenerateBaseColumns(sb, table, $"{tableAlias}.", indent, hasMoreColumns: true);
            GenerateParentTextColumns(sb, fkColumns, indent);

            sb.AppendLine(CultureInfo.InvariantCulture, $"{indent}FROM [{table.Name}] {tableAlias}");

            GenerateLeftJoins(sb, fkColumns, tableAlias, indent);
            GenerateWhereClause(sb, table, filterableIndexes, $"{tableAlias}.", indent);
        }

        private static void GenerateSimpleSelectAndWhere(
            StringBuilder sb,
            Table table,
            List<TargCC.Core.Interfaces.Models.Index> filterableIndexes,
            string indent)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"{indent}SELECT");

            GenerateBaseColumns(sb, table, string.Empty, indent, hasMoreColumns: false);

            sb.AppendLine(CultureInfo.InvariantCulture, $"{indent}FROM [{table.Name}]");
            GenerateWhereClause(sb, table, filterableIndexes, string.Empty, indent);
        }

        private static void GenerateBaseColumns(
            StringBuilder sb,
            Table table,
            string prefix,
            string indent,
            bool hasMoreColumns)
        {
            var allColumns = table.Columns.ToList();
            for (int i = 0; i < allColumns.Count; i++)
            {
                var col = allColumns[i];
                sb.Append(CultureInfo.InvariantCulture, $"{indent}    {prefix}[{col.Name}]");

                if (hasMoreColumns || i < allColumns.Count - 1)
                {
                    sb.AppendLine(",");
                }
                else
                {
                    sb.AppendLine();
                }
            }
        }

        private static void GenerateParentTextColumns(
            StringBuilder sb,
            List<Column> fkColumns,
            string indent)
        {
            for (int i = 0; i < fkColumns.Count; i++)
            {
                var fkCol = fkColumns[i];
                var parentAlias = $"p{i + 1}";
                var parentTextColumnName = $"{fkCol.Name}_Text";

                sb.Append(CultureInfo.InvariantCulture, $"{indent}    {parentAlias}.[Text] AS [{parentTextColumnName}]");
                sb.AppendLine(i < fkColumns.Count - 1 ? "," : string.Empty);
            }
        }

        private static void GenerateLeftJoins(
            StringBuilder sb,
            List<Column> fkColumns,
            string tableAlias,
            string indent)
        {
            for (int i = 0; i < fkColumns.Count; i++)
            {
                var fkCol = fkColumns[i];
                var parentAlias = $"p{i + 1}";
                var comboViewName = $"ccvwComboList_{fkCol.ReferencedTable}";

                sb.AppendLine(CultureInfo.InvariantCulture, $"{indent}LEFT JOIN [{comboViewName}] {parentAlias} ON {tableAlias}.[{fkCol.Name}] = {parentAlias}.[ID]");
            }
        }

        private static void GenerateWhereClause(
            StringBuilder sb,
            Table table,
            List<TargCC.Core.Interfaces.Models.Index> filterableIndexes,
            string prefix,
            string indent)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"{indent}WHERE 1=1");

            foreach (var index in filterableIndexes)
            {
                foreach (var columnName in index.ColumnNames)
                {
                    var column = table.Columns.Find(c => c.Name == columnName);
                    if (column == null)
                    {
                        continue;
                    }

                    var paramName = $"@{columnName}";
                    var condition = IsTextType(column.DataType)
                        ? $"{prefix}[{columnName}] LIKE '%' + {paramName} + '%'"
                        : $"{prefix}[{columnName}] = {paramName}";

                    sb.AppendLine(CultureInfo.InvariantCulture, $"{indent}  AND ({paramName} IS NULL OR {condition})");
                }
            }
        }

        private static void GenerateOrderByAndPagination(
            StringBuilder sb,
            Table table,
            string? tableAlias,
            string indent)
        {
            var pkColumns = table.Columns.Where(c => c.IsPrimaryKey).ToList();
            var prefix = string.IsNullOrEmpty(tableAlias) ? string.Empty : $"{tableAlias}.";

            if (pkColumns.Count > 0)
            {
                sb.Append(CultureInfo.InvariantCulture, $"{indent}ORDER BY ");
                for (int i = 0; i < pkColumns.Count; i++)
                {
                    var col = pkColumns[i];
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(CultureInfo.InvariantCulture, $"{prefix}[{col.Name}]");
                }

                sb.AppendLine();

                sb.AppendLine(CultureInfo.InvariantCulture, $"{indent}OFFSET ISNULL(@Skip, 0) ROWS");
                sb.AppendLine(CultureInfo.InvariantCulture, $"{indent}FETCH NEXT ISNULL(@Take, 2147483647) ROWS ONLY;");
            }
            else
            {
                sb.AppendLine(";");
            }
        }

        private static string GetSqlType(string dataType, int? maxLength)
        {
            var upperType = dataType.ToUpperInvariant();

            if (upperType.Contains("VARCHAR", StringComparison.Ordinal))
            {
                // NVARCHAR max is 4000, VARCHAR max is 8000 - use MAX for larger values
                var isNVarchar = upperType.Contains("NVARCHAR", StringComparison.Ordinal);
                var maxAllowed = isNVarchar ? 4000 : 8000;

                string length;
                if (!maxLength.HasValue || maxLength.Value <= 0 || maxLength.Value > maxAllowed)
                {
                    length = "MAX";
                }
                else
                {
                    length = maxLength.Value.ToString(CultureInfo.InvariantCulture);
                }

                return isNVarchar ? $"NVARCHAR({length})" : $"VARCHAR({length})";
            }

            if (upperType.Contains("CHAR", StringComparison.Ordinal))
            {
                var length = maxLength ?? 50;
                return upperType.Contains("NCHAR", StringComparison.Ordinal)
                    ? $"NCHAR({length})"
                    : $"CHAR({length})";
            }

            return upperType switch
            {
                _ when upperType.Contains("INT", StringComparison.Ordinal) => "INT",
                _ when upperType.Contains("BIGINT", StringComparison.Ordinal) => "BIGINT",
                _ when upperType.Contains("SMALLINT", StringComparison.Ordinal) => "SMALLINT",
                _ when upperType.Contains("TINYINT", StringComparison.Ordinal) => "TINYINT",
                _ when upperType.Contains("BIT", StringComparison.Ordinal) => "BIT",
                _ when upperType.Contains("DATE", StringComparison.Ordinal) => "DATETIME",
                _ when upperType.Contains("TIME", StringComparison.Ordinal) => "DATETIME",
                _ when upperType.Contains("DECIMAL", StringComparison.Ordinal) => "DECIMAL(18,2)",
                _ when upperType.Contains("NUMERIC", StringComparison.Ordinal) => "NUMERIC(18,2)",
                _ when upperType.Contains("MONEY", StringComparison.Ordinal) => "MONEY",
                _ when upperType.Contains("FLOAT", StringComparison.Ordinal) => "FLOAT",
                _ when upperType.Contains("REAL", StringComparison.Ordinal) => "REAL",
                _ when upperType.Contains("UNIQUEIDENTIFIER", StringComparison.Ordinal) => "UNIQUEIDENTIFIER",
                _ => "NVARCHAR(MAX)",
            };
        }

        private static bool IsTextType(string dataType)
        {
            var upperType = dataType.ToUpperInvariant();
            return upperType.Contains("CHAR", StringComparison.Ordinal) ||
                   upperType.Contains("TEXT", StringComparison.Ordinal);
        }
    }
}
