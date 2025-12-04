using System.Globalization;
using System.Text;
using TargCC.Core.Generators.Project.Models;

namespace TargCC.Core.Generators.Project;

/// <summary>
/// Generates Program.cs file for ASP.NET Core applications with Clean Architecture setup.
/// </summary>
public class ProgramCsGenerator : IProgramCsGenerator
{
    /// <inheritdoc/>
    public string Generate(ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(projectInfo);

        var sb = new StringBuilder();

        // Using statements
        sb.AppendLine("using Microsoft.AspNetCore.Builder;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Microsoft.Extensions.Hosting;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {projectInfo.Namespace}.Application;");
        sb.AppendLine(CultureInfo.InvariantCulture, $"using {projectInfo.Namespace}.Infrastructure;");
        sb.AppendLine();

        // Builder creation
        sb.AppendLine("var builder = WebApplication.CreateBuilder(args);");
        sb.AppendLine();

        // Service registration
        sb.AppendLine("// Add services to the container");
        sb.AppendLine("builder.Services.AddControllers();");
        sb.AppendLine("builder.Services.AddEndpointsApiExplorer();");
        sb.AppendLine("builder.Services.AddSwaggerGen();");
        sb.AppendLine();

        // Application services
        sb.AppendLine("// Application layer");
        sb.AppendLine("builder.Services.AddApplicationServices();");
        sb.AppendLine();

        // Infrastructure services
        sb.AppendLine("// Infrastructure layer");
        sb.AppendLine("builder.Services.AddInfrastructureServices();");
        sb.AppendLine();

        // CORS (optional)
        sb.AppendLine("// CORS");
        sb.AppendLine("builder.Services.AddCors(options =>");
        sb.AppendLine("{");
        sb.AppendLine("    options.AddDefaultPolicy(policy =>");
        sb.AppendLine("    {");
        sb.AppendLine("        policy.AllowAnyOrigin()");
        sb.AppendLine("              .AllowAnyMethod()");
        sb.AppendLine("              .AllowAnyHeader();");
        sb.AppendLine("    });");
        sb.AppendLine("});");
        sb.AppendLine();

        // Build app
        sb.AppendLine("var app = builder.Build();");
        sb.AppendLine();

        // Configure middleware
        sb.AppendLine("// Configure the HTTP request pipeline");
        sb.AppendLine("app.UseSwagger();");
        sb.AppendLine("app.UseSwaggerUI();");
        sb.AppendLine();

        sb.AppendLine("app.UseHttpsRedirection();");
        sb.AppendLine("app.UseCors();");
        sb.AppendLine("app.UseAuthorization();");
        sb.AppendLine("app.MapControllers();");
        sb.AppendLine();

        sb.AppendLine("app.Run();");

        return sb.ToString();
    }
}
