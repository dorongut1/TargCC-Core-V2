using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace UpayCard.RiskManagement.Application.Jobs;

/// <summary>
/// Sample manual job that can only be triggered via API or CLI.
/// Use this for on-demand operations like reports, exports, etc.
/// </summary>
[TargCCJob(JobType.Manual)]
[JobDisplayName("Sample Manual Job")]
[JobDescription("A sample job that can only be triggered manually")]
public class SampleManualJob : ITargCCJob
{
    private readonly ILogger<SampleManualJob> _logger;

    public SampleManualJob(ILogger<SampleManualJob> logger)
    {
        _logger = logger;
    }

    public async Task<JobResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SampleManualJob triggered at {Time}", DateTime.UtcNow);

        try
        {
            // TODO: Replace this with your actual business logic

            // Example: Generate a report
            await Task.Delay(2000, cancellationToken); // Simulate report generation

            // Example: Export data
            // var data = await _repository.GetDataAsync(cancellationToken);
            // await _exportService.ExportToExcelAsync(data, cancellationToken);

            _logger.LogInformation("SampleManualJob completed successfully");

            return JobResult.Success("Manual job completed successfully")
                .WithMetadata("ReportGenerated", true)
                .WithMetadata("FileName", "report.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SampleManualJob failed");
            return JobResult.Failure($"Manual job failed: {ex.Message}", ex);
        }
    }
}
