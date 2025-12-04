// <copyright file="SpGetByIdTemplate.cs" company="PlaceholderCompany">
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
    /// Template for generating SP_GetByID stored procedure.
    /// </summary>
    public class SpGetByIdTemplate
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpGetByIdTemplate"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public SpGetByIdTemplate(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates the SP_GetByID stored procedure for a table.
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
                throw new InvalidOperationException($"Table '{table.Name}' has no primary key. Cannot generate GetByID procedure.");
            }

            var sb = new StringBuilder();

            // Procedure header
            sb.AppendLine($"CREATE OR ALTER PROCEDURE [dbo].[SP_Get{table.Name}ByID]");

            // Parameters (PK columns)
            for (int i = 0; i < pkColumns.Count; i++)
            {
                var col = pkColumns[i];
                var sqlType = MapToSqlType(col);
                sb.Append($"    @{col.Name} {sqlType}");
                if (i < pkColumns.Count - 1)
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
