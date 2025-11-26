using System.Text;
using System.Text.Json;
using TargCC.Core.Generators.Project.Models;

namespace TargCC.Core.Generators.Project;

/// <summary>
/// Generates appsettings.json file for ASP.NET Core applications.
/// </summary>
public class AppSettingsGenerator : IAppSettingsGenerator
{
    /// <inheritdoc/>
    public string Generate(ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(projectInfo);

        var settings = new
        {
            Logging = new
            {
                LogLevel = new
                {
                    Default = "Information",
                    Microsoft_AspNetCore = "Warning"
                }
            },
            AllowedHosts = "*",
            ConnectionStrings = new
            {
                DefaultConnection = projectInfo.ConnectionString ??
                    $"Server=(localdb)\\mssqllocaldb;Database={projectInfo.Namespace};Trusted_Connection=True;MultipleActiveResultSets=true"
            }
        };

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var options = jsonSerializerOptions;

        return JsonSerializer.Serialize(settings, options);
    }
}
