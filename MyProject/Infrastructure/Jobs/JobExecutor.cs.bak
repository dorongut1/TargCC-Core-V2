using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UpayCard.RiskManagement.Application.Jobs;

namespace UpayCard.RiskManagement.Infrastructure.Jobs;

/// <summary>
/// Executes jobs with comprehensive logging and error handling
/// </summary>
public class JobExecutor : IJobExecutor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IJobLogger _jobLogger;
    private readonly ILogger<JobExecutor> _logger;

    public JobExecutor(
        IServiceProvider serviceProvider,
        IJobLogger jobLogger,
        ILogger<JobExecutor> logger)
    {
        _serviceProvider = serviceProvider;
        _jobLogger = jobLogger;
        _logger = logger;
    }

    public async Task<JobResult> ExecuteJobAsync(Type jobType, CancellationToken cancellationToken)
    {
        var jobName = jobType.Name;
        var startTime = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Starting job execution: {JobName}", jobName);

        try
        {
            // Create a scope for DI
            using var scope = _serviceProvider.CreateScope();

            // Resolve job instance
            var job = (ITargCCJob)ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, jobType);

            // Execute job
            var result = await job.ExecuteAsync(cancellationToken);

            stopwatch.Stop();

            // Log success
            await _jobLogger.LogJobExecutionAsync(new JobExecutionLog
            {
                JobName = jobName,
                StartedAt = startTime,
                CompletedAt = DateTime.UtcNow,
                DurationMs = (int)stopwatch.ElapsedMilliseconds,
                Success = result.Success,
                ResultMessage = result.Message,
                Metadata = result.Metadata
            });

            _logger.LogInformation(
                "Job completed successfully: {JobName} in {Duration}ms",
                jobName,
                stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();

            _logger.LogWarning("Job was cancelled: {JobName}", jobName);

            await _jobLogger.LogJobExecutionAsync(new JobExecutionLog
            {
                JobName = jobName,
                StartedAt = startTime,
                CompletedAt = DateTime.UtcNow,
                DurationMs = (int)stopwatch.ElapsedMilliseconds,
                Success = false,
                ErrorMessage = "Job was cancelled"
            });

            return JobResult.Failure("Job was cancelled");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex, "Job failed: {JobName}", jobName);

            await _jobLogger.LogJobExecutionAsync(new JobExecutionLog
            {
                JobName = jobName,
                StartedAt = startTime,
                CompletedAt = DateTime.UtcNow,
                DurationMs = (int)stopwatch.ElapsedMilliseconds,
                Success = false,
                ErrorMessage = ex.Message,
                StackTrace = ex.StackTrace
            });

            // Re-throw for Hangfire retry logic
            throw;
        }
    }

    public async Task<JobResult> ExecuteJobByNameAsync(string jobName, CancellationToken cancellationToken)
    {
        // Find job type by name
        var jobType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch
                {
                    return Array.Empty<Type>();
                }
            })
            .FirstOrDefault(t => t.Name.Equals(jobName, StringComparison.OrdinalIgnoreCase)
                              && typeof(ITargCCJob).IsAssignableFrom(t));

        if (jobType == null)
        {
            throw new InvalidOperationException($"Job '{jobName}' not found");
        }

        return await ExecuteJobAsync(jobType, cancellationToken);
    }
}

/// <summary>
/// Log entry for job execution
/// </summary>
public class JobExecutionLog
{
    public required string JobName { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public int DurationMs { get; set; }
    public bool Success { get; set; }
    public string? ResultMessage { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    public System.Collections.Generic.Dictionary<string, object>? Metadata { get; set; }
}
