using System;
using System.Collections.Generic;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Common
{
    /// <summary>
    /// Centralized detector for TargCC column prefixes.
    /// Determines prefix from column name or from extended property "ccType".
    /// </summary>
    public static class ColumnPrefixDetector
    {
        private static readonly string[] KnownPrefixes = {
            "eno_", "ent_", "lkp_", "enm_", "loc_", "clc_", "blg_", "agg_",
            "spt_", "upl_", "scb_", "spl_", "fui_"
        };

        /// <summary>
        /// Determine prefix from column name or from extended properties "ccType".
        /// If extendedProperties contains "ccType", the first token is mapped (comma separated).
        /// Otherwise the column name is scanned for known prefixes (case-insensitive).
        /// </summary>
        public static ColumnPrefix DeterminePrefix(string? columnName, IDictionary<string, string>? extendedProperties = null)
        {
            // Check extended properties first (explicit override)
            if (extendedProperties != null && extendedProperties.TryGetValue("ccType", out var ccType) && !string.IsNullOrWhiteSpace(ccType))
            {
                // Use first token as canonical override (matches previous behavior)
                var token = ccType.Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                return MapCcTypeToken(token);
            }

            if (string.IsNullOrWhiteSpace(columnName))
            {
                return ColumnPrefix.None;
            }

            foreach (var p in KnownPrefixes)
            {
                if (columnName.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                {
                    return MapPrefixString(p);
                }
            }

            // Some callers previously checked prefixes without underscore (historic)
            var lower = columnName.ToLowerInvariant();
            if (lower.StartsWith("eno", StringComparison.OrdinalIgnoreCase)) return ColumnPrefix.OneWayEncryption;
            if (lower.StartsWith("ent", StringComparison.OrdinalIgnoreCase)) return ColumnPrefix.TwoWayEncryption;
            if (lower.StartsWith("lkp", StringComparison.OrdinalIgnoreCase)) return ColumnPrefix.Lookup;
            if (lower.StartsWith("enm", StringComparison.OrdinalIgnoreCase)) return ColumnPrefix.Enumeration;

            return ColumnPrefix.None;
        }

        private static ColumnPrefix MapCcTypeToken(string token) =>
            token.Trim().ToLowerInvariant() switch
            {
                "eno" => ColumnPrefix.OneWayEncryption,
                "ent" => ColumnPrefix.TwoWayEncryption,
                "lkp" => ColumnPrefix.Lookup,
                "enm" or "enm_" => ColumnPrefix.Enumeration,
                "loc" => ColumnPrefix.Localized,
                "clc" => ColumnPrefix.Calculated,
                "blg" => ColumnPrefix.BusinessLogic,
                "agg" => ColumnPrefix.Aggregate,
                "spt" => ColumnPrefix.SeparateUpdate,
                "upl" => ColumnPrefix.Upload,
                "fui" => ColumnPrefix.FakeUniqueIndex,
                "scb" => ColumnPrefix.SeparateChangedBy,
                "spl" => ColumnPrefix.SeparateList,
                _ => ColumnPrefix.None
            };

        private static ColumnPrefix MapPrefixString(string prefix) =>
            prefix.TrimEnd('_').ToLowerInvariant() switch
            {
                "eno" => ColumnPrefix.OneWayEncryption,
                "ent" => ColumnPrefix.TwoWayEncryption,
                "lkp" => ColumnPrefix.Lookup,
                "enm" => ColumnPrefix.Enumeration,
                "loc" => ColumnPrefix.Localized,
                "clc" => ColumnPrefix.Calculated,
                "blg" => ColumnPrefix.BusinessLogic,
                "agg" => ColumnPrefix.Aggregate,
                "spt" => ColumnPrefix.SeparateUpdate,
                "upl" => ColumnPrefix.Upload,
                "fui" => ColumnPrefix.FakeUniqueIndex,
                "scb" => ColumnPrefix.SeparateChangedBy,
                "spl" => ColumnPrefix.SeparateList,
                _ => ColumnPrefix.None
            };
    }
}
