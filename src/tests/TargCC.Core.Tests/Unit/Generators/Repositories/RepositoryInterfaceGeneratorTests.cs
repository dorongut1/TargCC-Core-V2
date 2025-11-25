namespace TargCC.Core.Tests.Unit.Generators.Repositories;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Generators.Repositories;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Tests.TestHelpers;
using Xunit;

/// <summary>
/// Tests for RepositoryInterfaceGenerator to ensure correct repository interface generation.
/// </summary>
public class RepositoryInterfaceGeneratorTests
{
    private readonly Mock<ILogger<RepositoryInterfaceGenerator>> _mockLogger;
    private readonly RepositoryInterfaceGenerator _generator;

    public RepositoryInterfaceGeneratorTests()
    {
        _mockLogger = new Mock<ILogger<RepositoryInterfaceGenerator>>();
        _generator = new RepositoryInterfaceGenerator(_mockLogger.Object);
    }

    /// <summary>
    /// Test 1: Constructor validates required dependencies.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new RepositoryInterfaceGenerator(null!));

        exception.ParamName.Should().Be("logger");
    }

    /// <summary>
    /// Test 2: GenerateAsync validates null table parameter.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithNullTable_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _generator.GenerateAsync(null!));

        exception.ParamName.Should().Be("table");
    }

    /// <summary>
    /// Test 3: GenerateAsync validates table has primary key.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithNoPrimaryKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .Build();

        table.PrimaryKeyColumns.Clear(); // Remove primary key

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _generator.GenerateAsync(table));

        exception.Message.Should().Contain("must have a primary key defined");
    }

    /// <summary>
    /// Test 4: Generates basic interface with CRUD methods for simple table.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithSimpleTable_GeneratesInterfaceWithCrudMethods()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("Name", "nvarchar", maxLength: 100, isNullable: false)
            .WithColumn("Email", "nvarchar", maxLength: 100, isNullable: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.Should().NotBeNullOrEmpty();

        // Interface declaration
        result.Should().Contain("public interface ICustomerRepository");

        // CRUD methods
        result.Should().Contain("Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);");
        result.Should().Contain("Task<IEnumerable<Customer>> GetAllAsync(int? skip = null, int? take = null, CancellationToken cancellationToken = default);");
        result.Should().Contain("Task AddAsync(Customer entity, CancellationToken cancellationToken = default);");
        result.Should().Contain("Task UpdateAsync(Customer entity, CancellationToken cancellationToken = default);");
        result.Should().Contain("Task DeleteAsync(int id, CancellationToken cancellationToken = default);");

        // Helper methods
        result.Should().Contain("Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);");

        // Namespace and usings
        result.Should().Contain("namespace TargCC.Domain.Interfaces;");
        result.Should().Contain("using TargCC.Domain.Entities;");
    }

    /// <summary>
    /// Test 5: Generates interface with unique index query methods.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithUniqueIndex_GeneratesGetByMethod()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Email", "nvarchar", maxLength: 100)
            .WithIndex("IX_Customer_Email", isUnique: true, columns: new[] { "Email" })
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.Should().Contain("Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);");
        result.Should().Contain("/// Gets a Customer entity by Email.");
    }

    /// <summary>
    /// Test 6: Generates interface with non-unique index filter methods.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithNonUniqueIndex_GeneratesGetByMethodReturningCollection()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Status", "nvarchar", maxLength: 50)
            .WithIndex("IX_Customer_Status", isUnique: false, columns: new[] { "Status" })
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.Should().Contain("Task<IEnumerable<Customer>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);");
        result.Should().Contain("/// Gets all Customer entities matching the specified Status.");
    }

    /// <summary>
    /// Test 7: Generates interface with composite index methods.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithCompositeIndex_GeneratesGetByMethodWithMultipleParameters()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("LastName", "nvarchar", maxLength: 100)
            .WithColumn("FirstName", "nvarchar", maxLength: 100)
            .WithIndex("IX_Customer_Name", isUnique: true, columns: new[] { "LastName", "FirstName" })
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.Should().Contain("Task<Customer?> GetByLastNameAndFirstNameAsync(string lastName, string firstName, CancellationToken cancellationToken = default);");
    }

    /// <summary>
    /// Test 8: Generates UpdateAggregatesAsync for table with agg_ columns.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithAggregateColumns_GeneratesUpdateAggregatesMethod()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("agg_OrderCount", "int", isNullable: false)
            .WithColumn("agg_TotalSpent", "decimal", precision: 18, scale: 2)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.Should().Contain("Task UpdateAggregatesAsync(int id, int orderCount, decimal totalSpent, CancellationToken cancellationToken = default);");
        result.Should().Contain("/// Updates aggregate columns for a Customer entity.");
    }

    /// <summary>
    /// Test 9: Handles TargCC prefixes in column names correctly.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithPrefixedColumns_RemovesPrefixesFromMethodParameters()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("lkp_Status", "nvarchar", maxLength: 50)
            .WithIndex("IX_Customer_Status", isUnique: false, columns: new[] { "lkp_Status" })
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        // Should use "status" (camelCase, no prefix) as parameter name
        result.Should().Contain("Task<IEnumerable<Customer>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);");
    }

    /// <summary>
    /// Test 10: Generates correct primary key type for different data types.
    /// </summary>
    [Theory]
    [InlineData("int", "int")]
    [InlineData("bigint", "long")]
    [InlineData("uniqueidentifier", "Guid")]
    [InlineData("varchar", "string")]
    public async Task GenerateAsync_WithDifferentPrimaryKeyTypes_UsesCorrectCSharpType(string sqlType, string expectedCSharpType)
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", sqlType, isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.Should().Contain($"Task<Customer?> GetByIdAsync({expectedCSharpType} id, CancellationToken cancellationToken = default);");
        result.Should().Contain($"Task DeleteAsync({expectedCSharpType} id, CancellationToken cancellationToken = default);");
        result.Should().Contain($"Task<bool> ExistsAsync({expectedCSharpType} id, CancellationToken cancellationToken = default);");
    }

    /// <summary>
    /// Test 11: Generates XML documentation for all methods.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GeneratesXmlDocumentationForAllMethods()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Email", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        // Check for XML documentation tags
        result.Should().Contain("/// <summary>");
        result.Should().Contain("/// <param name=");
        result.Should().Contain("/// <returns>");
        result.Should().Contain("/// Gets a Customer entity by its primary key.");
        result.Should().Contain("/// Adds a new Customer entity to the database.");
    }

    /// <summary>
    /// Test 12: Generates auto-generated file header.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesAutoGeneratedHeader()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.Should().Contain("// <auto-generated>");
        result.Should().Contain("//     This code was generated by TargCC.Core.Generators v2.0");
        result.Should().Contain($"//     Generated from table: {table.FullName}");
        result.Should().Contain("//     the code is regenerated.");
        result.Should().Contain("// </auto-generated>");
    }

    /// <summary>
    /// Test 13: Does not generate methods for primary key index.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_DoesNotGenerateMethodsForPrimaryKeyIndex()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithIndex("PK_Customer", isUnique: true, isPrimaryKey: true, columns: new[] { "ID" })
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        // Should have only one GetById method (from CRUD), not two
        var getByIdCount = System.Text.RegularExpressions.Regex.Matches(result, "GetByIdAsync").Count;
        getByIdCount.Should().Be(1);
    }

    /// <summary>
    /// Test 14: Generates interface for table with complex scenario.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithComplexTable_GeneratesCompleteInterface()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Order")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("OrderNumber", "nvarchar", maxLength: 50, isNullable: false)
            .WithColumn("CustomerID", "int", isNullable: false, isForeignKey: true)
            .WithColumn("OrderDate", "datetime", isNullable: false)
            .WithColumn("Status", "nvarchar", maxLength: 50)
            .WithColumn("agg_TotalAmount", "decimal", precision: 18, scale: 2)
            .WithColumn("agg_ItemCount", "int")
            .WithIndex("IX_Order_OrderNumber", isUnique: true, columns: new[] { "OrderNumber" })
            .WithIndex("IX_Order_CustomerID", isUnique: false, columns: new[] { "CustomerID" })
            .WithIndex("IX_Order_Status", isUnique: false, columns: new[] { "Status" })
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.Should().NotBeNullOrEmpty();

        // Basic CRUD
        result.Should().Contain("public interface IOrderRepository");
        result.Should().Contain("Task<Order?> GetByIdAsync(int id");
        result.Should().Contain("Task AddAsync(Order entity");

        // Index-based queries
        result.Should().Contain("Task<Order?> GetByOrderNumberAsync(string orderNumber"); // Unique
        result.Should().Contain("Task<IEnumerable<Order>> GetByCustomerIDAsync(int customerID"); // Non-unique
        result.Should().Contain("Task<IEnumerable<Order>> GetByStatusAsync(string status"); // Non-unique

        // Aggregate method
        result.Should().Contain("Task UpdateAggregatesAsync(int id, decimal totalAmount, int itemCount");
    }

    /// <summary>
    /// Test 15: Logs information during generation.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_LogsInformationMessages()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        await _generator.GenerateAsync(table);

        // Assert - Verify IsEnabled was called (LoggerMessage delegates check this first)
        _mockLogger.Verify(
            x => x.IsEnabled(LogLevel.Information),
            Times.AtLeastOnce);
    }
}
