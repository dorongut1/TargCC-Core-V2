namespace TargCC.Core.Generators.Data;

using TargCC.Core.Interfaces.Models;

/// <summary>
/// Interface for generating Entity Framework Core DbContext classes from database schema metadata.
/// </summary>
/// <remarks>
/// <para>
/// The DbContext generator creates the main database context class that serves as the entry point
/// for Entity Framework Core. The generated DbContext includes DbSet properties for all entities
/// and configures the model through fluent API or data annotations.
/// </para>
/// <para>
/// <strong>Generated DbContext Structure:</strong>
/// </para>
/// <code>
/// public class ApplicationDbContext : DbContext
/// {
///     public DbSet&lt;Customer&gt; Customers { get; set; } = null!;
///     public DbSet&lt;Order&gt; Orders { get; set; } = null!;
///     // ... more DbSets
///
///     public ApplicationDbContext(DbContextOptions&lt;ApplicationDbContext&gt; options)
///         : base(options)
///     {
///     }
///
///     protected override void OnModelCreating(ModelBuilder modelBuilder)
///     {
///         modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
///     }
/// }
/// </code>
/// <para>
/// <strong>Key Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Generates DbSet properties for all tables in the schema</description></item>
/// <item><description>Configures entity relationships using Fluent API</description></item>
/// <item><description>Applies entity configurations from separate classes</description></item>
/// <item><description>Supports dependency injection through constructor</description></item>
/// <item><description>Includes XML documentation for all generated code</description></item>
/// </list>
/// </remarks>
public interface IDbContextGenerator
{
    /// <summary>
    /// Generates a complete Entity Framework Core DbContext class from database schema metadata.
    /// </summary>
    /// <param name="schema">The database schema containing all tables and relationships.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the generated
    /// DbContext class code as a string, including all DbSet properties, constructor, and
    /// OnModelCreating configuration.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="schema"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the schema contains no tables or has invalid configuration.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The generated DbContext will:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Include a DbSet property for each table in the schema</description></item>
    /// <item><description>Use proper naming conventions (pluralized DbSet names)</description></item>
    /// <item><description>Apply entity configurations from separate IEntityTypeConfiguration classes</description></item>
    /// <item><description>Support constructor-based dependency injection</description></item>
    /// <item><description>Include proper XML documentation</description></item>
    /// </list>
    /// <para>
    /// <strong>Usage Example:</strong>
    /// </para>
    /// <code>
    /// var generator = new DbContextGenerator(logger);
    /// var dbContextCode = await generator.GenerateAsync(databaseSchema);
    ///
    /// // Write to file
    /// await File.WriteAllTextAsync("ApplicationDbContext.cs", dbContextCode);
    /// </code>
    /// </remarks>
    Task<string> GenerateAsync(DatabaseSchema schema);
}
