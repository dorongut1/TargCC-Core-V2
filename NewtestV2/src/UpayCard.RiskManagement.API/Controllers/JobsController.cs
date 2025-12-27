using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UpayCard.RiskManagement.API.Controllers;

/// <summary>
/// API controller for job management and monitoring
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly ILogger<JobsController> _logger;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;

    public JobsController(
        ILogger<JobsController> logger,
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
        _recurringJobManager = recurringJobManager ?? throw new ArgumentNullException(nameof(recurringJobManager));
    }

    /// <summary>
    /// Get list of all registered jobs
    /// </summary>
    [HttpGet]
    public IActionResult GetJobs()
    {
        try
        {
            using var connection = JobStorage.Current.GetConnection();
            var recurringJobs = connection.GetRecurringJobs();

            var jobs = recurringJobs.Select(job => new
            {
                id = job.Id,
                cron = job.Cron,
                nextExecution = job.NextExecution,
                lastExecution = job.LastExecution,
                lastJobState = job.LastJobState,
                createdAt = job.CreatedAt
            }).ToList();

            return Ok(new { jobs, total = jobs.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve jobs");
            return StatusCode(500, new { error = "Failed to retrieve jobs", message = ex.Message });
        }
    }

    /// <summary>
    /// Get detailed information about a specific job
    /// </summary>
    /// <param name="jobId">Job identifier</param>
    [HttpGet("{jobId}")]
    public IActionResult GetJob(string jobId)
    {
        try
        {
            using var connection = JobStorage.Current.GetConnection();
            var recurringJobs = connection.GetRecurringJobs();
            var job = recurringJobs.FirstOrDefault(j => j.Id == jobId);

            if (job == null)
            {
                return NotFound(new { error = $"Job '{jobId}' not found" });
            }

            return Ok(new
            {
                id = job.Id,
                cron = job.Cron,
                nextExecution = job.NextExecution,
                lastExecution = job.LastExecution,
                lastJobState = job.LastJobState,
                createdAt = job.CreatedAt,
                queue = job.Queue,
                timeZone = job.TimeZoneId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve job {JobId}", jobId);
            return StatusCode(500, new { error = "Failed to retrieve job", message = ex.Message });
        }
    }

    /// <summary>
    /// Trigger a job manually
    /// </summary>
    /// <param name="jobId">Job identifier</param>
    [HttpPost("{jobId}/trigger")]
    public IActionResult TriggerJob(string jobId)
    {
        try
        {
            _recurringJobManager.Trigger(jobId);
            _logger.LogInformation("Job {JobId} triggered manually", jobId);

            return Ok(new
            {
                message = $"Job '{jobId}' triggered successfully",
                jobId,
                triggeredAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger job {JobId}", jobId);
            return StatusCode(500, new { error = "Failed to trigger job", message = ex.Message });
        }
    }

    /// <summary>
    /// Get execution history for a job
    /// </summary>
    /// <param name="jobId">Job identifier</param>
    /// <param name="count">Number of history items to return (default: 50)</param>
    [HttpGet("{jobId}/history")]
    public IActionResult GetJobHistory(string jobId, [FromQuery] int count = 50)
    {
        try
        {
            using var connection = JobStorage.Current.GetConnection();
            var monitoringApi = JobStorage.Current.GetMonitoringApi();

            var succeededJobs = monitoringApi.SucceededJobs(0, count)
                .Where(j => j.Value?.Job?.Type?.Name == jobId)
                .Select(j => new
                {
                    jobId = j.Key,
                    succeededAt = j.Value.SucceededAt,
                    totalDuration = j.Value.TotalDuration,
                    status = "Succeeded"
                });

            var failedJobs = monitoringApi.FailedJobs(0, count)
                .Where(j => j.Value?.Job?.Type?.Name == jobId)
                .Select(j => new
                {
                    jobId = j.Key,
                    failedAt = j.Value.FailedAt,
                    exceptionMessage = j.Value.ExceptionMessage,
                    status = "Failed"
                });

            var history = succeededJobs.Concat<object>(failedJobs)
                .Take(count)
                .ToList();

            return Ok(new { jobId, history, total = history.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve history for job {JobId}", jobId);
            return StatusCode(500, new { error = "Failed to retrieve job history", message = ex.Message });
        }
    }

    /// <summary>
    /// Get statistics for a job
    /// </summary>
    /// <param name="jobId">Job identifier</param>
    [HttpGet("{jobId}/stats")]
    public IActionResult GetJobStats(string jobId)
    {
        try
        {
            using var connection = JobStorage.Current.GetConnection();
            var monitoringApi = JobStorage.Current.GetMonitoringApi();

            var succeededCount = monitoringApi.SucceededJobs(0, int.MaxValue)
                .Count(j => j.Value?.Job?.Type?.Name == jobId);

            var failedCount = monitoringApi.FailedJobs(0, int.MaxValue)
                .Count(j => j.Value?.Job?.Type?.Name == jobId);

            var totalExecutions = succeededCount + failedCount;
            var successRate = totalExecutions > 0
                ? (double)succeededCount / totalExecutions * 100
                : 0;

            return Ok(new
            {
                jobId,
                totalExecutions,
                succeeded = succeededCount,
                failed = failedCount,
                successRate = Math.Round(successRate, 2)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve stats for job {JobId}", jobId);
            return StatusCode(500, new { error = "Failed to retrieve job stats", message = ex.Message });
        }
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    [HttpGet("dashboard/stats")]
    public IActionResult GetDashboardStats()
    {
        try
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var stats = monitoringApi.GetStatistics();

            return Ok(new
            {
                enqueued = stats.Enqueued,
                scheduled = stats.Scheduled,
                processing = stats.Processing,
                succeeded = stats.Succeeded,
                failed = stats.Failed,
                deleted = stats.Deleted,
                recurring = stats.Recurring,
                servers = stats.Servers,
                queues = stats.Queues
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve dashboard stats");
            return StatusCode(500, new { error = "Failed to retrieve dashboard stats", message = ex.Message });
        }
    }
}
