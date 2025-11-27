// <copyright file="IResponseParser.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.AI.Parsers;

/// <summary>
/// Interface for parsing AI responses.
/// </summary>
/// <typeparam name="T">The type to parse into.</typeparam>
public interface IResponseParser<T>
{
    /// <summary>
    /// Parses a response string into a structured object.
    /// </summary>
    /// <param name="response">The response string to parse.</param>
    /// <returns>The parsed object.</returns>
    T Parse(string response);

    /// <summary>
    /// Tries to parse a response string into a structured object.
    /// </summary>
    /// <param name="response">The response string to parse.</param>
    /// <param name="result">The parsed object if successful.</param>
    /// <returns>True if parsing succeeded, false otherwise.</returns>
    bool TryParse(string response, out T? result);
}
