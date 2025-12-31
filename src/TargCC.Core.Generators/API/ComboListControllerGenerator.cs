using System.Globalization;
using System.Text;

namespace TargCC.Core.Generators.API;

/// <summary>
/// Generates ComboList API controller for dropdown lookups.
/// </summary>
public static class ComboListControllerGenerator
{
    /// <summary>
    /// Generates the ComboListController with endpoints for all combo views.
    /// </summary>
    /// <param name="comboListTables">List of tables that have combo views.</param>
    /// <param name="rootNamespace">The root namespace.</param>
    /// <returns>The generated controller code.</returns>
    public static string Generate(IEnumerable<string> comboListTables, string rootNamespace)
    {
        var sb = new StringBuilder();

        // Using statements
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
        sb.AppendLine("using Microsoft.Data.SqlClient;");
        sb.AppendLine("using Microsoft.Extensions.Configuration;");
        sb.AppendLine("using Dapper;");
        sb.AppendLine();

        // Namespace
        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.API.Controllers;");
        sb.AppendLine();

        // Controller documentation
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// API controller for ComboList dropdown lookups.");
        sb.AppendLine("/// </summary>");

        // Controller attributes and declaration
        sb.AppendLine("[ApiController]");
        sb.AppendLine("[Route(\"api/[controller]\")]");
        sb.AppendLine("public class ComboListController : ControllerBase");
        sb.AppendLine("{");

        // Private fields
        sb.AppendLine("    private readonly IConfiguration _configuration;");
        sb.AppendLine();

        // Constructor
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Initializes a new instance of the <see cref=\"ComboListController\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"configuration\">The configuration.</param>");
        sb.AppendLine("    public ComboListController(IConfiguration configuration)");
        sb.AppendLine("    {");
        sb.AppendLine("        _configuration = configuration;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate endpoint for each table
        foreach (var tableName in comboListTables)
        {
            sb.AppendLine("    /// <summary>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Gets combo list items for {tableName}.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    /// <returns>List of combo items.</returns>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    [HttpGet(\"{tableName}\")]");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    public async Task<ActionResult<IEnumerable<ComboItem>>> Get{tableName}Combo()");
            sb.AppendLine("    {");
            sb.AppendLine("        using var connection = new SqlConnection(_configuration.GetConnectionString(\"DefaultConnection\"));");
            sb.AppendLine(CultureInfo.InvariantCulture, $"        const string sql = \"SELECT ID, Text, TextNS FROM dbo.ccvwComboList_{tableName}\";");
            sb.AppendLine("        var items = await connection.QueryAsync<ComboItem>(sql);");
            sb.AppendLine("        return Ok(items);");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        sb.AppendLine("}");
        sb.AppendLine();

        // ComboItem record
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Represents a combo list item.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("/// <param name=\"ID\">The item ID.</param>");
        sb.AppendLine("/// <param name=\"Text\">The display text.</param>");
        sb.AppendLine("/// <param name=\"TextNS\">The text without spaces.</param>");
        sb.AppendLine("public record ComboItem(int ID, string Text, string TextNS);");

        return sb.ToString();
    }
}
