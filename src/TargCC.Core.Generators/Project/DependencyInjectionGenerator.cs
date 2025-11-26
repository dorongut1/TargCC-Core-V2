using System.Globalization;
using System.Linq;
using System.Text;
using TargCC.Core.Generators.Project.Models;

namespace TargCC.Core.Generators.Project;

/// <summary>
/// Generates Dependency Injection registration code for Clean Architecture layers.
/// </summary>
public class DependencyInjectionGenerator : IDependencyInjectionGenerator
{
    /// <inheritdoc/>
    public string GenerateApplicationDI(ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(projectInfo);

        var sb = new StringBuilder();

        // Using statements
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using MediatR;");
        sb.AppendLine("using System.Reflection;");
        sb.AppendLine();

        // Namespace
        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {projectInfo.Namespace}.Application;");
        sb.AppendLine();

        // Class
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Dependency injection configuration for Application layer.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class DependencyInjection");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Adds Application layer services to the DI container.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"services\">The service collection.</param>");
        sb.AppendLine("    /// <returns>The service collection for chaining.</returns>");
        sb.AppendLine("    public static IServiceCollection AddApplicationServices(this IServiceCollection services)");
        sb.AppendLine("    {");
        sb.AppendLine("        // MediatR");
        sb.AppendLine("        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));");
        sb.AppendLine();
        sb.AppendLine("        // FluentValidation (if used)");
        sb.AppendLine("        // services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());");
        sb.AppendLine();
        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <inheritdoc/>
    public string GenerateInfrastructureDI(ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(projectInfo);

        var sb = new StringBuilder();

        // Using statements
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {projectInfo.Namespace}.Domain.Interfaces;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {projectInfo.Namespace}.Infrastructure.Repositories;");
        sb.AppendLine();

        // Namespace
        sb.AppendLine(CultureInfo.InvariantCulture, $"namespace {projectInfo.Namespace}.Infrastructure;");
        sb.AppendLine();

        // Class
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Dependency injection configuration for Infrastructure layer.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class DependencyInjection");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Adds Infrastructure layer services to the DI container.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"services\">The service collection.</param>");
        sb.AppendLine("    /// <returns>The service collection for chaining.</returns>");
        sb.AppendLine("    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)");
        sb.AppendLine("    {");
        sb.AppendLine("        // Register repositories");

        // Register each table's repository
        if (projectInfo.Tables != null && projectInfo.Tables.Count > 0)
        {
            foreach (var tableName in from table in projectInfo.Tables
                                      let tableName = table.Name
                                      select tableName)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        services.AddScoped<I{tableName}Repository, {tableName}Repository>();");
            }
        }
        else
        {
            sb.AppendLine("        // TODO: Register your repositories here");
            sb.AppendLine("        // services.AddScoped<IYourEntityRepository, YourEntityRepository>();");
        }

        sb.AppendLine();
        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
