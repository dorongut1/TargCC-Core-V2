using TargCC.Core.Generators.Project.Models;

namespace TargCC.Core.Generators.Project;

/// <summary>
/// Interface for generating Dependency Injection registration code.
/// </summary>
public interface IDependencyInjectionGenerator
{
    /// <summary>
    /// Generates DI registration code for Application layer (DependencyInjection.cs).
    /// </summary>
    /// <param name="projectInfo">Project information including namespace and tables.</param>
    /// <returns>The generated DI registration code for Application layer.</returns>
    string GenerateApplicationDI(ProjectInfo projectInfo);

    /// <summary>
    /// Generates DI registration code for Infrastructure layer (DependencyInjection.cs).
    /// </summary>
    /// <param name="projectInfo">Project information including namespace and tables.</param>
    /// <returns>The generated DI registration code for Infrastructure layer.</returns>
    string GenerateInfrastructureDI(ProjectInfo projectInfo);
}
