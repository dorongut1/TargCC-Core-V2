// <copyright file="QueryGeneratorTests.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators.CQRS;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Generators.CQRS;
using TargCC.Core.Tests.TestHelpers;
using Xunit;

/// <summary>
/// Unit tests for <see cref="QueryGenerator"/>.
/// </summary>
public class QueryGeneratorTests
{
    private readonly Mock<ILogger<QueryGenerator>> _loggerMock;
    private readonly QueryGenerator _generator;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryGeneratorTests"/> class.
    /// </summary>
    public QueryGeneratorTests()
    {
        _loggerMock = new Mock<ILogger<QueryGenerator>>();
        _generator = new QueryGenerator(_loggerMock.Object);
    }

    /// <summary>
    /// Verifies that constructor throws when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new QueryGenerator(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Verifies that GenerateAsync throws when table is null.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithNullTable_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _generator.GenerateAsync(null!, QueryType.GetById);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("table");
    }

    /// <summary>
    /// Verifies that GenerateAsync throws when table has no primary key for GetById query.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GetByIdWithNoPrimaryKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var act = () => _generator.GenerateAsync(table, QueryType.GetById);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*primary key*");
    }

    /// <summary>
    /// Verifies that GenerateAsync throws for GetByIndex query type.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GetByIndexQueryType_ThrowsInvalidOperationException()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var act = () => _generator.GenerateAsync(table, QueryType.GetByIndex);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*GenerateByIndexAsync*");
    }

    /// <summary>
    /// Verifies that GenerateAsync generates complete GetById query components.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GetByIdQuery_GeneratesAllComponents()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("Email", "nvarchar", maxLength: 200)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, QueryType.GetById);

        // Assert
        result.Should().NotBeNull();
        result.QueryClassName.Should().Be("GetCustomerQuery");
        result.HandlerClassName.Should().Be("GetCustomerHandler");
        result.ValidatorClassName.Should().Be("GetCustomerValidator");
        result.DtoClassName.Should().Be("CustomerDto");
    }

    /// <summary>
    /// Verifies that GetById query code contains correct record definition.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GetByIdQuery_QueryCodeContainsRecordDefinition()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, QueryType.GetById);

        // Assert
        result.QueryCode.Should().Contain("public record GetCustomerQuery(int ID)");
        result.QueryCode.Should().Contain("IRequest<Result<CustomerDto>>");
        result.QueryCode.Should().Contain("namespace TargCC.Application.Features.Customers.Queries");
    }

    /// <summary>
    /// Verifies that GetById handler code contains correct implementation.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GetByIdQuery_HandlerCodeContainsCorrectImplementation()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, QueryType.GetById);

        // Assert
        result.HandlerCode.Should().Contain("IRequestHandler<GetCustomerQuery, Result<CustomerDto>>");
        result.HandlerCode.Should().Contain("ICustomerRepository");
        result.HandlerCode.Should().Contain("IMapper");
        result.HandlerCode.Should().Contain("GetByIdAsync");
        result.HandlerCode.Should().Contain("ArgumentNullException.ThrowIfNull");
    }

    /// <summary>
    /// Verifies that GetById validator code contains correct validation rules.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GetByIdQuery_ValidatorCodeContainsCorrectRules()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, QueryType.GetById);

        // Assert
        result.ValidatorCode.Should().Contain("AbstractValidator<GetCustomerQuery>");
        result.ValidatorCode.Should().Contain("RuleFor(x => x.ID)");
        result.ValidatorCode.Should().Contain("GreaterThan(0)");
    }

    /// <summary>
    /// Verifies that GetById with Guid primary key uses NotEmpty validation.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GetByIdWithGuidPrimaryKey_ValidatorUsesNotEmpty()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "uniqueidentifier", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, QueryType.GetById);

        // Assert
        result.ValidatorCode.Should().Contain("NotEmpty()");
    }

    /// <summary>
    /// Verifies that GetAll query generates correct components.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GetAllQuery_GeneratesAllComponents()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, QueryType.GetAll);

        // Assert
        result.QueryClassName.Should().Be("GetCustomersQuery");
        result.HandlerClassName.Should().Be("GetCustomersHandler");
        result.ValidatorClassName.Should().Be("GetCustomersValidator");
    }

    /// <summary>
    /// Verifies that GetAll query code contains pagination parameters.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GetAllQuery_QueryCodeContainsPaginationParameters()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, QueryType.GetAll);

        // Assert
        result.QueryCode.Should().Contain("PageNumber");
        result.QueryCode.Should().Contain("PageSize");
        result.QueryCode.Should().Contain("SearchTerm");
        result.QueryCode.Should().Contain("PaginatedList<CustomerDto>");
    }

    /// <summary>
    /// Verifies that GetAll validator contains pagination rules.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GetAllQuery_ValidatorContainsPaginationRules()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, QueryType.GetAll);

        // Assert
        result.ValidatorCode.Should().Contain("RuleFor(x => x.PageNumber)");
        result.ValidatorCode.Should().Contain("GreaterThan(0)");
        result.ValidatorCode.Should().Contain("RuleFor(x => x.PageSize)");
        result.ValidatorCode.Should().Contain("InclusiveBetween(1, 100)");
    }

    /// <summary>
    /// Verifies that GenerateByIndexAsync throws when table is null.
    /// </summary>
    [Fact]
    public async Task GenerateByIndexAsync_WithNullTable_ThrowsArgumentNullException()
    {
        // Arrange
        var tempTable = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Email", "nvarchar", maxLength: 200)
            .WithIndex("IX_Customer_Email", isUnique: true, columns: ["Email"])
            .Build();

        var index = tempTable.Indexes.First();

        // Act
        var act = () => _generator.GenerateByIndexAsync(null!, index);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("table");
    }

    /// <summary>
    /// Verifies that GenerateByIndexAsync throws when index is null.
    /// </summary>
    [Fact]
    public async Task GenerateByIndexAsync_WithNullIndex_ThrowsArgumentNullException()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var act = () => _generator.GenerateByIndexAsync(table, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("index");
    }

    /// <summary>
    /// Verifies that GenerateByIndexAsync generates correct components for unique index.
    /// </summary>
    [Fact]
    public async Task GenerateByIndexAsync_UniqueIndex_GeneratesCorrectComponents()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Email", "nvarchar", maxLength: 200, isNullable: false)
            .WithIndex("IX_Customer_Email", isUnique: true, columns: ["Email"])
            .Build();

        var index = table.Indexes.First(i => i.Name == "IX_Customer_Email");

        // Act
        var result = await _generator.GenerateByIndexAsync(table, index);

        // Assert
        result.QueryClassName.Should().Be("GetCustomerByEmailQuery");
        result.HandlerClassName.Should().Be("GetCustomerByEmailHandler");
        result.QueryCode.Should().Contain("Result<CustomerDto?>");
    }

    /// <summary>
    /// Verifies that GenerateByIndexAsync generates correct components for non-unique index.
    /// </summary>
    [Fact]
    public async Task GenerateByIndexAsync_NonUniqueIndex_GeneratesCorrectComponents()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Status", "nvarchar", maxLength: 50, isNullable: false)
            .WithIndex("IX_Customer_Status", isUnique: false, columns: ["Status"])
            .Build();

        var index = table.Indexes.First(i => i.Name == "IX_Customer_Status");

        // Act
        var result = await _generator.GenerateByIndexAsync(table, index);

        // Assert
        result.QueryCode.Should().Contain("IEnumerable<CustomerDto>");
        result.HandlerCode.Should().Contain("IEnumerable<CustomerDto>");
    }

    /// <summary>
    /// Verifies that GenerateByIndexAsync handles composite index correctly.
    /// </summary>
    [Fact]
    public async Task GenerateByIndexAsync_CompositeIndex_GeneratesCorrectMethodName()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("FirstName", "nvarchar", maxLength: 100)
            .WithColumn("LastName", "nvarchar", maxLength: 100)
            .WithIndex("IX_Customer_Name", isUnique: false, columns: ["FirstName", "LastName"])
            .Build();

        var index = table.Indexes.First(i => i.Name == "IX_Customer_Name");

        // Act
        var result = await _generator.GenerateByIndexAsync(table, index);

        // Assert
        result.QueryClassName.Should().Be("GetCustomerByFirstNameAndLastNameQuery");
        result.QueryCode.Should().Contain("FirstName");
        result.QueryCode.Should().Contain("LastName");
    }

    /// <summary>
    /// Verifies that GenerateDtoAsync generates correct DTO.
    /// </summary>
    [Fact]
    public async Task GenerateDtoAsync_SimpleTable_GeneratesCorrectDto()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("Email", "nvarchar", maxLength: 200)
            .Build();

        // Act
        var result = await _generator.GenerateDtoAsync(table);

        // Assert
        result.Should().Contain("public class CustomerDto");
        result.Should().Contain("public int ID { get; init; }");
        result.Should().Contain("public string Name { get; init; } = string.Empty;");
        result.Should().Contain("public string Email { get; init; } = string.Empty;");
    }

    /// <summary>
    /// Verifies that DTO excludes encrypted columns.
    /// </summary>
    [Fact]
    public async Task GenerateDtoAsync_WithEncryptedColumn_ExcludesEncryptedColumn()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("ent_CreditCard", "nvarchar", maxLength: 500)
            .Build();

        // Act
        var result = await _generator.GenerateDtoAsync(table);

        // Assert
        result.Should().Contain("Name");
        result.Should().NotContain("CreditCard");
        result.Should().NotContain("ent_");
    }

    /// <summary>
    /// Verifies that DTO excludes hashed columns.
    /// </summary>
    [Fact]
    public async Task GenerateDtoAsync_WithHashedColumn_ExcludesHashedColumn()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("eno_Password", "nvarchar", maxLength: 128)
            .Build();

        // Act
        var result = await _generator.GenerateDtoAsync(table);

        // Assert
        result.Should().Contain("Name");
        result.Should().NotContain("Password");
        result.Should().NotContain("eno_");
    }

    /// <summary>
    /// Verifies that GenerateAllAsync generates multiple query types.
    /// </summary>
    [Fact]
    public async Task GenerateAllAsync_TableWithPrimaryKey_GeneratesBothQueryTypes()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var results = await _generator.GenerateAllAsync(table);

        // Assert
        var resultList = results.ToList();
        resultList.Should().HaveCount(2);
        resultList.Should().Contain(r => r.QueryClassName == "GetCustomerQuery");
        resultList.Should().Contain(r => r.QueryClassName == "GetCustomersQuery");
    }

    /// <summary>
    /// Verifies that GenerateAllAsync generates only GetAll when no primary key.
    /// </summary>
    [Fact]
    public async Task GenerateAllAsync_TableWithoutPrimaryKey_GeneratesOnlyGetAll()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var results = await _generator.GenerateAllAsync(table);

        // Assert
        var resultList = results.ToList();
        resultList.Should().HaveCount(1);
        resultList[0].QueryClassName.Should().Be("GetCustomersQuery");
    }

    /// <summary>
    /// Verifies that generated code includes auto-generated header.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_AnyQuery_IncludesAutoGeneratedHeader()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, QueryType.GetById);

        // Assert
        result.QueryCode.Should().Contain("// <auto-generated>");
        result.QueryCode.Should().Contain("This code was generated by TargCC");
        result.HandlerCode.Should().Contain("// <auto-generated>");
        result.ValidatorCode.Should().Contain("// <auto-generated>");
        result.DtoCode.Should().Contain("// <auto-generated>");
    }

    /// <summary>
    /// Verifies that handler includes try-catch error handling.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GetByIdQuery_HandlerIncludesErrorHandling()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, QueryType.GetById);

        // Assert
        result.HandlerCode.Should().Contain("try");
        result.HandlerCode.Should().Contain("catch (Exception ex)");
        result.HandlerCode.Should().Contain("_logger.LogError");
        result.HandlerCode.Should().Contain("throw;");
    }

    /// <summary>
    /// Verifies that handler includes logging.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GetByIdQuery_HandlerIncludesLogging()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, QueryType.GetById);

        // Assert
        result.HandlerCode.Should().Contain("_logger.LogDebug");
        result.HandlerCode.Should().Contain("_logger.LogWarning");
    }

    /// <summary>
    /// Verifies that nullable columns generate nullable DTO properties.
    /// </summary>
    [Fact]
    public async Task GenerateDtoAsync_NullableColumn_GeneratesNullableProperty()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Age", "int", isNullable: true)
            .WithColumn("BirthDate", "datetime", isNullable: true)
            .Build();

        // Act
        var result = await _generator.GenerateDtoAsync(table);

        // Assert
        result.Should().Contain("int? Age");
        result.Should().Contain("DateTime? BirthDate");
    }

    /// <summary>
    /// Verifies that TargCC prefixes are removed from property names.
    /// </summary>
    [Fact]
    public async Task GenerateDtoAsync_ColumnWithPrefix_RemovesPrefixFromPropertyName()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("lkp_Status", "nvarchar", maxLength: 50)
            .WithColumn("agg_OrderCount", "int")
            .Build();

        // Act
        var result = await _generator.GenerateDtoAsync(table);

        // Assert
        result.Should().Contain("Status");
        result.Should().Contain("OrderCount");
        result.Should().NotContain("lkp_");
        result.Should().NotContain("agg_");
    }

    /// <summary>
    /// Verifies that query handler uses correct repository method name.
    /// </summary>
    [Fact]
    public async Task GenerateByIndexAsync_UniqueIndex_HandlerCallsCorrectRepositoryMethod()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Email", "nvarchar", maxLength: 200)
            .WithIndex("IX_Customer_Email", isUnique: true, columns: ["Email"])
            .Build();

        var index = table.Indexes.First(i => i.Name == "IX_Customer_Email");

        // Act
        var result = await _generator.GenerateByIndexAsync(table, index);

        // Assert
        result.HandlerCode.Should().Contain("GetByEmailAsync");
    }

    /// <summary>
    /// Verifies validator generates MaximumLength for string columns.
    /// </summary>
    [Fact]
    public async Task GenerateByIndexAsync_StringColumnWithMaxLength_ValidatorIncludesMaximumLength()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Email", "nvarchar", maxLength: 200, isNullable: false)
            .WithIndex("IX_Customer_Email", isUnique: true, columns: ["Email"])
            .Build();

        var index = table.Indexes.First(i => i.Name == "IX_Customer_Email");

        // Act
        var result = await _generator.GenerateByIndexAsync(table, index);

        // Assert
        result.ValidatorCode.Should().Contain("MaximumLength(200)");
    }

    /// <summary>
    /// Verifies that GetAll handler includes skip/take pagination.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GetAllQuery_HandlerImplementsPagination()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, QueryType.GetAll);

        // Assert
        result.HandlerCode.Should().Contain("skip");
        result.HandlerCode.Should().Contain("(request.PageNumber - 1) * request.PageSize");
        result.HandlerCode.Should().Contain("GetAllAsync");
    }

    /// <summary>
    /// Verifies that generated handler includes using statements.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_AnyQuery_HandlerIncludesUsingStatements()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, QueryType.GetById);

        // Assert
        result.HandlerCode.Should().Contain("using AutoMapper;");
        result.HandlerCode.Should().Contain("using MediatR;");
        result.HandlerCode.Should().Contain("using Microsoft.Extensions.Logging;");
        result.HandlerCode.Should().Contain("using TargCC.Domain.Interfaces;");
    }
}
