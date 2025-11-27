// Copyright (c) TargCC Team. All rights reserved.

namespace TargCC.AI.Configuration;

/// <summary>
/// Configuration settings for the AI service.
/// </summary>
public sealed class AIConfiguration
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "AI";

    /// <summary>
    /// Gets or sets a value indicating whether AI features are enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the AI provider to use.
    /// Supported values: "Claude", "OpenAI".
    /// </summary>
    public string Provider { get; set; } = "Claude";

    /// <summary>
    /// Gets or sets the API key for the AI provider.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API endpoint URL.
    /// </summary>
    public string ApiEndpoint { get; set; } = "https://api.anthropic.com/v1/messages";

    /// <summary>
    /// Gets or sets the model to use.
    /// Default: claude-3-5-sonnet-20241022 (Claude 3.5 Sonnet).
    /// </summary>
    public string Model { get; set; } = "claude-3-5-sonnet-20241022";

    /// <summary>
    /// Gets or sets the maximum number of tokens to generate.
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Gets or sets the temperature for response generation (0.0-1.0).
    /// Higher values make output more creative, lower values more deterministic.
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Gets or sets the request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the maximum number of retries on failure.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets a value indicating whether to cache responses.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache duration in minutes.
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the API version header.
    /// </summary>
    public string ApiVersion { get; set; } = "2023-06-01";

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <returns>True if configuration is valid; otherwise, false.</returns>
    public bool IsValid()
    {
        if (!this.Enabled)
        {
            return true; // If disabled, no need to validate
        }

        if (string.IsNullOrWhiteSpace(this.ApiKey))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(this.Provider))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(this.ApiEndpoint))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(this.Model))
        {
            return false;
        }

        if (this.MaxTokens <= 0)
        {
            return false;
        }

        if (this.Temperature < 0.0 || this.Temperature > 1.0)
        {
            return false;
        }

        if (this.TimeoutSeconds <= 0)
        {
            return false;
        }

        if (this.MaxRetries < 0)
        {
            return false;
        }

        return true;
    }
}
