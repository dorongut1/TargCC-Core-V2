using System.Globalization;
using System.Text;
using TargCC.Core.Generators.API;
using TargCC.Core.Generators.Common;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Generators.Domain;

/// <summary>
/// Generates domain entity classes for database views.
/// </summary>
public static class ViewEntityGenerator
{
    /// <summary>
    /// Generates an entity class for a view.
    /// </summary>
    /// <param name="view">The view information.</param>
    /// <param name="rootNamespace">The root namespace.</param>
    /// <returns>The generated entity code.</returns>
    public static string Generate(ViewInfo view, string rootNamespace)
    {
        var className = BaseApiGenerator.GetClassName(view.ViewName);
        var sb = new StringBuilder();

        // Using statements
        sb.AppendLine("using System;");
        sb.AppendLine();

        // Namespace
        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {rootNamespace}.Domain.Entities;");
        sb.AppendLine();

        // Class documentation
        sb.AppendLine("/// <summary>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"/// Represents a record from the {view.ViewName} view.");
        sb.AppendLine("/// This is a read-only entity as it maps to a database view.");
        sb.AppendLine("/// </summary>");

        // Class declaration
        sb.AppendLine(CultureInfo.InvariantCulture, $"public class {className}");
        sb.AppendLine("{");

        // Properties
        foreach (var column in view.Columns.OrderBy(c => c.OrdinalPosition))
        {
            var propertyName = CodeGenerationHelpers.SanitizeColumnName(column.Name);
            var propertyType = MapSqlTypeToCSharp(column);

            sb.AppendLine("    /// <summary>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    /// Gets or sets the {propertyName}.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    public {propertyType} {propertyName} {{ get; set; }}");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string MapSqlTypeToCSharp(ViewColumn column)
    {
        var baseType = column.DataType.ToUpperInvariant() switch
        {
            "int" => "int",
            "bigint" => "long",
            "smallint" => "short",
            "tinyint" => "byte",
            "bit" => "bool",
            "decimal" or "numeric" or "money" or "smallmoney" => "decimal",
            "float" => "double",
            "real" => "float",
            "datetime" or "datetime2" or "smalldatetime" or "date" => "DateTime",
            "time" => "TimeSpan",
            "datetimeoffset" => "DateTimeOffset",
            "uniqueidentifier" => "Guid",
            "char" or "varchar" or "nchar" or "nvarchar" or "text" or "ntext" => "string",
            "binary" or "varbinary" or "image" => "byte[]",
            _ => "object"
        };

        // Add nullable suffix for value types if column is nullable
        if (column.IsNullable && IsValueType(baseType))
        {
            return baseType + "?";
        }

        // String is already nullable, no need for ?
        return baseType;
    }

    private static bool IsValueType(string typeName)
    {
        return typeName is not("string" or "byte[]" or "object");
    }
}
