using System.Text.Json.Serialization;

namespace TargCC.CLI.Configuration;

/// <summary>
/// CLI configuration settings.
/// </summary>
public class CliConfiguration
{
    /// <summary>
    /// Gets or sets the default database connection string.
    /// </summary>
    [JsonPropertyName("connectionString")]
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the default output directory.
    /// </summary>
    [JsonPropertyName("outputDirectory")]
    public string? OutputDirectory { get; set; }

    /// <summary>
    /// Gets or sets the default namespace.
    /// </summary>
    [JsonPropertyName("defaultNamespace")]
    public string DefaultNamespace { get; set; } = "MyApp";

    /// <summary>
    /// Gets or sets whether to use Clean Architecture structure.
    /// </summary>
    [JsonPropertyName("useCleanArchitecture")]
    public bool UseCleanArchitecture { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate CQRS patterns.
    /// </summary>
    [JsonPropertyName("generateCqrs")]
    public bool GenerateCqrs { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate API controllers.
    /// </summary>
    [JsonPropertyName("generateApiControllers")]
    public bool GenerateApiControllers { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate repositories.
    /// </summary>
    [JsonPropertyName("generateRepositories")]
    public bool GenerateRepositories { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate stored procedures.
    /// </summary>
    [JsonPropertyName("generateStoredProcedures")]
    public bool GenerateStoredProcedures { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use Dapper for data access.
    /// </summary>
    [JsonPropertyName("useDapper")]
    public bool UseDapper { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate FluentValidation validators.
    /// </summary>
    [JsonPropertyName("generateValidators")]
    public bool GenerateValidators { get; set; } = true;

    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    [JsonPropertyName("logLevel")]
    public string LogLevel { get; set; } = "Information";

    /// <summary>
    /// Gets or sets whether to show verbose output.
    /// </summary>
    [JsonPropertyName("verbose")]
    public bool Verbose { get; set; }

    /// <summary>
    /// Gets or sets the last used database.
    /// </summary>
    [JsonPropertyName("lastUsedDatabase")]
    public string? LastUsedDatabase { get; set; }

    /// <summary>
    /// Gets or sets whether CLI was initialized.
    /// </summary>
    [JsonPropertyName("isInitialized")]
    public bool IsInitialized { get; set; }

    /// <summary>
    /// Gets or sets the initialization date.
    /// </summary>
    [JsonPropertyName("initializationDate")]
    public DateTime? InitializationDate { get; set; }

    /// <summary>
    /// Gets or sets user preferences.
    /// </summary>
    [JsonPropertyName("preferences")]
    public Dictionary<string, string> Preferences { get; set; } = new();
}
