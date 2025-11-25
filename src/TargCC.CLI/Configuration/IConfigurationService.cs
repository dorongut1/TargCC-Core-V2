namespace TargCC.CLI.Configuration;

/// <summary>
/// Service for managing CLI configuration.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets the configuration file path.
    /// </summary>
    string ConfigFilePath { get; }

    /// <summary>
    /// Loads the configuration.
    /// </summary>
    /// <returns>Configuration instance.</returns>
    Task<CliConfiguration> LoadAsync();

    /// <summary>
    /// Saves the configuration.
    /// </summary>
    /// <param name="configuration">Configuration to save.</param>
    Task SaveAsync(CliConfiguration configuration);

    /// <summary>
    /// Gets a configuration value.
    /// </summary>
    /// <param name="key">Configuration key.</param>
    /// <returns>Configuration value or null if not found.</returns>
    Task<string?> GetValueAsync(string key);

    /// <summary>
    /// Sets a configuration value.
    /// </summary>
    /// <param name="key">Configuration key.</param>
    /// <param name="value">Configuration value.</param>
    Task SetValueAsync(string key, string value);

    /// <summary>
    /// Checks if configuration file exists.
    /// </summary>
    /// <returns>True if exists, false otherwise.</returns>
    bool Exists();

    /// <summary>
    /// Initializes a new configuration file.
    /// </summary>
    /// <returns>New configuration instance.</returns>
    Task<CliConfiguration> InitializeAsync();

    /// <summary>
    /// Resets configuration to defaults.
    /// </summary>
    Task ResetAsync();
}
