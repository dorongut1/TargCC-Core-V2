// <copyright file="QueryGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.CQRS;

using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using TargCC.Core.Generators.Common;
using TargCC.Core.Interfaces.Models;

/// <summary>
/// Generates CQRS Query components including Query, Handler, Validator, and DTO classes.
/// </summary>
/// <remarks>
/// <para>
/// This generator creates complete CQRS query implementations following the MediatR pattern.
/// Each generated query includes proper error handling, logging, and validation.
/// </para>
/// <para>
/// <strong>Generated Components:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Query record implementing IRequest</description></item>
/// <item><description>Handler class implementing IRequestHandler</description></item>
/// <item><description>Validator class using FluentValidation</description></item>
/// <item><description>DTO class for response data</description></item>
/// </list>
/// </remarks>
public class QueryGenerator : IQueryGenerator
{
    private static readonly Action<ILogger, string, string, Exception?> LogGeneratingQuery =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(1, nameof(GenerateAsync)),
            "Generating {QueryType} query for {TableName}");

    private static readonly Action<ILogger, string, Exception?> LogQueryGenerated =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(2, nameof(GenerateAsync)),
            "Successfully generated query components for {TableName}");

    private static readonly Action<ILogger, string, string, Exception?> LogGeneratingIndexQuery =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(3, nameof(GenerateByIndexAsync)),
            "Generating index query for {IndexName} on {TableName}");

    private readonly ILogger<QueryGenerator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryGenerator"/> class.
    /// </summary>
    /// <param name="logger">Logger for tracking generation process.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public QueryGenerator(ILogger<QueryGenerator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<QueryGenerationResult> GenerateAsync(Table table, QueryType queryType, string rootNamespace = "TargCC")
    {
        ArgumentNullException.ThrowIfNull(table);

        if (queryType == QueryType.GetById && table.PrimaryKey == null)
        {
            throw new InvalidOperationException($"Table '{table.Name}' must have a primary key for GetById query.");
        }

        LogGeneratingQuery(_logger, queryType.ToString(), table.Name, null);

        var result = queryType switch
        {
            QueryType.GetById => GenerateGetByIdQuery(table, rootNamespace),
            QueryType.GetAll => GenerateGetAllQuery(table, rootNamespace),
            QueryType.GetByIndex => throw new InvalidOperationException("Use GenerateByIndexAsync for index-based queries."),
            _ => throw new ArgumentOutOfRangeException(nameof(queryType), queryType, "Unknown query type.")
        };

        LogQueryGenerated(_logger, table.Name, null);

        return await Task.FromResult(result);
    }

    /// <inheritdoc/>
    public async Task<QueryGenerationResult> GenerateByIndexAsync(Table table, Index index, string rootNamespace = "TargCC")
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(index);

        LogGeneratingIndexQuery(_logger, index.Name, table.Name, null);

        var result = GenerateGetByIndexQuery(table, index, rootNamespace);

        LogQueryGenerated(_logger, table.Name, null);

        return await Task.FromResult(result);
    }

    /// <inheritdoc/>
    public async Task<string> GenerateDtoAsync(Table table, string rootNamespace = "TargCC")
    {
        ArgumentNullException.ThrowIfNull(table);

        return await Task.FromResult(GenerateDto(table, rootNamespace));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<QueryGenerationResult>> GenerateAllAsync(Table table, string rootNamespace = "TargCC")
    {
        ArgumentNullException.ThrowIfNull(table);

        var results = new List<QueryGenerationResult>();

        if (table.PrimaryKey != null)
        {
            results.Add(GenerateGetByIdQuery(table, rootNamespace));
        }

        results.Add(GenerateGetAllQuery(table, rootNamespace));

        return await Task.FromResult(results);
    }

    private static QueryGenerationResult GenerateGetByIdQuery(Table table, string rootNamespace)
    {
        var pkColumn = table.Columns.Find(c => c.IsPrimaryKey) ?? table.Columns.First(c => c.IsPrimaryKey);
        var pkType = CodeGenerationHelpers.GetCSharpType(pkColumn.DataType);

        // For property names, sanitize but preserve common acronyms like ID
        var pkPropertyName = CodeGenerationHelpers.SanitizeColumnName(pkColumn.Name);

        // Use PascalCase conversion for consistency with other generators
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);

        var queryClassName = $"Get{entityName}Query";
        var handlerClassName = $"Get{entityName}Handler";
        var validatorClassName = $"Get{entityName}Validator";
        var dtoClassName = $"{entityName}Dto";

        return new QueryGenerationResult
        {
            QueryCode = GenerateGetByIdQueryRecord(table, pkColumn, pkType, pkPropertyName, queryClassName, dtoClassName, rootNamespace),
            HandlerCode = GenerateGetByIdHandler(table, pkType, pkPropertyName, queryClassName, handlerClassName, dtoClassName, rootNamespace),
            ValidatorCode = GenerateGetByIdValidator(table, pkType, queryClassName, validatorClassName, rootNamespace),
            DtoCode = GenerateDto(table, rootNamespace),
            QueryClassName = queryClassName,
            HandlerClassName = handlerClassName,
            ValidatorClassName = validatorClassName,
            DtoClassName = dtoClassName,
        };
    }

    private static QueryGenerationResult GenerateGetAllQuery(Table table, string rootNamespace)
    {
        // Use PascalCase conversion for consistency with other generators
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);
        var pluralName = CodeGenerationHelpers.MakePlural(entityName);

        var queryClassName = $"Get{pluralName}Query";
        var handlerClassName = $"Get{pluralName}Handler";
        var validatorClassName = $"Get{pluralName}Validator";
        var dtoClassName = $"{entityName}Dto";

        return new QueryGenerationResult
        {
            QueryCode = GenerateGetAllQueryRecord(table, queryClassName, dtoClassName, rootNamespace),
            HandlerCode = GenerateGetAllHandler(table, queryClassName, handlerClassName, dtoClassName, rootNamespace),
            ValidatorCode = GenerateGetAllValidator(queryClassName, validatorClassName, pluralName, rootNamespace),
            DtoCode = GenerateDto(table, rootNamespace),
            QueryClassName = queryClassName,
            HandlerClassName = handlerClassName,
            ValidatorClassName = validatorClassName,
            DtoClassName = dtoClassName,
        };
    }

    private static QueryGenerationResult GenerateGetByIndexQuery(Table table, Index index, string rootNamespace)
    {
        var indexColumns = GetIndexColumns(table, index);
        var methodSuffix = BuildMethodSuffix(indexColumns);
        var isUnique = index.IsUnique;

        // Use PascalCase conversion for consistency with other generators
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);
        var pluralName = CodeGenerationHelpers.MakePlural(entityName);

        var queryClassName = $"Get{entityName}By{methodSuffix}Query";
        var handlerClassName = $"Get{entityName}By{methodSuffix}Handler";
        var validatorClassName = $"Get{entityName}By{methodSuffix}Validator";
        var dtoClassName = $"{entityName}Dto";

        return new QueryGenerationResult
        {
            QueryCode = GenerateGetByIndexQueryRecord(table, index, indexColumns, queryClassName, dtoClassName, isUnique, rootNamespace),
            HandlerCode = GenerateGetByIndexHandler(table, indexColumns, queryClassName, handlerClassName, dtoClassName, isUnique, methodSuffix, rootNamespace),
            ValidatorCode = GenerateGetByIndexValidator(indexColumns, queryClassName, validatorClassName, pluralName, rootNamespace),
            DtoCode = GenerateDto(table, rootNamespace),
            QueryClassName = queryClassName,
            HandlerClassName = handlerClassName,
            ValidatorClassName = validatorClassName,
            DtoClassName = dtoClassName,
        };
    }

    private static string GenerateGetByIdQueryRecord(
        Table table,
        Column pkColumn,
        string pkType,
        string pkPropertyName,
        string queryClassName,
        string dtoClassName,
        string rootNamespace)
    {
        var sb = new StringBuilder();

        GenerateFileHeader(sb, table.Name, "Query");
        GenerateQueryUsings(sb, rootNamespace);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{CodeGenerationHelpers.MakePlural(table.Name)}.Queries;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Query to retrieve a {table.Name} by its primary key.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// <param name=\"{pkPropertyName}\">The {pkColumn.Name} of the {table.Name} to retrieve.</param>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public record {queryClassName}({pkType} {pkPropertyName}) : IRequest<Result<{dtoClassName}>>;");

        return sb.ToString();
    }

    private static string GenerateGetAllQueryRecord(Table table, string queryClassName, string dtoClassName, string rootNamespace)
    {
        var sb = new StringBuilder();
        var pluralName = CodeGenerationHelpers.MakePlural(table.Name);
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);

        GenerateFileHeader(sb, table.Name, "Query");
        GenerateQueryUsings(sb, rootNamespace);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Queries;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Query to retrieve all {pluralName} with server-side filtering, sorting, and pagination.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"public record {queryClassName} : IRequest<Result<PagedResult<{dtoClassName}>>>");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets or initializes the filters to apply.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {entityName}Filters? Filters {{ get; init; }}");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets or initializes the page number (1-based). Default is 1.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public int Page { get; init; } = 1;");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets or initializes the page size. Default is 100.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public int PageSize { get; init; } = 100;");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets or initializes the field to sort by. Default is \"Id\".");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public string? SortBy { get; init; } = \"Id\";");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets or initializes the sort direction. Default is \"asc\".");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public string SortDirection { get; init; } = \"asc\";");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// {entityName} filters class for filtering queries.");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {entityName}Filters");
        sb.AppendLine("{");
        sb.AppendLine("    // TODO: Add filter properties based on table columns");
        sb.AppendLine("    // Example: public string? Name { get; set; }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateGetByIndexQueryRecord(
        Table table,
        Index index,
        List<Column> indexColumns,
        string queryClassName,
        string dtoClassName,
        bool isUnique,
        string rootNamespace)
    {
        _ = index; // Mark as intentionally unused - reserved for future use

        var sb = new StringBuilder();
        var pluralName = CodeGenerationHelpers.MakePlural(table.Name);

        GenerateFileHeader(sb, table.Name, "Query");
        GenerateQueryUsings(sb, rootNamespace);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Queries;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        var columnNames = indexColumns.Select(c => CodeGenerationHelpers.SanitizeColumnName(c.Name));
        var returnDesc = isUnique ? $"a {table.Name}" : $"{pluralName}";
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Query to retrieve {returnDesc} by {string.Join(" and ", columnNames)}.");
        sb.AppendLine("/// </summary>");

        sb.Append(string.Join(
            Environment.NewLine,
            indexColumns.Select(col =>
    {
        var paramName = CodeGenerationHelpers.ToCamelCase(CodeGenerationHelpers.SanitizeColumnName(col.Name));
        var propertyName = char.ToUpperInvariant(paramName[0]).ToString() + paramName.AsSpan(1).ToString();
        return string.Format(CultureInfo.InvariantCulture, $"/// <param name=\"{propertyName}\">The {CodeGenerationHelpers.SanitizeColumnName(col.Name)} to search for.</param>");
    })));
        sb.AppendLine();

        var parameters = indexColumns.Select(col =>
        {
            var type = CodeGenerationHelpers.GetCSharpType(col.DataType);
            var name = CodeGenerationHelpers.SanitizeColumnName(col.Name);
            if (col.IsNullable && type != "string")
            {
                type += "?";
            }

            return $"{type} {name}";
        });

        var resultType = isUnique ? $"Result<{dtoClassName}?>" : $"Result<IEnumerable<{dtoClassName}>>";
        sb.AppendLine(CultureInfo.InvariantCulture, $"public record {queryClassName}({string.Join(", ", parameters)}) : IRequest<{resultType}>;");

        return sb.ToString();
    }

    private static string GenerateGetByIdHandler(
        Table table,
        string pkType,
        string pkPropertyName,
        string queryClassName,
        string handlerClassName,
        string dtoClassName,
        string rootNamespace)
    {
        _ = pkType; // Mark as intentionally unused - reserved for future type-specific handling

        var sb = new StringBuilder();
        var pluralName = CodeGenerationHelpers.MakePlural(table.Name);
        var repoInterfaceName = $"I{table.Name}Repository";
        const string repoFieldName = "_repository";

        GenerateFileHeader(sb, table.Name, "Handler");
        GenerateHandlerUsings(sb, rootNamespace);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Queries;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Handles the {queryClassName} request.");
        sb.AppendLine("/// </summary>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {handlerClassName} : IRequestHandler<{queryClassName}, Result<{dtoClassName}>>");
        sb.AppendLine("{");

        sb.AppendLine(CultureInfo.InvariantCulture, $"    private readonly {repoInterfaceName} {repoFieldName};");
        sb.AppendLine("    private readonly IMapper _mapper;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    private readonly ILogger<{handlerClassName}> _logger;");
        sb.AppendLine();

        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Initializes a new instance of the <see cref=\"{handlerClassName}\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {handlerClassName}(");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        {repoInterfaceName} repository,");
        sb.AppendLine("        IMapper mapper,");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        ILogger<{handlerClassName}> logger)");
        sb.AppendLine("    {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        {repoFieldName} = repository ?? throw new ArgumentNullException(nameof(repository));");
        sb.AppendLine("        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));");
        sb.AppendLine("        _logger = logger ?? throw new ArgumentNullException(nameof(logger));");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    /// <inheritdoc/>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public async Task<Result<{dtoClassName}>> Handle({queryClassName} request, CancellationToken cancellationToken)");
        sb.AppendLine("    {");
        sb.AppendLine("        ArgumentNullException.ThrowIfNull(request);");
        sb.AppendLine();

        sb.AppendLine(CultureInfo.InvariantCulture, $"        _logger.LogDebug(\"Getting {table.Name} by ID: {{Id}}\", request.{pkPropertyName});");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            var entity = await {repoFieldName}.GetByIdAsync(request.{pkPropertyName}, cancellationToken);");
        sb.AppendLine();
        sb.AppendLine("            if (entity is null)");
        sb.AppendLine("            {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"                _logger.LogWarning(\"{table.Name} not found with ID: {{Id}}\", request.{pkPropertyName});");
        sb.AppendLine(CultureInfo.InvariantCulture, $"                return Result<{dtoClassName}>.Failure(\"{table.Name} not found.\");");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            var dto = _mapper.Map<{dtoClassName}>(entity);");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogDebug(\"Successfully retrieved {table.Name} with ID: {{Id}}\", request.{pkPropertyName});");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            return Result<{dtoClassName}>.Success(dto);");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogError(ex, \"Error retrieving {table.Name} with ID: {{Id}}\", request.{pkPropertyName});");
        sb.AppendLine("            throw;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateGetAllHandler(
        Table table,
        string queryClassName,
        string handlerClassName,
        string dtoClassName,
        string rootNamespace)
    {
        var sb = new StringBuilder();
        var pluralName = CodeGenerationHelpers.MakePlural(table.Name);
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);

        // Check if entity name conflicts with System types (e.g., Lookup, Task, etc.)
        string? entityAliasName = entityName switch
        {
            "Lookup" or "Task" or "Action" or "Exception" or "Environment" => entityName,
            _ => null
        };

        GenerateFileHeader(sb, table.Name, "Handler");
        GenerateHandlerUsings(sb, rootNamespace, entityAliasName);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Queries;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Handles the {queryClassName} request with server-side filtering, sorting, and pagination.");
        sb.AppendLine("/// </summary>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {handlerClassName} : IRequestHandler<{queryClassName}, Result<PagedResult<{dtoClassName}>>>");
        sb.AppendLine("{");

        sb.AppendLine("    private readonly IApplicationDbContext _context;");
        sb.AppendLine("    private readonly IMapper _mapper;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    private readonly ILogger<{handlerClassName}> _logger;");
        sb.AppendLine();

        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Initializes a new instance of the <see cref=\"{handlerClassName}\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {handlerClassName}(");
        sb.AppendLine("        IApplicationDbContext context,");
        sb.AppendLine("        IMapper mapper,");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        ILogger<{handlerClassName}> logger)");
        sb.AppendLine("    {");
        sb.AppendLine("        _context = context ?? throw new ArgumentNullException(nameof(context));");
        sb.AppendLine("        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));");
        sb.AppendLine("        _logger = logger ?? throw new ArgumentNullException(nameof(logger));");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    /// <inheritdoc/>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public async Task<Result<PagedResult<{dtoClassName}>>> Handle({queryClassName} request, CancellationToken cancellationToken)");
        sb.AppendLine("    {");
        sb.AppendLine("        ArgumentNullException.ThrowIfNull(request);");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"        _logger.LogDebug(\"Getting {pluralName} - Page: {{Page}}, Size: {{Size}}, SortBy: {{SortBy}}\", request.Page, request.PageSize, request.SortBy);");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            var query = _context.{pluralName}.AsQueryable();");
        sb.AppendLine();
        sb.AppendLine("            // Apply filters");
        sb.AppendLine("            query = ApplyFilters(query, request.Filters);");
        sb.AppendLine();
        sb.AppendLine("            // Get total count BEFORE pagination");
        sb.AppendLine("            var totalCount = await query.CountAsync(cancellationToken);");
        sb.AppendLine();
        sb.AppendLine("            // Apply sorting");
        sb.AppendLine("            query = ApplySorting(query, request.SortBy, request.SortDirection);");
        sb.AppendLine();
        sb.AppendLine("            // Apply pagination");
        sb.AppendLine("            query = query");
        sb.AppendLine("                .Skip((request.Page - 1) * request.PageSize)");
        sb.AppendLine("                .Take(request.PageSize);");
        sb.AppendLine();
        sb.AppendLine("            var entities = await query.ToListAsync(cancellationToken);");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            var dtos = _mapper.Map<List<{dtoClassName}>>(entities);");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            var result = new PagedResult<{dtoClassName}>");
        sb.AppendLine("            {");
        sb.AppendLine("                Items = dtos,");
        sb.AppendLine("                TotalCount = totalCount,");
        sb.AppendLine("                Page = request.Page,");
        sb.AppendLine("                PageSize = request.PageSize");
        sb.AppendLine("            };");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogDebug(\"Successfully retrieved {{Count}} of {{Total}} {pluralName}\", dtos.Count, totalCount);");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            return Result<PagedResult<{dtoClassName}>>.Success(result);");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogError(ex, \"Error retrieving {pluralName}\");");
        sb.AppendLine("            throw;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Use entity alias if there's a name conflict
        var entityTypeName = entityAliasName != null ? $"{entityAliasName}Entity" : entityName;

        // Find primary key column for sorting
        var pkColumn = table.Columns.Find(c => c.IsPrimaryKey);
        var pkPropertyName = pkColumn != null ? Entities.PrefixHandler.GetPropertyName(pkColumn) : "ID";

        // Add ApplyFilters method
        sb.AppendLine(CultureInfo.InvariantCulture, $"    private static IQueryable<{entityTypeName}> ApplyFilters(IQueryable<{entityTypeName}> query, {entityName}Filters? filters)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (filters == null)");
        sb.AppendLine("        {");
        sb.AppendLine("            return query;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // TODO: Add filter logic based on table columns");
        sb.AppendLine("        // Example:");
        sb.AppendLine("        // if (!string.IsNullOrEmpty(filters.Name))");
        sb.AppendLine("        // {");
        sb.AppendLine("        //     query = query.Where(e => e.Name.Contains(filters.Name));");
        sb.AppendLine("        // }");
        sb.AppendLine();
        sb.AppendLine("        return query;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Add ApplySorting method
        sb.AppendLine(CultureInfo.InvariantCulture, $"    private static IQueryable<{entityTypeName}> ApplySorting(IQueryable<{entityTypeName}> query, string? sortBy, string? sortDirection)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (string.IsNullOrEmpty(sortBy))");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            return query.OrderBy(e => e.{pkPropertyName}); // Default sort");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        var isDescending = sortDirection?.ToLower() == \"desc\";");
        sb.AppendLine();
        sb.AppendLine("        // TODO: Add sorting logic based on table columns");
        sb.AppendLine("        return sortBy.ToLower() switch");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            \"id\" => isDescending ? query.OrderByDescending(e => e.{pkPropertyName}) : query.OrderBy(e => e.{pkPropertyName}),");
        sb.AppendLine("            // Add more sortable columns here");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _ => query.OrderBy(e => e.{pkPropertyName}) // Default to {pkPropertyName} if unknown field");
        sb.AppendLine("        };");
        sb.AppendLine("    }");

        sb.AppendLine("}");

        return sb.ToString();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S107:MethodsWithTooManyParameters", Justification = "All parameters are necessary for flexible code generation")]
    private static string GenerateGetByIndexHandler(
        Table table,
        List<Column> indexColumns,
        string queryClassName,
        string handlerClassName,
        string dtoClassName,
        bool isUnique,
        string methodSuffix,
        string rootNamespace)
    {
        var sb = new StringBuilder();
        var pluralName = CodeGenerationHelpers.MakePlural(table.Name);
        var repoInterfaceName = $"I{table.Name}Repository";
        const string repoFieldName = "_repository";
        var resultType = isUnique ? $"Result<{dtoClassName}?>" : $"Result<IEnumerable<{dtoClassName}>>";

        GenerateFileHeader(sb, table.Name, "Handler");
        GenerateHandlerUsings(sb, rootNamespace);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Queries;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Handles the {queryClassName} request.");
        sb.AppendLine("/// </summary>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {handlerClassName} : IRequestHandler<{queryClassName}, {resultType}>");
        sb.AppendLine("{");

        sb.AppendLine(CultureInfo.InvariantCulture, $"    private readonly {repoInterfaceName} {repoFieldName};");
        sb.AppendLine("    private readonly IMapper _mapper;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    private readonly ILogger<{handlerClassName}> _logger;");
        sb.AppendLine();

        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Initializes a new instance of the <see cref=\"{handlerClassName}\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {handlerClassName}(");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        {repoInterfaceName} repository,");
        sb.AppendLine("        IMapper mapper,");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        ILogger<{handlerClassName}> logger)");
        sb.AppendLine("    {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        {repoFieldName} = repository ?? throw new ArgumentNullException(nameof(repository));");
        sb.AppendLine("        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));");
        sb.AppendLine("        _logger = logger ?? throw new ArgumentNullException(nameof(logger));");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    /// <inheritdoc/>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public async Task<{resultType}> Handle({queryClassName} request, CancellationToken cancellationToken)");
        sb.AppendLine("    {");
        sb.AppendLine("        ArgumentNullException.ThrowIfNull(request);");
        sb.AppendLine();

        var logParams = string.Join(", ", indexColumns.Select(c => $"request.{CodeGenerationHelpers.SanitizeColumnName(c.Name)}"));
        var logPlaceholders = string.Join(", ", indexColumns.Select(c => $"{{{CodeGenerationHelpers.SanitizeColumnName(c.Name)}}}"));
        sb.AppendLine(CultureInfo.InvariantCulture, $"        _logger.LogDebug(\"Getting {table.Name} by {methodSuffix}: {logPlaceholders}\", {logParams});");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");

        var repoMethodName = $"GetBy{methodSuffix}Async";
        var repoParams = string.Join(", ", indexColumns.Select(c => $"request.{CodeGenerationHelpers.SanitizeColumnName(c.Name)}"));

        if (isUnique)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"            var entity = await {repoFieldName}.{repoMethodName}({repoParams}, cancellationToken);");
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"            var dto = entity is not null ? _mapper.Map<{dtoClassName}>(entity) : null;");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogDebug(\"{table.Name} found: {{Found}}\", dto is not null);");
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"            return Result<{dtoClassName}?>.Success(dto);");
        }
        else
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"            var entities = await {repoFieldName}.{repoMethodName}({repoParams}, cancellationToken);");
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"            var dtos = _mapper.Map<IEnumerable<{dtoClassName}>>(entities);");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogDebug(\"Retrieved {{Count}} {pluralName}\", dtos.Count());");
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"            return Result<IEnumerable<{dtoClassName}>>.Success(dtos);");
        }

        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogError(ex, \"Error retrieving {table.Name} by {methodSuffix}\");");
        sb.AppendLine("            throw;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateGetByIdValidator(Table table, string pkType, string queryClassName, string validatorClassName, string rootNamespace)
    {
        var sb = new StringBuilder();
        var pluralName = CodeGenerationHelpers.MakePlural(table.Name);
        var pkColumn = table.Columns.Find(c => c.IsPrimaryKey) ?? table.Columns.First(c => c.IsPrimaryKey);
        var pkPropertyName = pkColumn.Name;

        GenerateFileHeader(sb, table.Name, "Validator");
        GenerateValidatorUsings(sb);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Queries;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Validator for {queryClassName}.");
        sb.AppendLine("/// </summary>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {validatorClassName} : AbstractValidator<{queryClassName}>");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Initializes a new instance of the <see cref=\"{validatorClassName}\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {validatorClassName}()");
        sb.AppendLine("    {");

        if (pkType is "int" or "long" or "short")
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"        RuleFor(x => x.{pkPropertyName})");
            sb.AppendLine("            .GreaterThan(0)");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            .WithMessage(\"{pkPropertyName} must be greater than 0.\");");
        }
        else if (pkType is "Guid" or "string")
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"        RuleFor(x => x.{pkPropertyName})");
            sb.AppendLine("            .NotEmpty()");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            .WithMessage(\"{pkPropertyName} is required.\");");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateGetAllValidator(string queryClassName, string validatorClassName, string pluralName, string rootNamespace)
    {
        var sb = new StringBuilder();

        GenerateFileHeader(sb, "Multiple", "Validator");
        GenerateValidatorUsings(sb);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Queries;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Validator for {queryClassName}.");
        sb.AppendLine("/// </summary>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {validatorClassName} : AbstractValidator<{queryClassName}>");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Initializes a new instance of the <see cref=\"{validatorClassName}\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {validatorClassName}()");
        sb.AppendLine("    {");
        sb.AppendLine("        RuleFor(x => x.PageNumber)");
        sb.AppendLine("            .GreaterThan(0)");
        sb.AppendLine("            .WithMessage(\"Page number must be greater than 0.\");");
        sb.AppendLine();
        sb.AppendLine("        RuleFor(x => x.PageSize)");
        sb.AppendLine("            .InclusiveBetween(1, 100)");
        sb.AppendLine("            .WithMessage(\"Page size must be between 1 and 100.\");");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateGetByIndexValidator(
        List<Column> indexColumns,
        string queryClassName,
        string validatorClassName,
        string pluralName,
        string rootNamespace)
    {
        var sb = new StringBuilder();

        GenerateFileHeader(sb, "Index", "Validator");
        GenerateValidatorUsings(sb);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Queries;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Validator for {queryClassName}.");
        sb.AppendLine("/// </summary>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {validatorClassName} : AbstractValidator<{queryClassName}>");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Initializes a new instance of the <see cref=\"{validatorClassName}\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {validatorClassName}()");
        sb.AppendLine("    {");

        foreach (var col in indexColumns)
        {
            var propName = CodeGenerationHelpers.SanitizeColumnName(col.Name);
            var type = CodeGenerationHelpers.GetCSharpType(col.DataType);

            if (!col.IsNullable)
            {
                if (type == "string")
                {
                    sb.AppendLine(CultureInfo.InvariantCulture, $"        RuleFor(x => x.{propName})");
                    sb.AppendLine("            .NotEmpty()");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            .WithMessage(\"{propName} is required.\");");

                    if (col.MaxLength.HasValue && col.MaxLength.Value > 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine(CultureInfo.InvariantCulture, $"        RuleFor(x => x.{propName})");
                        sb.AppendLine(CultureInfo.InvariantCulture, $"            .MaximumLength({col.MaxLength.Value})");
                        sb.AppendLine(CultureInfo.InvariantCulture, $"            .WithMessage(\"{propName} must not exceed {col.MaxLength.Value} characters.\");");
                    }
                }
                else if (type is "int" or "long")
                {
                    sb.AppendLine(CultureInfo.InvariantCulture, $"        RuleFor(x => x.{propName})");
                    sb.AppendLine("            .GreaterThan(0)");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            .WithMessage(\"{propName} must be greater than 0.\");");
                }
            }

            sb.AppendLine();
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateDto(Table table, string rootNamespace)
    {
        var sb = new StringBuilder();

        // Use GetClassName to convert table name to proper entity name (removes prefixes, handles PascalCase)
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);
        var dtoClassName = $"{entityName}Dto";

        GenerateFileHeader(sb, table.Name, "DTO");
        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.DTOs;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Data Transfer Object for {table.Name} entity.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("/// <remarks>");
        sb.AppendLine("/// This DTO excludes sensitive fields (encrypted, hashed) and exposes only safe properties.");
        sb.AppendLine("/// </remarks>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {dtoClassName}");
        sb.AppendLine("{");

        var seenProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var column in table.Columns)
        {
            if (column.Name.StartsWith("eno_", StringComparison.OrdinalIgnoreCase) ||
                column.Name.StartsWith("ent_", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var propName = CodeGenerationHelpers.SanitizeColumnName(column.Name);

            // Skip duplicate property names (common in tables with multiple FK relationships to same table)
            if (seenProperties.Contains(propName))
            {
                // Silently skip duplicate properties
                continue;
            }

            seenProperties.Add(propName);

            var propType = CodeGenerationHelpers.GetCSharpType(column.DataType);

            // Primary keys are never nullable in DTOs
            var nullableSuffix = column.IsNullable && propType != "string" && !column.IsPrimaryKey ? "?" : string.Empty;
            var defaultValue = propType == "string" ? " = string.Empty;" : string.Empty;

            sb.AppendLine("    /// <summary>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Gets or sets the {propName}.");
            sb.AppendLine("    /// </summary>");

            sb.AppendLine(CultureInfo.InvariantCulture, $"    public {propType}{nullableSuffix} {propName} {{ get; init; }}{defaultValue}");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void GenerateFileHeader(StringBuilder sb, string tableName, string componentType)
    {
        sb.AppendLine("// <auto-generated>");
        sb.AppendLine("//     This code was generated by TargCC");
        sb.AppendLine(CultureInfo.InvariantCulture, $"//     Generated for: {tableName} ({componentType})");
        sb.AppendLine(CultureInfo.InvariantCulture, $"//     Generation date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("//");
        sb.AppendLine("//     Changes to this file may cause incorrect behavior and will be lost if");
        sb.AppendLine("//     the code is regenerated.");
        sb.AppendLine("// </auto-generated>");
        sb.AppendLine();
    }

    private static void GenerateQueryUsings(StringBuilder sb, string rootNamespace)
    {
        sb.AppendLine("using MediatR;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Application.Common.Models;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Application.DTOs;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Domain.Common;");
        sb.AppendLine();
    }

    private static void GenerateHandlerUsings(StringBuilder sb, string rootNamespace, string? entityAliasName = null)
    {
        sb.AppendLine("using AutoMapper;");
        sb.AppendLine("using MediatR;");
        sb.AppendLine("using Microsoft.EntityFrameworkCore;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Application.Common.Interfaces;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Application.Common.Models;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Application.DTOs;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Domain.Common;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Domain.Entities;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Domain.Interfaces;");

        // Add alias for entity names that conflict with System types (e.g., Lookup)
        if (!string.IsNullOrEmpty(entityAliasName))
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"using {entityAliasName}Entity = {rootNamespace}.Domain.Entities.{entityAliasName};");
        }

        sb.AppendLine();
    }

    private static void GenerateValidatorUsings(StringBuilder sb)
    {
        sb.AppendLine("using FluentValidation;");
        sb.AppendLine();
    }

    private static List<Column> GetIndexColumns(Table table, Index index)
    {
        return
            [.. index.ColumnNames
            .Select(colName => table.Columns.Find(c =>
                c.Name.Equals(colName, StringComparison.OrdinalIgnoreCase)))
            .Where(c => c != null)
            .Cast<Column>()];
    }

    private static string BuildMethodSuffix(List<Column> columns)
    {
        return string.Join("And", columns.Select(c => CodeGenerationHelpers.SanitizeColumnName(c.Name)));
    }
}
