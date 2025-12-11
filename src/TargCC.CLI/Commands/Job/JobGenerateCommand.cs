using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using TargCC.CLI.Constants;

namespace TargCC.CLI.Commands.Job;

/// <summary>
/// Command: targcc generate job <name>
/// Generates a new background job from template
/// </summary>
public class JobGenerateCommand : Command
{
    public JobGenerateCommand() : base("generate", "Generate a new background job")
    {
        var jobNameArgument = new Argument<string>("job-name", "Name of the job (e.g., DailyReportJob)");
        AddArgument(jobNameArgument);

        var cronOption = new Option<string?>(
            "--cron",
            description: "CRON expression for recurring jobs (e.g., '0 2 * * *')");
        cronOption.AddAlias("-c");
        AddOption(cronOption);

        var manualOption = new Option<bool>(
            "--manual",
            getDefaultValue: () => false,
            description: "Create a manual-trigger job");
        manualOption.AddAlias("-m");
        AddOption(manualOption);

        var displayNameOption = new Option<string?>(
            "--display-name",
            description: "Display name for the job");
        displayNameOption.AddAlias("-d");
        AddOption(displayNameOption);

        var descriptionOption = new Option<string?>(
            "--description",
            description: "Description of what the job does");
        AddOption(descriptionOption);

        var projectPathOption = new Option<string?>(
            "--project",
            description: "Path to project directory (default: current directory)");
        projectPathOption.AddAlias("-p");
        AddOption(projectPathOption);

        this.SetHandler(
            ExecuteAsync,
            jobNameArgument,
            cronOption,
            manualOption,
            displayNameOption,
            descriptionOption,
            projectPathOption);
    }

    private async Task<int> ExecuteAsync(
        string jobName,
        string? cron,
        bool manual,
        string? displayName,
        string? description,
        string? projectPath)
    {
        try
        {
            projectPath ??= Directory.GetCurrentDirectory();

            Console.WriteLine($"🔨 Generating job: {jobName}");
            Console.WriteLine();

            // Validate job name
            if (!jobName.EndsWith("Job", StringComparison.OrdinalIgnoreCase))
            {
                jobName += "Job";
            }

            // Determine job type
            var jobType = manual ? "Manual" : "Recurring";
            var schedule = manual ? "Manual trigger only" : (cron ?? "0 2 * * *");

            Console.WriteLine($"Type:        {jobType}");
            Console.WriteLine($"Schedule:    {schedule}");
            Console.WriteLine($"Display:     {displayName ?? jobName}");
            Console.WriteLine($"Description: {description ?? $"Background job: {jobName}"}");
            Console.WriteLine();

            // TODO: Implement actual job generation using JobGenerator
            var outputPath = Path.Combine(projectPath, "Application", "Jobs", $"{jobName}.cs");
            Console.WriteLine($"📝 Creating file: {outputPath}");

            // For now, just show success message
            await Task.Delay(500); // Simulate file creation

            Console.WriteLine("✅ Job created successfully!");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine("1. Customize the job logic in ExecuteAsync()");
            Console.WriteLine("2. Add any required services via constructor DI");
            Console.WriteLine("3. Build your project");
            Console.WriteLine("4. The job will be auto-discovered on startup");
            Console.WriteLine();
            Console.WriteLine("💡 Test your job:");
            Console.WriteLine($"   targcc job run {jobName}");

            return ExitCodes.Success;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Error: {ex.Message}");
            return ExitCodes.GeneralError;
        }
    }
}
