// <copyright file="Program.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TargCC.AI.Services;
using TargCC.AI.Models;
using TargCC.CLI.Services.Generation;
using TargCC.WebAPI.Extensions;
using TargCC.WebAPI.Middleware;
using TargCC.WebAPI.Models;
using TargCC.WebAPI.Models.Requests;
using TargCC.WebAPI.Models.Responses;
using TargCC.WebAPI.Services;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/webapi-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

try
{

    // Add services
    builder.Host.UseSerilog();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Configure JSON serialization to use camelCase
    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReactApp", policy =>
        {
            policy.WithOrigins(
                      "http://localhost:5173",
                      "http://localhost:5174",
                      "http://localhost:5175",
                      "http://localhost:5176",
                      "http://localhost:5177",
                      "http://localhost:5178",
                      "http://localhost:5179",
                      "http://localhost:5180")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // Add TargCC services
    builder.Services.AddTargCCServices(builder.Configuration);
    
    // Add Schema Service
    builder.Services.AddScoped<ISchemaService, SchemaService>();
    
    // Add Connection Service
    builder.Services.AddSingleton<IConnectionService, ConnectionService>();
    
    // Add Generation History Service
    builder.Services.AddSingleton<IGenerationHistoryService, GenerationHistoryService>();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly during setup");
    throw;
}

var app = builder.Build();

try
{

    // Configure middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowReactApp");
    app.UseConnectionString();
    app.UseSerilogRequestLogging();

    // Health check endpoint
    app.MapGet("/api/health", () => Results.Ok(new
    {
        Status = "healthy",
        Timestamp = DateTime.UtcNow,
        Version = "2.0.0-beta.1",
    }))
    .WithName("HealthCheck")
    .WithOpenApi();

    // Schema endpoints - Get list of schemas
    app.MapGet("/api/schema", async (
        HttpContext context,
        [FromServices] ISchemaService schemaService,
        ILogger<Program> logger) =>
    {
        try
        {
            // Get connection string from middleware (X-Connection-String header) or fall back to default
            var connectionString = context.Items["ConnectionString"] as string
                ?? builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
            
            var schemas = await schemaService.GetSchemasAsync(connectionString);

            // Note: TableCount could be enhanced by querying INFORMATION_SCHEMA.TABLES
            // For now, returning schema list without table counts for performance
            var schemaList = schemas.Select(s => new
            {
                Name = s,
                DisplayName = s,
                TableCount = 0 // Could query: SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{s}'
            }).ToList();

            return Results.Ok(new
            {
                Success = true,
                Data = schemaList
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching schemas");
            return Results.Ok(new
            {
                Success = false,
                Error = ex.Message
            });
        }
    })
    .WithName("GetSchemas")
    .WithOpenApi();

    // Schema endpoints - Get schema details
    app.MapGet("/api/schema/{schemaName}", async (
        string schemaName,
        HttpContext context,
        [FromServices] ISchemaService schemaService,
        ILogger<Program> logger) =>
    {
        try
        {
            // Get connection string from middleware (X-Connection-String header) or fall back to default
            var connectionString = context.Items["ConnectionString"] as string
                ?? builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
            
            var schema = await schemaService.GetSchemaDetailsAsync(connectionString, schemaName);

            return Results.Ok(new
            {
                Success = true,
                Data = schema
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching schema details for {SchemaName}", schemaName);
            return Results.Ok(new
            {
                Success = false,
                Error = ex.Message
            });
        }
    })
    .WithName("GetSchemaDetails")
    .WithOpenApi();

    // Schema endpoints - Refresh schema
    app.MapPost("/api/schema/{schemaName}/refresh", async (
        string schemaName,
        HttpContext context,
        [FromServices] ISchemaService schemaService,
        ILogger<Program> logger) =>
    {
        try
        {
            // Get connection string from middleware (X-Connection-String header) or fall back to default
            var connectionString = context.Items["ConnectionString"] as string
                ?? builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
            
            var schema = await schemaService.GetSchemaDetailsAsync(connectionString, schemaName);

            return Results.Ok(new
            {
                Success = true,
                Data = schema,
                Message = "Schema refreshed successfully"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing schema for {SchemaName}", schemaName);
            return Results.Ok(new
            {
                Success = false,
                Error = ex.Message
            });
        }
    })
    .WithName("RefreshSchema")
    .WithOpenApi();

    // Schema endpoints - Get tables in schema
    app.MapGet("/api/schema/{schemaName}/tables", async (
        string schemaName,
        HttpContext context,
        [FromServices] ISchemaService schemaService,
        ILogger<Program> logger) =>
    {
        try
        {
            var connectionString = context.Items["ConnectionString"]?.ToString();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return Results.BadRequest(new
                {
                    Success = false,
                    Error = "Connection string is required. Please select a database connection."
                });
            }

            var schema = await schemaService.GetSchemaDetailsAsync(connectionString, schemaName);

            return Results.Ok(schema.Tables);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching tables for schema {SchemaName}", schemaName);
            return Results.Ok(new
            {
                Success = false,
                Error = ex.Message
            });
        }
    })
    .WithName("GetSchemaTablesV2")
    .WithOpenApi();

    // Connection endpoints - Get all connections
    app.MapGet("/api/connections", async ([FromServices] IConnectionService connectionService) =>
    {
        try
        {
            var connections = await connectionService.GetConnectionsAsync();
            return Results.Ok(new { Success = true, Data = connections });
        }
        catch (Exception ex)
        {
            return Results.Ok(new { Success = false, Error = ex.Message });
        }
    })
    .WithName("GetConnections")
    .WithOpenApi();

    // Connection endpoints - Get single connection
    app.MapGet("/api/connections/{id}", async (
        string id,
        [FromServices] IConnectionService connectionService) =>
    {
        try
        {
            var connection = await connectionService.GetConnectionAsync(id);
            if (connection == null)
            {
                return Results.NotFound(new { Success = false, Error = "Connection not found" });
            }
            return Results.Ok(new { Success = true, Data = connection });
        }
        catch (Exception ex)
        {
            return Results.Ok(new { Success = false, Error = ex.Message });
        }
    })
    .WithName("GetConnection")
    .WithOpenApi();

    // Connection endpoints - Add connection
    app.MapPost("/api/connections", async (
        [FromBody] DatabaseConnectionInfo connection,
        [FromServices] IConnectionService connectionService) =>
    {
        try
        {
            var added = await connectionService.AddConnectionAsync(connection);
            return Results.Ok(new { Success = true, Data = added });
        }
        catch (Exception ex)
        {
            return Results.Ok(new { Success = false, Error = ex.Message });
        }
    })
    .WithName("AddConnection")
    .WithOpenApi();

    // Connection endpoints - Update connection
    app.MapPut("/api/connections/{id}", async (
        string id,
        [FromBody] DatabaseConnectionInfo connection,
        [FromServices] IConnectionService connectionService) =>
    {
        try
        {
            connection.Id = id;
            await connectionService.UpdateConnectionAsync(connection);
            return Results.Ok(new { Success = true, Message = "Connection updated" });
        }
        catch (Exception ex)
        {
            return Results.Ok(new { Success = false, Error = ex.Message });
        }
    })
    .WithName("UpdateConnection")
    .WithOpenApi();

    // Connection endpoints - Delete connection
    app.MapDelete("/api/connections/{id}", async (
        string id,
        [FromServices] IConnectionService connectionService) =>
    {
        try
        {
            await connectionService.DeleteConnectionAsync(id);
            return Results.Ok(new { Success = true, Message = "Connection deleted" });
        }
        catch (Exception ex)
        {
            return Results.Ok(new { Success = false, Error = ex.Message });
        }
    })
    .WithName("DeleteConnection")
    .WithOpenApi();

    // Connection endpoints - Test connection
    app.MapPost("/api/connections/test", async (
        [FromBody] TestConnectionRequest request,
        [FromServices] IConnectionService connectionService,
        ILogger<Program> logger) =>
    {
        try
        {
            var isValid = await connectionService.TestConnectionAsync(request.ConnectionString);
            return Results.Ok(new { 
                Success = true, 
                IsValid = isValid,
                Message = isValid ? "Connection successful" : "Connection failed"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error testing connection");
            return Results.Ok(new { 
                Success = false, 
                IsValid = false,
                Error = ex.Message 
            });
        }
    })
    .WithName("TestConnection")
    .WithOpenApi();

    // Table preview endpoint
    app.MapGet("/api/schema/{schemaName}/{tableName}/preview", async (
        string schemaName,
        string tableName,
        [FromServices] ISchemaService schemaService,
        ILogger<Program> logger,
        [FromQuery] int rowCount = 10) =>
    {
        try
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
            
            var preview = await schemaService.GetTablePreviewAsync(connectionString, schemaName, tableName, rowCount);
            return Results.Ok(new { Success = true, Data = preview });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting table preview for {SchemaName}.{TableName}", schemaName, tableName);
            return Results.Ok(new { Success = false, Error = ex.Message });
        }
    })
    .WithName("GetTablePreview")
    .WithOpenApi();

    // Generate endpoint
    app.MapPost("/api/generate", async (
        HttpContext context,
        [FromBody] GenerateRequest request,
        [FromServices] IGenerationService generationService,
        [FromServices] IConfiguration configuration,
        ILogger<Program> logger) =>
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (request.TableNames == null || request.TableNames.Count == 0)
            {
                return Results.BadRequest(new GenerateResponse
                {
                    Success = false,
                    Message = "Table names are required",
                });
            }

            // Get connection string from HttpContext.Items (set by middleware) or request body
            var connectionString = context.Items["ConnectionString"] as string ?? request.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return Results.BadRequest(new GenerateResponse
                {
                    Success = false,
                    Message = "Connection string is required",
                });
            }

            // Use ProjectPath from request, or default to configured output directory
            var projectPath = request.ProjectPath;
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                projectPath = configuration["Generation:OutputDirectory"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Generated");
                // Ensure directory exists
                Directory.CreateDirectory(projectPath);
            }

            var generatedFiles = new List<string>();
            var errors = new List<string>();

            foreach (var tableName in request.TableNames)
            {
                try
                {
                    // Generate based on selected options
                    var tableResults = new List<GenerationResult>();

                    // Entity generation
                    if (request.GenerateEntity)
                    {
                        var entityResult = await generationService.GenerateEntityAsync(
                            connectionString,
                            tableName,
                            projectPath,
                            "MyApp");
                        tableResults.Add(entityResult);
                    }

                    // SQL stored procedures generation
                    if (request.IncludeStoredProcedures)
                    {
                        var sqlResult = await generationService.GenerateSqlAsync(
                            connectionString,
                            tableName,
                            projectPath);
                        tableResults.Add(sqlResult);
                    }

                    // Repository generation
                    if (request.GenerateRepository)
                    {
                        var repoResult = await generationService.GenerateRepositoryAsync(
                            connectionString,
                            tableName,
                            projectPath);
                        tableResults.Add(repoResult);
                    }

                    // API Controller generation
                    if (request.GenerateController)
                    {
                        var apiResult = await generationService.GenerateApiAsync(
                            connectionString,
                            tableName,
                            projectPath);
                        tableResults.Add(apiResult);
                    }

                    // CQRS handlers generation (if Service is selected, we generate CQRS handlers)
                    if (request.GenerateService)
                    {
                        var cqrsResult = await generationService.GenerateCqrsAsync(
                            connectionString,
                            tableName,
                            projectPath);
                        tableResults.Add(cqrsResult);
                    }

                    // Collect all generated files and errors
                    foreach (var result in tableResults)
                    {
                        if (result.Success)
                        {
                            generatedFiles.AddRange(result.GeneratedFiles.Select(f => f.FilePath));
                        }
                        else
                        {
                            errors.Add($"{tableName}: {result.ErrorMessage ?? "Unknown error"}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error generating code for table {TableName}", tableName);
                    errors.Add($"{tableName}: {ex.Message}");
                }
            }

            sw.Stop();

            return Results.Ok(new GenerateResponse
            {
                Success = errors.Count == 0,
                Message = errors.Count == 0
                    ? $"Successfully generated code for {request.TableNames.Count} table(s)"
                    : $"Generated with {errors.Count} error(s)",
                GeneratedFiles = generatedFiles,
                Errors = errors,
                Stats = new GenerationStats
                {
                    TablesProcessed = request.TableNames.Count,
                    FilesGenerated = generatedFiles.Count,
                    DurationMs = sw.ElapsedMilliseconds,
                },
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during code generation");
            return Results.Ok(new GenerateResponse
            {
                Success = false,
                Message = ex.Message,
                Errors = new List<string> { ex.Message },
            });
        }
    })
    .WithName("GenerateCode")
    .WithOpenApi();

    // System info endpoint
    app.MapGet("/api/system/info", () => Results.Ok(new
    {
        Version = "2.0.0-beta.1",
        Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
        MachineName = Environment.MachineName,
        ProcessorCount = Environment.ProcessorCount,
        WorkingSet = Environment.WorkingSet,
    }))
    .WithName("SystemInfo")
    .WithOpenApi();

    // Schema tables endpoint
    app.MapPost("/api/schema/tables", async (
        [FromBody] SchemaRequest request,
        ILogger<Program> logger) =>
    {
        if (string.IsNullOrWhiteSpace(request.ConnectionString))
        {
            return Results.BadRequest(new SchemaResponse
            {
                Success = false,
                Errors = new List<string> { "Connection string is required" },
            });
        }

        try
        {
            // MOCK ENDPOINT - Returning sample data for UI development
            // Future implementation: Use SchemaService to read actual database schema
            return Results.Ok(new SchemaResponse
            {
                Success = true,
                Tables = new List<TableInfo>
                {
                    new() { Name = "Customer", Schema = "dbo", ColumnCount = 5 },
                    new() { Name = "Order", Schema = "dbo", ColumnCount = 8 },
                    new() { Name = "Product", Schema = "dbo", ColumnCount = 6 },
                },
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving schema tables");
            return Results.Ok(new SchemaResponse
            {
                Success = false,
                Errors = new List<string> { ex.Message },
            });
        }
    })
    .WithName("GetSchemaTables")
    .WithOpenApi();

    // Security analysis endpoint
    app.MapPost("/api/analyze/security", async (
        [FromBody] AnalyzeRequest request,
        ILogger<Program> logger) =>
    {
        if (string.IsNullOrWhiteSpace(request.ConnectionString))
        {
            return Results.BadRequest(new AnalyzeResponse
            {
                Success = false,
                AnalysisType = "Security",
                Errors = new List<string> { "Connection string is required" },
            });
        }

        if (string.IsNullOrWhiteSpace(request.TableName))
        {
            return Results.BadRequest(new AnalyzeResponse
            {
                Success = false,
                AnalysisType = "Security",
                Errors = new List<string> { "Table name is required" },
            });
        }

        try
        {
            // MOCK ENDPOINT - Returning sample security analysis for UI development
            // Future implementation: Use ISecurityScanner service for actual analysis
            return Results.Ok(new AnalyzeResponse
            {
                Success = true,
                AnalysisType = "Security",
                Results = new
                {
                    TableName = request.TableName,
                    SecurityIssues = new List<string>(),
                    Recommendations = new List<string>
                    {
                        "Consider adding encryption for sensitive columns",
                        "Review access permissions for this table",
                    },
                },
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error analyzing security");
            return Results.Ok(new AnalyzeResponse
            {
                Success = false,
                AnalysisType = "Security",
                Errors = new List<string> { ex.Message },
            });
        }
    })
    .WithName("AnalyzeSecurity")
    .WithOpenApi();

    // Quality analysis endpoint
    app.MapPost("/api/analyze/quality", async (
        [FromBody] AnalyzeRequest request,
        ILogger<Program> logger) =>
    {
        if (string.IsNullOrWhiteSpace(request.ConnectionString))
        {
            return Results.BadRequest(new AnalyzeResponse
            {
                Success = false,
                AnalysisType = "Quality",
                Errors = new List<string> { "Connection string is required" },
            });
        }

        if (string.IsNullOrWhiteSpace(request.TableName))
        {
            return Results.BadRequest(new AnalyzeResponse
            {
                Success = false,
                AnalysisType = "Quality",
                Errors = new List<string> { "Table name is required" },
            });
        }

        try
        {
            // MOCK ENDPOINT - Returning sample quality analysis for UI development
            // Future implementation: Use ICodeQualityAnalyzer service for actual analysis
            return Results.Ok(new AnalyzeResponse
            {
                Success = true,
                AnalysisType = "Quality",
                Results = new
                {
                    TableName = request.TableName,
                    QualityScore = 85,
                    Issues = new List<string>(),
                    Suggestions = new List<string>
                    {
                        "Consider adding indexes for foreign key columns",
                        "Review naming conventions",
                    },
                },
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error analyzing quality");
            return Results.Ok(new AnalyzeResponse
            {
                Success = false,
                AnalysisType = "Quality",
                Errors = new List<string> { ex.Message },
            });
        }
    })
    .WithName("AnalyzeQuality")
    .WithOpenApi();

    // Chat endpoint
    app.MapPost("/api/chat", async (
        [FromBody] ChatRequest request,
        ILogger<Program> logger) =>
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return Results.BadRequest(new ChatResponse
            {
                Success = false,
                Errors = new List<string> { "Message is required" },
            });
        }

        try
        {
            // MOCK ENDPOINT - Returning echo response for UI development
            // Future implementation: Use IInteractiveChatService for actual AI chat
            return Results.Ok(new ChatResponse
            {
                Success = true,
                Message = $"Echo: {request.Message}",
                Context = new List<TargCC.WebAPI.Models.Responses.ConversationMessage>
                {
                    new() { Role = "user", Content = request.Message },
                    new() { Role = "assistant", Content = $"Echo: {request.Message}" },
                },
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing chat");
            return Results.Ok(new ChatResponse
            {
                Success = false,
                Errors = new List<string> { ex.Message },
            });
        }
    })
    .WithName("Chat")
    .WithOpenApi();

    // Generation History endpoints
    app.MapGet("/api/generation/history", async (
        [FromServices] IGenerationHistoryService historyService,
        [FromQuery] string? tableName,
        ILogger<Program> logger) =>
    {
        try
        {
            var history = await historyService.GetHistoryAsync(tableName);
            return Results.Ok(history);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting generation history");
            return Results.Problem(ex.Message);
        }
    })
    .WithName("GetGenerationHistory")
    .WithOpenApi();

    app.MapGet("/api/generation/history/{tableName}", async (
        string tableName,
        [FromServices] IGenerationHistoryService historyService,
        ILogger<Program> logger) =>
    {
        try
        {
            var history = await historyService.GetHistoryAsync(tableName);
            return Results.Ok(history);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting history for table");
            return Results.Problem(ex.Message);
        }
    })
    .WithName("GetTableHistory")
    .WithOpenApi();

    app.MapGet("/api/generation/status/{tableName}", async (
        string tableName,
        [FromServices] IGenerationHistoryService historyService,
        ILogger<Program> logger) =>
    {
        try
        {
            var status = await historyService.GetGenerationStatusAsync(tableName);
            var lastGeneration = await historyService.GetLastGenerationAsync(tableName);
            
            return Results.Ok(new
            {
                TableName = tableName,
                Status = status,
                LastGenerated = lastGeneration?.GeneratedAt,
                Success = lastGeneration?.Success ?? false,
                FilesGenerated = lastGeneration?.FilesGenerated.Length ?? 0,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting generation status");
            return Results.Problem(ex.Message);
        }
    })
    .WithName("GetGenerationStatus")
    .WithOpenApi();

    app.MapDelete("/api/generation/history", async (
        [FromServices] IGenerationHistoryService historyService,
        ILogger<Program> logger) =>
    {
        try
        {
            await historyService.ClearHistoryAsync();
            return Results.Ok(new { Success = true, Message = "History cleared" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error clearing history");
            return Results.Problem(ex.Message);
        }
    })
    .WithName("ClearGenerationHistory")
    .WithOpenApi();

    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Starting TargCC Web API...");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly during runtime");
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Make Program accessible to integration tests
public partial class Program
{
}
