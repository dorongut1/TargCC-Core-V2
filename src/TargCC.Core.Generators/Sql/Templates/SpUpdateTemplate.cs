// <copyright file="SpUpdateTemplate.cs" company="PlaceholderCompany">
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
    /// Template for generating SP_Update stored procedure.
    /// </summary>
    public static class SpUpdateTemplate
    {
        /// <summary>
        /// Generates the SP_Update stored procedure for a table.
        /// </summary>
        /// <param name="table">The table to generate the procedure for.</param>
        /// <returns>The generated SQL stored procedure.</returns>
        /// <exception cref="ArgumentNullException">Thrown when table is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when table has no primary key.</exception>
        public static Task<string> GenerateAsync(Table table)
        {
            ArgumentNullException.ThrowIfNull(table);

            var pkColumns = table.Columns.Where(c => c.IsPrimaryKey).ToList();
            if (pkColumns.Count == 0)
            {
                throw new InvalidOperationException($"Table '{table.Name}' has no primary key. Cannot generate Update procedure.");
            }

            var sb = new StringBuilder();

            // Get updateable columns
            var updateableColumns = GetUpdateableColumns(table);

            // Check for audit columns
            var hasChangedOn = table.Columns.Any(c => c.Name == "ChangedOn");
            var hasChangedBy = table.Columns.Any(c => c.Name == "ChangedBy");

            // Remove ChangedOn from updateable columns (will be auto-set to GETDATE())
            if (hasChangedOn)
            {
                updateableColumns = updateableColumns.Where(c => c.Name != "ChangedOn").ToList();
            }

            // Procedure header
            sb.AppendLine(CultureInfo.InvariantCulture, $"CREATE OR ALTER PROCEDURE [dbo].[SP_Update{table.Name}]");

            // Check if there are any updateable columns
            if (updateableColumns.Count == 0 && !hasChangedBy)
            {
                GenerateNoOpProcedure(sb);
                return Task.FromResult(sb.ToString());
            }

            GenerateParameters(sb, pkColumns, updateableColumns, hasChangedBy, table);
            GenerateUpdateStatement(sb, table, pkColumns, updateableColumns, hasChangedOn, hasChangedBy);

            return Task.FromResult(sb.ToString());
        }

        private static System.Collections.Generic.List<Column> GetUpdateableColumns(Table table)
        {
            return table.Columns.Where(c =>
                !c.IsPrimaryKey &&
                c.Name != "AddedOn" &&
                c.Name != "AddedBy" &&
                !c.Name.StartsWith("clc_", StringComparison.OrdinalIgnoreCase) && // Calculated
                !c.Name.StartsWith("blg_", StringComparison.OrdinalIgnoreCase) && // Business logic
                !c.Name.StartsWith("agg_", StringComparison.OrdinalIgnoreCase) && // Aggregate
                !c.Name.StartsWith("eno_", StringComparison.OrdinalIgnoreCase)    // One-way encryption
            ).ToList();
        }

        private static void GenerateNoOpProcedure(StringBuilder sb)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"    @DummyParam int = NULL");
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();
            sb.AppendLine("    -- No updateable columns in this table");
            sb.AppendLine("    -- This procedure exists for API consistency");
            sb.AppendLine("    -- All columns are either primary keys, calculated, business logic, or aggregate columns");
            sb.AppendLine();
            sb.AppendLine("    SELECT 1 AS Result;");
            sb.AppendLine("END");
        }

        private static void GenerateParameters(
            StringBuilder sb,
            System.Collections.Generic.List<Column> pkColumns,
            System.Collections.Generic.List<Column> updateableColumns,
            bool hasChangedBy,
            Table table)
        {
            // Parameters: PK columns first
            foreach (var col in pkColumns)
            {
                var sqlType = MapToSqlType(col);
                sb.AppendLine(CultureInfo.InvariantCulture, $"    @{col.Name} {sqlType},");
            }

            // Parameters: updateable columns
            foreach (var col in updateableColumns)
            {
                var sqlType = MapToSqlType(col);
                var nullable = col.IsNullable ? " = NULL" : string.Empty;
                sb.AppendLine(CultureInfo.InvariantCulture, $"    @{col.Name} {sqlType}{nullable},");
            }

            // ChangedBy parameter (if exists)
            if (hasChangedBy)
            {
                var changedByCol = table.Columns.Find(c => c.Name == "ChangedBy");
                var sqlType = MapToSqlType(changedByCol!);
                sb.AppendLine(CultureInfo.InvariantCulture, $"    @ChangedBy {sqlType}");
            }
            else
            {
                // Remove trailing comma from last updateable column
                var content = sb.ToString().TrimEnd();
                if (content.EndsWith(",", StringComparison.Ordinal))
                {
                    sb.Clear();
                    sb.Append(content[..^1]);
                    sb.AppendLine();
                }
            }
        }

        private static void GenerateUpdateStatement(
            StringBuilder sb,
            Table table,
            System.Collections.Generic.List<Column> pkColumns,
            System.Collections.Generic.List<Column> updateableColumns,
            bool hasChangedOn,
            bool hasChangedBy)
        {
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();

            // UPDATE statement
            sb.AppendLine(CultureInfo.InvariantCulture, $"    UPDATE [{table.Name}]");
            sb.AppendLine("    SET");

            var setStatements = new System.Collections.Generic.List<string>();

            // Regular updateable columns
            foreach (var col in updateableColumns)
            {
                setStatements.Add($"        [{col.Name}] = @{col.Name}");
            }

            // Auto-set ChangedOn
            if (hasChangedOn)
            {
                setStatements.Add($"        [ChangedOn] = GETDATE()");
            }

            // Set ChangedBy
            if (hasChangedBy)
            {
                setStatements.Add($"        [ChangedBy] = @ChangedBy");
            }

            sb.AppendLine(string.Join(",\n", setStatements));

            // WHERE clause
            sb.Append("    WHERE ");
            for (int i = 0; i < pkColumns.Count; i++)
            {
                var col = pkColumns[i];
                if (i > 0)
                {
                    sb.Append(" AND ");
                }

                sb.Append(CultureInfo.InvariantCulture, $"[{col.Name}] = @{col.Name}");
            }

            sb.AppendLine(";");
            sb.AppendLine("END");
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
