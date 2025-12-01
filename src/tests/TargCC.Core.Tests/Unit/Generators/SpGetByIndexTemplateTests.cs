// <copyright file="SpGetByIndexTemplateTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using TargCC.Core.Generators.Sql.Templates;
    using TargCC.Core.Interfaces.Models;
    using TargCC.Core.Tests.TestHelpers;
    using Xunit;
    using IndexModel = TargCC.Core.Interfaces.Models.Index;

    /// <summary>
    /// Unit tests for <see cref="SpGetByIndexTemplate"/>.
    /// </summary>
    public class SpGetByIndexTemplateTests
    {
        [Fact]
        public async Task GenerateAllIndexProcedures_UniqueIndex_GeneratesGetProcedure()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Customer")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("Email").AsNvarchar(100).Build())
                .WithColumn(c => c.WithName("Name").AsNvarchar(100).Build())
                .WithUniqueIndex("IX_Customer_Email", "Email")
                .Build();

            // Act
            var result = await SpGetByIndexTemplate.GenerateAllIndexProcedures(table);

            // Assert
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Contain("SP_GetCustomerByEmail");
            result.Should().Contain("CREATE OR ALTER PROCEDURE [dbo].[SP_GetCustomerByEmail]");
            result.Should().Contain("@Email nvarchar(100)");
            result.Should().Contain("-- Index Type: Unique");
            result.Should().Contain("SELECT");
            result.Should().Contain("[ID]");
            result.Should().Contain("[Email]");
            result.Should().Contain("[Name]");
            result.Should().Contain("WHERE [Email] = @Email");
            result.Should().NotContain("ORDER BY"); // Unique index, no ORDER BY needed
        }

        [Fact]
        public async Task GenerateAllIndexProcedures_NonUniqueIndex_GeneratesFillProcedure()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Order")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("CustomerID").AsInt().Build())
                .WithColumn(c => c.WithName("OrderDate").AsDateTime().Build())
                .WithNonUniqueIndex("IX_Order_CustomerID", "CustomerID")
                .Build();

            // Act
            var result = await SpGetByIndexTemplate.GenerateAllIndexProcedures(table);

            // Assert
            result.Should().Contain("SP_FillOrderByCustomer");
            result.Should().Contain("CREATE OR ALTER PROCEDURE [dbo].[SP_FillOrderByCustomer]");
            result.Should().Contain("@CustomerID int");
            result.Should().Contain("-- Index Type: Non-Unique");
            result.Should().Contain("WHERE [CustomerID] = @CustomerID");
            result.Should().Contain("ORDER BY [ID]"); // Non-unique, should have ORDER BY
        }

        [Fact]
        public async Task GenerateAllIndexProcedures_CompositeIndex_GeneratesCorrectProcedure()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("OrderDetail")
                .WithColumn(c => c.WithName("OrderID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("ProductID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("Quantity").AsInt().Build())
                .WithNonUniqueIndex("IX_OrderDetail_OrderID_ProductID", "OrderID", "ProductID")
                .Build();

            // Act
            var result = await SpGetByIndexTemplate.GenerateAllIndexProcedures(table);

            // Assert
            result.Should().Contain("SP_FillOrderDetailByOrderAndProduct");
            result.Should().Contain("@OrderID int,");
            result.Should().Contain("@ProductID int");
            result.Should().Contain("WHERE [OrderID] = @OrderID AND [ProductID] = @ProductID");
        }

        [Fact]
        public async Task GenerateAllIndexProcedures_NoIndexes_ReturnsEmptyString()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("SimpleTable")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("Name").AsNvarchar(100).Build())
                .Build();

            // Act
            var result = await SpGetByIndexTemplate.GenerateAllIndexProcedures(table);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GenerateAllIndexProcedures_NullTable_ReturnsEmptyString()
        {
            // Arrange
            // Act
            var result = await SpGetByIndexTemplate.GenerateAllIndexProcedures(null);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GenerateAllIndexProcedures_MultipleIndexes_GeneratesAllProcedures()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("User")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("Username").AsNvarchar(50).Build())
                .WithColumn(c => c.WithName("Email").AsNvarchar(100).Build())
                .WithColumn(c => c.WithName("Status").AsVarchar(20).Build())
                .WithUniqueIndex("IX_User_Username", "Username")
                .WithUniqueIndex("IX_User_Email", "Email")
                .WithNonUniqueIndex("IX_User_Status", "Status")
                .Build();

            // Act
            var result = await SpGetByIndexTemplate.GenerateAllIndexProcedures(table);

            // Assert
            result.Should().Contain("SP_GetUserByUsername");
            result.Should().Contain("SP_GetUserByEmail");
            result.Should().Contain("SP_FillUserByStatus");
            result.Should().Contain("GO"); // Multiple procedures separated by GO
        }

        [Fact]
        public async Task GenerateAllIndexProcedures_PrimaryKeyIndex_IsSkipped()
        {
            // Arrange
            var pkIndex = new IndexModel
            {
                Name = "PK_Customer",
                IsUnique = true,
                IsClustered = true,
                IsPrimaryKey = true,
                ColumnNames = new System.Collections.Generic.List<string> { "ID" }
            };

            var table = new TableBuilder()
                .WithName("Customer")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("Name").AsNvarchar(100).Build())
                .WithIndex(pkIndex)
                .WithUniqueIndex("IX_Customer_Name", "Name")
                .Build();

            // Act
            var result = await SpGetByIndexTemplate.GenerateAllIndexProcedures(table);

            // Assert
            result.Should().Contain("SP_GetCustomerByName");
            result.Should().NotContain("SP_GetCustomerByID"); // PK should be skipped
            result.Should().NotContain("PK_Customer");
        }

        [Fact]
        public async Task GenerateAllIndexProcedures_WithTargCCPrefixes_CleansNamesCorrectly()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Product")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("Name").AsNvarchar(100).Build())
                .WithColumn(c => c.WithName("lkp_Category").AsNvarchar(50).Build())
                .WithColumn(c => c.WithName("enm_Status").AsInt().Build())
                .WithNonUniqueIndex("IX_Product_Category", "lkp_Category")
                .WithNonUniqueIndex("IX_Product_Status", "enm_Status")
                .Build();

            // Act
            var result = await SpGetByIndexTemplate.GenerateAllIndexProcedures(table);

            // Assert
            result.Should().Contain("SP_FillProductByCategory"); // lkp_ removed
            result.Should().Contain("SP_FillProductByStatus"); // enm_ removed
            result.Should().NotContain("BylkpCategory");
            result.Should().NotContain("ByenmStatus");
        }

        [Fact]
        public async Task GenerateAllIndexProcedures_WithNOLOCKHint_IncludesInQuery()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Product")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("SKU").AsVarchar(50).Build())
                .WithUniqueIndex("IX_Product_SKU", "SKU")
                .Build();

            // Act
            var result = await SpGetByIndexTemplate.GenerateAllIndexProcedures(table);

            // Assert
            result.Should().Contain("FROM [Product] WITH (NOLOCK)");
        }
    }
}
