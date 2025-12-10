using System;
using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using TargCC.Core.Services.Metadata;

namespace TargCC.CLI.Commands.Metadata;

/// <summary>
/// Command for syncing metadata from database schema
/// </summary>
public class MetadataSyncCommand : Command
{
    public MetadataSyncCommand()
        : base("sync", "Sync metadata from database schema")
    {
        var databaseOption = new Option<string?>(
            aliases: new[] { "--database", "-d" },
            description: "Database name (uses default from config if not specified)");

        AddOption(databaseOption);

        this.SetHandler(ExecuteAsync, databaseOption);
    }

    private async Task<int> ExecuteAsync(string? database)
    {
        try
        {
            Console.WriteLine("🔄 Syncing metadata from database...");

            // Load configuration
            var configService = new ConfigurationService();
            var config = await configService.LoadAsync();

            if (config == null || string.IsNullOrWhiteSpace(config.ConnectionString))
            {
                Console.WriteLine("❌ Error: No connection string found. Run 'targcc init' first.");
                return 1;
            }

            // Create services
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<MetadataService>();
            var metadataService = new MetadataService(config.ConnectionString, logger);

            // Sync metadata
            var success = await metadataService.SyncMetadataFromDatabaseAsync(config.ConnectionString);

            if (success)
            {
                Console.WriteLine("✅ Metadata sync completed successfully!");

                // Get changed tables count
                var changedCount = await metadataService.GetChangedTablesCountAsync();
                if (changedCount > 0)
                {
                    Console.WriteLine($"⚠️  {changedCount} table(s) have schema changes");
                    Console.WriteLine($"   Run 'targcc metadata diff' to see details");
                }
                else
                {
                    Console.WriteLine("✨ All tables are up to date");
                }

                return 0;
            }
            else
            {
                Console.WriteLine("❌ Metadata sync failed");
                return 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            return 1;
        }
    }
}
