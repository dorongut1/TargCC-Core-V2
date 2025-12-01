// <copyright file="MappingProfileGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.API
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Generates AutoMapper mapping profiles for Entity to DTO mappings.
    /// </summary>
    public class MappingProfileGenerator : BaseApiGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingProfileGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public MappingProfileGenerator(ILogger<MappingProfileGenerator> logger)
            : base(logger)
        {
        }

        /// <inheritdoc/>
        protected override string GeneratorTypeName => "Mapping Profile";

        /// <inheritdoc/>
        protected override string Generate(Table table, DatabaseSchema schema, ApiGeneratorConfig config)
        {
            var className = GetClassName(table.Name);

            var sb = new StringBuilder();

            sb.Append(GenerateFileHeader(table.Name, "Mapping Profile Generator"));

            AppendUsings(sb);

            sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {config.Namespace}.Mapping");
            sb.AppendLine("{");

            GenerateMappingProfileClass(sb, table, className, config);

            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void AppendUsings(StringBuilder sb)
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using AutoMapper;");
            sb.AppendLine();
        }

        private static void GenerateMappingProfileClass(StringBuilder sb, Table table, string className, ApiGeneratorConfig config)
        {
            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("    /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"    /// AutoMapper profile for {className} entity mappings.");
                sb.AppendLine("    /// </summary>");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"    public class {className}MappingProfile : Profile");
            sb.AppendLine("    {");

            if (config.GenerateXmlDocumentation)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"        /// Initializes a new instance of the <see cref=\"{className}MappingProfile\"/> class.");
                sb.AppendLine("        /// </summary>");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"        public {className}MappingProfile()");
            sb.AppendLine("        {");

            GenerateEntityToDtoMapping(sb, table, className);
            sb.AppendLine();

            GenerateCreateRequestToEntityMapping(sb, table, className);
            sb.AppendLine();

            GenerateUpdateRequestToEntityMapping(sb, table, className);

            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }

        private static void GenerateEntityToDtoMapping(StringBuilder sb, Table table, string className)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"            // Entity to Response DTO");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            CreateMap<{className}, {className}Dto>()");

            var specialColumns = table.Columns
                .Where(c => !IsAuditField(c.Name))
                .Select(c => (column: c, split: SplitPrefix(c.Name)))
                .Where(x => !string.IsNullOrEmpty(x.split.prefix))
                .ToList();

            foreach (var (column, (prefix, baseName)) in specialColumns)
            {
                switch (prefix)
                {
                    case "LKP":
                        GenerateLkpEntityToDtoMapping(sb, baseName);
                        break;

                    case "LOC":
                        GenerateLocEntityToDtoMapping(sb, baseName);
                        break;

                    case "SPL":
                    case "SPT":
                        GenerateSplitListEntityToDtoMapping(sb, column.Name, prefix);
                        break;
                }
            }

            sb.AppendLine("                .ReverseMap();");
        }

        private static void GenerateCreateRequestToEntityMapping(StringBuilder sb, Table table, string className)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"            // CreateRequest to Entity");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            CreateMap<Create{className}Request, {className}>()");

            var mutableColumns = GetMutableColumns(table)
                .Select(c => (column: c, split: SplitPrefix(c.Name)))
                .Where(x => !string.IsNullOrEmpty(x.split.prefix))
                .ToList();

            foreach (var (column, (prefix, baseName)) in mutableColumns)
            {
                switch (prefix)
                {
                    case "ENO":
                        GenerateEnoCreateToEntityMapping(sb, baseName);
                        break;

                    case "LKP":
                        GenerateLkpCreateToEntityMapping(sb, baseName);
                        break;

                    case "LOC":
                        GenerateLocCreateToEntityMapping(sb, baseName);
                        break;

                    case "SPL":
                    case "SPT":
                        GenerateSplitListCreateToEntityMapping(sb, column.Name, prefix);
                        break;
                }
            }

            sb.AppendLine("                ;");
        }

        private static void GenerateUpdateRequestToEntityMapping(StringBuilder sb, Table table, string className)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"            // UpdateRequest to Entity");
            sb.AppendLine(CultureInfo.InvariantCulture, $"            CreateMap<Update{className}Request, {className}>()");

            var mutableColumns = GetMutableColumns(table)
                .Select(c => (column: c, split: SplitPrefix(c.Name)))
                .Where(x => !string.IsNullOrEmpty(x.split.prefix))
                .ToList();

            foreach (var (column, (prefix, baseName)) in mutableColumns)
            {
                switch (prefix)
                {
                    case "ENO":
                        GenerateEnoUpdateToEntityMapping(sb, baseName);
                        break;

                    case "LKP":
                        GenerateLkpUpdateToEntityMapping(sb, baseName);
                        break;

                    case "LOC":
                        GenerateLocUpdateToEntityMapping(sb, baseName);
                        break;

                    case "SPL":
                    case "SPT":
                        GenerateSplitListUpdateToEntityMapping(sb, column.Name, prefix);
                        break;
                }
            }

            sb.AppendLine("                ;");
        }

        private static void GenerateLkpEntityToDtoMapping(StringBuilder sb, string baseName)
        {
            var propName = ToPascalCase(baseName);
            sb.AppendLine(CultureInfo.InvariantCulture, $"                .ForMember(dest => dest.{propName}Code, opt => opt.MapFrom(src => src.{propName}))");
            sb.AppendLine(CultureInfo.InvariantCulture, $"                .ForMember(dest => dest.{propName}Text, opt => opt.Ignore()) // TODO: Implement lookup resolution");
        }

        private static void GenerateLkpCreateToEntityMapping(StringBuilder sb, string baseName)
        {
            var propName = ToPascalCase(baseName);
            sb.AppendLine(CultureInfo.InvariantCulture, $"                .ForMember(dest => dest.{propName}, opt => opt.MapFrom(src => src.{propName}Code))");
        }

        private static void GenerateLkpUpdateToEntityMapping(StringBuilder sb, string baseName)
        {
            var propName = ToPascalCase(baseName);
            sb.AppendLine(CultureInfo.InvariantCulture, $"                .ForMember(dest => dest.{propName}, opt => opt.MapFrom(src => src.{propName}Code))");
        }

        private static void GenerateLocEntityToDtoMapping(StringBuilder sb, string baseName)
        {
            var propName = ToPascalCase(baseName);
            sb.AppendLine(CultureInfo.InvariantCulture, $"                .ForMember(dest => dest.{propName}Localized, opt => opt.Ignore()) // TODO: Implement localization");
        }

        private static void GenerateLocCreateToEntityMapping(StringBuilder sb, string baseName)
        {
            // No special mapping needed for LOC in create - maps directly
        }

        private static void GenerateLocUpdateToEntityMapping(StringBuilder sb, string baseName)
        {
            // No special mapping needed for LOC in update - maps directly
        }

        private static void GenerateSplitListEntityToDtoMapping(StringBuilder sb, string columnName, string prefix)
        {
            var propName = GetPropertyName(columnName);
            var separator = prefix == "SPL" ? "|" : ",";

            sb.AppendLine(CultureInfo.InvariantCulture, $"                .ForMember(dest => dest.{propName}, opt => opt.MapFrom(src => ");
            sb.AppendLine(CultureInfo.InvariantCulture, $"                    string.IsNullOrEmpty(src.{propName}) ? Array.Empty<string>() : src.{propName}.Split('{separator}', StringSplitOptions.RemoveEmptyEntries)))");
        }

        private static void GenerateSplitListCreateToEntityMapping(StringBuilder sb, string columnName, string prefix)
        {
            var propName = GetPropertyName(columnName);
            var separator = prefix == "SPL" ? "|" : ",";

            sb.AppendLine(CultureInfo.InvariantCulture, $"                .ForMember(dest => dest.{propName}, opt => opt.MapFrom(src => ");
            sb.AppendLine(CultureInfo.InvariantCulture, $"                    src.{propName} != null ? string.Join(\"{separator}\", src.{propName}) : string.Empty))");
        }

        private static void GenerateSplitListUpdateToEntityMapping(StringBuilder sb, string columnName, string prefix)
        {
            var propName = GetPropertyName(columnName);
            var separator = prefix == "SPL" ? "|" : ",";

            sb.AppendLine(CultureInfo.InvariantCulture, $"                .ForMember(dest => dest.{propName}, opt => opt.MapFrom(src => ");
            sb.AppendLine(CultureInfo.InvariantCulture, $"                    src.{propName} != null ? string.Join(\"{separator}\", src.{propName}) : string.Empty))");
        }

        private static void GenerateEnoCreateToEntityMapping(StringBuilder sb, string baseName)
        {
            var propName = ToPascalCase(baseName);
            sb.AppendLine(CultureInfo.InvariantCulture, $"                .ForMember(dest => dest.{propName}Hashed, opt => opt.MapFrom(src => HashPassword(src.{propName}))) // TODO: Implement password hashing");
        }

        private static void GenerateEnoUpdateToEntityMapping(StringBuilder sb, string baseName)
        {
            var propName = ToPascalCase(baseName);
            sb.AppendLine(CultureInfo.InvariantCulture, $"                .ForMember(dest => dest.{propName}Hashed, opt => opt.MapFrom(src => HashPassword(src.{propName}))) // TODO: Implement password hashing");
        }
    }
}
