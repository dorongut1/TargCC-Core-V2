// <copyright file="ReactHookGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Generates React Query hooks from database tables.
    /// </summary>
    public class ReactHookGenerator : BaseUIGenerator
    {
        private static readonly Action<ILogger, string, Exception?> LogGeneratingHooks =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1, nameof(LogGeneratingHooks)),
                "Generating React hooks for table {TableName}");

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactHookGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public ReactHookGenerator(ILogger<ReactHookGenerator> logger)
            : base(logger)
        {
        }

        /// <inheritdoc/>
        public override UIGeneratorType GeneratorType => UIGeneratorType.ReactHooks;

        /// <inheritdoc/>
        public override async Task<string> GenerateAsync(Table table, DatabaseSchema schema, UIGeneratorConfig config)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(config);

            LogGeneratingHooks(Logger, table.Name, null);

            return await Task.Run(() => Generate(table, schema)).ConfigureAwait(false);
        }

        private static string GenerateUseEntityHook(string className, string camelName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("/**");
            sb.AppendLine(CultureInfo.InvariantCulture, $" * Hook to fetch single {className} by ID.");
            sb.AppendLine(" */");
            sb.AppendLine(CultureInfo.InvariantCulture, $"export const use{className} = (id: number | null) => {{");
            sb.AppendLine("  return useQuery({");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    queryKey: ['{camelName}', id],");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    queryFn: () => {camelName}Api.getById(id!),");
            sb.AppendLine("    enabled: id !== null,");
            sb.AppendLine("  });");
            sb.AppendLine("};");
            sb.AppendLine();
            return sb.ToString();
        }

        private static string GenerateUseEntitiesHook(string className, string camelName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("/**");
            sb.AppendLine(CultureInfo.InvariantCulture, $" * Hook to fetch all {className}s with optional filters.");
            sb.AppendLine(" */");
            sb.AppendLine(CultureInfo.InvariantCulture, $"export const use{className}s = (filters?: {className}Filters) => {{");
            sb.AppendLine("  return useQuery({");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    queryKey: ['{camelName}s', filters],");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    queryFn: () => {camelName}Api.getAll(filters),");
            sb.AppendLine("  });");
            sb.AppendLine("};");
            sb.AppendLine();
            return sb.ToString();
        }

        private static string GenerateUseCreateHook(string className, string camelName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("/**");
            sb.AppendLine(CultureInfo.InvariantCulture, $" * Hook to create {className}.");
            sb.AppendLine(" */");
            sb.AppendLine(CultureInfo.InvariantCulture, $"export const useCreate{className} = () => {{");
            sb.AppendLine("  const queryClient = useQueryClient();");
            sb.AppendLine();
            sb.AppendLine("  return useMutation({");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    mutationFn: (data: Create{className}Request) => {camelName}Api.create(data),");
            sb.AppendLine("    onSuccess: () => {");
            sb.AppendLine(CultureInfo.InvariantCulture, $"      queryClient.invalidateQueries({{ queryKey: ['{camelName}s'] }});");
            sb.AppendLine("    },");
            sb.AppendLine("  });");
            sb.AppendLine("};");
            sb.AppendLine();
            return sb.ToString();
        }

        private static string GenerateUseUpdateHook(string className, string camelName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("/**");
            sb.AppendLine(CultureInfo.InvariantCulture, $" * Hook to update {className}.");
            sb.AppendLine(" */");
            sb.AppendLine(CultureInfo.InvariantCulture, $"export const useUpdate{className} = () => {{");
            sb.AppendLine("  const queryClient = useQueryClient();");
            sb.AppendLine();
            sb.AppendLine("  return useMutation({");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    mutationFn: ({{ id, data }}: {{ id: number; data: Update{className}Request }}) =>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"      {camelName}Api.update(id, data),");
            sb.AppendLine("    onSuccess: (_, variables) => {");
            sb.AppendLine(CultureInfo.InvariantCulture, $"      queryClient.invalidateQueries({{ queryKey: ['{camelName}', variables.id] }});");
            sb.AppendLine(CultureInfo.InvariantCulture, $"      queryClient.invalidateQueries({{ queryKey: ['{camelName}s'] }});");
            sb.AppendLine("    },");
            sb.AppendLine("  });");
            sb.AppendLine("};");
            sb.AppendLine();
            return sb.ToString();
        }

        private static string GenerateUseDeleteHook(string className, string camelName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("/**");
            sb.AppendLine(CultureInfo.InvariantCulture, $" * Hook to delete {className}.");
            sb.AppendLine(" */");
            sb.AppendLine(CultureInfo.InvariantCulture, $"export const useDelete{className} = () => {{");
            sb.AppendLine("  const queryClient = useQueryClient();");
            sb.AppendLine();
            sb.AppendLine("  return useMutation({");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    mutationFn: (id: number) => {camelName}Api.delete(id),");
            sb.AppendLine("    onSuccess: () => {");
            sb.AppendLine(CultureInfo.InvariantCulture, $"      queryClient.invalidateQueries({{ queryKey: ['{camelName}s'] }});");
            sb.AppendLine("    },");
            sb.AppendLine("  });");
            sb.AppendLine("};");
            sb.AppendLine();
            return sb.ToString();
        }

        private static void GenerateImports(StringBuilder sb, Table table, DatabaseSchema schema, string className, string camelName)
        {
            sb.AppendLine("import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';");
            sb.AppendLine(CultureInfo.InvariantCulture, $"import {{ {camelName}Api }} from '../api/{camelName}Api';");
            sb.AppendLine(CultureInfo.InvariantCulture, $"import type {{");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  {className},");

            // Only import write types for tables, not for VIEWs
            if (!table.IsView)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"  Create{className}Request,");
                sb.AppendLine(CultureInfo.InvariantCulture, $"  Update{className}Request,");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"  {className}Filters,");
            sb.AppendLine(CultureInfo.InvariantCulture, $"}} from '../types/{className}.types';");

            // Import child entity types for related data hooks
            // Use HashSet to avoid duplicate imports when multiple relationships point to same table
            if (schema.Relationships != null)
            {
                var parentRelationships = schema.Relationships
                    .Where(r => r.ParentTable == table.FullName && r.IsEnabled)
                    .ToList();

                // Initialize with className to prevent importing the main entity again (e.g., self-references)
                var importedTypes = new HashSet<string> { className };

                foreach (var relationship in parentRelationships)
                {
                    var childTable = schema.Tables.Find(t => t.FullName == relationship.ChildTable);
                    if (childTable != null)
                    {
                        var childClassName = GetClassName(childTable.Name);
                        if (importedTypes.Add(childClassName))
                        {
                            sb.AppendLine(CultureInfo.InvariantCulture, $"import type {{ {childClassName} }} from '../types/{childClassName}.types';");
                        }
                    }
                }
            }

            sb.AppendLine();
        }

        private static void GenerateRelatedDataHooks(StringBuilder sb, Table table, DatabaseSchema schema)
        {
            var className = GetClassName(table.Name);
            var camelName = GetCamelCaseName(table.Name);

            var parentRelationships = schema.Relationships
                .Where(r => r.ParentTable == table.FullName && r.IsEnabled)
                .ToList();

            // Use HashSet to avoid duplicate hooks when multiple relationships point to same table
            var generatedHooks = new HashSet<string>();

            foreach (var relationship in parentRelationships)
            {
                var childTable = schema.Tables.Find(t => t.FullName == relationship.ChildTable);
                if (childTable == null)
                {
                    continue;
                }

                var childTableName = childTable.FullName;
                if (!generatedHooks.Add(childTableName))
                {
                    continue;
                }

                try
                {
                    GenerateSingleRelatedDataHook(sb, childTable, className, camelName);
                }
                catch
                {
                    // Skip relationships that cannot be generated
                }
            }
        }

        private static void GenerateSingleRelatedDataHook(StringBuilder sb, Table childTable, string parentClassName, string parentCamelName)
        {
            var childClassName = GetClassName(childTable.Name);
            var childrenName = Pluralize(childClassName);
            var childrenCamelCase = GetCamelCaseName(childrenName);

            sb.AppendLine("/**");
            sb.AppendLine(CultureInfo.InvariantCulture, $" * Hook to fetch {childrenCamelCase} for a specific {parentClassName.ToUpper(CultureInfo.InvariantCulture)}.");
            sb.AppendLine(" */");
            sb.AppendLine(CultureInfo.InvariantCulture, $"export const use{parentClassName}{childrenName} = (");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  {parentCamelName}Id: number | null,");
            sb.AppendLine("  skip?: number,");
            sb.AppendLine("  take?: number");
            sb.AppendLine(") => {");
            sb.AppendLine("  return useQuery({");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    queryKey: ['{parentCamelName}', {parentCamelName}Id, '{childrenCamelCase}', skip, take],");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    queryFn: () => {parentCamelName}Api.get{childrenName}({parentCamelName}Id!, skip, take),");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    enabled: {parentCamelName}Id !== null,");
            sb.AppendLine("  });");
            sb.AppendLine("};");
            sb.AppendLine();
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

        private string Generate(Table table, DatabaseSchema schema)
        {
            var sb = new StringBuilder();
            var className = GetClassName(table.Name);
            var camelName = GetCamelCaseName(table.Name);

            // File header
            sb.Append(GenerateFileHeader(table.Name, GeneratorType));

            // Imports
            GenerateImports(sb, table, schema, className, camelName);

            // Query hooks
            // VIEWs are read-only - no getById hook
            if (!table.IsView)
            {
                sb.AppendLine(GenerateUseEntityHook(className, camelName));
            }

            sb.AppendLine(GenerateUseEntitiesHook(className, camelName));

            // Related data hooks (Master-Detail Views)
            if (schema.Relationships != null && schema.Relationships.Count > 0)
            {
                GenerateRelatedDataHooks(sb, table, schema);
            }

            // Mutation hooks - only for tables, not for VIEWs
            if (!table.IsView)
            {
                sb.AppendLine(GenerateUseCreateHook(className, camelName));
                sb.AppendLine(GenerateUseUpdateHook(className, camelName));
                sb.AppendLine(GenerateUseDeleteHook(className, camelName));
            }

            return sb.ToString();
        }
    }
}
