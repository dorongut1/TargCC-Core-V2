// <copyright file="IDtoGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.CQRS;

using TargCC.Core.Interfaces.Models;

/// <summary>
/// Defines the types of DTOs that can be generated.
/// </summary>
public enum DtoType
{
    /// <summary>
    /// Basic DTO with all safe properties.
    /// Used for general data transfer.
    /// </summary>
    Basic,

    /// <summary>
    /// Minimal DTO for list views.
    /// Contains only essential display fields (ID, Name, key identifiers).
    /// </summary>
    List,

    /// <summary>
    /// Full detail DTO with all properties.
    /// Used for detail views and complete data retrieval.
    /// </summary>
    Detail,

    /// <summary>
    /// Input DTO for entity creation.
    /// Excludes auto-generated fields (ID, timestamps).
    /// </summary>
    Create,

    /// <summary>
    /// Input DTO for entity updates.
    /// Includes ID and updatable fields only.
    /// </summary>
    Update,
}

/// <summary>
/// Result container for generated DTO code.
/// </summary>
public class DtoGenerationResult
{
    /// <summary>
    /// Gets or sets the generated DTO class code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the DTO class name (e.g., "CustomerDto", "CustomerListDto").
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the DTO type that was generated.
    /// </summary>
    public DtoType DtoType { get; set; }

    /// <summary>
    /// Gets or sets the namespace for the generated DTO.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of property names included in this DTO.
    /// </summary>
    public IReadOnlyList<string> Properties { get; set; } = new List<string>();
}

/// <summary>
/// Interface for generating Data Transfer Object (DTO) classes.
/// </summary>
/// <remarks>
/// <para>
/// The DTO Generator creates type-safe data transfer objects optimized for
/// different use cases. It automatically handles:
/// </para>
/// <list type="bullet">
/// <item><description>Sensitive field exclusion (eno_, ent_ prefixes)</description></item>
/// <item><description>Read-only field handling (clc_, agg_ prefixes)</description></item>
/// <item><description>Nullable type generation</description></item>
/// <item><description>Proper C# type mapping</description></item>
/// </list>
/// <para>
/// <strong>DTO Types:</strong>
/// </para>
/// <list type="number">
/// <item>
/// <term>Basic</term>
/// <description>Standard DTO with all safe properties</description>
/// </item>
/// <item>
/// <term>List</term>
/// <description>Minimal DTO for list/grid views</description>
/// </item>
/// <item>
/// <term>Detail</term>
/// <description>Complete DTO with all details</description>
/// </item>
/// <item>
/// <term>Create</term>
/// <description>Input DTO for creating entities</description>
/// </item>
/// <item>
/// <term>Update</term>
/// <description>Input DTO for updating entities</description>
/// </item>
/// </list>
/// <para>
/// <strong>Example Usage:</strong>
/// </para>
/// <code>
/// var generator = new DtoGenerator(logger);
/// var result = await generator.GenerateAsync(customerTable, DtoType.List);
///
/// // result.Code contains:
/// // public class CustomerListDto
/// // {
/// //     public int ID { get; init; }
/// //     public string Name { get; init; } = string.Empty;
/// // }
/// </code>
/// </remarks>
public interface IDtoGenerator
{
    /// <summary>
    /// Generates a DTO class for the specified table and DTO type.
    /// </summary>
    /// <param name="table">The table metadata containing structure and constraints.</param>
    /// <param name="dtoType">The type of DTO to generate.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a
    /// <see cref="DtoGenerationResult"/> with the generated code and metadata.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="table"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// var result = await generator.GenerateAsync(customerTable, DtoType.Basic);
    /// await File.WriteAllTextAsync("CustomerDto.cs", result.Code);
    /// </code>
    /// </example>
    Task<DtoGenerationResult> GenerateAsync(Table table, DtoType dtoType);

    /// <summary>
    /// Generates all standard DTO types for a table.
    /// </summary>
    /// <param name="table">The table metadata containing structure and constraints.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// a collection of <see cref="DtoGenerationResult"/> for all DTO types.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="table"/> is null.
    /// </exception>
    /// <remarks>
    /// Generates Basic, List, Detail, Create, and Update DTOs in a single operation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = await generator.GenerateAllAsync(customerTable);
    /// foreach (var result in results)
    /// {
    ///     await File.WriteAllTextAsync($"{result.ClassName}.cs", result.Code);
    /// }
    /// </code>
    /// </example>
    Task<IEnumerable<DtoGenerationResult>> GenerateAllAsync(Table table);

    /// <summary>
    /// Gets the list of properties that should be included for a specific DTO type.
    /// </summary>
    /// <param name="table">The table metadata containing structure and constraints.</param>
    /// <param name="dtoType">The type of DTO to get properties for.</param>
    /// <returns>A list of column names that should be included in the DTO.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="table"/> is null.
    /// </exception>
    /// <remarks>
    /// This method applies filtering rules based on DTO type:
    /// <list type="bullet">
    /// <item><description>List: Only ID, Name, and key display fields</description></item>
    /// <item><description>Create: Excludes ID, timestamps, computed fields</description></item>
    /// <item><description>Update: Excludes computed and auto-generated fields</description></item>
    /// </list>
    /// </remarks>
    IReadOnlyList<string> GetPropertiesForDtoType(Table table, DtoType dtoType);

    /// <summary>
    /// Determines if a column should be excluded from DTOs based on its prefix.
    /// </summary>
    /// <param name="columnName">The column name to check.</param>
    /// <returns>True if the column should be excluded; otherwise, false.</returns>
    /// <remarks>
    /// Columns with eno_ (hashed) and ent_ (encrypted) prefixes are always excluded
    /// from DTOs for security reasons.
    /// </remarks>
    bool IsSensitiveColumn(string columnName);
}
