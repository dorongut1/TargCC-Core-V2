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
using TargCC.Core.Generators.UI;
using TargCC.Core.Generators.UI.Components;
using TargCC.Core.Generators.Jobs;
using TargCC.Core.Generators.CQRS;
using TargCC.Core.Generators.Data;
using TargCC.Core.Interfaces;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Writers;

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
    private readonly JobInfrastructureGenerator _jobInfrastructureGenerator;

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
        IDependencyInjectionGenerator diGenerator,
        IFileWriter fileWriter)
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
        _jobInfrastructureGenerator = new JobInfrastructureGenerator(fileWriter ?? throw new ArgumentNullException(nameof(fileWriter)));
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
            _output.Info("Step 0: Ensuring system tables exist...");

            // Check if c_Enumeration table exists
            var hasSysTables = await CheckSystemTablesExistAsync(connectionString);
            if (!hasSysTables)
            {
                _output.Warning("  System tables not found - creating them automatically...");
                var sysTablesGen = new SystemTablesGenerator(
                    _loggerFactory.CreateLogger<SystemTablesGenerator>());
                var sysTablesSql = await sysTablesGen.GenerateAsync(checkExists: true);

                await ExecuteSqlScriptAsync(connectionString, sysTablesSql);
                _output.Info("  ✓ System tables created successfully!");
            }
            else
            {
                _output.Info("  ✓ System tables already exist");
            }
            _output.BlankLine();

            _output.Info("Step 1: Analyzing database schema...");

            // Get all tables from database
            var analyzerLogger = _loggerFactory.CreateLogger<DatabaseAnalyzer>();
            var analyzer = new DatabaseAnalyzer(connectionString, analyzerLogger);
            var schema = await analyzer.AnalyzeAsync();

            // Filter out tables without primary keys and ccvwComboList views
            var allTables = schema.Tables.ToList();
            var skippedTables = allTables.Where(t =>
                !t.Columns.Any(c => c.IsPrimaryKey) ||
                t.IsComboListView ||
                !t.GenerateUI).ToList();

            var tables = allTables.Where(t =>
                t.Columns.Any(c => c.IsPrimaryKey) &&
                !t.IsComboListView &&
                t.GenerateUI).ToList();

            _output.Info($"  ✓ Found {allTables.Count} tables total");
            _output.Info($"  ✓ Processing {tables.Count} tables (skipping {skippedTables.Count} without PK or ComboList views)");
            _output.Info($"  ✓ Found {schema.Relationships?.Count ?? 0} relationships");

            if (skippedTables.Count > 0)
            {
                _output.Warning($"  Skipped tables: {string.Join(", ", skippedTables.Take(10).Select(t => t.Name))}{(skippedTables.Count > 10 ? "..." : "")}");
            }

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

            _output.Info("Step 2.5: Generating shared models...");

            // Generate shared C# models (PagedResult, PaginationParams, FilterOperator, FilterCriteria, Result)
            var sharedModelsGen = new SharedModelsGenerator(_loggerFactory.CreateLogger<SharedModelsGenerator>());
            var sharedModels = await sharedModelsGen.GenerateAllAsync(rootNamespace);
            foreach (var (fileName, content) in sharedModels)
            {
                // Result.cs goes to Application.Common.Models, others to Domain.Common
                var targetDir = fileName == "Result.cs"
                    ? Path.Combine(outputDirectory, "src", $"{rootNamespace}.Application", "Common", "Models")
                    : Path.Combine(outputDirectory, "src", $"{rootNamespace}.Domain", "Common");

                var sharedModelPath = Path.Combine(targetDir, fileName);
                await SaveFileAsync(sharedModelPath, content);
                _output.Info($"  ✓ {fileName}");
            }

            _output.Info($"  ✓ Generated {sharedModels.Count} shared model files!");
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

            // Generate ApplicationMappingProfile for AutoMapper
            _output.Info("Step 3.4: Generating AutoMapper MappingProfile...");
            var mappingProfileCode = GenerateMappingProfile(tables, rootNamespace);
            var mappingProfilePath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Application", "Mapping", "ApplicationMappingProfile.cs");
            await SaveFileAsync(mappingProfilePath, mappingProfileCode);
            _output.Info("  ✓ ApplicationMappingProfile.cs");
            _output.BlankLine();

            _output.Info("Step 3.5: Generating SQL stored procedures with Master-Detail relationships...");
            _output.Info($"  Schema has {schema.Relationships?.Count ?? 0} relationships for Master-Detail generation");

            // Generate ALL SQL including Master-Detail SPs for entire schema
            var sqlGen = new SqlGenerator(_loggerFactory.CreateLogger<SqlGenerator>());
            var allSql = await sqlGen.GenerateAsync(schema);

            // Generate ccvwComboList Views
            _output.Info("Step 3.6: Generating ccvwComboList Views for dropdown lookups...");
            var comboListGen = new TargCC.Core.Generators.Sql.ComboListViewGenerator(
                _loggerFactory.CreateLogger<TargCC.Core.Generators.Sql.ComboListViewGenerator>());
            var comboListViewsSql = await comboListGen.GenerateAllComboListViewsAsync(schema);

            // Combine all SQL - Views first, then procedures
            var combinedSql = new System.Text.StringBuilder();
            combinedSql.AppendLine(comboListViewsSql);
            combinedSql.AppendLine();
            combinedSql.AppendLine(allSql);

            var sqlPath = Path.Combine(outputDirectory, "sql", "all_procedures.sql");
            await SaveFileAsync(sqlPath, combinedSql.ToString());

            _output.Info($"  ✓ SQL file generated with {combinedSql.Length} characters");
            if (schema.Relationships != null && schema.Relationships.Count > 0)
            {
                _output.Info($"  ✓ Included Master-Detail stored procedures for {schema.Relationships.Count} relationships");
            }
            else
            {
                _output.Warning("  ⚠ No relationships found - Master-Detail SPs were NOT generated!");
            }
            _output.BlankLine();

            _output.Info("Step 3.7: Generating Database Context and Interface...");

            // Generate IApplicationDbContext interface (Application layer)
            var dbContextInterfaceGen = new TargCC.Core.Generators.Data.ApplicationDbContextInterfaceGenerator(
                _loggerFactory.CreateLogger<TargCC.Core.Generators.Data.ApplicationDbContextInterfaceGenerator>());
            var dbContextInterface = await dbContextInterfaceGen.GenerateAsync(schema, rootNamespace);
            var dbContextInterfacePath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Application", "Common", "Interfaces", "IApplicationDbContext.cs");
            await SaveFileAsync(dbContextInterfacePath, dbContextInterface);
            _output.Info("  ✓ IApplicationDbContext.cs");

            // Generate ApplicationDbContext class (Infrastructure layer)
            var dbContextGen = new TargCC.Core.Generators.Data.DbContextGenerator(
                _loggerFactory.CreateLogger<TargCC.Core.Generators.Data.DbContextGenerator>());
            var dbContextCode = await dbContextGen.GenerateAsync(schema, rootNamespace);
            var dbContextPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Infrastructure", "Data", "ApplicationDbContext.cs");
            await SaveFileAsync(dbContextPath, dbContextCode);
            _output.Info("  ✓ ApplicationDbContext.cs");

            _output.Info("  ✓ Database context generated!");
            _output.BlankLine();

            _output.Info("Step 3.8: Generating Enumeration API Controller...");

            // Generate EnumerationsController
            var enumControllerCode = TargCC.Core.Generators.API.EnumerationControllerGenerator.Generate(rootNamespace);
            var enumControllerPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.API", "Controllers", "EnumerationsController.cs");
            await SaveFileAsync(enumControllerPath, enumControllerCode);
            _output.Info("  ✓ EnumerationsController.cs");
            _output.BlankLine();

            _output.Info("Step 4: Generating support files...");

            // Generate Program.cs
            await GenerateProgramCsAsync(outputDirectory, rootNamespace, connectionString, tables);

            // Generate appsettings.json
            await GenerateAppSettingsAsync(outputDirectory, rootNamespace, connectionString);

            // Generate DI registration
            await GenerateDependencyInjectionAsync(outputDirectory, rootNamespace, tables);

            // Generate Dashboard Controller
            await GenerateDashboardControllerAsync(outputDirectory, rootNamespace, tables);

            _output.Info("  ✓ Support files generated!");
            _output.BlankLine();

            _output.Info("Step 5: Generating React Frontend Setup...");

            // Generate React setup files
            await GenerateReactSetupFilesAsync(outputDirectory, rootNamespace, tables, connectionString);

            _output.Info("  ✓ React setup files generated!");
            _output.BlankLine();

            _output.Info("Step 5.5: Generating Report Screens for MN Views...");
            await GenerateViewReportScreensAsync(outputDirectory, rootNamespace, connectionString);
            _output.BlankLine();

            _output.Info("Step 5.6: Generating ComboList API and Hooks...");
            var comboListTables = tables.Select(t => t.Name).ToList();
            var comboListControllerCode = TargCC.Core.Generators.API.ComboListControllerGenerator.Generate(comboListTables, rootNamespace);
            var comboListControllerPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.API", "Controllers", "ComboListController.cs");
            await SaveFileAsync(comboListControllerPath, comboListControllerCode);
            _output.Info("  ✓ ComboListController.cs");

            var comboListHooksCode = TargCC.Core.Generators.React.ComboListHooksGenerator.GenerateAllHooks(comboListTables);
            var comboListHooksPath = Path.Combine(outputDirectory, "client", "src", "hooks", "useComboLists.ts");
            await SaveFileAsync(comboListHooksPath, comboListHooksCode);
            _output.Info("  ✓ useComboLists.ts");
            _output.BlankLine();

            _output.Info("Step 6: Generating Job Scheduler Infrastructure...");

            // Generate job infrastructure
            await GenerateJobInfrastructureAsync(outputDirectory, rootNamespace, databaseName);

            _output.Info("  ✓ Job infrastructure generated!");
            _output.BlankLine();

            _output.Info($"✓ Complete project generated successfully!");
            _output.Info($"  Project: {rootNamespace}");
            _output.Info($"  Tables: {tables.Count}");
            _output.Info($"  Backend: {outputDirectory}/src");
            _output.Info($"  Frontend: {outputDirectory}/client");
            _output.Info($"  Jobs: {outputDirectory}/src/{rootNamespace}.Application/Jobs");
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

        // 2. SQL Stored Procedures - SKIP HERE, will be generated once for entire schema after all tables

        // 3. Repository Interface (with schema for Master-Detail methods)
        var repoInterfaceGen = new RepositoryInterfaceGenerator(_loggerFactory.CreateLogger<RepositoryInterfaceGenerator>());
        var repoInterface = await repoInterfaceGen.GenerateAsync(table, schema, rootNamespace);
        var repoInterfacePath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Domain", "Interfaces", $"I{table.Name}Repository.cs");
        await SaveFileAsync(repoInterfacePath, repoInterface);
        filesCount++;

        // 4. Repository Implementation (with schema for Master-Detail methods)
        var repoGen = new RepositoryGenerator(_loggerFactory.CreateLogger<RepositoryGenerator>());
        var repoImpl = await repoGen.GenerateAsync(table, schema, rootNamespace);
        var repoImplPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Infrastructure", "Repositories", $"{table.Name}Repository.cs");
        await SaveFileAsync(repoImplPath, repoImpl);
        filesCount++;

        // 4.5 CQRS Layer (Queries, Commands, DTOs, Filters, Handlers, Validators)
        var className = BaseApiGenerator.GetClassName(table.Name);
        var pluralName = CodeGenerationHelpers.MakePlural(className);

        // Query Generator - Queries, Handlers, Validators
        var queryGen = new QueryGenerator(_loggerFactory.CreateLogger<QueryGenerator>());
        var queryResults = await queryGen.GenerateAllAsync(table, rootNamespace);

        var queriesDir = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Application", "Features", pluralName, "Queries");
        Directory.CreateDirectory(queriesDir);

        foreach (var queryResult in queryResults)
        {
            // Save Query
            var queryPath = Path.Combine(queriesDir, $"{queryResult.QueryClassName}.cs");
            await SaveFileAsync(queryPath, queryResult.QueryCode);
            filesCount++;

            // Save Handler
            var handlerPath = Path.Combine(queriesDir, $"{queryResult.HandlerClassName}.cs");
            await SaveFileAsync(handlerPath, queryResult.HandlerCode);
            filesCount++;

            // Save Validator
            var validatorPath = Path.Combine(queriesDir, $"{queryResult.ValidatorClassName}.cs");
            await SaveFileAsync(validatorPath, queryResult.ValidatorCode);
            filesCount++;
        }

        // DTO Generator
        var dtoCode = await queryGen.GenerateDtoAsync(table, rootNamespace);
        var dtoPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Application", "DTOs", $"{className}Dto.cs");
        await SaveFileAsync(dtoPath, dtoCode);
        filesCount++;

        // Note: Mapping Profiles are created in a single ApplicationMappingProfile.cs file
        // Individual table mapping profiles are NOT generated to avoid using statement issues

        // Note: Filters class is generated inside GetAllQuery file by QueryGenerator

        // Command Generator - Commands, Handlers, Validators
        var commandGen = new CommandGenerator(_loggerFactory.CreateLogger<CommandGenerator>());
        var commandResults = await commandGen.GenerateAllAsync(table, rootNamespace);

        var commandsDir = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Application", "Features", pluralName, "Commands");
        Directory.CreateDirectory(commandsDir);

        foreach (var cmdResult in commandResults)
        {
            // Save Command
            var commandPath = Path.Combine(commandsDir, $"{cmdResult.CommandClassName}.cs");
            await SaveFileAsync(commandPath, cmdResult.CommandCode);
            filesCount++;

            // Save Handler
            var handlerPath = Path.Combine(commandsDir, $"{cmdResult.HandlerClassName}.cs");
            await SaveFileAsync(handlerPath, cmdResult.HandlerCode);
            filesCount++;

            // Save Validator
            var validatorPath = Path.Combine(commandsDir, $"{cmdResult.ValidatorClassName}.cs");
            await SaveFileAsync(validatorPath, cmdResult.ValidatorCode);
            filesCount++;
        }

        // 5. API Controller
        var apiGen = new ApiControllerGenerator(_loggerFactory.CreateLogger<ApiControllerGenerator>());
        var apiConfig = new ApiGeneratorConfig
        {
            Namespace = $"{rootNamespace}.API",
            GenerateXmlDocumentation = true,
            GenerateSwaggerAttributes = true
        };
        var apiCode = await apiGen.GenerateAsync(table, schema, apiConfig);
        className = BaseApiGenerator.GetClassName(table.Name);
        var controllerName = $"{CodeGenerationHelpers.MakePlural(className)}Controller";
        var apiPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.API", "Controllers", $"{controllerName}.cs");
        await SaveFileAsync(apiPath, apiCode);
        filesCount++;

        // 6. React Frontend Files
        var uiConfig = new UIGeneratorConfig
        {
            OutputDirectory = Path.Combine(outputDirectory, "client"),
            ApiBaseUrl = "http://localhost:5000",
            Framework = UIFramework.MaterialUI
        };

        // TypeScript Types
        var typeGen = new TypeScriptTypeGenerator(_loggerFactory.CreateLogger<TypeScriptTypeGenerator>());
        var typeCode = await typeGen.GenerateAsync(table, schema, uiConfig);
        var typePath = Path.Combine(outputDirectory, "client", "src", "types", $"{className}.types.ts");
        await SaveFileAsync(typePath, typeCode);
        filesCount++;

        // React API Client
        var reactApiGen = new ReactApiGenerator(_loggerFactory.CreateLogger<ReactApiGenerator>());
        var reactApiCode = await reactApiGen.GenerateAsync(table, schema, uiConfig);
        var camelClassName = char.ToLowerInvariant(className[0]) + className.Substring(1);
        var reactApiPath = Path.Combine(outputDirectory, "client", "src", "api", $"{camelClassName}Api.ts");
        await SaveFileAsync(reactApiPath, reactApiCode);
        filesCount++;

        // React Hooks
        var hookGen = new ReactHookGenerator(_loggerFactory.CreateLogger<ReactHookGenerator>());
        var hookCode = await hookGen.GenerateAsync(table, schema, uiConfig);
        var hookPath = Path.Combine(outputDirectory, "client", "src", "hooks", $"use{className}.ts");
        await SaveFileAsync(hookPath, hookCode);
        filesCount++;

        // React Components
        var componentConfig = new ComponentGeneratorConfig
        {
            OutputDirectory = Path.Combine(outputDirectory, "client", "src", "components"),
        };

        var listGen = new ReactListComponentGenerator(_loggerFactory.CreateLogger<ReactListComponentGenerator>());
        var listCode = await listGen.GenerateAsync(table, schema, componentConfig);
        var listPath = Path.Combine(outputDirectory, "client", "src", "components", className, $"{className}List.tsx");
        await SaveFileAsync(listPath, listCode);
        filesCount++;

        // Only generate Form for tables, not for views (views are read-only)
        if (!table.IsView)
        {
            var formGen = new ReactFormComponentGenerator(_loggerFactory.CreateLogger<ReactFormComponentGenerator>());
            var formCode = await formGen.GenerateAsync(table, schema, componentConfig);
            var formPath = Path.Combine(outputDirectory, "client", "src", "components", className, $"{className}Form.tsx");
            await SaveFileAsync(formPath, formCode);
            filesCount++;
        }

        var detailGen = new ReactDetailComponentGenerator(_loggerFactory.CreateLogger<ReactDetailComponentGenerator>());
        var detailCode = await detailGen.GenerateAsync(table, schema, componentConfig);
        var detailPath = Path.Combine(outputDirectory, "client", "src", "components", className, $"{className}Detail.tsx");
        await SaveFileAsync(detailPath, detailCode);
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

        // Generate launchSettings.json
        var launchSettings = GenerateLaunchSettings();
        var launchSettingsPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.API", "Properties", "launchSettings.json");
        await SaveFileAsync(launchSettingsPath, launchSettings);
        _output.Info("  ✓ Properties/launchSettings.json");
    }

    private static string GenerateLaunchSettings()
    {
        return @"{
  ""profiles"": {
    ""http"": {
      ""commandName"": ""Project"",
      ""dotnetRunMessages"": true,
      ""launchBrowser"": true,
      ""launchUrl"": ""swagger"",
      ""applicationUrl"": ""http://localhost:5000"",
      ""environmentVariables"": {
        ""ASPNETCORE_ENVIRONMENT"": ""Development""
      }
    },
    ""https"": {
      ""commandName"": ""Project"",
      ""dotnetRunMessages"": true,
      ""launchBrowser"": true,
      ""launchUrl"": ""swagger"",
      ""applicationUrl"": ""https://localhost:5001;http://localhost:5000"",
      ""environmentVariables"": {
        ""ASPNETCORE_ENVIRONMENT"": ""Development""
      }
    }
  }
}";
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

    private async Task GenerateReactSetupFilesAsync(
        string outputDirectory,
        string rootNamespace,
        List<Table> tables,
        string connectionString)
    {
        var clientDir = Path.Combine(outputDirectory, "client");

        // package.json
        var packageJson = GeneratePackageJson(rootNamespace);
        await SaveFileAsync(Path.Combine(clientDir, "package.json"), packageJson);
        _output.Info("  ✓ package.json");

        // tsconfig.json
        var tsConfig = GenerateTsConfig();
        await SaveFileAsync(Path.Combine(clientDir, "tsconfig.json"), tsConfig);
        _output.Info("  ✓ tsconfig.json");

        // tsconfig.node.json
        var tsConfigNode = GenerateTsConfigNode();
        await SaveFileAsync(Path.Combine(clientDir, "tsconfig.node.json"), tsConfigNode);
        _output.Info("  ✓ tsconfig.node.json");

        // vite.config.ts
        var viteConfig = GenerateViteConfig();
        await SaveFileAsync(Path.Combine(clientDir, "vite.config.ts"), viteConfig);
        _output.Info("  ✓ vite.config.ts");

        // index.html (root of client folder for Vite)
        var indexHtml = GenerateIndexHtml(rootNamespace);
        await SaveFileAsync(Path.Combine(clientDir, "index.html"), indexHtml);
        _output.Info("  ✓ index.html");

        // src/index.tsx
        var indexTsx = GenerateIndexTsx();
        await SaveFileAsync(Path.Combine(clientDir, "src", "index.tsx"), indexTsx);
        _output.Info("  ✓ src/index.tsx");

        // src/App.tsx
        var appTsx = GenerateAppTsx(tables);
        await SaveFileAsync(Path.Combine(clientDir, "src", "App.tsx"), appTsx);
        _output.Info("  ✓ src/App.tsx");

        // src/components/Dashboard/Dashboard.tsx
        var dashboardTsx = GenerateDashboardTsx(tables);
        await SaveFileAsync(Path.Combine(clientDir, "src", "components", "Dashboard", "Dashboard.tsx"), dashboardTsx);
        _output.Info("  ✓ src/components/Dashboard/Dashboard.tsx");

        // src/api/client.ts (axios setup)
        var apiClient = GenerateApiClient();
        await SaveFileAsync(Path.Combine(clientDir, "src", "api", "client.ts"), apiClient);
        _output.Info("  ✓ src/api/client.ts");

        // src/hooks/useEnumValues.ts - Enum loading hooks
        var enumHooksCode = TargCC.Core.Generators.React.EnumHooksGenerator.GenerateUseEnumValuesHook();
        await SaveFileAsync(Path.Combine(clientDir, "src", "hooks", "useEnumValues.ts"), enumHooksCode);
        _output.Info("  ✓ src/hooks/useEnumValues.ts");

        // .gitignore
        var gitignore = GenerateGitignore();
        await SaveFileAsync(Path.Combine(clientDir, ".gitignore"), gitignore);
        _output.Info("  ✓ .gitignore");

        // README.md
        var readme = GenerateReactReadme(rootNamespace);
        await SaveFileAsync(Path.Combine(clientDir, "README.md"), readme);
        _output.Info("  ✓ README.md");

        // src/types/common.types.ts - Shared TypeScript types for pagination and filtering
        var commonTypes = await TypeScriptTypeGenerator.GenerateCommonTypesAsync();
        await SaveFileAsync(Path.Combine(clientDir, "src", "types", "common.types.ts"), commonTypes);
        _output.Info("  ✓ src/types/common.types.ts");

        // src/enums/index.ts - TypeScript enums from c_Enumeration
        await GenerateEnumsAsync(outputDirectory, connectionString);
    }

    private static string GeneratePackageJson(string projectName)
    {
        return $$"""
        {
          "name": "{{projectName.ToLowerInvariant()}}-client",
          "version": "0.1.0",
          "private": true,
          "dependencies": {
            "@emotion/react": "^11.14.0",
            "@emotion/styled": "^11.14.0",
            "@mui/icons-material": "^6.3.0",
            "@mui/material": "^6.3.0",
            "@mui/x-data-grid": "^7.24.0",
            "@tanstack/react-query": "^5.62.0",
            "axios": "^1.7.9",
            "date-fns": "^3.0.0",
            "exceljs": "^4.4.0",
            "react": "^18.3.1",
            "react-dom": "^18.3.1",
            "react-hook-form": "^7.54.0",
            "react-router-dom": "^7.1.1",
            "recharts": "^2.10.0"
          },
          "devDependencies": {
            "@types/node": "^22.10.2",
            "@types/react": "^18.3.18",
            "@types/react-dom": "^18.3.5",
            "@vitejs/plugin-react": "^4.3.4",
            "typescript": "^5.7.2",
            "vite": "^6.2.6"
          },
          "scripts": {
            "dev": "vite",
            "build": "tsc && vite build",
            "preview": "vite preview",
            "lint": "tsc --noEmit"
          }
        }
        """;
    }

    private static string GenerateTsConfig()
    {
        return """
        {
          "compilerOptions": {
            "target": "ES2020",
            "useDefineForClassFields": true,
            "lib": ["ES2020", "DOM", "DOM.Iterable"],
            "module": "ESNext",
            "skipLibCheck": true,
            "moduleResolution": "bundler",
            "allowImportingTsExtensions": true,
            "resolveJsonModule": true,
            "isolatedModules": true,
            "noEmit": true,
            "jsx": "react-jsx",
            "strict": true,
            "noUnusedLocals": true,
            "noUnusedParameters": true,
            "noFallthroughCasesInSwitch": true
          },
          "include": ["src"],
          "references": [{ "path": "./tsconfig.node.json" }]
        }
        """;
    }

    private static string GenerateTsConfigNode()
    {
        return """
        {
          "compilerOptions": {
            "composite": true,
            "skipLibCheck": true,
            "module": "ESNext",
            "moduleResolution": "bundler",
            "allowSyntheticDefaultImports": true
          },
          "include": ["vite.config.ts"]
        }
        """;
    }

    private static string GenerateViteConfig()
    {
        return """
        import { defineConfig } from 'vite'
        import react from '@vitejs/plugin-react'

        // https://vitejs.dev/config/
        export default defineConfig({
          plugins: [react()],
          server: {
            port: 5173,
            proxy: {
              '/api': {
                target: 'http://localhost:5000',
                changeOrigin: true,
              },
            },
          },
        })
        """;
    }

    private static string GenerateIndexHtml(string projectName)
    {
        return $$"""
        <!DOCTYPE html>
        <html lang="en">
          <head>
            <meta charset="UTF-8" />
            <link rel="icon" type="image/svg+xml" href="/vite.svg" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>{{projectName}}</title>
          </head>
          <body>
            <div id="root"></div>
            <script type="module" src="/src/index.tsx"></script>
          </body>
        </html>
        """;
    }

    private static string GenerateIndexTsx()
    {
        return """
        import React from 'react';
        import ReactDOM from 'react-dom/client';
        import { BrowserRouter } from 'react-router-dom';
        import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
        import App from './App';

        const queryClient = new QueryClient({
          defaultOptions: {
            queries: {
              refetchOnWindowFocus: false,
              retry: 1,
            },
          },
        });

        ReactDOM.createRoot(document.getElementById('root')!).render(
          <React.StrictMode>
            <QueryClientProvider client={queryClient}>
              <BrowserRouter>
                <App />
              </BrowserRouter>
            </QueryClientProvider>
          </React.StrictMode>
        );
        """;
    }

    private static string GenerateAppTsx(List<Table> tables)
    {
        // Filter out c_Settings from UI generation (it's a config table, not user-facing)
        var uiTables = tables.Where(t =>
            !t.Name.Equals("c_Settings", StringComparison.OrdinalIgnoreCase) &&
            !t.Name.Equals("Settings", StringComparison.OrdinalIgnoreCase)).ToList();

        var imports = string.Join("\n", uiTables.Select(t =>
        {
            var className = BaseApiGenerator.GetClassName(t.Name);
            // For views, skip Form import (views are read-only)
            if (t.IsView)
            {
                return $"import {{ {className}List }} from './components/{className}/{className}List';\nimport {{ {className}Detail }} from './components/{className}/{className}Detail';";
            }
            return $"import {{ {className}List }} from './components/{className}/{className}List';\nimport {{ {className}Detail }} from './components/{className}/{className}Detail';\nimport {{ {className}Form }} from './components/{className}/{className}Form';";
        }));

        var menuItems = string.Join("\n", uiTables.Select(t =>
        {
            var className = BaseApiGenerator.GetClassName(t.Name);
            var camelName = char.ToLowerInvariant(className[0]) + className.Substring(1);
            // Add "Report" suffix for views to distinguish from tables
            var displayName = t.IsView ? $"{className}s Report" : $"{className}s";
            return $"            <ListItem disablePadding>\n              <ListItemButton component={{Link}} to=\"/{camelName}s\">\n                <ListItemText primary=\"{displayName}\" />\n              </ListItemButton>\n            </ListItem>";
        }));

        var routes = string.Join("\n", uiTables.Select(t =>
        {
            var className = BaseApiGenerator.GetClassName(t.Name);
            var camelName = char.ToLowerInvariant(className[0]) + className.Substring(1);
            // For views, only generate List and Detail routes (no create/edit)
            if (t.IsView)
            {
                return $"            <Route path=\"/{camelName}s\" element={{<{className}List />}} />\n            <Route path=\"/{camelName}s/:id\" element={{<{className}Detail />}} />";
            }
            return $"            <Route path=\"/{camelName}s\" element={{<{className}List />}} />\n            <Route path=\"/{camelName}s/new\" element={{<{className}Form />}} />\n            <Route path=\"/{camelName}s/:id\" element={{<{className}Detail />}} />\n            <Route path=\"/{camelName}s/:id/edit\" element={{<{className}Form />}} />";
        }));

        var appName = uiTables.Any() ? BaseApiGenerator.GetClassName(uiTables[0].Name) : "App";

        return $@"import {{ Routes, Route, Link }} from 'react-router-dom';
import {{ AppBar, Toolbar, Typography, Container, Box, Drawer, List, ListItem, ListItemButton, ListItemText }} from '@mui/material';
import {{ Dashboard }} from './components/Dashboard/Dashboard';
{imports}

const drawerWidth = 240;

function App() {{
  return (
    <Box sx={{{{ display: 'flex' }}}}>
      <AppBar position=""fixed"" sx={{{{ zIndex: (theme) => theme.zIndex.drawer + 1 }}}}>
        <Toolbar>
          <Typography variant=""h6"" noWrap component=""div"">
            {appName} Admin
          </Typography>
        </Toolbar>
      </AppBar>
      <Drawer
        variant=""permanent""
        sx={{{{
          width: drawerWidth,
          flexShrink: 0,
          '& .MuiDrawer-paper': {{ width: drawerWidth, boxSizing: 'border-box' }},
        }}}}
      >
        <Toolbar />
        <Box sx={{{{ overflow: 'auto' }}}}>
          <List>
            <ListItem disablePadding>
              <ListItemButton component={{Link}} to=""/"">
                <ListItemText primary=""Dashboard"" />
              </ListItemButton>
            </ListItem>
{menuItems}
          </List>
        </Box>
      </Drawer>
      <Box component=""main"" sx={{{{ flexGrow: 1, p: 3 }}}}>
        <Toolbar />
        <Container maxWidth=""xl"">
          <Routes>
            <Route path=""/"" element={{<Dashboard />}} />
{routes}
          </Routes>
        </Container>
      </Box>
    </Box>
  );
}}

export default App;
";
    }

    private static string GenerateApiClient()
    {
        return """
        import axios from 'axios';

        const apiClient = axios.create({
          baseURL: 'http://localhost:5000/api',
          headers: {
            'Content-Type': 'application/json',
          },
        });

        // Request interceptor
        apiClient.interceptors.request.use(
          (config) => {
            // Add auth token if available
            const token = localStorage.getItem('token');
            if (token) {
              config.headers.Authorization = `Bearer ${token}`;
            }
            return config;
          },
          (error) => Promise.reject(error)
        );

        // Response interceptor
        apiClient.interceptors.response.use(
          (response) => response,
          (error) => {
            if (error.response?.status === 401) {
              // Handle unauthorized
              localStorage.removeItem('token');
              window.location.href = '/login';
            }
            return Promise.reject(error);
          }
        );

        export const api = apiClient;
        export default apiClient;
        """;
    }

    private static string GenerateGitignore()
    {
        return """
        # Dependencies
        node_modules
        /.pnp
        .pnp.js

        # Testing
        /coverage

        # Production
        /build
        /dist

        # Misc
        .DS_Store
        .env.local
        .env.development.local
        .env.test.local
        .env.production.local

        npm-debug.log*
        yarn-debug.log*
        yarn-error.log*

        # Editor
        .vscode
        .idea
        """;
    }

    private static string GenerateReactReadme(string projectName)
    {
        return $$"""
        # {{projectName}} - React Frontend

        This is the React frontend for {{projectName}}, built with:
        - React 18
        - TypeScript
        - Material-UI
        - React Router
        - React Query
        - Vite

        ## Getting Started

        1. Install dependencies:
        ```bash
        npm install
        ```

        2. Start the development server:
        ```bash
        npm run dev
        ```

        3. Open http://localhost:5173 in your browser

        ## Available Scripts

        - `npm run dev` - Start development server
        - `npm run build` - Build for production
        - `npm run preview` - Preview production build
        - `npm run lint` - Run TypeScript checks

        ## Project Structure

        ```
        src/
          api/         - API client setup and endpoints
          components/  - React components
          hooks/       - Custom React hooks
          types/       - TypeScript type definitions
          App.tsx      - Main app component
          index.tsx    - Entry point
        ```
        """;
    }

    private static string GenerateDashboardTsx(List<Table> tables)
    {
        var tableTables = tables.Where(t => !t.IsView).ToList();
        var firstThreeTables = tableTables.Take(3).ToList();

        var kpiCards = string.Join("\n", firstThreeTables.Select((t, index) =>
        {
            var className = BaseApiGenerator.GetClassName(t.Name);
            var camelName = char.ToLowerInvariant(className[0]) + className.Substring(1);
            var pluralName = camelName + "s";
            var colors = new[] { "#4472C4", "#70AD47", "#FFC000" };
            var icons = new[] { "People", "ShoppingCart", "Inventory" };

            return $@"          <Grid item xs={{12}} sm={{4}}>
            <Card sx={{{{ bgcolor: '{colors[index]}', color: 'white' }}}}>
              <CardContent>
                <Box display=""flex"" alignItems=""center"" gap={{2}}>
                  <{icons[index]}Icon sx={{{{ fontSize: 48 }}}} />
                  <Box>
                    <Typography variant=""h4"">{{stats?.total{className}s || 0}}</Typography>
                    <Typography variant=""body1"">Total {className}s</Typography>
                    <Typography variant=""caption"" sx={{{{ color: 'lightgreen' }}}}>
                      +{{stats?.{pluralName}Growth || 0}}% vs last month
                    </Typography>
                  </Box>
                </Box>
              </CardContent>
            </Card>
          </Grid>";
        }));

        return $@"// <auto-generated>
//     This code was generated by TargCC Component Generator.
//     Dashboard Component
// </auto-generated>

import React from 'react';
import {{ useNavigate }} from 'react-router-dom';
import {{ useQuery }} from '@tanstack/react-query';
import {{
  Box,
  Grid,
  Card,
  CardContent,
  CardHeader,
  Typography,
  Button,
  CircularProgress,
  Avatar,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
}} from '@mui/material';
import {{
  People as PeopleIcon,
  ShoppingCart as ShoppingCartIcon,
  Inventory as InventoryIcon,
  Add as AddIcon,
  BarChart as BarChartIcon,
  AccessTime as AccessTimeIcon,
}} from '@mui/icons-material';
import {{ LineChart, Line, PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer }} from 'recharts';
import {{ format }} from 'date-fns';
import apiClient from '../../api/client';

interface DashboardStats {{
{string.Join("\n", firstThreeTables.Select(t =>
{
    var className = BaseApiGenerator.GetClassName(t.Name);
    var camelName = char.ToLowerInvariant(className[0]) + className.Substring(1);
    return $"  total{className}s: number;\n  {camelName}sGrowth: number;";
}))}
}}

interface Activity {{
  id: number;
  title: string;
  time: string;
  icon: string;
}}

export const Dashboard: React.FC = () => {{
  const navigate = useNavigate();

  // Fetch dashboard stats
  const {{ data: stats, isLoading: statsLoading }} = useQuery<DashboardStats>({{
    queryKey: ['dashboardStats'],
    queryFn: async () => {{
      const response = await apiClient.get<DashboardStats>('/dashboard/stats');
      return response.data;
    }},
  }});

  // Fetch recent activity
  const {{ data: activities, isLoading: activitiesLoading }} = useQuery<Activity[]>({{
    queryKey: ['recentActivity'],
    queryFn: async () => {{
      const response = await apiClient.get<Activity[]>('/dashboard/recent-activity');
      return response.data;
    }},
  }});

  // Sample data for charts (replace with real API data)
  const revenueData = [
    {{ month: 'Jan', revenue: 4000 }},
    {{ month: 'Feb', revenue: 3000 }},
    {{ month: 'Mar', revenue: 5000 }},
    {{ month: 'Apr', revenue: 4500 }},
    {{ month: 'May', revenue: 6000 }},
    {{ month: 'Jun', revenue: 7000 }},
  ];

  const statusData = [
    {{ name: 'Active', value: 60 }},
    {{ name: 'Pending', value: 25 }},
    {{ name: 'Completed', value: 15 }},
  ];

  const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042'];

  if (statsLoading) {{
    return (
      <Box display=""flex"" justifyContent=""center"" alignItems=""center"" minHeight=""80vh"">
        <CircularProgress />
      </Box>
    );
  }}

  return (
    <Box>
      {{/* Welcome Section */}}
      <Box mb={{3}}>
        <Typography variant=""h4"" gutterBottom>
          Welcome back! 👋
        </Typography>
        <Typography variant=""body1"" color=""text.secondary"">
          {{format(new Date(), 'EEEE, MMMM d, yyyy')}}
        </Typography>
      </Box>

      {{/* KPI Cards */}}
      <Grid container spacing={{3}} mb={{3}}>
{kpiCards}
      </Grid>

      {{/* Charts */}}
      <Grid container spacing={{3}} mb={{3}}>
        <Grid item xs={{12}} md={{6}}>
          <Card>
            <CardHeader title=""Revenue Trend"" />
            <CardContent>
              <ResponsiveContainer width=""100%"" height={{300}}>
                <LineChart data={{revenueData}}>
                  <CartesianGrid strokeDasharray=""3 3"" />
                  <XAxis dataKey=""month"" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Line type=""monotone"" dataKey=""revenue"" stroke=""#4472C4"" strokeWidth={{2}} />
                </LineChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={{12}} md={{6}}>
          <Card>
            <CardHeader title=""Status Distribution"" />
            <CardContent>
              <ResponsiveContainer width=""100%"" height={{300}}>
                <PieChart>
                  <Pie
                    data={{statusData}}
                    cx=""50%""
                    cy=""50%""
                    labelLine={{false}}
                    label={{(entry) => entry.name}}
                    outerRadius={{100}}
                    fill=""#8884d8""
                    dataKey=""value""
                  >
                    {{statusData.map((_, index) => (
                      <Cell key={{`cell-${{index}}`}} fill={{COLORS[index % COLORS.length]}} />
                    ))}}
                  </Pie>
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {{/* Quick Actions and Recent Activity */}}
      <Grid container spacing={{3}}>
        <Grid item xs={{12}} md={{6}}>
          <Card>
            <CardHeader title=""Quick Actions"" />
            <CardContent>
              <Box display=""flex"" gap={{2}} flexWrap=""wrap"">
{string.Join("\n", tableTables.Take(3).Select(t =>
{
    var className = BaseApiGenerator.GetClassName(t.Name);
    var camelName = char.ToLowerInvariant(className[0]) + className.Substring(1);
    return $@"                <Button
                  variant=""contained""
                  startIcon={{<AddIcon />}}
                  onClick={{() => navigate('/{camelName}s/new')}}
                >
                  New {className}
                </Button>";
}))}
                <Button
                  variant=""outlined""
                  startIcon={{<BarChartIcon />}}
                  onClick={{() => navigate('/{(tableTables.Any() ? char.ToLowerInvariant(BaseApiGenerator.GetClassName(tableTables[0].Name)[0]) + BaseApiGenerator.GetClassName(tableTables[0].Name).Substring(1) + "s" : "reports")}')}}
                >
                  Reports
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={{12}} md={{6}}>
          <Card>
            <CardHeader
              title=""Recent Activity""
              avatar={{<AccessTimeIcon />}}
            />
            <CardContent sx={{{{ maxHeight: 300, overflow: 'auto' }}}}>
              {{activitiesLoading ? (
                <Box display=""flex"" justifyContent=""center"" p={{2}}>
                  <CircularProgress size={{24}} />
                </Box>
              ) : activities && activities.length > 0 ? (
                <List>
                  {{activities.map((activity) => (
                    <ListItem key={{activity.id}}>
                      <ListItemAvatar>
                        <Avatar>{{activity.icon}}</Avatar>
                      </ListItemAvatar>
                      <ListItemText
                        primary={{activity.title}}
                        secondary={{activity.time}}
                      />
                    </ListItem>
                  ))}}
                </List>
              ) : (
                <Typography color=""text.secondary"" align=""center"">
                  No recent activity
                </Typography>
              )}}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}};
";
    }

    private async Task GenerateDashboardControllerAsync(
        string outputDirectory,
        string rootNamespace,
        List<Table> tables)
    {
        var dashboardCode = DashboardControllerGenerator.Generate(tables, rootNamespace);
        var dashboardPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.API", "Controllers", "DashboardController.cs");
        await SaveFileAsync(dashboardPath, dashboardCode);
        _output.Info("  ✓ DashboardController.cs");
    }


    private async Task GenerateViewReportScreensAsync(
        string outputDirectory,
        string rootNamespace,
        string connectionString)
    {
        var viewAnalyzer = new ViewAnalyzer(connectionString);
        var views = await viewAnalyzer.AnalyzeViewsAsync();
        var manualViews = views.Where(v => v.Type == ViewType.Manual).ToList();

        if (manualViews.Count > 0)
        {
            _output.Info($"  ✓ Found {manualViews.Count} manual views (MN)");

            foreach (var view in manualViews)
            {
                _output.Info($"  Processing view: {view.ViewName}");

                var className = BaseApiGenerator.GetClassName(view.ViewName);

                // Generate entity
                var viewEntityCode = TargCC.Core.Generators.Domain.ViewEntityGenerator.Generate(view, rootNamespace);
                var viewEntityPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Domain", "Entities", $"{className}.cs");
                await SaveFileAsync(viewEntityPath, viewEntityCode);

                // Generate repository interface
                var viewRepoInterface = TargCC.Core.Generators.Repositories.ViewRepositoryGenerator.GenerateInterface(view, rootNamespace);
                var viewRepoInterfacePath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Application", "Interfaces", "Repositories", $"I{className}Repository.cs");
                await SaveFileAsync(viewRepoInterfacePath, viewRepoInterface);

                // Generate repository implementation
                var viewRepoImpl = TargCC.Core.Generators.Repositories.ViewRepositoryGenerator.GenerateImplementation(view, rootNamespace);
                var viewRepoImplPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Infrastructure", "Repositories", $"{className}Repository.cs");
                await SaveFileAsync(viewRepoImplPath, viewRepoImpl);

                // Generate API controller
                var viewControllerCode = TargCC.Core.Generators.API.ViewControllerGenerator.Generate(view, rootNamespace);
                var pluralName = CodeGenerationHelpers.MakePlural(className);
                var viewControllerPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.API", "Controllers", $"{pluralName}Controller.cs");
                await SaveFileAsync(viewControllerPath, viewControllerCode);

                // Generate React report component
                var reportComponentCode = TargCC.Core.Generators.UI.Components.ReactReportComponentGenerator.Generate(view, rootNamespace);
                var reportComponentPath = Path.Combine(outputDirectory, "client", "src", "components", className, $"{className}Report.tsx");
                await SaveFileAsync(reportComponentPath, reportComponentCode);

                _output.Info($"    ✓ {view.ViewName} report screen generated");
            }

            // Update App.tsx to include MN view routes
            await UpdateAppTsxWithReportsAsync(outputDirectory, manualViews);

            _output.Info($"  ✓ Generated {manualViews.Count} report screens!");
        }
        else
        {
            _output.Warning("  No manual views (MN) found - skipping report screen generation");
        }
    }

    private async Task UpdateAppTsxWithReportsAsync(string outputDirectory, List<ViewInfo> manualViews)
    {
        var appTsxPath = Path.Combine(outputDirectory, "client", "src", "App.tsx");
        if (!File.Exists(appTsxPath))
        {
            _output.Warning("  App.tsx not found - skipping route update");
            return;
        }

        var appContent = await File.ReadAllTextAsync(appTsxPath);

        // Build import statements for reports
        var reportImports = string.Join("\n", manualViews.Select(v =>
        {
            var className = BaseApiGenerator.GetClassName(v.ViewName);
            return $"import {{ {className}Report }} from './components/{className}/{className}Report';";
        }));

        // Build menu items for reports (in a "Reports" section)
        var reportMenuItems = string.Join("\n", manualViews.Select(v =>
        {
            var className = BaseApiGenerator.GetClassName(v.ViewName);
            var camelName = CodeGenerationHelpers.ToCamelCase(className);
            var displayName = CodeGenerationHelpers.GetFriendlyViewName(v.ViewName);
            return $"            <ListItem disablePadding>\n              <ListItemButton component={{Link}} to=\"/reports/{camelName}\">\n                <ListItemText primary=\"{displayName}\" />\n              </ListItemButton>\n            </ListItem>";
        }));

        // Build routes for reports
        var reportRoutes = string.Join("\n", manualViews.Select(v =>
        {
            var className = BaseApiGenerator.GetClassName(v.ViewName);
            var camelName = CodeGenerationHelpers.ToCamelCase(className);
            return $"            <Route path=\"/reports/{camelName}\" element={{<{className}Report />}} />";
        }));

        // Find insertion point for imports (after Dashboard import)
        var dashboardImportLine = "import { Dashboard } from './components/Dashboard/Dashboard';";
        if (!appContent.Contains(dashboardImportLine))
        {
            _output.Warning("  Could not find Dashboard import in App.tsx - skipping route update");
            return;
        }

        // Insert report imports after Dashboard import
        appContent = appContent.Replace(
            dashboardImportLine,
            dashboardImportLine + "\n" + reportImports
        );

        // Find insertion point for menu items (before the closing </List>)
        var closingListTag = "          </List>";
        if (!appContent.Contains(closingListTag))
        {
            _output.Warning("  Could not find closing List tag in App.tsx - skipping menu update");
            return;
        }

        // Insert report menu items before closing </List>
        appContent = appContent.Replace(
            closingListTag,
            reportMenuItems + "\n" + closingListTag
        );

        // Find insertion point for routes (before closing </Routes>)
        var closingRoutesTag = "          </Routes>";
        if (!appContent.Contains(closingRoutesTag))
        {
            _output.Warning("  Could not find closing Routes tag in App.tsx - skipping routes update");
            return;
        }

        // Insert report routes before closing </Routes>
        appContent = appContent.Replace(
            closingRoutesTag,
            reportRoutes + "\n" + closingRoutesTag
        );

        await File.WriteAllTextAsync(appTsxPath, appContent);
        _output.Info("  ✓ App.tsx updated with report routes");
    }
    private async Task GenerateJobInfrastructureAsync(
        string outputDirectory,
        string rootNamespace,
        string projectName)
    {
        await _jobInfrastructureGenerator.GenerateCompleteInfrastructureAsync(
            outputDirectory,
            rootNamespace,
            projectName,
            includeSampleJobs: true);

        _output.Info("  ✓ Application/Jobs/ITargCCJob.cs");
        _output.Info("  ✓ Application/Jobs/JobResult.cs");
        _output.Info("  ✓ Application/Jobs/TargCCJobAttribute.cs");
        _output.Info("  ✓ Application/Jobs/SampleDailyJob.cs");
        _output.Info("  ✓ Application/Jobs/SampleManualJob.cs");
        _output.Info("  ✓ Infrastructure/Jobs/HangfireSetup.cs");
        _output.Info("  ✓ Infrastructure/Jobs/HangfireJobDiscoveryService.cs");
        _output.Info("  ✓ Infrastructure/Jobs/JobExecutor.cs");
        _output.Info("  ✓ Infrastructure/Jobs/JobLogger.cs");
        _output.Info("  ✓ Infrastructure/Jobs/HangfireAuthorizationFilter.cs");
        _output.Info("  ✓ API/Controllers/JobsController.cs");
        _output.Info("  ✓ API/Program.Hangfire.snippet.cs");
        _output.Info("  ✓ API/appsettings.Hangfire.snippet.json");
        _output.Info("  ✓ HANGFIRE_PACKAGES.txt");
    }

    private static string GenerateMappingProfile(List<Table> tables, string rootNamespace)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("// <auto-generated>");
        sb.AppendLine("//     This code was generated by TargCC");
        sb.AppendLine("//     AutoMapper Profile for all entities");
        sb.AppendLine($"//     Generation date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("// </auto-generated>");
        sb.AppendLine();
        sb.AppendLine("using AutoMapper;");
        sb.AppendLine($"using {rootNamespace}.Domain.Entities;");
        sb.AppendLine($"using {rootNamespace}.Application.DTOs;");
        sb.AppendLine();
        sb.AppendLine($"namespace {rootNamespace}.Application.Mapping;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// AutoMapper profile containing all entity to DTO mappings.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public class ApplicationMappingProfile : Profile");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Initializes a new instance of the <see cref=\"ApplicationMappingProfile\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public ApplicationMappingProfile()");
        sb.AppendLine("    {");

        foreach (var table in tables)
        {
            var className = BaseApiGenerator.GetClassName(table.Name);
            sb.AppendLine($"        // {className}");

            // Handle Task entity specially to avoid conflict with System.Threading.Tasks.Task
            if (className == "Task")
            {
                sb.AppendLine($"        CreateMap<{rootNamespace}.Domain.Entities.Task, TaskDto>().ReverseMap();");
            }
            else
            {
                sb.AppendLine($"        CreateMap<{className}, {className}Dto>().ReverseMap();");
            }
            sb.AppendLine();
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Checks if system tables (c_Enumeration, c_User, etc.) exist in the database
    /// </summary>
    private async Task<bool> CheckSystemTablesExistAsync(string connectionString)
    {
        try
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = 'dbo'
                  AND TABLE_NAME = 'c_Enumeration'";

            var result = await command.ExecuteScalarAsync();
            var count = Convert.ToInt32(result);

            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check for system tables - assuming they don't exist");
            return false;
        }
    }

    /// <summary>
    /// Executes a SQL script against the database
    /// </summary>
    private async Task ExecuteSqlScriptAsync(string connectionString, string sqlScript)
    {
        try
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            await connection.OpenAsync();

            // Split script by GO statements and execute each batch
            var batches = System.Text.RegularExpressions.Regex.Split(
                sqlScript,
                @"^\s*GO\s*$",
                System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            foreach (var batch in batches)
            {
                var trimmedBatch = batch.Trim();
                if (string.IsNullOrWhiteSpace(trimmedBatch))
                    continue;

                var command = connection.CreateCommand();
                command.CommandText = trimmedBatch;
                command.CommandTimeout = 300; // 5 minutes

                await command.ExecuteNonQueryAsync();
            }

            _logger.LogInformation("System tables SQL script executed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute system tables SQL script");
            throw new InvalidOperationException($"Failed to create system tables: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generates TypeScript enums from c_Enumeration table
    /// </summary>
    private async Task GenerateEnumsAsync(string outputDirectory, string connectionString)
    {
        try
        {
            // Load enums from c_Enumeration
            var enumLoader = new TargCC.Core.Analyzers.Database.EnumLoader(
                connectionString,
                _loggerFactory.CreateLogger<TargCC.Core.Analyzers.Database.EnumLoader>());

            var enums = await enumLoader.LoadAllEnumsAsync();

            if (enums.Count == 0)
            {
                _output.Warning("  ⚠ No enums found in c_Enumeration table");
                _output.Info("  You can add enum values to c_Enumeration to generate TypeScript enums");
                return;
            }

            // Generate TypeScript enums
            var enumGen = new TargCC.Core.Generators.TypeScript.TypeScriptEnumGenerator(
                _loggerFactory.CreateLogger<TargCC.Core.Generators.TypeScript.TypeScriptEnumGenerator>());

            var enumsCode = enumGen.GenerateEnumsCode(enums);

            // Save to client/src/enums/index.ts
            var enumsPath = Path.Combine(outputDirectory, "client", "src", "enums", "index.ts");
            await SaveFileAsync(enumsPath, enumsCode);

            var enumTypes = enums.Select(e => e.EnumType).Distinct().Count();
            _output.Info($"  ✓ src/enums/index.ts ({enumTypes} enum types, {enums.Count} values)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate TypeScript enums");
            _output.Warning($"  ⚠ Failed to generate enums: {ex.Message}");
        }
    }
}
