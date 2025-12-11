using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TargCC.Core.Interfaces;
using TargCC.Core.Writers;

namespace TargCC.Core.Generators.Jobs;

/// <summary>
/// Generates complete job infrastructure for a TargCC project.
/// </summary>
public class JobInfrastructureGenerator
{
    private readonly IFileWriter _fileWriter;
    private readonly string _templatePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobInfrastructureGenerator"/> class.
    /// </summary>
    /// <param name="fileWriter">The file writer service for writing generated files.</param>
    public JobInfrastructureGenerator(IFileWriter fileWriter)
    {
        _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
        _templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Jobs", "Templates");
    }

    /// <summary>
    /// Generates complete job infrastructure including all base classes and configuration.
    /// </summary>
    /// <param name="projectPath">The root path of the project.</param>
    /// <param name="namespaceName">The namespace for generated files.</param>
    /// <param name="projectName">The name of the project.</param>
    /// <param name="includeSampleJobs">Whether to include sample job files (default: true).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GenerateCompleteInfrastructureAsync(
        string projectPath,
        string namespaceName,
        string projectName,
        bool includeSampleJobs = true)
    {
        // 1. Generate Application layer files
        await GenerateApplicationLayerAsync(projectPath, namespaceName);

        // 2. Generate Infrastructure layer files
        await GenerateInfrastructureLayerAsync(projectPath, namespaceName);

        // 3. Generate API configuration
        await GenerateApiConfigurationAsync(projectPath, namespaceName, projectName);

        // 4. Generate API controllers
        await GenerateApiControllersAsync(projectPath, namespaceName);

        // 5. Generate sample jobs (optional)
        if (includeSampleJobs)
        {
            await GenerateSampleJobsAsync(projectPath, namespaceName);
        }

        // 6. Generate package references
        await GeneratePackageReferencesAsync(projectPath);
    }

    private async Task GenerateApplicationLayerAsync(string projectPath, string namespaceName)
    {
        var appJobsPath = Path.Combine(projectPath, "Application", "Jobs");
        Directory.CreateDirectory(appJobsPath);

        // Core interfaces and classes
        await GenerateFromTemplateAsync("ITargCCJob.cs.template", Path.Combine(appJobsPath, "ITargCCJob.cs"), namespaceName);
        await GenerateFromTemplateAsync("JobResult.cs.template", Path.Combine(appJobsPath, "JobResult.cs"), namespaceName);
        await GenerateFromTemplateAsync("TargCCJobAttribute.cs.template", Path.Combine(appJobsPath, "TargCCJobAttribute.cs"), namespaceName);
    }

    private async Task GenerateInfrastructureLayerAsync(string projectPath, string namespaceName)
    {
        var infraJobsPath = Path.Combine(projectPath, "Infrastructure", "Jobs");
        Directory.CreateDirectory(infraJobsPath);

        // Infrastructure services
        await GenerateFromTemplateAsync("HangfireSetup.cs.template", Path.Combine(infraJobsPath, "HangfireSetup.cs"), namespaceName);
        await GenerateFromTemplateAsync("IJobDiscoveryService.cs.template", Path.Combine(infraJobsPath, "IJobDiscoveryService.cs"), namespaceName);
        await GenerateFromTemplateAsync("HangfireJobDiscoveryService.cs.template", Path.Combine(infraJobsPath, "HangfireJobDiscoveryService.cs"), namespaceName);
        await GenerateFromTemplateAsync("IJobExecutor.cs.template", Path.Combine(infraJobsPath, "IJobExecutor.cs"), namespaceName);
        await GenerateFromTemplateAsync("JobExecutor.cs.template", Path.Combine(infraJobsPath, "JobExecutor.cs"), namespaceName);
        await GenerateFromTemplateAsync("IJobLogger.cs.template", Path.Combine(infraJobsPath, "IJobLogger.cs"), namespaceName);
        await GenerateFromTemplateAsync("JobLogger.cs.template", Path.Combine(infraJobsPath, "JobLogger.cs"), namespaceName);
        await GenerateFromTemplateAsync("HangfireAuthorizationFilter.cs.template", Path.Combine(infraJobsPath, "HangfireAuthorizationFilter.cs"), namespaceName);
    }

    private async Task GenerateApiConfigurationAsync(string projectPath, string namespaceName, string projectName)
    {
        var apiPath = Path.Combine(projectPath, "API");

        // Generate Program.cs snippet (to be manually added)
        var programSnippetPath = Path.Combine(apiPath, "Program.Hangfire.snippet.cs");
        var programContent = await File.ReadAllTextAsync(Path.Combine(_templatePath, "Program.Hangfire.cs.snippet"));
        programContent = programContent.Replace("{{Namespace}}", namespaceName, StringComparison.Ordinal)
                                       .Replace("{{ProjectName}}", projectName, StringComparison.Ordinal);
        await _fileWriter.WriteFileAsync(programSnippetPath, programContent);

        // Generate appsettings snippet
        var settingsSnippetPath = Path.Combine(apiPath, "appsettings.Hangfire.snippet.json");
        var settingsContent = await File.ReadAllTextAsync(Path.Combine(_templatePath, "appsettings.Hangfire.json.snippet"));
        settingsContent = settingsContent.Replace("{{ProjectName}}", projectName, StringComparison.Ordinal);
        await _fileWriter.WriteFileAsync(settingsSnippetPath, settingsContent);
    }

    private async Task GenerateSampleJobsAsync(string projectPath, string namespaceName)
    {
        var appJobsPath = Path.Combine(projectPath, "Application", "Jobs");

        await GenerateFromTemplateAsync("SampleDailyJob.cs.template", Path.Combine(appJobsPath, "SampleDailyJob.cs"), namespaceName);
        await GenerateFromTemplateAsync("SampleManualJob.cs.template", Path.Combine(appJobsPath, "SampleManualJob.cs"), namespaceName);
    }

    private async Task GeneratePackageReferencesAsync(string projectPath)
    {
        // Get the namespace by finding any .csproj file
        var apiCsprojFiles = Directory.GetFiles(Path.Combine(projectPath, "src"), "*.API.csproj", SearchOption.AllDirectories);
        var infraCsprojFiles = Directory.GetFiles(Path.Combine(projectPath, "src"), "*.Infrastructure.csproj", SearchOption.AllDirectories);

        // Add Hangfire packages to API project
        if (apiCsprojFiles.Length > 0)
        {
            var apiPackages = new[]
            {
                ("Hangfire.Core", "1.8.9"),
                ("Hangfire.SqlServer", "1.8.9"),
                ("Hangfire.AspNetCore", "1.8.9")
            };
            await AddPackagesToCsprojAsync(apiCsprojFiles[0], apiPackages);
        }

        // Add Hangfire packages to Infrastructure project
        if (infraCsprojFiles.Length > 0)
        {
            var infraPackages = new[]
            {
                ("Hangfire.Core", "1.8.9")
            };
            await AddPackagesToCsprojAsync(infraCsprojFiles[0], infraPackages);
        }
    }

    private async Task AddPackagesToCsprojAsync(string csprojPath, (string packageName, string version)[] packages)
    {
        if (!File.Exists(csprojPath))
        {
            return;
        }

        var content = await File.ReadAllTextAsync(csprojPath);

        // Check if packages already exist to avoid duplicates
        var newPackages = new StringBuilder();
        foreach (var (packageName, version) in packages)
        {
            if (!content.Contains($"Include=\"{packageName}\"", StringComparison.OrdinalIgnoreCase))
            {
                newPackages.AppendLine(CultureInfo.InvariantCulture, $"    <PackageReference Include=\"{packageName}\" Version=\"{version}\" />");
            }
        }

        if (newPackages.Length == 0)
        {
            return; // All packages already exist
        }

        // Find the last </ItemGroup> that contains PackageReference
        var lastItemGroupIndex = content.LastIndexOf("</ItemGroup>", StringComparison.Ordinal);
        if (lastItemGroupIndex > 0)
        {
            // Check if this ItemGroup contains PackageReference
            var itemGroupStart = content.LastIndexOf("<ItemGroup>", lastItemGroupIndex, StringComparison.Ordinal);
            if (itemGroupStart > 0)
            {
                var itemGroupContent = content.Substring(itemGroupStart, lastItemGroupIndex - itemGroupStart);
                if (itemGroupContent.Contains("PackageReference", StringComparison.Ordinal))
                {
                    // Insert before the closing </ItemGroup>
                    content = content.Insert(lastItemGroupIndex, newPackages.ToString());
                }
                else
                {
                    // No PackageReference ItemGroup found, add a new one before the last </Project>
                    var projectEndIndex = content.LastIndexOf("</Project>", StringComparison.Ordinal);
                    if (projectEndIndex > 0)
                    {
                        var newItemGroup = $"\n  <ItemGroup>\n{newPackages}  </ItemGroup>\n\n";
                        content = content.Insert(projectEndIndex, newItemGroup);
                    }
                }
            }
        }

        await _fileWriter.WriteFileAsync(csprojPath, content);
    }

    private async Task GenerateApiControllersAsync(string projectPath, string namespaceName)
    {
        var apiControllersPath = Path.Combine(projectPath, "src", $"{namespaceName}.API", "Controllers");
        Directory.CreateDirectory(apiControllersPath);

        await GenerateFromTemplateAsync("JobsController.cs.template", Path.Combine(apiControllersPath, "JobsController.cs"), namespaceName);
    }

    private async Task GenerateFromTemplateAsync(string templateFileName, string outputPath, string namespaceName)
    {
        var templatePath = Path.Combine(_templatePath, templateFileName);

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template not found: {templatePath}");
        }

        var content = await File.ReadAllTextAsync(templatePath);
        content = content.Replace("{{Namespace}}", namespaceName, StringComparison.Ordinal);

        await _fileWriter.WriteFileAsync(outputPath, content);
    }
}
