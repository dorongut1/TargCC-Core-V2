using Spectre.Console;

namespace TargCC.CLI.Services;

/// <summary>
/// Service for console output with colors and formatting.
/// </summary>
public interface IOutputService
{
    /// <summary>
    /// Writes a success message.
    /// </summary>
    /// <param name="message">Message to write.</param>
    void Success(string message);

    /// <summary>
    /// Writes an error message.
    /// </summary>
    /// <param name="message">Message to write.</param>
    void Error(string message);

    /// <summary>
    /// Writes a warning message.
    /// </summary>
    /// <param name="message">Message to write.</param>
    void Warning(string message);

    /// <summary>
    /// Writes an info message.
    /// </summary>
    /// <param name="message">Message to write.</param>
    void Info(string message);

    /// <summary>
    /// Writes a debug message.
    /// </summary>
    /// <param name="message">Message to write.</param>
    void Debug(string message);

    /// <summary>
    /// Writes a heading.
    /// </summary>
    /// <param name="heading">Heading text.</param>
    void Heading(string heading);

    /// <summary>
    /// Writes a blank line.
    /// </summary>
    void BlankLine();

    /// <summary>
    /// Writes a table.
    /// </summary>
    /// <param name="table">Table to write.</param>
    void Table(Table table);

    /// <summary>
    /// Shows a progress bar.
    /// </summary>
    /// <param name="action">Action to execute with progress context.</param>
    Task ProgressAsync(Func<ProgressContext, Task> action);

    /// <summary>
    /// Shows a spinner.
    /// </summary>
    /// <param name="status">Status text.</param>
    /// <param name="action">Action to execute.</param>
    Task SpinnerAsync(string status, Func<Task> action);

    /// <summary>
    /// Prompts user for confirmation.
    /// </summary>
    /// <param name="message">Confirmation message.</param>
    /// <param name="defaultValue">Default value.</param>
    /// <returns>True if confirmed, false otherwise.</returns>
    bool Confirm(string message, bool defaultValue = true);

    /// <summary>
    /// Prompts user for text input.
    /// </summary>
    /// <param name="prompt">Prompt text.</param>
    /// <param name="defaultValue">Default value.</param>
    /// <returns>User input.</returns>
    string Prompt(string prompt, string? defaultValue = null);

    /// <summary>
    /// Shows selection menu.
    /// </summary>
    /// <param name="title">Menu title.</param>
    /// <param name="choices">Available choices.</param>
    /// <returns>Selected choice.</returns>
    string Select(string title, IEnumerable<string> choices);
}
