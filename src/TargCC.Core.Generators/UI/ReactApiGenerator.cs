// <copyright file="ReactApiGenerator.cs" company="PlaceholderCompany">
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
    /// Generates React API client code from database tables.
    /// </summary>
    public class ReactApiGenerator : BaseUIGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReactApiGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public ReactApiGenerator(ILogger<ReactApiGenerator> logger)
            : base(logger)
        {
        }

        /// <inheritdoc/>
        public override UIGeneratorType GeneratorType => UIGeneratorType.ReactApi;

        /// <inheritdoc/>
        public override async Task<string> GenerateAsync(Table table, DatabaseSchema schema, UIGeneratorConfig config)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(config);

            Logger.LogInformation("Generating React API client for table {TableName}", table.Name);

            return await Task.Run(() => Generate(table, schema, config)).ConfigureAwait(false);
        }

        private string Generate(Table table, DatabaseSchema schema, UIGeneratorConfig config)
        {
            var sb = new StringBuilder();
            var className = GetClassName(table.Name);
            var camelName = GetCamelCaseName(table.Name);
            var apiPath = $"/api/{camelName}s";

            // File header
            sb.Append(GenerateFileHeader(table.Name, GeneratorType));

            // Imports
            sb.AppendLine("import { api } from '../config';");
            sb.AppendLine($"import type {{");
            sb.AppendLine($"  {className},");
            sb.AppendLine($"  Create{className}Request,");
            sb.AppendLine($"  Update{className}Request,");
            sb.AppendLine($"  {className}Filters,");
            sb.AppendLine($"}} from '../types/{className}.types';");
            sb.AppendLine();

            // API object
            sb.AppendLine($"/**");
            sb.AppendLine($" * API client for {className} operations.");
            sb.AppendLine($" * Base URL: {apiPath}");
            sb.AppendLine($" */");
            sb.AppendLine($"export const {camelName}Api = {{");

            // CRUD methods
            sb.AppendLine(GenerateGetById(className, camelName, apiPath));
            sb.AppendLine(GenerateGetAll(className, camelName, apiPath));
            sb.AppendLine(GenerateCreate(className, camelName, apiPath));
            sb.AppendLine(GenerateUpdate(className, camelName, apiPath));
            sb.AppendLine(GenerateDelete(className, camelName, apiPath));

            // GetByXXX from indexes
            foreach (var index in table.Indexes.Where(i => i.Columns.Count == 1))
            {
                var column = table.Columns.FirstOrDefault(c => c.Name == index.Columns[0]);
                if (column != null)
                {
                    sb.AppendLine(GenerateGetByIndex(className, camelName, apiPath, column, index.IsUnique));
                }
            }

            // Relationship methods (one-to-many)
            var relationships = schema.GetChildRelationships(table.Name);
            foreach (var rel in relationships.Take(3)) // Limit to first 3
            {
                sb.AppendLine(GenerateGetChildren(className, camelName, apiPath, rel));
            }

            sb.AppendLine("};");

            return sb.ToString();
        }

        private string GenerateGetById(string className, string camelName, string apiPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("  /**");
            sb.AppendLine($"   * Get {className} by ID.");
            sb.AppendLine("   */");
            sb.AppendLine($"  getById: async (id: number): Promise<{className}> => {{");
            sb.AppendLine($"    const response = await api.get<{className}>(`{apiPath}/${{id}}`);");
            sb.AppendLine("    return response.data;");
            sb.AppendLine("  },");
            sb.AppendLine();
            return sb.ToString();
        }

        private string GenerateGetAll(string className, string camelName, string apiPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("  /**");
            sb.AppendLine($"   * Get all {className}s with optional filters.");
            sb.AppendLine("   */");
            sb.AppendLine($"  getAll: async (filters?: {className}Filters): Promise<{className}[]> => {{");
            sb.AppendLine($"    const response = await api.get<{className}[]>('{apiPath}', {{");
            sb.AppendLine("      params: filters,");
            sb.AppendLine("    });");
            sb.AppendLine("    return response.data;");
            sb.AppendLine("  },");
            sb.AppendLine();
            return sb.ToString();
        }

        private string GenerateCreate(string className, string camelName, string apiPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("  /**");
            sb.AppendLine($"   * Create new {className}.");
            sb.AppendLine("   */");
            sb.AppendLine($"  create: async (data: Create{className}Request): Promise<{className}> => {{");
            sb.AppendLine($"    const response = await api.post<{className}>('{apiPath}', data);");
            sb.AppendLine("    return response.data;");
            sb.AppendLine("  },");
            sb.AppendLine();
            return sb.ToString();
        }

        private string GenerateUpdate(string className, string camelName, string apiPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("  /**");
            sb.AppendLine($"   * Update existing {className}.");
            sb.AppendLine("   */");
            sb.AppendLine($"  update: async (id: number, data: Update{className}Request): Promise<{className}> => {{");
            sb.AppendLine($"    const response = await api.put<{className}>(`{apiPath}/${{id}}`, data);");
            sb.AppendLine("    return response.data;");
            sb.AppendLine("  },");
            sb.AppendLine();
            return sb.ToString();
        }

        private string GenerateDelete(string className, string camelName, string apiPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("  /**");
            sb.AppendLine($"   * Delete {className}.");
            sb.AppendLine("   */");
            sb.AppendLine("  delete: async (id: number): Promise<void> => {");
            sb.AppendLine($"    await api.delete(`{apiPath}/${{id}}`);");
            sb.AppendLine("  },");
            sb.AppendLine();
            return sb.ToString();
        }

        private string GenerateGetByIndex(string className, string camelName, string apiPath, Column column, bool isUnique)
        {
            var sb = new StringBuilder();
            var (prefix, baseName) = SplitPrefix(column.Name);
            var propertyName = GetPropertyName(column.Name);
            var paramName = ToCamelCase(propertyName);
            var methodName = $"getBy{propertyName}";
            var returnType = isUnique ? className : $"{className}[]";
            var paramType = GetTypeScriptTypeForColumn(column);

            sb.AppendLine("  /**");
            sb.AppendLine($"   * Get {className} by {propertyName}.");
            sb.AppendLine("   */");
            sb.AppendLine($"  {methodName}: async ({paramName}: {paramType}): Promise<{returnType}> => {{");
            sb.AppendLine($"    const response = await api.get<{returnType}>(`{apiPath}/by-{ToCamelCase(baseName)}/${{encodeURIComponent(String({paramName}))}}`);");
            sb.AppendLine("    return response.data;");
            sb.AppendLine("  },");
            sb.AppendLine();

            return sb.ToString();
        }

        private string GenerateGetChildren(string className, string camelName, string apiPath, Relationship relationship)
        {
            var childClassName = GetClassName(relationship.ChildTable);
            var childCamelName = GetCamelCaseName(relationship.ChildTable);

            var sb = new StringBuilder();
            sb.AppendLine("  /**");
            sb.AppendLine($"   * Get {childClassName}s for this {className}.");
            sb.AppendLine("   */");
            sb.AppendLine($"  get{childClassName}s: async (id: number): Promise<{childClassName}[]> => {{");
            sb.AppendLine($"    const response = await api.get<{childClassName}[]>(`{apiPath}/${{id}}/{childCamelName}s`);");
            sb.AppendLine("    return response.data;");
            sb.AppendLine("  },");
            sb.AppendLine();

            return sb.ToString();
        }

        private string GetTypeScriptTypeForColumn(Column column)
        {
            var sqlType = column.DataType.ToLowerInvariant();

            return sqlType switch
            {
                _ when sqlType.Contains("int") => "number",
                _ when sqlType.Contains("decimal") => "number",
                _ when sqlType.Contains("numeric") => "number",
                _ when sqlType.Contains("float") => "number",
                _ when sqlType.Contains("bit") => "boolean",
                _ when sqlType.Contains("date") => "Date",
                _ => "string",
            };
        }
    }
}
