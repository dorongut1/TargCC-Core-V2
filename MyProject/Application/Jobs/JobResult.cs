using System;
using System.Collections.Generic;

namespace UpayCard.RiskManagement.Application.Jobs;

/// <summary>
/// Result of a job execution
/// </summary>
public class JobResult
{
    /// <summary>
    /// Indicates whether the job completed successfully
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message describing the result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata about the job execution
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Creates a successful job result
    /// </summary>
    public static JobResult Success(string message = "Job completed successfully")
    {
        return new JobResult
        {
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// Creates a failed job result
    /// </summary>
    public static JobResult Failure(string message, Exception? exception = null)
    {
        var result = new JobResult
        {
            Success = false,
            Message = message
        };

        if (exception != null)
        {
            result.Metadata["Exception"] = exception.GetType().Name;
            result.Metadata["ExceptionMessage"] = exception.Message;
            result.Metadata["StackTrace"] = exception.StackTrace ?? string.Empty;
        }

        return result;
    }

    /// <summary>
    /// Adds metadata to the result
    /// </summary>
    public JobResult WithMetadata(string key, object value)
    {
        Metadata[key] = value;
        return this;
    }
}
