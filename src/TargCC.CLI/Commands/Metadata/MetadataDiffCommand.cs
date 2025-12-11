using System;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using TargCC.Core.Analyzers.Database;
using TargCC.Core.Services.Metadata;

namespace TargCC.CLI.Commands.Metadata;

/// <summary>
/// Command for showing metadata differences (what changed)
/// </summary>
public class MetadataDiffCommand : Command
{
    public MetadataDiffCommand()
        : base("diff", "Show schema changes detected by metadata system")
    {
        var databaseOption = new Option<string?>(
            aliases: new[] { "--database", "-d" },
            description: "Database name (uses default from config if not specified)");

        var verboseOption = new Option<bool>(
            aliases: new[] { "--verbose", "-v" },
            description: "Show detailed change information");

        AddOption(databaseOption);
        AddOption(verboseOption);

        this.SetHandler(ExecuteAsync, databaseOption, verboseOption);
    }

    private async Task<int> ExecuteAsync(string? database, bool verbose)
    {
        try
        {
            Console.WriteLine("🔍 Analyzing schema changes...");
            Console.WriteLine();

            // Create logger factory
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

            // Load configuration
            var configLogger = loggerFactory.CreateLogger<ConfigurationService>();
            var configService = new ConfigurationService(configLogger);
            var config = await configService.LoadAsync();

            if (config == null || string.IsNullOrWhiteSpace(config.ConnectionString))
            {
                Console.WriteLine("❌ Error: No connection string found. Run 'targcc init' first.");
                return 1;
            }

            // Create services
            var metadataLogger = loggerFactory.CreateLogger<MetadataService>();
            var changeLogger = loggerFactory.CreateLogger<ChangeDetectionService>();
            var incrementalLogger = loggerFactory.CreateLogger<IncrementalGenerationService>();

            var metadataService = new MetadataService(config.ConnectionString, metadataLogger);
            var changeDetectionService = new ChangeDetectionService(changeLogger);
            var incrementalService = new IncrementalGenerationService(metadataService, changeDetectionService, incrementalLogger);

            // Analyze database
            var analyzerLogger = loggerFactory.CreateLogger<DatabaseAnalyzer>();
            var analyzer = new DatabaseAnalyzer(config.ConnectionString, analyzerLogger);
            var schema = await analyzer.AnalyzeAsync();

            // Get changes summary
            var summary = await incrementalService.GetChangesSummaryAsync(schema);

            // Display results
            Console.WriteLine($"📊 Summary:");
            Console.WriteLine($"   Total Tables:     {summary.TotalTables}");
            Console.WriteLine($"   New Tables:       {summary.NewTables} 🆕");
            Console.WriteLine($"   Modified Tables:  {summary.ModifiedTables} 📝");
            Console.WriteLine($"   Unchanged Tables: {summary.UnchangedTables} ✓");
            Console.WriteLine();

            if (summary.NewTables > 0)
            {
                Console.WriteLine("🆕 New Tables:");
                foreach (var tableName in summary.NewTableNames)
                {
                    Console.WriteLine($"   + {tableName}");
                }
                Console.WriteLine();
            }

            if (summary.ModifiedTables > 0)
            {
                Console.WriteLine("📝 Modified Tables:");
                foreach (var tableName in summary.ModifiedTableNames)
                {
                    Console.WriteLine($"   ~ {tableName}");
                }
                Console.WriteLine();
            }

            if (summary.NewTables + summary.ModifiedTables > 0)
            {
                Console.WriteLine("💡 Tip: Run 'targcc generate incremental' to regenerate changed tables only");
            }
            else
            {
                Console.WriteLine("✨ All tables are up to date - no changes detected");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            return 1;
        }
    }
}
