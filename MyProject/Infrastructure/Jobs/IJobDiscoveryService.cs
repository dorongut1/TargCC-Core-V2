using System.Collections.Generic;

namespace UpayCard.RiskManagement.Infrastructure.Jobs;

/// <summary>
/// Service for discovering and registering background jobs
/// </summary>
public interface IJobDiscoveryService
{
    /// <summary>
    /// Discovers all jobs in the assembly and registers them with Hangfire
    /// </summary>
    void RegisterAllJobs();

    /// <summary>
    /// Gets metadata for all discovered jobs
    /// </summary>
    IEnumerable<JobMetadata> GetAllJobs();

    /// <summary>
    /// Gets metadata for a specific job by name
    /// </summary>
    JobMetadata? GetJobByName(string jobName);
}

/// <summary>
/// Metadata about a discovered job
/// </summary>
public class JobMetadata
{
    public required string JobName { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string JobType { get; set; } = "Recurring";
    public string? CronExpression { get; set; }
    public string Queue { get; set; } = "default";
    public int RetryAttempts { get; set; } = 3;
    public required System.Type JobTypeReference { get; set; }
}
