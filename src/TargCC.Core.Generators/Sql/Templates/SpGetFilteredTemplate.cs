// <copyright file="SpGetFilteredTemplate.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.Sql.Templates
{
    using System;
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
                // No filterable indexes, return empty
                return Task.FromResult(string.Empty);
            }

            var sb = new StringBuilder();

            // Procedure header
            sb.AppendLine(CultureInfo.InvariantCulture, $"CREATE OR ALTER PROCEDURE [dbo].[SP_GetFiltered{table.Name}s]");

            // Add parameters for each indexed column
            var parameters = new List<string>();
            foreach (var index in filterableIndexes)
            {
                foreach (var columnName in index.ColumnNames)
                {
                    var column = table.Columns.Find(c => c.Name == columnName);
                    if (column != null)
                    {
                        // Avoid duplicate parameters
                        var paramName = $"@{columnName}";
                        if (!parameters.Contains(paramName))
                        {
                            parameters.Add(paramName);
                            var sqlType = GetSqlType(column.DataType, column.MaxLength);
                            sb.AppendLine(CultureInfo.InvariantCulture, $"    {paramName} {sqlType} = NULL,");
                        }
                    }
                }
            }

            // Add pagination parameters
            sb.AppendLine("    @Skip INT = NULL,");
            sb.AppendLine("    @Take INT = NULL");
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();

            // SELECT statement
            sb.AppendLine("    SELECT");

            var allColumns = table.Columns.ToList();
            for (int i = 0; i < allColumns.Count; i++)
            {
                var col = allColumns[i];
                sb.Append(CultureInfo.InvariantCulture, $"        [{col.Name}]");
                sb.AppendLine(i < allColumns.Count - 1 ? "," : string.Empty);
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"    FROM [{table.Name}]");

            // WHERE clause
            sb.AppendLine("    WHERE 1=1");

            foreach (var index in filterableIndexes)
            {
                foreach (var columnName in index.ColumnNames)
                {
                    var column = table.Columns.Find(c => c.Name == columnName);
                    if (column != null)
                    {
                        var isTextType = IsTextType(column.DataType);
                        var paramName = $"@{columnName}";

                        if (isTextType)
                        {
                            // LIKE for text columns
                            sb.AppendLine(CultureInfo.InvariantCulture, $"      AND ({paramName} IS NULL OR [{columnName}] LIKE '%' + {paramName} + '%')");
                        }
                        else
                        {
                            // Exact match for non-text columns
                            sb.AppendLine(CultureInfo.InvariantCulture, $"      AND ({paramName} IS NULL OR [{columnName}] = {paramName})");
                        }
                    }
                }
            }

            // Order by primary key
            var pkColumns = table.Columns.Where(c => c.IsPrimaryKey).ToList();
            if (pkColumns.Count > 0)
            {
                sb.Append("    ORDER BY ");
                for (int i = 0; i < pkColumns.Count; i++)
                {
                    var col = pkColumns[i];
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(CultureInfo.InvariantCulture, $"[{col.Name}]");
                }

                sb.AppendLine();

                // Add pagination using OFFSET/FETCH
                sb.AppendLine("    OFFSET ISNULL(@Skip, 0) ROWS");
                sb.AppendLine("    FETCH NEXT ISNULL(@Take, 2147483647) ROWS ONLY;");
            }
            else
            {
                sb.AppendLine(";");
            }

            sb.AppendLine("END");

            return Task.FromResult(sb.ToString());
        }

        private static string GetSqlType(string dataType, int? maxLength)
        {
            var upperType = dataType.ToUpperInvariant();

            if (upperType.Contains("VARCHAR", StringComparison.Ordinal))
            {
                var length = maxLength.HasValue && maxLength.Value > 0 ? maxLength.Value.ToString(CultureInfo.InvariantCulture) : "MAX";
                return upperType.Contains("NVARCHAR", StringComparison.Ordinal)
                    ? $"NVARCHAR({length})"
                    : $"VARCHAR({length})";
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
