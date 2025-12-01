// <copyright file="ComponentType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI.Components
{
    /// <summary>
    /// Types of React components that can be generated.
    /// </summary>
    public enum ComponentType
    {
        /// <summary>
        /// List component (table view with pagination, sorting, filtering).
        /// </summary>
        List,

        /// <summary>
        /// Form component (create/update form with validation).
        /// </summary>
        Form,

        /// <summary>
        /// Detail component (read-only view).
        /// </summary>
        Detail,
    }
}
