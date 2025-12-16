namespace TargCC.Core.Generators.Data;

using TargCC.Core.Interfaces.Models;

/// <summary>
/// Interface for generating IApplicationDbContext interface.
/// </summary>
public interface IApplicationDbContextInterfaceGenerator
{
    /// <summary>
    /// Generates the IApplicationDbContext interface from database schema metadata.
    /// </summary>
    /// <param name="schema">The database schema containing all tables.</param>
    /// <param name="rootNamespace">The root namespace for the project.</param>
    /// <returns>The generated interface code as a string.</returns>
    Task<string> GenerateAsync(DatabaseSchema schema, string rootNamespace);
}
