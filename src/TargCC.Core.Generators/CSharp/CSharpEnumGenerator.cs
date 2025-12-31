// <copyright file="CSharpEnumGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.CSharp
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Generates C# enums from c_Enumeration database table.
    /// </summary>
    public class CSharpEnumGenerator
    {
        private static readonly Action<ILogger, int, Exception?> LogGeneratedEnums =
            LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(1, nameof(LogGeneratedEnums)),
                "Generated {Count} C# enums from c_Enumeration");

        private static readonly Action<ILogger, Exception?> LogNoEnumsFound =
            LoggerMessage.Define(
                LogLevel.Warning,
                new EventId(2, nameof(LogNoEnumsFound)),
                "No enums found in c_Enumeration table");

        private static readonly Action<ILogger, string, Exception?> LogGeneratingEnum =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(3, nameof(LogGeneratingEnum)),
                "Generating C# enum: {EnumType}");

        private readonly ILogger _logger;
        private readonly string _namespace;

        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpEnumGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="namespace">Namespace for generated enums.</param>
        public CSharpEnumGenerator(ILogger logger, string @namespace = "Generated.Enums")
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _namespace = @namespace;
        }

        /// <summary>
        /// Generates C# enums from a list of enum records.
        /// </summary>
        /// <param name="enums">List of enum records.</param>
        /// <returns>Generated C# code.</returns>
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
            sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {_namespace}");
            sb.AppendLine("{");

            var grouped = enumList.GroupBy(e => e.EnumType);
            var groupCount = 0;

            foreach (var group in grouped)
            {
                LogGeneratingEnum(_logger, group.Key, null);

                GenerateEnumDeclaration(sb, group);
                GenerateExtensionClass(sb, group);

                groupCount++;
            }

            sb.AppendLine("}");

            LogGeneratedEnums(_logger, groupCount, null);

            return sb.ToString();
        }

        private static void GenerateHeader(StringBuilder sb)
        {
            sb.AppendLine("// <copyright file=\"GeneratedEnums.cs\" company=\"PlaceholderCompany\">");
            sb.AppendLine("// Copyright (c) PlaceholderCompany. All rights reserved.");
            sb.AppendLine("// </copyright>");
            sb.AppendLine();
            sb.AppendLine("// Auto-generated enums from c_Enumeration table");
            sb.AppendLine("// Do not edit manually - changes will be overwritten");
            sb.AppendLine(CultureInfo.InvariantCulture, $"// Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine();
        }

        private static void GenerateEnumDeclaration(StringBuilder sb, IGrouping<string, EnumRecord> group)
        {
            var safeEnumName = MakeSafeIdentifier(group.Key);

            sb.AppendLine("    /// <summary>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    /// {safeEnumName} enumeration values.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    public enum {safeEnumName}");
            sb.AppendLine("    {");

            var items = group.ToList();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var safeValue = MakeSafeIdentifier(item.EnumValue);
                var displayText = item.EnumText ?? item.EnumValue;

                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// {EscapeXmlComment(displayText)}.");
                sb.AppendLine("        /// </summary>");

                var comma = i < items.Count - 1 ? "," : string.Empty;
                sb.AppendLine(CultureInfo.InvariantCulture, $"        {safeValue}{comma}");
            }

            sb.AppendLine("    }");
            sb.AppendLine();
        }

        private static void GenerateExtensionClass(StringBuilder sb, IGrouping<string, EnumRecord> group)
        {
            var safeEnumName = MakeSafeIdentifier(group.Key);

            sb.AppendLine("    /// <summary>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Extension methods for <see cref=\"{safeEnumName}\"/>.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    public static class {safeEnumName}Extensions");
            sb.AppendLine("    {");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets the display text for the enum value.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"value\">The enum value.</param>");
            sb.AppendLine("        /// <returns>The display text.</returns>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"        public static string ToDisplayText(this {safeEnumName} value)");
            sb.AppendLine("        {");
            sb.AppendLine("            return value switch");
            sb.AppendLine("            {");

            foreach (var item in group)
            {
                var safeValue = MakeSafeIdentifier(item.EnumValue);
                var displayText = EscapeString(item.EnumText ?? item.EnumValue);
                sb.AppendLine(CultureInfo.InvariantCulture, $"                {safeEnumName}.{safeValue} => \"{displayText}\",");
            }

            sb.AppendLine("                _ => value.ToString(),");
            sb.AppendLine("            };");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
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
                .Replace("\"", "\\\"", StringComparison.Ordinal)
                .Replace("\n", "\\n", StringComparison.Ordinal)
                .Replace("\r", "\\r", StringComparison.Ordinal);
        }

        private static string EscapeXmlComment(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("&", "&amp;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal);
        }

        private string GenerateEmptyEnumsFile()
        {
            var sb = new StringBuilder();
            GenerateHeader(sb);
            sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {_namespace}");
            sb.AppendLine("{");
            sb.AppendLine("    // No enums found in c_Enumeration table");
            sb.AppendLine("    // Add enum values to c_Enumeration to generate enums");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
