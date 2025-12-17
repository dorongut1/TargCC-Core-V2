using System;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UpayCard.RiskManagement.Infrastructure.Jobs;

/// <summary>
/// Logs job executions to c_LoggedJob table
/// </summary>
public class JobLogger : IJobLogger
{
    private readonly string _connectionString;
    private readonly ILogger<JobLogger> _logger;

    public JobLogger(IConfiguration configuration, ILogger<JobLogger> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string not found");
        _logger = logger;
    }

    public async Task LogJobExecutionAsync(JobExecutionLog log)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var metadataJson = log.Metadata != null
                ? JsonSerializer.Serialize(log.Metadata)
                : null;

            var sql = @"
                INSERT INTO [dbo].[c_LoggedJob] (
                    [JobName],
                    [JobType],
                    [StartedAt],
                    [CompletedAt],
                    [DurationMs],
                    [Success],
                    [ResultMessage],
                    [ErrorMessage],
                    [StackTrace],
                    [ServerName],
                    [TriggeredBy],
                    [MetadataJson]
                )
                VALUES (
                    @JobName,
                    'Recurring',
                    @StartedAt,
                    @CompletedAt,
                    @DurationMs,
                    @Success,
                    @ResultMessage,
                    @ErrorMessage,
                    @StackTrace,
                    @ServerName,
                    'System',
                    @MetadataJson
                )";

            await connection.ExecuteAsync(sql, new
            {
                log.JobName,
                log.StartedAt,
                log.CompletedAt,
                log.DurationMs,
                log.Success,
                log.ResultMessage,
                log.ErrorMessage,
                log.StackTrace,
                ServerName = Environment.MachineName,
                MetadataJson = metadataJson
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log job execution for {JobName}", log.JobName);
            // Don't throw - logging failure shouldn't break the job
        }
    }
}
