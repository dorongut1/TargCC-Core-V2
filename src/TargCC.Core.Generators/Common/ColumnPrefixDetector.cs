using System;
using System.Collections.Generic;
using System.Linq;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Generators.Common
{
    /// <summary>
    /// Centralized detector for TargCC column prefixes.
    /// Determines prefix from column name or from extended property "ccType".
    /// </summary>
    public static class ColumnPrefixDetector
    {
        private static readonly string[] KnownPrefixes = new[]
        {
            "eno_", "ent_", "lkp_", "enm_", "loc_", "clc_", "blg_", "agg_",
            "spt_", "upl_", "scb_", "spl_", "fui_"
        };

        /// <summary>
        /// Determine prefix from column name or from extended properties "ccType".
        /// If extendedProperties contains "ccType", the first token is mapped (comma separated).
        /// Otherwise the column name is scanned for known prefixes (case-insensitive).
        /// </summary>
        /// <param name="columnName">Column name to inspect (may be null or empty).</param>
        /// <param name="extendedProperties">Optional extended properties dictionary, may contain "ccType".</param>
        /// <returns>The detected <see cref="ColumnPrefix"/>; <see cref="ColumnPrefix.None"/> if none.</returns>
        public static ColumnPrefix DeterminePrefix(string? columnName, IDictionary<string, string>? extendedProperties = null)
        {
            if (extendedProperties != null && extendedProperties.TryGetValue("ccType", out var ccType) && !string.IsNullOrWhiteSpace(ccType))
            {
                var token = ccType.Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                return MapCcTypeToken(token);
            }

            if (string.IsNullOrWhiteSpace(columnName))
            {
                return ColumnPrefix.None;
            }

            var match = Array.Find(KnownPrefixes, p => columnName.StartsWith(p, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                return MapPrefixString(match);
            }

            // Historic fallback for names without underscore
            var upper = columnName.ToUpperInvariant();

            if (upper.StartsWith("ENO", StringComparison.Ordinal))
            {
                return ColumnPrefix.OneWayEncryption;
            }

            if (upper.StartsWith("ENT", StringComparison.Ordinal))
            {
                return ColumnPrefix.TwoWayEncryption;
            }

            if (upper.StartsWith("LKP", StringComparison.Ordinal))
            {
                return ColumnPrefix.Lookup;
            }

            if (upper.StartsWith("ENM", StringComparison.Ordinal))
            {
                return ColumnPrefix.Enum;
            }

            return ColumnPrefix.None;
        }

        private static ColumnPrefix MapCcTypeToken(string token)
        {
            var key = token.Trim().ToUpperInvariant();

            return key switch
            {
                "ENO" => ColumnPrefix.OneWayEncryption,
                "ENT" => ColumnPrefix.TwoWayEncryption,
                "LKP" => ColumnPrefix.Lookup,
                "ENM" or "ENM_" => ColumnPrefix.Enum,
                "LOC" => ColumnPrefix.Localized,
                "CLC" => ColumnPrefix.Calculated,
                "BLG" => ColumnPrefix.BusinessLogic,
                "AGG" => ColumnPrefix.Aggregate,
                "SPT" => ColumnPrefix.SeparateUpdate,
                "UPL" => ColumnPrefix.Upload,
                "FUI" => ColumnPrefix.FakeUniqueIndex,
                "SCB" => ColumnPrefix.SeparateChangedBy,
                "SPL" => ColumnPrefix.SeparateList,
                _ => ColumnPrefix.None,
            };
        }

        private static ColumnPrefix MapPrefixString(string prefix)
        {
            var key = prefix.TrimEnd('_').ToUpperInvariant();

            return key switch
            {
                "ENO" => ColumnPrefix.OneWayEncryption,
                "ENT" => ColumnPrefix.TwoWayEncryption,
                "LKP" => ColumnPrefix.Lookup,
                "ENM" => ColumnPrefix.Enum,
                "LOC" => ColumnPrefix.Localized,
                "CLC" => ColumnPrefix.Calculated,
                "BLG" => ColumnPrefix.BusinessLogic,
                "AGG" => ColumnPrefix.Aggregate,
                "SPT" => ColumnPrefix.SeparateUpdate,
                "UPL" => ColumnPrefix.Upload,
                "FUI" => ColumnPrefix.FakeUniqueIndex,
                "SCB" => ColumnPrefix.SeparateChangedBy,
                "SPL" => ColumnPrefix.SeparateList,
                _ => ColumnPrefix.None,
            };
        }
    }
}
