// <copyright file="RelationshipGeneratorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators.Entities;

using TargCC.Core.Generators.Entities;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Tests.TestHelpers;
using Xunit;

/// <summary>
/// Unit tests for RelationshipGenerator class.
/// Tests navigation property generation for entity relationships.
/// </summary>
public class RelationshipGeneratorTests
{
    #region GenerateNavigationProperties - Basic Tests

    [Fact]
    public void GenerateNavigationProperties_NoRelationships_ReturnsEmptyString()
    {
        // Arrange
        var table = TableBuilder.CreateSimpleTable();
        var schema = new DatabaseSchemaBuilder()
            .WithTable(table)
            .Build();

        // Act
        var result = RelationshipGenerator.GenerateNavigationProperties(table, schema);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GenerateNavigationProperties_NullRelationships_ReturnsEmptyString()
    {
        // Arrange
        var table = TableBuilder.CreateSimpleTable();
        table.Relationships = null;
        var schema = new DatabaseSchemaBuilder()
            .WithTable(table)
            .Build();

        // Act
        var result = RelationshipGenerator.GenerateNavigationProperties(table, schema);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GenerateNavigationProperties_NullTable_ThrowsArgumentNullException()
    {
        // Arrange
        var schema = new DatabaseSchemaBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            RelationshipGenerator.GenerateNavigationProperties(null!, schema));
    }

    [Fact]
    public void GenerateNavigationProperties_NullSchema_ThrowsArgumentNullException()
    {
        // Arrange
        var table = TableBuilder.CreateSimpleTable();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            RelationshipGenerator.GenerateNavigationProperties(table, null!));
    }

    #endregion

    #region One-to-Many Relationships

    [Fact]
    public void GenerateNavigationProperties_ParentTable_GeneratesCollectionProperty()
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
        var result = RelationshipGenerator.GenerateNavigationProperties(customerTable, schema);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public virtual ICollection<Order> Orders { get; set; }", result);
        Assert.Contains("Navigation property to Orders collection", result);
        Assert.Contains("/// <summary>", result);
    }

    [Fact]
    public void GenerateNavigationProperties_ChildTable_GeneratesReferenceProperty()
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

        orderTable.Relationships = new List<Relationship> { relationship };

        var schema = new DatabaseSchemaBuilder()
            .WithTable(customerTable)
            .WithTable(orderTable)
            .Build();

        // Act
        var result = RelationshipGenerator.GenerateNavigationProperties(orderTable, schema);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public virtual Customer Customer { get; set; }", result);
        Assert.Contains("Navigation property to parent Customer", result);
    }

    #endregion

    #region Multiple Relationships

    [Fact]
    public void GenerateNavigationProperties_MultipleChildren_GeneratesAllCollections()
    {
        // Arrange
        var customerTable = new TableBuilder()
            .WithName("Customer")
            .WithIdColumn()
            .Build();

        var orderTable = new TableBuilder()
            .WithName("Order")
            .WithIdColumn()
            .Build();

        var invoiceTable = new TableBuilder()
            .WithName("Invoice")
            .WithIdColumn()
            .Build();

        var orderRelationship = new Relationship
        {
            Name = "FK_Order_Customer",
            ParentTable = "Customer",
            ParentColumn = "ID",
            ChildTable = "Order",
            ChildColumn = "CustomerID",
        };

        var invoiceRelationship = new Relationship
        {
            Name = "FK_Invoice_Customer",
            ParentTable = "Customer",
            ParentColumn = "ID",
            ChildTable = "Invoice",
            ChildColumn = "CustomerID",
        };

        customerTable.Relationships = new List<Relationship> { orderRelationship, invoiceRelationship };

        var schema = new DatabaseSchemaBuilder()
            .WithTable(customerTable)
            .WithTable(orderTable)
            .WithTable(invoiceTable)
            .Build();

        // Act
        var result = RelationshipGenerator.GenerateNavigationProperties(customerTable, schema);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("ICollection<Order> Orders", result);
        Assert.Contains("ICollection<Invoice> Invoices", result);
    }

    [Fact]
    public void GenerateNavigationProperties_MixedParentChild_GeneratesBothTypes()
    {
        // Arrange
        // Order is child of Customer but parent of OrderItem
        var customerTable = new TableBuilder()
            .WithName("Customer")
            .WithIdColumn()
            .Build();

        var orderTable = new TableBuilder()
            .WithName("Order")
            .WithIdColumn()
            .Build();

        var orderItemTable = new TableBuilder()
            .WithName("OrderItem")
            .WithIdColumn()
            .Build();

        var customerRelationship = new Relationship
        {
            Name = "FK_Order_Customer",
            ParentTable = "Customer",
            ParentColumn = "ID",
            ChildTable = "Order",
            ChildColumn = "CustomerID",
        };

        var orderItemRelationship = new Relationship
        {
            Name = "FK_OrderItem_Order",
            ParentTable = "Order",
            ParentColumn = "ID",
            ChildTable = "OrderItem",
            ChildColumn = "OrderID",
        };

        orderTable.Relationships = new List<Relationship> { customerRelationship, orderItemRelationship };

        var schema = new DatabaseSchemaBuilder()
            .WithTable(customerTable)
            .WithTable(orderTable)
            .WithTable(orderItemTable)
            .Build();

        // Act
        var result = RelationshipGenerator.GenerateNavigationProperties(orderTable, schema);

        // Assert
        Assert.NotNull(result);
        // As parent
        Assert.Contains("ICollection<OrderItem> OrderItems", result);
        // As child
        Assert.Contains("Customer Customer", result);
    }

    #endregion

    #region Pluralization Rules

    [Fact]
    public void GenerateNavigationProperties_PluralWithS_AddsS()
    {
        // Arrange - "Order" -> "Orders"
        var customerTable = new TableBuilder()
            .WithName("Customer")
            .WithIdColumn()
            .Build();

        var orderTable = new TableBuilder()
            .WithName("Order")
            .WithIdColumn()
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
        var result = RelationshipGenerator.GenerateNavigationProperties(customerTable, schema);

        // Assert
        Assert.Contains("Orders", result);
    }

    [Fact]
    public void GenerateNavigationProperties_PluralWithY_ChangesToIes()
    {
        // Arrange - "Category" -> "Categories"
        var productTable = new TableBuilder()
            .WithName("Product")
            .WithIdColumn()
            .Build();

        var categoryTable = new TableBuilder()
            .WithName("Category")
            .WithIdColumn()
            .Build();

        var relationship = new Relationship
        {
            Name = "FK_Category_Product",
            ParentTable = "Product",
            ParentColumn = "ID",
            ChildTable = "Category",
            ChildColumn = "ProductID",
        };

        productTable.Relationships = new List<Relationship> { relationship };

        var schema = new DatabaseSchemaBuilder()
            .WithTable(productTable)
            .WithTable(categoryTable)
            .Build();

        // Act
        var result = RelationshipGenerator.GenerateNavigationProperties(productTable, schema);

        // Assert
        Assert.Contains("Categories", result);
    }

    [Fact]
    public void GenerateNavigationProperties_PluralWithSuffix_AddsEs()
    {
        // Arrange - "Address" -> "Addresses"
        var customerTable = new TableBuilder()
            .WithName("Customer")
            .WithIdColumn()
            .Build();

        var addressTable = new TableBuilder()
            .WithName("Address")
            .WithIdColumn()
            .Build();

        var relationship = new Relationship
        {
            Name = "FK_Address_Customer",
            ParentTable = "Customer",
            ParentColumn = "ID",
            ChildTable = "Address",
            ChildColumn = "CustomerID",
        };

        customerTable.Relationships = new List<Relationship> { relationship };

        var schema = new DatabaseSchemaBuilder()
            .WithTable(customerTable)
            .WithTable(addressTable)
            .Build();

        // Act
        var result = RelationshipGenerator.GenerateNavigationProperties(customerTable, schema);

        // Assert
        Assert.Contains("Addresses", result);
    }

    #endregion

    #region Table Name Cleanup

    [Fact]
    public void GenerateNavigationProperties_TableWithTblPrefix_RemovesPrefix()
    {
        // Arrange
        var customerTable = new TableBuilder()
            .WithName("tbl_Customer")
            .WithIdColumn()
            .Build();

        var orderTable = new TableBuilder()
            .WithName("tbl_Order")
            .WithIdColumn()
            .Build();

        var relationship = new Relationship
        {
            Name = "FK_Order_Customer",
            ParentTable = "tbl_Customer",
            ParentColumn = "ID",
            ChildTable = "tbl_Order",
            ChildColumn = "CustomerID",
        };

        customerTable.Relationships = new List<Relationship> { relationship };

        var schema = new DatabaseSchemaBuilder()
            .WithTable(customerTable)
            .WithTable(orderTable)
            .Build();

        // Act
        var result = RelationshipGenerator.GenerateNavigationProperties(customerTable, schema);

        // Assert
        Assert.Contains("ICollection<Order> Orders", result);
        Assert.DoesNotContain("tbl_", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GenerateNavigationProperties_RelatedTableNotInSchema_SkipsProperty()
    {
        // Arrange
        var customerTable = new TableBuilder()
            .WithName("Customer")
            .WithIdColumn()
            .Build();

        var relationship = new Relationship
        {
            Name = "FK_Order_Customer",
            ParentTable = "Customer",
            ParentColumn = "ID",
            ChildTable = "NonExistentTable", // Not in schema
            ChildColumn = "CustomerID",
        };

        customerTable.Relationships = new List<Relationship> { relationship };

        var schema = new DatabaseSchemaBuilder()
            .WithTable(customerTable)
            .Build();

        // Act
        var result = RelationshipGenerator.GenerateNavigationProperties(customerTable, schema);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result); // Should skip the property
    }

    [Fact]
    public void GenerateNavigationProperties_EmptyRelationshipsList_ReturnsEmptyString()
    {
        // Arrange
        var table = TableBuilder.CreateSimpleTable();
        table.Relationships = new List<Relationship>(); // Empty list
        var schema = new DatabaseSchemaBuilder()
            .WithTable(table)
            .Build();

        // Act
        var result = RelationshipGenerator.GenerateNavigationProperties(table, schema);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion
}
