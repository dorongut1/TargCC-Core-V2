// <copyright file="SpGetRelatedTemplate.cs" company="PlaceholderCompany">
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
    /// Template for generating SP_Get{Parent}{Children} stored procedures.
    /// Generates procedures to fetch related child records based on parent FK relationship.
    /// Example: SP_GetCustomerOrders fetches all orders for a given customer.
    /// </summary>
    public static class SpGetRelatedTemplate
    {
        /// <summary>
        /// Generates a stored procedure to fetch related child records.
        /// </summary>
        /// <param name="parentTable">The parent table (e.g., Customer).</param>
        /// <param name="childTable">The child table (e.g., Order).</param>
        /// <param name="relationship">The FK relationship between tables.</param>
        /// <returns>The generated SQL stored procedure.</returns>
        /// <exception cref="ArgumentNullException">Thrown when parameters are null.</exception>
        public static Task<string> GenerateAsync(
            Table parentTable,
            Table childTable,
            Relationship relationship)
        {
            ArgumentNullException.ThrowIfNull(parentTable);
            ArgumentNullException.ThrowIfNull(childTable);
            ArgumentNullException.ThrowIfNull(relationship);

            var sb = new StringBuilder();

            // Pluralize child table name (Order → Orders)
            var childrenName = Pluralize(childTable.Name);

            // Procedure name: SP_Get{Parent}{Children}
            // Example: SP_GetCustomerOrders
            var procName = $"SP_Get{parentTable.Name}{childrenName}";

            // Get parent PK column
            var parentPkColumn = parentTable.Columns.Find(c => c.IsPrimaryKey)
                ?? throw new InvalidOperationException($"Parent table '{parentTable.Name}' has no primary key.");

            // Get child FK column
            var childFkColumn = childTable.Columns.Find(c => c.Name == relationship.ChildColumn)
                ?? throw new InvalidOperationException($"Child table '{childTable.Name}' does not have FK column '{relationship.ChildColumn}'.");

            // Header comment
            sb.AppendLine("-- =========================================");
            sb.AppendLine(CultureInfo.InvariantCulture, $"-- {procName}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"-- Fetches all {childrenName.ToLower(CultureInfo.CurrentCulture)} for a given {parentTable.Name.ToLower(CultureInfo.CurrentCulture)}");
            sb.AppendLine($"-- =========================================");

            // Procedure declaration
            sb.AppendLine(CultureInfo.InvariantCulture, $"CREATE OR ALTER PROCEDURE [dbo].[{procName}]");

            // Parameters
            GenerateParameters(sb, parentPkColumn);

            // Procedure body
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();

            // SELECT statement
            GenerateSelectStatement(sb, childTable, childFkColumn, parentPkColumn);

            sb.AppendLine("END");

            return Task.FromResult(sb.ToString());
        }

        /// <summary>
        /// Generates all SP_GetRelated procedures for a parent table's relationships.
        /// </summary>
        /// <param name="parentTable">The parent table.</param>
        /// <param name="schema">The full database schema.</param>
        /// <returns>SQL for all related data procedures.</returns>
        public static async Task<string> GenerateAllRelatedProcedures(
            Table parentTable,
            DatabaseSchema schema)
        {
            ArgumentNullException.ThrowIfNull(parentTable);
            ArgumentNullException.ThrowIfNull(schema);

            if (schema.Relationships == null || schema.Relationships.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            // Find all relationships where this table is the parent
            var parentRelationships = schema.Relationships
                .Where(r => r.ParentTable == parentTable.FullName && r.IsEnabled)
                .ToList();

            foreach (var relationship in parentRelationships)
            {
                var childTable = schema.Tables.Find(t => t.FullName == relationship.ChildTable);
                if (childTable == null)
                {
                    continue;
                }

                try
                {
                    var relatedSp = await GenerateAsync(parentTable, childTable, relationship);
                    sb.AppendLine(relatedSp);
                    sb.AppendLine("GO");
                    sb.AppendLine();
                }
                catch (Exception ex)
                {
                    sb.AppendLine(CultureInfo.InvariantCulture, $"-- Warning: Could not generate SP_Get{parentTable.Name}{Pluralize(childTable.Name)}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"-- Reason: {ex.Message}");
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private static void GenerateParameters(StringBuilder sb, Column parentPkColumn)
        {
            var pkType = MapToSqlType(parentPkColumn);

            sb.AppendLine(CultureInfo.InvariantCulture, $"    @{parentPkColumn.Name} {pkType},");
            sb.AppendLine("    @Skip INT = NULL,");
            sb.AppendLine("    @Take INT = NULL");
        }

        private static void GenerateSelectStatement(
            StringBuilder sb,
            Table childTable,
            Column childFkColumn,
            Column parentPkColumn)
        {
            // SELECT all displayable columns
            sb.AppendLine("    SELECT");

            var selectColumns = childTable.Columns
                .Where(c => !c.Name.StartsWith("eno_", StringComparison.OrdinalIgnoreCase)) // Exclude one-way encrypted
                .Select(c => $"        [{c.Name}]")
                .ToList();

            sb.AppendLine(string.Join(",\n", selectColumns));

            // FROM clause
            sb.AppendLine(CultureInfo.InvariantCulture, $"    FROM [{childTable.Name}]");

            // WHERE clause
            sb.AppendLine(CultureInfo.InvariantCulture, $"    WHERE [{childFkColumn.Name}] = @{parentPkColumn.Name}");

            // ORDER BY (try to order by date column if exists, otherwise by PK)
            var orderByColumn = childTable.Columns.Find(c =>
                c.Name.Contains("Date", StringComparison.OrdinalIgnoreCase) ||
                c.Name.Contains("Time", StringComparison.OrdinalIgnoreCase) ||
                c.Name.Contains("AddedOn", StringComparison.OrdinalIgnoreCase))
                ?? childTable.Columns.Find(c => c.IsPrimaryKey);

            if (orderByColumn != null)
            {
                var orderDirection = orderByColumn.Name.Contains("Date", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
                sb.AppendLine(CultureInfo.InvariantCulture, $"    ORDER BY [{orderByColumn.Name}] {orderDirection}");
            }

            // OFFSET/FETCH for pagination
            sb.AppendLine("    OFFSET COALESCE(@Skip, 0) ROWS");
            sb.AppendLine("    FETCH NEXT COALESCE(@Take, 1000) ROWS ONLY;");
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

        private static string Pluralize(string singular)
        {
            if (string.IsNullOrEmpty(singular))
            {
                return singular;
            }

            // Simple English pluralization rules
            // CA1867: String literals required here because char overload doesn't support StringComparison
#pragma warning disable CA1867
            if (singular.EndsWith("y", StringComparison.OrdinalIgnoreCase) &&
                !singular.EndsWith("ay", StringComparison.OrdinalIgnoreCase) &&
                !singular.EndsWith("ey", StringComparison.OrdinalIgnoreCase) &&
                !singular.EndsWith("oy", StringComparison.OrdinalIgnoreCase) &&
                !singular.EndsWith("uy", StringComparison.OrdinalIgnoreCase))
            {
                // Category → Categories
                return singular[..^1] + "ies";
            }

            if (singular.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
                singular.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
                singular.EndsWith("z", StringComparison.OrdinalIgnoreCase) ||
                singular.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
                singular.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
#pragma warning restore CA1867
            {
                // Address → Addresses, Box → Boxes
                return singular + "es";
            }

            // Default: just add 's'
            // Order → Orders, Customer → Customers
            return singular + "s";
        }
    }
}
