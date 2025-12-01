using TargCC.WebAPI.Models;

namespace TargCC.WebAPI.Services;

/// <summary>
/// Service for managing code generation history.
/// </summary>
public interface IGenerationHistoryService
{
    /// <summary>
    /// Gets the generation history for all tables or a specific table.
    /// </summary>
    /// <param name="tableName">Optional table name to filter by.</param>
    /// <returns>List of generation history records.</returns>
    Task<IEnumerable<GenerationHistory>> GetHistoryAsync(string? tableName = null);

    /// <summary>
    /// Gets the last generation record for a specific table.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <returns>The last generation record, or null if not found.</returns>
    Task<GenerationHistory?> GetLastGenerationAsync(string tableName);

    /// <summary>
    /// Adds a new generation history record.
    /// </summary>
    /// <param name="history">The history record to add.</param>
    Task AddHistoryAsync(GenerationHistory history);

    /// <summary>
    /// Clears all generation history.
    /// </summary>
    Task ClearHistoryAsync();

    /// <summary>
    /// Gets the generation status for a table.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <returns>Status: "Generated", "Modified", "Not Generated", or "Error".</returns>
    Task<string> GetGenerationStatusAsync(string tableName);
}
