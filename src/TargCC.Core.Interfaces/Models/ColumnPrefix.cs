// <copyright file="ColumnPrefix.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Interfaces.Models
{
    /// <summary>
    /// Defines the TargCC column prefix types that determine special behavior in generated code.
    /// </summary>
    public enum ColumnPrefix
    {
        /// <summary>
        /// No special prefix - regular column.
        /// </summary>
        None = 0,

        /// <summary>
        /// eno_ - One-way encryption (hashed, e.g., passwords).
        /// </summary>
        OneWayEncryption = 1,

        /// <summary>
        /// ent_ - Two-way encryption (encrypted, e.g., credit cards).
        /// </summary>
        TwoWayEncryption = 2,

        /// <summary>
        /// lkp_ - Lookup value from reference table.
        /// </summary>
        Lookup = 3,

        /// <summary>
        /// enm_ - Enum value.
        /// </summary>
        Enum = 4,

        /// <summary>
        /// loc_ - Localized text.
        /// </summary>
        Localized = 5,

        /// <summary>
        /// clc_ - Calculated field (read-only).
        /// </summary>
        Calculated = 6,

        /// <summary>
        /// blg_ - Business logic field (read-only on client).
        /// </summary>
        BusinessLogic = 7,

        /// <summary>
        /// agg_ - Aggregate field (read-only on client).
        /// </summary>
        Aggregate = 8,

        /// <summary>
        /// spt_ - Separate update field.
        /// </summary>
        SeparateUpdate = 9,

        /// <summary>
        /// FUI_ - Fake unique index (ignored).
        /// </summary>
        FakeUniqueIndex = 10,

        /// <summary>
        /// upl_ - File upload path.
        /// </summary>
        Upload = 11,

        /// <summary>
        /// scb_ - Separate changed by tracking.
        /// </summary>
        SeparateChangedBy = 12,
    }
}
