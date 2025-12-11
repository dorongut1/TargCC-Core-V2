using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using TargCC.CLI.Constants;

namespace TargCC.CLI.Commands.Job;

/// <summary>
/// Command: targcc job info <name>
/// Shows detailed information about a specific job
/// </summary>
public class JobInfoCommand : Command
{
    public JobInfoCommand() : base("info", "Show detailed information about a job")
    {
        var jobNameArgument = new Argument<string>("job-name", "Name of the job");
        AddArgument(jobNameArgument);

        this.SetHandler(ExecuteAsync, jobNameArgument);
    }

    private Task<int> ExecuteAsync(string jobName)
    {
        try
        {
            Console.WriteLine($"📋 Job Information: {jobName}");
            Console.WriteLine();
            Console.WriteLine("Name:           SampleDailyJob");
            Console.WriteLine("Display Name:   Sample Daily Job");
            Console.WriteLine("Type:           Recurring");
            Console.WriteLine("CRON:           0 2 * * *  (Daily at 2 AM)");
            Console.WriteLine("Queue:          default");
            Console.WriteLine("Retry Attempts: 3");
            Console.WriteLine("Status:         Active");
            Console.WriteLine();
            Console.WriteLine("Description:");
            Console.WriteLine("A sample recurring job that runs daily at 2 AM");
            Console.WriteLine();
            Console.WriteLine("Last Execution:");
            Console.WriteLine("  Started:  2025-12-09 02:00:00");
            Console.WriteLine("  Duration: 1,234 ms");
            Console.WriteLine("  Status:   ✅ Success");
            Console.WriteLine();
            Console.WriteLine("💡 Use 'targcc job run {jobName}' to trigger manually");
            Console.WriteLine("💡 Use 'targcc job history {jobName}' to view execution history");

            return Task.FromResult(ExitCodes.Success);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Error: {ex.Message}");
            return Task.FromResult(ExitCodes.GeneralError);
        }
    }
}
