// <copyright file="ReactListComponentGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI.Components
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Generates React List components with table view, sorting, pagination, and filtering.
    /// </summary>
    public class ReactListComponentGenerator : BaseComponentGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReactListComponentGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public ReactListComponentGenerator(ILogger<ReactListComponentGenerator> logger)
            : base(logger)
        {
        }

        /// <inheritdoc/>
        public override ComponentType ComponentType => ComponentType.List;

        /// <inheritdoc/>
        public override async Task<string> GenerateAsync(Table table, DatabaseSchema schema, ComponentGeneratorConfig config)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(config);

            LogComponentGeneration(table.Name);

            return await Task.Run(() => Generate(table, config)).ConfigureAwait(false);
        }

        private static string GenerateImports(string className, UIFramework framework)
        {
            var sb = new StringBuilder();

            sb.AppendLine("import React from 'react';");
            sb.AppendLine("import { useNavigate } from 'react-router-dom';");
            sb.AppendLine("import * as XLSX from 'xlsx';");

            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("import { DataGrid, GridColDef, GridActionsCellItem } from '@mui/x-data-grid';");
                sb.AppendLine("import { Button, Box, CircularProgress, Alert, TextField, Paper, IconButton } from '@mui/material';");
                sb.AppendLine("import { Edit as EditIcon, Delete as DeleteIcon, Add as AddIcon, FileDownload as FileDownloadIcon, Clear as ClearIcon, Search as SearchIcon } from '@mui/icons-material';");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"import {{ use{className}s, useDelete{className} }} from '../../hooks/use{className}';");
            sb.AppendLine(CultureInfo.InvariantCulture, $"import type {{ {className}, {className}Filters }} from '../../types/{className}.types';");

            return sb.ToString();
        }

        private static string GenerateColumns(Table table)
        {
            var sb = new StringBuilder();
            var className = GetClassName(table.Name);
            var dataColumns = GetDataColumns(table).Take(10).ToList(); // Limit to 10 columns for UI

            sb.AppendLine(CultureInfo.InvariantCulture, $"  const columns: GridColDef<{className}>[] = [");

            foreach (var column in dataColumns)
            {
                var propertyName = ToCamelCase(GetPropertyName(column.Name));
                var displayName = GetPropertyName(column.Name);
                var width = GetColumnWidth(column);

                sb.Append(CultureInfo.InvariantCulture, $"    {{ field: '{propertyName}', headerName: '{displayName}', width: {width}");

                // Add valueGetter for special types
                var (prefix, _) = SplitPrefix(column.Name);
                if (prefix == "LKP")
                {
                    var baseName = ToCamelCase(SplitPrefix(column.Name).baseName);
                    sb.Append(CultureInfo.InvariantCulture, $", valueGetter: (params) => params.row.{baseName}Text || params.row.{baseName}Code");
                }
                else if (prefix == "LOC")
                {
                    sb.Append(CultureInfo.InvariantCulture, $", valueGetter: (params) => params.row.{propertyName}Localized || params.row.{propertyName}");
                }
                else if (column.DataType.ToUpperInvariant().Contains("DATE", StringComparison.Ordinal))
                {
                    sb.Append(CultureInfo.InvariantCulture, $", valueFormatter: (params) => params.value ? new Date(params.value).toLocaleDateString() : ''");
                }

                sb.AppendLine(" },");
            }

            // Actions column
            sb.AppendLine("    {");
            sb.AppendLine("      field: 'actions',");
            sb.AppendLine("      type: 'actions',");
            sb.AppendLine("      headerName: 'Actions',");
            sb.AppendLine("      width: 100,");
            sb.AppendLine("      getActions: (params) => [");
            sb.AppendLine("        <GridActionsCellItem");
            sb.AppendLine("          icon={<EditIcon />}");
            sb.AppendLine("          label=\"Edit\"");
            sb.AppendLine(CultureInfo.InvariantCulture, $"          onClick={{() => navigate(`/{ToCamelCase(GetClassName(table.Name))}s/${{params.id}}`)}}");
            sb.AppendLine("        />,");
            sb.AppendLine("        <GridActionsCellItem");
            sb.AppendLine("          icon={<DeleteIcon />}");
            sb.AppendLine("          label=\"Delete\"");
            sb.AppendLine("          onClick={() => {");
            sb.AppendLine("            if (confirm('Are you sure?')) {");
            sb.AppendLine("              deleteEntity(params.id as number);");
            sb.AppendLine("            }");
            sb.AppendLine("          }}");
            sb.AppendLine("        />,");
            sb.AppendLine("      ],");
            sb.AppendLine("    },");
            sb.AppendLine("  ];");

            return sb.ToString();
        }

        private static string GenerateComponentBody(Table table, string className, string camelName, UIFramework framework)
        {
            var sb = new StringBuilder();
            var pluralName = camelName + "s";

            sb.AppendLine(CultureInfo.InvariantCulture, $"export const {className}List: React.FC = () => {{");
            sb.AppendLine("  const navigate = useNavigate();");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  const [filters, setFilters] = React.useState<{className}Filters>({{}});");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  const {{ data: {pluralName}, isLoading, error }} = use{className}s(filters);");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  const {{ mutate: deleteEntity }} = useDelete{className}();");
            sb.AppendLine();

            // Export to Excel handler
            sb.AppendLine("  const handleExportToExcel = () => {");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    if (!{pluralName} || {pluralName}.length === 0) {{");
            sb.AppendLine("      alert('No data to export');");
            sb.AppendLine("      return;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"    const ws = XLSX.utils.json_to_sheet({pluralName});");
            sb.AppendLine("    const wb = XLSX.utils.book_new();");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    XLSX.utils.book_append_sheet(wb, ws, '{className}s');");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    XLSX.writeFile(wb, `{className}s_${{new Date().toISOString().split('T')[0]}}.xlsx`);");
            sb.AppendLine("  };");
            sb.AppendLine();

            // Columns definition
            sb.AppendLine(GenerateColumns(table));
            sb.AppendLine();

            // Loading state
            sb.AppendLine("  if (isLoading) {");
            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("    return <CircularProgress />;");
            }
            else
            {
                sb.AppendLine("    return <div>Loading...</div>;");
            }

            sb.AppendLine("  }");
            sb.AppendLine();

            // Error state
            sb.AppendLine("  if (error) {");
            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"    return <Alert severity=\"error\">Failed to load {pluralName}</Alert>;");
            }
            else
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"    return <div className=\"text-red-500\">Failed to load {pluralName}</div>;");
            }

            sb.AppendLine("  }");
            sb.AppendLine();

            // Main render
            sb.AppendLine("  return (");
            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("    <Box sx={{ height: 600, width: '100%' }}>");
                sb.AppendLine("      <Box sx={{ mb: 2, display: 'flex', gap: 2 }}>");
                sb.AppendLine("        <Button");
                sb.AppendLine("          variant=\"contained\"");
                sb.AppendLine("          startIcon={<AddIcon />}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          onClick={{() => navigate('/{pluralName}/new')}}");
                sb.AppendLine("        >");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          Create {className}");
                sb.AppendLine("        </Button>");
                sb.AppendLine("        <Button");
                sb.AppendLine("          variant=\"outlined\"");
                sb.AppendLine("          startIcon={<FileDownloadIcon />}");
                sb.AppendLine("          onClick={handleExportToExcel}");
                sb.AppendLine("        >");
                sb.AppendLine("          Export to Excel");
                sb.AppendLine("        </Button>");
                sb.AppendLine("      </Box>");

                // Add Filter UI
                sb.AppendLine(GenerateFilterUI(table));

                sb.AppendLine("      <DataGrid");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        rows={{{pluralName} || []}}");
                sb.AppendLine("        columns={columns}");
                sb.AppendLine("        initialState={{");
                sb.AppendLine("          pagination: { paginationModel: { pageSize: 10 } },");
                sb.AppendLine("        }}");
                sb.AppendLine("        pageSizeOptions={[5, 10, 25, 100]}");
                sb.AppendLine("        checkboxSelection");
                sb.AppendLine("        disableRowSelectionOnClick");
                sb.AppendLine("      />");
                sb.AppendLine("    </Box>");
            }
            else
            {
                sb.AppendLine("    <div className=\"p-4\">");
                sb.AppendLine("      <div className=\"mb-4\">");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        <button onClick={{() => navigate('/{pluralName}/new')}} className=\"bg-blue-500 text-white px-4 py-2 rounded\">");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          Create {className}");
                sb.AppendLine("        </button>");
                sb.AppendLine("      </div>");
                sb.AppendLine("      {/* Table implementation */}");
                sb.AppendLine("    </div>");
            }

            sb.AppendLine("  );");
            sb.AppendLine("};");

            return sb.ToString();
        }

        private static string GenerateFilterUI(Table table)
        {
            var filterableIndexes = GetFilterableIndexes(table);
            if (filterableIndexes.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.AppendLine("      <Paper sx={{ p: 2, mb: 2 }}>");
            sb.AppendLine("        <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>");

            var processedColumns = new System.Collections.Generic.HashSet<string>();
            foreach (var index in filterableIndexes)
            {
                AppendFilterFieldsForIndex(sb, table, index, processedColumns);
            }

            AppendClearFiltersButton(sb);
            sb.AppendLine("        </Box>");
            sb.AppendLine("      </Paper>");

            return sb.ToString();
        }

        private static List<Index> GetFilterableIndexes(Table table)
        {
            return table.Indexes?
                .Where(i => !i.IsPrimaryKey && i.ColumnNames != null && i.ColumnNames.Count > 0)
                .ToList() ?? new List<Index>();
        }

        private static void AppendFilterFieldsForIndex(StringBuilder sb, Table table, Index index, System.Collections.Generic.HashSet<string> processedColumns)
        {
            foreach (var columnName in index.ColumnNames)
            {
                if (processedColumns.Add(columnName))
                {
                    var column = table.Columns.Find(c => c.Name == columnName);
                    if (column != null)
                    {
                        AppendFilterField(sb, column);
                    }
                }
            }
        }

        private static void AppendFilterField(StringBuilder sb, Column column)
        {
            var propertyName = ToCamelCase(GetPropertyName(column.Name));
            var displayName = GetPropertyName(column.Name);
            var isNumeric = IsNumericType(column.DataType);
            var isDate = IsDateType(column.DataType);

            var inputType = GetInputType(isDate, isNumeric);
            var valueConversion = GetValueConversion(isNumeric);
            var valueDisplay = GetValueDisplay(propertyName, isDate);

            sb.AppendLine("          <TextField");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            label=\"{displayName}\"");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            type=\"{inputType}\"");
            sb.AppendLine("            size=\"small\"");
            sb.AppendLine("            sx={{ minWidth: 200 }}");

            if (isDate)
            {
                sb.AppendLine("            InputLabelProps={{ shrink: true }}");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"            value={{{valueDisplay}}}");
            sb.AppendLine("            onChange={(e) => setFilters(prev => ({");
            sb.AppendLine("              ...prev,");
            sb.AppendLine(CultureInfo.InvariantCulture, $"              {propertyName}: {valueConversion}");
            sb.AppendLine("            }))}");
            sb.AppendLine("          />");
        }

        private static string GetInputType(bool isDate, bool isNumeric)
        {
            if (isDate)
            {
                return "date";
            }

            if (isNumeric)
            {
                return "number";
            }

            return "text";
        }

        private static string GetValueConversion(bool isNumeric)
        {
            if (isNumeric)
            {
                return "e.target.value ? Number(e.target.value) : undefined";
            }

            return "e.target.value || undefined";
        }

        private static string GetValueDisplay(string propertyName, bool isDate)
        {
            if (isDate)
            {
                return $"filters.{propertyName} ? (filters.{propertyName} instanceof Date ? filters.{propertyName}.toISOString().split('T')[0] : filters.{propertyName}) : ''";
            }

            return $"filters.{propertyName} ?? ''";
        }

        private static void AppendClearFiltersButton(StringBuilder sb)
        {
            sb.AppendLine("          <IconButton");
            sb.AppendLine("            color=\"primary\"");
            sb.AppendLine("            onClick={() => setFilters({})}");
            sb.AppendLine("            title=\"Clear Filters\"");
            sb.AppendLine("          >");
            sb.AppendLine("            <ClearIcon />");
            sb.AppendLine("          </IconButton>");
        }

        private static bool IsNumericType(string dataType)
        {
            var upperType = dataType.ToUpperInvariant();
            return upperType.Contains("INT", StringComparison.Ordinal) ||
                   upperType.Contains("DECIMAL", StringComparison.Ordinal) ||
                   upperType.Contains("NUMERIC", StringComparison.Ordinal) ||
                   upperType.Contains("FLOAT", StringComparison.Ordinal) ||
                   upperType.Contains("DOUBLE", StringComparison.Ordinal) ||
                   upperType.Contains("MONEY", StringComparison.Ordinal);
        }

        private static bool IsDateType(string dataType)
        {
            return dataType.ToUpperInvariant().Contains("DATE", StringComparison.Ordinal);
        }

        private static int GetColumnWidth(Column column)
        {
            var (prefix, _) = SplitPrefix(column.Name);

            if (column.IsPrimaryKey)
            {
                return 90;
            }

            if (prefix == "ENO")
            {
                return 150;
            }

            if (column.DataType.ToUpperInvariant().Contains("BIT", StringComparison.Ordinal))
            {
                return 100;
            }

            if (column.DataType.ToUpperInvariant().Contains("DATE", StringComparison.Ordinal))
            {
                return 150;
            }

            if (column.MaxLength.HasValue)
            {
                if (column.MaxLength.Value <= 50)
                {
                    return 150;
                }

                if (column.MaxLength.Value <= 100)
                {
                    return 200;
                }

                return 250;
            }

            return 200;
        }

        private static string Generate(Table table, ComponentGeneratorConfig config)
        {
            var sb = new StringBuilder();
            var className = GetClassName(table.Name);
            var camelName = GetCamelCaseName(table.Name);

            // Header
            sb.Append(GenerateComponentHeader(table.Name));

            // Imports
            sb.AppendLine(GenerateImports(className, config.Framework));
            sb.AppendLine();

            // Component
            sb.AppendLine(GenerateComponentBody(table, className, camelName, config.Framework));

            return sb.ToString();
        }
    }
}
