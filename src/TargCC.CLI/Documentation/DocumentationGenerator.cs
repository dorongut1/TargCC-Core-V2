// <copyright file="DocumentationGenerator.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using System.CommandLine;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TargCC.CLI.Documentation;

/// <summary>
/// Service for generating command documentation.
/// </summary>
public class DocumentationGenerator : IDocumentationGenerator
{
    private readonly ILogger<DocumentationGenerator> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentationGenerator"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public DocumentationGenerator(ILogger<DocumentationGenerator> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public string GenerateMarkdown(Command rootCommand)
    {
        ArgumentNullException.ThrowIfNull(rootCommand);

        var sb = new StringBuilder();

        // Header
        sb.AppendLine("# TargCC CLI Reference");
        sb.AppendLine();
        sb.AppendLine($"**Description:** {rootCommand.Description}");
        sb.AppendLine();

        // Global Options
        if (rootCommand.Options.Any())
        {
            sb.AppendLine("## Global Options");
            sb.AppendLine();
            foreach (var option in rootCommand.Options)
            {
                var aliases = string.Join(", ", option.Aliases.Select(a => $"`{a}`"));
                sb.AppendLine($"- **{aliases}**: {option.Description}");
            }

            sb.AppendLine();
        }

        // Commands
        sb.AppendLine("## Commands");
        sb.AppendLine();

        foreach (var subCommand in rootCommand.Subcommands)
        {
            sb.AppendLine(this.GenerateCommandMarkdown(subCommand, 3));
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public string GenerateCommandMarkdown(Command command, int level = 2)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sb = new StringBuilder();
        var heading = new string('#', level);

        // Command name and description
        sb.AppendLine($"{heading} {command.Name}");
        sb.AppendLine();
        sb.AppendLine($"**Description:** {command.Description}");
        sb.AppendLine();

        // Arguments
        if (command.Arguments.Any())
        {
            sb.AppendLine("**Arguments:**");
            sb.AppendLine();
            foreach (var argument in command.Arguments)
            {
                sb.AppendLine($"- `{argument.Name}`: {argument.Description}");
            }

            sb.AppendLine();
        }

        // Options
        if (command.Options.Any())
        {
            sb.AppendLine("**Options:**");
            sb.AppendLine();
            foreach (var option in command.Options)
            {
                var aliases = string.Join(", ", option.Aliases.Select(a => $"`{a}`"));
                sb.AppendLine($"- {aliases}: {option.Description}");
            }

            sb.AppendLine();
        }

        // Examples
        var examples = this.GetExamples(command);
        if (examples.Any())
        {
            sb.AppendLine("**Examples:**");
            sb.AppendLine();
            foreach (var example in examples)
            {
                sb.AppendLine($"```bash");
                sb.AppendLine(example);
                sb.AppendLine($"```");
                sb.AppendLine();
            }
        }

        // Subcommands
        if (command.Subcommands.Any())
        {
            sb.AppendLine("**Subcommands:**");
            sb.AppendLine();
            foreach (var subCommand in command.Subcommands)
            {
                sb.AppendLine($"- `{subCommand.Name}`: {subCommand.Description}");
            }

            sb.AppendLine();

            // Recurse into subcommands
            foreach (var subCommand in command.Subcommands)
            {
                sb.AppendLine(this.GenerateCommandMarkdown(subCommand, level + 1));
            }
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public async Task WriteToFileAsync(Command rootCommand, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(rootCommand);
        ArgumentNullException.ThrowIfNull(outputPath);

        try
        {
            var markdown = this.GenerateMarkdown(rootCommand);
            await File.WriteAllTextAsync(outputPath, markdown);
            this.logger.LogInformation("Documentation written to {OutputPath}", outputPath);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error writing documentation to {OutputPath}", outputPath);
            throw;
        }
    }

    /// <summary>
    /// Gets examples from command using reflection.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>List of example command lines.</returns>
    private List<string> GetExamples(Command command)
    {
        var examples = new List<string>();

        // System.CommandLine doesn't have a built-in Examples property,
        // but we can use reflection to check if there's an internal/private collection
        // For now, we'll return an empty list and handle examples manually in documentation
        // In production, you might want to use a custom attribute or property

        return examples;
    }
}
