// <copyright file="TemplateEngine.cs" company="Doron Gut">
// Copyright (c) Doron Gut. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Runtime;

/// <summary>
/// Template engine for rendering React UI components using Scriban templates.
/// </summary>
public class TemplateEngine : ITemplateEngine
{
    private readonly ILogger<TemplateEngine>? logger;
    private readonly Dictionary<string, Template> templateCache = new();
    private readonly string templateBasePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateEngine"/> class.
    /// </summary>
    /// <param name="logger">Optional logger instance.</param>
    /// <param name="templateBasePath">Optional custom template base path.</param>
    public TemplateEngine(ILogger<TemplateEngine>? logger = null, string? templateBasePath = null)
    {
        this.logger = logger;
        this.templateBasePath = templateBasePath ?? GetDefaultTemplatePath();
        this.logger?.LogInformation("TemplateEngine initialized with path: {Path}", this.templateBasePath);
    }

    /// <summary>
    /// Renders a template with the provided data.
    /// </summary>
    /// <param name="templateName">Name of the template (without .hbs extension).</param>
    /// <param name="data">Data object to render with the template.</param>
    /// <returns>Rendered template string.</returns>
    public string Render(string templateName, object data)
    {
        ArgumentNullException.ThrowIfNull(templateName);
        ArgumentNullException.ThrowIfNull(data);

        try
        {
            var template = this.GetOrLoadTemplate(templateName);
            var context = this.CreateScriptObject(data);

            var result = template.Render(context);
            this.logger?.LogDebug("Template {TemplateName} rendered successfully", templateName);

            return result;
        }
        catch (Exception ex)
        {
            this.logger?.LogError(ex, "Error rendering template {TemplateName}", templateName);
            throw new TemplateRenderException($"Failed to render template '{templateName}'", ex);
        }
    }

    /// <summary>
    /// Asynchronously renders a template with the provided data.
    /// </summary>
    /// <param name="templateName">Name of the template (without .hbs extension).</param>
    /// <param name="data">Data object to render with the template.</param>
    /// <returns>Task containing rendered template string.</returns>
    public async Task<string> RenderAsync(string templateName, object data)
    {
        ArgumentNullException.ThrowIfNull(templateName);
        ArgumentNullException.ThrowIfNull(data);

        try
        {
            var template = this.GetOrLoadTemplate(templateName);
            var context = this.CreateScriptObject(data);

            var result = await template.RenderAsync(context);
            this.logger?.LogDebug("Template {TemplateName} rendered successfully (async)", templateName);

            return result;
        }
        catch (Exception ex)
        {
            this.logger?.LogError(ex, "Error rendering template {TemplateName} asynchronously", templateName);
            throw new TemplateRenderException($"Failed to render template '{templateName}' asynchronously", ex);
        }
    }

    /// <summary>
    /// Clears the template cache, forcing templates to be reloaded on next use.
    /// </summary>
    public void ClearCache()
    {
        this.templateCache.Clear();
        this.logger?.LogInformation("Template cache cleared");
    }

    /// <summary>
    /// Pre-loads a template into cache.
    /// </summary>
    /// <param name="templateName">Name of the template to pre-load.</param>
    public void PreloadTemplate(string templateName)
    {
        ArgumentNullException.ThrowIfNull(templateName);
        this.GetOrLoadTemplate(templateName);
        this.logger?.LogDebug("Template {TemplateName} preloaded into cache", templateName);
    }

    /// <summary>
    /// Pre-loads all standard templates into cache.
    /// </summary>
    public void PreloadAllTemplates()
    {
        var standardTemplates = new[] { "Types", "Api", "Hooks", "EntityForm", "CollectionGrid", "Page" };

        foreach (var templateName in standardTemplates)
        {
            try
            {
                this.PreloadTemplate(templateName);
            }
            catch (Exception ex)
            {
                this.logger?.LogWarning(ex, "Failed to preload template {TemplateName}", templateName);
            }
        }

        this.logger?.LogInformation("All standard templates preloaded");
    }

    private static string GetDefaultTemplatePath()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyLocation = assembly.Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

        if (string.IsNullOrEmpty(assemblyDirectory))
        {
            throw new InvalidOperationException("Could not determine assembly directory");
        }

        return Path.Combine(assemblyDirectory, "UI", "Templates");
    }

    private Template GetOrLoadTemplate(string templateName)
    {
        if (this.templateCache.TryGetValue(templateName, out var cachedTemplate))
        {
            return cachedTemplate;
        }

        var templatePath = this.GetTemplatePath(templateName);

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}", templatePath);
        }

        var templateContent = File.ReadAllText(templatePath);
        var template = Template.Parse(templateContent);

        if (template.HasErrors)
        {
            var errors = string.Join(Environment.NewLine, template.Messages);
            throw new TemplateParseException($"Template '{templateName}' has errors:{Environment.NewLine}{errors}");
        }

        this.templateCache[templateName] = template;
        this.logger?.LogDebug("Template {TemplateName} loaded and cached from {Path}", templateName, templatePath);

        return template;
    }

    private string GetTemplatePath(string templateName)
    {
        var fileName = templateName.EndsWith(".hbs", StringComparison.OrdinalIgnoreCase)
            ? templateName
            : $"{templateName}.hbs";

        return Path.Combine(this.templateBasePath, fileName);
    }

    private ScriptObject CreateScriptObject(object data)
    {
        var scriptObject = new ScriptObject();

        if (data is IDictionary<string, object> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                scriptObject.Add(kvp.Key, kvp.Value);
            }
        }
        else
        {
            scriptObject.Import(data, renamer: member => member.Name);
        }

        return scriptObject;
    }
}
