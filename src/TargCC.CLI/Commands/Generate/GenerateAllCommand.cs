using System.CommandLine;
using Microsoft.Extensions.Logging;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Generation;

namespace TargCC.CLI.Commands.Generate;

/// <summary>
/// Command to generate all code for a table (entity + SQL).
/// </summary>
public class GenerateAllCommand : Command
{
    private readonly ILogger _logger;
    private readonly IConfigurationService _configService;
    private readonly IOutputService _output;
    private readonly IGenerationService _generationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateAllCommand"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="configService">Configuration service.</param>
    /// <param name="output">Output service.</param>
    /// <param name="generationService">Generation service.</param>
    public GenerateAllCommand(
        ILogger logger,
        IConfigurationService configService,
        IOutputService output,
        IGenerationService generationService)
        : base("all", "Generate all code for a table (entity + SQL)")
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
            _output.Heading($"Generate All: {tableName}");
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
                "Generating all code...",
                async () => 
                {
                    result = await _generationService.GenerateAllAsync(
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

                // Group by file type
                foreach (var group in result.GeneratedFiles.GroupBy(f => f.FileType))
                {
                    _output.Info($"  {group.Key}:");
                    foreach (var file in group)
                    {
                        _output.Info($"    ✓ {Path.GetFileName(file.FilePath)} ({file.LineCount} lines)");
                    }
                }

                _output.BlankLine();
                _output.Info($"Output directory: {outputDirectory}");

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
            _logger.LogError(ex, "Error in generate all command");
            _output.Error($"Unexpected error: {ex.Message}");
            return 1;
        }
    }
}
