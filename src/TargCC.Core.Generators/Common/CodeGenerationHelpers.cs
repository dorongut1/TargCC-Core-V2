namespace TargCC.Core.Generators.Common;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

/// <summary>
/// Shared helper methods for all code generators.
/// Provides common functionality including type mapping, naming conventions, and TargCC prefix handling.
/// </summary>
/// <remarks>
/// <para>
/// This class centralizes common logic used across multiple generators to:
/// </para>
/// <list type="bullet">
/// <item>Avoid code duplication (DRY principle)</item>
/// <item>Ensure consistent behavior across generators</item>
/// <item>Simplify maintenance and testing</item>
/// </list>
/// </remarks>
public static class CodeGenerationHelpers
{
    /// <summary>
    /// Maps SQL data types to C# types.
    /// </summary>
    /// <param name="sqlType">SQL Server data type (e.g., "int", "nvarchar", "datetime").</param>
    /// <returns>Corresponding C# type as string (e.g., "int", "string", "DateTime").</returns>
    public static string GetCSharpType(string sqlType)
    {
        return sqlType.ToUpperInvariant() switch
        {
            "INT" => "int",
            "BIGINT" => "long",
            "SMALLINT" => "short",
            "TINYINT" => "byte",
            "BIT" => "bool",
            "DECIMAL" or "NUMERIC" or "MONEY" or "SMALLMONEY" => "decimal",
            "FLOAT" => "double",
            "REAL" => "float",
            "DATETIME" or "DATETIME2" or "DATE" or "SMALLDATETIME" => "DateTime",
            "TIME" => "TimeSpan",
            "DATETIMEOFFSET" => "DateTimeOffset",
            "UNIQUEIDENTIFIER" => "Guid",
            "VARCHAR" or "NVARCHAR" or "CHAR" or "NCHAR" or "TEXT" or "NTEXT" => "string",
            "VARBINARY" or "BINARY" or "IMAGE" => "byte[]",
            _ => "string"
        };
    }

    /// <summary>
    /// Removes TargCC prefixes from column names for cleaner property and parameter names.
    /// </summary>
    /// <param name="columnName">Column name potentially containing a TargCC prefix.</param>
    /// <returns>Column name with prefix removed.</returns>
    public static string SanitizeColumnName(string columnName)
    {
        string[] prefixes = { "eno_", "ent_", "lkp_", "enm_", "loc_", "clc_", "blg_", "agg_", "spt_", "upl_", "scb_", "spl_", "FUI_" };

        var matchedPrefix = Array.Find(prefixes, prefix =>
            columnName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        return matchedPrefix != null
            ? columnName.Substring(matchedPrefix.Length)
            : columnName;
    }

    /// <summary>
    /// Escapes C# keywords by prefixing with @ symbol.
    /// </summary>
    /// <param name="identifier">The identifier to escape if it's a keyword.</param>
    /// <returns>The identifier with @ prefix if it's a keyword, otherwise unchanged.</returns>
    public static string EscapeCSharpKeyword(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return identifier;
        }

        return CSharpKeywords.Contains(identifier) ? "@" + identifier : identifier;
    }

    /// <summary>
    /// Converts a string to camelCase for parameter names.
    /// </summary>
    /// <param name="value">String to convert (typically PascalCase).</param>
    /// <returns>String in camelCase format, with C# keywords escaped.</returns>
    /// <remarks>
    /// Handles special cases:
    /// - All uppercase strings (like "ID", "URL") are converted entirely to lowercase.
    /// - Standard PascalCase strings have only the first letter lowercased.
    /// - C# keywords are prefixed with @ to avoid compilation errors.
    /// </remarks>
    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "CamelCase requires lowercase for parameter names in code generation.")]
    public static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        string camelCased;

        // If the entire string is uppercase (like "ID", "URL"), convert to all lowercase
        if (value.All(char.IsUpper))
        {
            camelCased = value.ToLowerInvariant();
        }
        else
        {
            camelCased = char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        // Escape C# keywords
        return EscapeCSharpKeyword(camelCased);
    }

    /// <summary>
    /// Formats a string using invariant culture for consistent generated code.
    /// </summary>
    /// <param name="format">Format string.</param>
    /// <param name="args">Format arguments.</param>
    /// <returns>Formatted string.</returns>
    public static string FormatInvariant(string format, params object[] args)
    {
        return string.Format(CultureInfo.InvariantCulture, format, args);
    }

    /// <summary>
    /// Generates a stored procedure name following TargCC conventions.
    /// </summary>
    /// <param name="prefix">Procedure prefix (e.g., "SP_Get", "SP_Update").</param>
    /// <param name="tableName">Table name.</param>
    /// <param name="suffix">Optional suffix (e.g., "ByID", "ByEmail").</param>
    /// <returns>Complete stored procedure name.</returns>
    public static string GetStoredProcedureName(string prefix, string tableName, string suffix)
    {
        return suffix.Length > 0
            ? FormatInvariant("{0}{1}{2}", prefix, tableName, suffix)
            : FormatInvariant("{0}{1}", prefix, tableName);
    }

    /// <summary>
    /// Builds a method name from column names (e.g., "GetByEmailAndPhone").
    /// </summary>
    /// <param name="methodPrefix">Method prefix (e.g., "GetBy").</param>
    /// <param name="columnNames">List of column names to include.</param>
    /// <returns>Complete method name.</returns>
    public static string BuildMethodName(string methodPrefix, IEnumerable<string> columnNames)
    {
        var sanitized = columnNames.Select(CodeGenerationHelpers.SanitizeColumnName);
        return methodPrefix + string.Join("And", sanitized);
    }

    /// <summary>
    /// Determines if a column is an aggregate column (has agg_ prefix).
    /// </summary>
    /// <param name="columnName">Column name to check.</param>
    /// <returns>True if column is an aggregate column.</returns>
    public static bool IsAggregateColumn(string columnName)
    {
        return columnName.StartsWith("agg_", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if a column is read-only (calculated, business logic, or aggregate).
    /// </summary>
    /// <param name="columnName">Column name to check.</param>
    /// <returns>True if column is read-only.</returns>
    public static bool IsReadOnlyColumn(string columnName)
    {
        return columnName.StartsWith("clc_", StringComparison.OrdinalIgnoreCase) ||
               columnName.StartsWith("blg_", StringComparison.OrdinalIgnoreCase) ||
               columnName.StartsWith("agg_", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Pluralizes an entity name for DbSet property naming.
    /// </summary>
    /// <param name="singular">The singular form of the entity name to pluralize.</param>
    /// <remarks>
    /// Simple pluralization rules:
    /// - Words ending in 'z' → double z and add 'es' (e.g., Quiz → Quizzes)
    /// - Words ending in 'y' → replace with 'ies' (e.g., Category → Categories)
    /// - Words ending in 's', 'x', 'ch', 'sh' → add 'es' (e.g., Address → Addresses)
    /// - Default → add 's' (e.g., Customer → Customers).
    /// </remarks>
    /// <returns>MakePlural.</returns>
    public static string MakePlural(string singular)
    {
        // Simple pluralization rules

        // Special case: 'z' needs to be doubled before adding 'es'
        // Quiz → Quizzes (not Quizes)
        if (singular.EndsWith('z'))
        {
            return singular + "zes";
        }

        if (singular.EndsWith('s') ||
            singular.EndsWith('x') ||
            singular.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
            singular.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
        {
            return singular + "es";
        }

        if (singular.EndsWith('y') &&
            singular.Length > 1 &&
            !IsVowel(singular[^2]))
        {
            return string.Concat(singular.AsSpan(0, singular.Length - 1), "ies");
        }

        return singular + "s";
    }

    private static bool IsVowel(char c)
    {
        return "aeiouAEIOU".Contains(c, StringComparison.Ordinal);
    }

    /// <summary>
    /// C# keywords that need to be escaped when used as identifiers.
    /// </summary>
    private static readonly HashSet<string> CSharpKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
        "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
        "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
        "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
        "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw",
        "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
        "virtual", "void", "volatile", "while"
    };
}
