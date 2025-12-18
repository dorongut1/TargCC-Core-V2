// <copyright file="ApiControllerGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.API
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Generators.Common;
    using TargCC.Core.Generators.Entities;

    // Note: Domain.Interfaces using added by generator based on config.Namespace
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Generates ASP.NET Core API Controllers with CRUD endpoints.
    /// </summary>
    public class ApiControllerGenerator : BaseApiGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiControllerGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public ApiControllerGenerator(ILogger<ApiControllerGenerator> logger)
            : base(logger)
        {
        }

        /// <inheritdoc/>
        protected override string GeneratorTypeName => "API Controller";

        /// <inheritdoc/>
        protected override string Generate(Table table, DatabaseSchema schema, ApiGeneratorConfig config)
        {
            var className = GetClassName(table.Name);
            var entityName = className;
            var controllerName = CodeGenerationHelpers.MakePlural(className);

            // Extract root namespace from config.Namespace (e.g., "TestApp.API" -> "TestApp", or "UpayCard.RiskManagement.API" -> "UpayCard.RiskManagement")
            // Remove the last segment (typically "API" or "Controllers") to get the root namespace
            var namespaceParts = config.Namespace.Split('.');
            var rootNamespace = namespaceParts.Length > 1
                ? string.Join(".", namespaceParts.Take(namespaceParts.Length - 1))
                : namespaceParts[0];

            var sb = new StringBuilder();

            sb.Append(GenerateFileHeader(table.Name, "API Controller Generator"));

            AppendUsings(sb, rootNamespace, controllerName);

            sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {config.Namespace}.Controllers");
            sb.AppendLine("{");

            GenerateControllerClass(sb, entityName, controllerName, table, schema, config, rootNamespace);

            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void AppendUsings(StringBuilder sb, string rootNamespace, string controllerName)
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using MediatR;");
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine("using Microsoft.Extensions.Logging;");
            sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Application.Common.Models;");
            sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Application.DTOs;");
            sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Application.Features.{controllerName}.Queries;");
            sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Domain.Common;");
            sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Domain.Entities;");
            sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Domain.Interfaces;");
            sb.AppendLine();
        }

        private static void GenerateControllerClass(StringBuilder sb, string entityName, string controllerName, Table table, DatabaseSchema? schema, ApiGeneratorConfig config, string rootNamespace)
        {
            string qualifiedEntityName = GetQualifiedEntityName(entityName, rootNamespace);

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("    /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"    /// API controller for {entityName} entities.");
                sb.AppendLine("    /// </summary>");
            }

            sb.AppendLine("    [ApiController]");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    [Route(\"api/[controller]\")]");

            if (config.GenerateSwaggerAttributes)
            {
                sb.AppendLine("    [Produces(\"application/json\")]");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"    public class {controllerName}Controller : ControllerBase");
            sb.AppendLine("    {");

            GenerateFields(sb, entityName, controllerName);
            sb.AppendLine();

            GenerateConstructor(sb, entityName, controllerName, config);
            sb.AppendLine();

            GenerateGetByIdMethod(sb, table, qualifiedEntityName, config);
            sb.AppendLine();

            GenerateGetAllMethod(sb, qualifiedEntityName, config);
            sb.AppendLine();

            GenerateGetFilteredMethod(sb, qualifiedEntityName, table, config);
            sb.AppendLine();

            // Generate related data endpoints (Master-Detail Views)
            if (schema != null)
            {
                GenerateRelatedDataEndpoints(sb, table, schema, config, rootNamespace);
            }

            // Only generate Create/Update/Delete for tables, not for views (views are read-only)
            if (!table.IsView)
            {
                GenerateCreateMethod(sb, table, qualifiedEntityName, config);
                sb.AppendLine();

                GenerateUpdateMethod(sb, table, qualifiedEntityName, config);
                sb.AppendLine();

                GenerateDeleteMethod(sb, table, entityName, config);
            }

            sb.AppendLine("    }");
        }

        private static void GenerateFields(StringBuilder sb, string entityName, string controllerName)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"        private readonly I{entityName}Repository _repository;");
            sb.AppendLine("        private readonly IMediator _mediator;");
            sb.AppendLine(CultureInfo.InvariantCulture, $"        private readonly ILogger<{controllerName}Controller> _logger;");
        }

        private static void GenerateConstructor(StringBuilder sb, string entityName, string controllerName, ApiGeneratorConfig config)
        {
            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Initializes a new instance of the <see cref=\"{controllerName}Controller\"/> class.");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// <param name=\"repository\">Repository for {entityName}.</param>");
                sb.AppendLine("        /// <param name=\"mediator\">MediatR instance for sending queries.</param>");
                sb.AppendLine("        /// <param name=\"logger\">Logger instance.</param>");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public {controllerName}Controller(");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            I{entityName}Repository repository,");
            sb.AppendLine("            IMediator mediator,");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            ILogger<{controllerName}Controller> logger)");
            sb.AppendLine("        {");
            sb.AppendLine("            _repository = repository ?? throw new ArgumentNullException(nameof(repository));");
            sb.AppendLine("            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));");
            sb.AppendLine("            _logger = logger ?? throw new ArgumentNullException(nameof(logger));");
            sb.AppendLine("        }");
        }

        private static void GenerateGetByIdMethod(StringBuilder sb, Table table, string qualifiedEntityName, ApiGeneratorConfig config)
        {
            // Get the primary key type
            var pkColumn = table.Columns.Find(c => c.IsPrimaryKey);
            string pkType = pkColumn != null ? Common.CodeGenerationHelpers.GetCSharpType(pkColumn.DataType) : "int";

            // Extract simple name for documentation
            string entityNameForDocs = qualifiedEntityName.Contains('.', StringComparison.Ordinal)
                ? qualifiedEntityName[(qualifiedEntityName.LastIndexOf('.') + 1) ..]
                : qualifiedEntityName;

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets a {entityNameForDocs} by ID.");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine("        /// <param name=\"id\">The entity ID.</param>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// <returns>The {entityNameForDocs} entity.</returns>");
            }

            sb.AppendLine("        [HttpGet(\"{id}\")]");

            if (config.GenerateSwaggerAttributes)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        [ProducesResponseType(typeof({qualifiedEntityName}), 200)]");
                sb.AppendLine("        [ProducesResponseType(404)]");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public async System.Threading.Tasks.Task<ActionResult<{qualifiedEntityName}>> GetById({pkType} id)");
            sb.AppendLine("        {");
            sb.AppendLine("            var entity = await _repository.GetByIdAsync(id).ConfigureAwait(false);");
            sb.AppendLine("            if (entity == null)");
            sb.AppendLine("            {");
            sb.AppendLine("                return NotFound();");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            return Ok(entity);");
            sb.AppendLine("        }");
        }

        private static void GenerateGetAllMethod(StringBuilder sb, string qualifiedEntityName, ApiGeneratorConfig config)
        {
            // Extract simple name for documentation
            string entityNameForDocs = qualifiedEntityName.Contains('.', StringComparison.Ordinal)
                ? qualifiedEntityName[(qualifiedEntityName.LastIndexOf('.') + 1) ..]
                : qualifiedEntityName;

            var pluralName = CodeGenerationHelpers.MakePlural(entityNameForDocs);

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets all {pluralName} with server-side filtering, sorting, and pagination.");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine("        /// <param name=\"filters\">Optional filters to apply.</param>");
                sb.AppendLine("        /// <param name=\"page\">Page number (1-based). Default is 1.</param>");
                sb.AppendLine("        /// <param name=\"pageSize\">Number of items per page. Default is 100.</param>");
                sb.AppendLine("        /// <param name=\"sortBy\">Field to sort by. Default is \"Id\".</param>");
                sb.AppendLine("        /// <param name=\"sortDirection\">Sort direction (asc/desc). Default is \"asc\".</param>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// <returns>Paginated result of {entityNameForDocs} DTOs.</returns>");
            }

            sb.AppendLine("        [HttpGet]");

            if (config.GenerateSwaggerAttributes)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        [ProducesResponseType(typeof(PagedResult<{entityNameForDocs}Dto>), 200)]");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public async System.Threading.Tasks.Task<ActionResult<PagedResult<{entityNameForDocs}Dto>>> GetAll(");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            [FromQuery] {entityNameForDocs}Filters? filters,");
            sb.AppendLine("            [FromQuery] int page = 1,");
            sb.AppendLine("            [FromQuery] int pageSize = 100,");
            sb.AppendLine("            [FromQuery] string? sortBy = \"Id\",");
            sb.AppendLine("            [FromQuery] string sortDirection = \"asc\")");
            sb.AppendLine("        {");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            var query = new Get{pluralName}Query");
            sb.AppendLine("            {");
            sb.AppendLine("                Filters = filters,");
            sb.AppendLine("                Page = page,");
            sb.AppendLine("                PageSize = pageSize,");
            sb.AppendLine("                SortBy = sortBy,");
            sb.AppendLine("                SortDirection = sortDirection");
            sb.AppendLine("            };");
            sb.AppendLine();
            sb.AppendLine("            var result = await _mediator.Send(query).ConfigureAwait(false);");
            sb.AppendLine();
            sb.AppendLine("            return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);");
            sb.AppendLine("        }");
        }

        private static void GenerateCreateMethod(StringBuilder sb, Table table, string qualifiedEntityName, ApiGeneratorConfig config)
        {
            // Extract simple name for documentation
            string entityNameForDocs = qualifiedEntityName.Contains('.', StringComparison.Ordinal)
                ? qualifiedEntityName[(qualifiedEntityName.LastIndexOf('.') + 1) ..]
                : qualifiedEntityName;

            // Get the primary key property name
            var pkColumn = table.Columns.Find(c => c.IsPrimaryKey);
            string pkPropertyName = pkColumn != null ? PrefixHandler.GetPropertyName(pkColumn) : "ID";

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Creates a new {entityNameForDocs}.");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine("        /// <param name=\"entity\">The {entityNameForDocs} to create.</param>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// <returns>The created {entityNameForDocs}.</returns>");
            }

            sb.AppendLine("        [HttpPost]");

            if (config.GenerateSwaggerAttributes)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        [ProducesResponseType(typeof({qualifiedEntityName}), 201)]");
                sb.AppendLine("        [ProducesResponseType(400)]");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public async System.Threading.Tasks.Task<ActionResult<{qualifiedEntityName}>> Create([FromBody] {qualifiedEntityName} entity)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (!ModelState.IsValid)");
            sb.AppendLine("            {");
            sb.AppendLine("                return BadRequest(ModelState);");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            await _repository.AddAsync(entity).ConfigureAwait(false);");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            return CreatedAtAction(nameof(GetById), new {{ id = entity.{pkPropertyName} }}, entity);");
            sb.AppendLine("        }");
        }

        private static void GenerateUpdateMethod(StringBuilder sb, Table table, string qualifiedEntityName, ApiGeneratorConfig config)
        {
            // Extract simple name for documentation
            string entityNameForDocs = qualifiedEntityName.Contains('.', StringComparison.Ordinal)
                ? qualifiedEntityName[(qualifiedEntityName.LastIndexOf('.') + 1) ..]
                : qualifiedEntityName;

            // Get the primary key property name and type
            var pkColumn = table.Columns.Find(c => c.IsPrimaryKey);
            string pkPropertyName = pkColumn != null ? PrefixHandler.GetPropertyName(pkColumn) : "ID";
            string pkType = pkColumn != null ? Common.CodeGenerationHelpers.GetCSharpType(pkColumn.DataType) : "int";

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Updates an existing {entityNameForDocs}.");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine("        /// <param name=\"id\">The entity ID.</param>");
                sb.AppendLine("        /// <param name=\"entity\">The updated {entityNameForDocs}.</param>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// <returns>The updated {entityNameForDocs}.</returns>");
            }

            sb.AppendLine("        [HttpPut(\"{id}\")]");

            if (config.GenerateSwaggerAttributes)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        [ProducesResponseType(typeof({qualifiedEntityName}), 200)]");
                sb.AppendLine("        [ProducesResponseType(404)]");
                sb.AppendLine("        [ProducesResponseType(400)]");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public async System.Threading.Tasks.Task<ActionResult<{qualifiedEntityName}>> Update({pkType} id, [FromBody] {qualifiedEntityName} entity)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (!ModelState.IsValid)");
            sb.AppendLine("            {");
            sb.AppendLine("                return BadRequest(ModelState);");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            var existing = await _repository.GetByIdAsync(id).ConfigureAwait(false);");
            sb.AppendLine("            if (existing == null)");
            sb.AppendLine("            {");
            sb.AppendLine("                return NotFound();");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"            entity.{pkPropertyName} = id; // Ensure ID matches route");
            sb.AppendLine("            await _repository.UpdateAsync(entity).ConfigureAwait(false);");
            sb.AppendLine("            return Ok(entity);");
            sb.AppendLine("        }");
        }

        private static void GenerateDeleteMethod(StringBuilder sb, Table table, string entityName, ApiGeneratorConfig config)
        {
            // Get the primary key type
            var pkColumn = table.Columns.Find(c => c.IsPrimaryKey);
            string pkType = pkColumn != null ? Common.CodeGenerationHelpers.GetCSharpType(pkColumn.DataType) : "int";

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Deletes a {entityName}.");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine("        /// <param name=\"id\">The entity ID.</param>");
                sb.AppendLine("        /// <returns>No content.</returns>");
            }

            sb.AppendLine("        [HttpDelete(\"{id}\")]");

            if (config.GenerateSwaggerAttributes)
            {
                sb.AppendLine("        [ProducesResponseType(204)]");
                sb.AppendLine("        [ProducesResponseType(404)]");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public async Task<IActionResult> Delete({pkType} id)");
            sb.AppendLine("        {");
            sb.AppendLine("            var existing = await _repository.GetByIdAsync(id).ConfigureAwait(false);");
            sb.AppendLine("            if (existing == null)");
            sb.AppendLine("            {");
            sb.AppendLine("                return NotFound();");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            await _repository.DeleteAsync(id).ConfigureAwait(false);");
            sb.AppendLine("            return NoContent();");
            sb.AppendLine("        }");
        }

        private static void GenerateGetFilteredMethod(StringBuilder sb, string qualifiedEntityName, Table table, ApiGeneratorConfig config)
        {
            // Extract simple name for documentation
            string entityNameForDocs = qualifiedEntityName.Contains('.', StringComparison.Ordinal)
                ? qualifiedEntityName[(qualifiedEntityName.LastIndexOf('.') + 1) ..]
                : qualifiedEntityName;

            var filterableIndexes = table.Indexes?
                .Where(i => !i.IsPrimaryKey && i.ColumnNames != null && i.ColumnNames.Count > 0)
                .ToList();

            if (filterableIndexes == null || filterableIndexes.Count == 0)
            {
                // No filterable indexes, skip
                return;
            }

            var parameters = CollectFilterParameters(table, filterableIndexes);
            if (parameters.Count == 0)
            {
                return;
            }

            GenerateGetFilteredDocumentation(sb, entityNameForDocs, parameters, config);
            GenerateGetFilteredAttributes(sb, qualifiedEntityName, config);
            GenerateGetFilteredMethodSignature(sb, qualifiedEntityName, parameters);
        }

        /// <summary>
        /// Collects filter parameters from table indexes.
        /// </summary>
        private static List<(string paramName, string paramType, string columnName)> CollectFilterParameters(
            Table table,
            List<TargCC.Core.Interfaces.Models.Index> filterableIndexes)
        {
            var parameters = new List<(string paramName, string paramType, string columnName)>();
            var processedColumns = new HashSet<string>();

            foreach (var index in filterableIndexes)
            {
                foreach (var columnName in index.ColumnNames)
                {
                    if (processedColumns.Contains(columnName))
                    {
                        continue;
                    }

                    processedColumns.Add(columnName);
                    var column = table.Columns.Find(c => c.Name == columnName);
                    if (column != null)
                    {
                        // Use same parameter naming as Repository to ensure consistency
                        string paramName = Common.CodeGenerationHelpers.ToCamelCase(
                            Common.CodeGenerationHelpers.SanitizeColumnName(columnName));
                        string paramType = GetCSharpTypeName(column.DataType);
                        parameters.Add((paramName, paramType, columnName));
                    }
                }
            }

            return parameters;
        }

        /// <summary>
        /// Generates XML documentation for GetFiltered method.
        /// </summary>
        private static void GenerateGetFilteredDocumentation(
            StringBuilder sb,
            string entityNameForDocs,
            List<(string paramName, string paramType, string columnName)> parameters,
            ApiGeneratorConfig config)
        {
            if (!config.GenerateXmlDocumentation)
            {
                return;
            }

            sb.AppendLine("        /// <summary>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets filtered {CodeGenerationHelpers.MakePlural(entityNameForDocs)} based on indexed columns.");
            sb.AppendLine("        /// </summary>");
            foreach (var (paramName, _, columnName) in parameters)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// <param name=\"{paramName}\">Filter by {columnName}.</param>");
            }

            if (parameters.Count > 0)
            {
                sb.AppendLine("        /// <param name=\"skip\">Number of records to skip for pagination.</param>");
                sb.AppendLine("        /// <param name=\"take\">Number of records to take for pagination.</param>");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        /// <returns>Collection of filtered {entityNameForDocs} entities.</returns>");
        }

        /// <summary>
        /// Generates attributes for GetFiltered method.
        /// </summary>
        private static void GenerateGetFilteredAttributes(
            StringBuilder sb,
            string qualifiedEntityName,
            ApiGeneratorConfig config)
        {
            sb.AppendLine("        [HttpGet(\"filter\")]");

            if (config.GenerateSwaggerAttributes)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        [ProducesResponseType(typeof(IEnumerable<{qualifiedEntityName}>), 200)]");
            }
        }

        /// <summary>
        /// Generates method signature and body for GetFiltered method.
        /// </summary>
        private static void GenerateGetFilteredMethodSignature(
            StringBuilder sb,
            string qualifiedEntityName,
            List<(string paramName, string paramType, string columnName)> parameters)
        {
            var queryParams = string.Join(", ", parameters.Select(p => $"[FromQuery] {p.paramType}? {p.paramName} = null"));

            // Add skip and take parameters if there are filter parameters
            if (parameters.Count > 0)
            {
                queryParams += ", [FromQuery] int? skip = null, [FromQuery] int? take = null";
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public async System.Threading.Tasks.Task<ActionResult<IEnumerable<{qualifiedEntityName}>>> GetFiltered({queryParams})");
            sb.AppendLine("        {");

            var repoParams = string.Join(", ", parameters.Select(p => p.paramName));

            // Add skip, take, and cancellationToken to repository call
            if (parameters.Count > 0)
            {
                repoParams += ", skip, take, CancellationToken.None";
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"            var entities = await _repository.GetFilteredAsync({repoParams}).ConfigureAwait(false);");
            sb.AppendLine("            return Ok(entities);");
            sb.AppendLine("        }");
        }

        private static string GetCSharpTypeName(string sqlType)
        {
            var upper = sqlType.ToUpperInvariant();
            return upper switch
            {
                _ when upper.Contains("INT", StringComparison.Ordinal) => "int",
                _ when upper.Contains("VARCHAR", StringComparison.Ordinal) || upper.Contains("CHAR", StringComparison.Ordinal) || upper.Contains("TEXT", StringComparison.Ordinal) => "string",
                _ when upper.Contains("DATE", StringComparison.Ordinal) || upper.Contains("TIME", StringComparison.Ordinal) => "DateTime",
                _ when upper.Contains("BIT", StringComparison.Ordinal) => "bool",
                _ when upper.Contains("DECIMAL", StringComparison.Ordinal) || upper.Contains("NUMERIC", StringComparison.Ordinal) || upper.Contains("MONEY", StringComparison.Ordinal) => "decimal",
                _ when !(!upper.Contains("FLOAT", StringComparison.Ordinal) && !upper.Contains("REAL", StringComparison.Ordinal)) => "double",
                _ => "string",
            };
        }

        /// <summary>
        /// Gets a qualified entity name that avoids naming conflicts with system types.
        /// </summary>
        /// <param name="entityName">The entity class name.</param>
        /// <param name="rootNamespace">The root namespace of the project.</param>
        /// <returns>Fully qualified entity name if there's a conflict, otherwise just the entity name.</returns>
        private static string GetQualifiedEntityName(string entityName, string rootNamespace)
        {
            // Check if entity name conflicts with common .NET types
            var conflictingTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Task",      // System.Threading.Tasks.Task
                "Action",    // System.Action
                "Func",      // System.Func
                "Exception", // System.Exception
                "Attribute", // System.Attribute
                "Object",    // System.Object
                "String",    // System.String
                "Thread",    // System.Threading.Thread
                "Timer",     // System.Threading.Timer
                "File",      // System.IO.File
                "Directory", // System.IO.Directory
                "Stream",    // System.IO.Stream
            };

            if (conflictingTypes.Contains(entityName))
            {
                return $"{rootNamespace}.Domain.Entities.{entityName}";
            }

            return entityName;
        }

        /// <summary>
        /// Generates related data endpoints for Master-Detail views.
        /// </summary>
        private static void GenerateRelatedDataEndpoints(StringBuilder sb, Table table, DatabaseSchema schema, ApiGeneratorConfig config, string rootNamespace)
        {
            if (schema.Relationships == null || schema.Relationships.Count == 0)
            {
                return;
            }

            var entityName = GetClassName(table.Name);

            // Find all relationships where this table is the parent
            var parentRelationships = schema.Relationships
                .Where(r => r.ParentTable == table.FullName && r.IsEnabled)
                .ToList();

            if (parentRelationships.Count == 0)
            {
                return;
            }

            // Track generated methods to avoid duplicates when multiple FKs point to same child table
            var generatedMethods = new HashSet<string>();

            foreach (var relationship in parentRelationships)
            {
                var childTable = schema.Tables.Find(t => t.FullName == relationship.ChildTable);
                if (childTable == null)
                {
                    continue;
                }

                // Generate method name to check for duplicates
                // IMPORTANT: Use childTable.Name directly (not GetClassName) to match repository generator
                string childrenName = CodeGenerationHelpers.MakePlural(childTable.Name);
                string methodName = $"Get{childrenName}";

                // Skip if we've already generated this method (happens with multiple FKs to same table)
                if (generatedMethods.Contains(methodName))
                {
                    continue;
                }

                generatedMethods.Add(methodName);

                try
                {
                    GenerateSingleRelatedDataEndpoint(sb, childTable, entityName, config, rootNamespace);
                    sb.AppendLine();
                }
                catch
                {
                    // Skip relationships that cannot be generated
                }
            }
        }

        /// <summary>
        /// Generates a single related data endpoint.
        /// </summary>
        private static void GenerateSingleRelatedDataEndpoint(
            StringBuilder sb,
            Table childTable,
            string parentEntityName,
            ApiGeneratorConfig config,
            string rootNamespace)
        {
            string childEntityName = GetClassName(childTable.Name);
            string qualifiedChildEntityName = GetQualifiedEntityName(childEntityName, rootNamespace);

            // IMPORTANT: Use childTable.Name directly (not GetClassName) to match repository generator
            string childrenName = CodeGenerationHelpers.MakePlural(childTable.Name);
            string childrenLowerCase = childrenName.ToUpper(CultureInfo.InvariantCulture);

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Gets all {childrenLowerCase} for a specific {parentEntityName.ToUpper(CultureInfo.InvariantCulture)}.");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine("        /// <param name=\"id\">The parent entity ID.</param>");
                sb.AppendLine("        /// <param name=\"skip\">Number of records to skip for pagination.</param>");
                sb.AppendLine("        /// <param name=\"take\">Number of records to take for pagination.</param>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// <returns>Collection of {childEntityName} entities.</returns>");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        [HttpGet(\"{{id}}/{childrenLowerCase}\")]");

            if (config.GenerateSwaggerAttributes)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        [ProducesResponseType(typeof(IEnumerable<{qualifiedChildEntityName}>), 200)]");
                sb.AppendLine("        [ProducesResponseType(404)]");
            }

            sb.AppendLine(
                CultureInfo.InvariantCulture,
                $"        public async System.Threading.Tasks.Task<ActionResult<IEnumerable<{qualifiedChildEntityName}>>> Get{childrenName}(int id, [FromQuery] int? skip = null, [FromQuery] int? take = null)");
            sb.AppendLine("        {");
            sb.AppendLine("            // Verify parent exists");
            sb.AppendLine("            var parent = await _repository.GetByIdAsync(id).ConfigureAwait(false);");
            sb.AppendLine("            if (parent == null)");
            sb.AppendLine("            {");
            sb.AppendLine("                return NotFound();");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"            var {childrenLowerCase} = await _repository.Get{childrenName}Async(id, skip, take).ConfigureAwait(false);");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            return Ok({childrenLowerCase});");
            sb.AppendLine("        }");
        }
    }
}
