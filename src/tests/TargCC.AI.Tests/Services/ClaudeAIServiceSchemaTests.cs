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

    [Fact]
    public async Task GetTableSuggestionsAsync_WithNullTable_ShouldThrowArgumentNullException()
    {
        var httpClient = new HttpClient();
        var options = Options.Create(this.configuration);
        var service = new ClaudeAIService(httpClient, options, this.loggerMock.Object);

        var act = async () => await service.GetTableSuggestionsAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetTableSuggestionsAsync_WithValidTable_ShouldReturnSuggestions()
    {
        var table = CreateSampleTable();
        var mockResponse = CreateMockSchemaAnalysisResponse();

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

        var result = await service.GetTableSuggestionsAsync(table, CancellationToken.None);

        result.Should().NotBeNull();
        result.TableName.Should().Be("Customer");
        result.Suggestions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetTableSuggestionsAsync_ShouldLogInformation()
    {
        var table = CreateSampleTable();
        var mockResponse = CreateMockSchemaAnalysisResponse();

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

        await service.GetTableSuggestionsAsync(table, CancellationToken.None);

        // Verify logging happened
        this.loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting suggestions")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetTableSuggestionsAsync_WithAPIError_ShouldThrowException()
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
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Bad Request"),
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var options = Options.Create(this.configuration);
        var service = new ClaudeAIService(httpClient, options, this.loggerMock.Object);

        var act = async () => await service.GetTableSuggestionsAsync(table, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetTableSuggestionsAsync_WithTableContainingSuggestions_ShouldParseSuggestions()
    {
        var table = new Table
        {
            Name = "Orders",
            SchemaName = "dbo",
            Columns = new List<Column>
            {
                new()
                {
                    Name = "OrderID",
                    DataType = "int",
                    IsNullable = false,
                    IsPrimaryKey = true,
                },
                new()
                {
                    Name = "CustomerID",
                    DataType = "int",
                    IsNullable = false,
                },
                new()
                {
                    Name = "TotalAmount",
                    DataType = "decimal",
                    IsNullable = false,
                },
            },
            Indexes = new List<TableIndex>(),
        };

        var analysisResult = new
        {
            tableName = "Orders",
            summary = "Needs performance improvements",
            qualityScore = 65,
            followsTargCCConventions = false,
            strengths = new[] { "Basic structure" },
            issues = new[] { "Missing indexes", "No temporal columns" },
            suggestions = new[]
            {
                new
                {
                    severity = "Critical",
                    category = "Performance",
                    message = "Add index on CustomerID",
                    target = "CustomerID",
                    recommendedAction = "CREATE INDEX IX_Orders_CustomerID ON Orders(CustomerID)",
                    context = "Foreign key lookup",
                },
                new
                {
                    severity = "Warning",
                    category = "TargCCConventions",
                    message = "Missing temporal columns",
                    target = "Orders",
                    recommendedAction = "Add ent_CreatedDate and ent_ModifiedDate",
                    context = "Audit trail",
                },
            },
        };

        var jsonText = $"```json\n{JsonSerializer.Serialize(analysisResult)}\n```";
        var claudeResponse = new
        {
            id = "msg_456",
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
                input_tokens = 150,
                output_tokens = 250,
            },
        };

        var mockResponse = JsonSerializer.Serialize(claudeResponse);

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

        var result = await service.GetTableSuggestionsAsync(table, CancellationToken.None);

        result.Should().NotBeNull();
        result.TableName.Should().Be("Orders");
        result.Suggestions.Should().HaveCount(2);
        result.Suggestions.Should().Contain(s => s.Category.ToString() == "Performance");
        result.Suggestions.Should().Contain(s => s.Category.ToString() == "TargCCConventions");
    }
}
