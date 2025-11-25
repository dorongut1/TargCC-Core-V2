using System.CommandLine;
using Microsoft.Extensions.Logging;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Generation;

namespace TargCC.CLI.Commands.Generate;

/// <summary>
/// Command to generate entity class for a table.
/// </summary>
public class GenerateEntityCommand : Command
{
    private readonly ILogger _logger;
    private readonly IConfigurationService _configService;
    private readonly IOutputService _output;
    private readonly IGenerationService _generationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateEntityCommand"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="configService">Configuration service.</param>
    /// <param name="output">Output service.</param>
    /// <param name="generationService">Generation service.</param>
    public GenerateEntityCommand(
        ILogger logger,
        IConfigurationService configService,
        IOutputService output,
        IGenerationService generationService)
        : base("entity", "Generate entity class for a table")
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _generationService = generationService ?? throw new ArgumentNullException(nameof(generationService));

        // Arguments
        var tableArgument = new Argument<string>(
            name: "table",
            description: "Name of the database table");

        AddArgument(tableArgument);

        // Options
        var connectionOption = new Option<string?>(
            aliases: new[] { "--connection", "-c" },
            description: "Database connection string");

        var outputOption = new Option<string?>(
            aliases: new[] { "--output", "-o" },
            description: "Output directory");

        var namespaceOption = new Option<string?>(
            aliases: new[] { "--namespace", "-n" },
            description: "Namespace for entity class");

        AddOption(connectionOption);
        AddOption(outputOption);
        AddOption(namespaceOption);

        // Handler
        this.SetHandler(
            ExecuteAsync,
            tableArgument,
            connectionOption,
            outputOption,
            namespaceOption);
    }

    private async Task<int> ExecuteAsync(
        string tableName,
        string? connectionString,
        string? outputDirectory,
        string? @namespace)
    {
        try
        {
            _output.Heading($"Generate Entity: {tableName}");
            _output.BlankLine();

            // Load config
            var config = await _configService.LoadAsync();

            // Use provided values or fall back to config
            connectionString ??= config.ConnectionString;
            outputDirectory ??= config.OutputDirectory ?? Directory.GetCurrentDirectory();
            @namespace ??= config.DefaultNamespace;

            // Validate
            if (string.IsNullOrEmpty(connectionString))
            {
                _output.Error("Connection string is required. Use --connection or configure it with 'targcc config set ConnectionString'");
                return 1;
            }

            // Generate
            GenerationResult result = null!;
            await _output.SpinnerAsync(
                "Generating entity class...",
                async () => 
                {
                    result = await _generationService.GenerateEntityAsync(
                        connectionString,
                        tableName,
                        outputDirectory,
                        @namespace);
                });

            // Show results
            _output.BlankLine();
            if (result.Success)
            {
                _output.Success($"✓ Generated {result.GeneratedFiles.Count} file(s) in {result.Duration.TotalSeconds:F1}s");
                _output.BlankLine();

                foreach (var file in result.GeneratedFiles)
                {
                    _output.Info($"  {file.FileType}: {Path.GetFileName(file.FilePath)} ({file.LineCount} lines)");
                }

                return 0;
            }
            else
            {
                _output.Error($"✗ Generation failed: {result.ErrorMessage}");
                return 1;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in generate entity command");
            _output.Error($"Unexpected error: {ex.Message}");
            return 1;
        }
    }
}
