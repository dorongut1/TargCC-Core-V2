// <copyright file="IComponentGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI.Components
{
    using System.Threading.Tasks;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Interface for React component generators.
    /// </summary>
    public interface IComponentGenerator
    {
        /// <summary>
        /// Gets the component type this generator produces.
        /// </summary>
        ComponentType ComponentType { get; }

        /// <summary>
        /// Generates a React component for a table.
        /// </summary>
        /// <param name="table">Table metadata.</param>
        /// <param name="schema">Database schema.</param>
        /// <param name="config">Configuration.</param>
        /// <returns>Generated component code.</returns>
        Task<string> GenerateAsync(Table table, DatabaseSchema schema, ComponentGeneratorConfig config);
    }
}
