using System.CommandLine;

namespace TargCC.CLI.Commands.Job;

/// <summary>
/// Root command for job management: targcc job
/// </summary>
public class JobCommand : Command
{
    public JobCommand() : base("job", "Manage background jobs and scheduled tasks")
    {
        // Add subcommands
        AddCommand(new JobListCommand());
        AddCommand(new JobInfoCommand());
        AddCommand(new JobRunCommand());
        AddCommand(new JobHistoryCommand());
        AddCommand(new JobStatsCommand());
        AddCommand(new JobGenerateCommand());
    }
}
