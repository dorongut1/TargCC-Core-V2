namespace TargCC.Core.Tests.Unit.Generators.Data;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Generators.Data;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Tests.TestHelpers;
using Xunit;

/// <summary>
/// Tests for DbContextGenerator to ensure correct DbContext generation.
/// </summary>
public class DbContextGeneratorTests
{
    private readonly Mock<ILogger<DbContextGenerator>> _mockLogger;
    private readonly DbContextGenerator _generator;

    public DbContextGeneratorTests()
    {
        _mockLogger = new Mock<ILogger<DbContextGenerator>>();
        _generator = new DbContextGenerator(_mockLogger.Object);
    }

    /// <summary>
    /// Test 1: Constructor validates required dependencies.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new DbContextGenerator(null!));

        exception.ParamName.Should().Be("logger");
    }

    /// <summary>
    /// Test 2: GenerateAsync validates null schema parameter.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithNullSchema_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _generator.GenerateAsync(null!));

        exception.ParamName.Should().Be("schema");
    }

    /// <summary>
    /// Test 3: GenerateAsync validates schema has tables.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithNoTables_ThrowsInvalidOperationException()
    {
        // Arrange
        var schema = new DatabaseSchema
        {
            DatabaseName = "TestDb",
            Tables = new List<Table>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _generator.GenerateAsync(schema));

        exception.Message.Should().Contain("must contain at least one table");
    }

    /// <summary>
    /// Test 4: Generates basic DbContext with DbSet properties.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithSimpleSchema_GeneratesDbContextWithDbSets()
    {
        // Arrange
        var schema = new DatabaseSchema
        {
            DatabaseName = "TestDb",
            Tables = new List<Table>
            {
                new TableBuilder()
                    .WithName("Customer")
                    .WithColumn("ID", "int", isPrimaryKey: true)
                    .Build(),
                new TableBuilder()
                    .WithName("Order")
                    .WithColumn("ID", "int", isPrimaryKey: true)
                    .Build()
            }
        };

        // Act
        var result = await _generator.GenerateAsync(schema);

        // Assert
        result.Should().NotBeNullOrEmpty();

        // Class declaration
        result.Should().Contain("public class ApplicationDbContext : DbContext");

        // DbSet properties (pluralized)
        result.Should().Contain("public DbSet<Customer> Customers { get; set; } = null!;");
        result.Should().Contain("public DbSet<Order> Orders { get; set; } = null!;");

        // Constructor
        result.Should().Contain("public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)");
        result.Should().Contain(": base(options)");

        // OnModelCreating
        result.Should().Contain("protected override void OnModelCreating(ModelBuilder modelBuilder)");
        result.Should().Contain("modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());");

        // Namespace and usings
        result.Should().Contain("namespace TargCC.Infrastructure.Data;");
        result.Should().Contain("using Microsoft.EntityFrameworkCore;");
        result.Should().Contain("using TargCC.Domain.Entities;");
        result.Should().Contain("using System.Reflection;");
    }

    /// <summary>
    /// Test 5: Pluralizes DbSet names correctly.
    /// </summary>
    [Theory]
    [InlineData("Customer", "Customers")]
    [InlineData("Order", "Orders")]
    [InlineData("Category", "Categories")]  // y → ies
    [InlineData("Address", "Addresses")]    // ss → sses
    [InlineData("Box", "Boxes")]            // x → xes
    [InlineData("Batch", "Batches")]        // ch → ches
    [InlineData("Wish", "Wishes")]          // sh → shes
    [InlineData("Quiz", "Quizzes")]         // z → zes (should be handled)
    public async Task GenerateAsync_PluralizesDbSetNamesCorrectly(string entityName, string expectedDbSetName)
    {
        // Arrange
        var schema = new DatabaseSchema
        {
            DatabaseName = "TestDb",
            Tables = new List<Table>
            {
                new TableBuilder()
                    .WithName(entityName)
                    .WithColumn("ID", "int", isPrimaryKey: true)
                    .Build()
            }
        };

        // Act
        var result = await _generator.GenerateAsync(schema);

        // Assert
        result.Should().Contain($"public DbSet<{entityName}> {expectedDbSetName} {{ get; set; }} = null!;");
    }

    /// <summary>
    /// Test 6: Generates auto-generated file header.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesAutoGeneratedHeader()
    {
        // Arrange
        var schema = new DatabaseSchema
        {
            DatabaseName = "TestDb",
            Tables = new List<Table>
            {
                new TableBuilder()
                    .WithName("Customer")
                    .WithColumn("ID", "int", isPrimaryKey: true)
                    .Build()
            }
        };

        // Act
        var result = await _generator.GenerateAsync(schema);

        // Assert
        result.Should().Contain("// <auto-generated>");
        result.Should().Contain("//     This code was generated by TargCC.Core.Generators v2.0");
        result.Should().Contain($"//     Generated from database: {schema.DatabaseName}");
        result.Should().Contain("// </auto-generated>");
    }

    /// <summary>
    /// Test 7: Includes comprehensive XML documentation.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesXmlDocumentation()
    {
        // Arrange
        var schema = new DatabaseSchema
        {
            DatabaseName = "TestDb",
            Tables = new List<Table>
            {
                new TableBuilder()
                    .WithName("Customer")
                    .WithColumn("ID", "int", isPrimaryKey: true)
                    .Build()
            }
        };

        // Act
        var result = await _generator.GenerateAsync(schema);

        // Assert
        // Class documentation
        result.Should().Contain("/// <summary>");
        result.Should().Contain($"/// Entity Framework Core DbContext for {schema.DatabaseName} database.");

        // DbSet documentation
        result.Should().Contain("/// Gets or sets the DbSet for Customer entities.");

        // Constructor documentation
        result.Should().Contain("/// Initializes a new instance of the <see cref=\"ApplicationDbContext\"/> class.");

        // OnModelCreating documentation
        result.Should().Contain("/// Configures the entity model using Fluent API.");
    }

    /// <summary>
    /// Test 8: Orders DbSets alphabetically.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_OrdersDbSetsAlphabetically()
    {
        // Arrange
        var schema = new DatabaseSchema
        {
            DatabaseName = "TestDb",
            Tables = new List<Table>
            {
                new TableBuilder().WithName("Zebra").WithColumn("ID", "int", isPrimaryKey: true).Build(),
                new TableBuilder().WithName("Apple").WithColumn("ID", "int", isPrimaryKey: true).Build(),
                new TableBuilder().WithName("Mango").WithColumn("ID", "int", isPrimaryKey: true).Build()
            }
        };

        // Act
        var result = await _generator.GenerateAsync(schema);

        // Assert
        var appleIndex = result.IndexOf("DbSet<Apple>", StringComparison.Ordinal);
        var mangoIndex = result.IndexOf("DbSet<Mango>", StringComparison.Ordinal);
        var zebraIndex = result.IndexOf("DbSet<Zebra>", StringComparison.Ordinal);

        appleIndex.Should().BeLessThan(mangoIndex);
        mangoIndex.Should().BeLessThan(zebraIndex);
    }

    /// <summary>
    /// Test 9: Includes regions for organization.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesRegionsForOrganization()
    {
        // Arrange
        var schema = new DatabaseSchema
        {
            DatabaseName = "TestDb",
            Tables = new List<Table>
            {
                new TableBuilder()
                    .WithName("Customer")
                    .WithColumn("ID", "int", isPrimaryKey: true)
                    .Build()
            }
        };

        // Act
        var result = await _generator.GenerateAsync(schema);

        // Assert
        result.Should().Contain("#region DbSets");
        result.Should().Contain("#region Constructor");
        result.Should().Contain("#region Configuration");
        result.Should().Contain("#endregion");
    }

    /// <summary>
    /// Test 10: Generates complete DbContext for complex schema.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithComplexSchema_GeneratesCompleteDbContext()
    {
        // Arrange
        var schema = new DatabaseSchema
        {
            DatabaseName = "ECommerceDb",
            Tables = new List<Table>
            {
                new TableBuilder()
                    .WithName("Customer")
                    .WithColumn("ID", "int", isPrimaryKey: true)
                    .WithColumn("Name", "nvarchar", maxLength: 100)
                    .Build(),
                new TableBuilder()
                    .WithName("Order")
                    .WithColumn("ID", "int", isPrimaryKey: true)
                    .WithColumn("CustomerID", "int", isForeignKey: true)
                    .Build(),
                new TableBuilder()
                    .WithName("Product")
                    .WithColumn("ID", "int", isPrimaryKey: true)
                    .WithColumn("Name", "nvarchar", maxLength: 200)
                    .Build(),
                new TableBuilder()
                    .WithName("OrderItem")
                    .WithColumn("ID", "int", isPrimaryKey: true)
                    .WithColumn("OrderID", "int", isForeignKey: true)
                    .WithColumn("ProductID", "int", isForeignKey: true)
                    .Build()
            }
        };

        // Act
        var result = await _generator.GenerateAsync(schema);

        // Assert
        result.Should().NotBeNullOrEmpty();

        // All DbSets present
        result.Should().Contain("public DbSet<Customer> Customers { get; set; } = null!;");
        result.Should().Contain("public DbSet<Order> Orders { get; set; } = null!;");
        result.Should().Contain("public DbSet<Product> Products { get; set; } = null!;");
        result.Should().Contain("public DbSet<OrderItem> OrderItems { get; set; } = null!;");

        // Class structure
        result.Should().Contain("public class ApplicationDbContext : DbContext");
        result.Should().Contain("public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)");
        result.Should().Contain("protected override void OnModelCreating(ModelBuilder modelBuilder)");

        // Configuration
        result.Should().Contain("modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());");
        result.Should().Contain("base.OnModelCreating(modelBuilder);");
    }

    /// <summary>
    /// Test 11: Validates modelBuilder parameter in OnModelCreating.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_ValidatesModelBuilderParameter()
    {
        // Arrange
        var schema = new DatabaseSchema
        {
            DatabaseName = "TestDb",
            Tables = new List<Table>
            {
                new TableBuilder()
                    .WithName("Customer")
                    .WithColumn("ID", "int", isPrimaryKey: true)
                    .Build()
            }
        };

        // Act
        var result = await _generator.GenerateAsync(schema);

        // Assert
        result.Should().Contain("ArgumentNullException.ThrowIfNull(modelBuilder);");
    }

    /// <summary>
    /// Test 12: Logs information during generation.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_LogsInformationMessages()
    {
        // Arrange
        var schema = new DatabaseSchema
        {
            DatabaseName = "TestDb",
            Tables = new List<Table>
            {
                new TableBuilder()
                    .WithName("Customer")
                    .WithColumn("ID", "int", isPrimaryKey: true)
                    .Build()
            }
        };

        // Setup IsEnabled to return true for LoggerMessage delegates to work
        _mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

        // Act
        await _generator.GenerateAsync(schema);

        // Assert - First log message
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Generating DbContext with 1 tables")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Assert - Second log message
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully generated DbContext with 1 DbSets")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
