// <copyright file="ReactFormComponentGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI.Components
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Generates React Form components for Create/Update operations with full validation.
    /// Handles all 12 prefix cases: ENO, LKP, LOC, ENM, SPL, UPL, CLC, BLG, AGG, SCB, SPT, ENT.
    /// </summary>
    public class ReactFormComponentGenerator : BaseComponentGenerator
    {
        private static readonly string[] EnumLookupImports = { "Select", "MenuItem", "FormControl", "InputLabel" };
        private static readonly string[] BooleanFieldImports = { "Checkbox", "FormControlLabel" };

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactFormComponentGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public ReactFormComponentGenerator(ILogger<ReactFormComponentGenerator> logger)
            : base(logger)
        {
        }

        /// <inheritdoc/>
        public override ComponentType ComponentType => ComponentType.Form;

        /// <inheritdoc/>
        public override async Task<string> GenerateAsync(Table table, DatabaseSchema schema, ComponentGeneratorConfig config)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(config);

            LogComponentGeneration(table.Name);

            return await Task.Run(() => Generate(table, config)).ConfigureAwait(false);
        }

        private static string GenerateImports(string className, UIFramework framework, FormValidationLibrary validationLibrary, Table table)
        {
            var sb = new StringBuilder();

            sb.AppendLine("import React, { useEffect } from 'react';");
            sb.AppendLine("import { useNavigate, useParams } from 'react-router-dom';");

            if (validationLibrary == FormValidationLibrary.ReactHookForm)
            {
                sb.AppendLine("import { useForm } from 'react-hook-form';");
            }

            if (framework == UIFramework.MaterialUI)
            {
                // Check if table has ENM or LKP columns that require Select/MenuItem
                var hasEnumOrLookupFields = table.Columns.Exists(c =>
                    c.Name.StartsWith("enm_", StringComparison.OrdinalIgnoreCase) ||
                    c.Name.StartsWith("lkp_", StringComparison.OrdinalIgnoreCase));

                // Check if table has BIT columns that will actually render as Checkbox
                // Only BIT columns without special prefixes and not excluded from create forms
                var hasBooleanFields = GetDataColumns(table)
                    .Where(c => !c.IsPrimaryKey && !c.IsIdentity)
                    .Any(c =>
                    {
                        var (prefix, _) = SplitPrefix(c.Name);

                        // Exclude read-only prefixes that are skipped in create mode
                        if (prefix == "CLC" || prefix == "BLG" || prefix == "AGG" || prefix == "SCB")
                        {
                            return false;
                        }

                        // Only columns without special prefixes render as Checkbox for BIT type
                        // Columns with prefixes ENO, LKP, LOC, ENM, SPL, UPL, SPT, ENT use other components
                        if (!string.IsNullOrEmpty(prefix))
                        {
                            return false;
                        }

                        return c.DataType.Contains("BIT", StringComparison.OrdinalIgnoreCase);
                    });

                var imports = new List<string> { "TextField", "Button", "Box", "CircularProgress" };

                if (hasEnumOrLookupFields)
                {
                    imports.AddRange(EnumLookupImports);
                }

                if (hasBooleanFields)
                {
                    imports.AddRange(BooleanFieldImports);
                }

                sb.AppendLine(CultureInfo.InvariantCulture, $"import {{ {string.Join(", ", imports)} }} from '@mui/material';");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"import {{ use{className}, useCreate{className}, useUpdate{className} }} from '../../hooks/use{className}';");
            sb.AppendLine(CultureInfo.InvariantCulture, $"import type {{ Create{className}Request, Update{className}Request }} from '../../types/{className}.types';");

            return sb.ToString();
        }

        private static string GenerateFormField(Column column, UIFramework framework, bool isCreate)
        {
            var (prefix, baseName) = SplitPrefix(column.Name);
            var propertyName = GetPropertyName(column.Name);
            var fieldName = ToCamelCase(propertyName);

            // Skip read-only fields in create mode
            if (isCreate && (prefix == "CLC" || prefix == "BLG" || prefix == "AGG" || prefix == "SCB"))
            {
                return string.Empty;
            }

            return prefix switch
            {
                "ENO" => GeneratePasswordField(fieldName, propertyName, column, framework),
                "LKP" => GenerateLookupField(propertyName, baseName, column, framework),
                "LOC" => GenerateLocalizedField(fieldName, propertyName, column, framework),
                "ENM" => GenerateEnumField(fieldName, propertyName, baseName, column, framework),
                "SPL" => GenerateArrayField(fieldName, propertyName, column, framework),
                "UPL" => GenerateFileUploadField(fieldName, propertyName, framework),
                "CLC" => GenerateReadOnlyField(fieldName, propertyName, framework),
                "BLG" => GenerateReadOnlyField(fieldName, propertyName, framework),
                "AGG" => GenerateReadOnlyField(fieldName, propertyName, framework),
                "SCB" => string.Empty, // Hidden field, not rendered
                "SPT" => GenerateTextField(fieldName, propertyName, column, framework),
                "ENT" => GenerateTextField(fieldName, propertyName, column, framework),
                _ => GenerateDefaultField(fieldName, propertyName, column, framework),
            };
        }

        private static string GeneratePasswordField(string fieldName, string propertyName, Column column, UIFramework framework)
        {
            var sb = new StringBuilder();
            var validationRules = GetValidationRules(column);

            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("      <TextField");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        label=\"{propertyName}\"");
                sb.AppendLine("        type=\"password\"");
                sb.AppendLine("        fullWidth");
                sb.AppendLine("        margin=\"normal\"");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        {{...register('{fieldName}', {{ {string.Join(", ", validationRules)} }})}}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        error={{!!errors.{fieldName}}}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        helperText={{errors.{fieldName}?.message}}");
                sb.AppendLine("      />");
            }
            else
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"      <input type=\"password\" {{...register('{fieldName}', {{ {string.Join(", ", validationRules)} }})}} />");
            }

            return sb.ToString();
        }

        private static string GenerateLookupField(string propertyName, string baseName, Column column, UIFramework framework)
        {
            var sb = new StringBuilder();
            var codeField = ToCamelCase(baseName) + "Code";
            var validationRules = GetValidationRules(column);

            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("      <FormControl fullWidth margin=\"normal\">");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        <InputLabel>{propertyName}</InputLabel>");
                sb.AppendLine("        <Select");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          label=\"{propertyName}\"");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          {{...register('{codeField}', {{ {string.Join(", ", validationRules)} }})}}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          error={{!!errors.{codeField}}}");
                sb.AppendLine("        >");
                sb.AppendLine("          {/* TODO: Load options from lookup table */}");
                sb.AppendLine("          <MenuItem value=\"\">Select...</MenuItem>");
                sb.AppendLine("        </Select>");
                sb.AppendLine("      </FormControl>");
            }

            return sb.ToString();
        }

        private static string GenerateLocalizedField(string fieldName, string propertyName, Column column, UIFramework framework)
        {
            var sb = new StringBuilder();
            var validationRules = GetValidationRules(column);

            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("      <TextField");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        label=\"{propertyName}\"");
                sb.AppendLine("        multiline");
                sb.AppendLine("        rows={4}");
                sb.AppendLine("        fullWidth");
                sb.AppendLine("        margin=\"normal\"");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        {{...register('{fieldName}', {{ {string.Join(", ", validationRules)} }})}}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        error={{!!errors.{fieldName}}}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        helperText={{errors.{fieldName}?.message || 'Localized text'}}");
                sb.AppendLine("      />");
            }

            return sb.ToString();
        }

        private static string GenerateEnumField(string fieldName, string propertyName, string baseName, Column column, UIFramework framework)
        {
            var sb = new StringBuilder();
            var enumName = GetClassName(baseName);
            var validationRules = GetValidationRules(column);

            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("      <FormControl fullWidth margin=\"normal\">");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        <InputLabel>{propertyName}</InputLabel>");
                sb.AppendLine("        <Select");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          label=\"{propertyName}\"");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          {{...register('{fieldName}', {{ {string.Join(", ", validationRules)} }})}}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          error={{!!errors.{fieldName}}}");
                sb.AppendLine("        >");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          {{/* TODO: Load enum values for {enumName} */}}");
                sb.AppendLine("          <MenuItem value=\"\">Select...</MenuItem>");
                sb.AppendLine("        </Select>");
                sb.AppendLine("      </FormControl>");
            }

            return sb.ToString();
        }

        private static string GenerateArrayField(string fieldName, string propertyName, Column column, UIFramework framework)
        {
            var sb = new StringBuilder();
            var validationRules = GetValidationRules(column);

            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("      <TextField");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        label=\"{propertyName} (comma-separated)\"");
                sb.AppendLine("        fullWidth");
                sb.AppendLine("        margin=\"normal\"");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        {{...register('{fieldName}', {{ {string.Join(", ", validationRules)} }})}}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        error={{!!errors.{fieldName}}}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        helperText={{errors.{fieldName}?.message || 'Enter values separated by commas'}}");
                sb.AppendLine("      />");
            }

            return sb.ToString();
        }

        private static string GenerateFileUploadField(string fieldName, string propertyName, UIFramework framework)
        {
            var sb = new StringBuilder();

            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("      <Box sx={{ my: 2 }}>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        <Button variant=\"outlined\" component=\"label\">");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          Upload {propertyName}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"          <input type=\"file\" hidden {{...register('{fieldName}')}} />");
                sb.AppendLine("        </Button>");
                sb.AppendLine("      </Box>");
            }

            return sb.ToString();
        }

        private static string GenerateReadOnlyField(string fieldName, string propertyName, UIFramework framework)
        {
            var sb = new StringBuilder();

            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("      <TextField");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        label=\"{propertyName}\"");
                sb.AppendLine("        fullWidth");
                sb.AppendLine("        margin=\"normal\"");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        {{...register('{fieldName}')}}");
                sb.AppendLine("        disabled");
                sb.AppendLine("        helperText=\"Read-only field\"");
                sb.AppendLine("      />");
            }

            return sb.ToString();
        }

        private static string GenerateTextField(string fieldName, string propertyName, Column column, UIFramework framework)
        {
            var sb = new StringBuilder();
            var validationRules = GetValidationRules(column);
            var sqlType = column.DataType.ToUpperInvariant();
            var isMultiline = sqlType.Contains("TEXT", StringComparison.Ordinal);

            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("      <TextField");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        label=\"{propertyName}\"");

                if (isMultiline)
                {
                    sb.AppendLine("        multiline");
                    sb.AppendLine("        rows={4}");
                }

                sb.AppendLine("        fullWidth");
                sb.AppendLine("        margin=\"normal\"");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        {{...register('{fieldName}', {{ {string.Join(", ", validationRules)} }})}}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        error={{!!errors.{fieldName}}}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        helperText={{errors.{fieldName}?.message}}");
                sb.AppendLine("      />");
            }

            return sb.ToString();
        }

        private static string GenerateDefaultField(string fieldName, string propertyName, Column column, UIFramework framework)
        {
            var sb = new StringBuilder();
            var validationRules = GetValidationRules(column);
            var sqlType = column.DataType.ToUpperInvariant();

            if (framework == UIFramework.MaterialUI)
            {
                // Determine input type
                var inputType = "text";
                if (sqlType.Contains("INT", StringComparison.Ordinal) ||
                    sqlType.Contains("DECIMAL", StringComparison.Ordinal) ||
                    sqlType.Contains("NUMERIC", StringComparison.Ordinal) ||
                    sqlType.Contains("MONEY", StringComparison.Ordinal))
                {
                    inputType = "number";
                }
                else if (sqlType.Contains("DATE", StringComparison.Ordinal))
                {
                    inputType = "date";
                }
                else if (sqlType.Contains("TIME", StringComparison.Ordinal))
                {
                    inputType = "datetime-local";
                }

                if (sqlType.Contains("BIT", StringComparison.Ordinal))
                {
                    // Checkbox for boolean
                    sb.AppendLine("      <FormControlLabel");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"        control={{<Checkbox {{...register('{fieldName}')}} />}}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"        label=\"{propertyName}\"");
                    sb.AppendLine("      />");
                }
                else
                {
                    sb.AppendLine("      <TextField");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"        label=\"{propertyName}\"");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"        type=\"{inputType}\"");
                    sb.AppendLine("        fullWidth");
                    sb.AppendLine("        margin=\"normal\"");

                    if (inputType == "date" || inputType == "datetime-local")
                    {
                        sb.AppendLine("        InputLabelProps={{ shrink: true }}");
                    }

                    sb.AppendLine(CultureInfo.InvariantCulture, $"        {{...register('{fieldName}', {{ {string.Join(", ", validationRules)} }})}}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"        error={{!!errors.{fieldName}}}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"        helperText={{errors.{fieldName}?.message}}");
                    sb.AppendLine("      />");
                }
            }

            return sb.ToString();
        }

        private static string GenerateFormFields(Table table, UIFramework framework)
        {
            var sb = new StringBuilder();
            var dataColumns = GetDataColumns(table)
                .Where(c => !c.IsPrimaryKey && !c.IsIdentity)
                .ToList();

            foreach (var column in dataColumns)
            {
                var field = GenerateFormField(column, framework, isCreate: true);
                if (!string.IsNullOrWhiteSpace(field))
                {
                    sb.AppendLine(field);
                }
            }

            return sb.ToString();
        }

        private static string GenerateComponentBody(Table table, string className, string camelName, UIFramework framework)
        {
            var sb = new StringBuilder();

            sb.AppendLine(CultureInfo.InvariantCulture, $"export const {className}Form: React.FC = () => {{");
            sb.AppendLine("  const navigate = useNavigate();");
            sb.AppendLine("  const { id } = useParams<{ id: string }>();");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  const {camelName}Id = id && id !== 'new' ? Number(id) : undefined;");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  const isEdit = !!{camelName}Id;");
            sb.AppendLine();

            // Load existing data if editing
            sb.AppendLine(CultureInfo.InvariantCulture, $"  const {{ data: existing{className}, isPending: isLoadingEntity }} = use{className}({camelName}Id || null);");
            sb.AppendLine();

            // Form hook
            sb.AppendLine("  const {");
            sb.AppendLine("    register,");
            sb.AppendLine("    handleSubmit,");
            sb.AppendLine("    reset,");
            sb.AppendLine("    formState: { errors },");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  }} = useForm<Create{className}Request>({{");
            sb.AppendLine("    defaultValues: {");
            sb.Append(GenerateDefaultValues(table));
            sb.AppendLine("    },");
            sb.AppendLine("  });");
            sb.AppendLine();
            sb.AppendLine("  // Update form when data loads");
            sb.AppendLine("  useEffect(() => {");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    if (existing{className}) {{");
            sb.AppendLine(CultureInfo.InvariantCulture, $"      reset(existing{className});");
            sb.AppendLine("    }");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  }}, [existing{className}, reset]);");
            sb.AppendLine();

            // Mutation hooks
            sb.AppendLine(CultureInfo.InvariantCulture, $"  const {{ mutate: createEntity, isPending: isCreating }} = useCreate{className}();");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  const {{ mutate: updateEntity, isPending: isUpdating }} = useUpdate{className}();");
            sb.AppendLine();
            sb.AppendLine("  const isSubmitting = isCreating || isUpdating;");
            sb.AppendLine();

            // Submit handler
            sb.AppendLine(CultureInfo.InvariantCulture, $"  const onSubmit = (data: Create{className}Request) => {{");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    if (isEdit && {camelName}Id) {{");
            sb.AppendLine("      updateEntity(");
            sb.AppendLine(CultureInfo.InvariantCulture, $"        {{ id: {camelName}Id, data: data as Update{className}Request }},");
            sb.AppendLine(CultureInfo.InvariantCulture, $"        {{ onSuccess: () => navigate('/{camelName}s') }}");
            sb.AppendLine("      );");
            sb.AppendLine("    } else {");
            sb.AppendLine(CultureInfo.InvariantCulture, $"      createEntity(data, {{ onSuccess: () => navigate('/{camelName}s') }});");
            sb.AppendLine("    }");
            sb.AppendLine("  };");
            sb.AppendLine();

            // Loading state
            sb.AppendLine("  if (isEdit && isLoadingEntity) {");
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

            // Render
            sb.AppendLine("  return (");
            if (framework == UIFramework.MaterialUI)
            {
                sb.AppendLine("    <Box component=\"form\" onSubmit={handleSubmit(onSubmit)} sx={{ maxWidth: 600 }}>");
                sb.AppendLine(GenerateFormFields(table, framework));
                sb.AppendLine();
                sb.AppendLine("      <Box sx={{ mt: 2, display: 'flex', gap: 2 }}>");
                sb.AppendLine("        <Button type=\"submit\" variant=\"contained\" disabled={isSubmitting}>");
                sb.AppendLine("          {isEdit ? 'Update' : 'Create'}");
                sb.AppendLine("        </Button>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        <Button variant=\"outlined\" onClick={{() => navigate('/{camelName}s')}}>");
                sb.AppendLine("          Cancel");
                sb.AppendLine("        </Button>");
                sb.AppendLine("      </Box>");
                sb.AppendLine("    </Box>");
            }
            else
            {
                sb.AppendLine("    <form onSubmit={handleSubmit(onSubmit)}>");
                sb.AppendLine(GenerateFormFields(table, framework));
                sb.AppendLine("      <button type=\"submit\">{isEdit ? 'Update' : 'Create'}</button>");
                sb.AppendLine("    </form>");
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
            sb.AppendLine(GenerateImports(className, config.Framework, config.ValidationLibrary, table));
            sb.AppendLine();

            // Component
            sb.AppendLine(GenerateComponentBody(table, className, camelName, config.Framework));

            return sb.ToString();
        }

        private static string GenerateDefaultValues(Table table)
        {
            var sb = new StringBuilder();
            var defaultValues = new List<string>();

            foreach (var column in table.Columns)
            {
                // Skip primary key and auto-generated fields
                if (column.IsPrimaryKey)
                {
                    continue;
                }

                var (prefix, baseName) = SplitPrefix(column.Name);
                var propertyName = GetPropertyName(column.Name);
                var fieldName = ToCamelCase(propertyName);

                // Skip read-only fields (CLC, BLG, AGG, SCB)
                if (prefix == "CLC" || prefix == "BLG" || prefix == "AGG" || prefix == "SCB")
                {
                    continue;
                }

                // Generate appropriate default value based on field type
                string? defaultValue = prefix switch
                {
                    "LKP" => $"{ToCamelCase(baseName)}Code: ''",  // Lookup fields need empty string for Select
                    "ENM" => $"{fieldName}: ''",  // Enum fields (Select) need empty string
                    "BIT" => $"{fieldName}: false",  // Boolean fields
                    _ => null,  // Other fields can be undefined
                };

                if (defaultValue != null)
                {
                    defaultValues.Add(defaultValue);
                }
            }

            if (defaultValues.Count > 0)
            {
                foreach (var value in defaultValues)
                {
                    sb.AppendLine(CultureInfo.InvariantCulture, $"      {value},");
                }
            }

            return sb.ToString();
        }
    }
}
