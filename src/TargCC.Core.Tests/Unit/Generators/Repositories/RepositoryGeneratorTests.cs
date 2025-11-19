namespace TargCC.Core.Tests.Unit.Generators.Repositories;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Generators.Repositories;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Tests.TestHelpers;
using Xunit;

/// <summary>
/// Tests for RepositoryGenerator to ensure correct repository implementation generation.
/// </summary>
public class RepositoryGeneratorTests
{
    private readonly Mock<ILogger<RepositoryGenerator>> _mockLogger;
    private readonly RepositoryGenerator _generator;

    public RepositoryGeneratorTests()
    {
        _mockLogger = new Mock<ILogger<RepositoryGenerator>>();
        _generator = new RepositoryGenerator(_mockLogger.Object);
    }

    /// <summary>
    /// Test 1: Constructor validates required dependencies.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new RepositoryGenerator(null!));

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
    /// Test 4: Generates basic repository class with CRUD methods.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithSimpleTable_GeneratesRepositoryWithCrudMethods()
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

        // Class declaration
        result.Should().Contain("public class CustomerRepository : ICustomerRepository");

        // Fields
        result.Should().Contain("private readonly IDbConnection _connection;");
        result.Should().Contain("private readonly ILogger<CustomerRepository> _logger;");

        // Constructor
        result.Should().Contain("public CustomerRepository(IDbConnection connection, ILogger<CustomerRepository> logger)");

        // CRUD methods with Dapper
        result.Should().Contain("public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)");
        result.Should().Contain("await _connection.QueryFirstOrDefaultAsync<Customer>");
        result.Should().Contain("SP_GetCustomerByID");

        result.Should().Contain("public async Task<IEnumerable<Customer>> GetAllAsync(int? skip = null, int? take = null, CancellationToken cancellationToken = default)");
        result.Should().Contain("await _connection.QueryAsync<Customer>");

        result.Should().Contain("public async Task AddAsync(Customer entity, CancellationToken cancellationToken = default)");
        result.Should().Contain("await _connection.ExecuteAsync");

        result.Should().Contain("public async Task UpdateAsync(Customer entity, CancellationToken cancellationToken = default)");
        result.Should().Contain("public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)");

        // Helper methods
        result.Should().Contain("public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)");

        // Namespace and usings
        result.Should().Contain("namespace TargCC.Infrastructure.Repositories;");
        result.Should().Contain("using System.Data;");
        result.Should().Contain("using Dapper;");
        result.Should().Contain("using Microsoft.Extensions.Logging;");
        result.Should().Contain("using TargCC.Domain.Entities;");
        result.Should().Contain("using TargCC.Domain.Interfaces;");
    }

    /// <summary>
    /// Test 5: Generates methods with proper error handling and logging.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GeneratesMethodsWithErrorHandlingAndLogging()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        // Try-catch blocks
        result.Should().Contain("try");
        result.Should().Contain("catch (Exception ex)");

        // Logging statements
        result.Should().Contain("_logger.LogDebug");
        result.Should().Contain("_logger.LogError");
        result.Should().Contain("_logger.LogInformation");

        // Error logging with exception
        result.Should().Contain("_logger.LogError(ex,");
    }

    /// <summary>
    /// Test 6: Generates index-based query methods with unique index.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithUniqueIndex_GeneratesGetByMethodWithDapper()
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
        result.Should().Contain("public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)");
        result.Should().Contain("SP_GetCustomerByEmail");
        result.Should().Contain("await _connection.QueryFirstOrDefaultAsync<Customer>");
        result.Should().Contain("new { Email = email }");
        result.Should().Contain("commandType: CommandType.StoredProcedure");
    }

    /// <summary>
    /// Test 7: Generates index-based query methods with non-unique index.
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
        result.Should().Contain("public async Task<IEnumerable<Customer>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)");
        result.Should().Contain("await _connection.QueryAsync<Customer>");
        result.Should().Contain("SP_GetCustomerByStatus");
    }

    /// <summary>
    /// Test 8: Generates composite index method.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithCompositeIndex_GeneratesMethodWithMultipleParameters()
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
        result.Should().Contain("public async Task<Customer?> GetByLastNameAndFirstNameAsync(string lastName, string firstName, CancellationToken cancellationToken = default)");
        result.Should().Contain("new { LastName = lastName, FirstName = firstName }");
        result.Should().Contain("SP_GetCustomerByLastNameAndFirstName");
    }

    /// <summary>
    /// Test 9: Generates UpdateAggregatesAsync for table with agg_ columns.
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
        result.Should().Contain("public async Task UpdateAggregatesAsync(int id, int orderCount, decimal totalSpent, CancellationToken cancellationToken = default)");
        result.Should().Contain("SP_UpdateCustomerAggregates");
        result.Should().Contain("new { ID = id, OrderCount = orderCount, TotalSpent = totalSpent }");
    }

    /// <summary>
    /// Test 10: Handles TargCC prefixes correctly in method parameters.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithPrefixedColumns_RemovesPrefixesFromParameters()
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
        // Parameter name should be "status" (without prefix)
        result.Should().Contain("GetByStatusAsync(string status,");
        // But SP parameter should use column name without prefix
        result.Should().Contain("new { Status = status }");
    }

    /// <summary>
    /// Test 11: Generates correct primary key type for different data types.
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
        result.Should().Contain($"public async Task<Customer?> GetByIdAsync({expectedCSharpType} id,");
        result.Should().Contain($"public async Task DeleteAsync({expectedCSharpType} id,");
        result.Should().Contain($"public async Task<bool> ExistsAsync({expectedCSharpType} id,");
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
        result.Should().Contain("// </auto-generated>");
    }

    /// <summary>
    /// Test 13: Validates entity parameter in Add and Update methods.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GeneratesNullChecksForEntityParameters()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.Should().Contain("if (entity == null)");
        result.Should().Contain("throw new ArgumentNullException(nameof(entity));");
    }

    /// <summary>
    /// Test 14: Uses correct stored procedure names.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_UsesCorrectStoredProcedureNames()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Order")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("OrderNumber", "nvarchar", maxLength: 50)
            .WithIndex("IX_Order_OrderNumber", isUnique: true, columns: new[] { "OrderNumber" })
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.Should().Contain("SP_GetOrderByID");
        result.Should().Contain("SP_GetAllOrders");
        result.Should().Contain("SP_AddOrder");
        result.Should().Contain("SP_UpdateOrder");
        result.Should().Contain("SP_DeleteOrder");
        result.Should().Contain("SP_GetOrderByOrderNumber");
    }

    /// <summary>
    /// Test 15: Generates complete repository for complex table.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithComplexTable_GeneratesCompleteRepository()
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

        // Class structure
        result.Should().Contain("public class OrderRepository : IOrderRepository");
        result.Should().Contain("private readonly IDbConnection _connection;");
        result.Should().Contain("public OrderRepository(IDbConnection connection, ILogger<OrderRepository> logger)");

        // CRUD methods
        result.Should().Contain("public async Task<Order?> GetByIdAsync(int id");
        result.Should().Contain("public async Task AddAsync(Order entity");
        result.Should().Contain("public async Task UpdateAsync(Order entity");
        result.Should().Contain("public async Task DeleteAsync(int id");

        // Index-based queries
        result.Should().Contain("public async Task<Order?> GetByOrderNumberAsync(string orderNumber");
        result.Should().Contain("public async Task<IEnumerable<Order>> GetByCustomerIDAsync(int customerID");
        result.Should().Contain("public async Task<IEnumerable<Order>> GetByStatusAsync(string status");

        // Aggregate method
        result.Should().Contain("public async Task UpdateAggregatesAsync(int id, decimal totalAmount, int itemCount");

        // Dapper usage
        result.Should().Contain("await _connection.QueryFirstOrDefaultAsync<Order>");
        result.Should().Contain("await _connection.QueryAsync<Order>");
        result.Should().Contain("await _connection.ExecuteAsync");
        result.Should().Contain("commandType: CommandType.StoredProcedure");

        // Error handling
        result.Should().Contain("try");
        result.Should().Contain("catch (Exception ex)");
        result.Should().Contain("_logger.LogError(ex,");
    }

    /// <summary>
    /// Test 16: Logs information during generation.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_LogsInformationMessages()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Setup IsEnabled to return true for LoggerMessage delegates to work
        _mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

        // Act
        await _generator.GenerateAsync(table);

        // Assert - First log message
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Generating repository implementation for table: Customer")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Assert - Second log message
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully generated repository implementation for table: Customer")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
