using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Configuration;
using Xunit;

namespace TargCC.Core.Tests.Unit.Engine;

/// <summary>
/// Unit tests for ConfigurationManager.
/// </summary>
public sealed class ConfigurationManagerTests : IDisposable
{
    private readonly Mock<ILogger<ConfigurationManager>> _mockLogger;
    private readonly ConfigurationManager _configManager;
    private readonly string _testConfigDirectory;

    public ConfigurationManagerTests()
    {
        _mockLogger = new Mock<ILogger<ConfigurationManager>>();
        _configManager = new ConfigurationManager(_mockLogger.Object);
        
        // Create temporary test directory
        _testConfigDirectory = Path.Combine(Path.GetTempPath(), $"TargCC_Test_Config_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testConfigDirectory);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ConfigurationManager(null!));
    }

    [Fact]
    public void Configuration_BeforeLoad_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _configManager.Configuration);
    }

    [Fact]
    public void LoadConfiguration_WithValidJson_LoadsSuccessfully()
    {
        // Arrange
        var configPath = Path.Combine(_testConfigDirectory, "appsettings.json");
        var json = @"{
  ""database"": {
    ""connectionString"": ""Server=localhost;Database=Test;"",
    ""provider"": ""SqlServer""
  },
  ""security"": {
    ""encryptionKey"": ""TestKey123"",
    ""enableEncryption"": false
  },
  ""plugins"": {
    ""pluginDirectory"": ""plugins""
  }
}";
        File.WriteAllText(configPath, json);

        // Act
        var config = _configManager.LoadConfiguration(configPath);

        // Assert
        Assert.NotNull(config);
        Assert.Equal("Server=localhost;Database=Test;", config.Database.ConnectionString);
        Assert.Equal("SqlServer", config.Database.Provider);
        Assert.Equal("plugins", config.Plugins.PluginDirectory);
    }

    [Fact]
    public void LoadConfiguration_WithMissingFile_ThrowsException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testConfigDirectory, "nonexistent.json");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => _configManager.LoadConfiguration(nonExistentPath, optional: false));
    }

    [Fact]
    public void LoadConfiguration_WithOptionalMissingFile_LoadsWithDefaults()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testConfigDirectory, "nonexistent.json");

        // Act
        var config = _configManager.LoadConfiguration(nonExistentPath, optional: true);

        // Assert - When file doesn't exist and is optional, we get default values (no validation)
        Assert.NotNull(config);
        Assert.NotNull(config.Database);
        Assert.NotNull(config.Plugins);
        Assert.NotNull(config.Security);
        Assert.NotNull(config.Logging);
        
        // These will be empty/default but that's OK when file is missing and optional
        Assert.Equal(string.Empty, config.Database.ConnectionString);
        Assert.Equal("plugins", config.Plugins.PluginDirectory);
        Assert.Equal("SqlServer", config.Database.Provider);
        Assert.True(config.Security.EnableEncryption);            

        // Verify Configuration property is accessible
        Assert.Same(config, _configManager.Configuration);
    }

    [Fact]
    public void LoadConfiguration_WithMissingConnectionString_ThrowsException()
    {
        // Arrange
        var configPath = Path.Combine(_testConfigDirectory, "invalid.json");
        var json = @"{
  ""database"": {
    ""connectionString"": """"
  },
  ""security"": {
    ""encryptionKey"": ""TestKey123""
  },
  ""plugins"": {
    ""pluginDirectory"": ""plugins""
  }
}";
        File.WriteAllText(configPath, json);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _configManager.LoadConfiguration(configPath));
    }

    [Fact]
    public void SaveConfiguration_AfterLoad_SavesSuccessfully()
    {
        // Arrange
        var loadPath = Path.Combine(_testConfigDirectory, "load.json");
        var savePath = Path.Combine(_testConfigDirectory, "save.json");
        
        var json = @"{
  ""database"": {
    ""connectionString"": ""Server=localhost;Database=Test;"",
    ""provider"": ""SqlServer""
  },
  ""security"": {
    ""encryptionKey"": ""TestKey123"",
    ""enableEncryption"": false
  },
  ""plugins"": {
    ""pluginDirectory"": ""plugins""
  }
}";
        File.WriteAllText(loadPath, json);
        _configManager.LoadConfiguration(loadPath);

        // Act
        _configManager.SaveConfiguration(savePath, encryptSensitiveData: false);

        // Assert
        Assert.True(File.Exists(savePath));
        var savedContent = File.ReadAllText(savePath);
        Assert.Contains("Server=localhost;Database=Test;", savedContent);
    }

    [Fact]
    public void SaveConfiguration_BeforeLoad_ThrowsInvalidOperationException()
    {
        // Arrange
        var savePath = Path.Combine(_testConfigDirectory, "save.json");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _configManager.SaveConfiguration(savePath));
    }

    [Fact]
    public void GetValue_WithValidKey_ReturnsValue()
    {
        // Arrange
        var configPath = Path.Combine(_testConfigDirectory, "appsettings.json");
        var json = @"{
  ""database"": {
    ""connectionString"": ""Server=localhost;Database=Test;"",
    ""provider"": ""SqlServer""
  },
  ""security"": {
    ""encryptionKey"": ""TestKey123""
  },
  ""plugins"": {
    ""pluginDirectory"": ""plugins""
  }
}";
        File.WriteAllText(configPath, json);
        _configManager.LoadConfiguration(configPath);

        // Act
        var value = _configManager.GetValue("database:provider");

        // Assert
        Assert.Equal("SqlServer", value);
    }

    [Fact]
    public void GetValue_WithInvalidKey_ReturnsNull()
    {
        // Arrange
        var configPath = Path.Combine(_testConfigDirectory, "appsettings.json");
        var json = @"{
  ""database"": {
    ""connectionString"": ""Server=localhost;Database=Test;""
  },
  ""security"": {
    ""encryptionKey"": ""TestKey123""
  },
  ""plugins"": {
    ""pluginDirectory"": ""plugins""
  }
}";
        File.WriteAllText(configPath, json);
        _configManager.LoadConfiguration(configPath);

        // Act
        var value = _configManager.GetValue("nonexistent:key");

        // Assert
        Assert.Null(value);
    }

    [Fact]
    public void SetValue_WithDatabaseKey_UpdatesValue()
    {
        // Arrange
        var configPath = Path.Combine(_testConfigDirectory, "appsettings.json");
        var json = @"{
  ""database"": {
    ""connectionString"": ""Server=localhost;Database=Test;"",
    ""provider"": ""SqlServer""
  },
  ""security"": {
    ""encryptionKey"": ""TestKey123""
  },
  ""plugins"": {
    ""pluginDirectory"": ""plugins""
  }
}";
        File.WriteAllText(configPath, json);
        _configManager.LoadConfiguration(configPath);

        // Act
        _configManager.SetValue("database:provider", "PostgreSQL");

        // Assert
        Assert.Equal("PostgreSQL", _configManager.Configuration.Database.Provider);
    }

    [Fact]
    public void SetValue_BeforeLoad_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            _configManager.SetValue("database:provider", "PostgreSQL"));
    }

    [Fact]
    public void LoadConfiguration_WithEncryptedConnectionString_DecryptsSuccessfully()
    {
        // Arrange
        var configPath = Path.Combine(_testConfigDirectory, "encrypted.json");
        
        // First, create and save with encryption
        var tempPath = Path.Combine(_testConfigDirectory, "temp.json");
        var json = @"{
  ""database"": {
    ""connectionString"": ""Server=localhost;Database=Test;"",
    ""provider"": ""SqlServer"",
    ""isEncrypted"": false
  },
  ""security"": {
    ""encryptionKey"": ""MySecretKey123"",
    ""enableEncryption"": true
  },
  ""plugins"": {
    ""pluginDirectory"": ""plugins""
  }
}";
        File.WriteAllText(tempPath, json);
        
        var tempManager = new ConfigurationManager(_mockLogger.Object);
        tempManager.LoadConfiguration(tempPath);
        tempManager.SaveConfiguration(configPath, encryptSensitiveData: true);

        // Act - Load the encrypted config
        var config = _configManager.LoadConfiguration(configPath);

        // Assert
        Assert.Equal("Server=localhost;Database=Test;", config.Database.ConnectionString);
        Assert.False(config.Database.IsEncrypted); // Should be decrypted in memory
    }

    public void Dispose()
    {
        // Cleanup test directory
        try
        {
            if (Directory.Exists(_testConfigDirectory))
            {
                Directory.Delete(_testConfigDirectory, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
