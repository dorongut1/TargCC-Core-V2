// <copyright file="ITemplateEngine.cs" company="Doron Gut">
// Copyright (c) Doron Gut. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI;

using System.Threading.Tasks;

/// <summary>
/// Interface for template engine that renders templates with data.
/// </summary>
public interface ITemplateEngine
{
    /// <summary>
    /// Renders a template with the provided data.
    /// </summary>
    /// <param name="templateName">Name of the template (without .hbs extension).</param>
    /// <param name="data">Data object to render with the template.</param>
    /// <returns>Rendered template string.</returns>
    string Render(string templateName, object data);

    /// <summary>
    /// Asynchronously renders a template with the provided data.
    /// </summary>
    /// <param name="templateName">Name of the template (without .hbs extension).</param>
    /// <param name="data">Data object to render with the template.</param>
    /// <returns>Task containing rendered template string.</returns>
    Task<string> RenderAsync(string templateName, object data);

    /// <summary>
    /// Clears the template cache, forcing templates to be reloaded on next use.
    /// </summary>
    void ClearCache();

    /// <summary>
    /// Pre-loads a template into cache.
    /// </summary>
    /// <param name="templateName">Name of the template to pre-load.</param>
    void PreloadTemplate(string templateName);

    /// <summary>
    /// Pre-loads all standard templates into cache.
    /// </summary>
    void PreloadAllTemplates();
}
