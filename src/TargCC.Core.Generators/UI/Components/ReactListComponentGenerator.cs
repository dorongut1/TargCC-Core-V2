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
            sb.AppendLine("import ExcelJS from 'exceljs';");

            if (framework == UIFramework.MaterialUI)
            {
                // GridActionsCellItem is only used for tables with edit/delete actions
                if (!table.IsView)
                {
                    sb.AppendLine("import { DataGrid, GridColDef, GridActionsCellItem, GridToolbarContainer, GridToolbarColumnsButton, GridToolbarDensitySelector, useGridApiRef } from '@mui/x-data-grid';");
                }
                else
                {
                    sb.AppendLine("import { DataGrid, GridColDef, GridToolbarContainer, GridToolbarColumnsButton, GridToolbarDensitySelector, useGridApiRef } from '@mui/x-data-grid';");
                }

                sb.AppendLine("import { Button, Box, CircularProgress, Alert, TextField, Paper, Typography } from '@mui/material';");

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
                    sb.Append(CultureInfo.InvariantCulture, $", valueFormatter: (value) => formatDate(value)");
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
            sb.AppendLine("  ");

            // Only define formatDate if there are date columns
            var hasDateColumns = table.Columns.Exists(c =>
                c.DataType.ToUpperInvariant().Contains("DATE", StringComparison.Ordinal));

            if (hasDateColumns)
            {
                sb.AppendLine("  // Helper function to format dates as DD/MM/YYYY");
                sb.AppendLine("  const formatDate = (date: any): string => {");
                sb.AppendLine("    if (!date) return '';");
                sb.AppendLine("    const d = new Date(date);");
                sb.AppendLine("    if (isNaN(d.getTime())) return '';");
                sb.AppendLine("    const day = d.getDate().toString().padStart(2, '0');");
                sb.AppendLine("    const month = (d.getMonth() + 1).toString().padStart(2, '0');");
                sb.AppendLine("    const year = d.getFullYear();");
                sb.AppendLine("    return `${day}/${month}/${year}`;");
                sb.AppendLine("  };");
                sb.AppendLine("  ");
            }

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
            sb.AppendLine("        // Cast to any to avoid TypeScript union type issues with instanceof");
            sb.AppendLine("        const itemValue = item[column as keyof typeof item] as any;");
            sb.AppendLine("        ");
            sb.AppendLine("        // Special handling for date filtering - compare as Date objects");
            sb.AppendLine("        if (typeof filterValue === 'string' && /^\\d{4}-\\d{2}-\\d{2}$/.test(filterValue)) {");
            sb.AppendLine("          // Filter value is from date picker (YYYY-MM-DD)");
            sb.AppendLine("          const filterDate = new Date(filterValue);");
            sb.AppendLine("          let itemDate: Date | null = null;");
            sb.AppendLine("          ");
            sb.AppendLine("          if (itemValue instanceof Date) {");
            sb.AppendLine("            itemDate = itemValue;");
            sb.AppendLine("          } else if (typeof itemValue === 'string' && /^\\d{4}-\\d{2}-\\d{2}/.test(itemValue)) {");
            sb.AppendLine("            itemDate = new Date(itemValue);");
            sb.AppendLine("          }");
            sb.AppendLine("          ");
            sb.AppendLine("          if (itemDate) {");
            sb.AppendLine("            // Compare dates (ignore time) - same year, month, and day");
            sb.AppendLine("            return itemDate.getFullYear() === filterDate.getFullYear() &&");
            sb.AppendLine("                   itemDate.getMonth() === filterDate.getMonth() &&");
            sb.AppendLine("                   itemDate.getDate() === filterDate.getDate();");
            sb.AppendLine("          }");
            sb.AppendLine("          return false;");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            sb.AppendLine("        // Convert boolean values to readable text");
            sb.AppendLine("        if (typeof itemValue === 'boolean') {");
            sb.AppendLine("          itemValue = itemValue ? 'true' : 'false';");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
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
            sb.AppendLine("  const handleExportToExcel = async () => {");
            sb.AppendLine("    // Export only the filtered data visible on screen");
            sb.AppendLine("    const visibleRows = filteredData || [];");
            sb.AppendLine();
            sb.AppendLine("    if (!visibleRows || visibleRows.length === 0) {");
            sb.AppendLine("      alert('No data to export');");
            sb.AppendLine("      return;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    // Get visible columns from DataGrid (respects column visibility)");
            sb.AppendLine("    const visibleColumns = apiRef.current");
            sb.AppendLine("      ? apiRef.current.getVisibleColumns().map(col => col.field).filter(field => field !== 'actions')");
            sb.AppendLine("      : Object.keys(visibleRows[0] || {});");
            sb.AppendLine();
            sb.AppendLine("    // Filter data to only include visible columns");
            sb.AppendLine("    const exportData = visibleRows.map(row => {");
            sb.AppendLine("      const filteredRow: any = {};");
            sb.AppendLine("      const rowAny = row as any;");
            sb.AppendLine("      visibleColumns.forEach(col => {");
            sb.AppendLine("        if (col in rowAny) filteredRow[col] = rowAny[col];");
            sb.AppendLine("      });");
            sb.AppendLine("      return filteredRow;");
            sb.AppendLine("    });");
            sb.AppendLine();
            sb.AppendLine("    // Create workbook and worksheet");
            sb.AppendLine("    const workbook = new ExcelJS.Workbook();");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    const worksheet = workbook.addWorksheet('{className}s');");
            sb.AppendLine();
            sb.AppendLine("    // Get column names from first row (only visible columns)");
            sb.AppendLine("    const columnNames = exportData.length > 0 ? Object.keys(exportData[0]) : [];");
            sb.AppendLine("    worksheet.columns = columnNames.map(name => ({ header: name, key: name, width: 15 }));");
            sb.AppendLine();
            sb.AppendLine("    // Add data rows");
            sb.AppendLine("    exportData.forEach(row => worksheet.addRow(row));");
            sb.AppendLine();
            sb.AppendLine("    // Style header row");
            sb.AppendLine("    const headerRow = worksheet.getRow(1);");
            sb.AppendLine("    headerRow.font = { bold: true, color: { argb: 'FFFFFFFF' } };");
            sb.AppendLine("    headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FF4472C4' } };");
            sb.AppendLine("    headerRow.alignment = { horizontal: 'center', vertical: 'middle' };");
            sb.AppendLine("    headerRow.height = 20;");
            sb.AppendLine();
            sb.AppendLine("    // Auto-fit columns based on content");
            sb.AppendLine("    worksheet.columns.forEach(column => {");
            sb.AppendLine("      let maxLength = column.header?.length || 10;");
            sb.AppendLine("      column.eachCell?.({ includeEmpty: false }, cell => {");
            sb.AppendLine("        const cellLength = cell.value ? String(cell.value).length : 0;");
            sb.AppendLine("        maxLength = Math.max(maxLength, cellLength);");
            sb.AppendLine("      });");
            sb.AppendLine("      column.width = Math.min(maxLength + 2, 50);");
            sb.AppendLine("    });");
            sb.AppendLine();
            sb.AppendLine("    // Freeze header row");
            sb.AppendLine("    worksheet.views = [{ state: 'frozen', ySplit: 1 }];");
            sb.AppendLine();
            sb.AppendLine("    // Generate Excel file and download");
            sb.AppendLine("    const buffer = await workbook.xlsx.writeBuffer();");
            sb.AppendLine("    const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });");
            sb.AppendLine("    const url = window.URL.createObjectURL(blob);");
            sb.AppendLine("    const link = document.createElement('a');");
            sb.AppendLine("    link.href = url;");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    link.download = `{className}s_${{new Date().toISOString().split('T')[0]}}.xlsx`;");
            sb.AppendLine("    link.click();");
            sb.AppendLine("    window.URL.revokeObjectURL(url);");
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

            // Add a filter field for each column (text, date, or dropdown)
            var dataColumns = GetDataColumns(table).Take(10).ToList();
            foreach (var column in dataColumns)
            {
                var propertyName = ToCamelCase(GetPropertyName(column.Name));
                var displayName = GetPropertyName(column.Name);
                var (prefix, _) = SplitPrefix(column.Name);
                var columnNameUpper = column.Name.ToUpperInvariant();
                var dataTypeUpper = column.DataType.ToUpperInvariant();

                // Detect column types for smart filtering
                var isDateColumn = dataTypeUpper.Contains("DATE", StringComparison.Ordinal);
                var isBooleanColumn = dataTypeUpper.Contains("BIT", StringComparison.Ordinal) ||
                                     dataTypeUpper.Contains("BOOL", StringComparison.Ordinal);
                var isEnumColumn = prefix == "LKP" ||
                                  columnNameUpper.EndsWith("STATUS", StringComparison.Ordinal) ||
                                  columnNameUpper.EndsWith("TYPE", StringComparison.Ordinal) ||
                                  columnNameUpper.EndsWith("CATEGORY", StringComparison.Ordinal) ||
                                  dataTypeUpper.Contains("ENUM", StringComparison.Ordinal);

                if (isBooleanColumn)
                {
                    // Dropdown for Boolean columns
                    sb.AppendLine("          <TextField");
                    sb.AppendLine("            select");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            label=\"{displayName}\"");
                    sb.AppendLine("            size=\"small\"");
                    sb.AppendLine("            sx={{ minWidth: 120 }}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            value={{columnFilters['{propertyName}'] || ''}}");
                    sb.AppendLine("            onChange={(e) => setColumnFilters(prev => ({");
                    sb.AppendLine("              ...prev,");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"              {propertyName}: e.target.value");
                    sb.AppendLine("            }))}");
                    sb.AppendLine("          >");
                    sb.AppendLine("            <MenuItem value=\"\">All</MenuItem>");
                    sb.AppendLine("            <MenuItem value=\"true\">Yes</MenuItem>");
                    sb.AppendLine("            <MenuItem value=\"false\">No</MenuItem>");
                    sb.AppendLine("          </TextField>");
                }
                else if (isDateColumn)
                {
                    // Date picker for DATE columns
                    sb.AppendLine("          <TextField");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            label=\"{displayName}\"");
                    sb.AppendLine("            type=\"date\"");
                    sb.AppendLine("            size=\"small\"");
                    sb.AppendLine("            sx={{ minWidth: 180 }}");
                    sb.AppendLine("            InputLabelProps={{ shrink: true }}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            value={{columnFilters['{propertyName}'] || ''}}");
                    sb.AppendLine("            onChange={(e) => setColumnFilters(prev => ({");
                    sb.AppendLine("              ...prev,");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"              {propertyName}: e.target.value");
                    sb.AppendLine("            }))}");
                    sb.AppendLine("          />");
                }
                else if (isEnumColumn)
                {
                    // Dropdown for ENUM/Status/Type columns
                    sb.AppendLine("          <TextField");
                    sb.AppendLine("            select");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            label=\"{displayName}\"");
                    sb.AppendLine("            size=\"small\"");
                    sb.AppendLine("            sx={{ minWidth: 150 }}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            value={{columnFilters['{propertyName}'] || ''}}");
                    sb.AppendLine("            onChange={(e) => setColumnFilters(prev => ({");
                    sb.AppendLine("              ...prev,");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"              {propertyName}: e.target.value");
                    sb.AppendLine("            }))}");
                    sb.AppendLine("          >");
                    sb.AppendLine("            <MenuItem value=\"\">All</MenuItem>");
                    sb.AppendLine("            <MenuItem value=\"Active\">Active</MenuItem>");
                    sb.AppendLine("            <MenuItem value=\"Inactive\">Inactive</MenuItem>");
                    sb.AppendLine("            <MenuItem value=\"Pending\">Pending</MenuItem>");
                    sb.AppendLine("          </TextField>");
                }
                else
                {
                    // Regular text field
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
