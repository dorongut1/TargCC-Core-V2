// <copyright file="TemplateEngineTests.cs" company="Doron Gut">
// Copyright (c) Doron Gut. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.UI;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Generators.UI;
using Xunit;

/// <summary>
/// Unit tests for TemplateEngine class.
/// </summary>
public sealed class TemplateEngineTests : IDisposable
{
    private readonly string tempTemplatePath;
    private readonly Mock<ILogger<TemplateEngine>> mockLogger;
    private readonly TemplateEngine sut;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateEngineTests"/> class.
    /// </summary>
    public TemplateEngineTests()
    {
        this.tempTemplatePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
        Directory.CreateDirectory(this.tempTemplatePath);

        this.mockLogger = new Mock<ILogger<TemplateEngine>>();
        this.sut = new TemplateEngine(this.mockLogger.Object, this.tempTemplatePath);
    }

    /// <summary>
    /// Test 1: Should render simple template with data successfully.
    /// </summary>
    [Fact]
    public void Render_WithSimpleTemplate_ShouldRenderSuccessfully()
    {
        // Arrange
        var templateContent = "Hello {{ name }}!";
        this.CreateTestTemplate("simple", templateContent);
        var data = new { name = "World" };

        // Act
        var result = this.sut.Render("simple", data);

        // Assert
        result.Should().Be("Hello World!");
    }

    /// <summary>
    /// Test 2: Should render template with complex object successfully.
    /// </summary>
    [Fact]
    public void Render_WithComplexObject_ShouldRenderSuccessfully()
    {
        // Arrange
        var templateContent = "{{~ for prop in properties ~}}\n{{ prop.name }}: {{ prop.type }}\n{{~ end ~}}";
        this.CreateTestTemplate("complex", templateContent);
        var data = new
        {
            properties = new[]
            {
                new { name = "Id", type = "number" },
                new { name = "Name", type = "string" },
            },
        };

        // Act
        var result = this.sut.Render("complex", data);

        // Assert
        result.Should().Contain("Id: number");
        result.Should().Contain("Name: string");
    }

    /// <summary>
    /// Test 3: Should render template with dictionary data.
    /// </summary>
    [Fact]
    public void Render_WithDictionary_ShouldRenderSuccessfully()
    {
        // Arrange
        var templateContent = "Entity: {{ entity_name }}, Count: {{ count }}";
        this.CreateTestTemplate("dict", templateContent);
        var data = new Dictionary<string, object>
        {
            { "entity_name", "User" },
            { "count", 42 },
        };

        // Act
        var result = this.sut.Render("dict", data);

        // Assert
        result.Should().Be("Entity: User, Count: 42");
    }

    /// <summary>
    /// Test 4: Should throw exception when template name is null.
    /// </summary>
    [Fact]
    public void Render_WithNullTemplateName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var data = new { name = "Test" };

        // Act
        Action act = () => this.sut.Render(null!, data);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Test 5: Should throw exception when data is null.
    /// </summary>
    [Fact]
    public void Render_WithNullData_ShouldThrowArgumentNullException()
    {
        // Arrange
        this.CreateTestTemplate("test", "Hello");

        // Act
        Action act = () => this.sut.Render("test", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Test 6: Should throw TemplateRenderException when template file does not exist.
    /// </summary>
    [Fact]
    public void Render_WithNonExistentTemplate_ShouldThrowTemplateRenderException()
    {
        // Arrange
        var data = new { name = "Test" };

        // Act
        Action act = () => this.sut.Render("nonexistent", data);

        // Assert
        act.Should().Throw<TemplateRenderException>()
            .WithInnerException<FileNotFoundException>();
    }

    /// <summary>
    /// Test 7: Should cache templates after first load.
    /// </summary>
    [Fact]
    public void Render_CalledTwice_ShouldCacheTemplate()
    {
        // Arrange
        var templateContent = "Hello {{ name }}!";
        var templatePath = this.CreateTestTemplate("cached", templateContent);
        var data = new { name = "World" };

        // Act
        var result1 = this.sut.Render("cached", data);
        File.Delete(templatePath); // Delete file to prove caching works
        var result2 = this.sut.Render("cached", data);

        // Assert
        result1.Should().Be("Hello World!");
        result2.Should().Be("Hello World!");
    }

    /// <summary>
    /// Test 8: Should clear cache successfully.
    /// </summary>
    [Fact]
    public void ClearCache_AfterRendering_ShouldClearCachedTemplates()
    {
        // Arrange
        var templateContent = "Hello {{ name }}!";
        this.CreateTestTemplate("cleartest", templateContent);
        var data = new { name = "World" };
        this.sut.Render("cleartest", data);

        // Act
        this.sut.ClearCache();

        // Assert - Should require file to exist again after cache clear
        File.Delete(Path.Combine(this.tempTemplatePath, "cleartest.hbs"));
        Action act = () => this.sut.Render("cleartest", data);
        act.Should().Throw<TemplateRenderException>()
            .WithInnerException<FileNotFoundException>();
    }

    /// <summary>
    /// Test 9: Should preload template into cache.
    /// </summary>
    [Fact]
    public void PreloadTemplate_WithValidTemplate_ShouldLoadSuccessfully()
    {
        // Arrange
        var templateContent = "Hello {{ name }}!";
        var templatePath = this.CreateTestTemplate("preload", templateContent);

        // Act
        this.sut.PreloadTemplate("preload");
        File.Delete(templatePath);
        var result = this.sut.Render("preload", new { name = "World" });

        // Assert
        result.Should().Be("Hello World!");
    }

    /// <summary>
    /// Test 10: Should handle template with .hbs extension in name.
    /// </summary>
    [Fact]
    public void Render_WithHbsExtensionInName_ShouldRenderSuccessfully()
    {
        // Arrange
        var templateContent = "Hello {{ name }}!";
        this.CreateTestTemplate("withext", templateContent);
        var data = new { name = "Test" };

        // Act
        var result = this.sut.Render("withext.hbs", data);

        // Assert
        result.Should().Be("Hello Test!");
    }

    /// <summary>
    /// Test 11: Should render template asynchronously.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RenderAsync_WithValidTemplate_ShouldRenderSuccessfully()
    {
        // Arrange
        var templateContent = "Hello {{ name }}!";
        this.CreateTestTemplate("async", templateContent);
        var data = new { name = "Async" };

        // Act
        var result = await this.sut.RenderAsync("async", data);

        // Assert
        result.Should().Be("Hello Async!");
    }

    /// <summary>
    /// Test 12: Should throw TemplateRenderException with TemplateParseException for invalid template syntax.
    /// </summary>
    [Fact]
    public void Render_WithInvalidTemplateSyntax_ShouldThrowTemplateRenderException()
    {
        // Arrange
        var templateContent = "{{ unclosed tag";
        this.CreateTestTemplate("invalid", templateContent);
        var data = new { name = "Test" };

        // Act
        Action act = () => this.sut.Render("invalid", data);

        // Assert
        act.Should().Throw<TemplateRenderException>()
            .WithInnerException<TemplateParseException>();
    }

    /// <summary>
    /// Test 13: Should handle conditional template logic.
    /// </summary>
    [Fact]
    public void Render_WithConditionalLogic_ShouldRenderCorrectly()
    {
        // Arrange
        var templateContent = "{{~ if is_active ~}}Active{{~ else ~}}Inactive{{~ end ~}}";
        this.CreateTestTemplate("conditional", templateContent);
        var activeData = new { is_active = true };
        var inactiveData = new { is_active = false };

        // Act
        var activeResult = this.sut.Render("conditional", activeData);
        var inactiveResult = this.sut.Render("conditional", inactiveData);

        // Assert
        activeResult.Should().Be("Active");
        inactiveResult.Should().Be("Inactive");
    }

    /// <summary>
    /// Test 14: Should handle nested objects in templates.
    /// </summary>
    [Fact]
    public void Render_WithNestedObjects_ShouldRenderSuccessfully()
    {
        // Arrange
        var templateContent = "{{ entity.name }} has {{ entity.properties.count }} properties";
        this.CreateTestTemplate("nested", templateContent);
        var data = new
        {
            entity = new
            {
                name = "User",
                properties = new { count = 5 },
            },
        };

        // Act
        var result = this.sut.Render("nested", data);

        // Assert
        result.Should().Be("User has 5 properties");
    }

    /// <summary>
    /// Test 15: Should preload all standard templates successfully.
    /// </summary>
    [Fact]
    public void PreloadAllTemplates_WithStandardTemplates_ShouldLoadWithoutErrors()
    {
        // Arrange
        var standardTemplates = new[] { "Types", "Api", "Hooks", "EntityForm", "CollectionGrid", "Page" };
        foreach (var templateName in standardTemplates)
        {
            this.CreateTestTemplate(templateName, "{{ test }}");
        }

        // Act
        Action act = () => this.sut.PreloadAllTemplates();

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Disposes resources used by the test class.
    /// </summary>
    public void Dispose()
    {
        if (!this.disposed)
        {
            if (Directory.Exists(this.tempTemplatePath))
            {
                Directory.Delete(this.tempTemplatePath, true);
            }

            this.disposed = true;
        }
    }

    private string CreateTestTemplate(string name, string content)
    {
        var fileName = name.EndsWith(".hbs", StringComparison.OrdinalIgnoreCase)
            ? name
            : string.Format(CultureInfo.InvariantCulture, "{0}.hbs", name);
        var path = Path.Combine(this.tempTemplatePath, fileName);
        File.WriteAllText(path, content);
        return path;
    }
}
