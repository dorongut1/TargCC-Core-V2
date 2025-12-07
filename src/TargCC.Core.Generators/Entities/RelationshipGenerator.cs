// <copyright file="RelationshipGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.Entities
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using TargCC.Core.Generators.Common;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Generates navigation properties for entity relationships.
    /// </summary>
    public static class RelationshipGenerator
    {
        /// <summary>
        /// Generates navigation properties for a table's relationships.
        /// </summary>
        /// <param name="table">The table to generate relationships for.</param>
        /// <param name="schema">The database schema (for looking up related tables).</param>
        /// <returns>Navigation properties code.</returns>
        public static string GenerateNavigationProperties(Table table, DatabaseSchema schema)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentNullException.ThrowIfNull(schema);

            if (table.Relationships == null || table.Relationships.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var relationship in table.Relationships)
            {
                var navProperty = GenerateNavigationProperty(relationship, table, schema);
                if (!string.IsNullOrEmpty(navProperty))
                {
                    sb.AppendLine(navProperty);
                }
            }

            return sb.ToString();
        }

        private static string GenerateNavigationProperty(
            Relationship relationship,
            Table currentTable,
            DatabaseSchema schema)
        {
            var sb = new StringBuilder();

            // Determine if this table is the parent or child in the relationship
            var isParent = relationship.ParentTable.Equals(currentTable.FullName, StringComparison.OrdinalIgnoreCase);

            if (isParent)
            {
                // This is the parent (one side) - generate collection navigation property
                var childTable = schema.Tables.Find(t =>
                    t.FullName.Equals(relationship.ChildTable, StringComparison.OrdinalIgnoreCase));

                if (childTable != null)
                {
                    var childClassName = GetClassName(childTable.Name);
                    var propertyName = CodeGenerationHelpers.MakePlural(childClassName);

                    sb.AppendLine("        /// <summary>");
                    sb.Append(CultureInfo.InvariantCulture, $"        /// Navigation property to {propertyName} collection.");
                    sb.AppendLine();
                    sb.AppendLine("        /// </summary>");
                    sb.Append(CultureInfo.InvariantCulture, $"        public virtual ICollection<{childClassName}> {propertyName} {{ get; set; }}");
                    sb.AppendLine();
                }
            }
            else
            {
                // This is the child (many side) - generate reference navigation property
                var parentTable = schema.Tables.Find(t =>
                    t.FullName.Equals(relationship.ParentTable, StringComparison.OrdinalIgnoreCase));

                if (parentTable != null)
                {
                    var parentClassName = GetClassName(parentTable.Name);
                    var propertyName = parentClassName;

                    sb.AppendLine("        /// <summary>");
                    sb.Append(CultureInfo.InvariantCulture, $"        /// Navigation property to parent {parentClassName}.");
                    sb.AppendLine();
                    sb.AppendLine("        /// </summary>");
                    sb.Append(CultureInfo.InvariantCulture, $"        public virtual {parentClassName} {propertyName} {{ get; set; }}");
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private static string GetClassName(string tableName)
        {
            // Remove common table prefixes
            var className = tableName.Replace("tbl_", string.Empty, StringComparison.OrdinalIgnoreCase)
                                    .Replace("tbl", string.Empty, StringComparison.OrdinalIgnoreCase);

            return className;
        }
    }
}
