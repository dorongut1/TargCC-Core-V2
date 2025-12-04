// <copyright file="SpGetByIndexTemplate.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.Sql.Templates
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TargCC.Core.Interfaces.Models;

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

            if (table.Indexes == null || !table.Indexes.Any())
            {
                return Task.FromResult(string.Empty);
            }

            var sb = new StringBuilder();

            foreach (var index in table.Indexes)
            {
                // Skip primary key indexes (handled by GetByID procedure)
                if (index.IsPrimaryKey)
                {
                    continue;
                }

                // Skip if no columns in index
                if (index.Columns == null || !index.Columns.Any())
                {
                    continue;
                }

                var procSql = GenerateProcedureForIndex(table, index);
                sb.AppendLine(procSql);
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            // Remove trailing newlines
            var result = sb.ToString().TrimEnd();
            return Task.FromResult(result);
        }

        private static string GenerateProcedureForIndex(Table table, Index index)
        {
            var sb = new StringBuilder();

            // Determine procedure name prefix based on uniqueness
            var prefix = index.IsUnique ? "Get" : "Fill";

            // Build procedure name from index columns
            var columnNames = string.Join("And", index.Columns.Select(ToTitleCase));
            var procName = $"SP_{prefix}{table.Name}By{columnNames}";

            // Header comment
            sb.AppendLine($"-- Procedure for Index: {index.Name}");
            sb.AppendLine($"-- Index Type: {(index.IsUnique ? "Unique" : "Non-Unique")}");
            sb.AppendLine();

            // Procedure declaration
            sb.AppendLine($"CREATE OR ALTER PROCEDURE [dbo].[{procName}]");

            // Parameters from index columns
            var indexColumns = index.Columns
                .Select(colName => table.Columns.FirstOrDefault(c => c.Name == colName))
                .Where(c => c != null)
                .ToList();

            for (int i = 0; i < indexColumns.Count; i++)
            {
                var col = indexColumns[i];
                var sqlType = MapToSqlType(col!);
                sb.Append($"    @{col!.Name} {sqlType}");
                if (i < indexColumns.Count - 1)
                {
                    sb.AppendLine(",");
                }
                else
                {
                    sb.AppendLine();
                }
            }

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
                sb.Append($"        [{col.Name}]");
                if (i < allColumns.Count - 1)
                {
                    sb.AppendLine(",");
                }
                else
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine($"    FROM [{table.Name}]");

            // WHERE clause
            sb.Append("    WHERE ");
            for (int i = 0; i < indexColumns.Count; i++)
            {
                var col = indexColumns[i];
                if (i > 0)
                {
                    sb.Append(" AND ");
                }

                sb.Append($"[{col!.Name}] = @{col.Name}");
            }

            sb.AppendLine();

            // ORDER BY clause for non-unique indexes
            if (!index.IsUnique)
            {
                var pkColumns = table.Columns.Where(c => c.IsPrimaryKey).ToList();
                if (pkColumns.Any())
                {
                    sb.Append("    ORDER BY ");
                    for (int i = 0; i < pkColumns.Count; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(", ");
                        }

                        sb.Append($"[{pkColumns[i].Name}]");
                    }

                    sb.AppendLine(";");
                }
                else
                {
                    sb.AppendLine(";");
                }
            }
            else
            {
                sb.AppendLine(";");
            }

            sb.AppendLine("END");

            return sb.ToString();
        }

        private static string ToTitleCase(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            // Remove underscores and convert to title case
            var parts = text.Split('_', StringSplitOptions.RemoveEmptyEntries);
            var titleCased = parts.Select(p =>
            {
                if (p.Length == 0)
                {
                    return p;
                }

                return char.ToUpperInvariant(p[0]) + p[1..].ToLowerInvariant();
            });

            return string.Join(string.Empty, titleCased);
        }

        private static string MapToSqlType(Column column)
        {
            var type = column.DataType.ToLowerInvariant();

            return type switch
            {
                "int" => "int",
                "bigint" => "bigint",
                "smallint" => "smallint",
                "tinyint" => "tinyint",
                "bit" => "bit",
                "decimal" or "numeric" => column.Scale.HasValue
                    ? $"decimal({column.Precision ?? 18},{column.Scale})"
                    : $"decimal({column.Precision ?? 18},0)",
                "money" => "money",
                "smallmoney" => "smallmoney",
                "float" => "float",
                "real" => "real",
                "date" => "date",
                "datetime" => "datetime",
                "datetime2" => "datetime2",
                "smalldatetime" => "smalldatetime",
                "time" => "time",
                "datetimeoffset" => "datetimeoffset",
                "char" => column.MaxLength == -1 ? "char(MAX)" : $"char({column.MaxLength})",
                "varchar" => column.MaxLength == -1 ? "varchar(MAX)" : $"varchar({column.MaxLength})",
                "text" => "text",
                "nchar" => column.MaxLength == -1 ? "nchar(MAX)" : $"nchar({column.MaxLength})",
                "nvarchar" => column.MaxLength == -1 ? "nvarchar(MAX)" : $"nvarchar({column.MaxLength})",
                "ntext" => "ntext",
                "binary" => column.MaxLength == -1 ? "binary(MAX)" : $"binary({column.MaxLength})",
                "varbinary" => column.MaxLength == -1 ? "varbinary(MAX)" : $"varbinary({column.MaxLength})",
                "image" => "image",
                "uniqueidentifier" => "uniqueidentifier",
                "xml" => "xml",
                _ => type
            };
        }
    }
}
