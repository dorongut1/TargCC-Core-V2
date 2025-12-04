// <copyright file="SpGetByIndexTemplate.cs" company="PlaceholderCompany">
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
    using IndexModel = TargCC.Core.Interfaces.Models.Index;

    /// <summary>
    /// Template for generating SP_GetByIndex stored procedures.
    /// </summary>
    public static class SpGetByIndexTemplate
    {
        /// <summary>
        /// Generates stored procedures for all indexes on a table.
        /// </summary>
        /// <param name="table">The table to generate procedures for.</param>
        /// <returns>The generated SQL stored procedures.</returns>
        public static Task<string> GenerateAllIndexProcedures(Table table)
        {
            if (table == null)
            {
                return Task.FromResult(string.Empty);
            }

            if (table.Indexes == null || table.Indexes.Count == 0)
            {
                return Task.FromResult(string.Empty);
            }

            var sb = new StringBuilder();

            foreach (var index in table.Indexes)
            {
                if (ShouldSkipIndex(index))
                {
                    continue;
                }

                var procSql = GenerateProcedureForIndex(table, index);
                sb.AppendLine(procSql);
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            var result = sb.ToString().TrimEnd();
            return Task.FromResult(result);
        }

        private static bool ShouldSkipIndex(IndexModel index)
        {
            // Skip primary key indexes (handled by GetByID procedure)
            if (index.IsPrimaryKey)
            {
                return true;
            }

            // Skip if no columns in index
            if (index.ColumnNames == null || index.ColumnNames.Count == 0)
            {
                return true;
            }

            return false;
        }

        private static string GenerateProcedureForIndex(Table table, IndexModel index)
        {
            var sb = new StringBuilder();
            var prefix = index.IsUnique ? "Get" : "Fill";
            var columnNames = string.Join("And", index.ColumnNames.Select(ToTitleCase));
            var procName = $"SP_{prefix}{table.Name}By{columnNames}";

            AppendHeader(sb, index);
            AppendProcedureDeclaration(sb, procName, table, index);
            AppendSelectStatement(sb, table, index);

            return sb.ToString();
        }

        private static void AppendHeader(StringBuilder sb, IndexModel index)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"-- Procedure for Index: {index.Name}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"-- Index Type: {(index.IsUnique ? "Unique" : "Non-Unique")}");
            sb.AppendLine();
        }

        private static void AppendProcedureDeclaration(StringBuilder sb, string procName, Table table, IndexModel index)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"CREATE OR ALTER PROCEDURE [dbo].[{procName}]");

            var indexColumns = index.ColumnNames
                .Select(colName => table.Columns.Find(c => c.Name == colName))
                .Where(c => c != null)
                .ToList();

            for (int i = 0; i < indexColumns.Count; i++)
            {
                var col = indexColumns[i];
                var sqlType = MapToSqlType(col!);
                sb.Append(CultureInfo.InvariantCulture, $"    @{col!.Name} {sqlType}");
                sb.AppendLine(i < indexColumns.Count - 1 ? "," : string.Empty);
            }

            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();
        }

        private static void AppendSelectStatement(StringBuilder sb, Table table, IndexModel index)
        {
            sb.AppendLine("    SELECT");

            var allColumns = table.Columns.ToList();
            for (int i = 0; i < allColumns.Count; i++)
            {
                sb.Append(CultureInfo.InvariantCulture, $"        [{allColumns[i].Name}]");
                sb.AppendLine(i < allColumns.Count - 1 ? "," : string.Empty);
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"    FROM [{table.Name}]");

            AppendWhereClause(sb, table, index);
            AppendOrderByClause(sb, table, index);

            sb.AppendLine("END");
        }

        private static void AppendWhereClause(StringBuilder sb, Table table, IndexModel index)
        {
            var indexColumns = index.ColumnNames
                .Select(colName => table.Columns.Find(c => c.Name == colName))
                .Where(c => c != null)
                .ToList();

            sb.Append("    WHERE ");
            for (int i = 0; i < indexColumns.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(" AND ");
                }

                sb.Append(CultureInfo.InvariantCulture, $"[{indexColumns[i]!.Name}] = @{indexColumns[i]!.Name}");
            }

            sb.AppendLine();
        }

        private static void AppendOrderByClause(StringBuilder sb, Table table, IndexModel index)
        {
            if (!index.IsUnique)
            {
                var pkColumns = table.Columns.Where(c => c.IsPrimaryKey).ToList();
                if (pkColumns.Count > 0)
                {
                    sb.Append("    ORDER BY ");
                    for (int i = 0; i < pkColumns.Count; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(", ");
                        }

                        sb.Append(CultureInfo.InvariantCulture, $"[{pkColumns[i].Name}]");
                    }

                    sb.AppendLine(";");
                    return;
                }
            }

            sb.AppendLine(";");
        }

        private static string ToTitleCase(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var parts = text.Split('_', StringSplitOptions.RemoveEmptyEntries);
            var titleCased = parts.Select(p =>
            {
                return p.Length == 0 ? p : char.ToUpperInvariant(p[0]) + p[1..].ToUpperInvariant();
            });

            return string.Join(string.Empty, titleCased);
        }

        private static string MapToSqlType(Column column)
        {
            var type = column.DataType.ToUpperInvariant();

            return type switch
            {
                "INT" => "int",
                "BIGINT" => "bigint",
                "SMALLINT" => "smallint",
                "TINYINT" => "tinyint",
                "BIT" => "bit",
                "DECIMAL" or "NUMERIC" => FormatDecimalType(column),
                "MONEY" => "money",
                "SMALLMONEY" => "smallmoney",
                "FLOAT" => "float",
                "REAL" => "real",
                "DATE" => "date",
                "DATETIME" => "datetime",
                "DATETIME2" => "datetime2",
                "SMALLDATETIME" => "smalldatetime",
                "TIME" => "time",
                "DATETIMEOFFSET" => "datetimeoffset",
                "CHAR" => FormatStringType("char", column.MaxLength),
                "VARCHAR" => FormatStringType("varchar", column.MaxLength),
                "TEXT" => "text",
                "NCHAR" => FormatStringType("nchar", column.MaxLength),
                "NVARCHAR" => FormatStringType("nvarchar", column.MaxLength),
                "NTEXT" => "ntext",
                "BINARY" => FormatStringType("binary", column.MaxLength),
                "VARBINARY" => FormatStringType("varbinary", column.MaxLength),
                "IMAGE" => "image",
                "UNIQUEIDENTIFIER" => "uniqueidentifier",
                "XML" => "xml",
                _ => type.ToUpperInvariant()
            };
        }

        private static string FormatDecimalType(Column column)
        {
            var precision = column.PrecisionNumeric ?? 18;
            var scale = column.ScaleNumeric ?? 0;
            return $"decimal({precision},{scale})";
        }

        private static string FormatStringType(string baseType, int? maxLength)
        {
            if (maxLength == -1)
            {
                return $"{baseType}(MAX)";
            }

            return maxLength.HasValue ? $"{baseType}({maxLength.Value})" : baseType;
        }
    }
}
