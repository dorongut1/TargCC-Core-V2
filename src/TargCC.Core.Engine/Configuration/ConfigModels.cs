namespace TargCC.Core.Configuration;

/// <summary>
/// Root configuration model for TargCC.
/// </summary>
public sealed class TargCCConfiguration
{
    /// <summary>
    /// Gets or sets the database configuration.
    /// </summary>
    public DatabaseConfiguration Database { get; set; } = new ();

    /// <summary>
    /// Gets or sets the plugin configuration.
    /// </summary>
    public PluginConfiguration Plugins { get; set; } = new ();

    /// <summary>
    /// Gets or sets the logging configuration.
    /// </summary>
    public LoggingConfiguration Logging { get; set; } = new ();

    /// <summary>
    /// Gets or sets the security configuration.
    /// </summary>
    public SecurityConfiguration Security { get; set; } = new ();

    /// <summary>
    /// Gets or sets custom application settings.
    /// </summary>
    public Dictionary<string, string> AppSettings { get; set; } = new ();
}

/// <summary>
/// Database connection and settings configuration.
/// </summary>
public sealed class DatabaseConfiguration
{
    /// <summary>
    /// Gets or sets the connection string.
    /// Can be encrypted using the encryption key.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the connection string is encrypted.
    /// </summary>
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// Gets or sets the database provider (SqlServer, PostgreSQL, etc.).
    /// </summary>
    public string Provider { get; set; } = "SqlServer";

    /// <summary>
    /// Gets or sets the command timeout in seconds.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets a value indicating whether to enable connection pooling.
    /// </summary>
    public bool EnablePooling { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum pool size.
    /// </summary>
    public int MinPoolSize { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum pool size.
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;
}

/// <summary>
/// Plugin loading and management configuration.
/// </summary>
public sealed class PluginConfiguration
{
    /// <summary>
    /// Gets or sets the directory where plugins are located.
    /// </summary>
    public string PluginDirectory { get; set; } = "plugins";

    /// <summary>
    /// Gets or sets a value indicating whether to use plugin isolation.
    /// </summary>
    public bool UseIsolation { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to auto-load plugins on startup.
    /// </summary>
    public bool AutoLoadOnStartup { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of plugins to load.
    /// Empty list means load all discovered plugins.
    /// </summary>
    public List<string> PluginsToLoad { get; set; } = new ();

    /// <summary>
    /// Gets or sets the list of plugins to exclude from loading.
    /// </summary>
    public List<string> PluginsToExclude { get; set; } = new ();
}

/// <summary>
/// Logging configuration.
/// </summary>
public sealed class LoggingConfiguration
{
    /// <summary>
    /// Gets or sets the minimum log level.
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// Gets or sets a value indicating whether to log to console.
    /// </summary>
    public bool LogToConsole { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to log to file.
    /// </summary>
    public bool LogToFile { get; set; } = true;

    /// <summary>
    /// Gets or sets the log file path.
    /// </summary>
    public string LogFilePath { get; set; } = "logs/targcc.log";

    /// <summary>
    /// Gets or sets a value indicating whether to log to database.
    /// </summary>
    public bool LogToDatabase { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum size of log files in MB before rotation.
    /// </summary>
    public int MaxLogFileSizeMB { get; set; } = 10;

    /// <summary>
    /// Gets or sets the number of log files to retain.
    /// </summary>
    public int RetainedFileCount { get; set; } = 7;
}

/// <summary>
/// Security and encryption configuration.
/// </summary>
public sealed class SecurityConfiguration
{
    /// <summary>
    /// Gets or sets the encryption key for sensitive data.
    /// Should be stored securely (e.g., in user secrets or key vault).
    /// </summary>
    public string EncryptionKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to enable encryption.
    /// </summary>
    public bool EnableEncryption { get; set; } = true;

    /// <summary>
    /// Gets or sets the authentication mode.
    /// </summary>
    public string AuthenticationMode { get; set; } = "Windows";

    /// <summary>
    /// Gets or sets a value indicating whether to require HTTPS.
    /// </summary>
    public bool RequireHttps { get; set; } = true;

    /// <summary>
    /// Gets or sets the allowed origins for CORS.
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = new ();
}
