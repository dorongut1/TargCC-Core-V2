namespace TargCC.CLI.Services.Generation;

/// <summary>
/// Service for generating complete Clean Architecture projects.
/// </summary>
public interface IProjectGenerationService
{
    /// <summary>
    /// Generates a complete Clean Architecture project from a database.
    /// </summary>
    /// <param name="databaseName">Name of the database.</param>
    /// <param name="connectionString">Database connection string.</param>
    /// <param name="outputDirectory">Output directory path.</param>
    /// <param name="rootNamespace">Root namespace for the project.</param>
    /// <param name="includeTests">Whether to include test project.</param>
    /// <param name="force">Whether to overwrite existing files.</param>
    /// <returns>A task representing the async operation.</returns>
    Task GenerateCompleteProjectAsync(
        string databaseName,
        string connectionString,
        string outputDirectory,
        string rootNamespace,
        bool includeTests,
        bool force);
}
