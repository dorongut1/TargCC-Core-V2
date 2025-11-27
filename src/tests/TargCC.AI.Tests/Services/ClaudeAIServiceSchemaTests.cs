// Copyright (c) TargCC Team. All rights reserved.

using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using TargCC.AI.Configuration;
using TargCC.AI.Services;
using TargCC.Core.Interfaces.Models;
using Xunit;
using TableIndex = TargCC.Core.Interfaces.Models.Index;

namespace TargCC.AI.Tests.Services;

public class ClaudeAIServiceSchemaTests
{
    private readonly Mock<ILogger<ClaudeAIService>> loggerMock;
    private readonly AIConfiguration configuration;

    public ClaudeAIServiceSchemaTests()
    {
        this.loggerMock = new Mock<ILogger<ClaudeAIService>>();
        this.configuration = new AIConfiguration
        {
            Enabled = true,
            Provider = "Claude",
            ApiKey = "test-api-key",
            Model = "claude-3-5-sonnet-20241022",
            ApiEndpoint = "https://api.anthropic.com/v1/messages",
            ApiVersion = "2023-06-01",
            MaxTokens = 4000,
            Temperature = 0.7,
            TimeoutSeconds = 30,
        };
    }

    [Fact]
    public async Task AnalyzeTableSchemaAsync_WithNullTable_ShouldThrowArgumentNullException()
    {
        var httpClient = new HttpClient();
        var options = Options.Create(this.configuration);
        var service = new ClaudeAIService(httpClient, options, this.loggerMock.Object);

        var act = async () => await service.AnalyzeTableSchemaAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AnalyzeTableSchemaAsync_WithValidTable_ShouldCallAPI()
    {
        var table = CreateSampleTable();
        var mockResponse = CreateMockSchemaAnalysisResponse();

        // Debug: Print the mock response
        Console.WriteLine("Mock Response:");
        Console.WriteLine(mockResponse);
        Console.WriteLine("---");

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mockResponse, System.Text.Encoding.UTF8, "application/json"),
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var options = Options.Create(this.configuration);
        var service = new ClaudeAIService(httpClient, options, this.loggerMock.Object);

        var result = await service.AnalyzeTableSchemaAsync(table, CancellationToken.None);

        result.Should().NotBeNull();
        result.TableName.Should().Be("Customer");
        result.QualityScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AnalyzeTableSchemaAsync_WithAPIFailure_ShouldThrowException()
    {
        var table = CreateSampleTable();
        
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("API Error"),
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var options = Options.Create(this.configuration);
        var service = new ClaudeAIService(httpClient, options, this.loggerMock.Object);

        var act = async () => await service.AnalyzeTableSchemaAsync(table, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static Table CreateSampleTable()
    {
        return new Table
        {
            Name = "Customer",
            SchemaName = "dbo",
            Columns = new List<Column>
            {
                new()
                {
                    Name = "ID",
                    DataType = "int",
                    IsNullable = false,
                    IsPrimaryKey = true,
                },
                new()
                {
                    Name = "Name",
                    DataType = "nvarchar",
                    IsNullable = false,
                    MaxLength = 100,
                },
                new()
                {
                    Name = "Email",
                    DataType = "nvarchar",
                    IsNullable = true,
                    MaxLength = 255,
                },
            },
            Indexes = new List<TableIndex>
            {
                new()
                {
                    Name = "PK_Customer",
                    IsUnique = true,
                    IsPrimaryKey = true,
                    ColumnNames = new List<string> { "ID" },
                },
            },
        };
    }

    private static string CreateMockSchemaAnalysisResponse()
    {
        var analysisResult = new
        {
            tableName = "Customer",
            summary = "Well-structured table",
            qualityScore = 85,
            followsTargCCConventions = true,
            strengths = new[] { "Good naming", "Proper indexes" },
            issues = new[] { "Missing ent_CreatedDate" },
            suggestions = new[]
            {
                new
                {
                    severity = "Warning",
                    category = "TargCCConventions",
                    message = "Add temporal column",
                    target = "Customer",
                    recommendedAction = "Add ent_CreatedDate",
                    context = "Tracking creation time",
                },
            },
        };

        // Wrap JSON in code block like Claude would
        var jsonText = $"```json\n{JsonSerializer.Serialize(analysisResult)}\n```";

        var claudeResponse = new
        {
            id = "msg_123",
            type = "message",
            role = "assistant",
            content = new[]
            {
                new
                {
                    type = "text",
                    text = jsonText,
                },
            },
            model = "claude-3-5-sonnet-20241022",
            stop_reason = "end_turn",
            usage = new
            {
                input_tokens = 100,
                output_tokens = 200,
            },
        };

        return JsonSerializer.Serialize(claudeResponse);
    }
}
