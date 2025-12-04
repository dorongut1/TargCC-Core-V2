using Microsoft.Extensions.Logging;
using TargCC.CLI.Services;
using TargCC.Core.Analyzers.Database;
using TargCC.Core.Generators.Project;
using TargCC.Core.Generators.Project.Models;
using TargCC.Core.Generators.Entities;
using TargCC.Core.Generators.Sql;
using TargCC.Core.Generators.Repositories;
using TargCC.Core.Generators.API;
using TargCC.Core.Generators.Common;
using TargCC.Core.Interfaces.Models;

namespace TargCC.CLI.Services.Generation;

/// <summary>
/// Service for generating complete Clean Architecture projects.
/// </summary>
public class ProjectGenerationService : IProjectGenerationService
{
    private readonly ILogger<ProjectGenerationService> _logger;
    private readonly IOutputService _output;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ISolutionGenerator _solutionGenerator;
    private readonly IProjectStructureGenerator _structureGenerator;
    private readonly IProjectFileGenerator _projectFileGenerator;
    private readonly IProgramCsGenerator _programCsGenerator;
    private readonly IAppSettingsGenerator _appSettingsGenerator;
    private readonly IDependencyInjectionGenerator _diGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectGenerationService"/> class.
    /// </summary>
    public ProjectGenerationService(
        ILogger<ProjectGenerationService> logger,
        IOutputService output,
        ILoggerFactory loggerFactory,
        ISolutionGenerator solutionGenerator,
        IProjectStructureGenerator structureGenerator,
        IProjectFileGenerator projectFileGenerator,
        IProgramCsGenerator programCsGenerator,
        IAppSettingsGenerator appSettingsGenerator,
        IDependencyInjectionGenerator diGenerator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _solutionGenerator = solutionGenerator ?? throw new ArgumentNullException(nameof(solutionGenerator));
        _structureGenerator = structureGenerator ?? throw new ArgumentNullException(nameof(structureGenerator));
        _projectFileGenerator = projectFileGenerator ?? throw new ArgumentNullException(nameof(projectFileGenerator));
        _programCsGenerator = programCsGenerator ?? throw new ArgumentNullException(nameof(programCsGenerator));
        _appSettingsGenerator = appSettingsGenerator ?? throw new ArgumentNullException(nameof(appSettingsGenerator));
        _diGenerator = diGenerator ?? throw new ArgumentNullException(nameof(diGenerator));
    }

    /// <inheritdoc/>
    public async Task GenerateCompleteProjectAsync(
        string databaseName,
        string connectionString,
        string outputDirectory,
        string rootNamespace,
        bool includeTests,
        bool force)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootNamespace);

        try
        {
            _output.Info("Step 1: Analyzing database schema...");

            // Get all tables from database
            var analyzerLogger = _loggerFactory.CreateLogger<DatabaseAnalyzer>();
            var analyzer = new DatabaseAnalyzer(connectionString, analyzerLogger);
            var schema = await analyzer.AnalyzeAsync();
            var tables = schema.Tables.ToList();

            _output.Info($"  ✓ Found {tables.Count} tables");
            _output.BlankLine();

            _output.Info("Step 2: Creating solution structure...");

            // Generate solution structure
            var projectOptions = new ProjectGenerationOptions
            {
                ProjectName = rootNamespace,
                RootNamespace = rootNamespace,
                OutputDirectory = outputDirectory,
                ConnectionString = connectionString,
                IncludeTests = includeTests
            };

            await GenerateSolutionStructureAsync(projectOptions);

            _output.Info("  ✓ Solution structure created!");
            _output.BlankLine();

            _output.Info($"Step 3: Generating from {tables.Count} tables...");

            // Generate for each table
            int totalFiles = 0;
            foreach (var table in tables)
            {
                _output.Info($"  Processing: {table.Name}");
                var filesGenerated = await GenerateForTableAsync(
                    table,
                    schema,
                    outputDirectory,
                    rootNamespace);
                totalFiles += filesGenerated;
            }

            _output.Info($"  ✓ Generated {totalFiles} files from {tables.Count} tables!");
            _output.BlankLine();

            _output.Info("Step 4: Generating support files...");

            // Generate Program.cs
            await GenerateProgramCsAsync(outputDirectory, rootNamespace, connectionString, tables);

            // Generate appsettings.json
            await GenerateAppSettingsAsync(outputDirectory, rootNamespace, connectionString);

            // Generate DI registration
            await GenerateDependencyInjectionAsync(outputDirectory, rootNamespace, tables);

            _output.Info("  ✓ Support files generated!");
            _output.BlankLine();

            _output.Info($"✓ Complete project generated successfully!");
            _output.Info($"  Project: {rootNamespace}");
            _output.Info($"  Tables: {tables.Count}");
            _output.Info($"  Location: {outputDirectory}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate complete project");
            _output.Error($"Generation failed: {ex.Message}");
            throw;
        }
    }

    private async Task GenerateSolutionStructureAsync(ProjectGenerationOptions options)
    {
        // Get project structure first
        var projects = _structureGenerator.GetProjectsForArchitecture(options);

        // Solution file with projects
        var solutionInfo = new SolutionInfo
        {
            Name = options.ProjectName,
            OutputDirectory = options.OutputDirectory,
            Projects = projects.ToList()
        };

        var solutionContent = _solutionGenerator.Generate(solutionInfo);
        var solutionPath = Path.Combine(options.OutputDirectory, $"{options.ProjectName}.sln");
        await File.WriteAllTextAsync(solutionPath, solutionContent);

        _output.Info($"  ✓ {Path.GetFileName(solutionPath)}");

        // Create directories
        var directories = await _structureGenerator.CreateFolderStructureAsync(options);
        _output.Info($"  ✓ Created {directories.Count} directories");

        // Project files (.csproj)
        foreach (var projectInfo in projects)
        {
            var projectContent = _projectFileGenerator.Generate(projectInfo, options);
            var projectPath = Path.Combine(options.OutputDirectory, projectInfo.CsprojPath);
            
            var projectDir = Path.GetDirectoryName(projectPath);
            if (!string.IsNullOrEmpty(projectDir) && !Directory.Exists(projectDir))
            {
                Directory.CreateDirectory(projectDir);
            }
            
            await File.WriteAllTextAsync(projectPath, projectContent);
            _output.Info($"  ✓ {projectInfo.Name}.csproj");
        }
    }

    private async Task SaveFileAsync(string filePath, string content)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, content);
    }

    private async Task<int> GenerateForTableAsync(
        Table table,
        DatabaseSchema schema,
        string outputDirectory,
        string rootNamespace)
    {
        int filesCount = 0;

        // 1. Entity
        var entityGen = new EntityGenerator(_loggerFactory.CreateLogger<EntityGenerator>());
        var entityCode = await entityGen.GenerateAsync(table, schema, $"{rootNamespace}.Domain.Entities");
        var entityPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Domain", "Entities", $"{table.Name}.cs");
        await SaveFileAsync(entityPath, entityCode);
        filesCount++;

        // 2. SQL Stored Procedures
        var sqlGen = new SqlGenerator(_loggerFactory.CreateLogger<SqlGenerator>());
        var sqlCode = await sqlGen.GenerateAsync(table);
        var sqlPath = Path.Combine(outputDirectory, "sql", $"{table.Name}.sql");
        await SaveFileAsync(sqlPath, sqlCode);
        filesCount++;

        // 3. Repository Interface
        var repoInterfaceGen = new RepositoryInterfaceGenerator(_loggerFactory.CreateLogger<RepositoryInterfaceGenerator>());
        var repoInterface = await repoInterfaceGen.GenerateAsync(table, rootNamespace);
        var repoInterfacePath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Domain", "Interfaces", $"I{table.Name}Repository.cs");
        await SaveFileAsync(repoInterfacePath, repoInterface);
        filesCount++;

        // 4. Repository Implementation
        var repoGen = new RepositoryGenerator(_loggerFactory.CreateLogger<RepositoryGenerator>());
        var repoImpl = await repoGen.GenerateAsync(table, rootNamespace);
        var repoImplPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Infrastructure", "Repositories", $"{table.Name}Repository.cs");
        await SaveFileAsync(repoImplPath, repoImpl);
        filesCount++;

        // 5. API Controller
        var apiGen = new ApiControllerGenerator(_loggerFactory.CreateLogger<ApiControllerGenerator>());
        var apiConfig = new ApiGeneratorConfig
        {
            Namespace = $"{rootNamespace}.API",
            GenerateXmlDocumentation = true,
            GenerateSwaggerAttributes = true
        };
        var apiCode = await apiGen.GenerateAsync(table, schema, apiConfig);
        var className = BaseApiGenerator.GetClassName(table.Name);
        var controllerName = $"{CodeGenerationHelpers.MakePlural(className)}Controller";
        var apiPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.API", "Controllers", $"{controllerName}.cs");
        await SaveFileAsync(apiPath, apiCode);
        filesCount++;

        return filesCount;
    }

    private async Task GenerateProgramCsAsync(
        string outputDirectory,
        string rootNamespace,
        string connectionString,
        List<Table> tables)
    {
        var projectInfo = new ProjectInfo
        {
            Name = $"{rootNamespace}.API",
            Type = ProjectType.Api,
            RelativePath = $"src/{rootNamespace}.API",
            Namespace = rootNamespace,
            ConnectionString = connectionString,
            Tables = tables
        };

        var content = _programCsGenerator.Generate(projectInfo);
        var path = Path.Combine(outputDirectory, "src", $"{rootNamespace}.API", "Program.cs");
        await SaveFileAsync(path, content);
        _output.Info("  ✓ Program.cs");
    }

    private async Task GenerateAppSettingsAsync(
        string outputDirectory,
        string rootNamespace,
        string connectionString)
    {
        var projectInfo = new ProjectInfo
        {
            Name = $"{rootNamespace}.API",
            Type = ProjectType.Api,
            RelativePath = $"src/{rootNamespace}.API",
            Namespace = rootNamespace,
            ConnectionString = connectionString
        };

        var content = _appSettingsGenerator.Generate(projectInfo);
        var path = Path.Combine(outputDirectory, "src", $"{rootNamespace}.API", "appsettings.json");
        await SaveFileAsync(path, content);
        _output.Info("  ✓ appsettings.json");
    }

    private async Task GenerateDependencyInjectionAsync(
        string outputDirectory,
        string rootNamespace,
        List<Table> tables)
    {
        var projectInfo = new ProjectInfo
        {
            Name = $"{rootNamespace}.Application",
            Type = ProjectType.Application,
            RelativePath = $"src/{rootNamespace}.Application",
            Namespace = rootNamespace,
            Tables = tables
        };

        // Application DI
        var applicationDI = _diGenerator.GenerateApplicationDI(projectInfo);
        var appDIPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Application", "DependencyInjection.cs");
        await SaveFileAsync(appDIPath, applicationDI);
        _output.Info("  ✓ Application/DependencyInjection.cs");

        // Infrastructure DI
        projectInfo.Name = $"{rootNamespace}.Infrastructure";
        projectInfo.Type = ProjectType.Infrastructure;
        projectInfo.RelativePath = $"src/{rootNamespace}.Infrastructure";

        var infrastructureDI = _diGenerator.GenerateInfrastructureDI(projectInfo);
        var infraDIPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Infrastructure", "DependencyInjection.cs");
        await SaveFileAsync(infraDIPath, infrastructureDI);
        _output.Info("  ✓ Infrastructure/DependencyInjection.cs");
    }
}
