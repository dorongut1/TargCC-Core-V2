// <copyright file="TableBuilder.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.TestHelpers;

using TargCC.Core.Interfaces.Models;
using Index = TargCC.Core.Interfaces.Models.Index;

/// <summary>
/// Builder pattern for creating test Table instances.
/// Provides fluent API for easy test data creation.
/// </summary>
public class TableBuilder
{
    private string schema = "dbo";
    private string name = "DefaultTable";
    private List<Column> columns = new();
    private List<Index> indexes = new();
    private List<string> primaryKeyColumns = new();

    /// <summary>
    /// Sets the table schema.
    /// </summary>
    /// <param name="schemaName">The schema name.</param>
    /// <returns>The builder instance.</returns>
    public TableBuilder WithSchema(string schemaName)
    {
        this.schema = schemaName;
        return this;
    }

    /// <summary>
    /// Sets the table name.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <returns>The builder instance.</returns>
    public TableBuilder WithName(string tableName)
    {
        this.name = tableName;
        return this;
    }

    /// <summary>
    /// Adds a column to the table.
    /// </summary>
    /// <param name="column">The column to add.</param>
    /// <returns>The builder instance.</returns>
    public TableBuilder WithColumn(Column column)
    {
        this.columns.Add(column);

        // Auto-add to primary key if marked
        if (column.IsPrimaryKey && !this.primaryKeyColumns.Contains(column.Name))
        {
            this.primaryKeyColumns.Add(column.Name);
        }

        return this;
    }

    /// <summary>
    /// Adds a column to the table (alias for WithColumn).
    /// </summary>
    /// <param name="column">The column to add.</param>
    /// <returns>The builder instance.</returns>
    public TableBuilder AddColumn(Column column)
    {
        return this.WithColumn(column);
    }

    /// <summary>
    /// Adds an ID column (Primary Key, Identity, INT).
    /// </summary>
    /// <param name="name">Column name (default: "ID").</param>
    /// <returns>The builder instance.</returns>
    public TableBuilder WithIdColumn(string name = "ID")
    {
        return this.WithColumn(ColumnBuilder.CreateIdColumn(name));
    }

    /// <summary>
    /// Adds a Name column (NVARCHAR(100), NOT NULL).
    /// </summary>
    /// <param name="name">Column name (default: "Name").</param>
    /// <returns>The builder instance.</returns>
    public TableBuilder WithNameColumn(string name = "Name")
    {
        return this.WithColumn(ColumnBuilder.CreateNameColumn(name));
    }

    /// <summary>
    /// Adds multiple columns to the table.
    /// </summary>
    /// <param name="tableColumns">The columns to add.</param>
    /// <returns>The builder instance.</returns>
    public TableBuilder WithColumns(params Column[] tableColumns)
    {
        foreach (var column in tableColumns)
        {
            this.WithColumn(column);
        }

        return this;
    }

    /// <summary>
    /// Adds a column using a builder pattern.
    /// </summary>
    /// <param name="columnBuilder">Action to configure the column builder.</param>
    /// <returns>The builder instance.</returns>
    public TableBuilder WithColumn(Func<ColumnBuilder, Column> columnBuilder)
    {
        var builder = new ColumnBuilder();
        var column = columnBuilder(builder);
        return this.WithColumn(column);
    }

    /// <summary>
    /// Adds an index to the table.
    /// </summary>
    /// <param name="index">The index to add.</param>
    /// <returns>The builder instance.</returns>
    public TableBuilder WithIndex(Index index)
    {
        this.indexes.Add(index);
        return this;
    }

    /// <summary>
    /// Adds an index to the table (alias for WithIndex).
    /// </summary>
    /// <param name="index">The index to add.</param>
    /// <returns>The builder instance.</returns>
    public TableBuilder AddIndex(Index index)
    {
        return this.WithIndex(index);
    }

    /// <summary>
    /// Adds a unique index.
    /// </summary>
    /// <param name="indexName">Index name.</param>
    /// <param name="columnNames">Column names in the index.</param>
    /// <returns>The builder instance.</returns>
    public TableBuilder WithUniqueIndex(string indexName, params string[] columnNames)
    {
        var index = new Index
        {
            Name = indexName,
            IsUnique = true,
            IsClustered = false,
            ColumnNames = columnNames.ToList(),
        };

        return this.WithIndex(index);
    }

    /// <summary>
    /// Adds a non-unique index.
    /// </summary>
    /// <param name="indexName">Index name.</param>
    /// <param name="columnNames">Column names in the index.</param>
    /// <returns>The builder instance.</returns>
    public TableBuilder WithNonUniqueIndex(string indexName, params string[] columnNames)
    {
        var index = new Index
        {
            Name = indexName,
            IsUnique = false,
            IsClustered = false,
            ColumnNames = columnNames.ToList(),
        };

        return this.WithIndex(index);
    }

    /// <summary>
    /// Adds a primary key constraint.
    /// </summary>
    /// <param name="columnNames">Column names in the primary key.</param>
    /// <returns>The builder instance.</returns>
    public TableBuilder WithPrimaryKey(params string[] columnNames)
    {
        this.primaryKeyColumns = columnNames.ToList();

        // Mark columns as primary key
        foreach (var colName in columnNames)
        {
            var column = this.columns.FirstOrDefault(c => c.Name == colName);
            if (column != null)
            {
                column.IsPrimaryKey = true;
            }
        }

        // Create clustered primary key index
        var pkIndex = new Index
        {
            Name = $"PK_{this.name}",
            IsUnique = true,
            IsClustered = true,
            IsPrimaryKey = true,
            ColumnNames = columnNames.ToList(),
        };

        this.indexes.Add(pkIndex);
        return this;
    }

    /// <summary>
    /// Builds the Table instance.
    /// </summary>
    /// <returns>The configured Table.</returns>
    public Table Build()
    {
        var table = new Table
        {
            SchemaName = this.schema,
            Name = this.name,
            Columns = this.columns,
            Indexes = this.indexes,
            PrimaryKeyColumns = this.primaryKeyColumns,
            PrimaryKey = this.primaryKeyColumns.FirstOrDefault(), // Set single PK property
        };

        return table;
    }

    /// <summary>
    /// Creates a simple table with ID and Name columns.
    /// </summary>
    /// <param name="tableName">Table name.</param>
    /// <returns>The configured Table.</returns>
    public static Table CreateSimpleTable(string tableName = "SimpleTable")
    {
        return new TableBuilder()
            .WithName(tableName)
            .WithColumn(ColumnBuilder.CreateIdColumn())
            .WithColumn(ColumnBuilder.CreateNameColumn())
            .WithPrimaryKey("ID")
            .Build();
    }

    /// <summary>
    /// Creates a simple table with given columns (alias for CreateSimpleTable).
    /// </summary>
    /// <param name="tableName">Table name.</param>
    /// <param name="columnList">Columns to add.</param>
    /// <returns>The configured Table.</returns>
    public static Table SimpleTable(string tableName, params Column[] columnList)
    {
        var builder = new TableBuilder().WithName(tableName);
        foreach (var col in columnList)
        {
            builder.WithColumn(col);
        }
        return builder.Build();
    }

    /// <summary>
    /// Creates a customer table with typical fields.
    /// </summary>
    /// <returns>The configured Table.</returns>
    public static Table CreateCustomerTable()
    {
        return new TableBuilder()
            .WithName("Customer")
            .WithColumns(
                ColumnBuilder.CreateIdColumn(),
                ColumnBuilder.CreateNameColumn("FirstName"),
                ColumnBuilder.CreateNameColumn("LastName"),
                new ColumnBuilder()
                    .WithName("Email")
                    .AsNvarchar(100)
                    .NotNullable()
                    .Build(),
                new ColumnBuilder()
                    .WithName("Phone")
                    .AsVarchar(20)
                    .Nullable()
                    .Build(),
                new ColumnBuilder()
                    .WithName("CreatedDate")
                    .AsDateTime()
                    .NotNullable()
                    .WithDefaultValue("GETDATE()")
                    .Build())
            .WithPrimaryKey("ID")
            .WithUniqueIndex("IX_Customer_Email", "Email")
            .WithNonUniqueIndex("IX_Customer_LastName", "LastName")
            .Build();
    }

    /// <summary>
    /// Creates a customer table (alias for CreateCustomerTable).
    /// </summary>
    /// <returns>The configured Table.</returns>
    public static Table CustomerTable()
    {
        return CreateCustomerTable();
    }

    /// <summary>
    /// Creates an order table with foreign keys.
    /// </summary>
    /// <returns>The configured Table.</returns>
    public static Table CreateOrderTable()
    {
        return new TableBuilder()
            .WithName("Order")
            .WithColumns(
                ColumnBuilder.CreateIdColumn(),
                ColumnBuilder.CreateForeignKeyColumn("CustomerID"),
                new ColumnBuilder()
                    .WithName("OrderDate")
                    .AsDateTime()
                    .NotNullable()
                    .Build(),
                new ColumnBuilder()
                    .WithName("TotalAmount")
                    .AsDecimal()
                    .NotNullable()
                    .Build(),
                new ColumnBuilder()
                    .WithName("Status")
                    .AsVarchar(20)
                    .NotNullable()
                    .Build())
            .WithPrimaryKey("ID")
            .WithNonUniqueIndex("IX_Order_CustomerID", "CustomerID")
            .WithNonUniqueIndex("IX_Order_OrderDate", "OrderDate")
            .Build();
    }

    /// <summary>
    /// Creates a table with various column prefixes for testing.
    /// </summary>
    /// <returns>The configured Table.</returns>
    public static Table CreateTableWithPrefixes()
    {
        return new TableBuilder()
            .WithName("PrefixTest")
            .WithColumns(
                ColumnBuilder.CreateIdColumn(),
                new ColumnBuilder()
                    .WithName("eno_Password")
                    .AsVarchar(64)
                    .WithOneWayEncryption()
                    .Build(),
                new ColumnBuilder()
                    .WithName("ent_SecretKey")
                    .AsNvarchar(200)
                    .WithTwoWayEncryption()
                    .Build(),
                new ColumnBuilder()
                    .WithName("loc_Description")
                    .AsNvarchar(500)
                    .AsLocalizable()
                    .Build(),
                new ColumnBuilder()
                    .WithName("clc_TotalAmount")
                    .AsDecimal()
                    .AsCalculated()
                    .Build())
            .WithPrimaryKey("ID")
            .Build();
    }
}
