// <copyright file="DtoGeneratorTests.cs" company="TargCC">
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
/// Unit tests for <see cref="DtoGenerator"/>.
/// </summary>
public class DtoGeneratorTests
{
    private readonly Mock<ILogger<DtoGenerator>> _loggerMock;
    private readonly DtoGenerator _generator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DtoGeneratorTests"/> class.
    /// </summary>
    public DtoGeneratorTests()
    {
        _loggerMock = new Mock<ILogger<DtoGenerator>>();
        _generator = new DtoGenerator(_loggerMock.Object);
    }

    #region Constructor Tests

    /// <summary>
    /// Verifies that constructor throws when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new DtoGenerator(null!);

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
        var act = () => _generator.GenerateAsync(null!, DtoType.Basic);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("table");
    }

    #endregion

    #region Basic DTO Tests

    /// <summary>
    /// Verifies that GenerateAsync generates Basic DTO with correct class name.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_BasicDto_GeneratesCorrectClassName()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Basic);

        // Assert
        result.Should().NotBeNull();
        result.ClassName.Should().Be("CustomerDto");
        result.DtoType.Should().Be(DtoType.Basic);
    }

    /// <summary>
    /// Verifies that Basic DTO excludes sensitive columns (eno_).
    /// </summary>
    [Fact]
    public async Task GenerateAsync_BasicDto_ExcludesHashedColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("eno_Password", "nvarchar", maxLength: 128)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Basic);

        // Assert
        result.Code.Should().Contain("Name");
        result.Code.Should().NotContain("Password");
        result.Code.Should().NotContain("eno_");
    }

    /// <summary>
    /// Verifies that Basic DTO excludes encrypted columns (ent_).
    /// </summary>
    [Fact]
    public async Task GenerateAsync_BasicDto_ExcludesEncryptedColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("ent_CreditCard", "nvarchar", maxLength: 500)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Basic);

        // Assert
        result.Code.Should().Contain("Name");
        result.Code.Should().NotContain("CreditCard");
        result.Code.Should().NotContain("ent_");
    }

    /// <summary>
    /// Verifies that Basic DTO excludes computed columns (clc_, blg_, agg_).
    /// </summary>
    [Fact]
    public async Task GenerateAsync_BasicDto_ExcludesComputedColumns()
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
        var result = await _generator.GenerateAsync(table, DtoType.Basic);

        // Assert
        result.Code.Should().Contain("Name");
        result.Code.Should().NotContain("Total");
        result.Code.Should().NotContain("Discount");
        result.Code.Should().NotContain("OrderCount");
    }

    #endregion

    #region List DTO Tests

    /// <summary>
    /// Verifies that List DTO has correct class name.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_ListDto_GeneratesCorrectClassName()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.List);

        // Assert
        result.ClassName.Should().Be("CustomerListDto");
        result.DtoType.Should().Be(DtoType.List);
    }

    /// <summary>
    /// Verifies that List DTO includes primary key.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_ListDto_IncludesPrimaryKey()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("Description", "nvarchar", maxLength: 4000)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.List);

        // Assert
        result.Properties.Should().Contain("ID");
    }

    /// <summary>
    /// Verifies that List DTO includes display fields (Name, Email, Status).
    /// </summary>
    [Fact]
    public async Task GenerateAsync_ListDto_IncludesDisplayFields()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("Email", "nvarchar", maxLength: 200)
            .WithColumn("Status", "nvarchar", maxLength: 50)
            .WithColumn("Notes", "nvarchar", maxLength: 4000)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.List);

        // Assert
        result.Properties.Should().Contain("Name");
        result.Properties.Should().Contain("Email");
        result.Properties.Should().Contain("Status");
    }

    #endregion

    #region Detail DTO Tests

    /// <summary>
    /// Verifies that Detail DTO has correct class name.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_DetailDto_GeneratesCorrectClassName()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Detail);

        // Assert
        result.ClassName.Should().Be("CustomerDetailDto");
        result.DtoType.Should().Be(DtoType.Detail);
    }

    /// <summary>
    /// Verifies that Detail DTO includes computed columns.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_DetailDto_IncludesComputedColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("clc_Total", "decimal", precision: 18, scale: 2)
            .WithColumn("agg_OrderCount", "int")
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Detail);

        // Assert
        result.Properties.Should().Contain("clc_Total");
        result.Properties.Should().Contain("agg_OrderCount");
    }

    /// <summary>
    /// Verifies that Detail DTO excludes sensitive columns.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_DetailDto_ExcludesSensitiveColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("eno_Password", "nvarchar", maxLength: 128)
            .WithColumn("ent_SSN", "nvarchar", maxLength: 200)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Detail);

        // Assert
        result.Properties.Should().NotContain("eno_Password");
        result.Properties.Should().NotContain("ent_SSN");
    }

    #endregion

    #region Create DTO Tests

    /// <summary>
    /// Verifies that Create DTO has correct class name.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_CreateDto_GeneratesCorrectClassName()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Create);

        // Assert
        result.ClassName.Should().Be("CreateCustomerDto");
        result.DtoType.Should().Be(DtoType.Create);
    }

    /// <summary>
    /// Verifies that Create DTO excludes auto-increment primary key.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_CreateDto_ExcludesIdentityPrimaryKey()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Create);

        // Assert
        result.Properties.Should().NotContain("ID");
        result.Properties.Should().Contain("Name");
    }

    /// <summary>
    /// Verifies that Create DTO excludes auto-generated timestamp columns.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_CreateDto_ExcludesTimestampColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("AddedOn", "datetime")
            .WithColumn("AddedBy", "nvarchar", maxLength: 100)
            .WithColumn("ChangedOn", "datetime")
            .WithColumn("ChangedBy", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Create);

        // Assert
        result.Properties.Should().NotContain("AddedOn");
        result.Properties.Should().NotContain("AddedBy");
        result.Properties.Should().NotContain("ChangedOn");
        result.Properties.Should().NotContain("ChangedBy");
    }

    /// <summary>
    /// Verifies that Create DTO uses 'set' accessor.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_CreateDto_UsesSetAccessor()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isIdentity: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Create);

        // Assert
        result.Code.Should().Contain("{ get; set; }");
    }

    #endregion

    #region Update DTO Tests

    /// <summary>
    /// Verifies that Update DTO has correct class name.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_UpdateDto_GeneratesCorrectClassName()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Update);

        // Assert
        result.ClassName.Should().Be("UpdateCustomerDto");
        result.DtoType.Should().Be(DtoType.Update);
    }

    /// <summary>
    /// Verifies that Update DTO includes primary key.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_UpdateDto_IncludesPrimaryKey()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Update);

        // Assert
        result.Properties.Should().Contain("ID");
    }

    /// <summary>
    /// Verifies that Update DTO excludes sensitive columns.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_UpdateDto_ExcludesSensitiveColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("eno_Password", "nvarchar", maxLength: 128)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Update);

        // Assert
        result.Properties.Should().NotContain("eno_Password");
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
    /// Verifies that GenerateAllAsync generates all five DTO types.
    /// </summary>
    [Fact]
    public async Task GenerateAllAsync_GeneratesAllFiveDtoTypes()
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
        resultList.Should().HaveCount(5);
        resultList.Should().Contain(r => r.ClassName == "CustomerDto");
        resultList.Should().Contain(r => r.ClassName == "CustomerListDto");
        resultList.Should().Contain(r => r.ClassName == "CustomerDetailDto");
        resultList.Should().Contain(r => r.ClassName == "CreateCustomerDto");
        resultList.Should().Contain(r => r.ClassName == "UpdateCustomerDto");
    }

    #endregion

    #region GetPropertiesForDtoType Tests

    /// <summary>
    /// Verifies that GetPropertiesForDtoType throws when table is null.
    /// </summary>
    [Fact]
    public void GetPropertiesForDtoType_WithNullTable_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _generator.GetPropertiesForDtoType(null!, DtoType.Basic);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("table");
    }

    /// <summary>
    /// Verifies that GetPropertiesForDtoType returns correct properties for Basic type.
    /// </summary>
    [Fact]
    public void GetPropertiesForDtoType_BasicType_ExcludesSensitiveAndComputed()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .WithColumn("eno_Password", "nvarchar", maxLength: 128)
            .WithColumn("clc_Total", "decimal", precision: 18, scale: 2)
            .Build();

        // Act
        var properties = _generator.GetPropertiesForDtoType(table, DtoType.Basic);

        // Assert
        properties.Should().Contain("ID");
        properties.Should().Contain("Name");
        properties.Should().NotContain("eno_Password");
        properties.Should().NotContain("clc_Total");
    }

    #endregion

    #region IsSensitiveColumn Tests

    /// <summary>
    /// Verifies that IsSensitiveColumn returns true for eno_ columns.
    /// </summary>
    [Fact]
    public void IsSensitiveColumn_WithEnoPrefix_ReturnsTrue()
    {
        // Act
        var result = _generator.IsSensitiveColumn("eno_Password");

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsSensitiveColumn returns true for ent_ columns.
    /// </summary>
    [Fact]
    public void IsSensitiveColumn_WithEntPrefix_ReturnsTrue()
    {
        // Act
        var result = _generator.IsSensitiveColumn("ent_CreditCard");

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsSensitiveColumn returns false for regular columns.
    /// </summary>
    [Fact]
    public void IsSensitiveColumn_RegularColumn_ReturnsFalse()
    {
        // Act
        var result = _generator.IsSensitiveColumn("Name");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsSensitiveColumn returns false for empty string.
    /// </summary>
    [Fact]
    public void IsSensitiveColumn_EmptyString_ReturnsFalse()
    {
        // Act
        var result = _generator.IsSensitiveColumn(string.Empty);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Auto-Generated Header Tests

    /// <summary>
    /// Verifies that generated DTO includes auto-generated header.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_AnyDtoType_IncludesAutoGeneratedHeader()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Basic);

        // Assert
        result.Code.Should().Contain("// <auto-generated>");
        result.Code.Should().Contain("This code was generated by TargCC");
        result.Code.Should().Contain("Source Table: Customer");
        result.Code.Should().Contain("DTO Type: Basic");
    }

    #endregion

    #region Namespace Tests

    /// <summary>
    /// Verifies that generated DTO has correct namespace.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_AnyDtoType_GeneratesCorrectNamespace()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Basic);

        // Assert
        result.Namespace.Should().Be("TargCC.Application.Features.Customers.Dtos");
        result.Code.Should().Contain("namespace TargCC.Application.Features.Customers.Dtos;");
    }

    #endregion

    #region Type Mapping Tests

    /// <summary>
    /// Verifies correct C# type for int column.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IntColumn_GeneratesIntProperty()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isNullable: false)
            .WithColumn("Age", "int", isNullable: false)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Basic);

        // Assert
        result.Code.Should().Contain("public int Age");
    }

    /// <summary>
    /// Verifies correct C# type for nullable int column.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_NullableIntColumn_GeneratesNullableIntProperty()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Age", "int", isNullable: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Basic);

        // Assert
        result.Code.Should().Contain("public int? Age");
    }

    /// <summary>
    /// Verifies correct C# type for string column.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_StringColumn_GeneratesStringProperty()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100, isNullable: false)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Basic);

        // Assert
        result.Code.Should().Contain("public string Name { get; init; } = string.Empty;");
    }

    /// <summary>
    /// Verifies correct C# type for decimal column.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_DecimalColumn_GeneratesDecimalProperty()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Order")
            .WithColumn("ID", "int", isPrimaryKey: true, isNullable: false)
            .WithColumn("Total", "decimal", precision: 18, scale: 2, isNullable: false)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Basic);

        // Assert
        result.Code.Should().Contain("public decimal Total");
    }

    /// <summary>
    /// Verifies correct C# type for datetime column.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_DateTimeColumn_GeneratesDateTimeProperty()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Order")
            .WithColumn("ID", "int", isPrimaryKey: true, isNullable: false)
            .WithColumn("OrderDate", "datetime", isNullable: false)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Basic);

        // Assert
        result.Code.Should().Contain("public DateTime OrderDate");
    }

    /// <summary>
    /// Verifies correct C# type for guid column.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GuidColumn_GeneratesGuidProperty()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "uniqueidentifier", isPrimaryKey: true, isNullable: false)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Basic);

        // Assert
        result.Code.Should().Contain("public Guid ID");
    }

    /// <summary>
    /// Verifies correct C# type for bool column.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_BitColumn_GeneratesBoolProperty()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true, isNullable: false)
            .WithColumn("IsActive", "bit", isNullable: false)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Basic);

        // Assert
        result.Code.Should().Contain("public bool IsActive");
    }

    #endregion

    #region XML Documentation Tests

    /// <summary>
    /// Verifies that properties have XML documentation.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_AnyDtoType_PropertiesHaveXmlDocumentation()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("Name", "nvarchar", maxLength: 100)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Basic);

        // Assert
        result.Code.Should().Contain("/// <summary>");
        result.Code.Should().Contain("/// Gets or sets the");
        result.Code.Should().Contain("/// </summary>");
    }

    /// <summary>
    /// Verifies that class has XML documentation.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_AnyDtoType_ClassHasXmlDocumentation()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Basic);

        // Assert
        result.Code.Should().Contain("/// <summary>");
        result.Code.Should().Contain("Data transfer object for Customer");
    }

    #endregion

    #region Logging Verification Tests

    /// <summary>
    /// Verifies that generator logs information messages.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WhenCalled_LogsMessages()
    {
        // Arrange
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .Build();

        // Act
        await _generator.GenerateAsync(table, DtoType.Basic);

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

    #region Prefix Sanitization Tests

    /// <summary>
    /// Verifies that TargCC prefixes are removed from property names in output.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_ColumnWithPrefix_SanitizesPropertyName()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumn("ID", "int", isPrimaryKey: true)
            .WithColumn("lkp_Status", "nvarchar", maxLength: 50)
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table, DtoType.Basic);

        // Assert
        result.Code.Should().Contain("public string Status");
        result.Code.Should().NotContain("lkp_Status");
    }

    #endregion
}
