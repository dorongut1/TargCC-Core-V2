// <copyright file="IUIGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Types of UI generators.
    /// </summary>
    public enum UIGeneratorType
    {
        /// <summary>
        /// TypeScript types and interfaces.
        /// </summary>
        TypeScriptTypes,

        /// <summary>
        /// React API client.
        /// </summary>
        ReactApi,

        /// <summary>
        /// React Query hooks.
        /// </summary>
        ReactHooks,

        /// <summary>
        /// React entity form component.
        /// </summary>
        ReactEntityForm,

        /// <summary>
        /// React collection grid component.
        /// </summary>
        ReactCollectionGrid,

        /// <summary>
        /// React page component.
        /// </summary>
        ReactPage,
    }

    /// <summary>
    /// Base interface for all UI generators.
    /// Generates TypeScript/React code from database schema.
    /// </summary>
    public interface IUIGenerator
    {
        /// <summary>
        /// Gets the type of output this generator produces.
        /// </summary>
        UIGeneratorType GeneratorType { get; }

        /// <summary>
        /// Generates UI code for a single table.
        /// </summary>
        /// <param name="table">The table to generate code for.</param>
        /// <param name="schema">The complete database schema for context.</param>
        /// <param name="config">Generator configuration.</param>
        /// <returns>Generated code as string.</returns>
        Task<string> GenerateAsync(Table table, DatabaseSchema schema, UIGeneratorConfig config);

        /// <summary>
        /// Generates UI code for all tables in the schema.
        /// </summary>
        /// <param name="schema">The database schema.</param>
        /// <param name="config">Generator configuration.</param>
        /// <returns>Dictionary of table name to generated code.</returns>
        Task<Dictionary<string, string>> GenerateAllAsync(DatabaseSchema schema, UIGeneratorConfig config);
    }
}
