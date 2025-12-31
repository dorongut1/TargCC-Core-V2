// <copyright file="TypeScriptEnumGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.TypeScript
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Generates TypeScript enums from c_Enumeration database table.
    /// </summary>
    public class TypeScriptEnumGenerator
    {
        private static readonly Action<ILogger, int, Exception?> LogGeneratedEnums =
            LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(1, nameof(LogGeneratedEnums)),
                "Generated {Count} TypeScript enums from c_Enumeration");

        private static readonly Action<ILogger, Exception?> LogNoEnumsFound =
            LoggerMessage.Define(
                LogLevel.Warning,
                new EventId(2, nameof(LogNoEnumsFound)),
                "No enums found in c_Enumeration table");

        private static readonly Action<ILogger, string, Exception?> LogGeneratingEnum =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(3, nameof(LogGeneratingEnum)),
                "Generating TypeScript enum: {EnumType}");

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeScriptEnumGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public TypeScriptEnumGenerator(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates TypeScript enums from a list of enum records.
        /// </summary>
        /// <param name="enums">List of enum records.</param>
        /// <returns>Generated TypeScript code.</returns>
        public string GenerateEnumsCode(IEnumerable<EnumRecord> enums)
        {
            ArgumentNullException.ThrowIfNull(enums);

            var enumList = enums.ToList();

            if (enumList.Count == 0)
            {
                LogNoEnumsFound(_logger, null);
                return GenerateEmptyEnumsFile();
            }

            var sb = new StringBuilder();

            GenerateHeader(sb);

            var grouped = enumList.GroupBy(e => e.EnumType);
            var groupCount = 0;

            foreach (var group in grouped)
            {
                LogGeneratingEnum(_logger, group.Key, null);

                GenerateEnumDeclaration(sb, group.Key, group.ToList());
                GenerateLabelsObject(sb, group.Key, group.ToList());

                groupCount++;
            }

            LogGeneratedEnums(_logger, groupCount, null);

            return sb.ToString();
        }

        private static string GenerateEmptyEnumsFile()
        {
            var sb = new StringBuilder();
            GenerateHeader(sb);
            sb.AppendLine("// No enums found in c_Enumeration table");
            sb.AppendLine("// Add enum values to c_Enumeration to generate enums");
            return sb.ToString();
        }

        private static void GenerateHeader(StringBuilder sb)
        {
            sb.AppendLine("/**");
            sb.AppendLine(" * Auto-generated enums from c_Enumeration table");
            sb.AppendLine(" * Do not edit manually - changes will be overwritten");
            sb.AppendLine(CultureInfo.InvariantCulture, $" * Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine(" */");
            sb.AppendLine();
        }

        private static void GenerateEnumDeclaration(StringBuilder sb, string enumType, List<EnumRecord> items)
        {
            var safeEnumName = MakeSafeIdentifier(enumType);

            sb.AppendLine(CultureInfo.InvariantCulture, $"export enum {safeEnumName} {{");

            var lines = items.Select(item =>
            {
                var safeValue = MakeSafeIdentifier(item.EnumValue);
                return $"    {safeValue} = '{item.EnumValue}',";
            });

            foreach (var line in lines)
            {
                sb.AppendLine(line);
            }

            sb.AppendLine("}");
            sb.AppendLine();
        }

        private static void GenerateLabelsObject(StringBuilder sb, string enumType, List<EnumRecord> items)
        {
            var safeEnumName = MakeSafeIdentifier(enumType);

            sb.AppendLine(CultureInfo.InvariantCulture, $"export const {safeEnumName}Labels: Record<{safeEnumName}, string> = {{");

            var lines = items.Select(item =>
            {
                var safeValue = MakeSafeIdentifier(item.EnumValue);
                var displayText = EscapeString(item.EnumText ?? item.EnumValue);
                return $"    [{safeEnumName}.{safeValue}]: '{displayText}',";
            });

            foreach (var line in lines)
            {
                sb.AppendLine(line);
            }

            sb.AppendLine("};");
            sb.AppendLine();
        }

        private static string MakeSafeIdentifier(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "_";
            }

            var result = new StringBuilder();

            foreach (var c in value)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    result.Append(c);
                }
                else
                {
                    result.Append('_');
                }
            }

            if (result.Length > 0 && char.IsDigit(result[0]))
            {
                result.Insert(0, '_');
            }

            return result.ToString();
        }

        private static string EscapeString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("'", "\\'", StringComparison.Ordinal)
                .Replace("\n", "\\n", StringComparison.Ordinal)
                .Replace("\r", "\\r", StringComparison.Ordinal);
        }
    }
}
