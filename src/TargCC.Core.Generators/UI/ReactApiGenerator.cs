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
        private static readonly Action<ILogger, string, Exception?> LogGeneratingApiClient =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1, nameof(LogGeneratingApiClient)),
                "Generating React API client for table {TableName}");

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

            LogGeneratingApiClient(Logger, table.Name, null);

            return await Task.Run(() => Generate(table)).ConfigureAwait(false);
        }

        private static string GenerateGetById(string className, string apiPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("  /**");
            sb.AppendLine(CultureInfo.InvariantCulture, $"   * Get {className} by ID.");
            sb.AppendLine("   */");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  getById: async (id: number): Promise<{className}> => {{");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    const response = await api.get<{className}>(`{apiPath}/${{id}}`);");
            sb.AppendLine("    return response.data;");
            sb.AppendLine("  },");
            sb.AppendLine();
            return sb.ToString();
        }

        private static string GenerateGetAll(string className, string apiPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("  /**");
            sb.AppendLine(CultureInfo.InvariantCulture, $"   * Get all {className}s with optional filters.");
            sb.AppendLine("   */");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  getAll: async (filters?: {className}Filters): Promise<{className}[]> => {{");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    const response = await api.get<{className}[]>('{apiPath}', {{");
            sb.AppendLine("      params: filters,");
            sb.AppendLine("    });");
            sb.AppendLine("    return response.data;");
            sb.AppendLine("  },");
            sb.AppendLine();
            return sb.ToString();
        }

        private static string GenerateCreate(string className, string apiPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("  /**");
            sb.AppendLine(CultureInfo.InvariantCulture, $"   * Create new {className}.");
            sb.AppendLine("   */");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  create: async (data: Create{className}Request): Promise<{className}> => {{");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    const response = await api.post<{className}>('{apiPath}', data);");
            sb.AppendLine("    return response.data;");
            sb.AppendLine("  },");
            sb.AppendLine();
            return sb.ToString();
        }

        private static string GenerateUpdate(string className, string apiPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("  /**");
            sb.AppendLine(CultureInfo.InvariantCulture, $"   * Update existing {className}.");
            sb.AppendLine("   */");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  update: async (id: number, data: Update{className}Request): Promise<{className}> => {{");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    const response = await api.put<{className}>(`{apiPath}/${{id}}`, data);");
            sb.AppendLine("    return response.data;");
            sb.AppendLine("  },");
            sb.AppendLine();
            return sb.ToString();
        }

        private static string GenerateDelete(string className, string apiPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("  /**");
            sb.AppendLine(CultureInfo.InvariantCulture, $"   * Delete {className}.");
            sb.AppendLine("   */");
            sb.AppendLine("  delete: async (id: number): Promise<void> => {");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    await api.delete(`{apiPath}/${{id}}`);");
            sb.AppendLine("  },");
            sb.AppendLine();
            return sb.ToString();
        }

        private static string GenerateGetByIndex(string className, string apiPath, Column column, bool isUnique)
        {
            var sb = new StringBuilder();
            var (_, baseName) = SplitPrefix(column.Name);
            var propertyName = GetPropertyName(column.Name);
            var paramName = ToCamelCase(propertyName);
            var methodName = $"getBy{propertyName}";
            var returnType = isUnique ? className : $"{className}[]";
            var paramType = GetTypeScriptTypeForColumn(column);

            sb.AppendLine("  /**");
            sb.AppendLine(CultureInfo.InvariantCulture, $"   * Get {className} by {propertyName}.");
            sb.AppendLine("   */");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  {methodName}: async ({paramName}: {paramType}): Promise<{returnType}> => {{");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    const response = await api.get<{returnType}>(`{apiPath}/by-{ToCamelCase(baseName)}/${{encodeURIComponent(String({paramName}))}}`);");
            sb.AppendLine("    return response.data;");
            sb.AppendLine("  },");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string GenerateGetChildren(string className, string apiPath, Relationship relationship)
        {
            var childClassName = GetClassName(relationship.ChildTable);
            var childCamelName = GetCamelCaseName(relationship.ChildTable);

            var sb = new StringBuilder();
            sb.AppendLine("  /**");
            sb.AppendLine(CultureInfo.InvariantCulture, $"   * Get {childClassName}s for this {className}.");
            sb.AppendLine("   */");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  get{childClassName}s: async (id: number): Promise<{childClassName}[]> => {{");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    const response = await api.get<{childClassName}[]>(`{apiPath}/${{id}}/{childCamelName}s`);");
            sb.AppendLine("    return response.data;");
            sb.AppendLine("  },");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string GetTypeScriptTypeForColumn(Column column)
        {
            var sqlType = column.DataType.ToUpperInvariant();

            return sqlType switch
            {
                _ when sqlType.Contains("INT", StringComparison.Ordinal) => "number",
                _ when sqlType.Contains("DECIMAL", StringComparison.Ordinal) => "number",
                _ when sqlType.Contains("NUMERIC", StringComparison.Ordinal) => "number",
                _ when sqlType.Contains("FLOAT", StringComparison.Ordinal) => "number",
                _ when sqlType.Contains("BIT", StringComparison.Ordinal) => "boolean",
                _ when sqlType.Contains("DATE", StringComparison.Ordinal) => "Date",
                _ => "string",
            };
        }

        private string Generate(Table table)
        {
            var sb = new StringBuilder();
            var className = GetClassName(table.Name);
            var camelName = GetCamelCaseName(table.Name);
            var apiPath = $"/api/{camelName}s";

            // File header
            sb.Append(GenerateFileHeader(table.Name, GeneratorType));

            // Imports
            sb.AppendLine("import { api } from '../config';");
            sb.AppendLine("import type {");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  {className},");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  Create{className}Request,");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  Update{className}Request,");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  {className}Filters,");
            sb.AppendLine(CultureInfo.InvariantCulture, $"}} from '../types/{className}.types';");
            sb.AppendLine();

            // API object
            sb.AppendLine("/**");
            sb.AppendLine(CultureInfo.InvariantCulture, $" * API client for {className} operations.");
            sb.AppendLine(CultureInfo.InvariantCulture, $" * Base URL: {apiPath}");
            sb.AppendLine(" */");
            sb.AppendLine(CultureInfo.InvariantCulture, $"export const {camelName}Api = {{");

            // CRUD methods
            sb.AppendLine(GenerateGetById(className, apiPath));
            sb.AppendLine(GenerateGetAll(className, apiPath));
            sb.AppendLine(GenerateCreate(className, apiPath));
            sb.AppendLine(GenerateUpdate(className, apiPath));
            sb.AppendLine(GenerateDelete(className, apiPath));

            // GetByXXX from indexes
            foreach (var index in table.Indexes.Where(i => i.ColumnNames.Count == 1 && !i.IsPrimaryKey))
            {
                var column = table.Columns.Find(c => c.Name == index.ColumnNames[0]);
                if (column != null)
                {
                    sb.AppendLine(GenerateGetByIndex(className, apiPath, column, index.IsUnique));
                }
            }

            // Relationship methods (one-to-many)
            var relationships = table.Relationships.Where(r => r.ChildTable == table.Name).Take(3);
            foreach (var rel in relationships)
            {
                sb.AppendLine(GenerateGetChildren(className, apiPath, rel));
            }

            sb.AppendLine("};");

            return sb.ToString();
        }
    }
}
