// <copyright file="IDocumentationGenerator.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using System.CommandLine;

namespace TargCC.CLI.Documentation;

/// <summary>
/// Service for generating command documentation.
/// </summary>
public interface IDocumentationGenerator
{
    /// <summary>
    /// Generates markdown documentation for all commands.
    /// </summary>
    /// <param name="rootCommand">The root command.</param>
    /// <returns>Markdown documentation.</returns>
    string GenerateMarkdown(Command rootCommand);

    /// <summary>
    /// Generates markdown documentation for a specific command.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="level">Heading level (default: 2).</param>
    /// <returns>Markdown documentation for the command.</returns>
    string GenerateCommandMarkdown(Command command, int level = 2);

    /// <summary>
    /// Writes documentation to a file.
    /// </summary>
    /// <param name="rootCommand">The root command.</param>
    /// <param name="outputPath">Output file path.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    Task WriteToFileAsync(Command rootCommand, string outputPath);
}
