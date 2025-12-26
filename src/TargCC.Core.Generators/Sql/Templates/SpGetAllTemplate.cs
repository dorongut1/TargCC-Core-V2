// <copyright file="SpGetAllTemplate.cs" company="PlaceholderCompany">
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
    /// Template for generating SP_GetAll stored procedure.
    /// </summary>
    public static class SpGetAllTemplate
    {
        /// <summary>
        /// Generates the SP_GetAll stored procedure for a table.
        /// </summary>
        /// <param name="table">The table to generate the procedure for.</param>
        /// <returns>The generated SQL stored procedure.</returns>
        /// <exception cref="ArgumentNullException">Thrown when table is null.</exception>
        public static Task<string> GenerateAsync(Table table)
        {
            ArgumentNullException.ThrowIfNull(table);

            // Use PascalCase conversion for procedure name consistency with Repository
            var entityName = API.BaseApiGenerator.GetClassName(table.Name);

            var sb = new StringBuilder();

            // Procedure header with pagination and WithParentText parameters
            sb.AppendLine(CultureInfo.InvariantCulture, $"CREATE OR ALTER PROCEDURE [dbo].[SP_GetAll{entityName}s]");
            sb.AppendLine("    @Skip INT = NULL,");
            sb.AppendLine("    @Take INT = NULL,");
            sb.AppendLine("    @WithParentText BIT = 1");
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();

            // Get FK columns for parent text lookup
            var fkColumns = GetForeignKeyColumns(table);

            if (fkColumns.Count > 0)
            {
                // Generate two branches: with and without parent text
                GenerateWithParentTextBranch(sb, table, fkColumns);
            }
            else
            {
                // No FK columns, generate simple SELECT
                GenerateSimpleSelect(sb, table);
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

        private static void GenerateWithParentTextBranch(
            StringBuilder sb,
            Table table,
            List<Column> fkColumns)
        {
            // IF @WithParentText = 1
            sb.AppendLine("    IF @WithParentText = 1");
            sb.AppendLine("    BEGIN");

            GenerateSelectWithJoins(sb, table, fkColumns);

            sb.AppendLine("    END");
            sb.AppendLine("    ELSE");
            sb.AppendLine("    BEGIN");

            GenerateSimpleSelect(sb, table, indent: "        ");

            sb.AppendLine("    END");
        }

        private static void GenerateSelectWithJoins(StringBuilder sb, Table table, List<Column> fkColumns)
        {
            sb.AppendLine("        SELECT");

            var allColumns = table.Columns.ToList();
            var tableAlias = "t";

            // Add all base columns with table alias
            for (int i = 0; i < allColumns.Count; i++)
            {
                var col = allColumns[i];
                sb.Append(CultureInfo.InvariantCulture, $"            {tableAlias}.[{col.Name}]");
                sb.AppendLine(",");
            }

            // Add parent text columns from joined tables
            for (int i = 0; i < fkColumns.Count; i++)
            {
                var fkCol = fkColumns[i];
                var parentAlias = $"p{i + 1}";
                var parentTextColumnName = $"{fkCol.Name}_Text";

                // Get text from ccvwComboList view
                sb.Append(CultureInfo.InvariantCulture, $"            {parentAlias}.[Text] AS [{parentTextColumnName}]");
                sb.AppendLine(i < fkColumns.Count - 1 ? "," : string.Empty);
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        FROM [{table.Name}] {tableAlias}");

            // Add LEFT JOINs for each FK column
            for (int i = 0; i < fkColumns.Count; i++)
            {
                var fkCol = fkColumns[i];
                var parentAlias = $"p{i + 1}";
                var comboViewName = $"ccvwComboList_{fkCol.ReferencedTable}";

                // Use LEFT JOIN to avoid missing rows when parent doesn't exist
                sb.AppendLine(CultureInfo.InvariantCulture, $"        LEFT JOIN [{comboViewName}] {parentAlias} ON {tableAlias}.[{fkCol.Name}] = {parentAlias}.[ID]");
            }

            // Order by and pagination
            GenerateOrderByAndPagination(sb, table, tableAlias, "        ");
        }

        private static void GenerateSimpleSelect(StringBuilder sb, Table table, string indent = "    ")
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"{indent}SELECT");

            var allColumns = table.Columns.ToList();
            for (int i = 0; i < allColumns.Count; i++)
            {
                var col = allColumns[i];
                sb.Append(CultureInfo.InvariantCulture, $"{indent}    [{col.Name}]");
                sb.AppendLine(i < allColumns.Count - 1 ? "," : string.Empty);
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"{indent}FROM [{table.Name}]");

            GenerateOrderByAndPagination(sb, table, null, indent);
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

                // Add pagination using OFFSET/FETCH if parameters provided
                sb.AppendLine(CultureInfo.InvariantCulture, $"{indent}OFFSET ISNULL(@Skip, 0) ROWS");
                sb.AppendLine(CultureInfo.InvariantCulture, $"{indent}FETCH NEXT ISNULL(@Take, 2147483647) ROWS ONLY;");
            }
            else
            {
                sb.AppendLine(";");
            }
        }
    }
}
