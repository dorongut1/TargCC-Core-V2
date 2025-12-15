// <copyright file="ReactDetailComponentGenerator.cs" company="PlaceholderCompany">
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
    /// Generates React Detail components for read-only entity display.
    /// </summary>
    public class ReactDetailComponentGenerator : BaseComponentGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReactDetailComponentGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public ReactDetailComponentGenerator(ILogger<ReactDetailComponentGenerator> logger)
            : base(logger)
        {
        }

        /// <inheritdoc/>
        public override ComponentType ComponentType => ComponentType.Detail;

        /// <inheritdoc/>
        public override async Task<string> GenerateAsync(Table table, DatabaseSchema schema, ComponentGeneratorConfig config)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(config);

            LogComponentGeneration(table.Name);

            return await Task.Run(() => Generate(table, schema, config)).ConfigureAwait(false);
        }

        private static string GenerateImports(Table table, DatabaseSchema schema, string className, UIFramework framework)
        {
            var sb = new StringBuilder();

            sb.AppendLine("import React from 'react';");

            // VIEWs don't use useParams
            if (table.IsView)
            {
                // VIEWs only show an info message, no navigation needed
            }
            else
            {
                sb.AppendLine("import { useNavigate, useParams } from 'react-router-dom';");
            }

            GenerateFrameworkImports(sb, table, schema, framework);

            // VIEWs are read-only - no getById or delete hooks
            if (!table.IsView)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"import {{ use{className}, useDelete{className} }} from '../../hooks/use{className}';");
                GenerateRelatedDataImports(sb, table, schema, className);

                // Entity type imports removed - TypeScript infers all types from hooks
            }

            return sb.ToString();
        }

        private static void GenerateFrameworkImports(StringBuilder sb, Table table, DatabaseSchema schema, UIFramework framework)
        {
            if (framework == UIFramework.MaterialUI)
            {
                // VIEWs only need Box and Alert for the info message
                if (table.IsView)
                {
                    sb.AppendLine("import { Box, Alert } from '@mui/material';");
                }
                else
                {
                    sb.AppendLine("import { Box, Typography, Button, CircularProgress, Alert, Card as MuiCard, CardContent, Grid } from '@mui/material';");
                    sb.AppendLine("import { Edit as EditIcon, Delete as DeleteIcon, ArrowBack as ArrowBackIcon } from '@mui/icons-material';");

                    var hasRelatedData = schema.Relationships != null &&
                        schema.Relationships.Exists(r => r.ParentTable == table.FullName && r.IsEnabled);

                    if (hasRelatedData)
                    {
                        sb.AppendLine("import { DataGrid } from '@mui/x-data-grid';");
                    }
                }
            }
        }

        private static void GenerateRelatedDataImports(StringBuilder sb, Table table, DatabaseSchema schema, string className)
        {
            if (schema.Relationships == null)
            {
                return;
            }

            var parentRelationships = schema.Relationships
                .Where(r => r.ParentTable == table.FullName && r.IsEnabled)
                .ToList();

            // Use HashSet to avoid duplicate hook imports when multiple relationships point to same table
            var importedHooks = new HashSet<string>();

            foreach (var relationship in parentRelationships)
            {
                var childTable = schema.Tables.Find(t => t.FullName == relationship.ChildTable);
                if (childTable != null)
                {
                    var childClassName = GetClassName(childTable.Name);
                    var childrenName = Pluralize(childClassName);
                    var hookName = $"use{className}{childrenName}";

                    if (importedHooks.Add(hookName))
                    {
                        sb.AppendLine(CultureInfo.InvariantCulture, $"import {{ {hookName} }} from '../../hooks/use{className}';");
                    }
                }
            }
        }

        private static string Pluralize(string singular)
        {
            if (string.IsNullOrEmpty(singular))
            {
                return singular;
            }

            // CA1867: String literals required here because char overload doesn't support StringComparison
            #pragma warning disable CA1867

            // Category → Categories
            if (singular.EndsWith("y", StringComparison.OrdinalIgnoreCase) &&
                !singular.EndsWith("ay", StringComparison.OrdinalIgnoreCase) &&
                !singular.EndsWith("ey", StringComparison.OrdinalIgnoreCase) &&
                !singular.EndsWith("oy", StringComparison.OrdinalIgnoreCase) &&
                !singular.EndsWith("uy", StringComparison.OrdinalIgnoreCase))
            {
                return singular[..^1] + "ies";
            }

            // CA1867: String literals required here because char overload doesn't support StringComparison
            #pragma warning disable CA1867

            // Address → Addresses, Box → Boxes
            if (singular.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
                singular.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
                singular.EndsWith("z", StringComparison.OrdinalIgnoreCase) ||
                singular.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
                singular.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
            {
                return singular + "es";
            }

            // Order → Orders, Customer → Customers
            return singular + "s";
        }

        private static string GenerateDetailField(Column column, UIFramework framework)
        {
            var sb = new StringBuilder();
            var propertyName = GetPropertyName(column.Name);
            var fieldName = ToCamelCase(propertyName);
            var (prefix, baseName) = SplitPrefix(column.Name);

            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("        <Grid item xs={12} sm={6}>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          <Typography variant=\"subtitle2\" color=\"text.secondary\">");
                sb.AppendLine(CultureInfo.InvariantCulture, $"            {propertyName}");
                sb.AppendLine("          </Typography>");
                sb.AppendLine("          <Typography variant=\"body1\">");

                var valueExpression = GenerateValueExpression(column, fieldName, prefix, baseName);
                sb.AppendLine(CultureInfo.InvariantCulture, $"            {{{valueExpression}}}");

                sb.AppendLine("          </Typography>");
                sb.AppendLine("        </Grid>");
            }
            else
            {
                var valueExpression = GenerateValueExpression(column, fieldName, prefix, baseName);
                sb.AppendLine(CultureInfo.InvariantCulture, $"      <div><strong>{propertyName}:</strong> {{{valueExpression}}}</div>");
            }

            return sb.ToString();
        }

        private static string GenerateValueExpression(Column column, string fieldName, string prefix, string baseName)
        {
            return prefix switch
            {
                "ENO" => "'********'", // Hide passwords
                "LKP" => $"entity.{ToCamelCase(baseName)}Text || entity.{ToCamelCase(baseName)}Code",
                "LOC" => $"entity.{fieldName}Localized || entity.{fieldName}",
                "ENM" => $"entity.{fieldName}",
                "SPL" => $"entity.{fieldName}?.join(', ')",
                "UPL" => $"entity.{fieldName}",
                "CLC" => FormatValueForDisplay(column, $"entity.{fieldName}"),
                "BLG" => FormatValueForDisplay(column, $"entity.{fieldName}"),
                "AGG" => FormatValueForDisplay(column, $"entity.{fieldName}"),
                _ => FormatValueForDisplay(column, $"entity.{fieldName}"),
            };
        }

        private static string GenerateDetailFields(Table table, UIFramework framework)
        {
            var sb = new StringBuilder();
            var dataColumns = GetDataColumns(table).ToList();

            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("      <Grid container spacing={2}>");
                foreach (var column in dataColumns)
                {
                    sb.AppendLine(GenerateDetailField(column, framework));
                }

                sb.AppendLine("      </Grid>");
            }
            else
            {
                foreach (var column in dataColumns)
                {
                    sb.AppendLine(GenerateDetailField(column, framework));
                }
            }

            return sb.ToString();
        }

        private static void GenerateRelatedDataHooksDeclarations(StringBuilder sb, Table table, DatabaseSchema schema, string className)
        {
            if (schema.Relationships == null)
            {
                return;
            }

            var parentRelationships = schema.Relationships
                .Where(r => r.ParentTable == table.FullName && r.IsEnabled)
                .ToList();

            // Use HashSet to avoid duplicate hook declarations when multiple relationships point to same table
            var declaredHooks = new HashSet<string>();

            foreach (var relationship in parentRelationships)
            {
                var childTable = schema.Tables.Find(t => t.FullName == relationship.ChildTable);
                if (childTable != null)
                {
                    var childClassName = GetClassName(childTable.Name);
                    var childrenName = Pluralize(childClassName);
                    var hookName = $"use{className}{childrenName}";

                    if (declaredHooks.Add(hookName))
                    {
                        var childrenCamelCase = ToCamelCase(childrenName);
                        sb.AppendLine(
                            CultureInfo.InvariantCulture,
                            $"  const {{ data: {childrenCamelCase}, isLoading: {childrenCamelCase}Loading }} = {hookName}(id ? parseInt(id, 10) : null);");
                    }
                }
            }
        }

        private static void GenerateDeleteHandler(StringBuilder sb, string camelName)
        {
            sb.AppendLine("  const handleDelete = () => {");
            sb.AppendLine("    if (confirm('Are you sure you want to delete this item?')) {");
            sb.AppendLine("      deleteEntity(parseInt(id!, 10), {");
            sb.AppendLine(CultureInfo.InvariantCulture, $"        onSuccess: () => navigate('/{camelName}s'),");
            sb.AppendLine("      });");
            sb.AppendLine("    }");
            sb.AppendLine("  };");
        }

        private static void GenerateLoadingState(StringBuilder sb, UIFramework framework)
        {
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
        }

        private static void GenerateErrorState(StringBuilder sb, UIFramework framework, string camelName)
        {
            sb.AppendLine("  if (error || !entity) {");
            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"    return <Alert severity=\"error\">Failed to load {camelName}</Alert>;");
            }
            else
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"    return <div>Failed to load {camelName}</div>;");
            }

            sb.AppendLine("  }");
        }

        private static string GenerateComponentBody(Table table, DatabaseSchema schema, string className, string camelName, UIFramework framework)
        {
            var sb = new StringBuilder();

            sb.AppendLine(CultureInfo.InvariantCulture, $"export const {className}Detail: React.FC = () => {{");

            // VIEWs don't need navigate or id
            if (!table.IsView)
            {
                sb.AppendLine("  const navigate = useNavigate();");
                sb.AppendLine("  const { id } = useParams<{ id: string }>();");
            }

            GenerateHooksSection(sb, table, schema, className, camelName);

            // VIEWs don't need loading/error states
            if (!table.IsView)
            {
                sb.AppendLine();
                GenerateLoadingState(sb, framework);
                sb.AppendLine();
                GenerateErrorState(sb, framework, camelName);
                sb.AppendLine();
            }

            // Render
            sb.AppendLine("  return (");
            GenerateRenderSection(sb, table, schema, className, camelName, framework);
            sb.AppendLine("  );");
            sb.AppendLine("};");

            return sb.ToString();
        }

        private static void GenerateHooksSection(StringBuilder sb, Table table, DatabaseSchema schema, string className, string camelName)
        {
            // VIEWs don't have getById hook - they're read-only and Detail page is not applicable
            if (!table.IsView)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"  const {{ data: entity, isLoading, error }} = use{className}(id ? parseInt(id, 10) : null);");
                sb.AppendLine(CultureInfo.InvariantCulture, $"  const {{ mutate: deleteEntity }} = useDelete{className}();");

                GenerateRelatedDataHooksDeclarations(sb, table, schema, className);
                sb.AppendLine();
                GenerateDeleteHandler(sb, camelName);
            }

            // VIEWs don't need any hooks - they just show an info message
        }

        private static void GenerateRenderSection(StringBuilder sb, Table table, DatabaseSchema schema, string className, string camelName, UIFramework framework)
        {
            // VIEWs are read-only - Detail page not applicable, just show message
            if (table.IsView)
            {
                if (framework == UIFramework.MaterialUI)
                {
                    sb.AppendLine("    <Box sx={{ maxWidth: 800 }}>");
                    sb.AppendLine("      <Alert severity=\"info\">");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"        {className} is a VIEW (read-only). Detail view is not applicable.");
                    sb.AppendLine("        Please use the List view to see the data.");
                    sb.AppendLine("      </Alert>");
                    sb.AppendLine("    </Box>");
                }
                else
                {
                    sb.AppendLine("    <div>");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"      <p>{className} is a VIEW (read-only). Detail view is not applicable.</p>");
                    sb.AppendLine("    </div>");
                }

                return;
            }

            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("    <Box sx={{ maxWidth: 800 }}>");
                GenerateActionButtons(sb, table, camelName);
                sb.AppendLine();
                GenerateDetailCard(sb, table, className, framework);
                GenerateRelatedDataGridsSection(sb, table, schema);
                sb.AppendLine("    </Box>");
            }
            else
            {
                sb.AppendLine("    <div>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"      <h2>{className} Details</h2>");
                sb.AppendLine(GenerateDetailFields(table, framework));
                sb.AppendLine("      <button onClick={handleDelete}>Delete</button>");
                sb.AppendLine("    </div>");
            }
        }

        private static void GenerateActionButtons(StringBuilder sb, Table table, string camelName)
        {
            sb.AppendLine("      <Box sx={{ mb: 2, display: 'flex', gap: 2 }}>");
            sb.AppendLine("        <Button");
            sb.AppendLine("          variant=\"outlined\"");
            sb.AppendLine("          startIcon={<ArrowBackIcon />}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"          onClick={{() => navigate('/{camelName}s')}}");
            sb.AppendLine("        >");
            sb.AppendLine("          Back");
            sb.AppendLine("        </Button>");

            // Only show Edit/Delete buttons for tables, not for views (views are read-only)
            if (!table.IsView)
            {
                sb.AppendLine("        <Button");
                sb.AppendLine("          variant=\"contained\"");
                sb.AppendLine("          startIcon={<EditIcon />}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          onClick={{() => navigate(`/{camelName}s/${{id}}/edit`)}}");
                sb.AppendLine("        >");
                sb.AppendLine("          Edit");
                sb.AppendLine("        </Button>");
                sb.AppendLine("        <Button");
                sb.AppendLine("          variant=\"outlined\"");
                sb.AppendLine("          color=\"error\"");
                sb.AppendLine("          startIcon={<DeleteIcon />}");
                sb.AppendLine("          onClick={handleDelete}");
                sb.AppendLine("        >");
                sb.AppendLine("          Delete");
                sb.AppendLine("        </Button>");
            }

            sb.AppendLine("      </Box>");
        }

        private static void GenerateDetailCard(StringBuilder sb, Table table, string className, UIFramework framework)
        {
            sb.AppendLine("      <MuiCard>");
            sb.AppendLine("        <CardContent>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"          <Typography variant=\"h5\" component=\"h2\" gutterBottom>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            {className} Details");
            sb.AppendLine("          </Typography>");
            sb.AppendLine(GenerateDetailFields(table, framework));
            sb.AppendLine("        </CardContent>");
            sb.AppendLine("      </MuiCard>");
        }

        private static void GenerateRelatedDataGridsSection(StringBuilder sb, Table table, DatabaseSchema schema)
        {
            // Add related data grids (Master-Detail Views)
            if (schema.Relationships != null)
            {
                var parentRelationships = schema.Relationships
                    .Where(r => r.ParentTable == table.FullName && r.IsEnabled)
                    .ToList();

                // Use HashSet to avoid duplicate grids when multiple relationships point to same table
                var generatedGrids = new HashSet<string>();

                foreach (var relationship in parentRelationships)
                {
                    var childTable = schema.Tables.Find(t => t.FullName == relationship.ChildTable);
                    if (childTable != null && generatedGrids.Add(childTable.FullName))
                    {
                        sb.AppendLine();
                        var relatedGrid = GenerateRelatedDataGrid(childTable, UIFramework.MaterialUI);
                        sb.Append(relatedGrid);
                    }
                }
            }
        }

        private static string GenerateRelatedDataGrid(Table childTable, UIFramework framework)
        {
            var sb = new StringBuilder();
            var childClassName = GetClassName(childTable.Name);
            var childrenName = Pluralize(childClassName);
            var childrenCamelCase = ToCamelCase(childrenName);

            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("      <MuiCard sx={{ mt: 2 }}>");
                sb.AppendLine("        <CardContent>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          <Typography variant=\"h6\" gutterBottom>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"            {childrenName}");
                sb.AppendLine("          </Typography>");

                // Column definitions
                sb.AppendLine("          <Box sx={{ height: 400, width: '100%' }}>");
                sb.AppendLine("            <DataGrid");
                sb.AppendLine(CultureInfo.InvariantCulture, $"              rows={{{childrenCamelCase} || []}}");
                sb.AppendLine("              columns={[");

                // Generate columns for visible fields (first 5 columns excluding ENO)
                var displayColumns = GetDataColumns(childTable)
                    .Where(c => !c.Name.StartsWith("eno_", StringComparison.OrdinalIgnoreCase))
                    .Take(5)
                    .ToList();

                foreach (var column in displayColumns)
                {
                    var propertyName = GetPropertyName(column.Name);
                    var fieldName = ToCamelCase(propertyName);
                    var width = column.DataType.Contains("INT", StringComparison.InvariantCultureIgnoreCase) ? 100 : 150;

                    sb.AppendLine(
                        CultureInfo.InvariantCulture,
                        $"                {{ field: '{fieldName}', headerName: '{propertyName}', width: {width} }},");
                }

                sb.AppendLine("              ]}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"              loading={{{childrenCamelCase}Loading}}");
                sb.AppendLine("              pageSizeOptions={[5, 10, 25]}");
                sb.AppendLine("              initialState={{");
                sb.AppendLine("                pagination: { paginationModel: { pageSize: 5 } },");
                sb.AppendLine("              }}");
                sb.AppendLine("            />");
                sb.AppendLine("          </Box>");
                sb.AppendLine("        </CardContent>");
                sb.AppendLine("      </MuiCard>");
            }

            return sb.ToString();
        }

        private static string Generate(Table table, DatabaseSchema schema, ComponentGeneratorConfig config)
        {
            var sb = new StringBuilder();
            var className = GetClassName(table.Name);
            var camelName = GetCamelCaseName(table.Name);

            // Header
            sb.Append(GenerateComponentHeader(table.Name));

            // Imports
            sb.AppendLine(GenerateImports(table, schema, className, config.Framework));
            sb.AppendLine();

            // Component
            sb.AppendLine(GenerateComponentBody(table, schema, className, camelName, config.Framework));

            return sb.ToString();
        }
    }
}
