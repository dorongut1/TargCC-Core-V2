// <copyright file="ApiOrchestratorGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.API
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Generators.Common;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Orchestrates all API generators to produce a complete API layer.
    /// </summary>
    public class ApiOrchestratorGenerator
    {
        private static readonly Action<ILogger, int, Exception?> LogStartingGeneration =
            LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(1, nameof(LogStartingGeneration)),
                "Starting API generation for {TableCount} tables");

        private static readonly Action<ILogger, int, Exception?> LogCompletedGeneration =
            LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(2, nameof(LogCompletedGeneration)),
                "Completed API generation for {TableCount} tables");

        private readonly DtoGenerator _dtoGenerator;
        private readonly ApiControllerGenerator _controllerGenerator;
        private readonly MappingProfileGenerator _mappingGenerator;
        private readonly ILogger<ApiOrchestratorGenerator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiOrchestratorGenerator"/> class.
        /// </summary>
        /// <param name="dtoGenerator">DTO generator.</param>
        /// <param name="controllerGenerator">Controller generator.</param>
        /// <param name="mappingGenerator">Mapping profile generator.</param>
        /// <param name="logger">Logger instance.</param>
        public ApiOrchestratorGenerator(
            DtoGenerator dtoGenerator,
            ApiControllerGenerator controllerGenerator,
            MappingProfileGenerator mappingGenerator,
            ILogger<ApiOrchestratorGenerator> logger)
        {
            _dtoGenerator = dtoGenerator ?? throw new ArgumentNullException(nameof(dtoGenerator));
            _controllerGenerator = controllerGenerator ?? throw new ArgumentNullException(nameof(controllerGenerator));
            _mappingGenerator = mappingGenerator ?? throw new ArgumentNullException(nameof(mappingGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates all API components for the entire schema.
        /// </summary>
        /// <param name="schema">The database schema.</param>
        /// <param name="config">Generator configuration.</param>
        /// <returns>Dictionary of file paths to generated content.</returns>
        public async Task<Dictionary<string, string>> GenerateAllAsync(DatabaseSchema schema, ApiGeneratorConfig config)
        {
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(config);

            config.Validate();

            LogStartingGeneration(_logger, schema.Tables.Count, null);

            var results = new Dictionary<string, string>();

            foreach (var table in schema.Tables)
            {
                var className = BaseApiGenerator.GetClassName(table.Name);

                // Generate DTOs
                var dtoCode = await _dtoGenerator.GenerateAsync(table, schema, config).ConfigureAwait(false);
                var dtoPath = $"{config.DtosOutputDirectory}/{className}/{className}Dto.cs";
                results[dtoPath] = dtoCode;

                // Generate Controller
                var controllerCode = await _controllerGenerator.GenerateAsync(table, schema, config).ConfigureAwait(false);
                var controllerPath = $"{config.ControllersOutputDirectory}/{CodeGenerationHelpers.MakePlural(className)}Controller.cs";
                results[controllerPath] = controllerCode;

                // Generate Mapping Profile
                var mappingCode = await _mappingGenerator.GenerateAsync(table, schema, config).ConfigureAwait(false);
                var mappingPath = $"{config.MappingOutputDirectory}/{className}MappingProfile.cs";
                results[mappingPath] = mappingCode;
            }

            // Generate ServiceCollectionExtensions
            var extensionsCode = GenerateServiceCollectionExtensions(schema, config);
            var extensionsPath = $"{config.ExtensionsOutputDirectory}/ServiceCollectionExtensions.cs";
            results[extensionsPath] = extensionsCode;

            LogCompletedGeneration(_logger, schema.Tables.Count, null);

            return results;
        }

        private static string GenerateServiceCollectionExtensions(DatabaseSchema schema, ApiGeneratorConfig config)
        {
            var sb = new StringBuilder();

            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("//     This code was generated by TargCC API Generator.");
            sb.AppendLine("//     Generator: Service Collection Extensions");
            sb.AppendLine(CultureInfo.InvariantCulture, $"//     Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("//     Changes to this file may cause incorrect behavior and will be lost if");
            sb.AppendLine("//     the code is regenerated.");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();

            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine();

            sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {config.Namespace}.Extensions");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Extension methods for registering API services.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static class ServiceCollectionExtensions");
            sb.AppendLine("    {");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Registers all API services including controllers and AutoMapper profiles.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"services\">The service collection.</param>");
            sb.AppendLine("        /// <returns>The service collection for chaining.</returns>");
            sb.AppendLine("        public static IServiceCollection AddApiServices(this IServiceCollection services)");
            sb.AppendLine("        {");
            sb.AppendLine("            // Register Controllers");
            sb.AppendLine("            services.AddControllers();");
            sb.AppendLine();
            sb.AppendLine("            // Register AutoMapper with all profiles");
            sb.AppendLine("            services.AddAutoMapper(config =>");
            sb.AppendLine("            {");

            foreach (var table in schema.Tables)
            {
                var className = BaseApiGenerator.GetClassName(table.Name);
                sb.AppendLine(CultureInfo.InvariantCulture, $"                config.AddProfile<{config.Namespace}.Mapping.{className}MappingProfile>();");
            }

            sb.AppendLine("            });");
            sb.AppendLine();
            sb.AppendLine("            // TODO: Register repositories");
            sb.AppendLine("            // services.AddScoped(typeof(IRepository<>), typeof(Repository<>));");
            sb.AppendLine();
            sb.AppendLine("            return services;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
