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

            return await Task.Run(() => Generate(table, config)).ConfigureAwait(false);
        }

        private static string GenerateImports(string className, UIFramework framework)
        {
            var sb = new StringBuilder();

            sb.AppendLine("import React from 'react';");
            sb.AppendLine("import { useNavigate, useParams } from 'react-router-dom';");

            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("import { Box, Typography, Button, CircularProgress, Alert, Card, CardContent, Grid } from '@mui/material';");
                sb.AppendLine("import { Edit as EditIcon, Delete as DeleteIcon, ArrowBack as ArrowBackIcon } from '@mui/icons-material';");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"import {{ use{className}, useDelete{className} }} from '../../hooks/use{className}';");
            sb.AppendLine(CultureInfo.InvariantCulture, $"import type {{ {className} }} from '../../types/{className}.types';");

            return sb.ToString();
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

        private static string GenerateComponentBody(Table table, string className, string camelName, UIFramework framework)
        {
            var sb = new StringBuilder();

            sb.AppendLine(CultureInfo.InvariantCulture, $"export const {className}Detail: React.FC = () => {{");
            sb.AppendLine("  const navigate = useNavigate();");
            sb.AppendLine("  const { id } = useParams<{ id: string }>();");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  const {{ data: entity, isLoading, error }} = use{className}(id ? parseInt(id, 10) : null);");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  const {{ mutate: deleteEntity }} = useDelete{className}();");
            sb.AppendLine();

            // Handle delete
            sb.AppendLine("  const handleDelete = () => {");
            sb.AppendLine("    if (confirm('Are you sure you want to delete this item?')) {");
            sb.AppendLine("      deleteEntity(parseInt(id!, 10), {");
            sb.AppendLine(CultureInfo.InvariantCulture, $"        onSuccess: () => navigate('/{camelName}s'),");
            sb.AppendLine("      });");
            sb.AppendLine("    }");
            sb.AppendLine("  };");
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
            sb.AppendLine();

            // Render
            sb.AppendLine("  return (");
            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("    <Box sx={{ maxWidth: 800 }}>");
                sb.AppendLine("      <Box sx={{ mb: 2, display: 'flex', gap: 2 }}>");
                sb.AppendLine("        <Button");
                sb.AppendLine("          variant=\"outlined\"");
                sb.AppendLine("          startIcon={<ArrowBackIcon />}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          onClick={{() => navigate('/{camelName}s')}}");
                sb.AppendLine("        >");
                sb.AppendLine("          Back");
                sb.AppendLine("        </Button>");
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
                sb.AppendLine("      </Box>");
                sb.AppendLine();
                sb.AppendLine("      <Card>");
                sb.AppendLine("        <CardContent>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          <Typography variant=\"h5\" component=\"h2\" gutterBottom>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"            {className} Details");
                sb.AppendLine("          </Typography>");
                sb.AppendLine(GenerateDetailFields(table, framework));
                sb.AppendLine("        </CardContent>");
                sb.AppendLine("      </Card>");
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

            sb.AppendLine("  );");
            sb.AppendLine("};");

            return sb.ToString();
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
