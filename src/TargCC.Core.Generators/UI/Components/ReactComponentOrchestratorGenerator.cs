// <copyright file="ReactComponentOrchestratorGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI.Components
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Orchestrates generation of all React components for a table and creates routing configuration.
    /// </summary>
    public class ReactComponentOrchestratorGenerator : BaseUIGenerator
    {
        private static readonly Action<ILogger, string, Exception?> LogOrchestrating =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(301, nameof(LogOrchestrating)),
                "Orchestrating component generation for table {TableName}");

        private readonly ReactListComponentGenerator _listGenerator;
        private readonly ReactFormComponentGenerator _formGenerator;
        private readonly ReactDetailComponentGenerator _detailGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactComponentOrchestratorGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="listGenerator">List component generator.</param>
        /// <param name="formGenerator">Form component generator.</param>
        /// <param name="detailGenerator">Detail component generator.</param>
        public ReactComponentOrchestratorGenerator(
            ILogger<ReactComponentOrchestratorGenerator> logger,
            ReactListComponentGenerator listGenerator,
            ReactFormComponentGenerator formGenerator,
            ReactDetailComponentGenerator detailGenerator)
            : base(logger)
        {
            _listGenerator = listGenerator ?? throw new ArgumentNullException(nameof(listGenerator));
            _formGenerator = formGenerator ?? throw new ArgumentNullException(nameof(formGenerator));
            _detailGenerator = detailGenerator ?? throw new ArgumentNullException(nameof(detailGenerator));
        }

        /// <inheritdoc/>
        public override UIGeneratorType GeneratorType => UIGeneratorType.ReactComponents;

        /// <inheritdoc/>
        public override async Task<string> GenerateAsync(Table table, DatabaseSchema schema, UIGeneratorConfig config)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(config);

            LogOrchestrating(Logger, table.Name, null);

            var componentConfig = new ComponentGeneratorConfig
            {
                OutputDirectory = config.OutputDirectory,
            };

            return await GenerateAllComponentsAsync(table, schema, componentConfig).ConfigureAwait(false);
        }

        /// <summary>
        /// Generates all components for a table and returns a summary.
        /// </summary>
        /// <param name="table">Table metadata.</param>
        /// <param name="schema">Database schema.</param>
        /// <param name="config">Configuration.</param>
        /// <returns>Dictionary of generated files and their contents.</returns>
        public async Task<Dictionary<string, string>> GenerateAllComponentsAsync(
            Table table,
            DatabaseSchema schema,
            ComponentGeneratorConfig config)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(config);

            var result = new Dictionary<string, string>();
            var className = GetClassName(table.Name);

            // Generate individual components
            var listComponent = await _listGenerator.GenerateAsync(table, schema, config).ConfigureAwait(false);
            result[$"{className}List.tsx"] = listComponent;

            var formComponent = await _formGenerator.GenerateAsync(table, schema, config).ConfigureAwait(false);
            result[$"{className}Form.tsx"] = formComponent;

            var detailComponent = await _detailGenerator.GenerateAsync(table, schema, config).ConfigureAwait(false);
            result[$"{className}Detail.tsx"] = detailComponent;

            // Generate barrel export
            var barrelExport = GenerateBarrelExport(className);
            result["index.ts"] = barrelExport;

            // Generate routes
            var routes = GenerateRoutes(table);
            result[$"{className}Routes.tsx"] = routes;

            return result;
        }

        /// <summary>
        /// Generates all components for all tables in a schema.
        /// </summary>
        /// <param name="schema">Database schema.</param>
        /// <param name="config">Configuration.</param>
        /// <returns>Dictionary of table names to their generated files.</returns>
        public async Task<Dictionary<string, Dictionary<string, string>>> GenerateAllTablesAsync(
            DatabaseSchema schema,
            ComponentGeneratorConfig config)
        {
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(config);

            var result = new Dictionary<string, Dictionary<string, string>>();

            foreach (var table in schema.Tables)
            {
                var components = await GenerateAllComponentsAsync(table, schema, config).ConfigureAwait(false);
                result[table.Name] = components;
            }

            // Generate root routing config
            var rootRouting = GenerateRootRoutingConfig(schema);
            result["__routing__"] = new Dictionary<string, string>
            {
                { "routes.config.ts", rootRouting },
            };

            return result;
        }

        private static string GenerateBarrelExport(string className)
        {
            var sb = new StringBuilder();

            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("//     This code was generated by TargCC Component Generator.");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();

            sb.AppendLine(CultureInfo.InvariantCulture, $"export {{ {className}List }} from './{className}List';");
            sb.AppendLine(CultureInfo.InvariantCulture, $"export {{ {className}Form }} from './{className}Form';");
            sb.AppendLine(CultureInfo.InvariantCulture, $"export {{ {className}Detail }} from './{className}Detail';");

            return sb.ToString();
        }

        private static string GenerateRoutes(Table table)
        {
            var sb = new StringBuilder();
            var className = GetClassName(table.Name);

            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("//     This code was generated by TargCC Component Generator.");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();

            sb.AppendLine("import React from 'react';");
            sb.AppendLine("import { Routes, Route } from 'react-router-dom';");
            sb.AppendLine(CultureInfo.InvariantCulture, $"import {{ {className}List, {className}Form, {className}Detail }} from '.';");
            sb.AppendLine();

            sb.AppendLine(CultureInfo.InvariantCulture, $"export const {className}Routes: React.FC = () => {{");
            sb.AppendLine("  return (");
            sb.AppendLine("    <Routes>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"      <Route path=\"/\" element={{<{className}List />}} />");
            sb.AppendLine(CultureInfo.InvariantCulture, $"      <Route path=\"/new\" element={{<{className}Form />}} />");
            sb.AppendLine(CultureInfo.InvariantCulture, $"      <Route path=\"/:id\" element={{<{className}Detail />}} />");
            sb.AppendLine(CultureInfo.InvariantCulture, $"      <Route path=\"/:id/edit\" element={{<{className}Form />}} />");
            sb.AppendLine("    </Routes>");
            sb.AppendLine("  );");
            sb.AppendLine("};");

            return sb.ToString();
        }

        private static string GenerateRootRoutingConfig(DatabaseSchema schema)
        {
            var sb = new StringBuilder();

            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("//     This code was generated by TargCC Component Generator.");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();

            sb.AppendLine("import React from 'react';");
            sb.AppendLine("import { Routes, Route } from 'react-router-dom';");

            var tables = schema.Tables.Take(10).ToList(); // Limit to first 10 for example

            // Import all routes
            foreach (var className in tables.Select(t => GetClassName(t.Name)))
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"import {{ {className}Routes }} from '../components/{className}/{className}Routes';");
            }

            sb.AppendLine();
            sb.AppendLine("export const AppRoutes: React.FC = () => {");
            sb.AppendLine("  return (");
            sb.AppendLine("    <Routes>");

            foreach (var table in tables)
            {
                var className = GetClassName(table.Name);
                var camelName = GetCamelCaseName(table.Name);
                var pluralName = camelName + "s";
                sb.AppendLine(CultureInfo.InvariantCulture, $"      <Route path=\"/{pluralName}/*\" element={{<{className}Routes />}} />");
            }

            sb.AppendLine("    </Routes>");
            sb.AppendLine("  );");
            sb.AppendLine("};");

            return sb.ToString();
        }
    }
}
