// <copyright file="ErrorSuggestionService.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;

namespace TargCC.CLI.Services;

/// <summary>
/// Service for providing helpful error suggestions.
/// </summary>
public class ErrorSuggestionService : IErrorSuggestionService
{
    private readonly ILogger<ErrorSuggestionService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorSuggestionService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public ErrorSuggestionService(ILogger<ErrorSuggestionService> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetSuggestions(string errorMessage, string? context = null)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            return Array.Empty<string>();
        }

        var suggestions = new List<string>();

        // Database connection errors
        if (errorMessage.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
            errorMessage.Contains("cannot connect", StringComparison.OrdinalIgnoreCase) ||
            errorMessage.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        {
            suggestions.AddRange(this.GetDatabaseConnectionSuggestions());
        }

        // Configuration errors
        else if (errorMessage.Contains("configuration", StringComparison.OrdinalIgnoreCase) ||
                 errorMessage.Contains("targcc.json", StringComparison.OrdinalIgnoreCase))
        {
            suggestions.AddRange(this.GetConfigurationSuggestions());
        }

        // File not found
        else if (errorMessage.Contains("file not found", StringComparison.OrdinalIgnoreCase) ||
                 errorMessage.Contains("could not find file", StringComparison.OrdinalIgnoreCase))
        {
            suggestions.AddRange(this.GetFileNotFoundSuggestions(context ?? string.Empty));
        }

        // Table not found
        else if (errorMessage.Contains("table", StringComparison.OrdinalIgnoreCase) &&
                 errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            suggestions.AddRange(this.GetTableNotFoundSuggestions(context ?? string.Empty));
        }

        // Permission errors
        else if (errorMessage.Contains("permission", StringComparison.OrdinalIgnoreCase) ||
                 errorMessage.Contains("access denied", StringComparison.OrdinalIgnoreCase))
        {
            suggestions.Add("Check file/directory permissions");
            suggestions.Add("Try running as administrator");
            suggestions.Add("Verify you have write access to the output directory");
        }

        // General fallback
        if (suggestions.Count == 0)
        {
            suggestions.Add("Check the error message above for details");
            suggestions.Add("Run with --verbose for more information");
            suggestions.Add("Verify your configuration with 'targcc config show'");
        }

        return suggestions;
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetDatabaseConnectionSuggestions()
    {
        return new List<string>
        {
            "Verify the connection string in targcc.json",
            "Check if the database server is running",
            "Ensure your credentials are correct",
            "Check if the database exists",
            "Verify network connectivity to the database server",
            "Check firewall settings",
        };
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetConfigurationSuggestions()
    {
        return new List<string>
        {
            "Run 'targcc init' to create a configuration file",
            "Verify targcc.json exists in the current directory",
            "Check targcc.json for syntax errors",
            "Use 'targcc config show' to view current configuration",
            "Use 'targcc config set <key> <value>' to update settings",
        };
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetFileNotFoundSuggestions(string fileName)
    {
        var suggestions = new List<string>
        {
            "Verify the file path is correct",
            "Check if the file exists",
        };

        if (!string.IsNullOrWhiteSpace(fileName) && fileName.Contains("targcc.json", StringComparison.OrdinalIgnoreCase))
        {
            suggestions.Add("Run 'targcc init' to create a configuration file");
        }

        suggestions.Add("Use absolute paths instead of relative paths");
        suggestions.Add("Check for typos in the file name");

        return suggestions;
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetTableNotFoundSuggestions(string tableName)
    {
        var suggestions = new List<string>
        {
            "Use 'targcc analyze schema' to see available tables",
            "Check if the table name is spelled correctly",
            "Verify the database connection is correct",
        };

        if (!string.IsNullOrWhiteSpace(tableName))
        {
            suggestions.Add($"Search for similar table names containing '{tableName}'");
        }

        suggestions.Add("Check if the table exists in the specified database");
        suggestions.Add("Verify you have permissions to access the table");

        return suggestions;
    }
}
