// <copyright file="TypeMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.Entities
{
    using System;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Maps SQL Server data types to C# types.
    /// </summary>
    public static class TypeMapper
    {
        /// <summary>
        /// Maps a SQL column to its corresponding C# type.
        /// </summary>
        /// <param name="column">The column to map.</param>
        /// <returns>The C# type string (e.g., "int", "string", "DateTime?").</returns>
        public static string MapSqlTypeToCSharp(Column column)
        {
            ArgumentNullException.ThrowIfNull(column);

            var baseType = column.SqlDataType.ToUpperInvariant();

            var csharpType = baseType switch
            {
                // Integer types
                "INT" => "int",
                "BIGINT" => "long",
                "SMALLINT" => "short",
                "TINYINT" => "byte",

                // Decimal types
                "DECIMAL" or "NUMERIC" or "MONEY" or "SMALLMONEY" => "decimal",
                "FLOAT" => "double",
                "REAL" => "float",

                // String types
                "VARCHAR" or "NVARCHAR" or "CHAR" or "NCHAR" or "TEXT" or "NTEXT" => "string",

                // Date/Time types
                "DATETIME" or "DATETIME2" or "DATE" or "SMALLDATETIME" => "DateTime",
                "TIME" => "TimeSpan",
                "DATETIMEOFFSET" => "DateTimeOffset",

                // Binary types
                "VARBINARY" or "BINARY" or "IMAGE" => "byte[]",

                // Other types
                "BIT" => "bool",
                "UNIQUEIDENTIFIER" => "Guid",
                "XML" => "string",
                "SQL_VARIANT" => "string",  // SQL_VARIANT is stored as string to avoid object type
                "GEOGRAPHY" => "string",    // Spatial types stored as string (WKT format)
                "GEOMETRY" => "string",     // Spatial types stored as string (WKT format)
                "HIERARCHYID" => "string",  // HierarchyId stored as string

                // Fallback - use string instead of object to avoid EF Core mapping issues
                _ => "string",
            };

            // Add nullable modifier if needed
            if (column.IsNullable && IsValueType(csharpType))
            {
                return $"{csharpType}?";
            }

            return csharpType;
        }

        /// <summary>
        /// Gets the default value for a C# type.
        /// </summary>
        /// <param name="csharpType">The C# type string.</param>
        /// <returns>The default value as a string.</returns>
        public static string GetDefaultValue(string csharpType)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(csharpType);

            // Remove nullable modifier
            var baseType = csharpType.TrimEnd('?');

            return baseType switch
            {
                "int" => "0",
                "long" => "0L",
                "short" => "0",
                "byte" => "0",
                "decimal" => "0m",
                "double" => "0.0",
                "float" => "0f",
                "bool" => "false",
                "DateTime" => "DateTime.Now",
                "DateTimeOffset" => "DateTimeOffset.Now",
                "TimeSpan" => "TimeSpan.Zero",
                "Guid" => "Guid.Empty",
                "string" => "null",
                "byte[]" => "null",
                _ => csharpType.EndsWith('?') ? "null" : "default",
            };
        }

        /// <summary>
        /// Determines if a C# type is a value type.
        /// </summary>
        /// <param name="csharpType">The C# type string.</param>
        /// <returns>True if value type; otherwise, false.</returns>
        private static bool IsValueType(string csharpType)
        {
            return !string.Equals(csharpType, "string", StringComparison.Ordinal) &&
                   !string.Equals(csharpType, "byte[]", StringComparison.Ordinal);
        }
    }
}
