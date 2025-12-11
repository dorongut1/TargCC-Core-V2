// <copyright file="MethodGenerator.cs" company="PlaceholderCompany">
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
    /// Generates methods for entity classes (Constructor, ToString, Clone, Equals, GetHashCode).
    /// </summary>
    public static class MethodGenerator
    {
        /// <summary>
        /// Generates the default constructor.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="className">The class name.</param>
        /// <param name="schema">The database schema.</param>
        /// <returns>Constructor code.</returns>
        public static string GenerateConstructor(Table table, string className, DatabaseSchema schema)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentException.ThrowIfNullOrWhiteSpace(className);

            var sb = new StringBuilder();

            sb.AppendLine("        /// <summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        /// Initializes a new instance of the <see cref=\"{className}\"/> class.");
            sb.AppendLine();
            sb.AppendLine("        /// </summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        public {className}()");
            sb.AppendLine();
            sb.AppendLine("        {");

            // Initialize collections for navigation properties
            if (table.Relationships != null && table.Relationships.Count > 0 && schema != null)
            {
                foreach (var relationship in table.Relationships)
                {
                    var isParent = relationship.ParentTable.Equals(table.FullName, StringComparison.OrdinalIgnoreCase);
                    if (isParent)
                    {
                        var childTable = schema.Tables.Find(t =>
                            t.FullName.Equals(relationship.ChildTable, StringComparison.OrdinalIgnoreCase));

                        if (childTable != null)
                        {
                            var childClassName = GetClassName(childTable.Name);
                            var propertyName = CodeGenerationHelpers.MakePlural(childClassName);
                            sb.Append(CultureInfo.InvariantCulture, $"            this.{propertyName} = new List<{childClassName}>();");
                            sb.AppendLine();
                        }
                    }
                }
            }

            // Initialize audit columns
            var addedOnColumn = table.Columns.Find(c => c.Name.Equals("AddedOn", StringComparison.OrdinalIgnoreCase));
            if (addedOnColumn != null)
            {
                sb.AppendLine("            this.AddedOn = DateTime.Now;");
            }

            // Initialize aggregate columns to 0
            var aggregateColumns = table.Columns.Where(c => c.Prefix == ColumnPrefix.Aggregate);
            foreach (var col in aggregateColumns)
            {
                var propertyName = PrefixHandler.GetPropertyName(col);
                var csharpType = TypeMapper.MapSqlTypeToCSharp(col);
                var defaultValue = TypeMapper.GetDefaultValue(csharpType);
                sb.Append(CultureInfo.InvariantCulture, $"            this.{propertyName} = {defaultValue};");
                sb.AppendLine();
            }

            sb.AppendLine("        }");

            return sb.ToString();
        }

        /// <summary>
        /// Generates the ToString method.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="className">The class name.</param>
        /// <returns>ToString method code.</returns>
        public static string GenerateToString(Table table, string className)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentException.ThrowIfNullOrWhiteSpace(className);

            var sb = new StringBuilder();

            sb.AppendLine("        /// <summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        /// Returns a string representation of this {className}.");
            sb.AppendLine();
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public override string ToString()");
            sb.AppendLine("        {");

            // Find primary key
            var pkColumns = table.Columns.Where(c => c.IsPrimaryKey).ToList();

            // Find a good display column (Name, Title, Description, Email, etc.)
            var displayColumn = table.Columns.Find(c =>
                c.Name.Equals("Name", StringComparison.OrdinalIgnoreCase) ||
                c.Name.Equals("Title", StringComparison.OrdinalIgnoreCase) ||
                c.Name.Equals("Description", StringComparison.OrdinalIgnoreCase) ||
                c.Name.Equals("Email", StringComparison.OrdinalIgnoreCase));

            if (pkColumns.Count > 0 && displayColumn != null)
            {
                var pkName = PrefixHandler.GetPropertyName(pkColumns[0]);
                var displayName = PrefixHandler.GetPropertyName(displayColumn);
                sb.Append(CultureInfo.InvariantCulture, $"            return $\"{className} #{{{pkName}}}: {{{displayName}}}\";");
                sb.AppendLine();
            }
            else if (pkColumns.Count > 0)
            {
                var pkName = PrefixHandler.GetPropertyName(pkColumns[0]);
                sb.Append(CultureInfo.InvariantCulture, $"            return $\"{className} #{{{pkName}}}\";");
                sb.AppendLine();
            }
            else
            {
                sb.Append(CultureInfo.InvariantCulture, $"            return $\"{className}\";");
                sb.AppendLine();
            }

            sb.AppendLine("        }");

            return sb.ToString();
        }

        /// <summary>
        /// Generates the Clone method.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="className">The class name.</param>
        /// <returns>Clone method code.</returns>
        public static string GenerateClone(Table table, string className)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentException.ThrowIfNullOrWhiteSpace(className);

            var sb = new StringBuilder();

            sb.AppendLine("        /// <summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        /// Creates a shallow copy of this {className}.");
            sb.AppendLine();
            sb.AppendLine("        /// </summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        public {className} Clone()");
            sb.AppendLine();
            sb.AppendLine("        {");
            sb.Append(CultureInfo.InvariantCulture, $"            return new {className}");
            sb.AppendLine();
            sb.AppendLine("            {");

            // Clone only regular data properties (not audit, not PK, not navigation)
            var cloneableColumns = table.Columns.Where(c =>
                !c.IsPrimaryKey &&
                !PropertyGenerator.IsAuditColumn(c) &&
                c.Prefix != ColumnPrefix.Aggregate &&
                c.Prefix != ColumnPrefix.Calculated &&
                c.Prefix != ColumnPrefix.BusinessLogic).ToList();

            for (int i = 0; i < cloneableColumns.Count; i++)
            {
                var col = cloneableColumns[i];
                var propertyName = PrefixHandler.GetPropertyName(col);
                var line = string.Format(CultureInfo.InvariantCulture, "                {0} = this.{0}", propertyName);

                if (i < cloneableColumns.Count - 1)
                {
                    line += ",";
                }

                sb.AppendLine(line);
            }

            sb.AppendLine("            };");
            sb.AppendLine("        }");

            return sb.ToString();
        }

        /// <summary>
        /// Generates the Equals method.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="className">The class name.</param>
        /// <returns>Equals method code.</returns>
        public static string GenerateEquals(Table table, string className)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentException.ThrowIfNullOrWhiteSpace(className);

            var sb = new StringBuilder();

            sb.AppendLine("        /// <summary>");
            sb.Append(CultureInfo.InvariantCulture, $"        /// Determines whether the specified object is equal to this {className}.");
            sb.AppendLine();
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public override bool Equals(object obj)");
            sb.AppendLine("        {");
            sb.Append(CultureInfo.InvariantCulture, $"            if (obj is not {className} other)");
            sb.AppendLine();
            sb.AppendLine("                return false;");
            sb.AppendLine();

            var pkColumns = table.Columns.Where(c => c.IsPrimaryKey).ToList();

            if (pkColumns.Count > 0)
            {
                // Compare by primary key if both have values
                var comparisons = pkColumns.Select(pk =>
                {
                    var propertyName = PrefixHandler.GetPropertyName(pk);
                    var csharpType = TypeMapper.MapSqlTypeToCSharp(pk);

                    if (csharpType.Contains("int", StringComparison.OrdinalIgnoreCase) ||
                        csharpType.Contains("long", StringComparison.OrdinalIgnoreCase))
                    {
                        return string.Format(CultureInfo.InvariantCulture, "this.{0} > 0 && other.{0} > 0", propertyName);
                    }

                    return string.Format(CultureInfo.InvariantCulture, "this.{0} != default && other.{0} != default", propertyName);
                });

                sb.Append(CultureInfo.InvariantCulture, $"            if ({string.Join(" && ", comparisons)})");
                sb.AppendLine();

                var pkComparisons = pkColumns.Select(pk =>
                {
                    var propertyName = PrefixHandler.GetPropertyName(pk);
                    return string.Format(CultureInfo.InvariantCulture, "this.{0} == other.{0}", propertyName);
                });

                sb.Append(CultureInfo.InvariantCulture, $"                return {string.Join(" && ", pkComparisons)};");
                sb.AppendLine();
            }

            // Fallback to comparing a unique column or first non-PK column
            var uniqueColumn = table.Columns.Find(c =>
                !c.IsPrimaryKey &&
                (c.Name.Contains("Email", StringComparison.OrdinalIgnoreCase) ||
                 c.Name.Contains("Username", StringComparison.OrdinalIgnoreCase)));

            if (uniqueColumn != null)
            {
                var propertyName = PrefixHandler.GetPropertyName(uniqueColumn);
                sb.AppendLine();
                sb.Append(CultureInfo.InvariantCulture, $"            return this.{propertyName} == other.{propertyName};");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine("            return base.Equals(obj);");
            }

            sb.AppendLine("        }");

            return sb.ToString();
        }

        /// <summary>
        /// Generates the GetHashCode method.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns>GetHashCode method code.</returns>
        public static string GenerateGetHashCode(Table table)
        {
            ArgumentNullException.ThrowIfNull(table);

            var sb = new StringBuilder();

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Returns a hash code for this instance.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public override int GetHashCode()");
            sb.AppendLine("        {");

            var pkColumns = table.Columns.Where(c => c.IsPrimaryKey).ToList();

            if (pkColumns.Count == 1)
            {
                var pk = pkColumns[0];
                var propertyName = PrefixHandler.GetPropertyName(pk);
                var csharpType = TypeMapper.MapSqlTypeToCSharp(pk);

                if (csharpType.Contains("int", StringComparison.OrdinalIgnoreCase) ||
                    csharpType.Contains("long", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(CultureInfo.InvariantCulture, $"            return this.{propertyName} > 0 ? this.{propertyName}.GetHashCode() : 0;");
                    sb.AppendLine();
                }
                else if (csharpType.Contains("string", StringComparison.OrdinalIgnoreCase) ||
                         csharpType.Contains("?", StringComparison.Ordinal))
                {
                    // Reference types (string) or nullable value types can use ?.
                    sb.Append(CultureInfo.InvariantCulture, $"            return this.{propertyName}?.GetHashCode() ?? 0;");
                    sb.AppendLine();
                }
                else
                {
                    // Non-nullable value types (bool, DateTime, decimal, etc.) - use GetHashCode directly
                    sb.Append(CultureInfo.InvariantCulture, $"            return this.{propertyName}.GetHashCode();");
                    sb.AppendLine();
                }
            }
            else if (pkColumns.Count > 1)
            {
                // Composite key
                var propertyNames = pkColumns.Select(pk => string.Format(CultureInfo.InvariantCulture, "this.{0}", PrefixHandler.GetPropertyName(pk)));
                sb.Append(CultureInfo.InvariantCulture, $"            return HashCode.Combine({string.Join(", ", propertyNames)});");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("            return base.GetHashCode();");
            }

            sb.AppendLine("        }");

            return sb.ToString();
        }

        /// <summary>
        /// Generates helper methods for encryption (placeholder implementations).
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns>Helper methods code.</returns>
        public static string GenerateHelperMethods(Table table)
        {
            ArgumentNullException.ThrowIfNull(table);

            var hasEncryptedColumns = table.Columns.Exists(c => c.Prefix == ColumnPrefix.TwoWayEncryption);
            var hasHashedColumns = table.Columns.Exists(c => c.Prefix == ColumnPrefix.OneWayEncryption);

            if (!hasEncryptedColumns && !hasHashedColumns)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            if (hasEncryptedColumns)
            {
                sb.AppendLine("        private string EncryptValue(string plainText) =>");
                sb.AppendLine("            string.IsNullOrEmpty(plainText) ? null : $\"[PleaseEncrypt]{plainText}\";");
                sb.AppendLine();
                sb.AppendLine("        private string DecryptValue(string encrypted) =>");
                sb.AppendLine("            string.IsNullOrEmpty(encrypted) ? null : $\"[PleaseDecrypt]{encrypted}\";");
            }

            if (hasHashedColumns)
            {
                sb.AppendLine();
                sb.AppendLine("        /// <summary>");
                sb.AppendLine("        /// Sets the password (will be hashed before saving).");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine("        public void SetPassword(string plainTextPassword)");
                sb.AppendLine("        {");
                sb.AppendLine("            if (string.IsNullOrEmpty(plainTextPassword))");
                sb.AppendLine("                throw new ArgumentNullException(nameof(plainTextPassword));");
                sb.AppendLine();

                var hashedColumn = table.Columns.First(c => c.Prefix == ColumnPrefix.OneWayEncryption);
                var propertyName = PrefixHandler.GetPropertyName(hashedColumn);

                sb.Append(CultureInfo.InvariantCulture, $"            this.{propertyName} = $\"[PleaseHash]{{plainTextPassword}}\";");
                sb.AppendLine();
                sb.AppendLine("        }");
            }

            return sb.ToString();
        }

        private static string GetClassName(string tableName)
        {
            return tableName.Replace("tbl_", string.Empty, StringComparison.OrdinalIgnoreCase)
                           .Replace("tbl", string.Empty, StringComparison.OrdinalIgnoreCase);
        }
    }
}
