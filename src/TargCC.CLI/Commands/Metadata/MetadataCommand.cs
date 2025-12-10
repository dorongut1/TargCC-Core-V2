using System.CommandLine;

namespace TargCC.CLI.Commands.Metadata;

/// <summary>
/// Root command for metadata management operations
/// </summary>
public class MetadataCommand : Command
{
    public MetadataCommand()
        : base("metadata", "Manage database metadata and change detection")
    {
        AddCommand(new MetadataSyncCommand());
        AddCommand(new MetadataDiffCommand());
        AddCommand(new MetadataListCommand());
    }
}
