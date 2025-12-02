// <copyright file="ComponentGeneratorConfig.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI.Components
{
    /// <summary>
    /// UI framework options.
    /// </summary>
    public enum UIFramework
    {
        /// <summary>
        /// Material-UI (MUI) framework.
        /// </summary>
        MaterialUI,

        /// <summary>
        /// Tailwind CSS framework.
        /// </summary>
        TailwindCSS,
    }

    /// <summary>
    /// Form validation library options.
    /// </summary>
    public enum FormValidationLibrary
    {
        /// <summary>
        /// React Hook Form library.
        /// </summary>
        ReactHookForm,

        /// <summary>
        /// Formik library.
        /// </summary>
        Formik,
    }

    /// <summary>
    /// Configuration for React component generation.
    /// </summary>
    public class ComponentGeneratorConfig
    {
        /// <summary>
        /// Gets or sets the UI framework to use.
        /// </summary>
        public UIFramework Framework { get; set; } = UIFramework.MaterialUI;

        /// <summary>
        /// Gets or sets the form validation library.
        /// </summary>
        public FormValidationLibrary ValidationLibrary { get; set; } = FormValidationLibrary.ReactHookForm;

        /// <summary>
        /// Gets or sets a value indicating whether to generate inline styles.
        /// </summary>
        public bool UseInlineStyles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include accessibility attributes.
        /// </summary>
        public bool IncludeAccessibility { get; set; } = true;

        /// <summary>
        /// Gets or sets the output directory for generated components.
        /// </summary>
        public string OutputDirectory { get; set; } = "./components";

        /// <summary>
        /// Gets or sets a value indicating whether to generate TypeScript (vs JavaScript).
        /// </summary>
        public bool UseTypeScript { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to use React Router for navigation.
        /// </summary>
        public bool UseReactRouter { get; set; } = true;
    }
}
