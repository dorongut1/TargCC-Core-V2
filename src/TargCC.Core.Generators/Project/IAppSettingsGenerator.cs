using TargCC.Core.Generators.Project.Models;

namespace TargCC.Core.Generators.Project;

/// <summary>
/// Interface for generating appsettings.json file for ASP.NET Core applications.
/// </summary>
public interface IAppSettingsGenerator
{
    /// <summary>
    /// Generates appsettings.json content.
    /// </summary>
    /// <param name="projectInfo">Project information including connection strings.</param>
    /// <returns>The generated appsettings.json content.</returns>
    string Generate(ProjectInfo projectInfo);
}
