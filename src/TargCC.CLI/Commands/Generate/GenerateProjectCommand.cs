using System.CommandLine;
using Microsoft.Extensions.Logging;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Generation;
using TargCC.Core.Generators.Project;

namespace TargCC.CLI.Commands.Generate;

/// <summary>
/// Command for generating a complete Clean Architecture project from a database.
/// </summary>
public class GenerateProjectCommand : Command
{
    private readonly ILogger _logger;
    private readonly IConfigurationService _configService;
    private readonly IOutputService _output;
    private readonly IProjectGenerationService _projectGenerationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateProjectCommand"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="configService">Configuration service.</param>
    /// <param name="output">Output service.</param>
    /// <param name="projectGenerationService">Project generation service.</param>
    public GenerateProjectCommand(
        ILogger logger,
        IConfigurationService configService,
        IOutputService output,
        IProjectGenerationService projectGenerationService)
        : base("project", "Generate a complete Clean Architecture project from database")
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _projectGenerationService = projectGenerationService ?? throw new ArgumentNullException(nameof(projectGenerationService));

        var databaseOption = new Option<string>(
            aliases: new[] { "--database", "-d" },
            description: "Database name to generate from")
        {
            IsRequired = true
        };

        var outputOption = new Option<string>(
            aliases: new[] { "--output", "-o" },
            description: "Output directory (default: current directory)",
            getDefaultValue: () => Directory.GetCurrentDirectory());

        var namespaceOption = new Option<string?>(
            aliases: new[] { "--namespace", "-n" },
            description: "Root namespace (default: same as database name)");

        var forceOption = new Option<bool>(
            aliases: new[] { "--force", "-f" },
            description: "Overwrite existing files",
            getDefaultValue: () => false);

        var skipTestsOption = new Option<bool>(
            aliases: new[] { "--skip-tests" },
            description: "Skip test project generation",
            getDefaultValue: () => false);

        AddOption(databaseOption);
        AddOption(outputOption);
        AddOption(namespaceOption);
        AddOption(forceOption);
        AddOption(skipTestsOption);

        this.SetHandler(
            ExecuteAsync,
            databaseOption,
            outputOption,
            namespaceOption,
            forceOption,
            skipTestsOption);
    }

    private async Task<int> ExecuteAsync(
        string database,
        string outputDir,
        string? rootNamespace,
        bool force,
        bool skipTests)
    {
        try
        {
            _output.Heading("Generating Clean Architecture Project");

            // Get configuration
            var config = await _configService.LoadAsync();
            var connectionString = config?.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _output.Error("Connection string not found. Run 'targcc init' first.");
                return 1;
            }

            // Determine namespace
            var ns = rootNamespace ?? database;

            _output.Info($"Database: {database}");
            _output.Info($"Output: {outputDir}");
            _output.Info($"Namespace: {ns}");
            _output.Info($"Include Tests: {!skipTests}");
            _output.BlankLine();

            // Create output directory
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                _output.Success($"Created directory: {outputDir}");
            }

            // Generate project
            await _projectGenerationService.GenerateCompleteProjectAsync(
                database,
                connectionString,
                outputDir,
                ns,
                !skipTests,
                force);

            _output.BlankLine();
            _output.Success("Project generation completed successfully!");

            return 0;
        }
        catch (Exception ex)
        {
            _output.Error($"Error generating project: {ex.Message}");
            _logger.LogError(ex, "Failed to generate project");
            return 1;
        }
    }
}
