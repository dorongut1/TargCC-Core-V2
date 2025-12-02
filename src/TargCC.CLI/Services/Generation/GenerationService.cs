using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TargCC.Core.Analyzers.Database;
using TargCC.Core.Generators.API;
using TargCC.Core.Generators.Common;
using TargCC.Core.Generators.CQRS;
using TargCC.Core.Generators.Entities;
using TargCC.Core.Generators.Repositories;
using TargCC.Core.Generators.Sql;
using TargCC.Core.Interfaces.Models;

namespace TargCC.CLI.Services.Generation;

/// <summary>
/// Service for orchestrating code generation operations.
/// </summary>
public class GenerationService : IGenerationService
{
    private readonly ILogger<GenerationService> _logger;
    private readonly IOutputService _output;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerationService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="output">Output service.</param>
    /// <param name="loggerFactory">Logger factory.</param>
    public GenerationService(
        ILogger<GenerationService> logger,
        IOutputService output,
        ILoggerFactory loggerFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <inheritdoc/>
    public async Task<GenerationResult> GenerateEntityAsync(
        string connectionString,
        string tableName,
        string outputDirectory,
        string @namespace)
    {
        var sw = Stopwatch.StartNew();
        var result = new GenerationResult();

        try
        {
            _output.Info($"Analyzing table: {tableName}");

            // Analyze database
            var (schema, table) = await AnalyzeDatabaseAsync(connectionString, tableName);

            // Generate entity
            _output.Info("Generating entity class...");
            var generator = new EntityGenerator(_loggerFactory.CreateLogger<EntityGenerator>());
            var entityCode = await generator.GenerateAsync(table, schema, @namespace);

            // Save to file
            var entityFileName = $"{table.Name}.cs";
            var entityPath = Path.Combine(outputDirectory, "Entities", entityFileName);
            
            Directory.CreateDirectory(Path.GetDirectoryName(entityPath)!);
            await File.WriteAllTextAsync(entityPath, entityCode);

            result.GeneratedFiles.Add(new GeneratedFile
            {
                FilePath = entityPath,
                FileType = "Entity",
                SizeBytes = entityCode.Length,
                LineCount = entityCode.Split('\n').Length
            });

            result.Success = true;
            sw.Stop();
            result.Duration = sw.Elapsed;

            _output.Success($"Generated: {entityFileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating entity");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _output.Error($"Generation failed: {ex.Message}");
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<GenerationResult> GenerateSqlAsync(
        string connectionString,
        string tableName,
        string outputDirectory)
    {
        var sw = Stopwatch.StartNew();
        var result = new GenerationResult();

        try
        {
            _output.Info($"Analyzing table: {tableName}");

            // Analyze database
            var (schema, table) = await AnalyzeDatabaseAsync(connectionString, tableName);

            // Generate SQL
            _output.Info("Generating stored procedures...");
            var generator = new SqlGenerator(_loggerFactory.CreateLogger<SqlGenerator>(), includeAdvancedProcedures: true);
            var sqlScript = await generator.GenerateAsync(table);

            // Save to file
            var sqlDir = Path.Combine(outputDirectory, "Sql");
            Directory.CreateDirectory(sqlDir);
            
            var sqlFileName = $"{table.Name}_StoredProcedures.sql";
            var sqlPath = Path.Combine(sqlDir, sqlFileName);
            await File.WriteAllTextAsync(sqlPath, sqlScript);

            result.GeneratedFiles.Add(new GeneratedFile
            {
                FilePath = sqlPath,
                FileType = "StoredProcedures",
                SizeBytes = sqlScript.Length,
                LineCount = sqlScript.Split('\n').Length
            });

            _output.Success($"Generated: {sqlFileName}");

            result.Success = true;
            sw.Stop();
            result.Duration = sw.Elapsed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _output.Error($"Generation failed: {ex.Message}");
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<GenerationResult> GenerateAllAsync(
        string connectionString,
        string tableName,
        string outputDirectory,
        string @namespace)
    {
        var sw = Stopwatch.StartNew();
        var result = new GenerationResult();

        try
        {
            // Generate entity
            var entityResult = await GenerateEntityAsync(connectionString, tableName, outputDirectory, @namespace);
            result.GeneratedFiles.AddRange(entityResult.GeneratedFiles);

            // Generate SQL
            var sqlResult = await GenerateSqlAsync(connectionString, tableName, outputDirectory);
            result.GeneratedFiles.AddRange(sqlResult.GeneratedFiles);

            result.Success = entityResult.Success && sqlResult.Success;
            if (!result.Success)
            {
                result.ErrorMessage = $"{entityResult.ErrorMessage} {sqlResult.ErrorMessage}".Trim();
            }

            sw.Stop();
            result.Duration = sw.Elapsed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating all");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _output.Error($"Generation failed: {ex.Message}");
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<GenerationResult> GenerateRepositoryAsync(
        string connectionString,
        string tableName,
        string outputDirectory)
    {
        var sw = Stopwatch.StartNew();
        var result = new GenerationResult();

        try
        {
            _output.Info($"Analyzing table: {tableName}");

            // Analyze database
            var (schema, table) = await AnalyzeDatabaseAsync(connectionString, tableName);

            // Generate repository
            _output.Info("Generating repository implementation...");
            var generator = new RepositoryGenerator(_loggerFactory.CreateLogger<RepositoryGenerator>());
            var repositoryCode = await generator.GenerateAsync(table);

            // Save to file
            var repositoryFileName = $"{table.Name}Repository.cs";
            var repositoryPath = Path.Combine(outputDirectory, "Repositories", repositoryFileName);

            Directory.CreateDirectory(Path.GetDirectoryName(repositoryPath)!);
            await File.WriteAllTextAsync(repositoryPath, repositoryCode);

            result.GeneratedFiles.Add(new GeneratedFile
            {
                FilePath = repositoryPath,
                FileType = "Repository",
                SizeBytes = repositoryCode.Length,
                LineCount = repositoryCode.Split('\n').Length
            });

            result.Success = true;
            sw.Stop();
            result.Duration = sw.Elapsed;

            _output.Success($"Generated: {repositoryFileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating repository");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _output.Error($"Generation failed: {ex.Message}");
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<GenerationResult> GenerateCqrsAsync(
        string connectionString,
        string tableName,
        string outputDirectory)
    {
        var sw = Stopwatch.StartNew();
        var result = new GenerationResult();

        try
        {
            _output.Info($"Analyzing table: {tableName}");

            // Analyze database
            var (schema, table) = await AnalyzeDatabaseAsync(connectionString, tableName);

            // Generate Commands
            _output.Info("Generating CQRS commands...");
            var commandGenerator = new CommandGenerator(_loggerFactory.CreateLogger<CommandGenerator>());
            var commandResults = await commandGenerator.GenerateAllAsync(table);

            var commandsDir = Path.Combine(outputDirectory, "Application", "Commands");
            Directory.CreateDirectory(commandsDir);

            foreach (var cmdResult in commandResults)
            {
                // Save Command
                var commandPath = Path.Combine(commandsDir, $"{cmdResult.CommandClassName}.cs");
                await File.WriteAllTextAsync(commandPath, cmdResult.CommandCode);
                result.GeneratedFiles.Add(new GeneratedFile
                {
                    FilePath = commandPath,
                    FileType = "Command",
                    SizeBytes = cmdResult.CommandCode.Length,
                    LineCount = cmdResult.CommandCode.Split('\n').Length
                });

                // Save Handler
                var handlerPath = Path.Combine(commandsDir, $"{cmdResult.HandlerClassName}.cs");
                await File.WriteAllTextAsync(handlerPath, cmdResult.HandlerCode);
                result.GeneratedFiles.Add(new GeneratedFile
                {
                    FilePath = handlerPath,
                    FileType = "Handler",
                    SizeBytes = cmdResult.HandlerCode.Length,
                    LineCount = cmdResult.HandlerCode.Split('\n').Length
                });

                // Save Validator
                var validatorPath = Path.Combine(commandsDir, $"{cmdResult.ValidatorClassName}.cs");
                await File.WriteAllTextAsync(validatorPath, cmdResult.ValidatorCode);
                result.GeneratedFiles.Add(new GeneratedFile
                {
                    FilePath = validatorPath,
                    FileType = "Validator",
                    SizeBytes = cmdResult.ValidatorCode.Length,
                    LineCount = cmdResult.ValidatorCode.Split('\n').Length
                });

                _output.Success($"Generated: {cmdResult.CommandClassName}");
            }

            // Generate Queries
            _output.Info("Generating CQRS queries...");
            var queryGenerator = new QueryGenerator(_loggerFactory.CreateLogger<QueryGenerator>());
            var queryResults = await queryGenerator.GenerateAllAsync(table);

            var queriesDir = Path.Combine(outputDirectory, "Application", "Queries");
            Directory.CreateDirectory(queriesDir);

            foreach (var qryResult in queryResults)
            {
                // Save Query
                var queryPath = Path.Combine(queriesDir, $"{qryResult.QueryClassName}.cs");
                await File.WriteAllTextAsync(queryPath, qryResult.QueryCode);
                result.GeneratedFiles.Add(new GeneratedFile
                {
                    FilePath = queryPath,
                    FileType = "Query",
                    SizeBytes = qryResult.QueryCode.Length,
                    LineCount = qryResult.QueryCode.Split('\n').Length
                });

                // Save Handler
                var handlerPath = Path.Combine(queriesDir, $"{qryResult.HandlerClassName}.cs");
                await File.WriteAllTextAsync(handlerPath, qryResult.HandlerCode);
                result.GeneratedFiles.Add(new GeneratedFile
                {
                    FilePath = handlerPath,
                    FileType = "Handler",
                    SizeBytes = qryResult.HandlerCode.Length,
                    LineCount = qryResult.HandlerCode.Split('\n').Length
                });

                // Save Validator
                var validatorPath = Path.Combine(queriesDir, $"{qryResult.ValidatorClassName}.cs");
                await File.WriteAllTextAsync(validatorPath, qryResult.ValidatorCode);
                result.GeneratedFiles.Add(new GeneratedFile
                {
                    FilePath = validatorPath,
                    FileType = "Validator",
                    SizeBytes = qryResult.ValidatorCode.Length,
                    LineCount = qryResult.ValidatorCode.Split('\n').Length
                });

                // Save DTO if present
                if (!string.IsNullOrWhiteSpace(qryResult.DtoCode))
                {
                    var dtoPath = Path.Combine(queriesDir, $"{qryResult.DtoClassName}.cs");
                    await File.WriteAllTextAsync(dtoPath, qryResult.DtoCode);
                    result.GeneratedFiles.Add(new GeneratedFile
                    {
                        FilePath = dtoPath,
                        FileType = "DTO",
                        SizeBytes = qryResult.DtoCode.Length,
                        LineCount = qryResult.DtoCode.Split('\n').Length
                    });
                }

                _output.Success($"Generated: {qryResult.QueryClassName}");
            }

            result.Success = true;
            sw.Stop();
            result.Duration = sw.Elapsed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CQRS");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _output.Error($"Generation failed: {ex.Message}");
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<GenerationResult> GenerateApiAsync(
        string connectionString,
        string tableName,
        string outputDirectory)
    {
        var sw = Stopwatch.StartNew();
        var result = new GenerationResult();

        try
        {
            _output.Info($"Analyzing table: {tableName}");

            // Analyze database
            var (schema, table) = await AnalyzeDatabaseAsync(connectionString, tableName);

            // Generate API Controller
            _output.Info("Generating REST API controller...");
            var generator = new ApiControllerGenerator(_loggerFactory.CreateLogger<ApiControllerGenerator>());

            // Create config with defaults
            var config = new ApiGeneratorConfig
            {
                Namespace = "GeneratedApi",
                GenerateXmlDocumentation = true,
                GenerateSwaggerAttributes = true
            };

            var controllerCode = await generator.GenerateAsync(table, schema, config);

            // Extract controller name from table name
            var className = BaseApiGenerator.GetClassName(table.Name);
            var controllerName = $"{CodeGenerationHelpers.MakePlural(className)}Controller";

            // Save to file
            var controllerFileName = $"{controllerName}.cs";
            var controllerPath = Path.Combine(outputDirectory, "Api", "Controllers", controllerFileName);

            Directory.CreateDirectory(Path.GetDirectoryName(controllerPath)!);
            await File.WriteAllTextAsync(controllerPath, controllerCode);

            result.GeneratedFiles.Add(new GeneratedFile
            {
                FilePath = controllerPath,
                FileType = "ApiController",
                SizeBytes = controllerCode.Length,
                LineCount = controllerCode.Split('\n').Length
            });

            result.Success = true;
            sw.Stop();
            result.Duration = sw.Elapsed;

            _output.Success($"Generated: {controllerFileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating API controller");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _output.Error($"Generation failed: {ex.Message}");
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<GenerationResult> GenerateReactUIAsync(
        string connectionString,
        string tableName,
        string outputDirectory)
    {
        var sw = Stopwatch.StartNew();
        var result = new GenerationResult();

        try
        {
            _output.Info($"Analyzing table: {tableName}");

            // Analyze database
            var (schema, table) = await AnalyzeDatabaseAsync(connectionString, tableName);

            var className = GetClassName(table.Name);
            var componentDir = Path.Combine(outputDirectory, className);
            Directory.CreateDirectory(componentDir);

            // Configure UI generation
            var uiConfig = new TargCC.Core.Generators.UI.UIGeneratorConfig
            {
                OutputDirectory = outputDirectory,
                TypeScriptNamespace = "generated",
                UseReactQuery = true,
                UseMaterialUI = true,
                GenerateComments = true,
                GenerateJsDoc = true
            };

            var componentConfig = new TargCC.Core.Generators.UI.Components.ComponentGeneratorConfig
            {
                OutputDirectory = outputDirectory,
                Framework = TargCC.Core.Generators.UI.Components.UIFramework.MaterialUI,
                ValidationLibrary = TargCC.Core.Generators.UI.Components.FormValidationLibrary.ReactHookForm,
                UseTypeScript = true,
                UseReactRouter = true,
                IncludeAccessibility = true
            };

            // 1. Generate TypeScript Types
            _output.Info("Generating TypeScript types...");
            var typesGenerator = new TargCC.Core.Generators.UI.TypeScriptTypeGenerator(
                _loggerFactory.CreateLogger<TargCC.Core.Generators.UI.TypeScriptTypeGenerator>());
            var typesCode = await typesGenerator.GenerateAsync(table, schema, uiConfig);
            var typesPath = Path.Combine(componentDir, $"{className}.types.ts");
            await File.WriteAllTextAsync(typesPath, typesCode);
            result.GeneratedFiles.Add(new GeneratedFile
            {
                FilePath = typesPath,
                FileType = "TypeScriptTypes",
                SizeBytes = typesCode.Length,
                LineCount = typesCode.Split('\n').Length
            });
            _output.Success($"Generated: {className}.types.ts");

            // 2. Generate API Client
            _output.Info("Generating API client...");
            var apiGenerator = new TargCC.Core.Generators.UI.ReactApiGenerator(
                _loggerFactory.CreateLogger<TargCC.Core.Generators.UI.ReactApiGenerator>());
            var apiCode = await apiGenerator.GenerateAsync(table, schema, uiConfig);
            var apiPath = Path.Combine(componentDir, $"{className}.api.ts");
            await File.WriteAllTextAsync(apiPath, apiCode);
            result.GeneratedFiles.Add(new GeneratedFile
            {
                FilePath = apiPath,
                FileType = "ApiClient",
                SizeBytes = apiCode.Length,
                LineCount = apiCode.Split('\n').Length
            });
            _output.Success($"Generated: {className}.api.ts");

            // 3. Generate React Hooks
            _output.Info("Generating React hooks...");
            var hookGenerator = new TargCC.Core.Generators.UI.ReactHookGenerator(
                _loggerFactory.CreateLogger<TargCC.Core.Generators.UI.ReactHookGenerator>());
            var hooksCode = await hookGenerator.GenerateAsync(table, schema, uiConfig);
            var hooksPath = Path.Combine(componentDir, $"use{className}.ts");
            await File.WriteAllTextAsync(hooksPath, hooksCode);
            result.GeneratedFiles.Add(new GeneratedFile
            {
                FilePath = hooksPath,
                FileType = "ReactHooks",
                SizeBytes = hooksCode.Length,
                LineCount = hooksCode.Split('\n').Length
            });
            _output.Success($"Generated: use{className}.ts");

            // 4. Generate React Components (Form, List, Detail, Routes, Index)
            _output.Info("Generating React components...");
            var listGenerator = new TargCC.Core.Generators.UI.Components.ReactListComponentGenerator(
                _loggerFactory.CreateLogger<TargCC.Core.Generators.UI.Components.ReactListComponentGenerator>());
            var formGenerator = new TargCC.Core.Generators.UI.Components.ReactFormComponentGenerator(
                _loggerFactory.CreateLogger<TargCC.Core.Generators.UI.Components.ReactFormComponentGenerator>());
            var detailGenerator = new TargCC.Core.Generators.UI.Components.ReactDetailComponentGenerator(
                _loggerFactory.CreateLogger<TargCC.Core.Generators.UI.Components.ReactDetailComponentGenerator>());
            var componentOrchestrator = new TargCC.Core.Generators.UI.Components.ReactComponentOrchestratorGenerator(
                _loggerFactory.CreateLogger<TargCC.Core.Generators.UI.Components.ReactComponentOrchestratorGenerator>(),
                listGenerator,
                formGenerator,
                detailGenerator);

            var components = await componentOrchestrator.GenerateAllComponentsAsync(table, schema, componentConfig);

            foreach (var kvp in components)
            {
                var filePath = Path.Combine(componentDir, kvp.Key);
                var fileCode = kvp.Value;
                await File.WriteAllTextAsync(filePath, fileCode);
                result.GeneratedFiles.Add(new GeneratedFile
                {
                    FilePath = filePath,
                    FileType = "ReactComponent",
                    SizeBytes = fileCode.Length,
                    LineCount = fileCode.Split('\n').Length
                });
                _output.Success($"Generated: {kvp.Key}");
            }

            result.Success = true;
            sw.Stop();
            result.Duration = sw.Elapsed;

            _output.Success($"✓ Generated {result.GeneratedFiles.Count} React UI files for {tableName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating React UI");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _output.Error($"Generation failed: {ex.Message}");
        }

        return result;
    }

    private static string GetClassName(string tableName)
    {
        // Remove common prefixes
        var prefixes = new[] { "tbl", "TBL", "Tbl" };
        foreach (var prefix in prefixes)
        {
            if (tableName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                tableName = tableName.Substring(prefix.Length);
                break;
            }
        }

        // Convert to PascalCase
        return char.ToUpper(tableName[0]) + tableName.Substring(1);
    }

    private async Task<(DatabaseSchema Schema, Table Table)> AnalyzeDatabaseAsync(
        string connectionString,
        string tableName)
    {
        var analyzer = new DatabaseAnalyzer(connectionString, _loggerFactory.CreateLogger<DatabaseAnalyzer>());
        var schema = await analyzer.AnalyzeAsync();

        var table = schema.Tables.FirstOrDefault(t =>
            string.Equals(t.Name, tableName, StringComparison.OrdinalIgnoreCase));

        if (table == null)
        {
            throw new InvalidOperationException($"Table '{tableName}' not found in database");
        }

        _output.Info($"✓ Found table: {table.Name}");
        _output.Info($"✓ Columns: {table.Columns.Count}");
        _output.Info($"✓ Primary Key: {(table.PrimaryKey != null ? "Yes" : "No")}");

        return (schema, table);
    }
}
