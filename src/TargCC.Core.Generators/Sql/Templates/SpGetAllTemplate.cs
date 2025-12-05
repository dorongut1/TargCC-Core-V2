// <copyright file="SpGetAllTemplate.cs" company="PlaceholderCompany">
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

            var sb = new StringBuilder();

            // Procedure header
            sb.AppendLine(CultureInfo.InvariantCulture, $"CREATE OR ALTER PROCEDURE [dbo].[SP_GetAll{table.Name}s]");
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
                sb.AppendLine(";");
            }
            else
            {
                sb.AppendLine(";");
            }

            sb.AppendLine("END");

            return Task.FromResult(sb.ToString());
        }
    }
}
