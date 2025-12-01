// <copyright file="BaseUIGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI
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
    /// Base class for all UI generators with common functionality.
    /// </summary>
    public abstract class BaseUIGenerator : IUIGenerator
    {
        private static readonly string[] LineSeparators = new[] { "\r\n", "\r", "\n" };
        private static readonly string[] AuditFieldNames = new[] { "AddedBy", "AddedOn", "ChangedBy", "ChangedOn" };

        private static readonly Action<ILogger, UIGeneratorType, string, Exception> LogGenerationFailed =
            LoggerMessage.Define<UIGeneratorType, string>(
                LogLevel.Error,
                new EventId(1, nameof(LogGenerationFailed)),
                "Failed to generate {GeneratorType} for table {TableName}");

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseUIGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        protected BaseUIGenerator(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public abstract UIGeneratorType GeneratorType { get; }

        /// <summary>
        /// Gets the logger instance.
        /// </summary>
        protected ILogger Logger => _logger;

        /// <inheritdoc/>
        public abstract Task<string> GenerateAsync(Table table, DatabaseSchema schema, UIGeneratorConfig config);

        /// <inheritdoc/>
        public virtual async Task<Dictionary<string, string>> GenerateAllAsync(DatabaseSchema schema, UIGeneratorConfig config)
        {
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(config);

            var result = new Dictionary<string, string>();

            foreach (var table in schema.Tables)
            {
                try
                {
                    var code = await GenerateAsync(table, schema, config).ConfigureAwait(false);
                    result[table.Name] = code;
                }
                catch (Exception ex)
                {
                    LogGenerationFailed(Logger, GeneratorType, table.Name, ex);
                    throw;
                }
            }

            return result;
        }

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
        /// Gets the camelCase name from table name.
        /// Example: "Customer" -> "customer", "OrderItem" -> "orderItem".
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <returns>camelCase name.</returns>
        protected static string GetCamelCaseName(string tableName)
        {
            var className = GetClassName(tableName);
            if (string.IsNullOrEmpty(className))
            {
                return string.Empty;
            }

            return char.ToLowerInvariant(className[0]) + className.Substring(1);
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

            // Handle special prefixes
            var (prefix, baseName) = SplitPrefix(columnName);

            var propertyName = ToPascalCase(baseName);

            // Add suffix based on prefix
            return prefix switch
            {
                "ENO" => propertyName + "Hashed",
                "LKP" => propertyName, // Will generate 2 properties: Code and Text
                "LOC" => propertyName, // Will generate 2 properties: Value and Localized
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
        /// Example: "eno_password" -> ("eno", "password"), "customer_id" -> ("", "customer_id").
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

            // Split by underscore, space, or dash
            var words = Regex.Split(input, @"[_\s-]+");

            var result = new StringBuilder();
            foreach (var word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    // First character uppercase
                    result.Append(char.ToUpperInvariant(word[0]));

                    // Rest of characters lowercase (manually to avoid CA1308)
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
        /// Generates a file header comment.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="generatorType">Generator type.</param>
        /// <returns>Header comment.</returns>
        protected static string GenerateFileHeader(string tableName, UIGeneratorType generatorType)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("//     This code was generated by TargCC UI Generator.");
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
        /// Indents code by the specified number of spaces.
        /// </summary>
        /// <param name="code">Code to indent.</param>
        /// <param name="spaces">Number of spaces.</param>
        /// <returns>Indented code.</returns>
        protected static string Indent(string code, int spaces)
        {
            if (string.IsNullOrEmpty(code))
            {
                return code;
            }

            var indent = new string(' ', spaces);
            var lines = code.Split(LineSeparators, StringSplitOptions.None);
            return string.Join(Environment.NewLine, lines.Select(line => string.IsNullOrWhiteSpace(line) ? line : indent + line));
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
        /// Checks if a column name is an audit field.
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <returns>True if audit field.</returns>
        protected static bool IsAuditField(string columnName)
        {
            return AuditFieldNames.Contains(columnName, StringComparer.OrdinalIgnoreCase);
        }
    }
}
