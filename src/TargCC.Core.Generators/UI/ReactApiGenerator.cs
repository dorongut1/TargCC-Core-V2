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

            return await Task.Run(() => Generate(table, schema)).ConfigureAwait(false);
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
            sb.AppendLine("    // Use /filter endpoint when filters are provided");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    const endpoint = filters && Object.keys(filters).length > 0 ? '{apiPath}/filter' : '{apiPath}';");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    const response = await api.get<{className}[]>(endpoint, {{");
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

        private static string GenerateGetRelatedData(string parentClassName, string apiPath, Table childTable)
        {
            var childClassName = GetClassName(childTable.Name);
            var childrenName = Pluralize(childClassName);
            var childrenLowerCase = childrenName.ToUpper(CultureInfo.InvariantCulture);

            var sb = new StringBuilder();
            sb.AppendLine("  /**");
            sb.AppendLine(CultureInfo.InvariantCulture, $"   * Get {childrenLowerCase} for this {parentClassName.ToUpper(CultureInfo.InvariantCulture)}.");
            sb.AppendLine("   */");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  get{childrenName}: async (id: number, skip?: number, take?: number): Promise<{childClassName}[]> => {{");
            sb.AppendLine("    const params = new URLSearchParams();");
            sb.AppendLine("    if (skip !== undefined) params.append('skip', skip.toString());");
            sb.AppendLine("    if (take !== undefined) params.append('take', take.toString());");
            sb.AppendLine("    const queryString = params.toString() ? `?${params.toString()}` : '';");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    const response = await api.get<{childClassName}[]>(`{apiPath}/${{id}}/{childrenLowerCase}${{queryString}}`);");
            sb.AppendLine("    return response.data;");
            sb.AppendLine("  },");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string Pluralize(string singular)
        {
            if (string.IsNullOrEmpty(singular))
            {
                return singular;
            }

            // Category → Categories
            // CA1867: String literals required here because char overload doesn't support StringComparison
#pragma warning disable CA1867
            if (singular.EndsWith("y", StringComparison.OrdinalIgnoreCase) &&
                !singular.EndsWith("ay", StringComparison.OrdinalIgnoreCase) &&
                !singular.EndsWith("ey", StringComparison.OrdinalIgnoreCase) &&
                !singular.EndsWith("oy", StringComparison.OrdinalIgnoreCase) &&
                !singular.EndsWith("uy", StringComparison.OrdinalIgnoreCase))
            {
                return singular[..^1] + "ies";
            }

            // Address → Addresses, Box → Boxes
            if (singular.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
                singular.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
                singular.EndsWith("z", StringComparison.OrdinalIgnoreCase) ||
                singular.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
                singular.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
#pragma warning restore CA1867
            {
                return singular + "es";
            }

            // Order → Orders, Customer → Customers
            return singular + "s";
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

        private static void GenerateImports(StringBuilder sb, Table table, DatabaseSchema schema, string className)
        {
            sb.AppendLine("import { api } from './client';");
            sb.AppendLine("import type {");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  {className},");

            // Only import write types for tables, not for VIEWs
            if (!table.IsView)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"  Create{className}Request,");
                sb.AppendLine(CultureInfo.InvariantCulture, $"  Update{className}Request,");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"  {className}Filters,");
            sb.AppendLine(CultureInfo.InvariantCulture, $"}} from '../types/{className}.types';");

            // Import child entity types for related data methods
            if (schema.Relationships != null)
            {
                var parentRelationships = schema.Relationships
                    .Where(r => r.ParentTable == table.FullName && r.IsEnabled)
                    .ToList();

                foreach (var relationship in parentRelationships)
                {
                    var childTable = schema.Tables.Find(t => t.FullName == relationship.ChildTable);
                    if (childTable != null)
                    {
                        var childClassName = GetClassName(childTable.Name);
                        sb.AppendLine(CultureInfo.InvariantCulture, $"import type {{ {childClassName} }} from '../types/{childClassName}.types';");
                    }
                }
            }

            sb.AppendLine();
        }

        /// <summary>
        /// Generates the API object header with documentation.
        /// </summary>
        private static void GenerateApiObjectHeader(StringBuilder sb, string className, string camelName, string apiPath)
        {
            sb.AppendLine("/**");
            sb.AppendLine(CultureInfo.InvariantCulture, $" * API client for {className} operations.");
            sb.AppendLine(CultureInfo.InvariantCulture, $" * Base URL: {apiPath}");
            sb.AppendLine(" */");
            sb.AppendLine(CultureInfo.InvariantCulture, $"export const {camelName}Api = {{");
        }

        /// <summary>
        /// Generates CRUD methods (Create, Read, Update, Delete).
        /// </summary>
        private static void GenerateCrudMethods(StringBuilder sb, Table table, string className, string apiPath)
        {
            // VIEWs are read-only - only generate Get methods
            if (!table.IsView)
            {
                sb.AppendLine(GenerateGetById(className, apiPath));
            }

            sb.AppendLine(GenerateGetAll(className, apiPath));

            // Write methods only for tables, not for VIEWs
            if (!table.IsView)
            {
                sb.AppendLine(GenerateCreate(className, apiPath));
                sb.AppendLine(GenerateUpdate(className, apiPath));
                sb.AppendLine(GenerateDelete(className, apiPath));
            }
        }

        /// <summary>
        /// Generates GetByXXX methods from table indexes.
        /// </summary>
        private static void GenerateIndexMethods(StringBuilder sb, Table table, string className, string apiPath)
        {
            foreach (var index in table.Indexes.Where(i => i.ColumnNames.Count == 1 && !i.IsPrimaryKey))
            {
                var column = table.Columns.Find(c => c.Name == index.ColumnNames[0]);
                if (column != null)
                {
                    sb.AppendLine(GenerateGetByIndex(className, apiPath, column, index.IsUnique));
                }
            }
        }

        /// <summary>
        /// Generates methods for related data (Master-Detail Views).
        /// </summary>
        private static void GenerateRelatedDataMethods(StringBuilder sb, Table table, DatabaseSchema schema, string className, string apiPath)
        {
            if (schema.Relationships == null)
            {
                return;
            }

            var parentRelationships = schema.Relationships
                .Where(r => r.ParentTable == table.FullName && r.IsEnabled)
                .ToList();

            foreach (var relationship in parentRelationships)
            {
                var childTable = schema.Tables.Find(t => t.FullName == relationship.ChildTable);
                if (childTable != null)
                {
                    sb.AppendLine(GenerateGetRelatedData(className, apiPath, childTable));
                }
            }
        }

        private string Generate(Table table, DatabaseSchema schema)
        {
            var sb = new StringBuilder();
            var className = GetClassName(table.Name);
            var camelName = GetCamelCaseName(table.Name);

            // Use PascalCase for API path to match ASP.NET Core controller routing
            // Controller: VwCustomerOrderSummariesController → /api/VwCustomerOrderSummaries
            var apiPath = $"/{Pluralize(className)}";

            // File header
            sb.Append(GenerateFileHeader(table.Name, GeneratorType));

            // Imports
            GenerateImports(sb, table, schema, className);

            // API object
            GenerateApiObjectHeader(sb, className, camelName, apiPath);

            // CRUD methods
            GenerateCrudMethods(sb, table, className, apiPath);

            // GetByXXX from indexes
            GenerateIndexMethods(sb, table, className, apiPath);

            // Related data methods (Master-Detail Views)
            GenerateRelatedDataMethods(sb, table, schema, className, apiPath);

            sb.AppendLine("};");

            return sb.ToString();
        }
    }
}
