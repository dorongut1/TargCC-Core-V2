// <copyright file="PrefixHandler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.Entities
{
    using System;
    using System.Globalization;
    using System.Text;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Handles TargCC column prefix transformations for entity properties.
    /// </summary>
    public static class PrefixHandler
    {
        /// <summary>
        /// Gets the property name from a column, handling TargCC prefixes.
        /// </summary>
        /// <param name="column">The column to process.</param>
        /// <returns>The property name without prefix.</returns>
        public static string GetPropertyName(Column column)
        {
            ArgumentNullException.ThrowIfNull(column);

            return column.Prefix switch
            {
                ColumnPrefix.OneWayEncryption => RemovePrefix(column.Name, "eno_") + "Hashed",
                ColumnPrefix.TwoWayEncryption => RemovePrefix(column.Name, "ent_"),
                ColumnPrefix.Lookup => RemovePrefix(column.Name, "lkp_"),
                ColumnPrefix.Enumeration => RemovePrefix(column.Name, "enm_"),
                ColumnPrefix.Localization => RemovePrefix(column.Name, "loc_"),
                ColumnPrefix.Calculated => RemovePrefix(column.Name, "clc_") + "Calculated",
                ColumnPrefix.BusinessLogic => RemovePrefix(column.Name, "blg_") + "BL",
                ColumnPrefix.Aggregate => RemovePrefix(column.Name, "agg_") + "Aggregate",
                ColumnPrefix.SeparateUpdate => RemovePrefix(column.Name, "spt_") + "Separate",
                ColumnPrefix.Upload => RemovePrefix(column.Name, "upl_") + "Upload",
                ColumnPrefix.SeparateList => RemovePrefix(column.Name, "spl_"),
                ColumnPrefix.FakeUniqueIndex => RemovePrefix(column.Name, "FUI_"),
                _ => column.Name,
            };
        }

        /// <summary>
        /// Generates the property declaration for a prefixed column.
        /// </summary>
        /// <param name="column">The column to process.</param>
        /// <param name="csharpType">The C# type.</param>
        /// <returns>The property code.</returns>
        public static string? GeneratePrefixedProperty(Column column, string csharpType)
        {
            ArgumentNullException.ThrowIfNull(column);
            ArgumentException.ThrowIfNullOrWhiteSpace(csharpType);

            return column.Prefix switch
            {
                ColumnPrefix.OneWayEncryption => GenerateOneWayEncryptionProperty(column, csharpType),
                ColumnPrefix.TwoWayEncryption => GenerateTwoWayEncryptionProperty(column, csharpType),
                ColumnPrefix.Lookup => GenerateLookupProperty(column, csharpType),
                ColumnPrefix.Enumeration => GenerateEnumProperty(column, csharpType),
                ColumnPrefix.Localization => GenerateLocalizedProperty(column, csharpType),
                ColumnPrefix.Calculated => GenerateCalculatedProperty(column, csharpType),
                ColumnPrefix.BusinessLogic => GenerateBusinessLogicProperty(column, csharpType),
                ColumnPrefix.Aggregate => GenerateAggregateProperty(column, csharpType),
                ColumnPrefix.SeparateUpdate => GenerateSeparateUpdateProperty(column, csharpType),
                ColumnPrefix.Upload => GenerateUploadProperty(column, csharpType),
                ColumnPrefix.SeparateList => GenerateSeparateListProperty(column, csharpType),
                _ => null, // Regular property, handled elsewhere
            };
        }

        /// <summary>
        /// Determines if a prefix requires a backing field.
        /// </summary>
        /// <param name="prefix">The column prefix.</param>
        /// <returns>True if backing field needed.</returns>
        public static bool RequiresBackingField(ColumnPrefix prefix)
        {
            return prefix == ColumnPrefix.TwoWayEncryption;
        }

        /// <summary>
        /// Generates backing field for prefixed column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="csharpType">The C# type.</param>
        /// <returns>Backing field declaration.</returns>
        public static string? GenerateBackingField(Column column, string csharpType)
        {
            if (column.Prefix != ColumnPrefix.TwoWayEncryption)
            {
                return null;
            }

            var propertyName = GetPropertyName(column);
            return string.Format(CultureInfo.InvariantCulture, "private string _{0}Encrypted;", ToCamelCase(propertyName));
        }

        private static string GenerateOneWayEncryptionProperty(Column column, string csharpType)
        {
            var propertyName = GetPropertyName(column);
            var sb = new StringBuilder();

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Hashed value (one-way encryption).");
            sb.AppendLine("        /// This field is hashed by the application before saving.");
            sb.AppendLine("        /// </summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        [Column(\"{column.Name}\")]");
            sb.AppendLine();
            sb.AppendLine("        [JsonIgnore]");

            if (!column.IsNullable)
            {
                sb.AppendLine("        [Required]");
            }

            sb.Append(CultureInfo.InvariantCulture, $"        public {csharpType} {propertyName} {{ get; private set; }}");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string GenerateTwoWayEncryptionProperty(Column column, string csharpType)
        {
            var propertyName = GetPropertyName(column);
            var backingField = string.Format(CultureInfo.InvariantCulture, "_{0}Encrypted", ToCamelCase(propertyName));
            var sb = new StringBuilder();

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Encrypted value (two-way encryption).");
            sb.AppendLine("        /// Automatically encrypted/decrypted by the application.");
            sb.AppendLine("        /// </summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        [Column(\"{column.Name}\")]");
            sb.AppendLine();
            sb.AppendLine("        [JsonIgnore]");
            sb.Append(CultureInfo.InvariantCulture, $"        public {csharpType} {propertyName}");
            sb.AppendLine();
            sb.AppendLine("        {");
            sb.Append(CultureInfo.InvariantCulture, $"            get => DecryptValue({backingField});");
            sb.AppendLine();
            sb.Append(CultureInfo.InvariantCulture, $"            set => {backingField} = EncryptValue(value);");
            sb.AppendLine();
            sb.AppendLine("        }");

            return sb.ToString();
        }

        private static string GenerateLookupProperty(Column column, string csharpType)
        {
            var propertyName = GetPropertyName(column);
            var sb = new StringBuilder();

            // Main property
            sb.AppendLine("        /// <summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        /// Lookup value for {propertyName}.");
            sb.AppendLine();
            sb.AppendLine("        /// </summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        [Column(\"{column.Name}\")]");
            sb.AppendLine();
            sb.Append(CultureInfo.InvariantCulture, $"        public {csharpType} {propertyName} {{ get; set; }}");
            sb.AppendLine();
            sb.AppendLine();

            // Text property
            sb.AppendLine("        /// <summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        /// Display text for {propertyName} (from lookup table).");
            sb.AppendLine();
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [NotMapped]");
            sb.Append(CultureInfo.InvariantCulture, $"        public string {propertyName}Text {{ get; set; }}");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string GenerateEnumProperty(Column column, string csharpType)
        {
            var propertyName = GetPropertyName(column);
            var sb = new StringBuilder();

            // Main property
            sb.AppendLine("        /// <summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        /// Enum value for {propertyName}.");
            sb.AppendLine();
            sb.AppendLine("        /// </summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        [Column(\"{column.Name}\")]");
            sb.AppendLine();
            sb.Append(CultureInfo.InvariantCulture, $"        public {csharpType} {propertyName} {{ get; set; }}");
            sb.AppendLine();
            sb.AppendLine();

            // Enum property
            sb.AppendLine("        /// <summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        /// Enum representation of {propertyName}.");
            sb.AppendLine();
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [NotMapped]");
            sb.Append(CultureInfo.InvariantCulture, $"        public int {propertyName}Enum {{ get; set; }}");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string GenerateLocalizedProperty(Column column, string csharpType)
        {
            var propertyName = GetPropertyName(column);
            var sb = new StringBuilder();

            sb.AppendLine("        /// <summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        /// Localized value for {propertyName}.");
            sb.AppendLine();
            sb.AppendLine("        /// </summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        [Column(\"{column.Name}\")]");
            sb.AppendLine();
            sb.Append(CultureInfo.InvariantCulture, $"        public {csharpType} {propertyName} {{ get; set; }}");
            sb.AppendLine();
            sb.AppendLine();

            // Localized property
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Localized display value (runtime).");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [NotMapped]");
            sb.Append(CultureInfo.InvariantCulture, $"        public {csharpType} {propertyName}Localized {{ get; set; }}");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string GenerateCalculatedProperty(Column column, string csharpType)
        {
            var propertyName = GetPropertyName(column);
            var sb = new StringBuilder();

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Calculated field (read-only).");
            sb.AppendLine("        /// </summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        [Column(\"{column.Name}\")]");
            sb.AppendLine();
            sb.Append(CultureInfo.InvariantCulture, $"        public {csharpType} {propertyName} {{ get; internal set; }}");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string GenerateBusinessLogicProperty(Column column, string csharpType)
        {
            var propertyName = GetPropertyName(column);
            var sb = new StringBuilder();

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Business logic field (read-only on client).");
            sb.AppendLine("        /// </summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        [Column(\"{column.Name}\")]");
            sb.AppendLine();
            sb.Append(CultureInfo.InvariantCulture, $"        public {csharpType} {propertyName} {{ get; internal set; }}");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string GenerateAggregateProperty(Column column, string csharpType)
        {
            var propertyName = GetPropertyName(column);
            var sb = new StringBuilder();

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Aggregate field (read-only on client).");
            sb.AppendLine("        /// </summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        [Column(\"{column.Name}\")]");
            sb.AppendLine();
            sb.Append(CultureInfo.InvariantCulture, $"        public {csharpType} {propertyName} {{ get; internal set; }}");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string GenerateSeparateUpdateProperty(Column column, string csharpType)
        {
            var propertyName = GetPropertyName(column);
            var sb = new StringBuilder();

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Separate update field.");
            sb.AppendLine("        /// </summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        [Column(\"{column.Name}\")]");
            sb.AppendLine();
            sb.Append(CultureInfo.InvariantCulture, $"        public {csharpType} {propertyName} {{ get; set; }}");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string GenerateUploadProperty(Column column, string csharpType)
        {
            var propertyName = GetPropertyName(column);
            var sb = new StringBuilder();

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// File upload path.");
            sb.AppendLine("        /// </summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        [Column(\"{column.Name}\")]");
            sb.AppendLine();
            sb.Append(CultureInfo.InvariantCulture, $"        public {csharpType} {propertyName} {{ get; set; }}");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string GenerateSeparateListProperty(Column column, string csharpType)
        {
            var propertyName = GetPropertyName(column);
            var sb = new StringBuilder();

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Separate list field (NewLine delimited).");
            sb.AppendLine("        /// </summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        [Column(\"{column.Name}\")]");
            sb.AppendLine();
            sb.Append(CultureInfo.InvariantCulture, $"        public {csharpType} {propertyName} {{ get; set; }}");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string RemovePrefix(string columnName, string prefix)
        {
            if (columnName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return columnName.Substring(prefix.Length);
            }

            return columnName;
        }

        private static string ToCamelCase(string pascalCase)
        {
            if (string.IsNullOrEmpty(pascalCase))
            {
                return pascalCase;
            }

            return char.ToLowerInvariant(pascalCase[0]) + pascalCase.Substring(1);
        }
    }
}
