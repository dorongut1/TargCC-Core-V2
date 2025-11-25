using TargCC.CLI.Configuration;

namespace TargCC.CLI.Tests.Configuration;

/// <summary>
/// Tests for <see cref="CliConfiguration"/>.
/// </summary>
public class CliConfigurationTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var config = new CliConfiguration();

        // Assert
        config.DefaultNamespace.Should().Be("MyApp");
        config.UseCleanArchitecture.Should().BeTrue();
        config.GenerateCqrs.Should().BeTrue();
        config.GenerateApiControllers.Should().BeTrue();
        config.GenerateRepositories.Should().BeTrue();
        config.GenerateStoredProcedures.Should().BeTrue();
        config.UseDapper.Should().BeTrue();
        config.GenerateValidators.Should().BeTrue();
        config.LogLevel.Should().Be("Information");
        config.Verbose.Should().BeFalse();
        config.IsInitialized.Should().BeFalse();
        config.Preferences.Should().NotBeNull();
        config.Preferences.Should().BeEmpty();
    }

    [Fact]
    public void ConnectionString_CanBeSet()
    {
        // Arrange
        var config = new CliConfiguration();
        var connectionString = "Server=localhost;Database=Test";

        // Act
        config.ConnectionString = connectionString;

        // Assert
        config.ConnectionString.Should().Be(connectionString);
    }

    [Fact]
    public void OutputDirectory_CanBeSet()
    {
        // Arrange
        var config = new CliConfiguration();
        var outputDir = @"C:\Output";

        // Act
        config.OutputDirectory = outputDir;

        // Assert
        config.OutputDirectory.Should().Be(outputDir);
    }

    [Fact]
    public void DefaultNamespace_CanBeChanged()
    {
        // Arrange
        var config = new CliConfiguration();
        var newNamespace = "CustomApp";

        // Act
        config.DefaultNamespace = newNamespace;

        // Assert
        config.DefaultNamespace.Should().Be(newNamespace);
    }

    [Fact]
    public void BooleanProperties_CanBeToggled()
    {
        // Arrange
        var config = new CliConfiguration();

        // Act
        config.UseCleanArchitecture = false;
        config.GenerateCqrs = false;
        config.GenerateApiControllers = false;

        // Assert
        config.UseCleanArchitecture.Should().BeFalse();
        config.GenerateCqrs.Should().BeFalse();
        config.GenerateApiControllers.Should().BeFalse();
    }

    [Fact]
    public void Preferences_CanBeModified()
    {
        // Arrange
        var config = new CliConfiguration();

        // Act
        config.Preferences["CustomKey"] = "CustomValue";
        config.Preferences["AnotherKey"] = "AnotherValue";

        // Assert
        config.Preferences.Should().HaveCount(2);
        config.Preferences["CustomKey"].Should().Be("CustomValue");
        config.Preferences["AnotherKey"].Should().Be("AnotherValue");
    }

    [Fact]
    public void IsInitialized_CanBeSet()
    {
        // Arrange
        var config = new CliConfiguration();

        // Act
        config.IsInitialized = true;

        // Assert
        config.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public void InitializationDate_CanBeSet()
    {
        // Arrange
        var config = new CliConfiguration();
        var date = DateTime.UtcNow;

        // Act
        config.InitializationDate = date;

        // Assert
        config.InitializationDate.Should().Be(date);
    }

    [Fact]
    public void LastUsedDatabase_CanBeSet()
    {
        // Arrange
        var config = new CliConfiguration();
        var database = "TestDatabase";

        // Act
        config.LastUsedDatabase = database;

        // Assert
        config.LastUsedDatabase.Should().Be(database);
    }

    [Fact]
    public void LogLevel_CanBeChanged()
    {
        // Arrange
        var config = new CliConfiguration();

        // Act
        config.LogLevel = "Debug";

        // Assert
        config.LogLevel.Should().Be("Debug");
    }

    [Fact]
    public void Verbose_CanBeEnabled()
    {
        // Arrange
        var config = new CliConfiguration();

        // Act
        config.Verbose = true;

        // Assert
        config.Verbose.Should().BeTrue();
    }
}
