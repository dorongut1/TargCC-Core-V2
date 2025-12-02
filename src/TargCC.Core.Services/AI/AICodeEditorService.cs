// Copyright (c) TargCC Team. All rights reserved.

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TargCC.Core.Services.AI.Models;

namespace TargCC.Core.Services.AI;

/// <summary>
/// Implementation of AI-powered code editing service using Claude AI.
/// </summary>
public partial class AICodeEditorService : IAICodeEditorService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AICodeEditorService> _logger;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly int _maxTokens;

    /// <summary>
    /// Initializes a new instance of the <see cref="AICodeEditorService"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="apiKey">Claude AI API key.</param>
    /// <param name="model">AI model to use (default: claude-sonnet-4-20250514).</param>
    /// <param name="maxTokens">Maximum tokens for AI response (default: 4000).</param>
    public AICodeEditorService(
        HttpClient httpClient,
        ILogger<AICodeEditorService> logger,
        string apiKey,
        string? model = null,
        int maxTokens = 4000)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _model = model ?? "claude-sonnet-4-20250514";
        _maxTokens = maxTokens;

        _httpClient.BaseAddress = new Uri("https://api.anthropic.com/v1/");
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    /// <inheritdoc/>
    public async Task<CodeModificationResult> ModifyCodeAsync(
        string originalCode,
        string instruction,
        ModificationContext context,
        string? conversationId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting code modification for table {TableName} with instruction: {Instruction}",
            context.TableName,
            instruction);

        try
        {
            // Build the prompt with context
            var prompt = BuildModificationPrompt(originalCode, instruction, context);

            // Call Claude AI
            var aiResponse = await CallClaudeAIAsync(prompt, cancellationToken);

            // Parse the response
            var modifiedCode = ExtractCodeFromResponse(aiResponse);
            var explanation = ExtractExplanationFromResponse(aiResponse);

            // Generate diff
            var changes = GenerateDiff(originalCode, modifiedCode);

            // Validate the modified code
            var validation = await ValidateModificationAsync(originalCode, modifiedCode, cancellationToken);

            if (!validation.IsValid)
            {
                _logger.LogWarning(
                    "Code modification validation failed for table {TableName}: {ErrorCount} errors",
                    context.TableName,
                    validation.Errors.Count);

                return CodeModificationResult.CreateFailure(
                    $"Modified code has validation errors: {string.Join(", ", validation.Errors.Select(e => e.Message))}",
                    originalCode);
            }

            var result = CodeModificationResult.CreateSuccess(
                originalCode,
                modifiedCode,
                changes,
                explanation);

            result.ConversationId = conversationId ?? Guid.NewGuid().ToString();
            result.Validation = validation;

            _logger.LogInformation(
                "Code modification completed successfully for table {TableName}: {ChangeCount} changes",
                context.TableName,
                changes.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error modifying code for table {TableName}",
                context.TableName);

            return CodeModificationResult.CreateFailure(
                $"Failed to modify code: {ex.Message}",
                originalCode);
        }
    }

    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateModificationAsync(
        string originalCode,
        string modifiedCode,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Validating modified code");

        var errors = new List<ValidationError>();
        var warnings = new List<ValidationWarning>();

        // Basic syntax validation
        if (string.IsNullOrWhiteSpace(modifiedCode))
        {
            errors.Add(new ValidationError
            {
                Message = "Modified code is empty",
                Severity = ErrorSeverity.Error,
            });
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        // Check for balanced braces
        var openBraces = modifiedCode.Count(c => c == '{');
        var closeBraces = modifiedCode.Count(c => c == '}');
        if (openBraces != closeBraces)
        {
            errors.Add(new ValidationError
            {
                Message = $"Mismatched braces: {openBraces} opening, {closeBraces} closing",
                Severity = ErrorSeverity.Error,
            });
        }

        // Check for balanced parentheses
        var openParens = modifiedCode.Count(c => c == '(');
        var closeParens = modifiedCode.Count(c => c == ')');
        if (openParens != closeParens)
        {
            errors.Add(new ValidationError
            {
                Message = $"Mismatched parentheses: {openParens} opening, {closeParens} closing",
                Severity = ErrorSeverity.Error,
            });
        }

        // Check for balanced brackets
        var openBrackets = modifiedCode.Count(c => c == '[');
        var closeBrackets = modifiedCode.Count(c => c == ']');
        if (openBrackets != closeBrackets)
        {
            errors.Add(new ValidationError
            {
                Message = $"Mismatched brackets: {openBrackets} opening, {closeBrackets} closing",
                Severity = ErrorSeverity.Error,
            });
        }

        // Check for basic React/TypeScript patterns
        if (modifiedCode.Contains("export default") && !modifiedCode.Contains("function") && !modifiedCode.Contains("const"))
        {
            warnings.Add(new ValidationWarning
            {
                Message = "Export default without function or const declaration",
            });
        }

        // Check if imports were removed (potential breaking change)
        var originalImportCount = CountImports(originalCode);
        var modifiedImportCount = CountImports(modifiedCode);
        if (modifiedImportCount < originalImportCount)
        {
            warnings.Add(new ValidationWarning
            {
                Message = $"Some imports were removed ({originalImportCount} -> {modifiedImportCount})",
            });
        }

        var hasBreakingChanges = warnings.Any(w => w.Message.Contains("removed"));

        _logger.LogDebug(
            "Validation completed: {ErrorCount} errors, {WarningCount} warnings",
            errors.Count,
            warnings.Count);

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings,
            HasBreakingChanges = hasBreakingChanges,
        };
    }

    /// <inheritdoc/>
    public List<CodeChange> GenerateDiff(string originalCode, string modifiedCode)
    {
        _logger.LogDebug("Generating diff between original and modified code");

        var changes = new List<CodeChange>();
        var originalLines = originalCode.Split('\n');
        var modifiedLines = modifiedCode.Split('\n');

        var maxLines = Math.Max(originalLines.Length, modifiedLines.Length);

        for (int i = 0; i < maxLines; i++)
        {
            var originalLine = i < originalLines.Length ? originalLines[i] : string.Empty;
            var modifiedLine = i < modifiedLines.Length ? modifiedLines[i] : string.Empty;

            if (originalLine != modifiedLine)
            {
                CodeChangeType changeType;
                string description;

                if (string.IsNullOrWhiteSpace(originalLine))
                {
                    changeType = CodeChangeType.Addition;
                    description = "Line added";
                }
                else if (string.IsNullOrWhiteSpace(modifiedLine))
                {
                    changeType = CodeChangeType.Deletion;
                    description = "Line deleted";
                }
                else
                {
                    changeType = CodeChangeType.Modification;
                    description = "Line modified";
                }

                changes.Add(new CodeChange
                {
                    LineNumber = i + 1,
                    Type = changeType,
                    Description = description,
                    OldValue = originalLine,
                    NewValue = modifiedLine,
                });
            }
        }

        _logger.LogDebug("Generated {ChangeCount} changes", changes.Count);

        return changes;
    }

    /// <inheritdoc/>
    public async Task<ModificationContext> BuildCodeContextAsync(
        string tableName,
        string schema = "dbo",
        List<string>? relatedTables = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Building code context for table {TableName}", tableName);

        var context = new ModificationContext
        {
            TableName = tableName,
            Schema = schema,
            RelatedTables = relatedTables ?? new List<string>(),
            ProjectConventions = GetProjectConventions(),
        };

        // TODO: In future, integrate with database analyzer to get actual schema
        // For now, return basic context

        _logger.LogDebug("Code context built for table {TableName}", tableName);

        return await Task.FromResult(context);
    }

    private string BuildModificationPrompt(string originalCode, string instruction, ModificationContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an expert React/TypeScript developer. Modify the following code based on the user's instruction.");
        sb.AppendLine();
        sb.AppendLine("Context:");
        sb.AppendLine($"- Table: {context.TableName}");
        sb.AppendLine($"- Schema: {context.Schema}");

        if (context.RelatedTables.Any())
        {
            sb.AppendLine($"- Related Tables: {string.Join(", ", context.RelatedTables)}");
        }

        if (context.ProjectConventions.Any())
        {
            sb.AppendLine();
            sb.AppendLine("Project Conventions:");
            foreach (var convention in context.ProjectConventions)
            {
                sb.AppendLine($"- {convention.Key}: {convention.Value}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("Original Code:");
        sb.AppendLine("```typescript");
        sb.AppendLine(originalCode);
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine($"User Instruction: {instruction}");
        sb.AppendLine();
        sb.AppendLine("Please provide:");
        sb.AppendLine("1. The modified code in a typescript code block");
        sb.AppendLine("2. A brief explanation of what you changed and why");
        sb.AppendLine();
        sb.AppendLine("Important:");
        sb.AppendLine("- Maintain all existing imports unless explicitly asked to remove them");
        sb.AppendLine("- Keep the code structure consistent with Material-UI and React best practices");
        sb.AppendLine("- Ensure all braces, parentheses, and brackets are properly balanced");
        sb.AppendLine("- Preserve TypeScript types and interfaces");

        return sb.ToString();
    }

    private async Task<string> CallClaudeAIAsync(string prompt, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Calling Claude AI API");

        var request = new
        {
            model = _model,
            max_tokens = _maxTokens,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt,
                },
            },
        };

        var response = await _httpClient.PostAsJsonAsync("messages", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        using var jsonDoc = JsonDocument.Parse(responseContent);

        var content = jsonDoc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString();

        _logger.LogDebug("Claude AI API call completed successfully");

        return content ?? string.Empty;
    }

    private static string ExtractCodeFromResponse(string aiResponse)
    {
        // Extract code from typescript/tsx code blocks
        var codeBlockRegex = TypeScriptCodeBlockRegex();
        var match = codeBlockRegex.Match(aiResponse);

        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Fallback: try any code block
        var anyCodeBlockRegex = AnyCodeBlockRegex();
        match = anyCodeBlockRegex.Match(aiResponse);

        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // If no code block found, return the entire response
        return aiResponse.Trim();
    }

    private static string ExtractExplanationFromResponse(string aiResponse)
    {
        // Extract explanation (text outside code blocks)
        var codeBlockRegex = AnyCodeBlockRegex();
        var explanation = codeBlockRegex.Replace(aiResponse, string.Empty);

        return explanation.Trim();
    }

    private static int CountImports(string code)
    {
        var importRegex = ImportLineRegex();
        return importRegex.Matches(code).Count;
    }

    private static Dictionary<string, string> GetProjectConventions()
    {
        return new Dictionary<string, string>
        {
            { "Framework", "React 19 with TypeScript 5.7" },
            { "UI Library", "Material-UI (MUI)" },
            { "State Management", "React Query + React Context" },
            { "Form Handling", "Formik + Yup validation" },
            { "Styling", "MUI styled-components and sx prop" },
            { "Naming", "PascalCase for components, camelCase for variables" },
            { "File Structure", "Feature-based organization" },
        };
    }

    [GeneratedRegex(@"```(?:typescript|tsx|ts)\s*\n(.*?)\n```", RegexOptions.Singleline)]
    private static partial Regex TypeScriptCodeBlockRegex();

    [GeneratedRegex(@"```.*?\n(.*?)\n```", RegexOptions.Singleline)]
    private static partial Regex AnyCodeBlockRegex();

    [GeneratedRegex(@"^\s*import\s+", RegexOptions.Multiline)]
    private static partial Regex ImportLineRegex();
}
