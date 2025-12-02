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
