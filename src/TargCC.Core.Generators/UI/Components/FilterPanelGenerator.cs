using System.Globalization;
using System.Text;
using TargCC.Core.Generators.API;
using TargCC.Core.Generators.Common;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Generators.UI.Components;

/// <summary>
/// Generates filter panel components for list screens.
/// </summary>
public static class FilterPanelGenerator
{
    /// <summary>
    /// Generates filter panel code for a table.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <returns>Filter panel JSX code.</returns>
    public static string GenerateFilterPanel(Table table)
    {
        var sb = new StringBuilder();
        var columns = table.Columns
            .Where(c => !c.IsPrimaryKey && !c.IsIdentity && IsFilterableColumn(c))
            .Take(5) // Limit to first 5 filterable columns
            .ToList();

        if (columns.Count == 0)
        {
            return string.Empty;
        }

        sb.AppendLine("  const [filters, setFilters] = useState({");
        foreach (var column in columns)
        {
            var fieldName = CodeGenerationHelpers.ToCamelCase(CodeGenerationHelpers.SanitizeColumnName(column.Name));
            var defaultValue = GetDefaultFilterValue(column);
            sb.AppendLine(CultureInfo.InvariantCulture, $"    {fieldName}: {defaultValue},");
        }

        sb.AppendLine("  });");
        sb.AppendLine();

        // Filter UI
        sb.AppendLine("  const renderFilters = () => (");
        sb.AppendLine("    <Paper sx={{ p: 2, mb: 2 }}>");
        sb.AppendLine("      <Grid container spacing={2} alignItems=\"center\">");

        foreach (var column in columns)
        {
            var fieldName = CodeGenerationHelpers.ToCamelCase(CodeGenerationHelpers.SanitizeColumnName(column.Name));
            var label = CodeGenerationHelpers.SanitizeColumnName(column.Name);

            sb.AppendLine("        <Grid item xs={12} sm={6} md={2}>");

            if (IsEnumColumn(column))
            {
                sb.AppendLine("          <FormControl fullWidth size=\"small\">");
                sb.AppendLine(CultureInfo.InvariantCulture, $"            <InputLabel>{label}</InputLabel>");
                sb.AppendLine("            <Select");
                sb.AppendLine(CultureInfo.InvariantCulture, $"              label=\"{label}\"");
                sb.AppendLine(CultureInfo.InvariantCulture, $"              value={{filters.{fieldName}}}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"              onChange={{(e) => setFilters({{...filters, {fieldName}: e.target.value}})}}");
                sb.AppendLine("            >");
                sb.AppendLine("              <MenuItem value=\"\">All</MenuItem>");
                sb.AppendLine("              {/* Add enum options here */}");
                sb.AppendLine("            </Select>");
                sb.AppendLine("          </FormControl>");
            }
            else if (IsBooleanColumn(column))
            {
                sb.AppendLine("          <FormControl fullWidth size=\"small\">");
                sb.AppendLine(CultureInfo.InvariantCulture, $"            <InputLabel>{label}</InputLabel>");
                sb.AppendLine("            <Select");
                sb.AppendLine(CultureInfo.InvariantCulture, $"              label=\"{label}\"");
                sb.AppendLine(CultureInfo.InvariantCulture, $"              value={{filters.{fieldName}}}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"              onChange={{(e) => setFilters({{...filters, {fieldName}: e.target.value}})}}");
                sb.AppendLine("            >");
                sb.AppendLine("              <MenuItem value=\"\">All</MenuItem>");
                sb.AppendLine("              <MenuItem value=\"true\">Yes</MenuItem>");
                sb.AppendLine("              <MenuItem value=\"false\">No</MenuItem>");
                sb.AppendLine("            </Select>");
                sb.AppendLine("          </FormControl>");
            }
            else if (IsDateColumn(column))
            {
                sb.AppendLine("          <TextField");
                sb.AppendLine("            fullWidth");
                sb.AppendLine("            size=\"small\"");
                sb.AppendLine(CultureInfo.InvariantCulture, $"            label=\"{label}\"");
                sb.AppendLine("            type=\"date\"");
                sb.AppendLine(CultureInfo.InvariantCulture, $"            value={{filters.{fieldName}}}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"            onChange={{(e) => setFilters({{...filters, {fieldName}: e.target.value}})}}");
                sb.AppendLine("            InputLabelProps={{ shrink: true }}");
                sb.AppendLine("          />");
            }
            else
            {
                sb.AppendLine("          <TextField");
                sb.AppendLine("            fullWidth");
                sb.AppendLine("            size=\"small\"");
                sb.AppendLine(CultureInfo.InvariantCulture, $"            label=\"{label}\"");
                sb.AppendLine(CultureInfo.InvariantCulture, $"            value={{filters.{fieldName}}}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"            onChange={{(e) => setFilters({{...filters, {fieldName}: e.target.value}})}}");
                sb.AppendLine("          />");
            }

            sb.AppendLine("        </Grid>");
        }

        // Apply/Clear buttons
        sb.AppendLine("        <Grid item xs={12} sm={6} md={2}>");
        sb.AppendLine("          <Button");
        sb.AppendLine("            fullWidth");
        sb.AppendLine("            variant=\"contained\"");
        sb.AppendLine("            onClick={handleApplyFilters}");
        sb.AppendLine("          >");
        sb.AppendLine("            Apply");
        sb.AppendLine("          </Button>");
        sb.AppendLine("        </Grid>");
        sb.AppendLine("        <Grid item xs={12} sm={6} md={2}>");
        sb.AppendLine("          <Button");
        sb.AppendLine("            fullWidth");
        sb.AppendLine("            variant=\"outlined\"");
        sb.AppendLine("            onClick={handleClearFilters}");
        sb.AppendLine("          >");
        sb.AppendLine("            Clear");
        sb.AppendLine("          </Button>");
        sb.AppendLine("        </Grid>");

        sb.AppendLine("      </Grid>");
        sb.AppendLine("    </Paper>");
        sb.AppendLine("  );");

        return sb.ToString();
    }

    private static bool IsFilterableColumn(Column column)
    {
        // Don't filter audit columns or computed columns
        var name = column.Name.ToUpperInvariant();
        if (name.Contains("CREATEDON", StringComparison.Ordinal) ||
            name.Contains("CREATEDBY", StringComparison.Ordinal) ||
            name.Contains("UPDATEDON", StringComparison.Ordinal) ||
            name.Contains("UPDATEDBY", StringComparison.Ordinal) ||
            name.Contains("DELETEDON", StringComparison.Ordinal) ||
            name.Contains("DELETEDBY", StringComparison.Ordinal) ||
            column.IsComputed)
        {
            return false;
        }

        return true;
    }

    private static bool IsEnumColumn(Column column)
    {
        return column.Name.StartsWith("enm_", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBooleanColumn(Column column)
    {
        return column.DataType.Contains("BIT", StringComparison.OrdinalIgnoreCase) ||
               column.DataType.Contains("BOOL", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDateColumn(Column column)
    {
        var type = column.DataType.ToUpperInvariant();
        return type.Contains("DATE", StringComparison.Ordinal) ||
               type.Contains("TIME", StringComparison.Ordinal);
    }

    private static string GetDefaultFilterValue(Column column)
    {
        if (IsBooleanColumn(column))
        {
            return "\"\"";
        }

        if (IsDateColumn(column))
        {
            return "\"\"";
        }

        return "\"\"";
    }
}
