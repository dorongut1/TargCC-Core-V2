using System.CommandLine;
using Microsoft.Extensions.Logging;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Generation;

namespace TargCC.CLI.Commands.Generate;

/// <summary>
/// Parent command for all generation commands.
/// </summary>
public class GenerateCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateCommand"/> class.
    /// </summary>
    /// <param name="loggerFactory">Logger factory.</param>
    /// <param name="configService">Configuration service.</param>
    /// <param name="output">Output service.</param>
    /// <param name="generationService">Generation service.</param>
    /// <param name="projectGenerationService">Project generation service.</param>
    public GenerateCommand(
        ILoggerFactory loggerFactory,
        IConfigurationService configService,
        IOutputService output,
        IGenerationService generationService,
        IProjectGenerationService projectGenerationService)
        : base("generate", "Generate code from database schema")
    {
        // Add subcommands
        AddCommand(new GenerateEntityCommand(loggerFactory.CreateLogger<GenerateEntityCommand>(), configService, output, generationService));
        AddCommand(new GenerateSqlCommand(loggerFactory.CreateLogger<GenerateSqlCommand>(), configService, output, generationService));
        AddCommand(new GenerateRepositoryCommand(loggerFactory.CreateLogger<GenerateRepositoryCommand>(), configService, output, generationService));
        AddCommand(new GenerateCqrsCommand(loggerFactory.CreateLogger<GenerateCqrsCommand>(), configService, output, generationService));
        AddCommand(new GenerateApiCommand(loggerFactory.CreateLogger<GenerateApiCommand>(), configService, output, generationService));
        AddCommand(new GenerateAllCommand(loggerFactory.CreateLogger<GenerateAllCommand>(), configService, output, generationService));
        AddCommand(new GenerateProjectCommand(loggerFactory.CreateLogger<GenerateProjectCommand>(), configService, output, projectGenerationService));
    }
}
