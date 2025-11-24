// <copyright file="CommandGeneratorTests.cs" company="TargCC">
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
/// Unit tests for <see cref="CommandGenerator"/>.
/// </summary>
public class CommandGeneratorTests
{
    private readonly Mock<ILogger<CommandGenerator>> _loggerMock;
    private readonly CommandGenerator _generator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandGeneratorTests"/> class.
    /// </summary>
    public CommandGeneratorTests()
    {
        _loggerMock = new Mock<ILogger<CommandGenerator>>();
        _generator = new CommandGenerator(_loggerMock.Object);
    }

    #region Constructor Tests

    /// <summary>
    /// Verifies that constructor throws when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CommandGenerator(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region GenerateAsync - Validation Tests

    /// <summary>
    /// Verifies that GenerateAsync throws when table is null.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithNullTable_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _generator.GenerateAsync(null!, CommandType.Create);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("table");
    }

    /// <summary>
    /// Verifies that GenerateAsync throws when table has no primary key for Update command.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_UpdateWithNoPrimaryKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var act = () => _generator.GenerateAsync(table, CommandType.Update);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*primary key*Update*");
    }

    /// <summary>
    /// Verifies that GenerateAsync throws when table has no primary key for Delete command.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_DeleteWithNoPrimaryKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var act = () => _generator.GenerateAsync(table, CommandType.Delete);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*primary key*Delete*");
    }

    /// <summary>
    /// Verifies that Create command does not require primary key.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_CreateWithNoPrimaryKey_Succeeds()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        result.Should().NotBeNull();
        result.CommandClassName.Should().Be("CreateCustomerCommand");
    }

    #endregion

    #region CreateCommand Tests

    /// <summary>
    /// Verifies that GenerateAsync generates complete Create command components.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_CreateCommand_GeneratesAllComponents()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("Email", "nvarchar", maxLength: 200)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        result.Should().NotBeNull();
        result.CommandClassName.Should().Be("CreateCustomerCommand");
        result.HandlerClassName.Should().Be("CreateCustomerHandler");
        result.ValidatorClassName.Should().Be("CreateCustomerValidator");
        result.CommandType.Should().Be(CommandType.Create);
    }

    /// <summary>
    /// Verifies that Create command code contains correct record definition.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_CreateCommand_CommandCodeContainsRecordDefinition()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        result.CommandCode.Should().Contain("public record CreateCustomerCommand : IRequest<Result<int>>");
        result.CommandCode.Should().Contain("namespace TargCC.Application.Features.Customers.Commands");
        result.CommandCode.Should().Contain("Name");
    }

    /// <summary>
    /// Verifies that Create command excludes Identity columns.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_CreateCommand_ExcludesIdentityColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        result.CommandCode.Should().NotContain("public int ID { get;");
    }

    /// <summary>
    /// Verifies that Create command includes hashed columns (eno_).
    /// </summary>
    [Fact]
    public async Task GenerateAsync_CreateCommand_IncludesHashedColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("eno_Password", "nvarchar", maxLength: 128)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        result.CommandCode.Should().Contain("Password");
        result.HandlerCode.Should().Contain("SetPassword");
    }

    /// <summary>
    /// Verifies that Create command excludes read-only columns.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_CreateCommand_ExcludesReadOnlyColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("clc_Total", "decimal", precision: 18, scale: 2)
            .WithColumn("blg_Discount", "decimal", precision: 18, scale: 2)
            .WithColumn("agg_OrderCount", "int")
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        result.CommandCode.Should().Contain("Name");
        result.CommandCode.Should().NotContain("Total");
        result.CommandCode.Should().NotContain("Discount");
        result.CommandCode.Should().NotContain("OrderCount");
    }

    /// <summary>
    /// Verifies that Create handler code contains correct implementation.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_CreateCommand_HandlerCodeContainsCorrectImplementation()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        result.HandlerCode.Should().Contain("IRequestHandler<CreateCustomerCommand, Result<int>>");
        result.HandlerCode.Should().Contain("ICustomerRepository");
        result.HandlerCode.Should().Contain("AddAsync");
        result.HandlerCode.Should().Contain("ArgumentNullException.ThrowIfNull");
        result.HandlerCode.Should().Contain("return Result<int>.Success(entity.ID)");
    }

    /// <summary>
    /// Verifies that Create validator code contains correct validation rules.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_CreateCommand_ValidatorContainsCorrectRules()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("Name", "nvarchar", maxLength: 100, isNullable: false)
            .WithColumn("Email", "nvarchar", maxLength: 200)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        result.ValidatorCode.Should().Contain("AbstractValidator<CreateCustomerCommand>");
        result.ValidatorCode.Should().Contain("RuleFor(x => x.Name)");
        result.ValidatorCode.Should().Contain(".NotEmpty()");
        result.ValidatorCode.Should().Contain(".MaximumLength(100)");
    }

    #endregion

    #region UpdateCommand Tests

    /// <summary>
    /// Verifies that GenerateAsync generates complete Update command components.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_UpdateCommand_GeneratesAllComponents()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Update);

        // Assert
        result.Should().NotBeNull();
        result.CommandClassName.Should().Be("UpdateCustomerCommand");
        result.HandlerClassName.Should().Be("UpdateCustomerHandler");
        result.ValidatorClassName.Should().Be("UpdateCustomerValidator");
        result.CommandType.Should().Be(CommandType.Update);
    }

    /// <summary>
    /// Verifies that Update command includes primary key.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_UpdateCommand_IncludesPrimaryKey()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Update);

        // Assert
        result.CommandCode.Should().Contain("public int ID { get; init; }");
    }

    /// <summary>
    /// Verifies that Update command excludes hashed columns (eno_).
    /// </summary>
    [Fact]
    public async Task GenerateAsync_UpdateCommand_ExcludesHashedColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("eno_Password", "nvarchar", maxLength: 128)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Update);

        // Assert
        result.CommandCode.Should().Contain("Name");
        result.CommandCode.Should().NotContain("Password");
    }

    /// <summary>
    /// Verifies that Update command excludes read-only columns.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_UpdateCommand_ExcludesReadOnlyColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("clc_Total", "decimal", precision: 18, scale: 2)
            .WithColumn("blg_Discount", "decimal", precision: 18, scale: 2)
            .WithColumn("agg_OrderCount", "int")
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Update);

        // Assert
        result.CommandCode.Should().Contain("Name");
        result.CommandCode.Should().NotContain("Total");
        result.CommandCode.Should().NotContain("Discount");
        result.CommandCode.Should().NotContain("OrderCount");
    }

    /// <summary>
    /// Verifies that Update handler checks if entity exists.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_UpdateCommand_HandlerChecksEntityExists()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Update);

        // Assert
        result.HandlerCode.Should().Contain("GetByIdAsync");
        result.HandlerCode.Should().Contain("if (entity is null)");
        result.HandlerCode.Should().Contain("Result.Failure");
        result.HandlerCode.Should().Contain("not found");
    }

    /// <summary>
    /// Verifies that Update handler updates entity properties.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_UpdateCommand_HandlerUpdatesProperties()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("Email", "nvarchar", maxLength: 200)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Update);

        // Assert
        result.HandlerCode.Should().Contain("entity.Name = request.Name");
        result.HandlerCode.Should().Contain("entity.Email = request.Email");
        result.HandlerCode.Should().Contain("UpdateAsync");
    }

    /// <summary>
    /// Verifies that Update validator validates primary key.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_UpdateCommand_ValidatorValidatesPrimaryKey()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Update);

        // Assert
        result.ValidatorCode.Should().Contain("RuleFor(x => x.ID)");
        result.ValidatorCode.Should().Contain("GreaterThan(0)");
    }

    #endregion

    #region DeleteCommand Tests

    /// <summary>
    /// Verifies that GenerateAsync generates complete Delete command components.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_DeleteCommand_GeneratesAllComponents()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Delete);

        // Assert
        result.Should().NotBeNull();
        result.CommandClassName.Should().Be("DeleteCustomerCommand");
        result.HandlerClassName.Should().Be("DeleteCustomerHandler");
        result.ValidatorClassName.Should().Be("DeleteCustomerValidator");
        result.CommandType.Should().Be(CommandType.Delete);
    }

    /// <summary>
    /// Verifies that Delete command is a simple record with primary key parameter.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_DeleteCommand_IsSimpleRecordWithPrimaryKey()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Delete);

        // Assert
        result.CommandCode.Should().Contain("public record DeleteCustomerCommand(int ID)");
        result.CommandCode.Should().Contain("IRequest<Result>");
    }

    /// <summary>
    /// Verifies that Delete handler checks if entity exists.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_DeleteCommand_HandlerChecksEntityExists()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Delete);

        // Assert
        result.HandlerCode.Should().Contain("ExistsAsync");
        result.HandlerCode.Should().Contain("if (!exists)");
        result.HandlerCode.Should().Contain("Result.Failure");
        result.HandlerCode.Should().Contain("not found");
    }

    /// <summary>
    /// Verifies that Delete handler calls DeleteAsync.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_DeleteCommand_HandlerCallsDeleteAsync()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Delete);

        // Assert
        result.HandlerCode.Should().Contain("DeleteAsync(request.ID");
        result.HandlerCode.Should().Contain("return Result.Success()");
    }

    /// <summary>
    /// Verifies that Delete validator validates primary key.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_DeleteCommand_ValidatorValidatesPrimaryKey()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Delete);

        // Assert
        result.ValidatorCode.Should().Contain("RuleFor(x => x.ID)");
        result.ValidatorCode.Should().Contain("GreaterThan(0)");
    }

    #endregion

    #region GenerateAllAsync Tests

    /// <summary>
    /// Verifies that GenerateAllAsync throws when table is null.
    /// </summary>
    [Fact]
    public async Task GenerateAllAsync_WithNullTable_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _generator.GenerateAllAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("table");
    }

    /// <summary>
    /// Verifies that GenerateAllAsync generates all three command types.
    /// </summary>
    [Fact]
    public async Task GenerateAllAsync_TableWithPrimaryKey_GeneratesAllThreeCommands()
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
        resultList.Should().HaveCount(3);
        resultList.Should().Contain(r => r.CommandClassName == "CreateCustomerCommand");
        resultList.Should().Contain(r => r.CommandClassName == "UpdateCustomerCommand");
        resultList.Should().Contain(r => r.CommandClassName == "DeleteCustomerCommand");
    }

    /// <summary>
    /// Verifies that GenerateAllAsync generates only Create when no primary key.
    /// </summary>
    [Fact]
    public async Task GenerateAllAsync_TableWithoutPrimaryKey_GeneratesOnlyCreate()
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
        resultList[0].CommandClassName.Should().Be("CreateCustomerCommand");
    }

    #endregion

    #region GetCreateColumns Tests

    /// <summary>
    /// Verifies that GetCreateColumns throws when table is null.
    /// </summary>
    [Fact]
    public void GetCreateColumns_WithNullTable_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _generator.GetCreateColumns(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("table");
    }

    /// <summary>
    /// Verifies that GetCreateColumns excludes Identity columns.
    /// </summary>
    [Fact]
    public void GetCreateColumns_ExcludesIdentityColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var columns = _generator.GetCreateColumns(table);

        // Assert
        columns.Should().NotContain(c => c.Name == "ID");
        columns.Should().Contain(c => c.Name == "Name");
    }

    /// <summary>
    /// Verifies that GetCreateColumns excludes read-only prefixed columns.
    /// </summary>
    [Fact]
    public void GetCreateColumns_ExcludesReadOnlyPrefixedColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("clc_Total", "decimal", precision: 18, scale: 2)
            .WithColumn("blg_Discount", "decimal", precision: 18, scale: 2)
            .WithColumn("agg_OrderCount", "int")
            .Build();

        // Act
        var columns = _generator.GetCreateColumns(table);

        // Assert
        columns.Should().NotContain(c => c.Name.StartsWith("clc_", StringComparison.OrdinalIgnoreCase));
        columns.Should().NotContain(c => c.Name.StartsWith("blg_", StringComparison.OrdinalIgnoreCase));
        columns.Should().NotContain(c => c.Name.StartsWith("agg_", StringComparison.OrdinalIgnoreCase));
        columns.Should().Contain(c => c.Name == "Name");
    }

    #endregion

    #region GetUpdateColumns Tests

    /// <summary>
    /// Verifies that GetUpdateColumns throws when table is null.
    /// </summary>
    [Fact]
    public void GetUpdateColumns_WithNullTable_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _generator.GetUpdateColumns(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("table");
    }

    /// <summary>
    /// Verifies that GetUpdateColumns excludes primary key columns.
    /// </summary>
    [Fact]
    public void GetUpdateColumns_ExcludesPrimaryKeyColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var columns = _generator.GetUpdateColumns(table);

        // Assert
        columns.Should().NotContain(c => c.IsPrimaryKey);
        columns.Should().Contain(c => c.Name == "Name");
    }

    /// <summary>
    /// Verifies that GetUpdateColumns excludes hashed columns (eno_).
    /// </summary>
    [Fact]
    public void GetUpdateColumns_ExcludesHashedColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("eno_Password", "nvarchar", maxLength: 128)
            .Build();

        // Act
        var columns = _generator.GetUpdateColumns(table);

        // Assert
        columns.Should().NotContain(c => c.Name.StartsWith("eno_", StringComparison.OrdinalIgnoreCase));
        columns.Should().Contain(c => c.Name == "Name");
    }

    /// <summary>
    /// Verifies that GetUpdateColumns excludes read-only prefixed columns.
    /// </summary>
    [Fact]
    public void GetUpdateColumns_ExcludesReadOnlyPrefixedColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("clc_Total", "decimal", precision: 18, scale: 2)
            .WithColumn("blg_Discount", "decimal", precision: 18, scale: 2)
            .WithColumn("agg_OrderCount", "int")
            .Build();

        // Act
        var columns = _generator.GetUpdateColumns(table);

        // Assert
        columns.Should().NotContain(c => c.Name.StartsWith("clc_", StringComparison.OrdinalIgnoreCase));
        columns.Should().NotContain(c => c.Name.StartsWith("blg_", StringComparison.OrdinalIgnoreCase));
        columns.Should().NotContain(c => c.Name.StartsWith("agg_", StringComparison.OrdinalIgnoreCase));
        columns.Should().Contain(c => c.Name == "Name");
    }

    #endregion

    #region GenerateValidationRules Tests

    /// <summary>
    /// Verifies that GenerateValidationRules generates NotEmpty for required string columns.
    /// </summary>
    [Fact]
    public void GenerateValidationRules_RequiredStringColumn_GeneratesNotEmpty()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("Name")
            .WithDataType("nvarchar")
            .WithMaxLength(100)
            .NotNullable()
            .Build();

        // Act
        var rules = _generator.GenerateValidationRules(column);

        // Assert
        rules.Should().Contain(".NotEmpty()");
    }

    /// <summary>
    /// Verifies that GenerateValidationRules generates MaximumLength for string columns.
    /// </summary>
    [Fact]
    public void GenerateValidationRules_StringColumnWithMaxLength_GeneratesMaximumLength()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("Name")
            .WithDataType("nvarchar")
            .WithMaxLength(100)
            .Build();

        // Act
        var rules = _generator.GenerateValidationRules(column);

        // Assert
        rules.Should().Contain(".MaximumLength(100)");
    }

    /// <summary>
    /// Verifies that GenerateValidationRules generates EmailAddress for email columns.
    /// </summary>
    [Fact]
    public void GenerateValidationRules_EmailColumn_GeneratesEmailAddress()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("Email")
            .WithDataType("nvarchar")
            .WithMaxLength(200)
            .Build();

        // Act
        var rules = _generator.GenerateValidationRules(column);

        // Assert
        rules.Should().Contain(".EmailAddress()");
    }

    /// <summary>
    /// Verifies that GenerateValidationRules generates Matches for phone columns.
    /// </summary>
    [Fact]
    public void GenerateValidationRules_PhoneColumn_GeneratesMatches()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("Phone")
            .WithDataType("nvarchar")
            .WithMaxLength(20)
            .Build();

        // Act
        var rules = _generator.GenerateValidationRules(column);

        // Assert
        rules.Should().Contain(".Matches(");
    }

    /// <summary>
    /// Verifies that GenerateValidationRules generates MinimumLength for password columns.
    /// </summary>
    [Fact]
    public void GenerateValidationRules_PasswordColumn_GeneratesMinimumLength()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("eno_Password")
            .WithDataType("nvarchar")
            .WithMaxLength(128)
            .Build();

        // Act
        var rules = _generator.GenerateValidationRules(column);

        // Assert
        rules.Should().Contain(".MinimumLength(8)");
    }

    /// <summary>
    /// Verifies that GenerateValidationRules returns empty for columns with no rules.
    /// </summary>
    [Fact]
    public void GenerateValidationRules_NullableIntColumn_ReturnsEmpty()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("Age")
            .WithDataType("int")
            .Nullable()
            .Build();

        // Act
        var rules = _generator.GenerateValidationRules(column);

        // Assert
        rules.Should().BeEmpty();
    }

    #endregion

    #region Error Handling and Logging Tests

    /// <summary>
    /// Verifies that generated handler includes try-catch error handling.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_AnyCommand_HandlerIncludesErrorHandling()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        result.HandlerCode.Should().Contain("try");
        result.HandlerCode.Should().Contain("catch (Exception ex)");
        result.HandlerCode.Should().Contain("_logger.LogError");
        result.HandlerCode.Should().Contain("throw;");
    }

    /// <summary>
    /// Verifies that generated handler includes logging.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_AnyCommand_HandlerIncludesLogging()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        result.HandlerCode.Should().Contain("_logger.LogDebug");
        result.HandlerCode.Should().Contain("_logger.LogInformation");
    }

    /// <summary>
    /// Verifies that Update and Delete handlers log warnings when entity not found.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_UpdateCommand_HandlerLogsWarningWhenNotFound()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Update);

        // Assert
        result.HandlerCode.Should().Contain("_logger.LogWarning");
    }

    #endregion

    #region Auto-Generated Header Tests

    /// <summary>
    /// Verifies that generated code includes auto-generated header.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_AnyCommand_IncludesAutoGeneratedHeader()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        result.CommandCode.Should().Contain("// <auto-generated>");
        result.CommandCode.Should().Contain("This code was generated by TargCC");
        result.HandlerCode.Should().Contain("// <auto-generated>");
        result.ValidatorCode.Should().Contain("// <auto-generated>");
    }

    #endregion

    #region Using Statements Tests

    /// <summary>
    /// Verifies that generated command includes correct using statements.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_AnyCommand_CommandIncludesUsingStatements()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        result.CommandCode.Should().Contain("using MediatR;");
        result.CommandCode.Should().Contain("using TargCC.Application.Common.Models;");
    }

    /// <summary>
    /// Verifies that generated handler includes correct using statements.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_AnyCommand_HandlerIncludesUsingStatements()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        result.HandlerCode.Should().Contain("using MediatR;");
        result.HandlerCode.Should().Contain("using Microsoft.Extensions.Logging;");
        result.HandlerCode.Should().Contain("using TargCC.Domain.Entities;");
        result.HandlerCode.Should().Contain("using TargCC.Domain.Interfaces;");
    }

    /// <summary>
    /// Verifies that generated validator includes correct using statements.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_AnyCommand_ValidatorIncludesUsingStatements()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        result.ValidatorCode.Should().Contain("using FluentValidation;");
    }

    #endregion

    #region Different Primary Key Types Tests

    /// <summary>
    /// Verifies that commands work with long primary key type.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithLongPrimaryKey_GeneratesCorrectType()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "bigint", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Delete);

        // Assert
        result.CommandCode.Should().Contain("DeleteCustomerCommand(long ID)");
        result.ValidatorCode.Should().Contain("GreaterThan(0)");
    }

    /// <summary>
    /// Verifies that commands work with Guid primary key type.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithGuidPrimaryKey_GeneratesCorrectType()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "uniqueidentifier", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Delete);

        // Assert
        result.CommandCode.Should().Contain("DeleteCustomerCommand(Guid ID)");
        result.ValidatorCode.Should().Contain("NotEmpty()");
    }

    /// <summary>
    /// Verifies that commands work with string primary key type.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithStringPrimaryKey_GeneratesCorrectType()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("Code", "nvarchar", maxLength: 50, isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Delete);

        // Assert
        result.CommandCode.Should().Contain("DeleteCustomerCommand(string Code)");
        result.ValidatorCode.Should().Contain("NotEmpty()");
    }

    #endregion

    #region Nullable Columns Tests

    /// <summary>
    /// Verifies that nullable columns generate nullable properties in commands.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_NullableColumns_GeneratesNullableProperties()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("Name", "nvarchar", maxLength: 100, isNullable: false)
            .WithColumn("Age", "int", isNullable: true)
            .WithColumn("BirthDate", "datetime", isNullable: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        result.CommandCode.Should().Contain("string Name { get; init; } = string.Empty;");
        result.CommandCode.Should().Contain("int? Age { get; init; }");
        result.CommandCode.Should().Contain("DateTime? BirthDate { get; init; }");
    }

    #endregion

    #region Prefix Handling Tests

    /// <summary>
    /// Verifies that TargCC prefixes are removed from property names.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_ColumnWithPrefix_RemovesPrefixFromPropertyName()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("lkp_Status", "nvarchar", maxLength: 50)
            .WithColumn("ent_CreditCard", "nvarchar", maxLength: 500)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        result.CommandCode.Should().Contain("Status");
        result.CommandCode.Should().Contain("CreditCard");
        result.CommandCode.Should().NotContain("lkp_");
        result.CommandCode.Should().NotContain("ent_");
    }

    #endregion

    #region Logging Verification Tests

    /// <summary>
    /// Verifies that generator logs information messages.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WhenCalled_LogsInformationMessages()
    {
        // Arrange
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        await _generator.GenerateAsync(table, CommandType.Create);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information || l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}
