// <copyright file="IEntityGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.Entities
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Interface for generating entity classes from database schema.
    /// </summary>
    public interface IEntityGenerator
    {
        /// <summary>
        /// Generates a C# entity class from a table schema.
        /// </summary>
        /// <param name="table">The table to generate entity for.</param>
        /// <param name="schema">The database schema (for relationships).</param>
        /// <param name="namespace">The namespace for the entity.</param>
        /// <returns>Generated C# class code.</returns>
        Task<string> GenerateAsync(Table table, DatabaseSchema schema, string @namespace = "YourNamespace.Entities");

        /// <summary>
        /// Generates entities for all tables in a database schema.
        /// </summary>
        /// <param name="schema">The database schema.</param>
        /// <param name="namespace">The namespace for entities.</param>
        /// <returns>Dictionary of ClassName → Code.</returns>
        Task<Dictionary<string, string>> GenerateAllAsync(DatabaseSchema schema, string @namespace = "YourNamespace.Entities");
    }
}
