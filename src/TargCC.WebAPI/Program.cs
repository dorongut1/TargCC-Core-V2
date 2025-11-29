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
using TargCC.WebAPI.Models.Requests;
using TargCC.WebAPI.Models.Responses;

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

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReactApp", policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // Add TargCC services
    builder.Services.AddTargCCServices(builder.Configuration);
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

    // Generate endpoint
    app.MapPost("/api/generate", async (
        [FromBody] GenerateRequest request,
        [FromServices] IGenerationService generationService,
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

            if (string.IsNullOrWhiteSpace(request.ConnectionString))
            {
                return Results.BadRequest(new GenerateResponse
                {
                    Success = false,
                    Message = "Connection string is required",
                });
            }

            if (string.IsNullOrWhiteSpace(request.ProjectPath))
            {
                return Results.BadRequest(new GenerateResponse
                {
                    Success = false,
                    Message = "Project path is required",
                });
            }

            var generatedFiles = new List<string>();
            var errors = new List<string>();

            foreach (var tableName in request.TableNames)
            {
                try
                {
                    var result = await generationService.GenerateAllAsync(
                        request.ConnectionString,
                        tableName,
                        request.ProjectPath,
                        "MyApp");

                    if (result.Success)
                    {
                        generatedFiles.AddRange(result.GeneratedFiles.Select(f => f.FilePath));
                    }
                    else
                    {
                        errors.Add(result.ErrorMessage ?? "Unknown error");
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
            // TODO: Implement actual schema reading
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
            // TODO: Implement actual security analysis using ISecurityScanner
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
            // TODO: Implement actual quality analysis using ICodeQualityAnalyzer
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
            // TODO: Implement actual chat using IInteractiveChatService
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
