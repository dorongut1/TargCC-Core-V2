using System.Threading.Tasks;

namespace UpayCard.RiskManagement.Infrastructure.Jobs;

/// <summary>
/// Service for logging job executions to database
/// </summary>
public interface IJobLogger
{
    /// <summary>
    /// Logs a job execution to c_LoggedJob table
    /// </summary>
    Task LogJobExecutionAsync(JobExecutionLog log);
}
