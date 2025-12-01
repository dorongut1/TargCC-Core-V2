// <copyright file="TemplateRenderException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI;

using System;

/// <summary>
/// Exception thrown when template rendering fails.
/// </summary>
public class TemplateRenderException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateRenderException"/> class.
    /// </summary>
    public TemplateRenderException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateRenderException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public TemplateRenderException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateRenderException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TemplateRenderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
