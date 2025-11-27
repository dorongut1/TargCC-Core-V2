// Copyright (c) TargCC Team. All rights reserved.

namespace TargCC.AI.Models;

/// <summary>
/// Represents a response from the AI service.
/// </summary>
public sealed class AIResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AIResponse"/> class.
    /// </summary>
    /// <param name="content">The AI-generated content.</param>
    /// <param name="success">Whether the request was successful.</param>
    /// <param name="errorMessage">Optional error message if request failed.</param>
    /// <param name="tokensUsed">Number of tokens used in the request.</param>
    /// <param name="model">The AI model used.</param>
    /// <param name="finishReason">The reason the response finished.</param>
    public AIResponse(
        string content,
        bool success,
        string? errorMessage = null,
        int tokensUsed = 0,
        string? model = null,
        string? finishReason = null)
    {
        this.Content = content ?? string.Empty;
        this.Success = success;
        this.ErrorMessage = errorMessage;
        this.TokensUsed = tokensUsed;
        this.Model = model;
        this.FinishReason = finishReason;
        this.Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the AI-generated content.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Gets a value indicating whether the request was successful.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets the error message if the request failed.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Gets the number of tokens used in the request.
    /// </summary>
    public int TokensUsed { get; }

    /// <summary>
    /// Gets the AI model that generated the response.
    /// </summary>
    public string? Model { get; }

    /// <summary>
    /// Gets the reason the response finished (e.g., "stop", "length", "content_filter").
    /// </summary>
    public string? FinishReason { get; }

    /// <summary>
    /// Gets the timestamp when the response was created.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <param name="content">The AI-generated content.</param>
    /// <param name="tokensUsed">Number of tokens used.</param>
    /// <param name="model">The model used.</param>
    /// <param name="finishReason">The finish reason.</param>
    /// <returns>A successful AIResponse.</returns>
    public static AIResponse CreateSuccess(
        string content,
        int tokensUsed = 0,
        string? model = null,
        string? finishReason = null)
    {
        return new AIResponse(content, true, null, tokensUsed, model, finishReason);
    }

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A failed AIResponse.</returns>
    public static AIResponse CreateFailure(string errorMessage)
    {
        return new AIResponse(string.Empty, false, errorMessage);
    }
}
