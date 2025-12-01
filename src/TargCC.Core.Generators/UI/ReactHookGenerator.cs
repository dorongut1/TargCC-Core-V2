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

            return await Task.Run(() => Generate(table)).ConfigureAwait(false);
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

        private string Generate(Table table)
        {
            var sb = new StringBuilder();
            var className = GetClassName(table.Name);
            var camelName = GetCamelCaseName(table.Name);

            // File header
            sb.Append(GenerateFileHeader(table.Name, GeneratorType));

            // Imports
            sb.AppendLine("import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';");
            sb.AppendLine(CultureInfo.InvariantCulture, $"import {{ {camelName}Api }} from '../api/{camelName}Api';");
            sb.AppendLine(CultureInfo.InvariantCulture, $"import type {{");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  {className},");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  Create{className}Request,");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  Update{className}Request,");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  {className}Filters,");
            sb.AppendLine(CultureInfo.InvariantCulture, $"}} from '../types/{className}.types';");
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
    }
}
