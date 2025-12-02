// <copyright file="ComponentFieldType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI.Components
{
    /// <summary>
    /// Describes a component field type.
    /// </summary>
    public class ComponentFieldType
    {
        /// <summary>
        /// Gets or sets the HTML input type (text, password, number, etc.).
        /// </summary>
        public string InputType { get; set; } = "text";

        /// <summary>
        /// Gets or sets the component name (TextField, Select, etc.).
        /// </summary>
        public string ComponentName { get; set; } = "TextField";

        /// <summary>
        /// Gets or sets a value indicating whether this is a multiline field.
        /// </summary>
        public bool Multiline { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field is read-only.
        /// </summary>
        public bool IsReadOnly { get; set; }
    }
}
