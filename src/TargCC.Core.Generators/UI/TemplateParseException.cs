// <copyright file="TemplateParseException.cs" company="Doron Gut">
// Copyright (c) Doron Gut. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI;

using System;

/// <summary>
/// Exception thrown when template parsing fails.
/// </summary>
public class TemplateParseException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateParseException"/> class.
    /// </summary>
    public TemplateParseException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateParseException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public TemplateParseException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateParseException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TemplateParseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
