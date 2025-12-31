using System.Globalization;
using System.Text;
using TargCC.Core.Generators.Common;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Generators.API;

/// <summary>
/// Generates read-only API controllers for database views.
/// </summary>
public static class ViewControllerGenerator
{
    /// <summary>
    /// Generates a read-only controller for a view.
    /// </summary>
    /// <param name="view">The view information.</param>
    /// <param name="rootNamespace">The root namespace.</param>
    /// <returns>The generated controller code.</returns>
    public static string Generate(ViewInfo view, string rootNamespace)
    {
        var className = BaseApiGenerator.GetClassName(view.ViewName);
        var pluralName = CodeGenerationHelpers.MakePlural(className);
        var sb = new StringBuilder();

        // Using statements
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Application.Interfaces.Repositories;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Domain.Entities;");
        sb.AppendLine();

        // Namespace
        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.API.Controllers;");
        sb.AppendLine();

        // Controller documentation
        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// API controller for the {view.ViewName} view.");
        sb.AppendLine("/// This controller provides read-only access to view data.");
        sb.AppendLine("/// </summary>");

        // Controller attributes and declaration
        sb.AppendLine("[ApiController]");
        sb.AppendLine(CultureInfo.InvariantCulture, $"[Route(\"api/{pluralName}\")]");
        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {pluralName}Controller : ControllerBase");
        sb.AppendLine("{");

        // Private fields
        sb.AppendLine(CultureInfo.InvariantCulture, $"    private readonly I{className}Repository _repository;");
        sb.AppendLine();

        // Constructor
        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Initializes a new instance of the <see cref=\"{pluralName}Controller\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"repository\">The repository.</param>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {pluralName}Controller(I{className}Repository repository)");
        sb.AppendLine("    {");
        sb.AppendLine("        _repository = repository;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // GET All endpoint
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets all records from the view.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <returns>A list of all records.</returns>");
        sb.AppendLine("    [HttpGet]");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public async Task<ActionResult<IEnumerable<{className}>>> GetAll()");
        sb.AppendLine("    {");
        sb.AppendLine("        var results = await _repository.GetAllAsync();");
        sb.AppendLine("        return Ok(results);");
        sb.AppendLine("    }");

        // Search endpoint if there are text columns
        var textColumns = view.Columns
            .Where(c => IsTextColumn(c.DataType))
            .ToList();

        if (textColumns.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Searches records by text.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    /// <param name=\"term\">The search term.</param>");
            sb.AppendLine("    /// <returns>A list of matching records.</returns>");
            sb.AppendLine("    [HttpGet(\"search\")]");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    public async Task<ActionResult<IEnumerable<{className}>>> Search([FromQuery] string term)");
            sb.AppendLine("    {");
            sb.AppendLine("        if (string.IsNullOrWhiteSpace(term))");
            sb.AppendLine("        {");
            sb.AppendLine("            return BadRequest(\"Search term is required\");");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        var results = await _repository.SearchAsync(term);");
            sb.AppendLine("        return Ok(results);");
            sb.AppendLine("    }");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static bool IsTextColumn(string dataType)
    {
        var lowerType = dataType.ToUpperInvariant();
        return lowerType.Contains("char", StringComparison.Ordinal) ||
               lowerType.Contains("text", StringComparison.Ordinal);
    }
}
