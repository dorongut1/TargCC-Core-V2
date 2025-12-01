// <copyright file="DtoGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.API
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Generates Data Transfer Objects (DTOs) for API endpoints.
    /// Creates 4 types: Response DTO, CreateRequest DTO, UpdateRequest DTO, and Filters DTO.
    /// </summary>
    public class DtoGenerator : BaseApiGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DtoGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public DtoGenerator(ILogger<DtoGenerator> logger)
            : base(logger)
        {
        }

        /// <inheritdoc/>
        protected override string GeneratorTypeName => "DTO";

        /// <inheritdoc/>
        protected override string Generate(Table table, DatabaseSchema schema, ApiGeneratorConfig config)
        {
            var className = GetClassName(table.Name);
            var sb = new StringBuilder();

            sb.Append(GenerateFileHeader(table.Name, "DTO Generator"));

            AppendUsings(sb);

            sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {config.Namespace}.DTOs.{className}");
            sb.AppendLine("{");

            GenerateResponseDto(sb, table, className, config);
            sb.AppendLine();

            GenerateCreateRequestDto(sb, table, className, config);
            sb.AppendLine();

            GenerateUpdateRequestDto(sb, table, className, config);
            sb.AppendLine();

            GenerateFiltersDto(sb, table, className, config);

            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void AppendUsings(StringBuilder sb)
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            sb.AppendLine();
        }

        private static void GenerateResponseDto(StringBuilder sb, Table table, string className, ApiGeneratorConfig config)
        {
            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("    /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Response DTO for {className} entity.");
                sb.AppendLine("    /// </summary>");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"    public class {className}Dto");
            sb.AppendLine("    {");

            var columns = table.Columns.Where(c => !IsAuditField(c.Name)).ToList();
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                GenerateResponseProperties(sb, column, config);

                if (i < columns.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            // Add audit fields
            sb.AppendLine();
            GenerateAuditProperties(sb, table, config);

            sb.AppendLine("    }");
        }

        private static void GenerateCreateRequestDto(StringBuilder sb, Table table, string className, ApiGeneratorConfig config)
        {
            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("    /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Request DTO for creating a new {className}.");
                sb.AppendLine("    /// </summary>");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"    public class Create{className}Request");
            sb.AppendLine("    {");

            var columns = GetMutableColumns(table).ToList();
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                GenerateCreateRequestProperties(sb, column, config);

                if (i < columns.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine("    }");
        }

        private static void GenerateUpdateRequestDto(StringBuilder sb, Table table, string className, ApiGeneratorConfig config)
        {
            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("    /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Request DTO for updating an existing {className}.");
                sb.AppendLine("    /// </summary>");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"    public class Update{className}Request");
            sb.AppendLine("    {");

            var columns = GetMutableColumns(table).ToList();
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                GenerateUpdateRequestProperties(sb, column, config);

                if (i < columns.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine("    }");
        }

        private static void GenerateFiltersDto(StringBuilder sb, Table table, string className, ApiGeneratorConfig config)
        {
            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("    /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Filters for querying {className} entities.");
                sb.AppendLine("    /// </summary>");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"    public class {className}Filters");
            sb.AppendLine("    {");

            // Add pagination properties
            sb.AppendLine("        public int? Page { get; set; }");
            sb.AppendLine();
            sb.AppendLine("        public int? PageSize { get; set; }");
            sb.AppendLine();
            sb.AppendLine("        public string? SortBy { get; set; }");
            sb.AppendLine();
            sb.AppendLine("        public string? SortDirection { get; set; }");

            // Add filterable properties (non-PK, non-audit, simple types)
            var filterableColumns = table.Columns
                .Where(c => !c.IsPrimaryKey && !IsAuditField(c.Name) && !IsReadOnlyPrefix(c.Name))
                .ToList();

            if (filterableColumns.Any())
            {
                sb.AppendLine();
            }

            for (int i = 0; i < filterableColumns.Count; i++)
            {
                var column = filterableColumns[i];
                var (prefix, baseName) = SplitPrefix(column.Name);

                if (prefix == "LKP")
                {
                    // For lookup, only filter by code
                    var propName = ToPascalCase(baseName);
                    sb.AppendLine();
                    sb.AppendLine(CultureInfo.InvariantCulture, $"        public string? {propName}Code {{ get; set; }}");
                }
                else if (prefix is not "LOC" and not "SPT" and not "SPL" and not "UPL" and not "AGG")
                {
                    // Regular filterable property
                    var propertyName = GetPropertyName(column.Name);
                    var csharpType = GetCSharpType(column.DataType, isNullable: true); // Filters are always nullable
                    sb.AppendLine();
                    sb.AppendLine(CultureInfo.InvariantCulture, $"        public {csharpType} {propertyName} {{ get; set; }}");
                }
            }

            sb.AppendLine("    }");
        }

        private static void GenerateResponseProperties(StringBuilder sb, Column column, ApiGeneratorConfig config)
        {
            var (prefix, baseName) = SplitPrefix(column.Name);

            switch (prefix)
            {
                case "ENO":
                    GenerateEnoResponseProperty(sb, column, baseName, config);
                    break;

                case "LKP":
                    GenerateLkpProperties(sb, column, baseName, config);
                    break;

                case "LOC":
                    GenerateLocProperties(sb, column, baseName, config);
                    break;

                case "SPL":
                case "SPT":
                    GenerateSplitListProperty(sb, column, config);
                    break;

                case "UPL":
                    GenerateUploadProperty(sb, column, config);
                    break;

                case "ENM":
                    GenerateEnumProperty(sb, column, baseName, config);
                    break;

                default:
                    GenerateRegularProperty(sb, column, config);
                    break;
            }
        }

        private static void GenerateCreateRequestProperties(StringBuilder sb, Column column, ApiGeneratorConfig config)
        {
            var (prefix, baseName) = SplitPrefix(column.Name);

            switch (prefix)
            {
                case "ENO":
                    // Create request uses plain password
                    GenerateEnoCreateProperty(sb, column, baseName, config);
                    break;

                case "LKP":
                    // Create request only needs code (text is readonly)
                    GenerateLkpCodeProperty(sb, column, baseName, config);
                    break;

                case "LOC":
                    // Create request only needs value (localized is readonly)
                    GenerateLocValueProperty(sb, column, baseName, config);
                    break;

                case "SPL":
                case "SPT":
                    GenerateSplitListProperty(sb, column, config);
                    break;

                case "UPL":
                    GenerateUploadProperty(sb, column, config);
                    break;

                case "ENM":
                    GenerateEnumProperty(sb, column, baseName, config);
                    break;

                default:
                    GenerateRegularProperty(sb, column, config);
                    break;
            }
        }

        private static void GenerateUpdateRequestProperties(StringBuilder sb, Column column, ApiGeneratorConfig config)
        {
            // Update request is the same as create request
            GenerateCreateRequestProperties(sb, column, config);
        }

        private static void GenerateEnoResponseProperty(StringBuilder sb, Column column, string baseName, ApiGeneratorConfig config)
        {
            var propertyName = ToPascalCase(baseName) + "Hashed";
            var csharpType = "string";
            var nullable = IsNullable(column) ? "?" : string.Empty;

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets or sets the hashed password for {baseName}.");
                sb.AppendLine("        /// </summary>");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public {csharpType}{nullable} {propertyName} {{ get; set; }}{GenerateDefaultValue(csharpType, nullable)}");
        }

        private static void GenerateEnoCreateProperty(StringBuilder sb, Column column, string baseName, ApiGeneratorConfig config)
        {
            var propertyName = ToPascalCase(baseName);
            var csharpType = "string";
            var nullable = IsNullable(column) ? "?" : string.Empty;

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets or sets the plain password for {baseName}.");
                sb.AppendLine("        /// </summary>");
            }

            if (config.GenerateValidationAttributes && !IsNullable(column))
            {
                sb.AppendLine("        [Required]");
            }

            if (config.GenerateValidationAttributes && column.MaxLength.HasValue)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        [StringLength({column.MaxLength.Value})]");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public {csharpType}{nullable} {propertyName} {{ get; set; }}{GenerateDefaultValue(csharpType, nullable)}");
        }

        private static void GenerateLkpProperties(StringBuilder sb, Column column, string baseName, ApiGeneratorConfig config)
        {
            var codePropertyName = ToPascalCase(baseName) + "Code";
            var textPropertyName = ToPascalCase(baseName) + "Text";
            var nullable = IsNullable(column) ? "?" : string.Empty;

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets or sets the {baseName} lookup code.");
                sb.AppendLine("        /// </summary>");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public string{nullable} {codePropertyName} {{ get; set; }}{GenerateDefaultValue("string", nullable)}");
            sb.AppendLine();

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets or sets the {baseName} lookup text (readonly).");
                sb.AppendLine("        /// </summary>");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public string? {textPropertyName} {{ get; set; }}");
        }

        private static void GenerateLkpCodeProperty(StringBuilder sb, Column column, string baseName, ApiGeneratorConfig config)
        {
            var propertyName = ToPascalCase(baseName) + "Code";
            var nullable = IsNullable(column) ? "?" : string.Empty;

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets or sets the {baseName} lookup code.");
                sb.AppendLine("        /// </summary>");
            }

            if (config.GenerateValidationAttributes && !IsNullable(column))
            {
                sb.AppendLine("        [Required]");
            }

            if (config.GenerateValidationAttributes && column.MaxLength.HasValue)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        [StringLength({column.MaxLength.Value})]");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public string{nullable} {propertyName} {{ get; set; }}{GenerateDefaultValue("string", nullable)}");
        }

        private static void GenerateLocProperties(StringBuilder sb, Column column, string baseName, ApiGeneratorConfig config)
        {
            var propertyName = ToPascalCase(baseName);
            var localizedPropertyName = propertyName + "Localized";
            var nullable = IsNullable(column) ? "?" : string.Empty;

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets or sets the {baseName} value.");
                sb.AppendLine("        /// </summary>");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public string{nullable} {propertyName} {{ get; set; }}{GenerateDefaultValue("string", nullable)}");
            sb.AppendLine();

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets or sets the localized {baseName} value (readonly).");
                sb.AppendLine("        /// </summary>");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public string? {localizedPropertyName} {{ get; set; }}");
        }

        private static void GenerateLocValueProperty(StringBuilder sb, Column column, string baseName, ApiGeneratorConfig config)
        {
            var propertyName = ToPascalCase(baseName);
            var nullable = IsNullable(column) ? "?" : string.Empty;

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets or sets the {baseName} value.");
                sb.AppendLine("        /// </summary>");
            }

            if (config.GenerateValidationAttributes && !IsNullable(column))
            {
                sb.AppendLine("        [Required]");
            }

            if (config.GenerateValidationAttributes && column.MaxLength.HasValue)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        [StringLength({column.MaxLength.Value})]");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public string{nullable} {propertyName} {{ get; set; }}{GenerateDefaultValue("string", nullable)}");
        }

        private static void GenerateSplitListProperty(StringBuilder sb, Column column, ApiGeneratorConfig config)
        {
            var propertyName = GetPropertyName(column.Name);
            var nullable = IsNullable(column) ? "?" : string.Empty;

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets or sets the {column.Name} as an array.");
                sb.AppendLine("        /// </summary>");
            }

            if (config.GenerateValidationAttributes && !IsNullable(column))
            {
                sb.AppendLine("        [Required]");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public string[]{nullable} {propertyName} {{ get; set; }}{GenerateDefaultValue("string[]", nullable)}");
        }

        private static void GenerateUploadProperty(StringBuilder sb, Column column, ApiGeneratorConfig config)
        {
            var propertyName = GetPropertyName(column.Name);
            var nullable = IsNullable(column) ? "?" : string.Empty;

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets or sets the {column.Name} file path.");
                sb.AppendLine("        /// </summary>");
            }

            if (config.GenerateValidationAttributes && !IsNullable(column))
            {
                sb.AppendLine("        [Required]");
            }

            if (config.GenerateValidationAttributes && column.MaxLength.HasValue)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        [StringLength({column.MaxLength.Value})]");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public string{nullable} {propertyName} {{ get; set; }} // File path{GenerateDefaultValue("string", nullable)}");
        }

        private static void GenerateEnumProperty(StringBuilder sb, Column column, string baseName, ApiGeneratorConfig config)
        {
            var propertyName = ToPascalCase(baseName);
            var enumType = GetClassName(baseName);
            var nullable = IsNullable(column) ? "?" : string.Empty;

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets or sets the {baseName} enum value.");
                sb.AppendLine("        /// </summary>");
            }

            if (config.GenerateValidationAttributes && !IsNullable(column))
            {
                sb.AppendLine("        [Required]");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public {enumType}{nullable} {propertyName} {{ get; set; }}");
        }

        private static void GenerateRegularProperty(StringBuilder sb, Column column, ApiGeneratorConfig config)
        {
            var propertyName = GetPropertyName(column.Name);
            var nullable = IsNullable(column);
            var csharpType = GetCSharpType(column.DataType, nullable);

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets or sets the {column.Name}.");
                sb.AppendLine("        /// </summary>");
            }

            if (config.GenerateValidationAttributes && !nullable)
            {
                sb.AppendLine("        [Required]");
            }

            if (config.GenerateValidationAttributes && column.MaxLength.HasValue && csharpType.Contains("string", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        [StringLength({column.MaxLength.Value})]");
            }

            // Add email validation for email-like column names
            if (config.GenerateValidationAttributes && column.Name.Contains("email", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("        [EmailAddress]");
            }

            var defaultValue = GenerateDefaultValue(csharpType, nullable ? "?" : string.Empty);
            sb.AppendLine(CultureInfo.InvariantCulture, $"        public {csharpType} {propertyName} {{ get; set; }}{defaultValue}");
        }

        private static void GenerateAuditProperties(StringBuilder sb, Table table, ApiGeneratorConfig config)
        {
            var auditColumns = table.Columns.Where(c => IsAuditField(c.Name)).ToList();

            foreach (var column in auditColumns)
            {
                var propertyName = GetPropertyName(column.Name);
                var nullable = IsNullable(column);
                var csharpType = GetCSharpType(column.DataType, nullable);
                var defaultValue = GenerateDefaultValue(csharpType, nullable ? "?" : string.Empty);

                if (config.GenerateXmlDocumentation)
                {
                    sb.AppendLine("        /// <summary>");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets or sets the {column.Name}.");
                    sb.AppendLine("        /// </summary>");
                }

                sb.AppendLine(CultureInfo.InvariantCulture, $"        public {csharpType} {propertyName} {{ get; set; }}{defaultValue}");
                sb.AppendLine();
            }
        }

        private static string GenerateDefaultValue(string csharpType, string nullable)
        {
            if (!string.IsNullOrEmpty(nullable))
            {
                return string.Empty; // Nullable types default to null
            }

            if (csharpType == "string")
            {
                return " = string.Empty;";
            }

            if (csharpType == "string[]")
            {
                return " = Array.Empty<string>();";
            }

            return string.Empty; // Value types default to their default value
        }
    }
}
