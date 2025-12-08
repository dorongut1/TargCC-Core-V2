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

        private static string GenerateImports(Table table, string className, UIFramework framework)
        {
            var sb = new StringBuilder();

            sb.AppendLine("import React from 'react';");
            sb.AppendLine("import { useNavigate } from 'react-router-dom';");
            sb.AppendLine("import * as XLSX from 'xlsx-js-style';");

            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("import { DataGrid, GridColDef, GridActionsCellItem, GridToolbarContainer, GridToolbarColumnsButton, GridToolbarDensitySelector, useGridApiRef } from '@mui/x-data-grid';");
                sb.AppendLine("import { Button, Box, CircularProgress, Alert, TextField, Paper, MenuItem, Typography } from '@mui/material';");

                // Only import Edit/Delete/Add icons for tables, not for VIEWs
                if (!table.IsView)
                {
                    sb.AppendLine("import { Edit as EditIcon, Delete as DeleteIcon, Add as AddIcon, FileDownload as FileDownloadIcon, Clear as ClearIcon } from '@mui/icons-material';");
                }
                else
                {
                    sb.AppendLine("import { FileDownload as FileDownloadIcon, Clear as ClearIcon } from '@mui/icons-material';");
                }
            }

            // Only import useDelete hook for tables, not for VIEWs
            if (!table.IsView)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"import {{ use{className}s, useDelete{className} }} from '../../hooks/use{className}';");
            }
            else
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"import {{ use{className}s }} from '../../hooks/use{className}';");
            }

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
                    sb.Append(CultureInfo.InvariantCulture, $", valueGetter: (_, row) => row.{baseName}Text || row.{baseName}Code");
                }
                else if (prefix == "LOC")
                {
                    sb.Append(CultureInfo.InvariantCulture, $", valueGetter: (_, row) => row.{propertyName}Localized || row.{propertyName}");
                }
                else if (column.DataType.ToUpperInvariant().Contains("DATE", StringComparison.Ordinal))
                {
                    sb.Append(CultureInfo.InvariantCulture, $", valueFormatter: (value) => value ? new Date(value).toLocaleDateString() : ''");
                }

                sb.AppendLine(" },");
            }

            // Actions column - only for tables, not for views (views are read-only)
            if (!table.IsView)
            {
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
            }

            sb.AppendLine("  ];");

            return sb.ToString();
        }

        private static string GenerateComponentBody(Table table, string className, string camelName, UIFramework framework)
        {
            var sb = new StringBuilder();
            var pluralName = camelName + "s";

            // Get primary key column name for DataGrid getRowId
            var pkColumn = table.Columns.Find(c => c.IsPrimaryKey);
            var pkPropertyName = pkColumn != null ? GetPropertyName(pkColumn.Name) : "id";
            var pkCamelName = pkPropertyName.Length > 0 ? char.ToLowerInvariant(pkPropertyName[0]) + pkPropertyName.Substring(1) : "id";

            sb.AppendLine(CultureInfo.InvariantCulture, $"export const {className}List: React.FC = () => {{");
            sb.AppendLine("  const navigate = useNavigate();");
            sb.AppendLine("  const apiRef = useGridApiRef();");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  const [columnFilters, setColumnFilters] = React.useState<Record<string, string>>({{}});");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  const {{ data: {pluralName}, isLoading, error }} = use{className}s({{}});");

            // Only add useDelete hook for tables, not for VIEWs
            if (!table.IsView)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"  const {{ mutate: deleteEntity }} = useDelete{className}();");
            }

            sb.AppendLine();

            // Generate filtered data logic
            sb.AppendLine("  // Client-side column filters");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  const filteredData = React.useMemo(() => {{");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    if (!{pluralName}) return {pluralName};");
            sb.AppendLine();
            sb.AppendLine("    // Get active filters");
            sb.AppendLine("    const activeFilters = Object.entries(columnFilters).filter(([_, value]) => value && value.trim() !== '');");
            sb.AppendLine("    if (activeFilters.length === 0) return " + pluralName + ";");
            sb.AppendLine();
            sb.AppendLine("    // Apply all filters with AND logic");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    return {pluralName}.filter(item => {{");
            sb.AppendLine("      return activeFilters.every(([column, filterValue]) => {");
            sb.AppendLine("        const itemValue = item[column as keyof typeof item];");
            sb.AppendLine("        return itemValue?.toString().toLowerCase().includes(filterValue.toLowerCase());");
            sb.AppendLine("      });");
            sb.AppendLine("    });");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  }}, [{pluralName}, columnFilters]);");
            sb.AppendLine();

            sb.AppendLine("  const handleClearAllFilters = () => {");
            sb.AppendLine("    setColumnFilters({});");
            sb.AppendLine("  };");
            sb.AppendLine();
            sb.AppendLine("  function CustomToolbar() {");
            sb.AppendLine("    return (");
            sb.AppendLine("      <GridToolbarContainer>");
            sb.AppendLine("        <GridToolbarColumnsButton />");
            sb.AppendLine("        <GridToolbarDensitySelector />");
            sb.AppendLine("        <Button");
            sb.AppendLine("          size=\"small\"");
            sb.AppendLine("          startIcon={<ClearIcon />}");
            sb.AppendLine("          onClick={handleClearAllFilters}");
            sb.AppendLine("        >");
            sb.AppendLine("          Clear All Filters");
            sb.AppendLine("        </Button>");
            sb.AppendLine("      </GridToolbarContainer>");
            sb.AppendLine("    );");
            sb.AppendLine("  }");
            sb.AppendLine();

            // Export to Excel handler
            sb.AppendLine("  const handleExportToExcel = () => {");
            sb.AppendLine("    // Export only the filtered data visible on screen");
            sb.AppendLine("    const visibleRows = filteredData || [];");
            sb.AppendLine();
            sb.AppendLine("    if (!visibleRows || visibleRows.length === 0) {");
            sb.AppendLine("      alert('No data to export');");
            sb.AppendLine("      return;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    const ws = XLSX.utils.json_to_sheet(visibleRows);");
            sb.AppendLine();
            sb.AppendLine("    // Get the range of the worksheet");
            sb.AppendLine("    const range = XLSX.utils.decode_range(ws['!ref'] || 'A1');");
            sb.AppendLine();
            sb.AppendLine("    // Style the header row");
            sb.AppendLine("    for (let col = range.s.c; col <= range.e.c; col++) {");
            sb.AppendLine("      const cellAddress = XLSX.utils.encode_cell({ r: 0, c: col });");
            sb.AppendLine("      if (!ws[cellAddress]) continue;");
            sb.AppendLine("      ws[cellAddress].s = {");
            sb.AppendLine("        font: { bold: true, color: { rgb: 'FFFFFF' } },");
            sb.AppendLine("        fill: { fgColor: { rgb: '4472C4' } },");
            sb.AppendLine("        alignment: { horizontal: 'center', vertical: 'center' }");
            sb.AppendLine("      };");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    // Set column widths (auto-fit)");
            sb.AppendLine("    const colWidths = [];");
            sb.AppendLine("    for (let col = range.s.c; col <= range.e.c; col++) {");
            sb.AppendLine("      let maxWidth = 10;");
            sb.AppendLine("      for (let row = range.s.r; row <= range.e.r; row++) {");
            sb.AppendLine("        const cellAddress = XLSX.utils.encode_cell({ r: row, c: col });");
            sb.AppendLine("        if (ws[cellAddress] && ws[cellAddress].v) {");
            sb.AppendLine("          const cellLength = String(ws[cellAddress].v).length;");
            sb.AppendLine("          maxWidth = Math.max(maxWidth, cellLength);");
            sb.AppendLine("        }");
            sb.AppendLine("      }");
            sb.AppendLine("      colWidths.push({ wch: Math.min(maxWidth + 2, 50) });");
            sb.AppendLine("    }");
            sb.AppendLine("    ws['!cols'] = colWidths;");
            sb.AppendLine();
            sb.AppendLine("    // Freeze the header row (using both methods for maximum compatibility)");
            sb.AppendLine("    ws['!freeze'] = { xSplit: 0, ySplit: 1 };");
            sb.AppendLine("    ws['!views'] = [{ state: 'frozen', xSplit: 0, ySplit: 1, topLeftCell: 'A2' }];");
            sb.AppendLine();
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
                sb.AppendLine("    <Box sx={{ height: 'calc(100vh - 200px)', width: '100%', display: 'flex', flexDirection: 'column' }}>");
                sb.AppendLine("      <Box sx={{ mb: 2, display: 'flex', gap: 2 }}>");

                // Only show Create button for tables, not for views (views are read-only)
                if (!table.IsView)
                {
                    sb.AppendLine("        <Button");
                    sb.AppendLine("          variant=\"contained\"");
                    sb.AppendLine("          startIcon={<AddIcon />}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"          onClick={{() => navigate('/{pluralName}/new')}}");
                    sb.AppendLine("        >");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"          Create {className}");
                    sb.AppendLine("        </Button>");
                }

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

                sb.AppendLine("      <Box sx={{ flex: 1, minHeight: 0 }}>");
                sb.AppendLine("        <DataGrid");
                sb.AppendLine("          apiRef={apiRef}");
                sb.AppendLine("          rows={filteredData || []}");
                sb.AppendLine("          columns={columns}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          getRowId={{(row) => row.{pkCamelName}}}");
                sb.AppendLine("          slots={{");
                sb.AppendLine("            toolbar: CustomToolbar,");
                sb.AppendLine("          }}");
                sb.AppendLine("          initialState={{");
                sb.AppendLine("            pagination: { paginationModel: { pageSize: 10 } },");
                sb.AppendLine("          }}");
                sb.AppendLine("          pageSizeOptions={[5, 10, 25, 100]}");
                sb.AppendLine("          checkboxSelection");
                sb.AppendLine("          disableRowSelectionOnClick");
                sb.AppendLine("        />");
                sb.AppendLine("      </Box>");
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
            var sb = new StringBuilder();
            sb.AppendLine("      <Paper sx={{ p: 2, mb: 2 }}>");
            sb.AppendLine("        <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>");

            // Add a text field for each column
            var dataColumns = GetDataColumns(table).Take(10).ToList();
            foreach (var (propertyName, displayName) in from column in dataColumns
                                                        let propertyName = ToCamelCase(GetPropertyName(column.Name))
                                                        let displayName = GetPropertyName(column.Name)
                                                        select (propertyName, displayName))
            {
                sb.AppendLine("          <TextField");
                sb.AppendLine(CultureInfo.InvariantCulture, $"            label=\"{displayName}\"");
                sb.AppendLine("            size=\"small\"");
                sb.AppendLine("            sx={{ minWidth: 150 }}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"            value={{columnFilters['{propertyName}'] || ''}}");
                sb.AppendLine("            onChange={(e) => setColumnFilters(prev => ({");
                sb.AppendLine("              ...prev,");
                sb.AppendLine(CultureInfo.InvariantCulture, $"              {propertyName}: e.target.value");
                sb.AppendLine("            }))}");
                sb.AppendLine("          />");
            }

            // Clear button
            sb.AppendLine("          <Button");
            sb.AppendLine("            variant=\"outlined\"");
            sb.AppendLine("            size=\"small\"");
            sb.AppendLine("            onClick={handleClearAllFilters}");
            sb.AppendLine("          >");
            sb.AppendLine("            Clear");
            sb.AppendLine("          </Button>");

            // Add row count display
            var pluralName = ToCamelCase(GetClassName(table.Name)) + "s";
            sb.AppendLine("          <Box sx={{ ml: 'auto', display: 'flex', alignItems: 'center' }}>");
            sb.AppendLine("            <Typography variant=\"body2\" color=\"text.secondary\">");
            sb.AppendLine(CultureInfo.InvariantCulture, $"              Showing {{filteredData?.length || 0}} of {{{pluralName}?.length || 0}} rows");
            sb.AppendLine("            </Typography>");
            sb.AppendLine("          </Box>");

            sb.AppendLine("        </Box>");
            sb.AppendLine("      </Paper>");

            return sb.ToString();
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
            sb.AppendLine(GenerateImports(table, className, config.Framework));
            sb.AppendLine();

            // Component
            sb.AppendLine(GenerateComponentBody(table, className, camelName, config.Framework));

            return sb.ToString();
        }
    }
}
