using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace TargCC.CLI.Services;

/// <summary>
/// Service for console output with colors and formatting using Spectre.Console.
/// </summary>
public class OutputService : IOutputService
{
    private readonly ILogger<OutputService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutputService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public OutputService(ILogger<OutputService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void Success(string message)
    {
        AnsiConsole.MarkupLine($"[green]✓[/] {Markup.Escape(message)}");
        _logger.LogInformation(message);
    }

    /// <inheritdoc />
    public void Error(string message)
    {
        AnsiConsole.MarkupLine($"[red]✗[/] {Markup.Escape(message)}");
        _logger.LogError(message);
    }

    /// <inheritdoc />
    public void Warning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]⚠[/] {Markup.Escape(message)}");
        _logger.LogWarning(message);
    }

    /// <inheritdoc />
    public void Info(string message)
    {
        AnsiConsole.MarkupLine($"[blue]ℹ[/] {Markup.Escape(message)}");
        _logger.LogInformation(message);
    }

    /// <inheritdoc />
    public void Debug(string message)
    {
        AnsiConsole.MarkupLine($"[grey]🐛[/] [grey]{Markup.Escape(message)}[/]");
        _logger.LogDebug(message);
    }

    /// <inheritdoc />
    public void Heading(string heading)
    {
        AnsiConsole.Write(new Rule($"[bold blue]{heading}[/]").LeftJustified());
        _logger.LogInformation("Heading: {Heading}", heading);
    }

    /// <inheritdoc />
    public void BlankLine()
    {
        AnsiConsole.WriteLine();
    }

    /// <inheritdoc />
    public void Table(Table table)
    {
        AnsiConsole.Write(table);
    }

    /// <inheritdoc />
    public async Task ProgressAsync(Func<ProgressContext, Task> action)
    {
        await AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn())
            .StartAsync(action);
    }

    /// <inheritdoc />
    public async Task SpinnerAsync(string status, Func<Task> action)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(status, async ctx =>
            {
                await action();
            });
    }

    /// <inheritdoc />
    public bool Confirm(string message, bool defaultValue = true)
    {
        return AnsiConsole.Confirm(message, defaultValue);
    }

    /// <inheritdoc />
    public string Prompt(string prompt, string? defaultValue = null)
    {
        var textPrompt = new TextPrompt<string>(prompt);
        
        if (!string.IsNullOrEmpty(defaultValue))
        {
            textPrompt.DefaultValue(defaultValue);
        }

        return AnsiConsole.Prompt(textPrompt);
    }

    /// <inheritdoc />
    public string Select(string title, IEnumerable<string> choices)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(title)
                .AddChoices(choices));
    }
}
