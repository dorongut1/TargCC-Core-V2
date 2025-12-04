// <copyright file="SpUpdateTemplate.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.Sql.Templates
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Template for generating SP_Update stored procedure.
    /// </summary>
    public class SpUpdateTemplate
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpUpdateTemplate"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public SpUpdateTemplate(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates the SP_Update stored procedure for a table.
        /// </summary>
        /// <param name="table">The table to generate the procedure for.</param>
        /// <returns>The generated SQL stored procedure.</returns>
        /// <exception cref="ArgumentNullException">Thrown when table is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when table has no primary key.</exception>
        public Task<string> GenerateAsync(Table table)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var pkColumns = table.Columns.Where(c => c.IsPrimaryKey).ToList();
            if (!pkColumns.Any())
            {
                throw new InvalidOperationException($"Table '{table.Name}' has no primary key. Cannot generate Update procedure.");
            }

            var sb = new StringBuilder();

            // Get updateable columns (exclude PK, AddedOn, AddedBy, calculated, business logic, aggregate, one-way encrypted)
            var updateableColumns = table.Columns.Where(c =>
                !c.IsPrimaryKey &&
                c.Name != "AddedOn" &&
                c.Name != "AddedBy" &&
                !c.Name.StartsWith("clc_", StringComparison.OrdinalIgnoreCase) && // Calculated
                !c.Name.StartsWith("blg_", StringComparison.OrdinalIgnoreCase) && // Business logic
                !c.Name.StartsWith("agg_", StringComparison.OrdinalIgnoreCase) && // Aggregate
                !c.Name.StartsWith("eno_", StringComparison.OrdinalIgnoreCase)    // One-way encryption
            ).ToList();

            // Check for audit columns
            var hasChangedOn = table.Columns.Any(c => c.Name == "ChangedOn");
            var hasChangedBy = table.Columns.Any(c => c.Name == "ChangedBy");

            // Remove ChangedOn from updateable columns (will be auto-set to GETDATE())
            if (hasChangedOn)
            {
                updateableColumns = updateableColumns.Where(c => c.Name != "ChangedOn").ToList();
            }

            // Procedure header
            sb.AppendLine($"CREATE OR ALTER PROCEDURE [dbo].[SP_Update{table.Name}]");

            // Parameters: PK columns first
            foreach (var col in pkColumns)
            {
                var sqlType = MapToSqlType(col);
                sb.AppendLine($"    @{col.Name} {sqlType},");
            }

            // Check if there are any updateable columns
            if (!updateableColumns.Any() && !hasChangedBy)
            {
                // No updateable columns - generate no-op procedure
                sb.AppendLine($"    @DummyParam int = NULL"); // Dummy parameter to avoid syntax error
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

                return Task.FromResult(sb.ToString());
            }

            // Parameters: updateable columns
            foreach (var col in updateableColumns)
            {
                var sqlType = MapToSqlType(col);
                var nullable = col.IsNullable ? " = NULL" : string.Empty;
                sb.AppendLine($"    @{col.Name} {sqlType}{nullable},");
            }

            // ChangedBy parameter (if exists)
            if (hasChangedBy)
            {
                var changedByCol = table.Columns.First(c => c.Name == "ChangedBy");
                var sqlType = MapToSqlType(changedByCol);
                sb.AppendLine($"    @ChangedBy {sqlType}");
            }
            else
            {
                // Remove trailing comma from last updateable column
                var content = sb.ToString().TrimEnd();
                if (content.EndsWith(","))
                {
                    sb.Clear();
                    sb.Append(content[..^1]);
                    sb.AppendLine();
                }
            }

            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();

            // UPDATE statement
            sb.AppendLine($"    UPDATE [{table.Name}]");
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

                sb.Append($"[{col.Name}] = @{col.Name}");
            }

            sb.AppendLine(";");
            sb.AppendLine("END");

            return Task.FromResult(sb.ToString());
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
