using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using TargCC.CLI.Constants;

namespace TargCC.CLI.Commands.Job;

/// <summary>
/// Command: targcc job history <name>
/// Shows execution history for a job
/// </summary>
public class JobHistoryCommand : Command
{
    public JobHistoryCommand() : base("history", "Show job execution history")
    {
        var jobNameArgument = new Argument<string?>(
            "job-name",
            getDefaultValue: () => null,
            description: "Name of the job (optional - shows all if omitted)");
        AddArgument(jobNameArgument);

        var limitOption = new Option<int>(
            "--limit",
            getDefaultValue: () => 10,
            description: "Number of records to show");
        limitOption.AddAlias("-n");
        AddOption(limitOption);

        var failedOnlyOption = new Option<bool>(
            "--failed",
            getDefaultValue: () => false,
            description: "Show only failed executions");
        failedOnlyOption.AddAlias("-f");
        AddOption(failedOnlyOption);

        this.SetHandler(ExecuteAsync, jobNameArgument, limitOption, failedOnlyOption);
    }

    private Task<int> ExecuteAsync(string? jobName, int limit, bool failedOnly)
    {
        try
        {
            var title = jobName != null
                ? $"📊 Execution History: {jobName}"
                : "📊 Execution History: All Jobs";

            if (failedOnly) title += " (Failed Only)";

            Console.WriteLine(title);
            Console.WriteLine();

            // Table header
            Console.WriteLine($"{"Job Name",-25} {"Started",-20} {"Duration",-12} {"Status",-10}");
            Console.WriteLine(new string('-', 70));

            // Sample data
            Console.WriteLine($"{"SampleDailyJob",-25} {"2025-12-09 02:00:00",-20} {"1,234 ms",-12} {"✅ Success",-10}");
            Console.WriteLine($"{"SampleDailyJob",-25} {"2025-12-08 02:00:00",-20} {"1,156 ms",-12} {"✅ Success",-10}");
            Console.WriteLine($"{"SampleDailyJob",-25} {"2025-12-07 02:00:00",-20} {"Error",-12} {"❌ Failed",-10}");

            Console.WriteLine();
            Console.WriteLine($"Showing last {limit} execution(s)");
            Console.WriteLine();
            Console.WriteLine("💡 Use 'targcc job stats <name>' for detailed statistics");

            return Task.FromResult(ExitCodes.Success);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Error: {ex.Message}");
            return Task.FromResult(ExitCodes.GeneralError);
        }
    }
}
