namespace TargCC.Core.Generators.Data;

using TargCC.Core.Interfaces.Models;

/// <summary>
/// Interface for generating Entity Framework Core entity configuration classes.
/// </summary>
/// <remarks>
/// <para>
/// The Entity Configuration generator creates IEntityTypeConfiguration classes that configure
/// entity properties, relationships, and constraints using the Fluent API. This provides a
/// clean separation of entity mapping logic from the DbContext.
/// </para>
/// <para>
/// <strong>Generated Configuration Structure:</strong>
/// </para>
/// <code>
/// public class CustomerConfiguration : IEntityTypeConfiguration&lt;Customer&gt;
/// {
///     public void Configure(EntityTypeBuilder&lt;Customer&gt; builder)
///     {
///         // Table mapping
///         builder.ToTable("Customer");
///
///         // Primary key
///         builder.HasKey(e => e.ID);
///
///         // Properties
///         builder.Property(e => e.Name)
///             .IsRequired()
///             .HasMaxLength(100);
///
///         // Relationships
///         builder.HasMany(e => e.Orders)
///             .WithOne(o => o.Customer)
///             .HasForeignKey(o => o.CustomerID);
///     }
/// }
/// </code>
/// </remarks>
public interface IEntityConfigurationGenerator
{
    /// <summary>
    /// Generates an Entity Framework Core entity configuration class from table metadata.
    /// </summary>
    /// <param name="table">The table metadata containing structure and constraints.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the generated
    /// entity configuration class code as a string.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="table"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the table has no primary key or invalid configuration.
    /// </exception>
    Task<string> GenerateAsync(Table table);
}
