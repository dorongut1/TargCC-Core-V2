// <copyright file="ApiGeneratorConfig.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.API
{
    using System;

    /// <summary>
    /// Configuration for API code generation.
    /// </summary>
    public class ApiGeneratorConfig
    {
        /// <summary>
        /// Gets or sets the root namespace for generated code.
        /// Default: "YourApp".
        /// </summary>
        public string Namespace { get; set; } = "YourApp";

        /// <summary>
        /// Gets or sets the output directory for Controllers.
        /// Default: "Controllers".
        /// </summary>
        public string ControllersOutputDirectory { get; set; } = "Controllers";

        /// <summary>
        /// Gets or sets the output directory for DTOs.
        /// Default: "DTOs".
        /// </summary>
        public string DtosOutputDirectory { get; set; } = "DTOs";

        /// <summary>
        /// Gets or sets the output directory for Mapping profiles.
        /// Default: "Mapping".
        /// </summary>
        public string MappingOutputDirectory { get; set; } = "Mapping";

        /// <summary>
        /// Gets or sets the output directory for Extensions.
        /// Default: "Extensions".
        /// </summary>
        public string ExtensionsOutputDirectory { get; set; } = "Extensions";

        /// <summary>
        /// Gets or sets a value indicating whether to generate authorization attributes.
        /// Default: false.
        /// </summary>
        public bool GenerateAuthorizationAttributes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate data validation attributes.
        /// Default: true.
        /// </summary>
        public bool GenerateValidationAttributes { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to generate Swagger/OpenAPI attributes.
        /// Default: true.
        /// </summary>
        public bool GenerateSwaggerAttributes { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to generate XML documentation comments.
        /// Default: true.
        /// </summary>
        public bool GenerateXmlDocumentation { get; set; } = true;

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when configuration is invalid.</exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Namespace))
            {
                throw new InvalidOperationException("Namespace cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(ControllersOutputDirectory))
            {
                throw new InvalidOperationException("ControllersOutputDirectory cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(DtosOutputDirectory))
            {
                throw new InvalidOperationException("DtosOutputDirectory cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(MappingOutputDirectory))
            {
                throw new InvalidOperationException("MappingOutputDirectory cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(ExtensionsOutputDirectory))
            {
                throw new InvalidOperationException("ExtensionsOutputDirectory cannot be null or empty.");
            }
        }
    }
}
