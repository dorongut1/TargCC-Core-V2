using System.Globalization;
using System.Text;
using TargCC.Core.Generators.API;
using TargCC.Core.Generators.Common;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Generators.UI.Components;

/// <summary>
/// Generates React report screen components for database views.
/// </summary>
public static class ReactReportComponentGenerator
{
    /// <summary>
    /// Generates a React report component for a view.
    /// </summary>
    /// <param name="view">The view information.</param>
    /// <param name="rootNamespace">The root namespace.</param>
    /// <returns>The generated component code.</returns>
    public static string Generate(ViewInfo view, string rootNamespace)
    {
        var className = BaseApiGenerator.GetClassName(view.ViewName);
        var pluralName = CodeGenerationHelpers.MakePlural(className);
        var camelCasePlural = CodeGenerationHelpers.ToCamelCase(pluralName);

        var sb = new StringBuilder();

        // Imports
        sb.AppendLine("import { useState } from 'react';");
        sb.AppendLine("import { useQuery } from '@tanstack/react-query';");
        sb.AppendLine("import {");
        sb.AppendLine("  Box,");
        sb.AppendLine("  Paper,");
        sb.AppendLine("  Toolbar,");
        sb.AppendLine("  Typography,");
        sb.AppendLine("  TextField,");
        sb.AppendLine("  Button,");
        sb.AppendLine("  CircularProgress");
        sb.AppendLine("} from '@mui/material';");
        sb.AppendLine("import { DataGrid, GridColDef } from '@mui/x-data-grid';");
        sb.AppendLine("import DownloadIcon from '@mui/icons-material/Download';");
        sb.AppendLine("import SearchIcon from '@mui/icons-material/Search';");
        sb.AppendLine("import api from '../../api/client';");
        sb.AppendLine();

        // Component
        sb.AppendLine(CultureInfo.InvariantCulture, $"export function {className}Report() {{");
        sb.AppendLine("  const [searchTerm, setSearchTerm] = useState('');");
        sb.AppendLine("  const [debouncedSearch, setDebouncedSearch] = useState('');");
        sb.AppendLine();

        // Query
        sb.AppendLine(CultureInfo.InvariantCulture, $"  const {{ data: rows, isLoading }} = useQuery({{");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    queryKey: ['{camelCasePlural}', debouncedSearch],");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    queryFn: () => {{");
        sb.AppendLine("      const url = debouncedSearch");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        ? `/{pluralName}/search?term=${{debouncedSearch}}`");
        sb.AppendLine(CultureInfo.InvariantCulture, $"        : `/{pluralName}`;");
        sb.AppendLine("      return api.get(url).then(res => res.data);");
        sb.AppendLine("    }");
        sb.AppendLine("  });");
        sb.AppendLine();

        // Handle search
        sb.AppendLine("  const handleSearch = () => {");
        sb.AppendLine("    setDebouncedSearch(searchTerm);");
        sb.AppendLine("  };");
        sb.AppendLine();

        // Handle export CSV
        sb.AppendLine("  const handleExportCsv = () => {");
        sb.AppendLine("    if (!rows || rows.length === 0) return;");
        sb.AppendLine();
        sb.AppendLine("    const headers = columns.map(col => col.headerName).join(',');");
        sb.AppendLine("    const csvRows = rows.map((row: any) =>");
        sb.AppendLine("      columns.map(col => {");
        sb.AppendLine("        const value = row[col.field];");
        sb.AppendLine("        if (value === null || value === undefined) return '';");
        sb.AppendLine("        if (typeof value === 'string' && value.includes(',')) {");
        sb.AppendLine("          return `\"${value}\"`;");
        sb.AppendLine("        }");
        sb.AppendLine("        return value;");
        sb.AppendLine("      }).join(',')");
        sb.AppendLine("    );");
        sb.AppendLine();
        sb.AppendLine("    const csv = [headers, ...csvRows].join('\\n');");
        sb.AppendLine("    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });");
        sb.AppendLine("    const link = document.createElement('a');");
        sb.AppendLine("    link.href = URL.createObjectURL(blob);");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    link.download = '{view.ViewName}_{{new Date().toISOString().split(\"T\")[0]}}.csv';");
        sb.AppendLine("    link.click();");
        sb.AppendLine("  };");
        sb.AppendLine();

        // Columns definition
        sb.AppendLine("  const columns: GridColDef[] = [");

        foreach (var column in view.Columns.OrderBy(c => c.OrdinalPosition))
        {
            var fieldName = CodeGenerationHelpers.ToCamelCase(CodeGenerationHelpers.SanitizeColumnName(column.Name));
            var headerName = CodeGenerationHelpers.SanitizeColumnName(column.Name);
            var columnType = DetectGridColumnType(column);
            var width = EstimateColumnWidth(column);

            sb.Append(CultureInfo.InvariantCulture, $"    {{ field: '{fieldName}', headerName: '{headerName}', width: {width}");

            if (columnType != "string")
            {
                sb.Append(CultureInfo.InvariantCulture, $", type: '{columnType}'");
            }

            sb.AppendLine(" },");
        }

        sb.AppendLine("  ];");
        sb.AppendLine();

        // Loading state
        sb.AppendLine("  if (isLoading) {");
        sb.AppendLine("    return (");
        sb.AppendLine("      <Box display=\"flex\" justifyContent=\"center\" alignItems=\"center\" minHeight=\"400px\">");
        sb.AppendLine("        <CircularProgress />");
        sb.AppendLine("      </Box>");
        sb.AppendLine("    );");
        sb.AppendLine("  }");
        sb.AppendLine();

        // Render
        sb.AppendLine("  return (");
        sb.AppendLine("    <Box sx={{ p: 3 }}>");
        sb.AppendLine("      <Paper>");
        sb.AppendLine("        <Toolbar sx={{ justifyContent: 'space-between' }}>");
        sb.AppendLine("          <Typography variant=\"h6\">");
        sb.AppendLine(CultureInfo.InvariantCulture, $"            {className} Report");
        sb.AppendLine("          </Typography>");
        sb.AppendLine();
        sb.AppendLine("          <Box sx={{ display: 'flex', gap: 2 }}>");
        sb.AppendLine("            <TextField");
        sb.AppendLine("              size=\"small\"");
        sb.AppendLine("              label=\"Search\"");
        sb.AppendLine("              value={searchTerm}");
        sb.AppendLine("              onChange={(e) => setSearchTerm(e.target.value)}");
        sb.AppendLine("              onKeyPress={(e) => {");
        sb.AppendLine("                if (e.key === 'Enter') handleSearch();");
        sb.AppendLine("              }}");
        sb.AppendLine("            />");
        sb.AppendLine("            <Button");
        sb.AppendLine("              variant=\"outlined\"");
        sb.AppendLine("              startIcon={<SearchIcon />}");
        sb.AppendLine("              onClick={handleSearch}");
        sb.AppendLine("            >");
        sb.AppendLine("              Search");
        sb.AppendLine("            </Button>");
        sb.AppendLine("            <Button");
        sb.AppendLine("              variant=\"outlined\"");
        sb.AppendLine("              startIcon={<DownloadIcon />}");
        sb.AppendLine("              onClick={handleExportCsv}");
        sb.AppendLine("            >");
        sb.AppendLine("              Export CSV");
        sb.AppendLine("            </Button>");
        sb.AppendLine("          </Box>");
        sb.AppendLine("        </Toolbar>");
        sb.AppendLine();
        sb.AppendLine("        <DataGrid");
        sb.AppendLine("          rows={rows || []}");
        sb.AppendLine("          columns={columns}");
        sb.AppendLine("          autoHeight");
        sb.AppendLine("          pageSizeOptions={[10, 25, 50, 100]}");
        sb.AppendLine("          initialState={{");
        sb.AppendLine("            pagination: {");
        sb.AppendLine("              paginationModel: { pageSize: 25 }");
        sb.AppendLine("            }");
        sb.AppendLine("          }}");
        sb.AppendLine("          getRowId={(row) => row.id || Math.random()}");
        sb.AppendLine("          disableRowSelectionOnClick");
        sb.AppendLine("        />");
        sb.AppendLine("      </Paper>");
        sb.AppendLine("    </Box>");
        sb.AppendLine("  );");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string DetectGridColumnType(ViewColumn column)
    {
        var typeLower = column.DataType.ToUpperInvariant();

        if (typeLower.Contains("int", StringComparison.Ordinal) ||
            typeLower.Contains("decimal", StringComparison.Ordinal) ||
            typeLower.Contains("money", StringComparison.Ordinal) ||
            typeLower.Contains("float", StringComparison.Ordinal) ||
            typeLower.Contains("real", StringComparison.Ordinal) ||
            typeLower.Contains("numeric", StringComparison.Ordinal))
        {
            return "number";
        }

        if (typeLower.Contains("date", StringComparison.Ordinal) ||
            typeLower.Contains("time", StringComparison.Ordinal))
        {
            return "date";
        }

        if (typeLower.Contains("bit", StringComparison.Ordinal) ||
            typeLower.Contains("bool", StringComparison.Ordinal))
        {
            return "boolean";
        }

        return "string";
    }

    private static int EstimateColumnWidth(ViewColumn column)
    {
        var typeLower = column.DataType.ToUpperInvariant();

        // Boolean columns are narrow
        if (typeLower.Contains("bit", StringComparison.Ordinal))
        {
            return 100;
        }

        // Date columns
        if (typeLower.Contains("date", StringComparison.Ordinal) ||
            typeLower.Contains("time", StringComparison.Ordinal))
        {
            return 150;
        }

        // Number columns
        if (typeLower.Contains("int", StringComparison.Ordinal) ||
            typeLower.Contains("decimal", StringComparison.Ordinal) ||
            typeLower.Contains("money", StringComparison.Ordinal))
        {
            return 120;
        }

        // String columns - estimate based on max length
        if (column.MaxLength.HasValue)
        {
            if (column.MaxLength.Value <= 10)
            {
                return 100;
            }

            if (column.MaxLength.Value <= 50)
            {
                return 150;
            }

            if (column.MaxLength.Value <= 100)
            {
                return 200;
            }

            // Very long text
            return 250;
        }

        // Default
        return 150;
    }
}
