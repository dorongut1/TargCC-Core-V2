// <copyright file="ICommandGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.CQRS;

using TargCC.Core.Interfaces.Models;

/// <summary>
/// Defines the types of commands that can be generated.
/// </summary>
public enum CommandType
{
    /// <summary>
    /// Command to create a new entity.
    /// Generates: CreateCustomerCommand, CreateCustomerHandler, CreateCustomerValidator.
    /// </summary>
    Create,

    /// <summary>
    /// Command to update an existing entity.
    /// Generates: UpdateCustomerCommand, UpdateCustomerHandler, UpdateCustomerValidator.
    /// </summary>
    Update,

    /// <summary>
    /// Command to delete an existing entity.
    /// Generates: DeleteCustomerCommand, DeleteCustomerHandler, DeleteCustomerValidator.
    /// </summary>
    Delete,
}

/// <summary>
/// Result container for generated CQRS command components.
/// </summary>
/// <remarks>
/// Contains all generated code files for a single command operation,
/// including the command record, handler, and validator.
/// </remarks>
public class CommandGenerationResult
{
    /// <summary>
    /// Gets or sets the generated Command record code.
    /// </summary>
    /// <example>
    /// <code>
    /// public record CreateCustomerCommand : IRequest&lt;Result&lt;int&gt;&gt;
    /// {
    ///     public string Name { get; init; } = string.Empty;
    ///     public string Email { get; init; } = string.Empty;
    /// }
    /// </code>
    /// </example>
    public string CommandCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generated Handler class code.
    /// </summary>
    /// <example>
    /// <code>
    /// public class CreateCustomerHandler : IRequestHandler&lt;CreateCustomerCommand, Result&lt;int&gt;&gt;
    /// {
    ///     // Implementation with repository, logging, error handling
    /// }
    /// </code>
    /// </example>
    public string HandlerCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generated Validator class code.
    /// </summary>
    /// <example>
    /// <code>
    /// public class CreateCustomerValidator : AbstractValidator&lt;CreateCustomerCommand&gt;
    /// {
    ///     public CreateCustomerValidator()
    ///     {
    ///         RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    ///         RuleFor(x => x.Email).NotEmpty().EmailAddress();
    ///     }
    /// }
    /// </code>
    /// </example>
    public string ValidatorCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the command class name (e.g., "CreateCustomerCommand").
    /// </summary>
    public string CommandClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the handler class name (e.g., "CreateCustomerHandler").
    /// </summary>
    public string HandlerClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validator class name (e.g., "CreateCustomerValidator").
    /// </summary>
    public string ValidatorClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the command type that was generated.
    /// </summary>
    public CommandType CommandType { get; set; }
}

/// <summary>
/// Interface for generating CQRS Command components.
/// </summary>
/// <remarks>
/// <para>
/// The Command Generator creates complete CQRS command implementations following
/// the MediatR pattern. For each command type, it generates:
/// </para>
/// <list type="bullet">
/// <item><description>Command record - The request object implementing IRequest</description></item>
/// <item><description>Handler class - The IRequestHandler implementation with error handling and logging</description></item>
/// <item><description>Validator class - FluentValidation rules based on column metadata</description></item>
/// </list>
/// <para>
/// <strong>Supported Command Types:</strong>
/// </para>
/// <list type="number">
/// <item>
/// <term>Create</term>
/// <description>Creates a new entity with validation and returns the generated ID</description>
/// </item>
/// <item>
/// <term>Update</term>
/// <description>Updates an existing entity (excludes read-only columns: clc_, blg_, agg_)</description>
/// </item>
/// <item>
/// <term>Delete</term>
/// <description>Deletes an entity by primary key with existence validation</description>
/// </item>
/// </list>
/// <para>
/// <strong>Column Handling:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>eno_ (hashed) - Included in Create, special handling for password hashing</description></item>
/// <item><description>ent_ (encrypted) - Included in Create/Update, stored encrypted</description></item>
/// <item><description>clc_, blg_, agg_ - Excluded from commands (read-only)</description></item>
/// <item><description>Identity columns - Excluded from Create (auto-generated)</description></item>
/// </list>
/// <para>
/// <strong>Example Usage:</strong>
/// </para>
/// <code>
/// var generator = new CommandGenerator(logger);
/// var result = await generator.GenerateAsync(customerTable, CommandType.Create);
///
/// // result.CommandCode contains:
/// // public record CreateCustomerCommand : IRequest&lt;Result&lt;int&gt;&gt; { ... }
///
/// // result.HandlerCode contains:
/// // public class CreateCustomerHandler : IRequestHandler&lt;CreateCustomerCommand, Result&lt;int&gt;&gt;
/// // { ... }
/// </code>
/// </remarks>
public interface ICommandGenerator
{
    /// <summary>
    /// Generates a complete CQRS command implementation for the specified table and command type.
    /// </summary>
    /// <param name="table">The table metadata containing structure and constraints.</param>
    /// <param name="commandType">The type of command to generate (Create, Update, Delete).</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a
    /// <see cref="CommandGenerationResult"/> with all generated code components.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="table"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the table has no primary key (required for Update/Delete commands).
    /// </exception>
    /// <example>
    /// <code>
    /// var table = new TableBuilder()
    ///     .WithName("Customer")
    ///     .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
    ///     .WithColumn("Name", "nvarchar", maxLength: 100, isNullable: false)
    ///     .WithColumn("Email", "nvarchar", maxLength: 100, isNullable: false)
    ///     .Build();
    ///
    /// var result = await generator.GenerateAsync(table, CommandType.Create);
    ///
    /// await File.WriteAllTextAsync("CreateCustomerCommand.cs", result.CommandCode);
    /// await File.WriteAllTextAsync("CreateCustomerHandler.cs", result.HandlerCode);
    /// await File.WriteAllTextAsync("CreateCustomerValidator.cs", result.ValidatorCode);
    /// </code>
    /// </example>
    Task<CommandGenerationResult> GenerateAsync(Table table, CommandType commandType);

    /// <summary>
    /// Generates all standard commands for a table (Create, Update, Delete).
    /// </summary>
    /// <param name="table">The table metadata containing structure and constraints.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// a collection of <see cref="CommandGenerationResult"/> for all generated commands.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="table"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// var results = await generator.GenerateAllAsync(customerTable);
    /// // Returns results for CreateCustomerCommand, UpdateCustomerCommand, DeleteCustomerCommand
    ///
    /// foreach (var result in results)
    /// {
    ///     await File.WriteAllTextAsync($"{result.CommandClassName}.cs", result.CommandCode);
    ///     await File.WriteAllTextAsync($"{result.HandlerClassName}.cs", result.HandlerCode);
    ///     await File.WriteAllTextAsync($"{result.ValidatorClassName}.cs", result.ValidatorCode);
    /// }
    /// </code>
    /// </example>
    Task<IEnumerable<CommandGenerationResult>> GenerateAllAsync(Table table);

    /// <summary>
    /// Gets the list of columns that should be included in a Create command.
    /// </summary>
    /// <param name="table">The table metadata containing structure and constraints.</param>
    /// <returns>
    /// List of columns that should be included in Create command.
    /// Excludes: Identity columns, read-only columns (clc_, blg_, agg_).
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="table"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>The following columns are excluded from Create commands:</para>
    /// <list type="bullet">
    /// <item><description>Identity columns (auto-generated by database)</description></item>
    /// <item><description>Calculated columns (clc_ prefix)</description></item>
    /// <item><description>Business logic columns (blg_ prefix)</description></item>
    /// <item><description>Aggregate columns (agg_ prefix)</description></item>
    /// </list>
    /// </remarks>
    IEnumerable<Column> GetCreateColumns(Table table);

    /// <summary>
    /// Gets the list of columns that should be included in an Update command.
    /// </summary>
    /// <param name="table">The table metadata containing structure and constraints.</param>
    /// <returns>
    /// List of columns that should be included in Update command.
    /// Excludes: Primary key (except in WHERE), read-only columns.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="table"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>The following columns are excluded from Update commands:</para>
    /// <list type="bullet">
    /// <item><description>Primary key columns (used for identification only)</description></item>
    /// <item><description>Identity columns</description></item>
    /// <item><description>Hashed columns (eno_ prefix) - cannot be updated</description></item>
    /// <item><description>Calculated columns (clc_ prefix)</description></item>
    /// <item><description>Business logic columns (blg_ prefix)</description></item>
    /// <item><description>Aggregate columns (agg_ prefix)</description></item>
    /// </list>
    /// </remarks>
    IEnumerable<Column> GetUpdateColumns(Table table);

    /// <summary>
    /// Generates validation rules based on column metadata.
    /// </summary>
    /// <param name="column">The column to generate validation rules for.</param>
    /// <returns>
    /// A string containing FluentValidation rule chain for the column.
    /// </returns>
    /// <remarks>
    /// <para>Generates rules based on:</para>
    /// <list type="bullet">
    /// <item><description>NOT NULL → .NotEmpty()</description></item>
    /// <item><description>MaxLength → .MaximumLength(n)</description></item>
    /// <item><description>Email columns → .EmailAddress()</description></item>
    /// <item><description>Phone columns → .Matches(phoneRegex)</description></item>
    /// <item><description>Password (eno_) → .MinimumLength(8)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // For column "Email" (nvarchar(100), NOT NULL)
    /// // Returns:
    /// // RuleFor(x => x.Email)
    /// //     .NotEmpty()
    /// //     .EmailAddress()
    /// //     .MaximumLength(100);
    /// </code>
    /// </example>
    string GenerateValidationRules(Column column);
}
