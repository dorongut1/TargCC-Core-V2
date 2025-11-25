using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TargCC.CLI.Configuration;

/// <summary>
/// Service for managing CLI configuration.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private static readonly string ConfigDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".targcc");

    private static readonly string ConfigFileName = "config.json";

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Ensure config directory exists
        if (!Directory.Exists(ConfigDirectory))
        {
            Directory.CreateDirectory(ConfigDirectory);
            _logger.LogInformation("Created configuration directory: {Directory}", ConfigDirectory);
        }
    }

    /// <inheritdoc />
    public string ConfigFilePath => Path.Combine(ConfigDirectory, ConfigFileName);

    /// <inheritdoc />
    public async Task<CliConfiguration> LoadAsync()
    {
        try
        {
            if (!File.Exists(ConfigFilePath))
            {
                _logger.LogWarning("Configuration file not found, creating default configuration");
                return await InitializeAsync();
            }

            var json = await File.ReadAllTextAsync(ConfigFilePath);
            var config = JsonSerializer.Deserialize<CliConfiguration>(json, GetJsonOptions());

            if (config == null)
            {
                _logger.LogWarning("Failed to deserialize configuration, creating default");
                return await InitializeAsync();
            }

            _logger.LogDebug("Configuration loaded successfully");
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(CliConfiguration configuration)
    {
        try
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var json = JsonSerializer.Serialize(configuration, GetJsonOptions());
            await File.WriteAllTextAsync(ConfigFilePath, json);

            _logger.LogInformation("Configuration saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configuration");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetValueAsync(string key)
    {
        var config = await LoadAsync();
        var property = typeof(CliConfiguration).GetProperty(key);

        if (property == null)
        {
            // Try preferences dictionary
            if (config.Preferences.TryGetValue(key, out var value))
            {
                return value;
            }

            _logger.LogWarning("Configuration key not found: {Key}", key);
            return null;
        }

        var propertyValue = property.GetValue(config);
        return propertyValue?.ToString();
    }

    /// <inheritdoc />
    public async Task SetValueAsync(string key, string value)
    {
        var config = await LoadAsync();
        var property = typeof(CliConfiguration).GetProperty(key);

        if (property == null)
        {
            // Store in preferences dictionary
            config.Preferences[key] = value;
            _logger.LogInformation("Set preference: {Key} = {Value}", key, value);
        }
        else
        {
            // Set property value
            var targetType = property.PropertyType;
            object? convertedValue;

            if (targetType == typeof(string))
            {
                convertedValue = value;
            }
            else if (targetType == typeof(bool))
            {
                convertedValue = bool.Parse(value);
            }
            else if (targetType == typeof(int))
            {
                convertedValue = int.Parse(value);
            }
            else
            {
                throw new NotSupportedException($"Property type {targetType.Name} is not supported");
            }

            property.SetValue(config, convertedValue);
            _logger.LogInformation("Set configuration: {Key} = {Value}", key, value);
        }

        await SaveAsync(config);
    }

    /// <inheritdoc />
    public bool Exists()
    {
        return File.Exists(ConfigFilePath);
    }

    /// <inheritdoc />
    public async Task<CliConfiguration> InitializeAsync()
    {
        var config = new CliConfiguration
        {
            IsInitialized = true,
            InitializationDate = DateTime.UtcNow
        };

        await SaveAsync(config);
        _logger.LogInformation("Configuration initialized at: {Path}", ConfigFilePath);

        return config;
    }

    /// <inheritdoc />
    public async Task ResetAsync()
    {
        if (File.Exists(ConfigFilePath))
        {
            File.Delete(ConfigFilePath);
            _logger.LogInformation("Configuration file deleted");
        }

        await InitializeAsync();
        _logger.LogInformation("Configuration reset to defaults");
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
}
