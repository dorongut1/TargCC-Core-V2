using System;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TargCC.CLI.Configuration;
using TargCC.Core.Services.Metadata;

namespace TargCC.CLI.Commands.Metadata;

/// <summary>
/// Command for listing table metadata
/// </summary>
public class MetadataListCommand : Command
{
    public MetadataListCommand()
        : base("list", "List all table metadata")
    {
        var formatOption = new Option<string>(
            aliases: new[] { "--format", "-f" },
            description: "Output format (table, json, csv)",
            getDefaultValue: () => "table");

        var changedOnlyOption = new Option<bool>(
            aliases: new[] { "--changed", "-c" },
            description: "Show only tables with detected changes");

        AddOption(formatOption);
        AddOption(changedOnlyOption);

        this.SetHandler(ExecuteAsync, formatOption, changedOnlyOption);
    }

    private async Task<int> ExecuteAsync(string format, bool changedOnly)
    {
        try
        {
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
            var logger = loggerFactory.CreateLogger<MetadataService>();
            var metadataService = new MetadataService(config.ConnectionString, logger);

            // Get metadata
            var allMetadata = await metadataService.GetAllTableMetadataAsync();
            var metadata = allMetadata.ToList();

            if (changedOnly)
            {
                metadata = metadata.Where(m => m.SchemaHash != m.SchemaHashPrevious || m.SchemaHashPrevious == null).ToList();
            }

            if (!metadata.Any())
            {
                Console.WriteLine(changedOnly
                    ? "No tables with changes found"
                    : "No metadata found. Run 'targcc metadata sync' first.");
                return 0;
            }

            // Display in requested format
            switch (format.ToLower())
            {
                case "json":
                    DisplayAsJson(metadata);
                    break;

                case "csv":
                    DisplayAsCsv(metadata);
                    break;

                case "table":
                default:
                    DisplayAsTable(metadata, changedOnly);
                    break;
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            return 1;
        }
    }

    private void DisplayAsTable(System.Collections.Generic.List<TargCC.Core.Interfaces.Models.Metadata.TableMetadata> metadata, bool changedOnly)
    {
        Console.WriteLine();
        Console.WriteLine($"{"Table Name",-40} {"Schema",-10} {"Last Generated",-20} {"Changed",-10}");
        Console.WriteLine(new string('-', 82));

        foreach (var table in metadata.OrderBy(t => t.TableName))
        {
            var changed = table.SchemaHash != table.SchemaHashPrevious || table.SchemaHashPrevious == null;
            var changedIcon = changed ? "Yes ⚠️" : "No";
            var lastGen = table.LastGenerated?.ToString("yyyy-MM-dd HH:mm") ?? "Never";

            Console.WriteLine($"{table.TableName,-40} {table.SchemaName,-10} {lastGen,-20} {changedIcon,-10}");
        }

        Console.WriteLine();
        Console.WriteLine($"Total: {metadata.Count} tables");
    }

    private void DisplayAsJson(System.Collections.Generic.List<TargCC.Core.Interfaces.Models.Metadata.TableMetadata> metadata)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(metadata, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine(json);
    }

    private void DisplayAsCsv(System.Collections.Generic.List<TargCC.Core.Interfaces.Models.Metadata.TableMetadata> metadata)
    {
        Console.WriteLine("TableName,SchemaName,LastGenerated,SchemaHash,Changed");

        foreach (var table in metadata.OrderBy(t => t.TableName))
        {
            var changed = table.SchemaHash != table.SchemaHashPrevious || table.SchemaHashPrevious == null;
            var lastGen = table.LastGenerated?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";

            Console.WriteLine($"{table.TableName},{table.SchemaName},{lastGen},{table.SchemaHash},{changed}");
        }
    }
}
