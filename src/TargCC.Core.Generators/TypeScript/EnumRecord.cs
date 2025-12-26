// <copyright file="EnumRecord.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.TypeScript
{
    /// <summary>
    /// Represents an enum value from c_Enumeration table.
    /// </summary>
    public class EnumRecord
    {
        /// <summary>
        /// Gets or sets the enum type name (e.g., "EntityType", "Status").
        /// </summary>
        public string EnumType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the enum value name (e.g., "Active", "Pending").
        /// </summary>
        public string EnumValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display text for the enum value.
        /// </summary>
        public string? EnumText { get; set; }

        /// <summary>
        /// Gets or sets the normalized display text (without special characters).
        /// </summary>
        public string? EnumTextNS { get; set; }

        /// <summary>
        /// Gets or sets the order number for sorting.
        /// </summary>
        public int OrderNum { get; set; }
    }
}
