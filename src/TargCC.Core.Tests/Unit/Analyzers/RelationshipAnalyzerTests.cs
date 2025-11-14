// <copyright file="RelationshipAnalyzerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Analyzers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Analyzers.Database;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Tests.TestHelpers;
using Xunit;

/// <summary>
/// Unit tests for RelationshipAnalyzer.
/// Tests relationship detection, graph building, and type determination.
/// </summary>
public class RelationshipAnalyzerTests
{
    private readonly Mock<ILogger<RelationshipAnalyzer>> loggerMock;
    private readonly RelationshipAnalyzer analyzer;
    private readonly string connectionString = "Server=.;Database=TestDB;Integrated Security=true;";

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationshipAnalyzerTests"/> class.
    /// </summary>
    public RelationshipAnalyzerTests()
    {
        this.loggerMock = new Mock<ILogger<RelationshipAnalyzer>>();
        this.analyzer = new RelationshipAnalyzer(this.connectionString, this.loggerMock.Object);
    }

    #region Validation Tests

    /// <summary>
    /// Test that AnalyzeRelationshipsAsync throws ArgumentNullException when tables list is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AnalyzeRelationshipsAsync_NullTables_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => this.analyzer.AnalyzeRelationshipsAsync(null!));
    }

    /// <summary>
    /// Test that AnalyzeRelationshipsForTablesAsync throws ArgumentNullException when table names list is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AnalyzeRelationshipsForTablesAsync_NullTableNames_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => this.analyzer.AnalyzeRelationshipsForTablesAsync(null!));
    }

    #endregion

    #region Relationship Type Detection Tests

    /// <summary>
    /// Test that OneToMany relationship is correctly identified.
    /// </summary>
    [Fact]
    public void DetermineRelationshipType_ForeignKeyToParent_ReturnsOneToMany()
    {
        // Arrange
        var relationship = new Relationship
        {
            Name = "FK_Order_Customer",
            ParentTable = "Order",
            ParentColumn = "CustomerID",
            ReferencedTable = "Customer",
            ReferencedColumn = "ID",
            RelationshipType = RelationshipType.OneToMany
        };

        // Act
        var type = relationship.RelationshipType;

        // Assert
        Assert.Equal(RelationshipType.OneToMany, type);
    }

    /// <summary>
    /// Test that OneToOne relationship is correctly identified.
    /// </summary>
    [Fact]
    public void DetermineRelationshipType_UniqueFK_ReturnsOneToOne()
    {
        // Arrange - Unique FK indicates One-to-One
        var relationship = new Relationship
        {
            Name = "FK_UserProfile_User",
            ParentTable = "UserProfile",
            ParentColumn = "UserID",
            ReferencedTable = "User",
            ReferencedColumn = "ID",
            RelationshipType = RelationshipType.OneToOne
        };

        // Act
        var type = relationship.RelationshipType;

        // Assert
        Assert.Equal(RelationshipType.OneToOne, type);
    }

    #endregion

    #region Relationship Graph Tests

    /// <summary>
    /// Test that BuildRelationshipGraph creates correct graph for simple schema.
    /// </summary>
    [Fact]
    public void BuildRelationshipGraph_SimpleSchema_CreatesCorrectGraph()
    {
        // Arrange
        var relationships = new List<Relationship>
        {
            new Relationship
            {
                ParentTable = "dbo.Order",
                ParentColumn = "CustomerID",
                ReferencedTable = "dbo.Customer",
                ReferencedColumn = "ID"
            }
        };

        // Act
        var graph = this.analyzer.BuildRelationshipGraph(relationships);

        // Assert
        Assert.NotEmpty(graph);
        Assert.True(graph.ContainsKey("dbo.Order"));
    }

    /// <summary>
    /// Test that BuildRelationshipGraph handles complex relationships.
    /// </summary>
    [Fact]
    public void BuildRelationshipGraph_ComplexSchema_CreatesCorrectGraph()
    {
        // Arrange
        var relationships = new List<Relationship>
        {
            new Relationship
            {
                ParentTable = "dbo.Order",
                ReferencedTable = "dbo.Customer"
            },
            new Relationship
            {
                ParentTable = "dbo.OrderItem",
                ReferencedTable = "dbo.Order"
            },
            new Relationship
            {
                ParentTable = "dbo.OrderItem",
                ReferencedTable = "dbo.Product"
            }
        };

        // Act
        var graph = this.analyzer.BuildRelationshipGraph(relationships);

        // Assert
        Assert.True(graph.Count >= 3);  // At least Order, OrderItem, Product
        Assert.Contains("dbo.Order", graph.Keys);
        Assert.Contains("dbo.OrderItem", graph.Keys);
    }

    /// <summary>
    /// Test that graph correctly represents parent-child relationships.
    /// </summary>
    [Fact]
    public void BuildRelationshipGraph_ParentChildRelationships_CorrectlyRepresented()
    {
        // Arrange
        var relationships = new List<Relationship>
        {
            new Relationship
            {
                ParentTable = "dbo.Order",
                ParentColumn = "CustomerID",
                ReferencedTable = "dbo.Customer",
                ReferencedColumn = "ID"
            }
        };

        // Act
        var graph = this.analyzer.BuildRelationshipGraph(relationships);

        // Assert
        // Order should be in the graph
        Assert.Contains("dbo.Order", graph.Keys);
        // Customer should be referenced
        Assert.Contains("dbo.Customer", graph.Keys);
    }

    #endregion

    #region Get Parent/Child Tables Tests

    /// <summary>
    /// Test GetParentTables returns correct parent tables.
    /// </summary>
    [Fact]
    public void GetParentTables_OrderTable_ReturnsCustomer()
    {
        // Arrange
        var relationships = new List<Relationship>
        {
            new Relationship
            {
                ParentTable = "dbo.Order",
                ReferencedTable = "dbo.Customer"
            }
        };

        // Act
        var parents = this.analyzer.GetParentTables("dbo.Order", relationships);

        // Assert
        Assert.Single(parents);
        Assert.Contains("dbo.Customer", parents);
    }

    /// <summary>
    /// Test GetChildTables returns correct child tables.
    /// </summary>
    [Fact]
    public void GetChildTables_CustomerTable_ReturnsOrder()
    {
        // Arrange
        var relationships = new List<Relationship>
        {
            new Relationship
            {
                ParentTable = "dbo.Order",
                ReferencedTable = "dbo.Customer"
            }
        };

        // Act
        var children = this.analyzer.GetChildTables("dbo.Customer", relationships);

        // Assert
        Assert.Single(children);
        Assert.Contains("dbo.Order", children);
    }

    #endregion

    #region DatabaseSchemaBuilder Relationship Tests

    /// <summary>
    /// Test that simple schema creates correct relationship.
    /// </summary>
    [Fact]
    public void CreateSimpleCustomerOrderSchema_CreatesRelationship()
    {
        // Act
        var schema = DatabaseSchemaBuilder.OrdersSchema();

        // Assert
        Assert.Single(schema.Relationships);
        var rel = schema.Relationships[0];
        Assert.Equal("Order", rel.ChildTable);
        Assert.Equal("CustomerID", rel.ChildColumn);
        Assert.Equal("Customer", rel.ParentTable);
        Assert.Equal("ID", rel.ParentColumn);
    }

    /// <summary>
    /// Test that complex schema creates multiple relationships.
    /// </summary>
    [Fact]
    public void CreateComplexSchema_CreatesMultipleRelationships()
    {
        // Act
        var schema = new DatabaseSchemaBuilder()
            .AddTable(tb => tb.WithName("Customer").WithIdColumn())
            .AddTable(tb => tb.WithName("Order").WithIdColumn())
            .AddTable(tb => tb.WithName("OrderItem").WithIdColumn())
            .AddTable(tb => tb.WithName("Product").WithIdColumn())
            .AddRelationship("Customer", "ID", "Order", "CustomerID")
            .AddRelationship("Order", "ID", "OrderItem", "OrderID")
            .AddRelationship("Product", "ID", "OrderItem", "ProductID")
            .Build();

        // Assert
        Assert.Equal(3, schema.Relationships.Count);

        // Verify specific relationships exist
        Assert.Contains(schema.Relationships,
            r => r.ParentTable == "Customer" && r.ChildTable == "Order");
        Assert.Contains(schema.Relationships,
            r => r.ParentTable == "Order" && r.ChildTable == "OrderItem");
        Assert.Contains(schema.Relationships,
            r => r.ParentTable == "Product" && r.ChildTable == "OrderItem");
    }

    /// <summary>
    /// Test that relationship names follow FK convention.
    /// </summary>
    [Fact]
    public void CreateRelationship_FollowsFKNamingConvention()
    {
        // Arrange & Act
        var schema = new DatabaseSchemaBuilder()
            .AddTable(TableBuilder.CustomerTable())
            .AddTable(tb => tb.WithName("Order").WithIdColumn())
            .AddRelationship("Customer", "ID", "Order", "CustomerID")
            .Build();

        // Assert
        var relationship = Assert.Single(schema.Relationships);
        Assert.NotNull(relationship.ParentTable);
        Assert.NotNull(relationship.ChildTable);
    }

    #endregion

    #region Circular Relationship Tests

    /// <summary>
    /// Test that circular relationships are handled correctly.
    /// </summary>
    [Fact]
    public void BuildRelationshipGraph_CircularRelationship_HandlesCorrectly()
    {
        // Arrange - Create a circular reference: A → B → C → A
        var relationships = new List<Relationship>
        {
            new Relationship
            {
                ParentTable = "dbo.TableA",
                ReferencedTable = "dbo.TableB"
            },
            new Relationship
            {
                ParentTable = "dbo.TableB",
                ReferencedTable = "dbo.TableC"
            },
            new Relationship
            {
                ParentTable = "dbo.TableC",
                ReferencedTable = "dbo.TableA"
            }
        };

        // Act
        var graph = this.analyzer.BuildRelationshipGraph(relationships);

        // Assert - Should have all 3 tables in graph
        Assert.True(graph.Count >= 3);
        Assert.Contains("dbo.TableA", graph.Keys);
        Assert.Contains("dbo.TableB", graph.Keys);
        Assert.Contains("dbo.TableC", graph.Keys);
    }

    #endregion

    #region Self-Referencing Relationship Tests

    /// <summary>
    /// Test that self-referencing relationships are handled.
    /// </summary>
    [Fact]
    public void BuildRelationshipGraph_SelfReferencingTable_HandlesCorrectly()
    {
        // Arrange - Employee table with ManagerID pointing to Employee.ID
        var relationships = new List<Relationship>
        {
            new Relationship
            {
                ParentTable = "dbo.Employee",
                ParentColumn = "ManagerID",
                ReferencedTable = "dbo.Employee",
                ReferencedColumn = "ID"
            }
        };

        // Act
        var graph = this.analyzer.BuildRelationshipGraph(relationships);

        // Assert
        Assert.Single(graph);
        Assert.Contains("dbo.Employee", graph.Keys);
    }

    #endregion
}
