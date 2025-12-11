using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace UpayCard.RiskManagement.Application.Jobs;

/// <summary>
/// Sample daily job that runs at 2 AM every day.
/// This is a template - customize it for your needs!
/// </summary>
[TargCCJob("0 2 * * *")]  // CRON: Daily at 2 AM
[JobDisplayName("Sample Daily Job")]
[JobDescription("A sample recurring job that runs daily at 2 AM")]
public class SampleDailyJob : ITargCCJob
{
    private readonly ILogger<SampleDailyJob> _logger;

    // Dependency Injection is fully supported!
    // Add any services you need through the constructor
    public SampleDailyJob(ILogger<SampleDailyJob> logger)
    {
        _logger = logger;
    }

    public async Task<JobResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SampleDailyJob started at {Time}", DateTime.UtcNow);

        try
        {
            // TODO: Replace this with your actual business logic

            // Example: Process data
            await Task.Delay(1000, cancellationToken); // Simulate work

            // Example: Call a service
            // var result = await _myService.ProcessDataAsync(cancellationToken);

            _logger.LogInformation("SampleDailyJob completed successfully");

            return JobResult.Success("Job completed successfully")
                .WithMetadata("ProcessedItems", 0)
                .WithMetadata("Duration", "1s");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("SampleDailyJob was cancelled");
            return JobResult.Failure("Job was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SampleDailyJob failed");
            return JobResult.Failure($"Job failed: {ex.Message}", ex);
        }
    }
}
