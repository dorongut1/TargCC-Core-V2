using System.Globalization;
using System.Text;
using TargCC.Core.Generators.API;
using TargCC.Core.Generators.Common;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Generators.Repositories;

/// <summary>
/// Generates read-only repository interfaces and implementations for database views.
/// </summary>
public static class ViewRepositoryGenerator
{
    /// <summary>
    /// Generates the repository interface for a view.
    /// </summary>
    /// <param name="view">The view information.</param>
    /// <param name="rootNamespace">The root namespace.</param>
    /// <returns>The generated interface code.</returns>
    public static string GenerateInterface(ViewInfo view, string rootNamespace)
    {
        var className = BaseApiGenerator.GetClassName(view.ViewName);
        var sb = new StringBuilder();

        // Using statements
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Domain.Entities;");
        sb.AppendLine();

        // Namespace
        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Application.Interfaces.Repositories;");
        sb.AppendLine();

        // Interface documentation
        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Repository interface for the {view.ViewName} view.");
        sb.AppendLine("/// This is a read-only repository as it operates on a database view.");
        sb.AppendLine("/// </summary>");

        // Interface declaration
        sb.AppendLine(CultureInfo.InvariantCulture, $"public interface I{className}Repository");
        sb.AppendLine("{");

        // GetAllAsync method
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Retrieves all records from the view.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <returns>A collection of all records.</returns>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    Task<IEnumerable<{className}>> GetAllAsync();");
        sb.AppendLine();

        // SearchAsync method for text columns
        var textColumns = view.Columns
            .Where(c => IsTextColumn(c.DataType))
            .ToList();

        if (textColumns.Count > 0)
        {
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Searches records by text across all text columns.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    /// <param name=\"searchTerm\">The search term.</param>");
            sb.AppendLine("    /// <returns>A collection of matching records.</returns>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    Task<IEnumerable<{className}>> SearchAsync(string searchTerm);");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates the repository implementation for a view.
    /// </summary>
    /// <param name="view">The view information.</param>
    /// <param name="rootNamespace">The root namespace.</param>
    /// <returns>The generated implementation code.</returns>
    public static string GenerateImplementation(ViewInfo view, string rootNamespace)
    {
        var className = BaseApiGenerator.GetClassName(view.ViewName);
        var sb = new StringBuilder();

        // Using statements
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Dapper;");
        sb.AppendLine("using Microsoft.Data.SqlClient;");
        sb.AppendLine("using Microsoft.Extensions.Configuration;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Application.Interfaces.Repositories;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {rootNamespace}.Domain.Entities;");
        sb.AppendLine();

        // Namespace
        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Infrastructure.Repositories;");
        sb.AppendLine();

        // Class documentation
        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Repository implementation for the {view.ViewName} view.");
        sb.AppendLine("/// This is a read-only repository as it operates on a database view.");
        sb.AppendLine("/// </summary>");

        // Class declaration
        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {className}Repository : I{className}Repository");
        sb.AppendLine("{");

        // Private fields
        sb.AppendLine("    private readonly IConfiguration _configuration;");
        sb.AppendLine();

        // Constructor
        sb.AppendLine("    /// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Initializes a new instance of the <see cref=\"{className}Repository\"/> class.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"configuration\">The configuration.</param>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public {className}Repository(IConfiguration configuration)");
        sb.AppendLine("    {");
        sb.AppendLine("        _configuration = configuration;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // GetAllAsync method
        sb.AppendLine("    /// <inheritdoc/>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    public async Task<IEnumerable<{className}>> GetAllAsync()");
        sb.AppendLine("    {");
        sb.AppendLine("        using var connection = new SqlConnection(_configuration.GetConnectionString(\"DefaultConnection\"));");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        const string sql = \"SELECT * FROM {view.SchemaName}.{view.ViewName}\";");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        return await connection.QueryAsync<{className}>(sql);");
        sb.AppendLine("    }");

        // SearchAsync method if there are text columns
        var textColumns = view.Columns
            .Where(c => IsTextColumn(c.DataType))
            .ToList();

        if (textColumns.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("    /// <inheritdoc/>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    public async Task<IEnumerable<{className}>> SearchAsync(string searchTerm)");
            sb.AppendLine("    {");
            sb.AppendLine("        using var connection = new SqlConnection(_configuration.GetConnectionString(\"DefaultConnection\"));");
            sb.AppendLine();
            sb.AppendLine("        var whereConditions = new List<string>();");
            sb.AppendLine("        var parameters = new DynamicParameters();");
            sb.AppendLine("        parameters.Add(\"@SearchTerm\", $\"%{searchTerm}%\");");
            sb.AppendLine();

            // Build WHERE clause for each text column
            foreach (var column in textColumns)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        whereConditions.Add(\"[{column.Name}] LIKE @SearchTerm\");");
            }

            sb.AppendLine();
            sb.AppendLine("        var whereClause = string.Join(\" OR \", whereConditions);");
            sb.AppendLine(CultureInfo.InvariantCulture, $"        var sql = $\"SELECT * FROM {view.SchemaName}.{view.ViewName} WHERE {{whereClause}}\";");
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"        return await connection.QueryAsync<{className}>(sql, parameters);");
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
