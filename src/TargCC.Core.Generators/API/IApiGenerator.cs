// <copyright file="IApiGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.API
{
    using System.Threading.Tasks;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Interface for API code generators (Controllers, DTOs, Mapping profiles).
    /// </summary>
    public interface IApiGenerator
    {
        /// <summary>
        /// Generates code for a single table.
        /// </summary>
        /// <param name="table">The table to generate code for.</param>
        /// <param name="schema">The database schema.</param>
        /// <param name="config">Generator configuration.</param>
        /// <returns>Generated code as a string.</returns>
        Task<string> GenerateAsync(Table table, DatabaseSchema schema, ApiGeneratorConfig config);
    }
}
