// <copyright file="ISqlGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators
{
    using System.Threading.Tasks;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Interface for SQL code generators (Stored Procedures, Scripts, etc.).
    /// </summary>
    /// <remarks>
    /// This interface defines the contract for all SQL generation components in TargCC.
    /// Each implementation can generate different types of SQL objects (SPs, Views, Functions, etc.).
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a SQL generator for stored procedures
    /// ISqlGenerator generator = new StoredProcedureGenerator();
    ///
    /// // Generate SQL for a table
    /// Table customerTable = analyzer.GetTable("Customer");
    /// string sql = await generator.GenerateAsync(customerTable);
    ///
    /// // Output: CREATE PROCEDURE SP_GetCustomerByID ...
    /// </code>
    /// </example>
    public interface ISqlGenerator
    {
        /// <summary>
        /// Gets the name of this generator.
        /// </summary>
        /// <value>A descriptive name for the generator (e.g., "Stored Procedure Generator").</value>
        string Name { get; }

        /// <summary>
        /// Gets the type of SQL object this generator creates.
        /// </summary>
        /// <value>The SQL object type (e.g., "StoredProcedure", "View", "Function").</value>
        string SqlObjectType { get; }

        /// <summary>
        /// Generate SQL code for a table.
        /// </summary>
        /// <param name="table">The table to generate SQL for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the generated SQL code.</returns>
        /// <exception cref="ArgumentNullException">Thrown when table is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the table structure is invalid for SQL generation.</exception>
        Task<string> GenerateAsync(Table table);

        /// <summary>
        /// Generate SQL code for an entire database schema.
        /// </summary>
        /// <param name="schema">The database schema to generate SQL for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the generated SQL code for all tables.</returns>
        /// <exception cref="ArgumentNullException">Thrown when schema is null.</exception>
        Task<string> GenerateAsync(DatabaseSchema schema);

        /// <summary>
        /// Validates whether a table is suitable for SQL generation.
        /// </summary>
        /// <param name="table">The table to validate.</param>
        /// <returns>True if the table can be processed by this generator; otherwise, false.</returns>
        bool CanGenerate(Table table);
    }
}
