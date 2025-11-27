// Copyright (c) TargCC Team. All rights reserved.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TargCC.AI.Configuration;
using TargCC.AI.Models;
using TargCC.Core.Interfaces.Models;

namespace TargCC.AI.Services;

/// <summary>
/// Implementation of AI service using Claude (Anthropic) API.
/// </summary>
public sealed class ClaudeAIService : IAIService
{
    private readonly HttpClient httpClient;
    private readonly AIConfiguration configuration;
    private readonly ILogger<ClaudeAIService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClaudeAIService"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls.</param>
    /// <param name="configuration">AI configuration.</param>
    /// <param name="logger">Logger instance.</param>
    public ClaudeAIService(
        HttpClient httpClient,
        IOptions<AIConfiguration> configuration,
        ILogger<ClaudeAIService> logger)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!this.configuration.IsValid())
        {
            throw new InvalidOperationException("AI configuration is invalid. Please check settings.");
        }

        this.ConfigureHttpClient();
    }

    /// <inheritdoc/>
    public async Task<AIResponse> CompleteAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (!this.configuration.Enabled)
        {
            this.logger.LogWarning("AI service is disabled in configuration");
            return AIResponse.CreateFailure("AI service is disabled");
        }

        try
        {
            this.logger.LogInformation("Sending completion request to Claude API");

            var requestBody = this.BuildClaudeRequest(request);
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await this.httpClient.PostAsync(
                this.configuration.ApiEndpoint,
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                this.logger.LogError("Claude API returned error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return AIResponse.CreateFailure($"API error: {response.StatusCode}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var claudeResponse = JsonSerializer.Deserialize<ClaudeApiResponse>(responseJson);

            if (claudeResponse?.Content == null || claudeResponse.Content.Count == 0)
            {
                this.logger.LogError("Claude API returned empty content");
                return AIResponse.CreateFailure("Empty response from API");
            }

            var firstContent = claudeResponse.Content[0];
            var tokenCount = claudeResponse.Usage?.InputTokens + claudeResponse.Usage?.OutputTokens ?? 0;

            this.logger.LogInformation(
                "Completion successful. Tokens used: {Tokens}, Model: {Model}",
                tokenCount,
                claudeResponse.Model);

            return AIResponse.CreateSuccess(
                firstContent.Text ?? string.Empty,
                tokenCount,
                claudeResponse.Model,
                claudeResponse.StopReason);
        }
        catch (HttpRequestException ex)
        {
            this.logger.LogError(ex, "HTTP error calling Claude API");
            return AIResponse.CreateFailure($"Network error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            this.logger.LogError(ex, "Error parsing Claude API response");
            return AIResponse.CreateFailure($"Parse error: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexpected error calling Claude API");
            return AIResponse.CreateFailure($"Unexpected error: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<AIResponse> ChatAsync(
        string message,
        ConversationContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be null or whitespace.", nameof(message));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        this.logger.LogInformation("Sending chat message to Claude API");

        // Add user message to context
        context.AddUserMessage(message);

        // Create request with conversation context
        var request = new AIRequest(
            message,
            context,
            systemMessage: "You are a helpful AI assistant for TargCC, a code generation platform.");

        var response = await this.CompleteAsync(request, cancellationToken);

        // Add assistant response to context
        if (response.Success)
        {
            context.AddAssistantMessage(response.Content);
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<AIResponse> AnalyzeSchemaAsync(string schemaJson, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaJson))
        {
            throw new ArgumentException("Schema JSON cannot be null or whitespace.", nameof(schemaJson));
        }

        this.logger.LogInformation("Analyzing database schema with Claude");

        var prompt = $@"Analyze this database schema and provide insights:

{schemaJson}

Please provide:
1. Overall schema quality assessment
2. Potential issues or concerns
3. Missing indexes or relationships
4. Naming convention issues
5. Security concerns (unencrypted sensitive data)

Format your response as structured JSON.";

        var request = new AIRequest(
            prompt,
            systemMessage: "You are a database expert analyzing schemas for quality and best practices.",
            temperature: 0.3); // Lower temperature for more focused analysis

        return await this.CompleteAsync(request, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<AIResponse> GetSuggestionsAsync(
        string schemaJson,
        string? context = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaJson))
        {
            throw new ArgumentException("Schema JSON cannot be null or whitespace.", nameof(schemaJson));
        }

        this.logger.LogInformation("Getting schema suggestions from Claude");

        var contextSection = string.IsNullOrWhiteSpace(context)
            ? string.Empty
            : $"\nAdditional context: {context}";

        var prompt = $@"Review this database schema and provide actionable suggestions for improvement:

{schemaJson}{contextSection}

Provide specific, prioritized suggestions for:
1. Performance optimizations (indexes, etc.)
2. Security improvements
3. Data integrity (constraints, relationships)
4. Naming conventions
5. Best practices

Focus on the most impactful improvements.";

        var request = new AIRequest(
            prompt,
            systemMessage: "You are a senior database architect providing optimization suggestions.",
            temperature: 0.5);

        return await this.CompleteAsync(request, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<AIResponse> AnalyzeSecurityAsync(
        string code,
        string language,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or whitespace.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(language))
        {
            throw new ArgumentException("Language cannot be null or whitespace.", nameof(language));
        }

        this.logger.LogInformation("Analyzing code security with Claude");

        var prompt = $@"Analyze this {language} code for security vulnerabilities:

```{language}
{code}
```

Identify:
1. SQL injection risks
2. XSS vulnerabilities
3. Authentication/authorization issues
4. Data exposure risks
5. Input validation problems

Provide specific line numbers and remediation suggestions.";

        var request = new AIRequest(
            prompt,
            systemMessage: "You are a security expert performing code review.",
            temperature: 0.2); // Very low temperature for precise analysis

        return await this.CompleteAsync(request, cancellationToken);
    }

    /// <summary>
    /// Analyzes a table schema and provides structured suggestions.
    /// </summary>
    /// <param name="table">The table definition to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A structured analysis result with suggestions.</returns>
    public async Task<SchemaAnalysisResult> AnalyzeTableSchemaAsync(
        Table table,
        CancellationToken cancellationToken = default)
    {
        if (table == null)
        {
            throw new ArgumentNullException(nameof(table));
        }

        this.logger.LogInformation("Analyzing table schema: {TableName}", table.Name);

        // Build the prompt using SchemaAnalysisPromptBuilder
        var promptBuilder = new Prompts.SchemaAnalysisPromptBuilder(table);
        var systemMessage = promptBuilder.GetSystemMessage();
        var userMessage = promptBuilder.Build();

        // Create AI request
        var request = new AIRequest(
            userMessage,
            systemMessage: systemMessage,
            temperature: 0.3); // Lower temperature for structured output

        // Call AI service
        var response = await this.CompleteAsync(request, cancellationToken);

        if (!response.Success)
        {
            this.logger.LogError("Failed to analyze schema: {Error}", response.ErrorMessage);
            throw new InvalidOperationException($"Schema analysis failed: {response.ErrorMessage}");
        }

        // Parse the response
        var parser = new Parsers.SchemaAnalysisParser();
        var result = parser.Parse(response.Content);

        this.logger.LogInformation(
            "Schema analysis completed for {TableName}. Quality Score: {Score}, Suggestions: {Count}",
            table.Name,
            result.QualityScore,
            result.Suggestions.Count);

        return result;
    }

    /// <summary>
    /// Gets structured suggestions for a specific table.
    /// </summary>
    /// <param name="table">The table to get suggestions for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The structured schema analysis result with suggestions.</returns>
    public async Task<SchemaAnalysisResult> GetTableSuggestionsAsync(
        Table table,
        CancellationToken cancellationToken = default)
    {
        if (table == null)
        {
            throw new ArgumentNullException(nameof(table));
        }

        this.logger.LogInformation("Getting suggestions for table: {TableName}", table.Name);

        // Use AnalyzeTableSchemaAsync as it provides structured suggestions
        var result = await this.AnalyzeTableSchemaAsync(table, cancellationToken);

        // Filter to show only suggestions (not the full analysis)
        this.logger.LogInformation(
            "Retrieved {Count} suggestions for {TableName}",
            result.Suggestions.Count,
            table.Name);

        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            this.logger.LogInformation("Checking Claude API health");

            // Simple test request
            var request = new AIRequest("Hi", maxTokens: 10, temperature: 0.5);
            var response = await this.CompleteAsync(request, cancellationToken);

            var isHealthy = response.Success;
            this.logger.LogInformation("Claude API health check: {Status}", isHealthy ? "Healthy" : "Unhealthy");

            return isHealthy;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Health check failed");
            return false;
        }
    }

    private void ConfigureHttpClient()
    {
        this.httpClient.DefaultRequestHeaders.Clear();
        this.httpClient.DefaultRequestHeaders.Add("x-api-key", this.configuration.ApiKey);
        this.httpClient.DefaultRequestHeaders.Add("anthropic-version", this.configuration.ApiVersion);
        this.httpClient.Timeout = TimeSpan.FromSeconds(this.configuration.TimeoutSeconds);
    }

    private object BuildClaudeRequest(AIRequest request)
    {
        var messages = new List<object>();

        // Add conversation history if present
        if (request.Context?.HasMessages == true)
        {
            foreach (var msg in request.Context.Messages)
            {
                messages.Add(new { role = msg.Role, content = msg.Content });
            }
        }

        // Add current prompt
        messages.Add(new { role = "user", content = request.Prompt });

        var claudeRequest = new
        {
            model = this.configuration.Model,
            max_tokens = request.MaxTokens,
            temperature = request.Temperature,
            messages,
        };

        // Add system message if present
        if (!string.IsNullOrWhiteSpace(request.SystemMessage))
        {
            return new
            {
                claudeRequest.model,
                claudeRequest.max_tokens,
                claudeRequest.temperature,
                claudeRequest.messages,
                system = request.SystemMessage,
            };
        }

        return claudeRequest;
    }

    // Claude API response models
    private sealed class ClaudeApiResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public List<ContentBlock> Content { get; set; } = new();

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("stop_reason")]
        public string? StopReason { get; set; }

        [JsonPropertyName("usage")]
        public UsageInfo? Usage { get; set; }
    }

    private sealed class ContentBlock
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private sealed class UsageInfo
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }
    }
}
