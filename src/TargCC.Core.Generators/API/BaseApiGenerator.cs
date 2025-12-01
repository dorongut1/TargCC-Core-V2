// <copyright file="BaseApiGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.API
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Base class for API generators with common functionality.
    /// </summary>
    public abstract class BaseApiGenerator : IApiGenerator
    {
        private static readonly string[] AuditFieldNames = new[] { "AddedBy", "AddedOn", "ChangedBy", "ChangedOn" };

        private static readonly Action<ILogger, string, string, Exception?> LogGenerationStarted =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(1, nameof(LogGenerationStarted)),
                "Starting {GeneratorType} generation for table {TableName}");

        private static readonly Action<ILogger, string, string, Exception?> LogGenerationCompleted =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(2, nameof(LogGenerationCompleted)),
                "Completed {GeneratorType} generation for table {TableName}");

        private static readonly Action<ILogger, string, string, Exception> LogGenerationFailed =
            LoggerMessage.Define<string, string>(
                LogLevel.Error,
                new EventId(3, nameof(LogGenerationFailed)),
                "Failed to generate {GeneratorType} for table {TableName}");

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseApiGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        protected BaseApiGenerator(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the logger instance.
        /// </summary>
        protected ILogger Logger => _logger;

        /// <summary>
        /// Gets the generator type name (for logging).
        /// </summary>
        protected abstract string GeneratorTypeName { get; }

        /// <inheritdoc/>
        public async Task<string> GenerateAsync(Table table, DatabaseSchema schema, ApiGeneratorConfig config)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(config);

            LogGenerationStarted(Logger, GeneratorTypeName, table.Name, null);

            try
            {
                var result = await Task.Run(() => Generate(table, schema, config)).ConfigureAwait(false);
                LogGenerationCompleted(Logger, GeneratorTypeName, table.Name, null);
                return result;
            }
            catch (Exception ex)
            {
                LogGenerationFailed(Logger, GeneratorTypeName, table.Name, ex);
                throw;
            }
        }

        /// <summary>
        /// Generates code synchronously (implemented by derived classes).
        /// </summary>
        /// <param name="table">The table to generate code for.</param>
        /// <param name="schema">The database schema.</param>
        /// <param name="config">Generator configuration.</param>
        /// <returns>Generated code.</returns>
        protected abstract string Generate(Table table, DatabaseSchema schema, ApiGeneratorConfig config);

        /// <summary>
        /// Gets the class name from table name (PascalCase).
        /// Example: "customer" -> "Customer", "order_item" -> "OrderItem".
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <returns>Class name in PascalCase.</returns>
        protected static string GetClassName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));
            }

            // Remove c_ prefix if exists
            if (tableName.StartsWith("c_", StringComparison.OrdinalIgnoreCase))
            {
                tableName = tableName.Substring(2);
            }

            // Convert to PascalCase
            return ToPascalCase(tableName);
        }

        /// <summary>
        /// Gets the property name from column name (PascalCase, with prefix handling).
        /// Example: "customer_id" -> "CustomerId", "eno_password" -> "PasswordHashed".
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <returns>Property name.</returns>
        protected static string GetPropertyName(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("Column name cannot be null or empty.", nameof(columnName));
            }

            var (prefix, baseName) = SplitPrefix(columnName);
            var propertyName = ToPascalCase(baseName);

            return prefix switch
            {
                "ENO" => propertyName + "Hashed",
                "LKP" => propertyName,
                "LOC" => propertyName,
                "CLC" => propertyName,
                "BLG" => propertyName,
                "AGG" => propertyName + "Aggregate",
                "SPT" => propertyName,
                "SCB" => propertyName + "ChangedBy",
                "SPL" => propertyName,
                "UPL" => propertyName,
                "ENT" => propertyName,
                "ENM" => propertyName,
                _ => propertyName,
            };
        }

        /// <summary>
        /// Splits a column name into prefix and base name.
        /// Example: "eno_password" -> ("ENO", "password"), "customer_id" -> ("", "customer_id").
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <returns>Tuple of (prefix, baseName).</returns>
        protected static (string prefix, string baseName) SplitPrefix(string columnName)
        {
            var match = Regex.Match(columnName, @"^(eno|ent|lkp|enm|loc|clc|blg|agg|spt|scb|spl|upl)_(.+)$", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return (match.Groups[1].Value.ToUpperInvariant(), match.Groups[2].Value);
            }

            return (string.Empty, columnName);
        }

        /// <summary>
        /// Converts a string to PascalCase.
        /// Example: "customer_id" -> "CustomerId", "order item" -> "OrderItem".
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <returns>PascalCase string.</returns>
        protected static string ToPascalCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var words = Regex.Split(input, @"[_\s-]+");

            if (words.Length == 1)
            {
                return NormalizeSingleWord(words[0]);
            }

            var result = new StringBuilder();
            foreach (var word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    result.Append(char.ToUpperInvariant(word[0]));
                    for (int i = 1; i < word.Length; i++)
                    {
                        result.Append(char.ToLowerInvariant(word[i]));
                    }
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Converts a string to camelCase.
        /// Example: "CustomerId" -> "customerId", "OrderItem" -> "orderItem".
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <returns>camelCase string.</returns>
        protected static string ToCamelCase(string input)
        {
            var pascalCase = ToPascalCase(input);
            if (string.IsNullOrEmpty(pascalCase))
            {
                return string.Empty;
            }

            return char.ToLowerInvariant(pascalCase[0]) + pascalCase.Substring(1);
        }

        /// <summary>
        /// Maps SQL data type to C# type.
        /// </summary>
        /// <param name="sqlType">SQL data type.</param>
        /// <param name="isNullable">Whether the column is nullable.</param>
        /// <returns>C# type string.</returns>
        protected static string GetCSharpType(string sqlType, bool isNullable)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sqlType);

            var baseType = sqlType.ToUpperInvariant() switch
            {
                "INT" or "INTEGER" => "int",
                "BIGINT" => "long",
                "SMALLINT" => "short",
                "TINYINT" => "byte",
                "BIT" => "bool",
                "DECIMAL" or "NUMERIC" or "MONEY" or "SMALLMONEY" => "decimal",
                "FLOAT" or "REAL" => "double",
                "VARCHAR" or "NVARCHAR" or "CHAR" or "NCHAR" or "TEXT" or "NTEXT" => "string",
                "DATE" or "DATETIME" or "DATETIME2" or "SMALLDATETIME" => "DateTime",
                "DATETIMEOFFSET" => "DateTimeOffset",
                "TIME" => "TimeSpan",
                "UNIQUEIDENTIFIER" => "Guid",
                "BINARY" or "VARBINARY" or "IMAGE" => "byte[]",
                _ => "string",
            };

            // Value types need nullable marker when nullable
            if (isNullable && baseType != "string" && baseType != "byte[]")
            {
                return baseType + "?";
            }

            return baseType;
        }

        /// <summary>
        /// Generates a file header comment.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="generatorType">Generator type description.</param>
        /// <returns>Header comment.</returns>
        protected static string GenerateFileHeader(string tableName, string generatorType)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("//     This code was generated by TargCC API Generator.");
            sb.AppendLine(CultureInfo.InvariantCulture, $"//     Generator: {generatorType}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"//     Table: {tableName}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"//     Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("//     Changes to this file may cause incorrect behavior and will be lost if");
            sb.AppendLine("//     the code is regenerated.");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Checks if a column is nullable.
        /// </summary>
        /// <param name="column">Column.</param>
        /// <returns>True if nullable.</returns>
        protected static bool IsNullable(Column column)
        {
            return column.IsNullable && !column.IsPrimaryKey;
        }

        /// <summary>
        /// Gets all columns excluding audit fields.
        /// </summary>
        /// <param name="table">Table.</param>
        /// <returns>Filtered columns.</returns>
        protected static IReadOnlyList<Column> GetDataColumns(Table table)
        {
            return table.Columns
                .Where(c => !IsAuditField(c.Name))
                .ToList();
        }

        /// <summary>
        /// Gets columns that should be included in Create/Update requests (excluding PK, audit, and readonly fields).
        /// </summary>
        /// <param name="table">Table.</param>
        /// <returns>Filtered columns.</returns>
        protected static IReadOnlyList<Column> GetMutableColumns(Table table)
        {
            return table.Columns
                .Where(c => !c.IsPrimaryKey &&
                           !IsAuditField(c.Name) &&
                           !IsReadOnlyPrefix(c.Name))
                .ToList();
        }

        /// <summary>
        /// Checks if a column name is an audit field.
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <returns>True if audit field.</returns>
        protected static bool IsAuditField(string columnName)
        {
            return AuditFieldNames.Contains(columnName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if a column has a readonly prefix (CLC, BLG, AGG, SCB).
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <returns>True if readonly.</returns>
        protected static bool IsReadOnlyPrefix(string columnName)
        {
            var (prefix, _) = SplitPrefix(columnName);
            return prefix is "CLC" or "BLG" or "AGG" or "SCB";
        }

        /// <summary>
        /// Normalizes a single word to PascalCase.
        /// All-caps or all-lowercase words are normalized (e.g., "ID" -> "Id").
        /// Mixed-case words are preserved (e.g., "EmailAddress" -> "EmailAddress").
        /// </summary>
        /// <param name="word">Single word to normalize.</param>
        /// <returns>Normalized word.</returns>
        private static string NormalizeSingleWord(string word)
        {
            if (word.Length == 0)
            {
                return string.Empty;
            }

            if (word.Length == 1)
            {
                return char.ToUpperInvariant(word[0]).ToString();
            }

            var tail = word.Substring(1);
            bool isAllUpper = tail.All(c => !char.IsLetter(c) || char.IsUpper(c));
            bool isAllLower = tail.All(c => !char.IsLetter(c) || char.IsLower(c));

            if (isAllUpper || isAllLower)
            {
                var normalized = new StringBuilder();
                normalized.Append(char.ToUpperInvariant(word[0]));
                for (int i = 1; i < word.Length; i++)
                {
                    normalized.Append(char.ToLowerInvariant(word[i]));
                }

                return normalized.ToString();
            }

            return char.ToUpperInvariant(word[0]) + word.Substring(1);
        }
    }
}
