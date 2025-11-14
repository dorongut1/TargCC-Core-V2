// <copyright file="DatabaseSchemaBuilder.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.TestHelpers;

using TargCC.Core.Interfaces.Models;

/// <summary>
/// Builder pattern for creating test DatabaseSchema instances.
/// Provides fluent API for easy test data creation.
/// </summary>
public class DatabaseSchemaBuilder
{
    private string databaseName = "TestDatabase";
    private string serverName = "localhost";
    private List<Table> tables = new();
    private List<Relationship> relationships = new();

    /// <summary>
    /// Sets the database name.
    /// </summary>
    /// <param name="name">The database name.</param>
    /// <returns>The builder instance.</returns>
    public DatabaseSchemaBuilder WithDatabaseName(string name)
    {
        this.databaseName = name;
        return this;
    }

    /// <summary>
    /// Alias for WithDatabaseName.
    /// </summary>
    public DatabaseSchemaBuilder WithName(string name) => this.WithDatabaseName(name);

    /// <summary>
    /// Sets the server name.
    /// </summary>
    /// <param name="server">The server name.</param>
    /// <returns>The builder instance.</returns>
    public DatabaseSchemaBuilder WithServerName(string server)
    {
        this.serverName = server;
        return this;
    }

    /// <summary>
    /// Alias for WithServerName.
    /// </summary>
    public DatabaseSchemaBuilder WithServer(string server) => this.WithServerName(server);

    /// <summary>
    /// Adds a table to the schema.
    /// </summary>
    /// <param name="table">The table to add.</param>
    /// <returns>The builder instance.</returns>
    public DatabaseSchemaBuilder WithTable(Table table)
    {
        this.tables.Add(table);
        return this;
    }

    /// <summary>
    /// Adds a table using an Action delegate.
    /// </summary>
    /// <param name="configure">Action to configure the table builder.</param>
    /// <returns>The builder instance.</returns>
    public DatabaseSchemaBuilder AddTable(Action<TableBuilder> configure)
    {
        var builder = new TableBuilder();
        configure(builder);
        var table = builder.Build();
        return this.WithTable(table);
    }

    /// <summary>
    /// Adds a table directly (overload for convenience).
    /// </summary>
    /// <param name="table">The table to add.</param>
    /// <returns>The builder instance.</returns>
    public DatabaseSchemaBuilder AddTable(Table table)
    {
        return this.WithTable(table);
    }

    /// <summary>
    /// Adds multiple tables to the schema.
    /// </summary>
    /// <param name="schemaTables">The tables to add.</param>
    /// <returns>The builder instance.</returns>
    public DatabaseSchemaBuilder WithTables(params Table[] schemaTables)
    {
        this.tables.AddRange(schemaTables);
        return this;
    }

    /// <summary>
    /// Adds a table using a builder pattern.
    /// </summary>
    /// <param name="tableBuilder">Function to configure the table builder.</param>
    /// <returns>The builder instance.</returns>
    public DatabaseSchemaBuilder WithTable(Func<TableBuilder, Table> tableBuilder)
    {
        var builder = new TableBuilder();
        var table = tableBuilder(builder);
        return this.WithTable(table);
    }

    /// <summary>
    /// Adds a relationship to the schema.
    /// </summary>
    /// <param name="relationship">The relationship to add.</param>
    /// <returns>The builder instance.</returns>
    public DatabaseSchemaBuilder WithRelationship(Relationship relationship)
    {
        this.relationships.Add(relationship);
        return this;
    }

    /// <summary>
    /// Adds a relationship between two tables.
    /// </summary>
    /// <param name="parentTable">Parent table name (referenced table).</param>
    /// <param name="parentColumn">Parent column name (PK).</param>
    /// <param name="childTable">Child table name (referencing table).</param>
    /// <param name="childColumn">Child column name (FK).</param>
    /// <returns>The builder instance.</returns>
    public DatabaseSchemaBuilder WithRelationship(
        string parentTable,
        string parentColumn,
        string childTable,
        string childColumn)
    {
        var relationship = new Relationship
        {
            Name = $"FK_{childTable}_{parentTable}",
            ParentTable = parentTable,
            ParentColumn = parentColumn,
            ChildTable = childTable,
            ChildColumn = childColumn,
            RelationshipType = RelationshipType.OneToMany,
            IsEnabled = true
        };

        return this.WithRelationship(relationship);
    }

    /// <summary>
    /// Adds a relationship between two tables (alias method).
    /// </summary>
    /// <param name="parentTable">Parent table name (referenced table).</param>
    /// <param name="parentColumn">Parent column name (PK).</param>
    /// <param name="childTable">Child table name (referencing table).</param>
    /// <param name="childColumn">Child column name (FK).</param>
    /// <returns>The builder instance.</returns>
    public DatabaseSchemaBuilder AddRelationship(
        string parentTable,
        string parentColumn,
        string childTable,
        string childColumn)
    {
        return this.WithRelationship(parentTable, parentColumn, childTable, childColumn);
    }

    /// <summary>
    /// Builds the DatabaseSchema instance.
    /// </summary>
    /// <returns>The configured DatabaseSchema.</returns>
    public DatabaseSchema Build()
    {
        return new DatabaseSchema
        {
            DatabaseName = this.databaseName,
            ServerName = this.serverName,
            Tables = this.tables,
            Relationships = this.relationships,
            AnalyzedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Creates a new builder instance.
    /// </summary>
    /// <returns>New DatabaseSchemaBuilder instance.</returns>
    public static DatabaseSchemaBuilder New() => new DatabaseSchemaBuilder();

    /// <summary>
    /// Creates a simple OrdersSchema with Customer and Order tables.
    /// </summary>
    /// <returns>The configured DatabaseSchema.</returns>
    public static DatabaseSchema OrdersSchema()
    {
        return new DatabaseSchemaBuilder()
            .WithDatabaseName("OrdersDB")
            .AddTable(tb => tb
                .WithName("Customer")
                .WithIdColumn()
                .WithNameColumn())
            .AddTable(tb => tb
                .WithName("Order")
                .WithIdColumn()
                .AddColumn(ColumnBuilder.New().WithName("CustomerID").AsInt().NotNullable().Build())
                .AddColumn(ColumnBuilder.New().WithName("OrderDate").AsDateTime().NotNullable().Build()))
            .AddRelationship("Customer", "ID", "Order", "CustomerID")
            .Build();
    }

    /// <summary>
    /// Creates a simple schema with Customer and Order tables.
    /// </summary>
    /// <returns>The configured DatabaseSchema.</returns>
    public static DatabaseSchema CreateSimpleCustomerOrderSchema()
    {
        var customerTable = TableBuilder.CustomerTable();
        
        var orderTable = new TableBuilder()
            .WithName("Order")
            .WithIdColumn()
            .AddColumn(ColumnBuilder.ForeignKeyColumn("CustomerID"))
            .AddColumn(new ColumnBuilder()
                .WithName("OrderDate")
                .AsDateTime()
                .NotNullable()
                .Build())
            .AddColumn(new ColumnBuilder()
                .WithName("TotalAmount")
                .AsDecimal(18, 2)
                .NotNullable()
                .Build())
            .Build();

        return new DatabaseSchemaBuilder()
            .WithDatabaseName("ECommerceDB")
            .WithTables(customerTable, orderTable)
            .WithRelationship("Customer", "ID", "Order", "CustomerID")
            .Build();
    }

    /// <summary>
    /// Creates an empty schema for testing.
    /// </summary>
    /// <param name="dbName">Database name.</param>
    /// <returns>The configured DatabaseSchema.</returns>
    public static DatabaseSchema CreateEmptySchema(string dbName = "EmptyDB")
    {
        return new DatabaseSchemaBuilder()
            .WithDatabaseName(dbName)
            .Build();
    }

    /// <summary>
    /// Creates a schema with a single simple table.
    /// </summary>
    /// <param name="tableName">Table name.</param>
    /// <returns>The configured DatabaseSchema.</returns>
    public static DatabaseSchema CreateSchemaWithSingleTable(string tableName = "TestTable")
    {
        var table = TableBuilder.SimpleTable(
            tableName,
            ColumnBuilder.IdColumn(),
            ColumnBuilder.NameColumn());

        return new DatabaseSchemaBuilder()
            .WithDatabaseName("SingleTableDB")
            .WithTable(table)
            .Build();
    }

    /// <summary>
    /// Creates a complex schema with multiple related tables.
    /// </summary>
    /// <returns>The configured DatabaseSchema.</returns>
    public static DatabaseSchema CreateComplexSchema()
    {
        // Customer table
        var customerTable = TableBuilder.CustomerTable();

        // Order table
        var orderTable = new TableBuilder()
            .WithName("Order")
            .WithIdColumn()
            .AddColumn(ColumnBuilder.ForeignKeyColumn("CustomerID"))
            .AddColumn(new ColumnBuilder()
                .WithName("OrderDate")
                .AsDateTime()
                .NotNullable()
                .Build())
            .AddColumn(new ColumnBuilder()
                .WithName("TotalAmount")
                .AsDecimal(18, 2)
                .NotNullable()
                .Build())
            .Build();

        // OrderItem table
        var orderItemTable = new TableBuilder()
            .WithName("OrderItem")
            .WithIdColumn()
            .AddColumn(ColumnBuilder.ForeignKeyColumn("OrderID"))
            .AddColumn(ColumnBuilder.ForeignKeyColumn("ProductID"))
            .AddColumn(new ColumnBuilder()
                .WithName("Quantity")
                .AsInt()
                .NotNullable()
                .Build())
            .AddColumn(new ColumnBuilder()
                .WithName("UnitPrice")
                .AsDecimal()
                .NotNullable()
                .Build())
            .Build();

        // Product table
        var productTable = new TableBuilder()
            .WithName("Product")
            .WithIdColumn()
            .WithNameColumn()
            .AddColumn(new ColumnBuilder()
                .WithName("Price")
                .AsDecimal()
                .NotNullable()
                .Build())
            .AddColumn(new ColumnBuilder()
                .WithName("StockQuantity")
                .AsInt()
                .NotNullable()
                .Build())
            .Build();

        return new DatabaseSchemaBuilder()
            .WithDatabaseName("ComplexECommerceDB")
            .WithTables(customerTable, orderTable, orderItemTable, productTable)
            .WithRelationship("Customer", "ID", "Order", "CustomerID")
            .WithRelationship("Order", "ID", "OrderItem", "OrderID")
            .WithRelationship("Product", "ID", "OrderItem", "ProductID")
            .Build();
    }

    /// <summary>
    /// Creates a simple schema (alias for OrdersSchema).
    /// </summary>
    /// <param name="tables">Tables to add.</param>
    /// <returns>The configured DatabaseSchema.</returns>
    public static DatabaseSchema SimpleSchema(params Table[] tables)
    {
        var builder = new DatabaseSchemaBuilder();
        foreach (var table in tables)
        {
            builder.WithTable(table);
        }
        return builder.Build();
    }
}
