// <copyright file="LookupControllerGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.API
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Generates a generic Lookup API Controller for serving ccvwComboList data.
    /// This controller provides endpoints for autocomplete/dropdown functionality.
    /// </summary>
    public class LookupControllerGenerator
    {
        private static readonly Action<ILogger, Exception?> LogGeneratingController =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(1, nameof(LogGeneratingController)),
                "Generating Lookup API Controller");

        private static readonly Action<ILogger, int, Exception?> LogGeneratedWithTables =
            LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(2, nameof(LogGeneratedWithTables)),
                "Generated Lookup Controller with {TableCount} allowed lookup tables");

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LookupControllerGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public LookupControllerGenerator(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates the LookupItem model class.
        /// </summary>
        /// <param name="config">API generator configuration.</param>
        /// <returns>Generated C# code for the LookupItem model.</returns>
        public static Task<string> GenerateLookupItemModelAsync(ApiGeneratorConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);

            var rootNamespace = GetRootNamespace(config.Namespace);

            var sb = new StringBuilder();

            sb.AppendLine("// <copyright file=\"LookupItem.cs\" company=\"PlaceholderCompany\">");
            sb.AppendLine("// Copyright (c) PlaceholderCompany. All rights reserved.");
            sb.AppendLine("// </copyright>");
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.DTOs");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Represents a lookup item for dropdowns and autocomplete.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public class LookupItem");
            sb.AppendLine("    {");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets or sets the unique identifier.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public int Id { get; set; }");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets or sets the display text.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public string Text { get; set; } = string.Empty;");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets or sets the normalized text (without spaces/special chars).");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public string? TextNS { get; set; }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return Task.FromResult(sb.ToString());
        }

        /// <summary>
        /// Generates the Lookup Controller code.
        /// </summary>
        /// <param name="schema">Database schema for determining valid lookup tables.</param>
        /// <param name="config">API generator configuration.</param>
        /// <returns>Generated C# code for the controller.</returns>
        public Task<string> GenerateLookupControllerAsync(DatabaseSchema schema, ApiGeneratorConfig config)
        {
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(config);

            LogGeneratingController(_logger, null);

            var validTables = GetValidLookupTables(schema);
            LogGeneratedWithTables(_logger, validTables.Count, null);

            var sb = new StringBuilder();

            GenerateFileHeader(sb);
            GenerateUsings(sb, config);
            GenerateNamespace(sb, config, validTables);

            return Task.FromResult(sb.ToString());
        }

        /// <summary>
        /// Gets tables that are valid for lookup operations.
        /// </summary>
        private static List<string> GetValidLookupTables(DatabaseSchema schema)
        {
            return schema.Tables
                .Where(t => !t.IsView)
                .Where(t => !t.Name.StartsWith("c_", StringComparison.OrdinalIgnoreCase))
                .Where(t => !t.Name.StartsWith("ccvwComboList_", StringComparison.OrdinalIgnoreCase))
                .Select(t => t.Name)
                .OrderBy(n => n)
                .ToList();
        }

        /// <summary>
        /// Gets the root namespace from the full namespace.
        /// </summary>
        private static string GetRootNamespace(string fullNamespace)
        {
            var parts = fullNamespace.Split('.');
            return parts.Length > 1
                ? string.Join(".", parts.Take(parts.Length - 1))
                : parts[0];
        }

        /// <summary>
        /// Generates the file header comment.
        /// </summary>
        private static void GenerateFileHeader(StringBuilder sb)
        {
            sb.AppendLine("// <copyright file=\"LookupController.cs\" company=\"PlaceholderCompany\">");
            sb.AppendLine("// Copyright (c) PlaceholderCompany. All rights reserved.");
            sb.AppendLine("// </copyright>");
            sb.AppendLine();
            sb.AppendLine("// Auto-generated Lookup Controller for ccvwComboList views");
            sb.AppendLine(CultureInfo.InvariantCulture, $"// Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine();
        }

        /// <summary>
        /// Generates using statements.
        /// </summary>
        private static void GenerateUsings(StringBuilder sb, ApiGeneratorConfig config)
        {
            var rootNamespace = GetRootNamespace(config.Namespace);

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Dapper;");
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine("using Microsoft.Extensions.Logging;");
            sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Application.DTOs;");
            sb.AppendLine();
        }

        /// <summary>
        /// Generates the namespace and controller class.
        /// </summary>
        private static void GenerateNamespace(
            StringBuilder sb,
            ApiGeneratorConfig config,
            List<string> validTables)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {config.Namespace}.Controllers");
            sb.AppendLine("{");

            GenerateControllerClass(sb, config, validTables);

            sb.AppendLine("}");
        }

        /// <summary>
        /// Generates the controller class.
        /// </summary>
        private static void GenerateControllerClass(
            StringBuilder sb,
            ApiGeneratorConfig config,
            List<string> validTables)
        {
            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("    /// <summary>");
                sb.AppendLine("    /// Generic Lookup API Controller for dropdown and autocomplete data.");
                sb.AppendLine("    /// Serves data from ccvwComboList views.");
                sb.AppendLine("    /// </summary>");
            }

            sb.AppendLine("    [ApiController]");
            sb.AppendLine("    [Route(\"api/[controller]\")]");

            if (config.GenerateSwaggerAttributes)
            {
                sb.AppendLine("    [Produces(\"application/json\")]");
            }

            sb.AppendLine("    public class LookupController : ControllerBase");
            sb.AppendLine("    {");

            GenerateValidTablesHashSet(sb, validTables);
            sb.AppendLine();

            GenerateFields(sb);
            sb.AppendLine();

            GenerateConstructor(sb, config);
            sb.AppendLine();

            GenerateGetLookupMethod(sb, config);
            sb.AppendLine();

            GenerateGetByIdMethod(sb, config);
            sb.AppendLine();

            GenerateIsValidTableMethod(sb);

            sb.AppendLine("    }");
        }

        /// <summary>
        /// Generates the valid tables HashSet.
        /// </summary>
        private static void GenerateValidTablesHashSet(StringBuilder sb, List<string> validTables)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Valid table names for lookup operations.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        private static readonly HashSet<string> ValidLookupTables = new(StringComparer.OrdinalIgnoreCase)");
            sb.AppendLine("        {");

            foreach (var table in validTables)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"            \"{table}\",");
            }

            sb.AppendLine("        };");
        }

        /// <summary>
        /// Generates the private fields.
        /// </summary>
        private static void GenerateFields(StringBuilder sb)
        {
            sb.AppendLine("        private readonly IDbConnection _db;");
            sb.AppendLine("        private readonly ILogger<LookupController> _logger;");
        }

        /// <summary>
        /// Generates the constructor.
        /// </summary>
        private static void GenerateConstructor(StringBuilder sb, ApiGeneratorConfig config)
        {
            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine("        /// Initializes a new instance of the <see cref=\"LookupController\"/> class.");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine("        /// <param name=\"db\">Database connection.</param>");
                sb.AppendLine("        /// <param name=\"logger\">Logger instance.</param>");
            }

            sb.AppendLine("        public LookupController(IDbConnection db, ILogger<LookupController> logger)");
            sb.AppendLine("        {");
            sb.AppendLine("            _db = db ?? throw new ArgumentNullException(nameof(db));");
            sb.AppendLine("            _logger = logger ?? throw new ArgumentNullException(nameof(logger));");
            sb.AppendLine("        }");
        }

        /// <summary>
        /// Generates the GetLookup method.
        /// </summary>
        private static void GenerateGetLookupMethod(StringBuilder sb, ApiGeneratorConfig config)
        {
            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine("        /// Gets lookup items for a specific entity type.");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine("        /// <param name=\"entityType\">The entity type (table name).</param>");
                sb.AppendLine("        /// <param name=\"search\">Optional search text.</param>");
                sb.AppendLine("        /// <param name=\"page\">Page number (1-based).</param>");
                sb.AppendLine("        /// <param name=\"pageSize\">Number of items per page.</param>");
                sb.AppendLine("        /// <param name=\"parentId\">Optional parent ID for filtered lookups.</param>");
                sb.AppendLine("        /// <returns>List of lookup items.</returns>");
            }

            sb.AppendLine("        [HttpGet(\"{entityType}\")]");

            if (config.GenerateSwaggerAttributes)
            {
                sb.AppendLine("        [ProducesResponseType(typeof(IEnumerable<LookupItem>), 200)]");
                sb.AppendLine("        [ProducesResponseType(400)]");
            }

            sb.AppendLine("        public async Task<IActionResult> GetLookup(");
            sb.AppendLine("            string entityType,");
            sb.AppendLine("            [FromQuery] string? search = null,");
            sb.AppendLine("            [FromQuery] int page = 1,");
            sb.AppendLine("            [FromQuery] int pageSize = 20,");
            sb.AppendLine("            [FromQuery] int? parentId = null)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (!IsValidLookupTable(entityType))");
            sb.AppendLine("            {");
            sb.AppendLine("                return BadRequest($\"Invalid lookup entity type: {entityType}\");");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            // Ensure page size is reasonable");
            sb.AppendLine("            pageSize = Math.Min(Math.Max(pageSize, 1), 100);");
            sb.AppendLine("            page = Math.Max(page, 1);");
            sb.AppendLine();
            sb.AppendLine("            var viewName = $\"ccvwComboList_{entityType}\";");
            sb.AppendLine("            var offset = (page - 1) * pageSize;");
            sb.AppendLine();
            sb.AppendLine("            // Note: Using parameterized query with validated table name");
            sb.AppendLine("            var sql = $@\"");
            sb.AppendLine("                SELECT ID AS Id, Text, TextNS");
            sb.AppendLine("                FROM [{viewName}]");
            sb.AppendLine("                WHERE (@search IS NULL OR Text LIKE '%' + @search + '%')");
            sb.AppendLine("                ORDER BY Text");
            sb.AppendLine("                OFFSET @offset ROWS");
            sb.AppendLine("                FETCH NEXT @pageSize ROWS ONLY\";");
            sb.AppendLine();
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                var results = await _db.QueryAsync<LookupItem>(");
            sb.AppendLine("                    sql,");
            sb.AppendLine("                    new { search, offset, pageSize }).ConfigureAwait(false);");
            sb.AppendLine();
            sb.AppendLine("                return Ok(results);");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine("                _logger.LogError(ex, \"Error fetching lookup for {EntityType}\", entityType);");
            sb.AppendLine("                return StatusCode(500, \"An error occurred while fetching lookup data\");");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
        }

        /// <summary>
        /// Generates the GetById method.
        /// </summary>
        private static void GenerateGetByIdMethod(StringBuilder sb, ApiGeneratorConfig config)
        {
            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine("        /// Gets a single lookup item by ID.");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine("        /// <param name=\"entityType\">The entity type (table name).</param>");
                sb.AppendLine("        /// <param name=\"id\">The item ID.</param>");
                sb.AppendLine("        /// <returns>The lookup item.</returns>");
            }

            sb.AppendLine("        [HttpGet(\"{entityType}/{id:int}\")]");

            if (config.GenerateSwaggerAttributes)
            {
                sb.AppendLine("        [ProducesResponseType(typeof(LookupItem), 200)]");
                sb.AppendLine("        [ProducesResponseType(400)]");
                sb.AppendLine("        [ProducesResponseType(404)]");
            }

            sb.AppendLine("        public async Task<IActionResult> GetById(string entityType, int id)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (!IsValidLookupTable(entityType))");
            sb.AppendLine("            {");
            sb.AppendLine("                return BadRequest($\"Invalid lookup entity type: {entityType}\");");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            var viewName = $\"ccvwComboList_{entityType}\";");
            sb.AppendLine();
            sb.AppendLine("            var sql = $@\"");
            sb.AppendLine("                SELECT ID AS Id, Text, TextNS");
            sb.AppendLine("                FROM [{viewName}]");
            sb.AppendLine("                WHERE ID = @id\";");
            sb.AppendLine();
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                var result = await _db.QuerySingleOrDefaultAsync<LookupItem>(");
            sb.AppendLine("                    sql,");
            sb.AppendLine("                    new { id }).ConfigureAwait(false);");
            sb.AppendLine();
            sb.AppendLine("                if (result == null)");
            sb.AppendLine("                {");
            sb.AppendLine("                    return NotFound();");
            sb.AppendLine("                }");
            sb.AppendLine();
            sb.AppendLine("                return Ok(result);");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine("                _logger.LogError(ex, \"Error fetching lookup by ID for {EntityType}\", entityType);");
            sb.AppendLine("                return StatusCode(500, \"An error occurred while fetching lookup data\");");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
        }

        /// <summary>
        /// Generates the IsValidLookupTable method.
        /// </summary>
        private static void GenerateIsValidTableMethod(StringBuilder sb)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Validates that the table name is allowed for lookup operations.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        private static bool IsValidLookupTable(string tableName)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (string.IsNullOrWhiteSpace(tableName))");
            sb.AppendLine("            {");
            sb.AppendLine("                return false;");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            // Prevent SQL injection by checking against whitelist");
            sb.AppendLine("            return ValidLookupTables.Contains(tableName);");
            sb.AppendLine("        }");
        }
    }
}
