// <copyright file="PropertyGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.Entities
{
    using System;
    using System.Globalization;
    using System.Text;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Generates property declarations for entity classes.
    /// </summary>
    public static class PropertyGenerator
    {
        /// <summary>
        /// Generates a complete property declaration with attributes and documentation.
        /// </summary>
        /// <param name="column">The column to generate property for.</param>
        /// <returns>The property code.</returns>
        public static string GenerateProperty(Column column)
        {
            ArgumentNullException.ThrowIfNull(column);

            // Check if this is a prefixed column with special handling
            var csharpType = TypeMapper.MapSqlTypeToCSharp(column);

            if (column.Prefix != ColumnPrefix.None && column.Prefix != ColumnPrefix.FakeUniqueIndex)
            {
                var prefixedProperty = PrefixHandler.GeneratePrefixedProperty(column, csharpType);
                if (!string.IsNullOrEmpty(prefixedProperty))
                {
                    return prefixedProperty;
                }
            }

            // Regular property
            return GenerateRegularProperty(column, csharpType);
        }

        /// <summary>
        /// Generates backing fields for columns that need them.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>Backing field declaration or null.</returns>
        public static string? GenerateBackingField(Column column)
        {
            ArgumentNullException.ThrowIfNull(column);

            if (PrefixHandler.RequiresBackingField(column.Prefix))
            {
                var csharpType = TypeMapper.MapSqlTypeToCSharp(column);
                return PrefixHandler.GenerateBackingField(column, csharpType);
            }

            return null;
        }

        /// <summary>
        /// Determines if a column is an audit column.
        /// </summary>
        /// <param name="column">The column to check.</param>
        /// <returns>True if audit column.</returns>
        public static bool IsAuditColumn(Column column)
        {
            ArgumentNullException.ThrowIfNull(column);

            var auditColumnNames = new[]
            {
                "AddedOn", "AddedBy", "CreatedOn", "CreatedBy",
                "ChangedOn", "ChangedBy", "ModifiedOn", "ModifiedBy",
                "UpdatedOn", "UpdatedBy",
            };

            return Array.Exists(auditColumnNames, name =>
                column.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private static string GenerateRegularProperty(Column column, string csharpType)
        {
            var sb = new StringBuilder();
            var propertyName = PrefixHandler.GetPropertyName(column);

            AppendDocumentation(sb, propertyName);
            AppendAttributes(sb, column, csharpType);
            AppendPropertyDeclaration(sb, column, csharpType, propertyName);

            return sb.ToString();
        }

        private static void AppendDocumentation(StringBuilder sb, string propertyName)
        {
            sb.AppendLine("        /// <summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        /// Gets or sets the {propertyName}.");
            sb.AppendLine();
            sb.AppendLine("        /// </summary>");
        }

        private static void AppendAttributes(StringBuilder sb, Column column, string csharpType)
        {
            // Column attribute
            sb.Append(CultureInfo.InvariantCulture, $"        [Column(\"{column.Name}\")]");
            sb.AppendLine();

            // Key attribute for primary keys
            if (column.IsPrimaryKey)
            {
                sb.AppendLine("        [Key]");
            }

            // Required attribute
            if (!column.IsNullable && !column.IsPrimaryKey)
            {
                sb.AppendLine("        [Required]");
            }

            // MaxLength for string types
            if (csharpType == "string" && column.MaxLength.HasValue)
            {
                var maxLength = column.MaxLength.Value == -1
                    ? "int.MaxValue"
                    : column.MaxLength.Value.ToString(CultureInfo.InvariantCulture);
                sb.Append(CultureInfo.InvariantCulture, $"        [MaxLength({maxLength})]");
                sb.AppendLine();
            }

            // DatabaseGenerated attribute for audit columns
            AppendDatabaseGeneratedAttribute(sb, column);
        }

        private static void AppendDatabaseGeneratedAttribute(StringBuilder sb, Column column)
        {
            if (!IsAuditColumn(column))
            {
                return;
            }

            if (column.Name.StartsWith("Added", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
            }
            else if (column.Name.StartsWith("Changed", StringComparison.OrdinalIgnoreCase) ||
                     column.Name.StartsWith("Modified", StringComparison.OrdinalIgnoreCase) ||
                     column.Name.StartsWith("Updated", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]");
            }
        }

        private static void AppendPropertyDeclaration(StringBuilder sb, Column column, string csharpType, string propertyName)
        {
            var setter = IsAuditColumn(column) ? "private set" : "set";
            sb.Append(CultureInfo.InvariantCulture, $"        public {csharpType} {propertyName} {{ get; {setter}; }}");

            // Default value for audit columns (S1066 fixed - merged if statements)
            if (IsAuditColumn(column) &&
                column.Name.EndsWith("On", StringComparison.OrdinalIgnoreCase) &&
                csharpType.StartsWith("DateTime", StringComparison.Ordinal))
            {
                sb.Append(" = DateTime.Now;");
            }

            sb.AppendLine();
        }
    }
}
