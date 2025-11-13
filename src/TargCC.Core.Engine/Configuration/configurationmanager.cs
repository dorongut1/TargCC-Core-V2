using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TargCC.Core.Configuration;

/// <summary>
/// Manages application configuration from multiple sources with encryption support.
/// Supports JSON files, environment variables, and in-memory configuration.
/// </summary>
public sealed class ConfigurationManager
{
    private readonly ILogger<ConfigurationManager> _logger;
    private IConfiguration? _configuration;
    private TargCCConfiguration? _targCCConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationManager"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic information.</param>
    public ConfigurationManager(ILogger<ConfigurationManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current TargCC configuration.
    /// </summary>
    public TargCCConfiguration Configuration => 
        _targCCConfig ?? throw new InvalidOperationException("Configuration not loaded. Call LoadConfiguration first.");

    /// <summary>
    /// Loads configuration from the specified JSON file and environment variables.
    /// </summary>
    /// <param name="configFilePath">Path to the JSON configuration file.</param>
    /// <param name="optional">Whether the config file is optional.</param>
    /// <returns>The loaded configuration.</returns>
    public TargCCConfiguration LoadConfiguration(string configFilePath = "appsettings.json", bool optional = false)
    {
        _logger.LogWarning("Raw Config Values:");

        _logger.LogInformation("Loading configuration from: {Path}", configFilePath);

        try
        {
            // Build configuration from multiple sources
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFilePath, optional: optional, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables(prefix: "TARGCC_");

            _configuration = builder.Build();

            // Bind to strongly-typed configuration
            _targCCConfig = _configuration.Get<TargCCConfiguration>() ?? new TargCCConfiguration();
            _configuration.Bind(_targCCConfig);

            // Decrypt sensitive data if needed
            if (_targCCConfig.Database.IsEncrypted && !string.IsNullOrEmpty(_targCCConfig.Security.EncryptionKey))
            {
                _targCCConfig.Database.ConnectionString = 
                    Decrypt(_targCCConfig.Database.ConnectionString, _targCCConfig.Security.EncryptionKey);
                _targCCConfig.Database.IsEncrypted = false; // Mark as decrypted in memory
            }

            // Validate configuration
            if (!optional || File.Exists(configFilePath))
            {
                ValidateConfiguration(_targCCConfig);
            }
            _logger.LogInformation("Configuration loaded successfully");
            return _targCCConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration from: {Path}", configFilePath);
            throw;
        }
    }

    /// <summary>
    /// Saves the current configuration to a JSON file.
    /// </summary>
    /// <param name="configFilePath">Path to save the configuration file.</param>
    /// <param name="encryptSensitiveData">Whether to encrypt sensitive data before saving.</param>
    public void SaveConfiguration(string configFilePath, bool encryptSensitiveData = true)
    {
        if (_targCCConfig == null)
            throw new InvalidOperationException("No configuration to save.");

        _logger.LogInformation("Saving configuration to: {Path}", configFilePath);

        try
        {
            // Clone the configuration to avoid modifying the in-memory version
            var configToSave = CloneConfiguration(_targCCConfig);

            // Encrypt sensitive data if needed
            if (encryptSensitiveData && !string.IsNullOrEmpty(configToSave.Security.EncryptionKey))
            {
                if (!configToSave.Database.IsEncrypted)
                {
                    configToSave.Database.ConnectionString = 
                        Encrypt(configToSave.Database.ConnectionString, configToSave.Security.EncryptionKey);
                    configToSave.Database.IsEncrypted = true;
                }
            }

            // Serialize to JSON with indentation
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(configToSave, options);
            File.WriteAllText(configFilePath, json);

            _logger.LogInformation("Configuration saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configuration to: {Path}", configFilePath);
            throw;
        }
    }

    /// <summary>
    /// Gets a configuration value by key.
    /// Supports nested keys using colon notation (e.g., "Database:ConnectionString").
    /// </summary>
    /// <param name="key">Configuration key.</param>
    /// <returns>Configuration value, or null if not found.</returns>
    public string? GetValue(string key)
    {
        return _configuration?[key];
    }

    /// <summary>
    /// Gets a configuration section.
    /// </summary>
    /// <param name="sectionName">Section name.</param>
    /// <returns>Configuration section.</returns>
    public IConfigurationSection? GetSection(string sectionName)
    {
        return _configuration?.GetSection(sectionName);
    }

    /// <summary>
    /// Updates a configuration value.
    /// Note: This only updates the in-memory configuration.
    /// Call SaveConfiguration to persist changes.
    /// </summary>
    /// <param name="key">Configuration key (supports nested keys with colon).</param>
    /// <param name="value">New value.</param>
    public void SetValue(string key, string value)
    {
        if (_targCCConfig == null)
            throw new InvalidOperationException("Configuration not loaded.");

        // Parse nested key
        var parts = key.Split(':');
        if (parts.Length == 2)
        {
            switch (parts[0].ToLower())
            {
                case "database":
                    SetDatabaseValue(parts[1], value);
                    break;
                case "plugins":
                    SetPluginValue(parts[1], value);
                    break;
                case "security":
                    SetSecurityValue(parts[1], value);
                    break;
            }
        }
        else if (parts.Length == 1)
        {
            _targCCConfig.AppSettings[key] = value;
        }

        _logger.LogDebug("Configuration value updated: {Key}", key);
    }

    /// <summary>
    /// Validates the configuration for required values and consistency.
    /// </summary>
    private void ValidateConfiguration(TargCCConfiguration config)
    {
        var errors = new List<string>();

        // Validate database
        if (string.IsNullOrWhiteSpace(config.Database.ConnectionString))
            errors.Add("Database connection string is required.");

        // Validate security
        if (config.Security.EnableEncryption && string.IsNullOrWhiteSpace(config.Security.EncryptionKey))
            errors.Add("Encryption key is required when encryption is enabled.");

        // Validate plugins
        if (string.IsNullOrWhiteSpace(config.Plugins.PluginDirectory))
            errors.Add("Plugin directory path is required.");

        if (errors.Any())
        {
            var message = "Configuration validation failed:\n" + string.Join("\n", errors);
            _logger.LogError(message);
            throw new InvalidOperationException(message);
        }

        _logger.LogDebug("Configuration validation passed");
    }

    /// <summary>
    /// Encrypts a string using AES encryption.
    /// </summary>
    private static string Encrypt(string plainText, string key)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = DeriveKey(key);
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        
        // Write IV first
        ms.Write(aes.IV, 0, aes.IV.Length);
        
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var writer = new StreamWriter(cs))
        {
            writer.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// Decrypts a string using AES encryption.
    /// </summary>
    private static string Decrypt(string cipherText, string key)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = DeriveKey(key);
        
        // Extract IV
        var iv = new byte[aes.IV.Length];
        Array.Copy(fullCipher, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs);
        
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Derives a 256-bit encryption key from the provided key string.
    /// </summary>
    private static byte[] DeriveKey(string key)
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(key));
    }

    /// <summary>
    /// Creates a deep clone of the configuration.
    /// </summary>
    private static TargCCConfiguration CloneConfiguration(TargCCConfiguration original)
    {
        var json = JsonSerializer.Serialize(original);
        return JsonSerializer.Deserialize<TargCCConfiguration>(json) 
            ?? throw new InvalidOperationException("Failed to clone configuration.");
    }

    private void SetDatabaseValue(string property, string value)
    {
        if (_targCCConfig == null) return;

        switch (property.ToLower())
        {
            case "connectionstring":
                _targCCConfig.Database.ConnectionString = value;
                break;
            case "provider":
                _targCCConfig.Database.Provider = value;
                break;
            case "commandtimeoutseconds":
                if (int.TryParse(value, out var timeout))
                    _targCCConfig.Database.CommandTimeoutSeconds = timeout;
                break;
        }
    }

    private void SetPluginValue(string property, string value)
    {
        if (_targCCConfig == null) return;

        switch (property.ToLower())
        {
            case "plugindirectory":
                _targCCConfig.Plugins.PluginDirectory = value;
                break;
            case "useisolation":
                if (bool.TryParse(value, out var useIsolation))
                    _targCCConfig.Plugins.UseIsolation = useIsolation;
                break;
            case "autoloadonstartup":
                if (bool.TryParse(value, out var autoLoad))
                    _targCCConfig.Plugins.AutoLoadOnStartup = autoLoad;
                break;
        }
    }

    private void SetSecurityValue(string property, string value)
    {
        if (_targCCConfig == null) return;

        switch (property.ToLower())
        {
            case "encryptionkey":
                _targCCConfig.Security.EncryptionKey = value;
                break;
            case "enableencryption":
                if (bool.TryParse(value, out var enable))
                    _targCCConfig.Security.EnableEncryption = enable;
                break;
            case "authenticationmode":
                _targCCConfig.Security.AuthenticationMode = value;
                break;
        }
    }
}
