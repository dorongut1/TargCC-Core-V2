// <copyright file="EndpointTests.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TargCC.WebAPI.Models.Requests;
using TargCC.WebAPI.Models.Responses;
using Xunit;

namespace TargCC.WebAPI.Tests;

/// <summary>
/// Tests for Web API endpoints.
/// </summary>
public class EndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;
    private readonly HttpClient client;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointTests"/> class.
    /// </summary>
    /// <param name="factory">The web application factory.</param>
    public EndpointTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
        this.client = factory.CreateClient();
    }

    /// <summary>
    /// Tests that health check endpoint returns healthy status.
    /// </summary>
    [Fact]
    public async Task HealthCheck_ReturnsHealthyStatus()
    {
        // Act
        var response = await this.client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    /// <summary>
    /// Tests that system info endpoint returns version information.
    /// </summary>
    [Fact]
    public async Task SystemInfo_ReturnsVersionInfo()
    {
        // Act
        var response = await this.client.GetAsync("/api/system/info");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("2.0.0-beta.1");
    }

    /// <summary>
    /// Tests that schema endpoint requires connection string.
    /// </summary>
    [Fact]
    public async Task GetSchemaTables_WithoutConnectionString_ReturnsBadRequest()
    {
        // Arrange
        var request = new SchemaRequest
        {
            ConnectionString = null,
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/schema/tables", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that generate endpoint requires table names.
    /// </summary>
    [Fact]
    public async Task Generate_WithoutTableNames_ReturnsBadRequest()
    {
        // Arrange
        var request = new GenerateRequest
        {
            TableNames = new List<string>(),
            ConnectionString = "test",
            ProjectPath = "test",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/generate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that generate endpoint requires connection string.
    /// </summary>
    [Fact]
    public async Task Generate_WithoutConnectionString_ReturnsBadRequest()
    {
        // Arrange
        var request = new GenerateRequest
        {
            TableNames = new List<string> { "Customer" },
            ConnectionString = null,
            ProjectPath = "test",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/generate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that generate endpoint requires project path.
    /// </summary>
    [Fact]
    public async Task Generate_WithoutProjectPath_ReturnsBadRequest()
    {
        // Arrange
        var request = new GenerateRequest
        {
            TableNames = new List<string> { "Customer" },
            ConnectionString = "test",
            ProjectPath = null,
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/generate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that security analysis endpoint requires connection string.
    /// </summary>
    [Fact]
    public async Task AnalyzeSecurity_WithoutConnectionString_ReturnsBadRequest()
    {
        // Arrange
        var request = new AnalyzeRequest
        {
            ConnectionString = null,
            TableName = "Customer",
            AnalysisType = "Security",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/analyze/security", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that security analysis endpoint requires table name.
    /// </summary>
    [Fact]
    public async Task AnalyzeSecurity_WithoutTableName_ReturnsBadRequest()
    {
        // Arrange
        var request = new AnalyzeRequest
        {
            ConnectionString = "test",
            TableName = null,
            AnalysisType = "Security",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/analyze/security", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that quality analysis endpoint requires connection string.
    /// </summary>
    [Fact]
    public async Task AnalyzeQuality_WithoutConnectionString_ReturnsBadRequest()
    {
        // Arrange
        var request = new AnalyzeRequest
        {
            ConnectionString = null,
            TableName = "Customer",
            AnalysisType = "Quality",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/analyze/quality", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that quality analysis endpoint requires table name.
    /// </summary>
    [Fact]
    public async Task AnalyzeQuality_WithoutTableName_ReturnsBadRequest()
    {
        // Arrange
        var request = new AnalyzeRequest
        {
            ConnectionString = "test",
            TableName = null,
            AnalysisType = "Quality",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/analyze/quality", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that chat endpoint requires message.
    /// </summary>
    [Fact]
    public async Task Chat_WithoutMessage_ReturnsBadRequest()
    {
        // Arrange
        var request = new ChatRequest
        {
            Message = string.Empty,
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/chat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that Swagger UI is available in development.
    /// </summary>
    [Fact]
    public async Task SwaggerUI_IsAvailable()
    {
        // Act
        var response = await this.client.GetAsync("/swagger/index.html");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that OpenAPI JSON is available.
    /// </summary>
    [Fact]
    public async Task OpenAPI_IsAvailable()
    {
        // Act
        var response = await this.client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("TargCC");
    }
}
