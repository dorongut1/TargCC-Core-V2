// <copyright file="SpAddTemplate.cs" company="PlaceholderCompany">
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
    /// Template for generating SP_Add stored procedure.
    /// </summary>
    public static class SpAddTemplate
    {
        /// <summary>
        /// Generates the SP_Add stored procedure for a table.
        /// </summary>
        /// <param name="table">The table to generate the procedure for.</param>
        /// <returns>The generated SQL stored procedure.</returns>
        /// <exception cref="ArgumentNullException">Thrown when table is null.</exception>
        public static Task<string> GenerateAsync(Table table)
        {
            ArgumentNullException.ThrowIfNull(table);

            var sb = new StringBuilder();

            // Get insertable columns (exclude IDENTITY, calculated, and auto-set columns)
            var insertableColumns = GetInsertableColumns(table);

            // Check for audit columns
            var hasAddedOn = table.Columns.Exists(c => c.Name == "AddedOn");
            var hasAddedBy = table.Columns.Exists(c => c.Name == "AddedBy");

            // Remove AddedOn from insertable columns (will be auto-set to GETDATE())
            if (hasAddedOn)
            {
                insertableColumns = insertableColumns.Where(c => c.Name != "AddedOn").ToList();
            }

            // Remove AddedBy from insertable columns (will be added as separate parameter)
            if (hasAddedBy)
            {
                insertableColumns = insertableColumns.Where(c => c.Name != "AddedBy").ToList();
            }

            // Procedure header
            sb.AppendLine(CultureInfo.InvariantCulture, $"CREATE OR ALTER PROCEDURE [dbo].[SP_Add{table.Name}]");

            // Check if there are any insertable columns
            if (insertableColumns.Count == 0 && !hasAddedBy)
            {
                GenerateNoOpProcedure(sb);
                return Task.FromResult(sb.ToString());
            }

            GenerateParameters(sb, insertableColumns, hasAddedBy, table);
            GenerateInsertStatement(sb, table, insertableColumns, hasAddedOn, hasAddedBy);

            return Task.FromResult(sb.ToString());
        }

        private static System.Collections.Generic.List<Column> GetInsertableColumns(Table table)
        {
            return table.Columns.Where(c =>
                !c.IsPrimaryKey && // Skip IDENTITY primary keys
                c.Name != "AddedOn" &&
                c.Name != "AddedBy" &&
                c.Name != "ChangedOn" && // Don't insert ChangedOn on creation
                c.Name != "ChangedBy" && // Don't insert ChangedBy on creation
                !c.Name.StartsWith("clc_", StringComparison.OrdinalIgnoreCase) && // Calculated
                !c.Name.StartsWith("blg_", StringComparison.OrdinalIgnoreCase) && // Business logic
                !c.Name.StartsWith("agg_", StringComparison.OrdinalIgnoreCase) && // Aggregate
                !c.Name.StartsWith("scb_", StringComparison.OrdinalIgnoreCase) && // Security
                !c.Name.StartsWith("eno_", StringComparison.OrdinalIgnoreCase)) // One-way encryption
            .ToList();
        }

        private static void GenerateNoOpProcedure(StringBuilder sb)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"    @DummyParam int = NULL");
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();
            sb.AppendLine("    -- No insertable columns in this table");
            sb.AppendLine("    -- This procedure exists for API consistency");
            sb.AppendLine();
            sb.AppendLine("    SELECT 0 AS NewID;");
            sb.AppendLine("END");
        }

        private static void GenerateParameters(
            StringBuilder sb,
            System.Collections.Generic.List<Column> insertableColumns,
            bool hasAddedBy,
            Table table)
        {
            // Parameters: insertable columns
            foreach (var col in insertableColumns)
            {
                var sqlType = MapToSqlType(col);
                var nullable = col.IsNullable ? " = NULL" : string.Empty;
                sb.AppendLine(CultureInfo.InvariantCulture, $"    @{col.Name} {sqlType}{nullable},");
            }

            // AddedBy parameter (if exists)
            if (hasAddedBy)
            {
                var addedByCol = table.Columns.Find(c => c.Name == "AddedBy");
                var sqlType = MapToSqlType(addedByCol!);
                sb.AppendLine(CultureInfo.InvariantCulture, $"    @AddedBy {sqlType}");
            }
            else
            {
                // Remove trailing comma from last insertable column
                var content = sb.ToString().TrimEnd();
                if (content.EndsWith(','))
                {
                    sb.Clear();
                    sb.Append(content[..^1]);
                    sb.AppendLine();
                }
            }
        }

        private static void GenerateInsertStatement(
            StringBuilder sb,
            Table table,
            System.Collections.Generic.List<Column> insertableColumns,
            bool hasAddedOn,
            bool hasAddedBy)
        {
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();

            // INSERT statement
            sb.AppendLine(CultureInfo.InvariantCulture, $"    INSERT INTO [{table.Name}] (");

            // Column names
            var columnNames = insertableColumns.Select(c => $"        [{c.Name}]").ToList();

            // Add audit columns
            if (hasAddedOn)
            {
                columnNames.Add("        [AddedOn]");
            }
            if (hasAddedBy)
            {
                columnNames.Add("        [AddedBy]");
            }

            sb.AppendLine(string.Join(",\n", columnNames));
            sb.AppendLine("    )");
            sb.AppendLine("    VALUES (");

            // Values
            var values = insertableColumns.Select(c => $"        @{c.Name}").ToList();

            // Add audit values
            if (hasAddedOn)
            {
                values.Add("        GETDATE()");
            }
            if (hasAddedBy)
            {
                values.Add("        @AddedBy");
            }

            sb.AppendLine(string.Join(",\n", values));
            sb.AppendLine("    );");
            sb.AppendLine();

            // Return the new ID
            var pkColumn = table.Columns.FirstOrDefault(c => c.IsPrimaryKey);
            if (pkColumn != null)
            {
                sb.AppendLine("    SELECT SCOPE_IDENTITY() AS NewID;");
            }

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
