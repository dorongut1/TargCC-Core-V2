using System.Globalization;
using System.Text;
using TargCC.Core.Generators.API;
using TargCC.Core.Generators.Common;

namespace TargCC.Core.Generators.React;

/// <summary>
/// Generates React hooks for ComboList dropdown lookups.
/// </summary>
public static class ComboListHooksGenerator
{
    /// <summary>
    /// Generates hooks file for all combo lists.
    /// </summary>
    /// <param name="comboListTables">List of tables that have combo views.</param>
    /// <returns>The generated hooks code.</returns>
    public static string GenerateAllHooks(IEnumerable<string> comboListTables)
    {
        var sb = new StringBuilder();

        // Imports
        sb.AppendLine("import { useQuery } from '@tanstack/react-query';");
        sb.AppendLine("import api from '../api/client';");
        sb.AppendLine();

        // ComboItem interface
        sb.AppendLine("export interface ComboItem {");
        sb.AppendLine("  id: number;");
        sb.AppendLine("  text: string;");
        sb.AppendLine("  textNS: string;");
        sb.AppendLine("}");
        sb.AppendLine();

        // Generate hook for each table
        foreach (var tableName in comboListTables)
        {
            var className = BaseApiGenerator.GetClassName(tableName);
            var hookName = $"use{className}Combo";
            var camelCase = CodeGenerationHelpers.ToCamelCase(className);

            sb.AppendLine("/// <summary>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"/// Hook to fetch {tableName} combo list.");
            sb.AppendLine("/// </summary>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"export function {hookName}() {{");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  return useQuery({{");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    queryKey: ['combo', '{camelCase}'],");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    queryFn: () => api.get<ComboItem[]>('/api/ComboList/{tableName}').then(res => res.data)");
            sb.AppendLine("  });");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
