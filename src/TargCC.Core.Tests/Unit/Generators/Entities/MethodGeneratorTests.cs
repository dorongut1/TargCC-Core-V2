// <copyright file="MethodGeneratorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators.Entities;

using TargCC.Core.Generators.Entities;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Tests.TestHelpers;
using Xunit;

/// <summary>
/// Unit tests for MethodGenerator class.
/// Tests entity method generation (Constructor, ToString, Clone, Equals, GetHashCode).
/// </summary>
public class MethodGeneratorTests
{
    #region GenerateConstructor Tests

    [Fact]
    public void GenerateConstructor_SimpleTable_ReturnsBasicConstructor()
    {
        // Arrange
        var table = TableBuilder.CreateSimpleTable("Customer");
        var schema = new DatabaseSchemaBuilder()
            .WithTable(table)
            .Build();

        // Act
        var result = MethodGenerator.GenerateConstructor(table, "Customer", schema);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public Customer()", result);
        Assert.Contains("/// <summary>", result);
        Assert.Contains("Initializes a new instance", result);
    }

    [Fact]
    public void GenerateConstructor_WithAuditColumn_InitializesAddedOn()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumns(
                ColumnBuilder.CreateIdColumn(),
                new ColumnBuilder()
                    .WithName("AddedOn")
                    .AsDateTime()
                    .NotNullable()
                    .Build())
            .Build();

        var schema = new DatabaseSchemaBuilder()
            .WithTable(table)
            .Build();

        // Act
        var result = MethodGenerator.GenerateConstructor(table, "Customer", schema);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("this.AddedOn = DateTime.Now;", result);
    }

    [Fact]
    public void GenerateConstructor_WithAggregateColumn_InitializesToZero()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumns(
                ColumnBuilder.CreateIdColumn(),
                new ColumnBuilder()
                    .WithName("agg_OrderCount")
                    .WithPrefix(ColumnPrefix.Aggregate)
                    .AsInt()
                    .NotNullable()
                    .Build())
            .Build();

        var schema = new DatabaseSchemaBuilder()
            .WithTable(table)
            .Build();

        // Act
        var result = MethodGenerator.GenerateConstructor(table, "Customer", schema);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("this.OrderCountAggregate = 0;", result);
    }

    [Fact]
    public void GenerateConstructor_WithRelationships_InitializesCollections()
    {
        // Arrange
        var customerTable = new TableBuilder()
            .WithName("Customer")
            .WithIdColumn()
            .Build();

        var orderTable = new TableBuilder()
            .WithName("Order")
            .WithIdColumn()
            .AddColumn(ColumnBuilder.CreateForeignKeyColumn("CustomerID"))
            .Build();

        var relationship = new Relationship
        {
            Name = "FK_Order_Customer",
            ParentTable = "Customer",
            ParentColumn = "ID",
            ChildTable = "Order",
            ChildColumn = "CustomerID",
        };

        customerTable.Relationships = new List<Relationship> { relationship };

        var schema = new DatabaseSchemaBuilder()
            .WithTable(customerTable)
            .WithTable(orderTable)
            .Build();

        // Act
        var result = MethodGenerator.GenerateConstructor(customerTable, "Customer", schema);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("this.Orders = new List<Order>();", result);
    }

    [Fact]
    public void GenerateConstructor_NullTable_ThrowsArgumentNullException()
    {
        // Arrange
        var schema = new DatabaseSchemaBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            MethodGenerator.GenerateConstructor(null!, "Customer", schema));
    }

    [Fact]
    public void GenerateConstructor_NullClassName_ThrowsArgumentNullException()
    {
        // Arrange
        var table = TableBuilder.CreateSimpleTable();
        var schema = new DatabaseSchemaBuilder().Build();

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            MethodGenerator.GenerateConstructor(table, null!, schema));
        Assert.NotNull(ex);
    }

    [Fact]
    public void GenerateConstructor_EmptyClassName_ThrowsArgumentException()
    {
        // Arrange
        var table = TableBuilder.CreateSimpleTable();
        var schema = new DatabaseSchemaBuilder().Build();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            MethodGenerator.GenerateConstructor(table, string.Empty, schema));
        Assert.NotNull(ex);
    }

    [Fact]
    public void GenerateConstructor_WhiteSpaceClassName_ThrowsArgumentException()
    {
        // Arrange
        var table = TableBuilder.CreateSimpleTable();
        var schema = new DatabaseSchemaBuilder().Build();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            MethodGenerator.GenerateConstructor(table, "   ", schema));
        Assert.NotNull(ex);
    }

    #endregion

    #region GenerateToString Tests

    [Fact]
    public void GenerateToString_TableWithPrimaryKeyAndName_ReturnsFormattedString()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumns(
                ColumnBuilder.CreateIdColumn(),
                ColumnBuilder.CreateNameColumn())
            .WithPrimaryKey("ID")
            .Build();

        // Act
        var result = MethodGenerator.GenerateToString(table, "Customer");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public override string ToString()", result);
        Assert.Contains("Customer #{ID}: {Name}", result);
    }

    [Fact]
    public void GenerateToString_TableWithPrimaryKeyOnly_ReturnsIdString()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithIdColumn()
            .WithPrimaryKey("ID")
            .Build();

        // Act
        var result = MethodGenerator.GenerateToString(table, "Customer");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Customer #{ID}", result);
        Assert.DoesNotContain("Name", result);
    }

    [Fact]
    public void GenerateToString_TableWithNoPrimaryKey_ReturnsSimpleString()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithNameColumn()
            .Build();

        // Act
        var result = MethodGenerator.GenerateToString(table, "Customer");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("return $\"Customer\";", result);
    }

    [Fact]
    public void GenerateToString_NullTable_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            MethodGenerator.GenerateToString(null!, "Customer"));
    }

    #endregion

    #region GenerateClone Tests

    [Fact]
    public void GenerateClone_SimpleTable_ClonesDataProperties()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumns(
                ColumnBuilder.CreateIdColumn(),
                ColumnBuilder.CreateNameColumn(),
                new ColumnBuilder()
                    .WithName("Email")
                    .AsNvarchar(100)
                    .Build())
            .WithPrimaryKey("ID")
            .Build();

        // Act
        var result = MethodGenerator.GenerateClone(table, "Customer");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public Customer Clone()", result);
        Assert.Contains("return new Customer", result);
        Assert.Contains("Name = this.Name", result);
        Assert.Contains("Email = this.Email", result);
        Assert.DoesNotContain("ID = this.ID", result); // PK not cloned
    }

    [Fact]
    public void GenerateClone_ExcludesAuditColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumns(
                ColumnBuilder.CreateIdColumn(),
                ColumnBuilder.CreateNameColumn(),
                new ColumnBuilder()
                    .WithName("AddedOn")
                    .AsDateTime()
                    .Build(),
                new ColumnBuilder()
                    .WithName("ChangedOn")
                    .AsDateTime()
                    .Build())
            .Build();

        // Act
        var result = MethodGenerator.GenerateClone(table, "Customer");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Name = this.Name", result);
        Assert.DoesNotContain("AddedOn", result);
        Assert.DoesNotContain("ChangedOn", result);
    }

    [Fact]
    public void GenerateClone_ExcludesCalculatedAndAggregateColumns()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumns(
                ColumnBuilder.CreateIdColumn(),
                ColumnBuilder.CreateNameColumn(),
                new ColumnBuilder()
                    .WithName("clc_TotalAmount")
                    .WithPrefix(ColumnPrefix.Calculated)
                    .AsDecimal()
                    .Build(),
                new ColumnBuilder()
                    .WithName("agg_OrderCount")
                    .WithPrefix(ColumnPrefix.Aggregate)
                    .AsInt()
                    .Build())
            .Build();

        // Act
        var result = MethodGenerator.GenerateClone(table, "Customer");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Name = this.Name", result);
        Assert.DoesNotContain("TotalAmountCalculated", result);
        Assert.DoesNotContain("OrderCountAggregate", result);
    }

    [Fact]
    public void GenerateClone_NullTable_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            MethodGenerator.GenerateClone(null!, "Customer"));
    }

    #endregion

    #region GenerateEquals Tests

    [Fact]
    public void GenerateEquals_TableWithPrimaryKey_ComparesByPrimaryKey()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithIdColumn()
            .WithPrimaryKey("ID")
            .Build();

        // Act
        var result = MethodGenerator.GenerateEquals(table, "Customer");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public override bool Equals(object obj)", result);
        Assert.Contains("if (obj is not Customer other)", result);
        Assert.Contains("this.ID > 0 && other.ID > 0", result);
        Assert.Contains("this.ID == other.ID", result);
    }

    [Fact]
    public void GenerateEquals_CompositePrimaryKey_ComparesAllKeys()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("OrderItem")
            .WithColumns(
                new ColumnBuilder()
                    .WithName("OrderID")
                    .AsInt()
                    .AsPrimaryKey()
                    .Build(),
                new ColumnBuilder()
                    .WithName("ItemID")
                    .AsInt()
                    .AsPrimaryKey()
                    .Build())
            .WithPrimaryKey("OrderID", "ItemID")
            .Build();

        // Act
        var result = MethodGenerator.GenerateEquals(table, "OrderItem");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("this.OrderID > 0 && other.OrderID > 0 && this.ItemID > 0 && other.ItemID > 0", result);
        Assert.Contains("this.OrderID == other.OrderID && this.ItemID == other.ItemID", result);
    }

    [Fact]
    public void GenerateEquals_TableWithEmail_FallsBackToEmail()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumns(
                ColumnBuilder.CreateIdColumn(),
                new ColumnBuilder()
                    .WithName("Email")
                    .AsNvarchar(100)
                    .Build())
            .WithPrimaryKey("ID")
            .Build();

        // Act
        var result = MethodGenerator.GenerateEquals(table, "Customer");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("this.Email == other.Email", result);
    }

    [Fact]
    public void GenerateEquals_NullTable_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            MethodGenerator.GenerateEquals(null!, "Customer"));
    }

    #endregion

    #region GenerateGetHashCode Tests

    [Fact]
    public void GenerateGetHashCode_SinglePrimaryKey_UsesKeyHashCode()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithIdColumn()
            .WithPrimaryKey("ID")
            .Build();

        // Act
        var result = MethodGenerator.GenerateGetHashCode(table);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public override int GetHashCode()", result);
        Assert.Contains("this.ID > 0 ? this.ID.GetHashCode() : 0", result);
    }

    [Fact]
    public void GenerateGetHashCode_CompositePrimaryKey_UsesHashCodeCombine()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("OrderItem")
            .WithColumns(
                new ColumnBuilder()
                    .WithName("OrderID")
                    .AsInt()
                    .AsPrimaryKey()
                    .Build(),
                new ColumnBuilder()
                    .WithName("ItemID")
                    .AsInt()
                    .AsPrimaryKey()
                    .Build())
            .WithPrimaryKey("OrderID", "ItemID")
            .Build();

        // Act
        var result = MethodGenerator.GenerateGetHashCode(table);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("HashCode.Combine(this.OrderID, this.ItemID)", result);
    }

    [Fact]
    public void GenerateGetHashCode_NoPrimaryKey_UsesBaseHashCode()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithNameColumn()
            .Build();

        // Act
        var result = MethodGenerator.GenerateGetHashCode(table);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("return base.GetHashCode();", result);
    }

    [Fact]
    public void GenerateGetHashCode_NullTable_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            MethodGenerator.GenerateGetHashCode(null!));
    }

    #endregion

    #region GenerateHelperMethods Tests

    [Fact]
    public void GenerateHelperMethods_NoEncryptedColumns_ReturnsEmptyString()
    {
        // Arrange
        var table = TableBuilder.CreateSimpleTable();

        // Act
        var result = MethodGenerator.GenerateHelperMethods(table);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GenerateHelperMethods_WithTwoWayEncryption_GeneratesEncryptDecrypt()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumns(
                ColumnBuilder.CreateIdColumn(),
                new ColumnBuilder()
                    .WithName("ent_SSN")
                    .WithPrefix(ColumnPrefix.TwoWayEncryption)
                    .Build())
            .Build();

        // Act
        var result = MethodGenerator.GenerateHelperMethods(table);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("private string EncryptValue(string plainText)", result);
        Assert.Contains("private string DecryptValue(string encrypted)", result);
        Assert.Contains("[PleaseEncrypt]", result);
        Assert.Contains("[PleaseDecrypt]", result);
    }

    [Fact]
    public void GenerateHelperMethods_WithOneWayEncryption_GeneratesSetPassword()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("User")
            .WithColumns(
                ColumnBuilder.CreateIdColumn(),
                new ColumnBuilder()
                    .WithName("eno_Password")
                    .WithPrefix(ColumnPrefix.OneWayEncryption)
                    .Build())
            .Build();

        // Act
        var result = MethodGenerator.GenerateHelperMethods(table);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public void SetPassword(string plainTextPassword)", result);
        Assert.Contains("this.PasswordHashed = $\"[PleaseHash]{plainTextPassword}\";", result);
    }

    [Fact]
    public void GenerateHelperMethods_NullTable_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            MethodGenerator.GenerateHelperMethods(null!));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void AllMethods_ComplexTable_GenerateCorrectly()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Customer")
            .WithColumns(
                ColumnBuilder.CreateIdColumn(),
                ColumnBuilder.CreateNameColumn(),
                new ColumnBuilder()
                    .WithName("Email")
                    .AsNvarchar(100)
                    .Build(),
                new ColumnBuilder()
                    .WithName("eno_Password")
                    .WithPrefix(ColumnPrefix.OneWayEncryption)
                    .Build(),
                new ColumnBuilder()
                    .WithName("agg_OrderCount")
                    .WithPrefix(ColumnPrefix.Aggregate)
                    .AsInt()
                    .Build(),
                new ColumnBuilder()
                    .WithName("AddedOn")
                    .AsDateTime()
                    .Build())
            .WithPrimaryKey("ID")
            .Build();

        var schema = new DatabaseSchemaBuilder()
            .WithTable(table)
            .Build();

        // Act
        var constructor = MethodGenerator.GenerateConstructor(table, "Customer", schema);
        var toString = MethodGenerator.GenerateToString(table, "Customer");
        var clone = MethodGenerator.GenerateClone(table, "Customer");
        var equals = MethodGenerator.GenerateEquals(table, "Customer");
        var getHashCode = MethodGenerator.GenerateGetHashCode(table);
        var helpers = MethodGenerator.GenerateHelperMethods(table);

        // Assert
        Assert.NotNull(constructor);
        Assert.Contains("this.AddedOn = DateTime.Now;", constructor);
        Assert.Contains("this.OrderCountAggregate = 0;", constructor);

        Assert.NotNull(toString);
        Assert.Contains("Customer #{ID}: {Name}", toString);

        Assert.NotNull(clone);
        Assert.Contains("Name = this.Name", clone);
        Assert.Contains("Email = this.Email", clone);

        Assert.NotNull(equals);
        Assert.Contains("this.ID == other.ID", equals);

        Assert.NotNull(getHashCode);
        Assert.Contains("this.ID.GetHashCode()", getHashCode);

        Assert.NotNull(helpers);
        Assert.Contains("SetPassword", helpers);
    }

    #endregion
}
