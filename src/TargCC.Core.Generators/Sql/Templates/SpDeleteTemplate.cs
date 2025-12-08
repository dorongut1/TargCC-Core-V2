// <copyright file="SpDeleteTemplate.cs" company="PlaceholderCompany">
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
    /// Template for generating SP_Delete stored procedure.
    /// </summary>
    public static class SpDeleteTemplate
    {
        /// <summary>
        /// Generates the SP_Delete stored procedure for a table.
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
                throw new InvalidOperationException($"Table '{table.Name}' has no primary key. Cannot generate Delete procedure.");
            }

            // Use PascalCase conversion for procedure name consistency with Repository
            var entityName = API.BaseApiGenerator.GetClassName(table.Name);

            var sb = new StringBuilder();

            // Procedure header
            sb.AppendLine(CultureInfo.InvariantCulture, $"CREATE OR ALTER PROCEDURE [dbo].[SP_Delete{entityName}]");

            // Parameters (PK columns)
            for (int i = 0; i < pkColumns.Count; i++)
            {
                var col = pkColumns[i];
                var sqlType = MapToSqlType(col);
                sb.Append(CultureInfo.InvariantCulture, $"    @{col.Name} {sqlType}");
                sb.AppendLine(i < pkColumns.Count - 1 ? "," : string.Empty);
            }

            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();

            // DELETE statement
            sb.AppendLine(CultureInfo.InvariantCulture, $"    DELETE FROM [{table.Name}]");

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

            return Task.FromResult(sb.ToString());
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
