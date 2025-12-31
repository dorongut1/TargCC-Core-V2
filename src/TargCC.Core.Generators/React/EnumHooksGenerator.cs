namespace TargCC.Core.Generators.React
{
    using System;
    using System.Text;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Generates React hooks for working with enums from c_Enumeration.
    /// </summary>
    public static class EnumHooksGenerator
    {
        /// <summary>
        /// Generates useEnumValues hook for loading enums from API.
        /// </summary>
        /// <returns>TypeScript code for the hook.</returns>
        public static string GenerateUseEnumValuesHook()
        {
            var sb = new StringBuilder();

            sb.AppendLine("/**");
            sb.AppendLine(" * Auto-generated enum hooks");
            sb.AppendLine(" * Provides React hooks for loading enum values from c_Enumeration table");
            sb.AppendLine(" */");
            sb.AppendLine();
            sb.AppendLine("import { useQuery } from '@tanstack/react-query';");
            sb.AppendLine("import { api } from '../api/client';");
            sb.AppendLine();
            sb.AppendLine("/**");
            sb.AppendLine(" * Represents an enum value from c_Enumeration table");
            sb.AppendLine(" */");
            sb.AppendLine("export interface EnumValue {");
            sb.AppendLine("  enumType: string;");
            sb.AppendLine("  enumValue: string;");
            sb.AppendLine("  locText: string;");
            sb.AppendLine("  locDescription?: string;");
            sb.AppendLine("  ordinalPosition: number;");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("/**");
            sb.AppendLine(" * Hook to load enum values for a specific enum type");
            sb.AppendLine(" * @param enumType - The enum type to load (e.g., 'PaymentMethod', 'OrderStatus')");
            sb.AppendLine(" * @returns Query result with enum values");
            sb.AppendLine(" */");
            sb.AppendLine("export const useEnumValues = (enumType: string) => {");
            sb.AppendLine("  return useQuery<EnumValue[]>({");
            sb.AppendLine("    queryKey: ['enums', enumType],");
            sb.AppendLine("    queryFn: async () => {");
            sb.AppendLine("      const response = await api.get<EnumValue[]>(`/api/enumerations/${enumType}`);");
            sb.AppendLine("      return response.data;");
            sb.AppendLine("    },");
            sb.AppendLine("    staleTime: 1000 * 60 * 60, // 1 hour - enums don't change often");
            sb.AppendLine("  });");
            sb.AppendLine("};");
            sb.AppendLine();
            sb.AppendLine("/**");
            sb.AppendLine(" * Hook to get the display text (locText) for a specific enum value");
            sb.AppendLine(" * @param enumType - The enum type");
            sb.AppendLine(" * @param enumValue - The enum value");
            sb.AppendLine(" * @returns The locText for the enum value, or the enumValue itself if not found");
            sb.AppendLine(" */");
            sb.AppendLine("export const useEnumLabel = (enumType: string, enumValue: string | undefined) => {");
            sb.AppendLine("  const { data: enumValues } = useEnumValues(enumType);");
            sb.AppendLine();
            sb.AppendLine("  if (!enumValue || !enumValues) {");
            sb.AppendLine("    return enumValue || '';");
            sb.AppendLine("  }");
            sb.AppendLine();
            sb.AppendLine("  const enumItem = enumValues.find((e) => e.enumValue === enumValue);");
            sb.AppendLine("  return enumItem?.locText ?? enumValue;");
            sb.AppendLine("};");
            sb.AppendLine();
            sb.AppendLine("/**");
            sb.AppendLine(" * Hook to load all enum types available in the system");
            sb.AppendLine(" * @returns Query result with list of enum type names");
            sb.AppendLine(" */");
            sb.AppendLine("export const useEnumTypes = () => {");
            sb.AppendLine("  return useQuery<string[]>({");
            sb.AppendLine("    queryKey: ['enum-types'],");
            sb.AppendLine("    queryFn: async () => {");
            sb.AppendLine("      const response = await api.get<string[]>('/api/enumerations/types');");
            sb.AppendLine("      return response.data;");
            sb.AppendLine("    },");
            sb.AppendLine("    staleTime: 1000 * 60 * 60, // 1 hour");
            sb.AppendLine("  });");
            sb.AppendLine("};");

            return sb.ToString();
        }
    }
}
