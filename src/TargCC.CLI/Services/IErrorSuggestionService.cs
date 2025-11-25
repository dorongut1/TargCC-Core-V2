// <copyright file="IErrorSuggestionService.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

namespace TargCC.CLI.Services;

/// <summary>
/// Service for providing helpful error suggestions.
/// </summary>
public interface IErrorSuggestionService
{
    /// <summary>
    /// Gets suggestions for a given error.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="context">Additional context (command name, arguments, etc.).</param>
    /// <returns>List of suggestions.</returns>
    IReadOnlyList<string> GetSuggestions(string errorMessage, string? context = null);

    /// <summary>
    /// Gets suggestions for a database connection error.
    /// </summary>
    /// <returns>List of suggestions for database connection issues.</returns>
    IReadOnlyList<string> GetDatabaseConnectionSuggestions();

    /// <summary>
    /// Gets suggestions for a configuration error.
    /// </summary>
    /// <returns>List of suggestions for configuration issues.</returns>
    IReadOnlyList<string> GetConfigurationSuggestions();

    /// <summary>
    /// Gets suggestions for a file not found error.
    /// </summary>
    /// <param name="fileName">The file that was not found.</param>
    /// <returns>List of suggestions for file not found issues.</returns>
    IReadOnlyList<string> GetFileNotFoundSuggestions(string fileName);

    /// <summary>
    /// Gets suggestions for a table not found error.
    /// </summary>
    /// <param name="tableName">The table that was not found.</param>
    /// <returns>List of suggestions for table not found issues.</returns>
    IReadOnlyList<string> GetTableNotFoundSuggestions(string tableName);
}
