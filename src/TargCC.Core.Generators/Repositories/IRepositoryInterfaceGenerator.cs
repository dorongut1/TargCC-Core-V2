namespace TargCC.Core.Generators.Repositories;

using TargCC.Core.Interfaces.Models;

/// <summary>
/// Interface for generating repository interface code from database table metadata.
/// </summary>
/// <remarks>
/// <para>
/// The RepositoryInterfaceGenerator creates repository interface definitions (IRepository)
/// that define the contract for data access operations on a specific entity.
/// </para>
/// <para>
/// <strong>Generated Interface Structure:</strong>
/// </para>
/// <list type="bullet">
/// <item><term>CRUD Methods</term><description>GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync</description></item>
/// <item><term>Query Methods</term><description>Methods based on table indexes (GetByEmailAsync, GetByStatusAsync)</description></item>
/// <item><term>Aggregate Methods</term><description>UpdateAggregatesAsync for tables with agg_ columns</description></item>
/// <item><term>Helper Methods</term><description>ExistsAsync for existence checks</description></item>
/// </list>
/// <para>
/// <strong>What Gets Generated:</strong>
/// </para>
/// <code>
/// // From: Customer table with Email unique index and agg_OrderCount column
///
/// public interface ICustomerRepository
/// {
///     // CRUD operations
///     Task&lt;Customer?&gt; GetByIdAsync(int id, CancellationToken cancellationToken = default);
///     Task&lt;IEnumerable&lt;Customer&gt;&gt; GetAllAsync(CancellationToken cancellationToken = default);
///     Task AddAsync(Customer entity, CancellationToken cancellationToken = default);
///     Task UpdateAsync(Customer entity, CancellationToken cancellationToken = default);
///     Task DeleteAsync(int id, CancellationToken cancellationToken = default);
///
///    // Index-based query (Email is unique)
///     Task&lt;Customer?&gt; GetByEmailAsync(string email, CancellationToken cancellationToken = default);
///
///     // Aggregate update (agg_OrderCount exists)
///     Task UpdateAggregatesAsync(int id, int orderCount, CancellationToken cancellationToken = default);
///
///     // Helper methods
///     Task&lt;bool&gt; ExistsAsync(int id, CancellationToken cancellationToken = default);
/// }
/// </code>
/// </remarks>
/// <example>
/// <para><strong>Example: Generate repository interface for Customer table</strong></para>
/// <code>
/// var generator = new RepositoryInterfaceGenerator(logger);
/// var customerTable = new Table
/// {
///     Name = "Customer",
///     PrimaryKeyColumns = new List&lt;string&gt; { "ID" },
///     Columns = new List&lt;Column&gt;
///     {
///         new() { Name = "ID", DataType = "int", IsPrimaryKey = true },
///         new() { Name = "Email", DataType = "nvarchar" },
///         new() { Name = "agg_OrderCount", Prefix = ColumnPrefix.Aggregate }
///     },
///     Indexes = new List&lt;Index&gt;
///     {
///         new() { Name = "IX_Customer_Email", IsUnique = true, Columns = new() { "Email" } }
///     }
/// };
///
/// string interfaceCode = await generator.GenerateAsync(customerTable);
///
/// // Output:
/// // - ICustomerRepository interface
/// // - CRUD methods
/// // - GetByEmailAsync method (from unique index)
/// // - UpdateAggregatesAsync method (from agg_ column)
/// </code>
/// </example>
public interface IRepositoryInterfaceGenerator
{
    /// <summary>
    /// Generates the repository interface code for a given table.
    /// </summary>
    /// <param name="table">The table metadata containing columns, indexes, and relationships.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated C# interface code as a string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="table"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the table has no primary key defined.</exception>
    /// <remarks>
    /// <para>
    /// This method analyzes the table structure and generates:
    /// </para>
    /// <list type="number">
    /// <item>Basic CRUD methods (Create, Read, Update, Delete)</item>
    /// <item>Query methods based on unique indexes</item>
    /// <item>Filter methods based on non-unique indexes</item>
    /// <item>Special methods for aggregate columns (agg_)</item>
    /// <item>Helper methods (ExistsAsync)</item>
    /// </list>
    /// <para>
    /// <strong>Method Naming Convention:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Unique index on Email → GetByEmailAsync</item>
    /// <item>Non-unique index on Status → GetByStatusAsync (returns IEnumerable)</item>
    /// <item>Composite unique index → GetByLastNameAndFirstNameAsync</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var table = new Table { Name = "Customer", PrimaryKeyColumns = new() { "ID" } };
    /// string code = await generator.GenerateAsync(table);
    ///
    /// // code contains:
    /// // public interface ICustomerRepository { ... }
    /// </code>
    /// </example>
    Task<string> GenerateAsync(Table table, string rootNamespace = "YourApp");
}
