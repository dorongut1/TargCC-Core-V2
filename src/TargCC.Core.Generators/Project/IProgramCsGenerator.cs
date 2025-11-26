using TargCC.Core.Generators.Project.Models;

namespace TargCC.Core.Generators.Project;

/// <summary>
/// Interface for generating Program.cs file for ASP.NET Core applications.
/// </summary>
public interface IProgramCsGenerator
{
    /// <summary>
    /// Generates Program.cs content for an ASP.NET Core application.
    /// </summary>
    /// <param name="projectInfo">Project information including namespace and configuration.</param>
    /// <returns>The generated Program.cs content.</returns>
    string Generate(ProjectInfo projectInfo);
}
