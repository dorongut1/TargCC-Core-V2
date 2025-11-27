// Copyright (c) TargCC Team. All rights reserved.

namespace TargCC.AI.Models;

/// <summary>
/// Represents a request to the AI service.
/// </summary>
public sealed class AIRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AIRequest"/> class.
    /// </summary>
    /// <param name="prompt">The prompt to send to the AI.</param>
    /// <param name="context">Optional conversation context.</param>
    /// <param name="systemMessage">Optional system message.</param>
    /// <param name="maxTokens">Maximum tokens in response.</param>
    /// <param name="temperature">Temperature for response generation (0.0-1.0).</param>
    public AIRequest(
        string prompt,
        ConversationContext? context = null,
        string? systemMessage = null,
        int maxTokens = 4096,
        double temperature = 0.7)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be null or whitespace.", nameof(prompt));
        }

        if (maxTokens <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxTokens), "Max tokens must be greater than 0.");
        }

        if (temperature < 0.0 || temperature > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(temperature), "Temperature must be between 0.0 and 1.0.");
        }

        this.Prompt = prompt;
        this.Context = context;
        this.SystemMessage = systemMessage;
        this.MaxTokens = maxTokens;
        this.Temperature = temperature;
    }

    /// <summary>
    /// Gets the prompt to send to the AI.
    /// </summary>
    public string Prompt { get; }

    /// <summary>
    /// Gets the conversation context.
    /// </summary>
    public ConversationContext? Context { get; }

    /// <summary>
    /// Gets the system message for the AI.
    /// </summary>
    public string? SystemMessage { get; }

    /// <summary>
    /// Gets the maximum number of tokens in the response.
    /// </summary>
    public int MaxTokens { get; }

    /// <summary>
    /// Gets the temperature for response generation.
    /// Higher values (0.8-1.0) make output more creative/random.
    /// Lower values (0.0-0.3) make output more focused/deterministic.
    /// </summary>
    public double Temperature { get; }
}
