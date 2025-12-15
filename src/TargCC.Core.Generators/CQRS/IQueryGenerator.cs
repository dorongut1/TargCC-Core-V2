// <copyright file="IQueryGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.CQRS;

using TargCC.Core.Interfaces.Models;

/// <summary>
/// Defines the types of queries that can be generated.
/// </summary>
public enum QueryType
{
    /// <summary>
    /// Query to get a single entity by its primary key.
    /// Generates: GetCustomerQuery, GetCustomerHandler, GetCustomerValidator.
    /// </summary>
    GetById,

    /// <summary>
    /// Query to get all entities with pagination support.
    /// Generates: GetCustomersQuery, GetCustomersHandler, GetCustomersValidator.
    /// </summary>
    GetAll,

    /// <summary>
    /// Query to get entities by a specific index (unique or non-unique).
    /// Generates: GetCustomersByEmailQuery, GetCustomersByEmailHandler, etc.
    /// </summary>
    GetByIndex,
}

/// <summary>
/// Result container for generated CQRS query components.
/// </summary>
/// <remarks>
/// Contains all generated code files for a single query operation,
/// including the query record, handler, validator, and DTO.
/// </remarks>
public class QueryGenerationResult
{
    /// <summary>
    /// Gets or sets the generated Query record code.
    /// </summary>
    /// <example>
    /// <code>
    /// public record GetCustomerQuery(int Id) : IRequest&lt;Result&lt;CustomerDto&gt;&gt;;
    /// </code>
    /// </example>
    public string QueryCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generated Handler class code.
    /// </summary>
    /// <example>
    /// <code>
    /// public class GetCustomerHandler : IRequestHandler&lt;GetCustomerQuery, Result&lt;CustomerDto&gt;&gt;
    /// {
    ///     // Implementation
    /// }
    /// </code>
    /// </example>
    public string HandlerCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generated Validator class code.
    /// </summary>
    /// <example>
    /// <code>
    /// public class GetCustomerValidator : AbstractValidator&lt;GetCustomerQuery&gt;
    /// {
    ///     // Validation rules
    /// }
    /// </code>
    /// </example>
    public string ValidatorCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generated DTO class code.
    /// </summary>
    /// <example>
    /// <code>
    /// public class CustomerDto
    /// {
    ///     public int ID { get; init; }
    ///     public string Name { get; init; } = string.Empty;
    /// }
    /// </code>
    /// </example>
    public string DtoCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the query class name (e.g., "GetCustomerQuery").
    /// </summary>
    public string QueryClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the handler class name (e.g., "GetCustomerHandler").
    /// </summary>
    public string HandlerClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validator class name (e.g., "GetCustomerValidator").
    /// </summary>
    public string ValidatorClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the DTO class name (e.g., "CustomerDto").
    /// </summary>
    public string DtoClassName { get; set; } = string.Empty;
}

/// <summary>
/// Interface for generating CQRS Query components.
/// </summary>
/// <remarks>
/// <para>
/// The Query Generator creates complete CQRS query implementations following
/// the MediatR pattern. For each query type, it generates:
/// </para>
/// <list type="bullet">
/// <item><description>Query record - The request object implementing IRequest</description></item>
/// <item><description>Handler class - The IRequestHandler implementation</description></item>
/// <item><description>Validator class - FluentValidation rules</description></item>
/// <item><description>DTO class - Data Transfer Object for the response</description></item>
/// </list>
/// <para>
/// <strong>Supported Query Types:</strong>
/// </para>
/// <list type="number">
/// <item>
/// <term>GetById</term>
/// <description>Retrieves a single entity by primary key</description>
/// </item>
/// <item>
/// <term>GetAll</term>
/// <description>Retrieves all entities with pagination</description>
/// </item>
/// <item>
/// <term>GetByIndex</term>
/// <description>Retrieves entities by index columns</description>
/// </item>
/// </list>
/// <para>
/// <strong>Example Usage:</strong>
/// </para>
/// <code>
/// var generator = new QueryGenerator(logger);
/// var result = await generator.GenerateAsync(customerTable, QueryType.GetById);
///
/// // result.QueryCode contains:
/// // public record GetCustomerQuery(int Id) : IRequest&lt;Result&lt;CustomerDto&gt;&gt;;
///
/// // result.HandlerCode contains:
/// // public class GetCustomerHandler : IRequestHandler&lt;GetCustomerQuery, Result&lt;CustomerDto&gt;&gt;
/// // { ... }
/// </code>
/// </remarks>
public interface IQueryGenerator
{
    /// <summary>
    /// Generates a complete CQRS query implementation for the specified table and query type.
    /// </summary>
    /// <param name="table">The table metadata containing structure and constraints.</param>
    /// <param name="queryType">The type of query to generate (GetById, GetAll, GetByIndex).</param>
    /// <param name="rootNamespace">The root namespace for the generated code (default: "TargCC").</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a
    /// <see cref="QueryGenerationResult"/> with all generated code components.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="table"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the table has no primary key (required for GetById queries).
    /// </exception>
    /// <example>
    /// <code>
    /// var table = new TableBuilder()
    ///     .WithName("Customer")
    ///     .WithColumn("ID", "int", isPrimaryKey: true)
    ///     .WithColumn("Name", "nvarchar", maxLength: 100)
    ///     .Build();
    ///
    /// var result = await generator.GenerateAsync(table, QueryType.GetById, "MyApp");
    ///
    /// await File.WriteAllTextAsync("GetCustomerQuery.cs", result.QueryCode);
    /// await File.WriteAllTextAsync("GetCustomerHandler.cs", result.HandlerCode);
    /// </code>
    /// </example>
    Task<QueryGenerationResult> GenerateAsync(Table table, QueryType queryType, string rootNamespace = "TargCC");

    /// <summary>
    /// Generates a GetByIndex query for a specific index on the table.
    /// </summary>
    /// <param name="table">The table metadata containing structure and constraints.</param>
    /// <param name="index">The index to generate the query for.</param>
    /// <param name="rootNamespace">The root namespace for the generated code (default: "TargCC").</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a
    /// <see cref="QueryGenerationResult"/> with all generated code components.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="table"/> or <paramref name="index"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// var emailIndex = table.Indexes.First(i => i.Name == "IX_Customer_Email");
    /// var result = await generator.GenerateByIndexAsync(table, emailIndex, "MyApp");
    ///
    /// // Generates GetCustomerByEmailQuery if index is unique
    /// // Generates GetCustomersByEmailQuery if index is non-unique
    /// </code>
    /// </example>
    Task<QueryGenerationResult> GenerateByIndexAsync(Table table, Index index, string rootNamespace = "TargCC");

    /// <summary>
    /// Generates only the DTO class for a table.
    /// </summary>
    /// <param name="table">The table metadata containing structure and constraints.</param>
    /// <param name="rootNamespace">The root namespace for the generated code (default: "TargCC").</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// the generated DTO class code as a string.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="table"/> is null.
    /// </exception>
    /// <remarks>
    /// The DTO excludes sensitive columns (eno_, ent_ prefixes) and includes
    /// only properties safe for external exposure.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dtoCode = await generator.GenerateDtoAsync(customerTable, "MyApp");
    /// // Generates CustomerDto without Password or CreditCard fields
    /// </code>
    /// </example>
    Task<string> GenerateDtoAsync(Table table, string rootNamespace = "TargCC");

    /// <summary>
    /// Generates all standard queries for a table (GetById, GetAll).
    /// </summary>
    /// <param name="table">The table metadata containing structure and constraints.</param>
    /// <param name="rootNamespace">The root namespace for the generated code (default: "TargCC").</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// a collection of <see cref="QueryGenerationResult"/> for all generated queries.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="table"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// var results = await generator.GenerateAllAsync(customerTable, "MyApp");
    /// // Returns results for GetCustomerQuery and GetCustomersQuery
    /// </code>
    /// </example>
    Task<IEnumerable<QueryGenerationResult>> GenerateAllAsync(Table table, string rootNamespace = "TargCC");
}
