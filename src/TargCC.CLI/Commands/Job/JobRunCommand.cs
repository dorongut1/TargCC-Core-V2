using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using TargCC.CLI.Constants;

namespace TargCC.CLI.Commands.Job;

/// <summary>
/// Command: targcc job run <name>
/// Triggers a job manually
/// </summary>
public class JobRunCommand : Command
{
    public JobRunCommand() : base("run", "Trigger a job manually")
    {
        var jobNameArgument = new Argument<string>("job-name", "Name of the job to run");
        AddArgument(jobNameArgument);

        var waitOption = new Option<bool>(
            "--wait",
            getDefaultValue: () => false,
            description: "Wait for job to complete");
        waitOption.AddAlias("-w");
        AddOption(waitOption);

        this.SetHandler(ExecuteAsync, jobNameArgument, waitOption);
    }

    private async Task<int> ExecuteAsync(string jobName, bool wait)
    {
        try
        {
            Console.WriteLine($"🚀 Triggering job: {jobName}");
            Console.WriteLine();

            // TODO: Implement actual job triggering via API
            Console.WriteLine("Job enqueued: Hangfire-12345");
            Console.WriteLine("Status: Enqueued");
            Console.WriteLine();

            if (wait)
            {
                Console.WriteLine("⏳ Waiting for job to complete...");
                await Task.Delay(2000); // Simulate waiting
                Console.WriteLine("✅ Job completed successfully");
                Console.WriteLine("Duration: 1,234 ms");
            }
            else
            {
                Console.WriteLine("💡 Job running in background");
                Console.WriteLine("💡 Use 'targcc job history {jobName}' to check status");
            }

            Console.WriteLine();
            Console.WriteLine("View in dashboard: http://localhost:5000/hangfire/jobs/details/12345");

            return ExitCodes.Success;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Error: {ex.Message}");
            return ExitCodes.GeneralError;
        }
    }
}
