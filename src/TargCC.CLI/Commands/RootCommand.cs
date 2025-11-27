using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.Logging;
using TargCC.AI.Services;
using TargCC.CLI.Commands.Analyze;
using TargCC.CLI.Commands.Generate;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Analysis;
using TargCC.CLI.Services.Generation;
using TargCC.Core.Interfaces;

namespace TargCC.CLI.Commands;

/// <summary>
/// Root command for TargCC CLI.
/// </summary>
public class RootCommand : Command
{
    private readonly ILogger<RootCommand> _logger;
    private readonly IConfigurationService _configService;
    private readonly IOutputService _output;
    private readonly IGenerationService _generationService;
    private readonly IProjectGenerationService _projectGenerationService;
    private readonly IAnalysisService _analysisService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Core.Analyzers.ISchemaChangeDetector _schemaChangeDetector;
    private readonly Core.Services.IGenerationTracker _generationTracker;
    private readonly IAIService _aiService;
    private readonly IDatabaseAnalyzer _databaseAnalyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RootCommand"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="configService">Configuration service.</param>
    /// <param name="output">Output service.</param>
    /// <param name="generationService">Generation service.</param>
    /// <param name="projectGenerationService">Project generation service.</param>
    /// <param name="analysisService">Analysis service.</param>
    /// <param name="loggerFactory">Logger factory.</param>
    /// <param name="schemaChangeDetector">Schema change detector.</param>
    /// <param name="generationTracker">Generation tracker.</param>
    /// <param name="aiService">AI service.</param>
    /// <param name="databaseAnalyzer">Database analyzer.</param>
    public RootCommand(
        ILogger<RootCommand> logger,
        IConfigurationService configService,
        IOutputService output,
        IGenerationService generationService,
        IProjectGenerationService projectGenerationService,
        IAnalysisService analysisService,
        ILoggerFactory loggerFactory,
        Core.Analyzers.ISchemaChangeDetector schemaChangeDetector,
        Core.Services.IGenerationTracker generationTracker,
        IAIService aiService,
        IDatabaseAnalyzer databaseAnalyzer)
        : base("targcc", "TargCC Core V2 - Modern code generation platform")
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _generationService = generationService ?? throw new ArgumentNullException(nameof(generationService));
        _projectGenerationService = projectGenerationService ?? throw new ArgumentNullException(nameof(projectGenerationService));
        _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _schemaChangeDetector = schemaChangeDetector ?? throw new ArgumentNullException(nameof(schemaChangeDetector));
        _generationTracker = generationTracker ?? throw new ArgumentNullException(nameof(generationTracker));
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _databaseAnalyzer = databaseAnalyzer ?? throw new ArgumentNullException(nameof(databaseAnalyzer));

        // Add global options
        var verboseOption = new Option<bool>(
            aliases: new[] { "--verbose", "-v" },
            description: "Enable verbose output");
        AddGlobalOption(verboseOption);

        var configOption = new Option<string?>(
            aliases: new[] { "--config" },
            description: "Path to configuration file (default: targcc.json in current directory)");
        AddGlobalOption(configOption);

        var noColorOption = new Option<bool>(
            aliases: new[] { "--no-color" },
            description: "Disable colored output");
        AddGlobalOption(noColorOption);

        var quietOption = new Option<bool>(
            aliases: new[] { "--quiet", "-q" },
            description: "Minimal output (errors only)");
        AddGlobalOption(quietOption);

        // Add commands
        AddCommand(CreateVersionCommand());
        AddCommand(CreateInitCommand());
        AddCommand(CreateConfigCommand());
        AddCommand(new GenerateCommand(_loggerFactory, _configService, _output, _generationService, _projectGenerationService));
        AddCommand(new AnalyzeCommand(_loggerFactory, _configService, _output, _analysisService));
        AddCommand(new SuggestCommand(_aiService, _databaseAnalyzer, _output, _loggerFactory));
        AddCommand(new WatchCommand(_configService, _schemaChangeDetector, _generationService, _generationTracker, _output, _loggerFactory));

        // Set default handler
        this.SetHandler(() =>
        {
            ShowWelcomeBanner();
            return Task.FromResult(0);
        });
    }

    /// <summary>
    /// Shows welcome banner.
    /// </summary>
    private void ShowWelcomeBanner()
    {
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "2.0.0-beta.1";

        _output.Heading($"TargCC Core V2 - CLI v{version}");
        _output.BlankLine();
        _output.Info("Modern code generation platform");
        _output.BlankLine();
        _output.Info("Usage: targcc [command] [options]");
        _output.BlankLine();
        _output.Info("Commands:");
        _output.Info("  init           Initialize TargCC in current directory");
        _output.Info("  config         Manage configuration");
        _output.Info("  generate       Generate code from database tables");
        _output.Info("  analyze        Analyze database schema and code quality");
        _output.Info("  suggest        Get AI-powered suggestions for improvements");
        _output.Info("  watch          Watch database for changes and auto-regenerate");
        _output.Info("  version        Show version information");
        _output.BlankLine();
        _output.Info("Use 'targcc [command] --help' for more information about a command");
    }

    /// <summary>
    /// Creates version command.
    /// </summary>
    /// <returns>Version command.</returns>
    private Command CreateVersionCommand()
    {
        var command = new Command("version", "Show version information");

        command.SetHandler(() =>
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion ?? "Unknown";
            var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?
                .Version ?? "Unknown";

            _output.Heading("TargCC Core V2 - Version Information");
            _output.BlankLine();
            _output.Info($"Version: {version}");
            _output.Info($"File Version: {fileVersion}");
            _output.Info($"Runtime: {Environment.Version}");
            _output.Info($"Platform: {Environment.OSVersion}");
            _output.BlankLine();

            return Task.FromResult(0);
        });

        return command;
    }

    /// <summary>
    /// Creates init command.
    /// </summary>
    /// <returns>Init command.</returns>
    private Command CreateInitCommand()
    {
        var command = new Command("init", "Initialize TargCC in current directory");

        var forceOption = new Option<bool>(
            aliases: new[] { "--force", "-f" },
            description: "Force initialization even if already initialized");

        command.AddOption(forceOption);

        command.SetHandler(async force =>
        {
            _output.Heading("Initializing TargCC");
            _output.BlankLine();

            // Check if already initialized
            if (_configService.Exists() && !force)
            {
                _output.Warning("TargCC is already initialized");
                _output.Info("Use --force to reinitialize");
                return;
            }

            await _output.SpinnerAsync("Creating configuration...", async () =>
            {
                var config = await _configService.InitializeAsync();
                await Task.Delay(500); // Simulate work
            });

            _output.Success("Configuration created successfully");
            _output.Info($"Config file: {_configService.ConfigFilePath}");
            _output.BlankLine();

            // Prompt for database connection
            var configureNow = _output.Confirm("Would you like to configure database connection now?", true);

            if (configureNow)
            {
                var connectionString = _output.Prompt("Enter connection string:");
                await _configService.SetValueAsync("ConnectionString", connectionString);

                var outputDir = _output.Prompt("Enter output directory:", Directory.GetCurrentDirectory());
                await _configService.SetValueAsync("OutputDirectory", outputDir);

                var defaultNamespace = _output.Prompt("Enter default namespace:", "MyApp");
                await _configService.SetValueAsync("DefaultNamespace", defaultNamespace);

                _output.Success("Configuration saved successfully");
            }

            _output.BlankLine();
            _output.Success("TargCC initialized successfully!");
            _output.Info("Run 'targcc --help' to see available commands");

        }, forceOption);

        return command;
    }

    /// <summary>
    /// Creates config command.
    /// </summary>
    /// <returns>Config command.</returns>
    private Command CreateConfigCommand()
    {
        var command = new Command("config", "Manage configuration");

        // config show
        var showCommand = new Command("show", "Show current configuration");
        showCommand.SetHandler(async () =>
        {
            if (!_configService.Exists())
            {
                _output.Warning("Configuration not found");
                _output.Info("Run 'targcc init' to initialize");
                return;
            }

            var config = await _configService.LoadAsync();

            _output.Heading("Current Configuration");
            _output.BlankLine();

            _output.Info($"[yellow]Connection String:[/] {config.ConnectionString ?? "[not set]"}");
            _output.Info($"[yellow]Output Directory:[/] {config.OutputDirectory ?? "[not set]"}");
            _output.Info($"[yellow]Default Namespace:[/] {config.DefaultNamespace}");
            _output.Info($"[yellow]Clean Architecture:[/] {config.UseCleanArchitecture}");
            _output.Info($"[yellow]Generate CQRS:[/] {config.GenerateCqrs}");
            _output.Info($"[yellow]Generate API Controllers:[/] {config.GenerateApiControllers}");
            _output.Info($"[yellow]Generate Repositories:[/] {config.GenerateRepositories}");
            _output.Info($"[yellow]Generate Stored Procedures:[/] {config.GenerateStoredProcedures}");
            _output.Info($"[yellow]Use Dapper:[/] {config.UseDapper}");
            _output.Info($"[yellow]Generate Validators:[/] {config.GenerateValidators}");
            _output.Info($"[yellow]Log Level:[/] {config.LogLevel}");
            _output.Info($"[yellow]Verbose:[/] {config.Verbose}");

            _output.BlankLine();
            _output.Info($"Config file: {_configService.ConfigFilePath}");
        });

        // config set
        var setCommand = new Command("set", "Set configuration value");
        var keyArg = new Argument<string>("key", "Configuration key");
        var valueArg = new Argument<string>("value", "Configuration value");
        setCommand.AddArgument(keyArg);
        setCommand.AddArgument(valueArg);

        setCommand.SetHandler(async (key, value) =>
        {
            try
            {
                await _configService.SetValueAsync(key, value);
                _output.Success($"Set {key} = {value}");
            }
            catch (Exception ex)
            {
                _output.Error($"Failed to set configuration: {ex.Message}");
                _logger.LogError(ex, "Error setting configuration");
            }
        }, keyArg, valueArg);

        // config reset
        var resetCommand = new Command("reset", "Reset configuration to defaults");
        resetCommand.SetHandler(async () =>
        {
            var confirm = _output.Confirm("Are you sure you want to reset configuration?", false);
            if (confirm)
            {
                await _configService.ResetAsync();
                _output.Success("Configuration reset successfully");
            }
        });

        command.AddCommand(showCommand);
        command.AddCommand(setCommand);
        command.AddCommand(resetCommand);

        return command;
    }
}
