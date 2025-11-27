// <copyright file="IPromptBuilder.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.AI.Prompts;

/// <summary>
/// Interface for building AI prompts.
/// </summary>
public interface IPromptBuilder
{
    /// <summary>
    /// Builds a prompt string.
    /// </summary>
    /// <returns>The constructed prompt.</returns>
    string Build();

    /// <summary>
    /// Gets the system message for the prompt.
    /// </summary>
    /// <returns>The system message.</returns>
    string GetSystemMessage();
}
