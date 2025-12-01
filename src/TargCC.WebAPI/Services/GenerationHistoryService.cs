using System.Text.Json;
using TargCC.WebAPI.Models;

namespace TargCC.WebAPI.Services;

/// <summary>
/// Service for managing code generation history using JSON file storage.
/// </summary>
public class GenerationHistoryService : IGenerationHistoryService
{
    private readonly string _historyFilePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private const int MaxEntriesPerTable = 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerationHistoryService"/> class.
    /// </summary>
    public GenerationHistoryService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var targccPath = Path.Combine(appDataPath, "TargCC");
        Directory.CreateDirectory(targccPath);
        _historyFilePath = Path.Combine(targccPath, "generation-history.json");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerationHistoryService"/> class.
    /// Used for testing with custom file path.
    /// </summary>
    /// <param name="logger">The logger (not used but kept for compatibility).</param>
    /// <param name="historyFilePath">Custom path for history file.</param>
    public GenerationHistoryService(Microsoft.Extensions.Logging.ILogger<GenerationHistoryService> logger, string historyFilePath)
    {
        _historyFilePath = historyFilePath;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<GenerationHistory>> GetHistoryAsync(string? tableName = null)
    {
        var allHistory = await LoadHistoryAsync();

        if (string.IsNullOrWhiteSpace(tableName))
        {
            return allHistory.OrderByDescending(h => h.GeneratedAt);
        }

        return allHistory
            .Where(h => h.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(h => h.GeneratedAt);
    }

    /// <inheritdoc />
    public async Task<GenerationHistory?> GetLastGenerationAsync(string tableName)
    {
        var history = await GetHistoryAsync(tableName);
        return history.FirstOrDefault();
    }

    /// <inheritdoc />
    public async Task AddHistoryAsync(GenerationHistory history)
    {
        await _fileLock.WaitAsync();
        try
        {
            var allHistory = (await LoadHistoryAsync()).ToList();

            // Add new history
            allHistory.Add(history);

            // Clean up old entries (keep last 100 per table)
            var groupedByTable = allHistory
                .GroupBy(h => h.TableName, StringComparer.OrdinalIgnoreCase);

            var cleanedHistory = new List<GenerationHistory>();
            foreach (var group in groupedByTable)
            {
                cleanedHistory.AddRange(
                    group.OrderByDescending(h => h.GeneratedAt)
                        .Take(MaxEntriesPerTable)
                );
            }

            await SaveHistoryAsync(cleanedHistory);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task ClearHistoryAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            await SaveHistoryAsync(new List<GenerationHistory>());
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<string> GetGenerationStatusAsync(string tableName)
    {
        var lastGeneration = await GetLastGenerationAsync(tableName);

        if (lastGeneration == null)
        {
            return "Not Generated";
        }

        if (!lastGeneration.Success)
        {
            return "Error";
        }

        // Check if files still exist
        var allFilesExist = lastGeneration.FilesGenerated.All(File.Exists);
        if (!allFilesExist)
        {
            return "Modified";
        }

        return "Generated";
    }

    private async Task<List<GenerationHistory>> LoadHistoryAsync()
    {
        if (!File.Exists(_historyFilePath))
        {
            return new List<GenerationHistory>();
        }

        await _fileLock.WaitAsync();
        try
        {
            var json = await File.ReadAllTextAsync(_historyFilePath);
            var history = JsonSerializer.Deserialize<List<GenerationHistory>>(json);
            return history ?? new List<GenerationHistory>();
        }
        catch (Exception)
        {
            // If file is corrupted, return empty list
            return new List<GenerationHistory>();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task SaveHistoryAsync(IEnumerable<GenerationHistory> history)
    {
        var json = JsonSerializer.Serialize(history, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(_historyFilePath, json);
    }
}
