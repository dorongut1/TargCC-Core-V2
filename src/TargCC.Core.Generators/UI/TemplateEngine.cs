// <copyright file="TemplateEngine.cs" company="Doron Gut">
// Copyright (c) Doron Gut. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI;

using System;
using System.Collections.Generic;
using System.Globalization;
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
    private static readonly Action<ILogger, string, Exception?> LogTemplateInitialized =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, nameof(LogTemplateInitialized)),
            "TemplateEngine initialized with path: {Path}");

    private static readonly Action<ILogger, string, Exception?> LogTemplateRendered =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(2, nameof(LogTemplateRendered)),
            "Template {TemplateName} rendered successfully");

    private static readonly Action<ILogger, string, Exception?> LogTemplateRenderedAsync =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(3, nameof(LogTemplateRenderedAsync)),
            "Template {TemplateName} rendered successfully (async)");

    private static readonly Action<ILogger, Exception?> LogCacheCleared =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(4, nameof(LogCacheCleared)),
            "Template cache cleared");

    private static readonly Action<ILogger, string, Exception?> LogTemplatePreloaded =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(5, nameof(LogTemplatePreloaded)),
            "Template {TemplateName} preloaded into cache");

    private static readonly Action<ILogger, Exception?> LogAllTemplatesPreloaded =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(6, nameof(LogAllTemplatesPreloaded)),
            "All standard templates preloaded");

    private static readonly Action<ILogger, string, string, Exception?> LogTemplateLoaded =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(7, nameof(LogTemplateLoaded)),
            "Template {TemplateName} loaded and cached from {Path}");

    private static readonly Action<ILogger, string, Exception> LogRenderError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(100, nameof(LogRenderError)),
            "Error rendering template {TemplateName}");

    private static readonly Action<ILogger, string, Exception> LogRenderAsyncError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(101, nameof(LogRenderAsyncError)),
            "Error rendering template {TemplateName} asynchronously");

    private static readonly Action<ILogger, string, Exception> LogPreloadWarning =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(200, nameof(LogPreloadWarning)),
            "Failed to preload template {TemplateName}");

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

        if (this.logger != null)
        {
            LogTemplateInitialized(this.logger, this.templateBasePath, null);
        }
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
            var context = CreateScriptObject(data);

            var result = template.Render(context);

            if (this.logger != null)
            {
                LogTemplateRendered(this.logger, templateName, null);
            }

            return result;
        }
        catch (Exception ex)
        {
            if (this.logger != null)
            {
                LogRenderError(this.logger, templateName, ex);
            }

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
            var context = CreateScriptObject(data);

            var result = await template.RenderAsync(context);

            if (this.logger != null)
            {
                LogTemplateRenderedAsync(this.logger, templateName, null);
            }

            return result;
        }
        catch (Exception ex)
        {
            if (this.logger != null)
            {
                LogRenderAsyncError(this.logger, templateName, ex);
            }

            throw new TemplateRenderException($"Failed to render template '{templateName}' asynchronously", ex);
        }
    }

    /// <summary>
    /// Clears the template cache, forcing templates to be reloaded on next use.
    /// </summary>
    public void ClearCache()
    {
        this.templateCache.Clear();

        if (this.logger != null)
        {
            LogCacheCleared(this.logger, null);
        }
    }

    /// <summary>
    /// Pre-loads a template into cache.
    /// </summary>
    /// <param name="templateName">Name of the template to pre-load.</param>
    public void PreloadTemplate(string templateName)
    {
        ArgumentNullException.ThrowIfNull(templateName);
        this.GetOrLoadTemplate(templateName);

        if (this.logger != null)
        {
            LogTemplatePreloaded(this.logger, templateName, null);
        }
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
                if (this.logger != null)
                {
                    LogPreloadWarning(this.logger, templateName, ex);
                }
            }
        }

        if (this.logger != null)
        {
            LogAllTemplatesPreloaded(this.logger, null);
        }
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

    private static ScriptObject CreateScriptObject(object data)
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
        var template = ParseTemplate(templateContent, templateName);

        this.templateCache[templateName] = template;

        if (this.logger != null)
        {
            LogTemplateLoaded(this.logger, templateName, templatePath, null);
        }

        return template;
    }

    private static Template ParseTemplate(string templateContent, string templateName)
    {
        var template = Template.Parse(templateContent);

        if (template.HasErrors)
        {
            var errors = string.Join(Environment.NewLine, template.Messages);
            throw new TemplateParseException(
                string.Format(CultureInfo.InvariantCulture, "Template '{0}' has errors:{1}{2}", templateName, Environment.NewLine, errors));
        }

        return template;
    }

    private string GetTemplatePath(string templateName)
    {
        var fileName = templateName.EndsWith(".hbs", StringComparison.OrdinalIgnoreCase)
            ? templateName
            : string.Format(CultureInfo.InvariantCulture, "{0}.hbs", templateName);

        return Path.Combine(this.templateBasePath, fileName);
    }
}
