// <copyright file="TypeScriptTypeGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Generates TypeScript types and interfaces from database tables.
    /// </summary>
    public class TypeScriptTypeGenerator : BaseUIGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeScriptTypeGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public TypeScriptTypeGenerator(ILogger<TypeScriptTypeGenerator> logger)
            : base(logger)
        {
        }

        /// <inheritdoc/>
        public override UIGeneratorType GeneratorType => UIGeneratorType.TypeScriptTypes;

        /// <inheritdoc/>
        public override async Task<string> GenerateAsync(Table table, DatabaseSchema schema, UIGeneratorConfig config)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(config);

            Logger.LogInformation("Generating TypeScript types for table {TableName}", table.Name);

            return await Task.Run(() => Generate(table, schema, config)).ConfigureAwait(false);
        }

        private string Generate(Table table, DatabaseSchema schema, UIGeneratorConfig config)
        {
            var sb = new StringBuilder();

            // File header
            sb.Append(GenerateFileHeader(table.Name, GeneratorType));

            // Generate enums first (if any)
            var enums = GenerateEnums(table);
            if (!string.IsNullOrEmpty(enums))
            {
                sb.AppendLine(enums);
                sb.AppendLine();
            }

            // Main interface
            sb.AppendLine(GenerateMainInterface(table, schema));
            sb.AppendLine();

            // Create request interface
            sb.AppendLine(GenerateCreateRequestInterface(table));
            sb.AppendLine();

            // Update request interface
            sb.AppendLine(GenerateUpdateRequestInterface(table));
            sb.AppendLine();

            // Filters interface
            sb.AppendLine(GenerateFiltersInterface(table));

            return sb.ToString();
        }

        private string GenerateMainInterface(Table table, DatabaseSchema schema)
        {
            var className = GetClassName(table.Name);
            var sb = new StringBuilder();

            sb.AppendLine($"/**");
            sb.AppendLine($" * {className} entity interface.");
            sb.AppendLine($" * Generated from table: {table.Name}");
            sb.AppendLine($" */");
            sb.AppendLine($"export interface {className} {{");

            var dataColumns = GetDataColumns(table);

            foreach (var column in dataColumns)
            {
                var properties = GeneratePropertyForColumn(column, schema, isCreate: false);
                foreach (var prop in properties)
                {
                    sb.AppendLine($"  {prop}");
                }
            }

            // Add audit fields if exposed
            if (table.HasAuditFields)
            {
                sb.AppendLine("  // Audit fields");
                sb.AppendLine("  readonly addedBy?: number;");
                sb.AppendLine("  readonly addedOn?: Date;");
                sb.AppendLine("  readonly changedBy?: number;");
                sb.AppendLine("  readonly changedOn?: Date;");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateCreateRequestInterface(Table table)
        {
            var className = GetClassName(table.Name);
            var sb = new StringBuilder();

            sb.AppendLine($"/**");
            sb.AppendLine($" * Create {className} request interface.");
            sb.AppendLine($" */");
            sb.AppendLine($"export interface Create{className}Request {{");

            var dataColumns = GetDataColumns(table)
                .Where(c => !c.IsPrimaryKey && !c.IsIdentity);

            foreach (var column in dataColumns)
            {
                var properties = GeneratePropertyForColumn(column, null, isCreate: true);
                foreach (var prop in properties)
                {
                    sb.AppendLine($"  {prop}");
                }
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateUpdateRequestInterface(Table table)
        {
            var className = GetClassName(table.Name);
            var sb = new StringBuilder();

            sb.AppendLine($"/**");
            sb.AppendLine($" * Update {className} request interface.");
            sb.AppendLine($" */");
            sb.AppendLine($"export interface Update{className}Request extends Create{className}Request {{");

            // Add primary key
            var pkColumn = table.Columns.FirstOrDefault(c => c.IsPrimaryKey);
            if (pkColumn != null)
            {
                var pkName = ToCamelCase(pkColumn.Name);
                var pkType = GetTypeScriptType(pkColumn.DataType);
                sb.AppendLine($"  {pkName}: {pkType};");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateFiltersInterface(Table table)
        {
            var className = GetClassName(table.Name);
            var sb = new StringBuilder();

            sb.AppendLine($"/**");
            sb.AppendLine($" * {className} filters for querying.");
            sb.AppendLine($" */");
            sb.AppendLine($"export interface {className}Filters {{");

            // Add common filters
            var searchableColumns = GetDataColumns(table)
                .Where(c => IsSearchableType(c.DataType))
                .Take(5); // Limit to first 5 searchable columns

            foreach (var column in searchableColumns)
            {
                var propName = ToCamelCase(GetPropertyName(column.Name));
                var propType = GetTypeScriptType(column.DataType);
                sb.AppendLine($"  {propName}?: {propType};");
            }

            // Add date range filters if date columns exist
            var dateColumns = GetDataColumns(table)
                .Where(c => c.DataType.Contains("date", StringComparison.OrdinalIgnoreCase))
                .Take(2);

            foreach (var column in dateColumns)
            {
                var baseName = ToCamelCase(GetPropertyName(column.Name));
                sb.AppendLine($"  {baseName}From?: Date;");
                sb.AppendLine($"  {baseName}To?: Date;");
            }

            // Pagination
            sb.AppendLine("  page?: number;");
            sb.AppendLine("  pageSize?: number;");
            sb.AppendLine("  sortBy?: string;");
            sb.AppendLine("  sortDirection?: 'asc' | 'desc';");

            sb.AppendLine("}");

            return sb.ToString();
        }

        private List<string> GeneratePropertyForColumn(Column column, DatabaseSchema? schema, bool isCreate)
        {
            var result = new List<string>();
            var (prefix, baseName) = SplitPrefix(column.Name);
            var basePropertyName = ToCamelCase(GetPropertyName(column.Name));
            var tsType = GetTypeScriptType(column.DataType);
            var nullable = IsNullable(column);
            var optional = nullable || !column.IsRequired ? "?" : string.Empty;
            var readonly = isCreate ? string.Empty : "readonly ";

            switch (prefix)
            {
                case "eno": // Hashed password
                    if (isCreate)
                    {
                        // In create, send plain password
                        result.Add($"plainPassword{ToPascalCase(baseName)}: string;");
                    }
                    else
                    {
                        // In entity, show hashed (readonly)
                        result.Add($"readonly {basePropertyName}: string{optional};");
                    }

                    break;

                case "lkp": // Lookup - generates 2 properties
                    result.Add($"{ToCamelCase(baseName)}Code: string{optional};");
                    if (!isCreate)
                    {
                        result.Add($"readonly {ToCamelCase(baseName)}Text{optional}: string{optional};");
                    }

                    break;

                case "enm": // Enum
                    var enumName = GetClassName(baseName);
                    result.Add($"{basePropertyName}: {enumName}{optional};");
                    break;

                case "loc": // Localized - generates 2 properties
                    result.Add($"{basePropertyName}: string{optional};");
                    if (!isCreate)
                    {
                        result.Add($"readonly {basePropertyName}Localized{optional}: string{optional};");
                    }

                    break;

                case "clc": // Calculated - readonly
                case "blg": // Business logic - readonly
                    if (!isCreate)
                    {
                        result.Add($"readonly {basePropertyName}{optional}: {tsType}{optional};");
                    }

                    break;

                case "agg": // Aggregate - readonly
                    if (!isCreate)
                    {
                        result.Add($"readonly {basePropertyName}{optional}: {tsType}{optional};");
                    }

                    break;

                case "spt": // Separate update
                    result.Add($"{basePropertyName}: {tsType}{optional};");
                    break;

                case "scb": // Separate changed by
                    if (!isCreate)
                    {
                        result.Add($"readonly {basePropertyName}{optional}: string{optional};");
                    }

                    break;

                case "spl": // Split list
                    result.Add($"{basePropertyName}: string[]{optional};");
                    break;

                case "upl": // Upload
                    result.Add($"{basePropertyName}: string{optional}; // File path");
                    break;

                case "ent": // Encrypted
                default:
                    // Regular property
                    var readonlyModifier = (prefix == "clc" || prefix == "blg" || prefix == "agg") && !isCreate ? "readonly " : string.Empty;
                    result.Add($"{readonlyModifier}{basePropertyName}{optional}: {tsType}{optional};");
                    break;
            }

            return result;
        }

        private string GenerateEnums(Table table)
        {
            var sb = new StringBuilder();
            var enumColumns = table.Columns.Where(c => c.Name.StartsWith("enm_", StringComparison.OrdinalIgnoreCase));

            foreach (var column in enumColumns)
            {
                var (prefix, baseName) = SplitPrefix(column.Name);
                var enumName = GetClassName(baseName);

                sb.AppendLine($"/**");
                sb.AppendLine($" * {enumName} enum.");
                sb.AppendLine($" */");
                sb.AppendLine($"export enum {enumName} {{");

                // TODO: Get actual enum values from c_Enumeration table
                // For now, generate placeholders
                sb.AppendLine("  Undefined = 0,");
                sb.AppendLine("  Value1 = 1,");
                sb.AppendLine("  Value2 = 2,");

                sb.AppendLine("}");
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        private string GetTypeScriptType(string sqlType)
        {
            var type = sqlType.ToLowerInvariant();

            return type switch
            {
                _ when type.Contains("int") => "number",
                _ when type.Contains("decimal") => "number",
                _ when type.Contains("numeric") => "number",
                _ when type.Contains("float") => "number",
                _ when type.Contains("real") => "number",
                _ when type.Contains("money") => "number",
                _ when type.Contains("bit") => "boolean",
                _ when type.Contains("date") => "Date",
                _ when type.Contains("time") => "Date",
                _ when type.Contains("char") => "string",
                _ when type.Contains("text") => "string",
                _ when type.Contains("binary") => "Uint8Array",
                _ when type.Contains("image") => "Uint8Array",
                _ => "string", // Default to string
            };
        }

        private bool IsSearchableType(string sqlType)
        {
            var type = sqlType.ToLowerInvariant();
            return type.Contains("char") || type.Contains("text") ||
                   type.Contains("int") || type.Contains("decimal");
        }
    }
}
