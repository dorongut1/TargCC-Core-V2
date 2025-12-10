using System;
using System.IO;
using System.Threading.Tasks;
using TargCC.Core.Interfaces;
using TargCC.Core.Writers;

namespace TargCC.Core.Generators.Jobs;

/// <summary>
/// Generator for TargCC background jobs infrastructure.
/// </summary>
public class JobGenerator
{
    private readonly IFileWriter _fileWriter;
    private readonly string _templatePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobGenerator"/> class.
    /// </summary>
    /// <param name="fileWriter">The file writer service for writing generated files.</param>
    public JobGenerator(IFileWriter fileWriter)
    {
        _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
        _templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Jobs", "Templates");
    }

    /// <summary>
    /// Generates all job infrastructure files (interfaces, attributes, base classes).
    /// </summary>
    /// <param name="projectPath">The root path of the project.</param>
    /// <param name="namespaceName">The namespace for generated files.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GenerateJobInfrastructureAsync(string projectPath, string namespaceName)
    {
        var applicationPath = Path.Combine(projectPath, "Application", "Jobs");
        Directory.CreateDirectory(applicationPath);

        // Generate ITargCCJob interface
        await GenerateFileFromTemplateAsync(
            "ITargCCJob.cs.template",
            Path.Combine(applicationPath, "ITargCCJob.cs"),
            namespaceName);

        // Generate JobResult class
        await GenerateFileFromTemplateAsync(
            "JobResult.cs.template",
            Path.Combine(applicationPath, "JobResult.cs"),
            namespaceName);

        // Generate TargCCJobAttribute
        await GenerateFileFromTemplateAsync(
            "TargCCJobAttribute.cs.template",
            Path.Combine(applicationPath, "TargCCJobAttribute.cs"),
            namespaceName);
    }

    /// <summary>
    /// Generates a sample job file.
    /// </summary>
    /// <param name="projectPath">The root path of the project.</param>
    /// <param name="namespaceName">The namespace for the generated job.</param>
    /// <param name="jobName">The name of the job class.</param>
    /// <param name="templateType">The type of job template to use (default: Daily).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GenerateSampleJobAsync(
        string projectPath,
        string namespaceName,
        string jobName,
        JobTemplateType templateType = JobTemplateType.Daily)
    {
        var applicationPath = Path.Combine(projectPath, "Application", "Jobs");
        Directory.CreateDirectory(applicationPath);

        var templateFile = templateType switch
        {
            JobTemplateType.Daily => "SampleDailyJob.cs.template",
            JobTemplateType.Manual => "SampleManualJob.cs.template",
            _ => "SampleDailyJob.cs.template"
        };

        var outputFileName = string.IsNullOrEmpty(jobName)
            ? GetDefaultJobFileName(templateType)
            : $"{jobName}.cs";

        await GenerateFileFromTemplateAsync(
            templateFile,
            Path.Combine(applicationPath, outputFileName),
            namespaceName);
    }

    /// <summary>
    /// Generates a custom job from template.
    /// </summary>
    /// <param name="projectPath">The root path of the project.</param>
    /// <param name="namespaceName">The namespace for the generated job.</param>
    /// <param name="jobName">The name of the job class.</param>
    /// <param name="cronExpression">The CRON expression for scheduling (default: "0 2 * * *").</param>
    /// <param name="displayName">The display name for the job.</param>
    /// <param name="description">The description of the job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GenerateCustomJobAsync(
        string projectPath,
        string namespaceName,
        string jobName,
        string cronExpression = "0 2 * * *",
        string displayName = "",
        string description = "")
    {
        var applicationPath = Path.Combine(projectPath, "Application", "Jobs");
        Directory.CreateDirectory(applicationPath);

        var jobContent = GenerateCustomJobContent(
            namespaceName,
            jobName,
            cronExpression,
            displayName,
            description);

        var outputPath = Path.Combine(applicationPath, $"{jobName}.cs");
        await _fileWriter.WriteFileAsync(outputPath, jobContent);
    }

    private static string GetDefaultJobFileName(JobTemplateType templateType)
    {
        return templateType == JobTemplateType.Manual
            ? "SampleManualJob.cs"
            : "SampleDailyJob.cs";
    }

    private static string GenerateCustomJobContent(
        string namespaceName,
        string jobName,
        string cronExpression,
        string displayName,
        string description)
    {
        var actualDisplayName = string.IsNullOrEmpty(displayName) ? jobName : displayName;
        var actualDescription = string.IsNullOrEmpty(description)
            ? $"Background job: {jobName}"
            : description;

        return $@"using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace {namespaceName}.Application.Jobs;

/// <summary>
/// {actualDescription}
/// </summary>
[TargCCJob(""{cronExpression}"")]
[JobDisplayName(""{actualDisplayName}"")]
[JobDescription(""{actualDescription}"")]
public class {jobName} : ITargCCJob
{{
    private readonly ILogger<{jobName}> _logger;

    public {jobName}(ILogger<{jobName}> logger)
    {{
        _logger = logger;
    }}

    public async Task<JobResult> ExecuteAsync(CancellationToken cancellationToken)
    {{
        _logger.LogInformation(""{jobName} started at {{Time}}"", DateTime.UtcNow);

        try
        {{
            // TODO: Implement your job logic here

            await Task.CompletedTask;

            _logger.LogInformation(""{jobName} completed successfully"");

            return JobResult.Success(""Job completed successfully"");
        }}
        catch (OperationCanceledException)
        {{
            _logger.LogWarning(""{jobName} was cancelled"");
            return JobResult.Failure(""Job was cancelled"");
        }}
        catch (Exception ex)
        {{
            _logger.LogError(ex, ""{jobName} failed"");
            return JobResult.Failure($""Job failed: {{ex.Message}}"", ex);
        }}
    }}
}}";
    }

    private async Task GenerateFileFromTemplateAsync(
        string templateFileName,
        string outputPath,
        string namespaceName)
    {
        var templatePath = Path.Combine(_templatePath, templateFileName);

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}");
        }

        var content = await File.ReadAllTextAsync(templatePath);
        content = content.Replace("{{Namespace}}", namespaceName, StringComparison.Ordinal);

        await _fileWriter.WriteFileAsync(outputPath, content);
    }
}

/// <summary>
/// Types of job templates available.
/// </summary>
public enum JobTemplateType
{
    /// <summary>
    /// Daily recurring job template.
    /// </summary>
    Daily,

    /// <summary>
    /// Manual trigger job template.
    /// </summary>
    Manual,

    /// <summary>
    /// Custom job template.
    /// </summary>
    Custom
}
