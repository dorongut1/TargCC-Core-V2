using System.Linq;

// <copyright file="CommandGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace TargCC.Core.Generators.CQRS;

using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using TargCC.Core.Generators.Common;
using TargCC.Core.Interfaces.Models;

/// <summary>
/// Generates CQRS Command components including Command, Handler, and Validator classes.
/// </summary>
/// <remarks>
/// <para>
/// This generator creates complete CQRS command implementations following the MediatR pattern.
/// Each generated command includes proper error handling, logging, and validation.
/// </para>
/// <para>
/// <strong>Generated Components:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Command record implementing IRequest</description></item>
/// <item><description>Handler class implementing IRequestHandler</description></item>
/// <item><description>Validator class using FluentValidation</description></item>
/// </list>
/// <para>
/// <strong>Column Handling:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Identity columns excluded from Create commands</description></item>
/// <item><description>Read-only columns (clc_, blg_, agg_) excluded from all commands</description></item>
/// <item><description>Hashed columns (eno_) included in Create with special handling</description></item>
/// <item><description>Encrypted columns (ent_) included with automatic encryption</description></item>
/// </list>
/// </remarks>
public class CommandGenerator : ICommandGenerator
{
    private static readonly Action<ILogger, string, string, Exception?> LogGeneratingCommand =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(1, nameof(GenerateAsync)),
            "Generating {CommandType} command for {TableName}");

    private static readonly Action<ILogger, string, Exception?> LogCommandGenerated =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(2, nameof(GenerateAsync)),
            "Successfully generated command components for {TableName}");

    private static readonly Action<ILogger, string, int, Exception?> LogGeneratingAllCommands =
        LoggerMessage.Define<string, int>(
            LogLevel.Information,
            new EventId(3, nameof(GenerateAllAsync)),
            "Generating all commands for {TableName} ({Count} commands)");

    private readonly ILogger<CommandGenerator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandGenerator"/> class.
    /// </summary>
    /// <param name="logger">Logger for tracking generation process.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public CommandGenerator(ILogger<CommandGenerator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<CommandGenerationResult> GenerateAsync(Table table, CommandType commandType, string rootNamespace = "TargCC")
    {
        ArgumentNullException.ThrowIfNull(table);

        // Skip Create command for views (read-only)
        if (commandType == CommandType.Create && table.IsView)
        {
            return await Task.FromResult(new CommandGenerationResult
            {
                CommandCode = string.Empty,
                HandlerCode = string.Empty,
                ValidatorCode = string.Empty
            });
        }

        if (commandType != CommandType.Create && table.PrimaryKey == null)
        {
            throw new InvalidOperationException(
                $"Table '{table.Name}' must have a primary key for {commandType} command.");
        }

        LogGeneratingCommand(_logger, commandType.ToString(), table.Name, null);

        var result = commandType switch
        {
            CommandType.Create => GenerateCreateCommand(table, rootNamespace),
            CommandType.Update => GenerateUpdateCommand(table, rootNamespace),
            CommandType.Delete => GenerateDeleteCommand(table, rootNamespace),
            _ => throw new ArgumentOutOfRangeException(nameof(commandType), commandType, "Unknown command type.")
        };

        LogCommandGenerated(_logger, table.Name, null);

        return await Task.FromResult(result);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CommandGenerationResult>> GenerateAllAsync(Table table, string rootNamespace = "TargCC")
    {
        ArgumentNullException.ThrowIfNull(table);

        LogGeneratingAllCommands(_logger, table.Name, 3, null);

        var results = new List<CommandGenerationResult>();

        // Skip Create command for views (read-only)
        if (!table.IsView)
        {
            results.Add(GenerateCreateCommand(table, rootNamespace));
        }

        if (table.PrimaryKey != null)
        {
            results.Add(GenerateUpdateCommand(table, rootNamespace));
            results.Add(GenerateDeleteCommand(table, rootNamespace));
        }

        return await Task.FromResult(results);
    }

    /// <inheritdoc/>
    public IEnumerable<Column> GetCreateColumns(Table table)
    {
        ArgumentNullException.ThrowIfNull(table);

        return GetCreateColumnsInternal(table);
    }

    /// <inheritdoc/>
    public IEnumerable<Column> GetUpdateColumns(Table table)
    {
        ArgumentNullException.ThrowIfNull(table);

        return table.Columns.Where(c =>
            !c.IsPrimaryKey &&
            !c.IsIdentity &&
            !c.Name.StartsWith("eno_", StringComparison.OrdinalIgnoreCase) &&
            !CodeGenerationHelpers.IsReadOnlyColumn(c.Name));
    }

    /// <inheritdoc/>
    public string GenerateValidationRules(Column column)
    {
        var sb = new StringBuilder();
        var propName = CodeGenerationHelpers.SanitizeColumnName(column.Name);
        var propType = CodeGenerationHelpers.GetCSharpType(column.DataType);

        sb.AppendLine(CultureInfo.InvariantCulture, $"        RuleFor(x => x.{propName})");

        var rules = new List<string>();

        // Not nullable validation
        if (!column.IsNullable)
        {
            if (propType == "string")
            {
                rules.Add("            .NotEmpty()");
            }
            else if (propType is "int" or "long" or "short")
            {
                // For numeric types, we might want GreaterThan(0) for IDs
                // but for regular numbers, just not null is fine
            }
        }

        // String length validation
        if (propType == "string" && column.MaxLength.HasValue && column.MaxLength.Value > 0)
        {
            rules.Add(string.Format(
                CultureInfo.InvariantCulture,
                "            .MaximumLength({0})",
                column.MaxLength.Value));
        }

        // Email validation
        if (column.Name.Contains("email", StringComparison.OrdinalIgnoreCase))
        {
            rules.Add("            .EmailAddress()");
        }

        // Phone validation
        if (column.Name.Contains("phone", StringComparison.OrdinalIgnoreCase))
        {
            rules.Add("            .Matches(@\"^[+]?[0-9\\s\\-().]{7,20}$\")");
        }

        // Password validation (eno_ prefix)
        if (column.Name.StartsWith("eno_", StringComparison.OrdinalIgnoreCase) &&
            column.Name.Contains("password", StringComparison.OrdinalIgnoreCase))
        {
            rules.Add("            .MinimumLength(8)");
        }

        if (rules.Count > 0)
        {
            sb.Append(string.Join(Environment.NewLine, rules));
            sb.AppendLine(";");
        }
        else
        {
            // Remove the RuleFor line if no rules
            sb.Clear();
        }

        return sb.ToString();
    }

    private static CommandGenerationResult GenerateCreateCommand(Table table, string rootNamespace)
    {
        // Use PascalCase conversion for consistency with other generators
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);

        var commandClassName = $"Create{entityName}Command";
        var handlerClassName = $"Create{entityName}Handler";
        var validatorClassName = $"Create{entityName}Validator";

        return new CommandGenerationResult
        {
            CommandCode = GenerateCreateCommandRecord(table, commandClassName, rootNamespace),
            HandlerCode = GenerateCreateHandler(table, commandClassName, handlerClassName, rootNamespace),
            ValidatorCode = GenerateCreateValidator(table, commandClassName, validatorClassName, rootNamespace),
            CommandClassName = commandClassName,
            HandlerClassName = handlerClassName,
            ValidatorClassName = validatorClassName,
            CommandType = CommandType.Create,
        };
    }

    private static CommandGenerationResult GenerateUpdateCommand(Table table, string rootNamespace)
    {
        // Use PascalCase conversion for consistency with other generators
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);

        var commandClassName = $"Update{entityName}Command";
        var handlerClassName = $"Update{entityName}Handler";
        var validatorClassName = $"Update{entityName}Validator";

        return new CommandGenerationResult
        {
            CommandCode = GenerateUpdateCommandRecord(table, commandClassName, rootNamespace),
            HandlerCode = GenerateUpdateHandler(table, commandClassName, handlerClassName, rootNamespace),
            ValidatorCode = GenerateUpdateValidator(table, commandClassName, validatorClassName, rootNamespace),
            CommandClassName = commandClassName,
            HandlerClassName = handlerClassName,
            ValidatorClassName = validatorClassName,
            CommandType = CommandType.Update,
        };
    }

    private static CommandGenerationResult GenerateDeleteCommand(Table table, string rootNamespace)
    {
        // Use PascalCase conversion for consistency with other generators
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);

        var commandClassName = $"Delete{entityName}Command";
        var handlerClassName = $"Delete{entityName}Handler";
        var validatorClassName = $"Delete{entityName}Validator";

        return new CommandGenerationResult
        {
            CommandCode = GenerateDeleteCommandRecord(table, commandClassName, rootNamespace),
            HandlerCode = GenerateDeleteHandler(table, commandClassName, handlerClassName, rootNamespace),
            ValidatorCode = GenerateDeleteValidator(table, commandClassName, validatorClassName, rootNamespace),
            CommandClassName = commandClassName,
            HandlerClassName = handlerClassName,
            ValidatorClassName = validatorClassName,
            CommandType = CommandType.Delete,
        };
    }

    private static string GenerateCreateCommandRecord(Table table, string commandClassName, string rootNamespace)
    {
        var sb = new StringBuilder();
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);
        var pluralName = CodeGenerationHelpers.MakePlural(entityName);
        var pkColumn = table.Columns.Find(c => c.IsPrimaryKey) ?? table.Columns.First(c => c.IsPrimaryKey);
        var pkType = CodeGenerationHelpers.GetCSharpType(pkColumn.DataType);
        if (pkColumn.IsNullable && pkType != "string")
        {
            pkType += "?";
        }

        GenerateFileHeader(sb, table.Name, "Command");
        GenerateCommandUsings(sb, rootNamespace);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Commands;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Command to create a new {table.Name}.");
        sb.AppendLine("/// </summary>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public record {commandClassName} : IRequest<Result<{pkType}>>");
        sb.AppendLine("{");

        var createColumns = GetCreateColumnsInternal(table);
        var seenProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var column in createColumns)
        {
            var propName = CodeGenerationHelpers.SanitizeColumnName(column.Name);

            // Skip duplicate property names
            if (seenProperties.Contains(propName))
            {
                continue; // Skip silently in generated code
            }

            seenProperties.Add(propName);

            var propType = CodeGenerationHelpers.GetCSharpType(column.DataType);
            var nullableSuffix = column.IsNullable && propType != "string" ? "?" : string.Empty;
            var defaultValue = GetDefaultValue(propType, column.IsNullable);

            sb.AppendLine("    /// <summary>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Gets or sets the {propName}.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    public {propType}{nullableSuffix} {propName} {{ get; init; }}{defaultValue}");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateUpdateCommandRecord(Table table, string commandClassName, string rootNamespace)
    {
        var sb = new StringBuilder();
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);
        var pluralName = CodeGenerationHelpers.MakePlural(entityName);
        var pkColumn = table.Columns.Find(c => c.IsPrimaryKey) ?? table.Columns.First(c => c.IsPrimaryKey);
        var pkType = CodeGenerationHelpers.GetCSharpType(pkColumn.DataType);

        GenerateFileHeader(sb, table.Name, "Command");
        GenerateCommandUsings(sb, rootNamespace);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Commands;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Command to update an existing {table.Name}.");
        sb.AppendLine("/// </summary>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public record {commandClassName} : IRequest<Result>");
        sb.AppendLine("{");

        // Primary key first
        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Gets or sets the {pkColumn.Name} (primary key).");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {pkType} {pkColumn.Name} {{ get; init; }}");
        sb.AppendLine();

        var updateColumns = GetUpdateColumnsInternal(table);
        var seenProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var column in updateColumns)
        {
            var propName = CodeGenerationHelpers.SanitizeColumnName(column.Name);

            // Skip duplicate property names
            if (seenProperties.Contains(propName))
            {
                continue; // Skip silently in generated code
            }

            seenProperties.Add(propName);

            var propType = CodeGenerationHelpers.GetCSharpType(column.DataType);
            var nullableSuffix = column.IsNullable && propType != "string" ? "?" : string.Empty;
            var defaultValue = GetDefaultValue(propType, column.IsNullable);

            sb.AppendLine("    /// <summary>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Gets or sets the {propName}.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    public {propType}{nullableSuffix} {propName} {{ get; init; }}{defaultValue}");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateDeleteCommandRecord(Table table, string commandClassName, string rootNamespace)
    {
        var sb = new StringBuilder();
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);
        var pluralName = CodeGenerationHelpers.MakePlural(entityName);
        var pkColumn = table.Columns.Find(c => c.IsPrimaryKey) ?? table.Columns.First(c => c.IsPrimaryKey);
        var pkType = CodeGenerationHelpers.GetCSharpType(pkColumn.DataType);

        GenerateFileHeader(sb, table.Name, "Command");
        GenerateCommandUsings(sb, rootNamespace);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Commands;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Command to delete a {table.Name} by its primary key.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// <param name=\"{pkColumn.Name}\">The {pkColumn.Name} of the {table.Name} to delete.</param>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public record {commandClassName}({pkType} {pkColumn.Name}) : IRequest<Result>;");

        return sb.ToString();
    }

    private static string GenerateCreateHandler(Table table, string commandClassName, string handlerClassName, string rootNamespace)
    {
        var sb = new StringBuilder();
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);
        var pluralName = CodeGenerationHelpers.MakePlural(entityName);
        var repoInterfaceName = $"I{entityName}Repository";
        const string repoFieldName = "_repository";
        var pkColumn = table.Columns.Find(c => c.IsPrimaryKey) ?? table.Columns.First(c => c.IsPrimaryKey);
        var pkPropName = CodeGenerationHelpers.SanitizeColumnName(pkColumn.Name);
        var pkType = CodeGenerationHelpers.GetCSharpType(pkColumn.DataType);
        if (pkColumn.IsNullable && pkType != "string")
        {
            pkType += "?";
        }

        // Check for namespace collision
        string? entityAliasName = entityName == pluralName ? entityName : null;

        GenerateFileHeader(sb, table.Name, "Handler");
        GenerateHandlerUsings(sb, rootNamespace, entityAliasName);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Commands;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Handles the {commandClassName} request.");
        sb.AppendLine("/// </summary>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {handlerClassName} : IRequestHandler<{commandClassName}, Result<{pkType}>>");
        sb.AppendLine("{");

        sb.AppendLine(CultureInfo.InvariantCulture, $"    private readonly {repoInterfaceName} {repoFieldName};");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    private readonly ILogger<{handlerClassName}> _logger;");
        sb.AppendLine();

        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Initializes a new instance of the <see cref=\"{handlerClassName}\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {handlerClassName}(");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        {repoInterfaceName} repository,");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        ILogger<{handlerClassName}> logger)");
        sb.AppendLine("    {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        {repoFieldName} = repository ?? throw new ArgumentNullException(nameof(repository));");
        sb.AppendLine("        _logger = logger ?? throw new ArgumentNullException(nameof(logger));");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    /// <inheritdoc/>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public async Task<Result<{pkType}>> Handle({commandClassName} request, CancellationToken cancellationToken)");
        sb.AppendLine("    {");
        sb.AppendLine("        ArgumentNullException.ThrowIfNull(request);");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"        _logger.LogDebug(\"Creating new {table.Name}\");");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");

        // Use fully qualified name for Task entity to avoid ambiguity with System.Threading.Tasks.Task
        var entityNameForInstantiation = entityName == "Task" ? $"{rootNamespace}.Domain.Entities.Task" : entityName;
        sb.AppendLine(CultureInfo.InvariantCulture, $"            var entity = new {entityNameForInstantiation}");
        sb.AppendLine("            {");

        var createColumns = GetCreateColumnsInternal(table);
        var assignments = new List<string>();
        foreach (var (column, propName) in from column in createColumns
                                           let propName = CodeGenerationHelpers.SanitizeColumnName(column.Name)
                                           select (column, propName))
        {
            if (column.Name.StartsWith("eno_", StringComparison.OrdinalIgnoreCase))
            {
                // Hashed columns - handled separately
                continue;
            }

            assignments.Add(string.Format(CultureInfo.InvariantCulture, "                {0} = request.{0}", propName));
        }

        sb.AppendLine(string.Join("," + Environment.NewLine, assignments));
        sb.AppendLine("            };");
        sb.AppendLine();

        // Handle hashed columns (eno_)
        var hashedColumns = createColumns.Where(c =>
            c.Name.StartsWith("eno_", StringComparison.OrdinalIgnoreCase));

        foreach (var col in hashedColumns)
        {
            var propName = CodeGenerationHelpers.SanitizeColumnName(col.Name);
            sb.AppendLine(CultureInfo.InvariantCulture, $"            if (!string.IsNullOrEmpty(request.{propName}))");
            sb.AppendLine("            {");
            sb.AppendLine(CultureInfo.InvariantCulture, $"                entity.Set{propName}(request.{propName});");
            sb.AppendLine("            }");
            sb.AppendLine();
        }

        sb.AppendLine(CultureInfo.InvariantCulture, $"            await {repoFieldName}.AddAsync(entity, cancellationToken);");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogInformation(\"{table.Name} created successfully with ID: {{Id}}\", entity.{pkPropName});");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            return Result<{pkType}>.Success(entity.{pkPropName});");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogError(ex, \"Error creating {table.Name}\");");
        sb.AppendLine("            throw;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateUpdateHandler(Table table, string commandClassName, string handlerClassName, string rootNamespace)
    {
        var sb = new StringBuilder();
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);
        var pluralName = CodeGenerationHelpers.MakePlural(entityName);
        var repoInterfaceName = $"I{entityName}Repository";
        const string repoFieldName = "_repository";
        var pkColumn = table.Columns.Find(c => c.IsPrimaryKey) ?? table.Columns.First(c => c.IsPrimaryKey);

        // Check for namespace collision
        string? entityAliasName = entityName == pluralName ? entityName : null;

        GenerateFileHeader(sb, table.Name, "Handler");
        GenerateHandlerUsings(sb, rootNamespace, entityAliasName);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Commands;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Handles the {commandClassName} request.");
        sb.AppendLine("/// </summary>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {handlerClassName} : IRequestHandler<{commandClassName}, Result>");
        sb.AppendLine("{");

        sb.AppendLine(CultureInfo.InvariantCulture, $"    private readonly {repoInterfaceName} {repoFieldName};");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    private readonly ILogger<{handlerClassName}> _logger;");
        sb.AppendLine();

        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Initializes a new instance of the <see cref=\"{handlerClassName}\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {handlerClassName}(");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        {repoInterfaceName} repository,");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        ILogger<{handlerClassName}> logger)");
        sb.AppendLine("    {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        {repoFieldName} = repository ?? throw new ArgumentNullException(nameof(repository));");
        sb.AppendLine("        _logger = logger ?? throw new ArgumentNullException(nameof(logger));");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    /// <inheritdoc/>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public async Task<Result> Handle({commandClassName} request, CancellationToken cancellationToken)");
        sb.AppendLine("    {");
        sb.AppendLine("        ArgumentNullException.ThrowIfNull(request);");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"        _logger.LogDebug(\"Updating {table.Name} with ID: {{Id}}\", request.{pkColumn.Name});");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            var entity = await {repoFieldName}.GetByIdAsync(request.{pkColumn.Name}, cancellationToken);");
        sb.AppendLine();
        sb.AppendLine("            if (entity is null)");
        sb.AppendLine("            {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"                _logger.LogWarning(\"{table.Name} not found with ID: {{Id}}\", request.{pkColumn.Name});");
        sb.AppendLine(CultureInfo.InvariantCulture, $"                return Result.Failure(\"{table.Name} not found.\");");
        sb.AppendLine("            }");
        sb.AppendLine();

        // Update properties
        var updateColumns = GetUpdateColumnsInternal(table);

        foreach (var column in updateColumns)
        {
            var propName = CodeGenerationHelpers.SanitizeColumnName(column.Name);
            sb.AppendLine(CultureInfo.InvariantCulture, $"            entity.{propName} = request.{propName};");
        }

        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            await {repoFieldName}.UpdateAsync(entity, cancellationToken);");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogInformation(\"{table.Name} updated successfully. ID: {{Id}}\", request.{pkColumn.Name});");
        sb.AppendLine();
        sb.AppendLine("            return Result.Success();");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogError(ex, \"Error updating {table.Name} with ID: {{Id}}\", request.{pkColumn.Name});");
        sb.AppendLine("            throw;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateDeleteHandler(Table table, string commandClassName, string handlerClassName, string rootNamespace)
    {
        var sb = new StringBuilder();
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);
        var pluralName = CodeGenerationHelpers.MakePlural(entityName);
        var repoInterfaceName = $"I{entityName}Repository";
        const string repoFieldName = "_repository";
        var pkColumn = table.Columns.Find(c => c.IsPrimaryKey) ?? table.Columns.First(c => c.IsPrimaryKey);

        // Check for namespace collision
        string? entityAliasName = entityName == pluralName ? entityName : null;

        GenerateFileHeader(sb, table.Name, "Handler");
        GenerateHandlerUsings(sb, rootNamespace, entityAliasName);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Commands;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Handles the {commandClassName} request.");
        sb.AppendLine("/// </summary>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {handlerClassName} : IRequestHandler<{commandClassName}, Result>");
        sb.AppendLine("{");

        sb.AppendLine(CultureInfo.InvariantCulture, $"    private readonly {repoInterfaceName} {repoFieldName};");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    private readonly ILogger<{handlerClassName}> _logger;");
        sb.AppendLine();

        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Initializes a new instance of the <see cref=\"{handlerClassName}\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {handlerClassName}(");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        {repoInterfaceName} repository,");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        ILogger<{handlerClassName}> logger)");
        sb.AppendLine("    {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        {repoFieldName} = repository ?? throw new ArgumentNullException(nameof(repository));");
        sb.AppendLine("        _logger = logger ?? throw new ArgumentNullException(nameof(logger));");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    /// <inheritdoc/>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public async Task<Result> Handle({commandClassName} request, CancellationToken cancellationToken)");
        sb.AppendLine("    {");
        sb.AppendLine("        ArgumentNullException.ThrowIfNull(request);");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"        _logger.LogDebug(\"Deleting {table.Name} with ID: {{Id}}\", request.{pkColumn.Name});");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            var exists = await {repoFieldName}.ExistsAsync(request.{pkColumn.Name}, cancellationToken);");
        sb.AppendLine();
        sb.AppendLine("            if (!exists)");
        sb.AppendLine("            {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"                _logger.LogWarning(\"{table.Name} not found with ID: {{Id}}\", request.{pkColumn.Name});");
        sb.AppendLine(CultureInfo.InvariantCulture, $"                return Result.Failure(\"{table.Name} not found.\");");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            await {repoFieldName}.DeleteAsync(request.{pkColumn.Name}, cancellationToken);");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogInformation(\"{table.Name} deleted successfully. ID: {{Id}}\", request.{pkColumn.Name});");
        sb.AppendLine();
        sb.AppendLine("            return Result.Success();");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            _logger.LogError(ex, \"Error deleting {table.Name} with ID: {{Id}}\", request.{pkColumn.Name});");
        sb.AppendLine("            throw;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateCreateValidator(Table table, string commandClassName, string validatorClassName, string rootNamespace)
    {
        var sb = new StringBuilder();
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);
        var pluralName = CodeGenerationHelpers.MakePlural(entityName);

        GenerateFileHeader(sb, table.Name, "Validator");
        GenerateValidatorUsings(sb);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Commands;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Validator for {commandClassName}.");
        sb.AppendLine("/// </summary>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {validatorClassName} : AbstractValidator<{commandClassName}>");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Initializes a new instance of the <see cref=\"{validatorClassName}\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {validatorClassName}()");
        sb.AppendLine("    {");

        var createColumns = GetCreateColumnsInternal(table);

        foreach (var column in createColumns)
        {
            var rules = GenerateValidationRulesInternal(column);
            if (!string.IsNullOrEmpty(rules))
            {
                sb.Append(rules);
                sb.AppendLine();
            }
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateUpdateValidator(Table table, string commandClassName, string validatorClassName, string rootNamespace)
    {
        var sb = new StringBuilder();
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);
        var pluralName = CodeGenerationHelpers.MakePlural(entityName);
        var pkColumn = table.Columns.Find(c => c.IsPrimaryKey) ?? table.Columns.First(c => c.IsPrimaryKey);
        var pkType = CodeGenerationHelpers.GetCSharpType(pkColumn.DataType);

        GenerateFileHeader(sb, table.Name, "Validator");
        GenerateValidatorUsings(sb);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Commands;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Validator for {commandClassName}.");
        sb.AppendLine("/// </summary>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {validatorClassName} : AbstractValidator<{commandClassName}>");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Initializes a new instance of the <see cref=\"{validatorClassName}\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {validatorClassName}()");
        sb.AppendLine("    {");

        // Validate primary key
        if (pkType is "int" or "long" or "short")
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"        RuleFor(x => x.{pkColumn.Name})");
            sb.AppendLine("            .GreaterThan(0)");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            .WithMessage(\"{pkColumn.Name} must be greater than 0.\");");
            sb.AppendLine();
        }
        else if (pkType is "Guid" or "string")
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"        RuleFor(x => x.{pkColumn.Name})");
            sb.AppendLine("            .NotEmpty()");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            .WithMessage(\"{pkColumn.Name} is required.\");");
            sb.AppendLine();
        }

        var updateColumns = GetUpdateColumnsInternal(table);

        foreach (var column in updateColumns)
        {
            var rules = GenerateValidationRulesInternal(column);
            if (!string.IsNullOrEmpty(rules))
            {
                sb.Append(rules);
                sb.AppendLine();
            }
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateDeleteValidator(Table table, string commandClassName, string validatorClassName, string rootNamespace)
    {
        var sb = new StringBuilder();
        var entityName = API.BaseApiGenerator.GetClassName(table.Name);
        var pluralName = CodeGenerationHelpers.MakePlural(entityName);
        var pkColumn = table.Columns.Find(c => c.IsPrimaryKey) ?? table.Columns.First(c => c.IsPrimaryKey);
        var pkType = CodeGenerationHelpers.GetCSharpType(pkColumn.DataType);

        GenerateFileHeader(sb, table.Name, "Validator");
        GenerateValidatorUsings(sb);

        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Features.{pluralName}.Commands;");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Validator for {commandClassName}.");
        sb.AppendLine("/// </summary>");

        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {validatorClassName} : AbstractValidator<{commandClassName}>");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Initializes a new instance of the <see cref=\"{validatorClassName}\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {validatorClassName}()");
        sb.AppendLine("    {");

        if (pkType is "int" or "long" or "short")
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"        RuleFor(x => x.{pkColumn.Name})");
            sb.AppendLine("            .GreaterThan(0)");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            .WithMessage(\"{pkColumn.Name} must be greater than 0.\");");
        }
        else if (pkType is "Guid" or "string")
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"        RuleFor(x => x.{pkColumn.Name})");
            sb.AppendLine("            .NotEmpty()");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            .WithMessage(\"{pkColumn.Name} is required.\");");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static IEnumerable<Column> GetCreateColumnsInternal(Table table)
    {
        // Audit fields that are typically auto-generated or have private setters
        var auditFields = new[] { "AddedBy", "AddedOn", "ChangedBy", "ChangedOn" };

        return table.Columns.Where(c => !ShouldExcludeFromCreate(c, auditFields));
    }

    private static bool ShouldExcludeFromCreate(Column column, string[] auditFields)
    {
        // Exclude identity columns
        if (column.IsIdentity)
        {
            return true;
        }

        // Exclude audit fields
        if (auditFields.Contains(column.Name, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        // Exclude read-only columns (clc_, blg_, agg_, spt_, upl_, spl_)
        if (CodeGenerationHelpers.IsReadOnlyColumn(column.Name))
        {
            return true;
        }

        // Exclude columns with prefixes in their names (both with and without underscore)
        // Historic support: some columns use "enoPassword" instead of "eno_Password"
        // ENM = Enumeration, SCB = Separate Changed By, SPT = Separate Update, ENO = One-Way Encryption, ENT = Two-Way Encryption
        var upperName = column.Name.ToUpperInvariant();
        if (upperName.StartsWith("ENO", StringComparison.Ordinal) ||
            upperName.StartsWith("ENT", StringComparison.Ordinal) ||
            upperName.StartsWith("SPT", StringComparison.Ordinal) ||
            upperName.StartsWith("ENM", StringComparison.Ordinal) ||
            upperName.StartsWith("SCB", StringComparison.Ordinal))
        {
            return true;
        }

        // Check ExtendedProperties for ccType metadata
        if (HasExcludedCcType(column))
        {
            return true;
        }

        // Check Prefix enum for transforming/read-only columns
        if (HasExcludedPrefix(column))
        {
            return true;
        }

        return false;
    }

    private static bool HasExcludedCcType(Column column)
    {
        if (column.ExtendedProperties == null || !column.ExtendedProperties.TryGetValue("ccType", out var ccType))
        {
            return false;
        }

        var ccTypeUpper = ccType.ToUpperInvariant();

        return ccTypeUpper.Contains("SPT", StringComparison.Ordinal) ||
               ccTypeUpper.Contains("ENO", StringComparison.Ordinal) ||
               ccTypeUpper.Contains("ENT", StringComparison.Ordinal) ||
               ccTypeUpper.Contains("ENM", StringComparison.Ordinal) ||
               ccTypeUpper.Contains("SCB", StringComparison.Ordinal) ||
               ccTypeUpper.Contains("CLC", StringComparison.Ordinal) ||
               ccTypeUpper.Contains("BLG", StringComparison.Ordinal) ||
               ccTypeUpper.Contains("AGG", StringComparison.Ordinal);
    }

    private static bool HasExcludedPrefix(Column column)
    {
        return column.Prefix == ColumnPrefix.SeparateUpdate ||
               column.Prefix == ColumnPrefix.Calculated ||
               column.Prefix == ColumnPrefix.BusinessLogic ||
               column.Prefix == ColumnPrefix.Aggregate ||
               column.Prefix == ColumnPrefix.OneWayEncryption ||
               column.Prefix == ColumnPrefix.TwoWayEncryption;
    }

    private static IEnumerable<Column> GetUpdateColumnsInternal(Table table)
    {
        return table.Columns.Where(c =>
            !c.IsPrimaryKey &&
            !c.IsIdentity &&
            !c.Name.StartsWith("eno_", StringComparison.OrdinalIgnoreCase) &&
            !CodeGenerationHelpers.IsReadOnlyColumn(c.Name));
    }

    private static string GenerateValidationRulesInternal(Column column)
    {
        var sb = new StringBuilder();
        var propName = CodeGenerationHelpers.SanitizeColumnName(column.Name);
        var propType = CodeGenerationHelpers.GetCSharpType(column.DataType);

        var rules = new List<string>();

        // Not nullable validation
        if (!column.IsNullable && propType == "string")
        {
            rules.Add("            .NotEmpty()");
        }

        // String length validation
        if (propType == "string" && column.MaxLength.HasValue && column.MaxLength.Value > 0)
        {
            rules.Add(string.Format(
                CultureInfo.InvariantCulture,
                "            .MaximumLength({0})",
                column.MaxLength.Value));
        }

        // Email validation
        if (column.Name.Contains("email", StringComparison.OrdinalIgnoreCase))
        {
            rules.Add("            .EmailAddress()");
        }

        // Phone validation
        if (column.Name.Contains("phone", StringComparison.OrdinalIgnoreCase))
        {
            rules.Add("            .Matches(@\"^[+]?[0-9\\s\\-().]{7,20}$\")");
        }

        // Password validation (eno_ prefix)
        if (column.Name.StartsWith("eno_", StringComparison.OrdinalIgnoreCase) &&
            column.Name.Contains("password", StringComparison.OrdinalIgnoreCase))
        {
            rules.Add("            .MinimumLength(8)");
        }

        if (rules.Count > 0)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"        RuleFor(x => x.{propName})");
            sb.AppendLine(string.Join(Environment.NewLine, rules) + ";");
        }

        return sb.ToString();
    }

    private static string GetDefaultValue(string propType, bool isNullable)
    {
        if (propType == "string")
        {
            return isNullable ? string.Empty : " = string.Empty;";
        }

        return string.Empty;
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

    private static void GenerateCommandUsings(StringBuilder sb, string rootNamespace)
    {
        sb.AppendLine("using MediatR;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Application.Common.Models;");
        sb.AppendLine();
    }

    private static void GenerateHandlerUsings(StringBuilder sb, string rootNamespace)
    {
        sb.AppendLine("using MediatR;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Application.Common.Models;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Domain.Entities;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Domain.Interfaces;");
        sb.AppendLine();
    }

    private static void GenerateValidatorUsings(StringBuilder sb)
    {
        sb.AppendLine("using FluentValidation;");
        sb.AppendLine();
    }
}
