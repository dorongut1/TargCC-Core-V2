// <copyright file="UIGeneratorConfig.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using TargCC.Core.Generators.UI.Components;

    /// <summary>
    /// Configuration for UI generators.
    /// </summary>
    public class UIGeneratorConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UIGeneratorConfig"/> class.
        /// </summary>
        public UIGeneratorConfig()
        {
            OutputDirectory = "./generated";
            TypeScriptNamespace = "generated";
            ApiBaseUrl = "http://localhost:5000";
            Framework = UIFramework.MaterialUI;
            UseReactQuery = true;
            UseMaterialUI = true;
            UseFormik = true;
            UseYupValidation = true;
            GenerateComments = true;
            GenerateJsDoc = true;
            IndentSize = 2;
        }

        /// <summary>
        /// Gets or sets the output directory for generated files.
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the API base URL.
        /// </summary>
        /// <remarks>
        /// This property is intentionally a string (not System.Uri) because it's used
        /// to generate JavaScript/TypeScript code where it will be consumed as a string.
        /// </remarks>
        [SuppressMessage("Design", "CA1056:URI properties should not be strings", Justification = "Used for code generation - output needs to be a string for JavaScript/TypeScript")]
        public string ApiBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the UI framework to use.
        /// </summary>
        public UIFramework Framework { get; set; }

        /// <summary>
        /// Gets or sets the TypeScript module namespace.
        /// </summary>
        public string TypeScriptNamespace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use React Query for data fetching.
        /// </summary>
        public bool UseReactQuery { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use Material-UI components.
        /// </summary>
        public bool UseMaterialUI { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use Formik for forms.
        /// </summary>
        public bool UseFormik { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use Yup for validation.
        /// </summary>
        public bool UseYupValidation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate code comments.
        /// </summary>
        public bool GenerateComments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate JSDoc comments.
        /// </summary>
        public bool GenerateJsDoc { get; set; }

        /// <summary>
        /// Gets or sets the indent size (spaces).
        /// </summary>
        public int IndentSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite existing .prt.tsx files.
        /// Default is false (never overwrite partial files).
        /// </summary>
        public bool OverwritePartialFiles { get; set; }

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when configuration is invalid.</exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(OutputDirectory))
            {
                throw new ArgumentException("OutputDirectory cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(TypeScriptNamespace))
            {
                throw new ArgumentException("TypeScriptNamespace cannot be null or empty.");
            }

            if (IndentSize < 0 || IndentSize > 8)
            {
                throw new ArgumentException("IndentSize must be between 0 and 8.");
            }
        }
    }
}
