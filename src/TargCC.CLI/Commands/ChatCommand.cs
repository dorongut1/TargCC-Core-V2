// <copyright file="ChatCommand.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using System.CommandLine;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using TargCC.AI.Models;
using TargCC.AI.Services;
using TargCC.CLI.Services;

namespace TargCC.CLI.Commands;

/// <summary>
/// Command for interactive chat with the AI assistant.
/// </summary>
public class ChatCommand : Command
{
    private readonly IAIService aiService;
    private readonly IOutputService outputService;
    private readonly ILogger<ChatCommand> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatCommand"/> class.
    /// </summary>
    /// <param name="aiService">AI service.</param>
    /// <param name="outputService">Output service.</param>
    /// <param name="loggerFactory">Logger factory.</param>
    public ChatCommand(
        IAIService aiService,
        IOutputService outputService,
        ILoggerFactory loggerFactory)
        : base("chat", "Start an interactive chat session with AI")
    {
        this.aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        this.outputService = outputService ?? throw new ArgumentNullException(nameof(outputService));
        ArgumentNullException.ThrowIfNull(loggerFactory);
        this.logger = loggerFactory.CreateLogger<ChatCommand>();

        // System message option
        var systemMessageOption = new Option<string?>(
            aliases: ["--system-message", "-s"],
            description: "Custom system message for the AI");

        // Context option
        var contextOption = new Option<string?>(
            aliases: ["--context", "-c"],
            description: "Additional context to provide to the AI");

        // No colors option
        var noColorsOption = new Option<bool>(
            aliases: ["--no-colors"],
            description: "Disable colored output",
            getDefaultValue: () => false);

        this.AddOption(systemMessageOption);
        this.AddOption(contextOption);
        this.AddOption(noColorsOption);

        this.SetHandler(
            this.HandleAsync,
            systemMessageOption,
            contextOption,
            noColorsOption);
    }

    private async Task<int> HandleAsync(
        string? systemMessage,
        string? context,
        bool noColors)
    {
        try
        {
            this.logger.LogInformation("Starting chat session");

            // Check AI service health
            bool isHealthy = await this.aiService.IsHealthyAsync();
            if (!isHealthy)
            {
                this.outputService.Error("AI service is not available. Please check your configuration.");
                return 1;
            }

            // Create conversation context
            var conversationContext = new ConversationContext();

            // Add system message if provided
            if (!string.IsNullOrWhiteSpace(systemMessage))
            {
                conversationContext.AddSystemMessage(systemMessage);
            }
            else
            {
                // Default system message for TargCC context
                conversationContext.AddSystemMessage(
                    "You are an AI assistant helping with TargCC database code generation. " +
                    "Provide helpful advice about database design, code generation, and best practices.");
            }

            // Add initial context if provided
            if (!string.IsNullOrWhiteSpace(context))
            {
                conversationContext.AddSystemMessage($"Context: {context}");
            }

            // Display welcome message
            this.DisplayWelcome(noColors);

            // Start interactive loop
            while (true)
            {
                // Get user input
                var userInput = this.GetUserInput(noColors);

                // Check for exit commands
                if (this.IsExitCommand(userInput))
                {
                    this.DisplayGoodbye(noColors);
                    break;
                }

                // Skip empty input
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    continue;
                }

                // Add user message to context
                conversationContext.AddUserMessage(userInput);

                try
                {
                    // Get AI response
                    AIResponse response = await this.aiService.ChatAsync(
                        userInput,
                        conversationContext,
                        CancellationToken.None);

                    if (response.Success && !string.IsNullOrWhiteSpace(response.Content))
                    {
                        // Add AI response to context
                        conversationContext.AddAssistantMessage(response.Content);

                        // Display AI response
                        this.DisplayAIResponse(response.Content, noColors);
                    }
                    else
                    {
                        this.outputService.Warning("No response received from AI.");
                        this.logger.LogWarning("Empty or unsuccessful AI response");
                    }
                }
                catch (Exception ex)
                {
                    this.outputService.Error($"Error communicating with AI: {ex.Message}");
                    this.logger.LogError(ex, "Error during chat interaction");
                }

                // Add blank line between exchanges
                this.outputService.BlankLine();
            }

            this.logger.LogInformation("Chat session ended");
            return 0;
        }
        catch (Exception ex)
        {
            this.outputService.Error($"Error during chat session: {ex.Message}");
            this.logger.LogError(ex, "Error during chat session");
            return 1;
        }
    }

    private void DisplayWelcome(bool noColors)
    {
        if (!noColors)
        {
            AnsiConsole.Write(
                new FigletText("TargCC AI Chat")
                    .Centered()
                    .Color(Color.Cyan1));
        }

        this.outputService.Info("Type your questions or requests. Commands:");
        this.outputService.Info("  - 'exit' or 'quit' to end the session");
        this.outputService.Info("  - 'clear' to clear conversation history");
        this.outputService.Info("  - 'history' to show conversation history");
        this.outputService.BlankLine();
    }

    private void DisplayGoodbye(bool noColors)
    {
        this.outputService.BlankLine();
        if (!noColors)
        {
            AnsiConsole.MarkupLine("[green]✓[/] Chat session ended. Goodbye!");
        }
        else
        {
            Console.WriteLine("✓ Chat session ended. Goodbye!");
        }
    }

    private string GetUserInput(bool noColors)
    {
        if (!noColors)
        {
            return AnsiConsole.Ask<string>("[cyan]You:[/]");
        }
        else
        {
            Console.Write("You: ");
            return Console.ReadLine() ?? string.Empty;
        }
    }

    private bool IsExitCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var normalized = input.Trim().ToLowerInvariant();
        return normalized == "exit" || normalized == "quit" || normalized == "bye";
    }

    private void DisplayAIResponse(string content, bool noColors)
    {
        if (!noColors)
        {
            var panel = new Panel(new Markup(this.EscapeMarkup(content)))
            {
                Header = new PanelHeader("AI Assistant", Justify.Left),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green),
                Padding = new Padding(1, 0, 1, 0),
            };

            AnsiConsole.Write(panel);
        }
        else
        {
            Console.WriteLine($"AI: {content}");
        }
    }

    private string EscapeMarkup(string text)
    {
        return text
            .Replace("[", "[[")
            .Replace("]", "]]");
    }
}
