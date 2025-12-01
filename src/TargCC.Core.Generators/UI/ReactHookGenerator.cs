// <copyright file="ReactHookGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI
{
    using System;
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

            Logger.LogInformation("Generating React hooks for table {TableName}", table.Name);

            return await Task.Run(() => Generate(table, schema, config)).ConfigureAwait(false);
        }

        private string Generate(Table table, DatabaseSchema schema, UIGeneratorConfig config)
        {
            var sb = new StringBuilder();
            var className = GetClassName(table.Name);
            var camelName = GetCamelCaseName(table.Name);

            // File header
            sb.Append(GenerateFileHeader(table.Name, GeneratorType));

            // Imports
            sb.AppendLine("import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';");
            sb.AppendLine($"import {{ {camelName}Api }} from '../api/{camelName}Api';");
            sb.AppendLine($"import type {{");
            sb.AppendLine($"  {className},");
            sb.AppendLine($"  Create{className}Request,");
            sb.AppendLine($"  Update{className}Request,");
            sb.AppendLine($"  {className}Filters,");
            sb.AppendLine($"}} from '../types/{className}.types';");
            sb.AppendLine();

            // Query hooks
            sb.AppendLine(GenerateUseEntityHook(className, camelName));
            sb.AppendLine(GenerateUseEntitiesHook(className, camelName));

            // Mutation hooks
            sb.AppendLine(GenerateUseCreateHook(className, camelName));
            sb.AppendLine(GenerateUseUpdateHook(className, camelName));
            sb.AppendLine(GenerateUseDeleteHook(className, camelName));

            return sb.ToString();
        }

        private string GenerateUseEntityHook(string className, string camelName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("/**");
            sb.AppendLine($" * Hook to fetch single {className} by ID.");
            sb.AppendLine(" */");
            sb.AppendLine($"export const use{className} = (id: number | null) => {{");
            sb.AppendLine("  return useQuery({");
            sb.AppendLine($"    queryKey: ['{camelName}', id],");
            sb.AppendLine($"    queryFn: () => {camelName}Api.getById(id!),");
            sb.AppendLine("    enabled: id !== null,");
            sb.AppendLine("  });");
            sb.AppendLine("};");
            sb.AppendLine();
            return sb.ToString();
        }

        private string GenerateUseEntitiesHook(string className, string camelName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("/**");
            sb.AppendLine($" * Hook to fetch all {className}s with optional filters.");
            sb.AppendLine(" */");
            sb.AppendLine($"export const use{className}s = (filters?: {className}Filters) => {{");
            sb.AppendLine("  return useQuery({");
            sb.AppendLine($"    queryKey: ['{camelName}s', filters],");
            sb.AppendLine($"    queryFn: () => {camelName}Api.getAll(filters),");
            sb.AppendLine("  });");
            sb.AppendLine("};");
            sb.AppendLine();
            return sb.ToString();
        }

        private string GenerateUseCreateHook(string className, string camelName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("/**");
            sb.AppendLine($" * Hook to create {className}.");
            sb.AppendLine(" */");
            sb.AppendLine($"export const useCreate{className} = () => {{");
            sb.AppendLine("  const queryClient = useQueryClient();");
            sb.AppendLine();
            sb.AppendLine("  return useMutation({");
            sb.AppendLine($"    mutationFn: (data: Create{className}Request) => {camelName}Api.create(data),");
            sb.AppendLine("    onSuccess: () => {");
            sb.AppendLine($"      queryClient.invalidateQueries({{ queryKey: ['{camelName}s'] }});");
            sb.AppendLine("    },");
            sb.AppendLine("  });");
            sb.AppendLine("};");
            sb.AppendLine();
            return sb.ToString();
        }

        private string GenerateUseUpdateHook(string className, string camelName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("/**");
            sb.AppendLine($" * Hook to update {className}.");
            sb.AppendLine(" */");
            sb.AppendLine($"export const useUpdate{className} = () => {{");
            sb.AppendLine("  const queryClient = useQueryClient();");
            sb.AppendLine();
            sb.AppendLine("  return useMutation({");
            sb.AppendLine($"    mutationFn: ({{ id, data }}: {{ id: number; data: Update{className}Request }}) =>");
            sb.AppendLine($"      {camelName}Api.update(id, data),");
            sb.AppendLine("    onSuccess: (_, variables) => {");
            sb.AppendLine($"      queryClient.invalidateQueries({{ queryKey: ['{camelName}', variables.id] }});");
            sb.AppendLine($"      queryClient.invalidateQueries({{ queryKey: ['{camelName}s'] }});");
            sb.AppendLine("    },");
            sb.AppendLine("  });");
            sb.AppendLine("};");
            sb.AppendLine();
            return sb.ToString();
        }

        private string GenerateUseDeleteHook(string className, string camelName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("/**");
            sb.AppendLine($" * Hook to delete {className}.");
            sb.AppendLine(" */");
            sb.AppendLine($"export const useDelete{className} = () => {{");
            sb.AppendLine("  const queryClient = useQueryClient();");
            sb.AppendLine();
            sb.AppendLine("  return useMutation({");
            sb.AppendLine($"    mutationFn: (id: number) => {camelName}Api.delete(id),");
            sb.AppendLine("    onSuccess: () => {");
            sb.AppendLine($"      queryClient.invalidateQueries({{ queryKey: ['{camelName}s'] }});");
            sb.AppendLine("    },");
            sb.AppendLine("  });");
            sb.AppendLine("};");
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
