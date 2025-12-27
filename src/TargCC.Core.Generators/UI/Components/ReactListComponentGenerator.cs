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

            // Only import useNavigate for tables (edit/delete actions), not for read-only VIEWs
            if (!table.IsView)
            {
                sb.AppendLine("import { useNavigate, useSearchParams } from 'react-router-dom';");
            }
            else
            {
                sb.AppendLine("import { useSearchParams } from 'react-router-dom';");
            }

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

                // Check if MenuItem is needed for boolean/enum filters
                var dataColumns = GetDataColumns(table).Take(10).ToList();
                var needsMenuItem = dataColumns.Exists(c =>
                {
                    var dataTypeUpper = c.DataType.ToUpperInvariant();
                    var (prefix, _) = SplitPrefix(c.Name);
                    var columnNameUpper = c.Name.ToUpperInvariant();

                    return dataTypeUpper.Contains("BIT", StringComparison.Ordinal) ||
                           dataTypeUpper.Contains("BOOL", StringComparison.Ordinal) ||
                           prefix == "LKP" ||
                           columnNameUpper.EndsWith("STATUS", StringComparison.Ordinal) ||
                           columnNameUpper.EndsWith("TYPE", StringComparison.Ordinal) ||
                           columnNameUpper.EndsWith("CATEGORY", StringComparison.Ordinal) ||
                           dataTypeUpper.Contains("ENUM", StringComparison.Ordinal);
                });

                if (needsMenuItem)
                {
                    sb.AppendLine("import { Button, Box, CircularProgress, Alert, TextField, Paper, MenuItem, Typography } from '@mui/material';");
                }
                else
                {
                    sb.AppendLine("import { Button, Box, CircularProgress, Alert, TextField, Paper, Typography } from '@mui/material';");
                }

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

            // Only import the entity type, not Filters (Filters type is never used)
            sb.AppendLine(CultureInfo.InvariantCulture, $"import type {{ {className} }} from '../../types/{className}.types';");

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
            var pkCamelName = GetPrimaryKeyPropertyName(table);

            sb.AppendLine(CultureInfo.InvariantCulture, $"export const {className}List: React.FC = () => {{");
            sb.Append(GenerateComponentSetup(table, className, pluralName));
            sb.Append(GenerateFilterLogic());
            sb.Append(GenerateHandlersAndToolbar());
            sb.Append(GenerateExportToExcelHandler(className));
            sb.AppendLine(GenerateColumns(table));
            sb.AppendLine();
            sb.Append(GenerateLoadingAndErrorStates(framework, pluralName));
            sb.Append(GenerateMainRender(table, framework, className, pluralName, pkCamelName));
            sb.AppendLine("  );");
            sb.AppendLine("};");

            return sb.ToString();
        }

        private static string GetPrimaryKeyPropertyName(Table table)
        {
            var pkColumn = table.Columns.Find(c => c.IsPrimaryKey &&
                (c.DataType.Contains("INT", StringComparison.OrdinalIgnoreCase) ||
                 c.DataType.Contains("CHAR", StringComparison.OrdinalIgnoreCase) ||
                 c.DataType.Contains("GUID", StringComparison.OrdinalIgnoreCase)));

            if (pkColumn == null)
            {
                pkColumn = table.Columns.Find(c =>
                    c.Name.Equals("id", StringComparison.OrdinalIgnoreCase) ||
                    c.Name.EndsWith("_id", StringComparison.OrdinalIgnoreCase) ||
                    c.Name.EndsWith("ID", StringComparison.OrdinalIgnoreCase));
            }

            var pkPropertyName = pkColumn != null ? GetPropertyName(pkColumn.Name) : "id";
            return pkPropertyName.Length > 0 ? char.ToLowerInvariant(pkPropertyName[0]) + pkPropertyName.Substring(1) : "id";
        }

        private static string GenerateComponentSetup(Table table, string className, string pluralName)
        {
            var sb = new StringBuilder();

            // Only declare navigate for tables (edit/delete actions), not for read-only VIEWs
            if (!table.IsView)
            {
                sb.AppendLine("  const navigate = useNavigate();");
            }

            sb.AppendLine("  const [searchParams, setSearchParams] = useSearchParams();");
            sb.AppendLine("  const apiRef = useGridApiRef();");
            sb.AppendLine("  ");
            sb.AppendLine("  // Local state for filter inputs (debounced before URL update)");
            sb.AppendLine("  const [localFilters, setLocalFilters] = React.useState<Record<string, string>>({});");
            sb.AppendLine("  const debounceTimerRef = React.useRef<NodeJS.Timeout | null>(null);");
            sb.AppendLine("  ");

            // Only generate formatDate if the first 10 displayed columns contain DATE fields
            var dataColumns = GetDataColumns(table).Take(10).ToList();
            var hasDateColumns = dataColumns.Exists(c =>
                c.DataType.ToUpperInvariant().Contains("DATE", StringComparison.Ordinal));

            if (hasDateColumns)
            {
                sb.Append(GenerateDateFormatter());
            }

            // Server-side state management from URL params
            sb.AppendLine("  // Extract pagination, sorting, and filters from URL");
            sb.AppendLine("  const page = parseInt(searchParams.get('page') || '1', 10);");
            sb.AppendLine("  const pageSize = parseInt(searchParams.get('pageSize') || '10', 10);");
            sb.AppendLine("  const sortBy = searchParams.get('sortBy') || 'id';");
            sb.AppendLine("  const sortDirection = (searchParams.get('sortDirection') || 'asc') as 'asc' | 'desc';");
            sb.AppendLine();
            sb.AppendLine("  // Extract filters from URL");
            sb.AppendLine("  const filters = React.useMemo(() => {");
            sb.AppendLine("    const filtersObj: Record<string, string> = {};");
            sb.AppendLine("    searchParams.forEach((value, key) => {");
            sb.AppendLine("      if (!['page', 'pageSize', 'sortBy', 'sortDirection'].includes(key)) {");
            sb.AppendLine("        filtersObj[key] = value;");
            sb.AppendLine("      }");
            sb.AppendLine("    });");
            sb.AppendLine("    return filtersObj;");
            sb.AppendLine("  }, [searchParams]);");
            sb.AppendLine();
            sb.AppendLine("  // Sync local filters when URL filters change (e.g., browser back/forward)");
            sb.AppendLine("  React.useEffect(() => {");
            sb.AppendLine("    setLocalFilters(prev => {");
            sb.AppendLine("      // Only update if filters actually changed");
            sb.AppendLine("      const hasChanges = Object.keys(filters).some(key => prev[key] !== filters[key]) ||");
            sb.AppendLine("                         Object.keys(prev).some(key => filters[key] === undefined && prev[key] !== undefined);");
            sb.AppendLine("      return hasChanges ? { ...filters } : prev;");
            sb.AppendLine("    });");
            sb.AppendLine("  }, [filters]);");
            sb.AppendLine();
            sb.AppendLine("  // Fetch data with server-side options");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  const {{ data, isLoading, error }} = use{className}s({{");
            sb.AppendLine("    page,");
            sb.AppendLine("    pageSize,");
            sb.AppendLine("    sortBy,");
            sb.AppendLine("    sortDirection,");
            sb.AppendLine("    filters: Object.keys(filters).length > 0 ? filters : undefined,");
            sb.AppendLine("  });");
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"  const {pluralName} = data?.items || [];");
            sb.AppendLine("  const totalCount = data?.totalCount || 0;");
            sb.AppendLine();

            if (!table.IsView)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"  const {{ mutate: deleteEntity }} = useDelete{className}();");
            }

            sb.AppendLine();
            return sb.ToString();
        }

        private static string GenerateDateFormatter()
        {
            var sb = new StringBuilder();
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
            return sb.ToString();
        }

        private static string GenerateFilterLogic()
        {
            var sb = new StringBuilder();
            sb.AppendLine("  // Debounced filter update - only updates URL after user stops typing");
            sb.AppendLine("  const handleFilterChange = (key: string, value: string) => {");
            sb.AppendLine("    // Update local state immediately for responsive UI");
            sb.AppendLine("    setLocalFilters(prev => ({ ...prev, [key]: value }));");
            sb.AppendLine();
            sb.AppendLine("    // Clear existing debounce timer");
            sb.AppendLine("    if (debounceTimerRef.current) {");
            sb.AppendLine("      clearTimeout(debounceTimerRef.current);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    // Set new debounce timer (500ms delay)");
            sb.AppendLine("    debounceTimerRef.current = setTimeout(() => {");
            sb.AppendLine("      const newParams = new URLSearchParams(searchParams);");
            sb.AppendLine("      if (value && value.trim() !== '') {");
            sb.AppendLine("        newParams.set(key, value);");
            sb.AppendLine("      } else {");
            sb.AppendLine("        newParams.delete(key);");
            sb.AppendLine("      }");
            sb.AppendLine("      // Reset to page 1 when filter changes");
            sb.AppendLine("      newParams.set('page', '1');");
            sb.AppendLine("      setSearchParams(newParams);");
            sb.AppendLine("    }, 500);");
            sb.AppendLine("  };");
            sb.AppendLine();
            sb.AppendLine("  // Cleanup debounce timer on unmount");
            sb.AppendLine("  React.useEffect(() => {");
            sb.AppendLine("    return () => {");
            sb.AppendLine("      if (debounceTimerRef.current) {");
            sb.AppendLine("        clearTimeout(debounceTimerRef.current);");
            sb.AppendLine("      }");
            sb.AppendLine("    };");
            sb.AppendLine("  }, []);");
            sb.AppendLine();
            sb.AppendLine("  const handlePaginationChange = (newPage: number, newPageSize: number) => {");
            sb.AppendLine("    const newParams = new URLSearchParams(searchParams);");
            sb.AppendLine("    newParams.set('page', (newPage + 1).toString()); // MUI DataGrid is 0-based");
            sb.AppendLine("    newParams.set('pageSize', newPageSize.toString());");
            sb.AppendLine("    setSearchParams(newParams);");
            sb.AppendLine("  };");
            sb.AppendLine();
            sb.AppendLine("  const handleSortChange = (field: string, direction: 'asc' | 'desc' | null) => {");
            sb.AppendLine("    const newParams = new URLSearchParams(searchParams);");
            sb.AppendLine("    if (field && direction) {");
            sb.AppendLine("      newParams.set('sortBy', field);");
            sb.AppendLine("      newParams.set('sortDirection', direction);");
            sb.AppendLine("    } else {");
            sb.AppendLine("      newParams.delete('sortBy');");
            sb.AppendLine("      newParams.delete('sortDirection');");
            sb.AppendLine("    }");
            sb.AppendLine("    setSearchParams(newParams);");
            sb.AppendLine("  };");
            sb.AppendLine();
            return sb.ToString();
        }

        private static string GenerateHandlersAndToolbar()
        {
            var sb = new StringBuilder();
            sb.AppendLine("  const handleClearAllFilters = () => {");
            sb.AppendLine("    // Clear local filters");
            sb.AppendLine("    setLocalFilters({});");
            sb.AppendLine("    // Clear debounce timer");
            sb.AppendLine("    if (debounceTimerRef.current) {");
            sb.AppendLine("      clearTimeout(debounceTimerRef.current);");
            sb.AppendLine("    }");
            sb.AppendLine("    // Keep only page and pageSize, remove all filters and sorting");
            sb.AppendLine("    const newParams = new URLSearchParams();");
            sb.AppendLine("    newParams.set('page', '1');");
            sb.AppendLine("    newParams.set('pageSize', pageSize.toString());");
            sb.AppendLine("    setSearchParams(newParams);");
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
            return sb.ToString();
        }

        private static string GenerateExportToExcelHandler(string className)
        {
            var sb = new StringBuilder();
            var pluralName = ToCamelCase(className) + "s";
            sb.AppendLine("  const handleExportToExcel = async () => {");
            sb.AppendLine("    // Export the current page data (server-side filtered)");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    const visibleRows = {pluralName} || [];");
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
            sb.Append(GenerateExcelWorkbookCreation(className));
            sb.AppendLine("  };");
            sb.AppendLine();
            return sb.ToString();
        }

        private static string GenerateExcelWorkbookCreation(string className)
        {
            var sb = new StringBuilder();
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
            sb.Append(GenerateExcelStyling());
            sb.Append(GenerateExcelDownload(className));
            return sb.ToString();
        }

        private static string GenerateExcelStyling()
        {
            var sb = new StringBuilder();
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
            return sb.ToString();
        }

        private static string GenerateExcelDownload(string className)
        {
            var sb = new StringBuilder();
            sb.AppendLine("    // Generate Excel file and download");
            sb.AppendLine("    const buffer = await workbook.xlsx.writeBuffer();");
            sb.AppendLine("    const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });");
            sb.AppendLine("    const url = window.URL.createObjectURL(blob);");
            sb.AppendLine("    const link = document.createElement('a');");
            sb.AppendLine("    link.href = url;");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    link.download = `{className}s_${{new Date().toISOString().split('T')[0]}}.xlsx`;");
            sb.AppendLine("    link.click();");
            sb.AppendLine("    window.URL.revokeObjectURL(url);");
            return sb.ToString();
        }

        private static string GenerateLoadingAndErrorStates(UIFramework framework, string pluralName)
        {
            var sb = new StringBuilder();
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
            return sb.ToString();
        }

        private static string GenerateMainRender(Table table, UIFramework framework, string className, string pluralName, string pkCamelName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("  return (");

            if (framework == UIFramework.MaterialUI)
            {
                sb.Append(GenerateMaterialUIRender(table, className, pluralName, pkCamelName));
            }
            else
            {
                sb.Append(GenerateTailwindRender(className, pluralName));
            }

            return sb.ToString();
        }

        private static string GenerateMaterialUIRender(Table table, string className, string pluralName, string pkCamelName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("    <Box sx={{ height: 'calc(100vh - 200px)', width: '100%', display: 'flex', flexDirection: 'column' }}>");
            sb.Append(GenerateActionButtons(table, className, pluralName));
            sb.AppendLine(GenerateFilterUI(table));
            sb.Append(GenerateDataGrid(pluralName, pkCamelName));
            sb.AppendLine("    </Box>");
            return sb.ToString();
        }

        private static string GenerateActionButtons(Table table, string className, string pluralName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("      <Box sx={{ mb: 2, display: 'flex', gap: 2 }}>");

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
            return sb.ToString();
        }

        private static string GenerateDataGrid(string pluralName, string pkCamelName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("      <Box sx={{ flex: 1, minHeight: 0 }}>");
            sb.AppendLine("        <DataGrid");
            sb.AppendLine("          apiRef={apiRef}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"          rows={{{pluralName} || []}}");
            sb.AppendLine("          columns={columns}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"          getRowId={{(row) => row.{pkCamelName} ?? 0}}");
            sb.AppendLine("          // Server-side pagination");
            sb.AppendLine("          paginationMode=\"server\"");
            sb.AppendLine("          rowCount={totalCount}");
            sb.AppendLine("          paginationModel={{");
            sb.AppendLine("            page: page - 1, // MUI DataGrid is 0-based, our API is 1-based");
            sb.AppendLine("            pageSize: pageSize,");
            sb.AppendLine("          }}");
            sb.AppendLine("          onPaginationModelChange={(model) => handlePaginationChange(model.page, model.pageSize)}");
            sb.AppendLine("          pageSizeOptions={[5, 10, 25, 50, 100]}");
            sb.AppendLine("          // Server-side sorting");
            sb.AppendLine("          sortingMode=\"server\"");
            sb.AppendLine("          sortModel={[{");
            sb.AppendLine("            field: sortBy,");
            sb.AppendLine("            sort: sortDirection,");
            sb.AppendLine("          }]}");
            sb.AppendLine("          onSortModelChange={(model) => {");
            sb.AppendLine("            if (model.length > 0) {");
            sb.AppendLine("              handleSortChange(model[0].field, model[0].sort ?? 'asc');");
            sb.AppendLine("            } else {");
            sb.AppendLine("              handleSortChange('id', 'asc');");
            sb.AppendLine("            }");
            sb.AppendLine("          }}");
            sb.AppendLine("          slots={{");
            sb.AppendLine("            toolbar: CustomToolbar,");
            sb.AppendLine("          }}");
            sb.AppendLine("          checkboxSelection");
            sb.AppendLine("          disableRowSelectionOnClick");
            sb.AppendLine("        />");
            sb.AppendLine("      </Box>");
            return sb.ToString();
        }

        private static string GenerateTailwindRender(string className, string pluralName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("    <div className=\"p-4\">");
            sb.AppendLine("      <div className=\"mb-4\">");
            sb.AppendLine(CultureInfo.InvariantCulture, $"        <button onClick={{() => navigate('/{pluralName}/new')}} className=\"bg-blue-500 text-white px-4 py-2 rounded\">");
            sb.AppendLine(CultureInfo.InvariantCulture, $"          Create {className}");
            sb.AppendLine("        </button>");
            sb.AppendLine("      </div>");
            sb.AppendLine("      {/* Table implementation */}");
            sb.AppendLine("    </div>");
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
                    // Dropdown for Boolean columns - no debounce needed for dropdowns
                    sb.AppendLine("          <TextField");
                    sb.AppendLine("            select");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            label=\"{displayName}\"");
                    sb.AppendLine("            size=\"small\"");
                    sb.AppendLine("            sx={{ minWidth: 120 }}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            value={{localFilters['{propertyName}'] ?? filters['{propertyName}'] ?? ''}}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            onChange={{(e) => handleFilterChange('{propertyName}', e.target.value)}}");
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
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            value={{localFilters['{propertyName}'] ?? filters['{propertyName}'] ?? ''}}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            onChange={{(e) => handleFilterChange('{propertyName}', e.target.value)}}");
                    sb.AppendLine("          />");
                }
                else if (isEnumColumn)
                {
                    // Dropdown for ENUM/Status/Type columns - no debounce needed for dropdowns
                    sb.AppendLine("          <TextField");
                    sb.AppendLine("            select");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            label=\"{displayName}\"");
                    sb.AppendLine("            size=\"small\"");
                    sb.AppendLine("            sx={{ minWidth: 150 }}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            value={{localFilters['{propertyName}'] ?? filters['{propertyName}'] ?? ''}}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            onChange={{(e) => handleFilterChange('{propertyName}', e.target.value)}}");
                    sb.AppendLine("          >");
                    sb.AppendLine("            <MenuItem value=\"\">All</MenuItem>");
                    sb.AppendLine("            <MenuItem value=\"Active\">Active</MenuItem>");
                    sb.AppendLine("            <MenuItem value=\"Inactive\">Inactive</MenuItem>");
                    sb.AppendLine("            <MenuItem value=\"Pending\">Pending</MenuItem>");
                    sb.AppendLine("          </TextField>");
                }
                else
                {
                    // Regular text field with debounce
                    sb.AppendLine("          <TextField");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            label=\"{displayName}\"");
                    sb.AppendLine("            size=\"small\"");
                    sb.AppendLine("            sx={{ minWidth: 150 }}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            value={{localFilters['{propertyName}'] ?? filters['{propertyName}'] ?? ''}}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"            onChange={{(e) => handleFilterChange('{propertyName}', e.target.value)}}");
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

            // Add row count display (server-side)
            var pluralName = ToCamelCase(GetClassName(table.Name)) + "s";
            sb.AppendLine("          <Box sx={{ ml: 'auto', display: 'flex', alignItems: 'center' }}>");
            sb.AppendLine("            <Typography variant=\"body2\" color=\"text.secondary\">");
            sb.AppendLine(CultureInfo.InvariantCulture, $"              Showing {{{pluralName}?.length || 0}} of {{totalCount}} rows");
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
