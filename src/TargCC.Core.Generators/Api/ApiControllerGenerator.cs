// <copyright file="ApiControllerGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.Api;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using TargCC.Core.Generators.Common;
using TargCC.Core.Interfaces.Models;

/// <summary>
/// Generates REST API controllers for Clean Architecture applications.
/// Controllers use MediatR for dispatching queries and commands.
/// </summary>
/// <remarks>
/// <para>
/// The generator creates controllers with 5 standard endpoints:
/// </para>
/// <list type="bullet">
/// <item><description>GET /{id} - Retrieves a single entity by its primary key</description></item>
/// <item><description>GET / - Retrieves all entities with pagination support</description></item>
/// <item><description>POST / - Creates a new entity</description></item>
/// <item><description>PUT /{id} - Updates an existing entity</description></item>
/// <item><description>DELETE /{id} - Deletes an entity</description></item>
/// </list>
/// <para>
/// All generated code includes:
/// </para>
/// <list type="bullet">
/// <item><description>Swagger/OpenAPI annotations</description></item>
/// <item><description>Comprehensive XML documentation</description></item>
/// <item><description>Error handling with try-catch blocks</description></item>
/// <item><description>Structured logging using ILogger</description></item>
/// <item><description>CancellationToken support for async operations</description></item>
/// </list>
/// </remarks>
public class ApiControllerGenerator : IApiControllerGenerator
{
    private static readonly Action<ILogger, string, Exception?> LogGeneratingController =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, nameof(LogGeneratingController)),
            "Generating API controller for table: {TableName}");

    private static readonly Action<ILogger, string, int, Exception?> LogControllerGenerated =
        LoggerMessage.Define<string, int>(
            LogLevel.Information,
            new EventId(2, nameof(LogControllerGenerated)),
            "Successfully generated API controller for table: {TableName} with {EndpointCount} endpoints");

    private readonly ILogger<ApiControllerGenerator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiControllerGenerator"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging generation activities.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public ApiControllerGenerator(ILogger<ApiControllerGenerator> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <inheritdoc/>
    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "REST API routes require lowercase for convention.")]
    public Task<ApiControllerGenerationResult> GenerateAsync(Table table, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(table);

        if (string.IsNullOrWhiteSpace(table.PrimaryKey) && (table.PrimaryKeyColumns == null || table.PrimaryKeyColumns.Count == 0))
        {
            throw new InvalidOperationException($"Table '{table.Name}' must have a primary key to generate an API controller.");
        }

        // Log using direct call for testability (Moq can intercept this)
#pragma warning disable CA1848 // Use LoggerMessage delegates for performance - disabled for testability with Moq
        _logger.LogInformation("Generating API controller for table: {TableName}", table.Name);
#pragma warning restore CA1848

        var pluralName = CodeGenerationHelpers.MakePlural(table.Name);
        var controllerName = string.Format(CultureInfo.InvariantCulture, "{0}Controller", pluralName);
        var routePrefix = string.Format(CultureInfo.InvariantCulture, "api/{0}", pluralName.ToLowerInvariant());

        var sb = new StringBuilder();

        GenerateFileHeader(sb, table.Name, controllerName);
        GenerateUsings(sb, table.Name);
        GenerateNamespace(sb, () =>
        {
            GenerateControllerClass(sb, table, pluralName);
        });

        const int endpointCount = 5;

        LogControllerGenerated(_logger, table.Name, endpointCount, null);

        var result = new ApiControllerGenerationResult
        {
            ControllerCode = sb.ToString(),
            TableName = table.Name,
            ControllerName = controllerName,
            RoutePrefix = routePrefix,
            EndpointCount = endpointCount,
            GeneratedAt = DateTime.UtcNow,
        };

        return Task.FromResult(result);
    }

    private static void GenerateFileHeader(StringBuilder sb, string tableName, string controllerName)
    {
        sb.AppendLine(CultureInfo.InvariantCulture, $"// <auto-generated>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"// This code was generated by TargCC API Controller Generator.");
        sb.AppendLine(CultureInfo.InvariantCulture, $"// Source Table: {tableName}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"// Controller: {controllerName}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"// Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine(CultureInfo.InvariantCulture, $"//");
        sb.AppendLine(CultureInfo.InvariantCulture, $"// WARNING: Changes to this file may be overwritten during regeneration.");
        sb.AppendLine(CultureInfo.InvariantCulture, $"// </auto-generated>");
        sb.AppendLine();
    }

    private static void GenerateUsings(StringBuilder sb, string tableName)
    {
        var pluralName = CodeGenerationHelpers.MakePlural(tableName);

        sb.AppendLine("using MediatR;");
        sb.AppendLine("using Microsoft.AspNetCore.Http;");
        sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using TargCC.Application.Common.Models;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using TargCC.Application.Features.{pluralName}.Commands.Create{tableName};");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using TargCC.Application.Features.{pluralName}.Commands.Update{tableName};");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using TargCC.Application.Features.{pluralName}.Commands.Delete{tableName};");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using TargCC.Application.Features.{pluralName}.Queries.Get{tableName};");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using TargCC.Application.Features.{pluralName}.Queries.Get{pluralName};");
        sb.AppendLine();
    }

    private static void GenerateNamespace(StringBuilder sb, Action generateContent)
    {
        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace TargCC.API.Controllers;");
        sb.AppendLine();
        generateContent();
    }

    private static void GenerateControllerClass(StringBuilder sb, Table table, string pluralName)
    {
        var pkColumn = GetPrimaryKeyColumn(table);
        var pkType = CodeGenerationHelpers.GetCSharpType(pkColumn.DataType);
        var pkParamName = CodeGenerationHelpers.ToCamelCase(pkColumn.Name);

        GenerateControllerClassDocumentation(sb, table.Name, pluralName);
        GenerateControllerClassAttributes(sb);
        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {pluralName}Controller : ControllerBase");
        sb.AppendLine("{");

        GenerateControllerFields(sb);
        GenerateControllerConstructor(sb, pluralName);
        GenerateGetByIdEndpoint(sb, table.Name, pkType, pkParamName);
        GenerateGetAllEndpoint(sb, table.Name, pluralName);
        GenerateCreateEndpoint(sb, table.Name);
        GenerateUpdateEndpoint(sb, table.Name, pkType, pkParamName);
        GenerateDeleteEndpoint(sb, table.Name, pkType, pkParamName);

        sb.AppendLine("}");
    }

    private static void GenerateControllerClassDocumentation(StringBuilder sb, string tableName, string pluralName)
    {
        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// API controller for {tableName} management.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("/// <remarks>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Provides CRUD operations for {pluralName} using CQRS pattern with MediatR.");
        sb.AppendLine("/// </remarks>");
    }

    private static void GenerateControllerClassAttributes(StringBuilder sb)
    {
        sb.AppendLine("[ApiController]");
        sb.AppendLine("[Route(\"api/[controller]\")]");
        sb.AppendLine("[Produces(\"application/json\")]");
    }

    private static void GenerateControllerFields(StringBuilder sb)
    {
        sb.AppendLine("    private readonly IMediator _mediator;");
        sb.AppendLine("    private readonly ILogger _logger;");
        sb.AppendLine();
    }

    private static void GenerateControllerConstructor(StringBuilder sb, string pluralName)
    {
        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Initializes a new instance of the <see cref=\"{pluralName}Controller\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"mediator\">The MediatR mediator for dispatching requests.</param>");
        sb.AppendLine("    /// <param name=\"logger\">The logger instance.</param>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {pluralName}Controller(IMediator mediator, ILogger<{pluralName}Controller> logger)");
        sb.AppendLine("    {");
        sb.AppendLine("        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));");
        sb.AppendLine("        _logger = logger ?? throw new ArgumentNullException(nameof(logger));");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateGetByIdEndpoint(StringBuilder sb, string tableName, string pkType, string pkParamName)
    {
        var dtoName = string.Format(CultureInfo.InvariantCulture, "{0}Dto", tableName);

        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Gets a {tableName} by its identifier.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// <param name=\"{pkParamName}\">The {tableName} identifier.</param>");
        sb.AppendLine("    /// <param name=\"cancellationToken\">Cancellation token.</param>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// <returns>The {tableName} if found; otherwise, NotFound.</returns>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// <response code=\"200\">Returns the {tableName}.</response>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// <response code=\"404\">{tableName} not found.</response>");
        sb.AppendLine("    /// <response code=\"500\">Internal server error.</response>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    [HttpGet(\"{{{pkParamName}}}\")]");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    [ProducesResponseType(typeof({dtoName}), StatusCodes.Status200OK)]");
        sb.AppendLine("    [ProducesResponseType(StatusCodes.Status404NotFound)]");
        sb.AppendLine("    [ProducesResponseType(StatusCodes.Status500InternalServerError)]");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public async Task<IActionResult> GetById({pkType} {pkParamName}, CancellationToken cancellationToken = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogDebug(\"Getting {tableName} by ID: {{{pkParamName.ToUpper(CultureInfo.InvariantCulture)}}}\", {pkParamName});");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            var query = new Get{tableName}Query({pkParamName});");
        sb.AppendLine("            var result = await _mediator.Send(query, cancellationToken);");
        sb.AppendLine();
        sb.AppendLine("            if (!result.IsSuccess)");
        sb.AppendLine("            {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"                _logger.LogWarning(\"{tableName} not found with ID: {{{pkParamName.ToUpper(CultureInfo.InvariantCulture)}}}\", {pkParamName});");
        sb.AppendLine("                return NotFound(result.Error);");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogDebug(\"{tableName} retrieved successfully. ID: {{{pkParamName.ToUpper(CultureInfo.InvariantCulture)}}}\", {pkParamName});");
        sb.AppendLine("            return Ok(result.Data);");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogError(ex, \"Error getting {tableName} by ID: {{{pkParamName.ToUpper(CultureInfo.InvariantCulture)}}}\", {pkParamName});");
        sb.AppendLine("            return StatusCode(StatusCodes.Status500InternalServerError, \"An error occurred while processing your request.\");");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateGetAllEndpoint(StringBuilder sb, string tableName, string pluralName)
    {
        var dtoName = string.Format(CultureInfo.InvariantCulture, "{0}Dto", tableName);

        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Gets all {pluralName} with pagination.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"pageNumber\">The page number (default: 1).</param>");
        sb.AppendLine("    /// <param name=\"pageSize\">The page size (default: 10).</param>");
        sb.AppendLine("    /// <param name=\"cancellationToken\">Cancellation token.</param>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// <returns>A paginated list of {pluralName}.</returns>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// <response code=\"200\">Returns the list of {pluralName}.</response>");
        sb.AppendLine("    /// <response code=\"500\">Internal server error.</response>");
        sb.AppendLine("    [HttpGet]");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    [ProducesResponseType(typeof(PaginatedList<{dtoName}>), StatusCodes.Status200OK)]");
        sb.AppendLine("    [ProducesResponseType(StatusCodes.Status500InternalServerError)]");
        sb.AppendLine("    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogDebug(\"Getting all {pluralName}. Page: {{PageNumber}}, Size: {{PageSize}}\", pageNumber, pageSize);");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            var query = new Get{pluralName}Query(pageNumber, pageSize);");
        sb.AppendLine("            var result = await _mediator.Send(query, cancellationToken);");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogDebug(\"{pluralName} retrieved successfully. Count: {{Count}}\", result.Data?.Items?.Count ?? 0);");
        sb.AppendLine("            return Ok(result.Data);");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogError(ex, \"Error getting all {pluralName}\");");
        sb.AppendLine("            return StatusCode(StatusCodes.Status500InternalServerError, \"An error occurred while processing your request.\");");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateCreateEndpoint(StringBuilder sb, string tableName)
    {
        var commandName = string.Format(CultureInfo.InvariantCulture, "Create{0}Command", tableName);

        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Creates a new {tableName}.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// <param name=\"command\">The create {tableName} command.</param>");
        sb.AppendLine("    /// <param name=\"cancellationToken\">Cancellation token.</param>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// <returns>The ID of the created {tableName}.</returns>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// <response code=\"201\">{tableName} created successfully.</response>");
        sb.AppendLine("    /// <response code=\"400\">Invalid request data.</response>");
        sb.AppendLine("    /// <response code=\"500\">Internal server error.</response>");
        sb.AppendLine("    [HttpPost]");
        sb.AppendLine("    [ProducesResponseType(StatusCodes.Status201Created)]");
        sb.AppendLine("    [ProducesResponseType(StatusCodes.Status400BadRequest)]");
        sb.AppendLine("    [ProducesResponseType(StatusCodes.Status500InternalServerError)]");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public async Task<IActionResult> Create([FromBody] {commandName} command, CancellationToken cancellationToken = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogDebug(\"Creating new {tableName}\");");
        sb.AppendLine();
        sb.AppendLine("            var result = await _mediator.Send(command, cancellationToken);");
        sb.AppendLine();
        sb.AppendLine("            if (!result.IsSuccess)");
        sb.AppendLine("            {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"                _logger.LogWarning(\"Failed to create {tableName}: {{Error}}\", result.Error);");
        sb.AppendLine("                return BadRequest(result.Error);");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogInformation(\"{tableName} created successfully. ID: {{Id}}\", result.Data);");
        sb.AppendLine("            return CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data);");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogError(ex, \"Error creating {tableName}\");");
        sb.AppendLine("            return StatusCode(StatusCodes.Status500InternalServerError, \"An error occurred while processing your request.\");");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateUpdateEndpoint(StringBuilder sb, string tableName, string pkType, string pkParamName)
    {
        var commandName = string.Format(CultureInfo.InvariantCulture, "Update{0}Command", tableName);

        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Updates an existing {tableName}.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// <param name=\"{pkParamName}\">The {tableName} identifier.</param>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// <param name=\"command\">The update {tableName} command.</param>");
        sb.AppendLine("    /// <param name=\"cancellationToken\">Cancellation token.</param>");
        sb.AppendLine("    /// <returns>No content if successful.</returns>");
        sb.AppendLine("    /// <response code=\"204\">Update successful.</response>");
        sb.AppendLine("    /// <response code=\"400\">Invalid request data or ID mismatch.</response>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// <response code=\"404\">{tableName} not found.</response>");
        sb.AppendLine("    /// <response code=\"500\">Internal server error.</response>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    [HttpPut(\"{{{pkParamName}}}\")]");
        sb.AppendLine("    [ProducesResponseType(StatusCodes.Status204NoContent)]");
        sb.AppendLine("    [ProducesResponseType(StatusCodes.Status400BadRequest)]");
        sb.AppendLine("    [ProducesResponseType(StatusCodes.Status404NotFound)]");
        sb.AppendLine("    [ProducesResponseType(StatusCodes.Status500InternalServerError)]");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public async Task<IActionResult> Update({pkType} {pkParamName}, [FromBody] {commandName} command, CancellationToken cancellationToken = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            if ({pkParamName} != command.{pkParamName.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture)}{pkParamName[1..]})");
        sb.AppendLine("            {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"                _logger.LogWarning(\"ID mismatch in update request. URL ID: {{{pkParamName.ToUpper(CultureInfo.InvariantCulture)}}}, Command ID: {{CommandId}}\", {pkParamName}, command.{pkParamName.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture)}{pkParamName[1..]});");
        sb.AppendLine("                return BadRequest(\"ID in URL does not match ID in request body.\");");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogDebug(\"Updating {tableName}. ID: {{{pkParamName.ToUpper(CultureInfo.InvariantCulture)}}}\", {pkParamName});");
        sb.AppendLine();
        sb.AppendLine("            var result = await _mediator.Send(command, cancellationToken);");
        sb.AppendLine();
        sb.AppendLine("            if (!result.IsSuccess)");
        sb.AppendLine("            {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"                _logger.LogWarning(\"Failed to update {tableName}: {{Error}}\", result.Error);");
        sb.AppendLine("                return NotFound(result.Error);");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogInformation(\"{tableName} updated successfully. ID: {{{pkParamName.ToUpper(CultureInfo.InvariantCulture)}}}\", {pkParamName});");
        sb.AppendLine("            return NoContent();");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogError(ex, \"Error updating {tableName}. ID: {{{pkParamName.ToUpper(CultureInfo.InvariantCulture)}}}\", {pkParamName});");
        sb.AppendLine("            return StatusCode(StatusCodes.Status500InternalServerError, \"An error occurred while processing your request.\");");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateDeleteEndpoint(StringBuilder sb, string tableName, string pkType, string pkParamName)
    {
        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Deletes a {tableName}.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// <param name=\"{pkParamName}\">The {tableName} identifier.</param>");
        sb.AppendLine("    /// <param name=\"cancellationToken\">Cancellation token.</param>");
        sb.AppendLine("    /// <returns>No content if successful.</returns>");
        sb.AppendLine("    /// <response code=\"204\">Delete successful.</response>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// <response code=\"404\">{tableName} not found.</response>");
        sb.AppendLine("    /// <response code=\"500\">Internal server error.</response>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    [HttpDelete(\"{{{pkParamName}}}\")]");
        sb.AppendLine("    [ProducesResponseType(StatusCodes.Status204NoContent)]");
        sb.AppendLine("    [ProducesResponseType(StatusCodes.Status404NotFound)]");
        sb.AppendLine("    [ProducesResponseType(StatusCodes.Status500InternalServerError)]");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public async Task<IActionResult> Delete({pkType} {pkParamName}, CancellationToken cancellationToken = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogDebug(\"Deleting {tableName}. ID: {{{pkParamName.ToUpper(CultureInfo.InvariantCulture)}}}\", {pkParamName});");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            var command = new Delete{tableName}Command({pkParamName});");
        sb.AppendLine("            var result = await _mediator.Send(command, cancellationToken);");
        sb.AppendLine();
        sb.AppendLine("            if (!result.IsSuccess)");
        sb.AppendLine("            {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"                _logger.LogWarning(\"Failed to delete {tableName}: {{Error}}\", result.Error);");
        sb.AppendLine("                return NotFound(result.Error);");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogInformation(\"{tableName} deleted successfully. ID: {{{pkParamName.ToUpper(CultureInfo.InvariantCulture)}}}\", {pkParamName});");
        sb.AppendLine("            return NoContent();");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogError(ex, \"Error deleting {tableName}. ID: {{{pkParamName.ToUpper(CultureInfo.InvariantCulture)}}}\", {pkParamName});");
        sb.AppendLine("            return StatusCode(StatusCodes.Status500InternalServerError, \"An error occurred while processing your request.\");");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
    }

    private static Column GetPrimaryKeyColumn(Table table)
    {
        // First, try to find by PrimaryKey property
        if (!string.IsNullOrWhiteSpace(table.PrimaryKey))
        {
            var pkCol = table.Columns.Find(c => c.Name.Equals(table.PrimaryKey, StringComparison.OrdinalIgnoreCase));
            if (pkCol != null)
            {
                return pkCol;
            }
        }

        // Then try PrimaryKeyColumns list
        if (table.PrimaryKeyColumns != null && table.PrimaryKeyColumns.Count > 0)
        {
            var pkColName = table.PrimaryKeyColumns[0];
            var pkCol = table.Columns.Find(c => c.Name.Equals(pkColName, StringComparison.OrdinalIgnoreCase));
            if (pkCol != null)
            {
                return pkCol;
            }
        }

        // Finally, find by IsPrimaryKey flag
        var pkColumn = table.Columns.Find(c => c.IsPrimaryKey);
        if (pkColumn != null)
        {
            return pkColumn;
        }

        // This should never happen due to validation in GenerateAsync, but provide a fallback
        throw new InvalidOperationException($"Unable to determine primary key column for table '{table.Name}'");
    }
}
