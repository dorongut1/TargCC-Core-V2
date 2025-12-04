// <copyright file="SqlGeneratorIntegrationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Integration
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using TargCC.Core.Generators.Sql.Templates;
    using TargCC.Core.Tests.TestHelpers;
    using Xunit;

    /// <summary>
    /// Integration tests for SQL generators working together end-to-end.
    /// </summary>
    public class SqlGeneratorIntegrationTests
    {

        [Fact]
        public async Task EndToEnd_CompleteTableGeneration_GeneratesAllProcedures()
        {
            // Arrange - Create a complete table with all column types and indexes
            var table = new TableBuilder()
                .WithName("Customer")
                .WithColumn(c => c.WithName("ID").AsInt().AsIdentity().Build())
                .WithColumn(c => c.WithName("Name").AsNvarchar(100).NotNullable().Build())
                .WithColumn(c => c.WithName("Email").AsNvarchar(100).NotNullable().Build())
                .WithColumn(c => c.WithName("Phone").AsVarchar(20).Build())
                .WithColumn(c => c.WithName("eno_Password").AsVarchar(64).WithOneWayEncryption().Build())
                .WithColumn(c => c.WithName("ent_CreditCard").AsVarchar(-1).WithTwoWayEncryption().Build())
                .WithColumn(c => c.WithName("lkp_Status").AsNvarchar(50).AsLookup().Build())
                .WithColumn(c => c.WithName("clc_Age").AsInt().AsCalculated().Build())
                .WithColumn(c => c.WithName("blg_CreditScore").AsInt().AsBusinessLogic().Build())
                .WithColumn(c => c.WithName("agg_OrderCount").AsInt().AsAggregate().Build())
                .WithColumn(c => c.WithName("AddedOn").AsDateTime().NotNullable().WithDefaultValue("GETDATE()").Build())
                .WithColumn(c => c.WithName("AddedBy").AsNvarchar(100).NotNullable().Build())
                .WithColumn(c => c.WithName("ChangedOn").AsDateTime().Build())
                .WithColumn(c => c.WithName("ChangedBy").AsNvarchar(100).Build())
                .WithUniqueIndex("IX_Customer_Email", "Email")
                .WithNonUniqueIndex("IX_Customer_Status", "lkp_Status")
                .Build();

            // Act - Generate all procedures
            var getByIdSql = await SpGetByIdTemplate.GenerateAsync(table);
            var updateSql = await SpUpdateTemplate.GenerateAsync(table);
            var indexSql = await SpGetByIndexTemplate.GenerateAllIndexProcedures(table);

            // Assert - Verify GetByID
            getByIdSql.Should().NotBeNullOrWhiteSpace();
            getByIdSql.Should().Contain("SP_GetCustomerByID");
            getByIdSql.Should().Contain("@ID int");
            getByIdSql.Should().Contain("[eno_Password]"); // Should include all columns
            getByIdSql.Should().Contain("[ent_CreditCard]");
            getByIdSql.Should().Contain("[clc_Age]");
            getByIdSql.Should().Contain("[blg_CreditScore]");
            getByIdSql.Should().Contain("[agg_OrderCount]");

            // Assert - Verify Update
            updateSql.Should().NotBeNullOrWhiteSpace();
            updateSql.Should().Contain("SP_UpdateCustomer");
            updateSql.Should().Contain("@Name nvarchar(100)");
            updateSql.Should().Contain("@Email nvarchar(100)");
            updateSql.Should().Contain("@ent_CreditCard varchar(MAX) = NULL"); // Should include encrypted
            updateSql.Should().NotContain("@eno_Password"); // Should exclude hashed (one-way encryption)
            updateSql.Should().NotContain("@clc_Age"); // Should exclude calculated
            updateSql.Should().NotContain("@blg_CreditScore"); // Should exclude business logic
            updateSql.Should().NotContain("@agg_OrderCount"); // Should exclude aggregate
            updateSql.Should().NotContain("@AddedOn"); // Should exclude AddedOn
            updateSql.Should().NotContain("@AddedBy"); // Should exclude AddedBy
            updateSql.Should().Contain("@ChangedBy"); // Should include ChangedBy parameter
            updateSql.Should().Contain("[ChangedOn] = GETDATE()"); // Should auto-update ChangedOn
            updateSql.Should().Contain("[ChangedBy] = @ChangedBy"); // Should update ChangedBy

            // Assert - Verify Index procedures
            indexSql.Should().NotBeNullOrWhiteSpace();
            indexSql.Should().Contain("SP_GetCustomerByEmail"); // Unique index
            indexSql.Should().Contain("SP_FillCustomerByStatus"); // Non-unique index
            indexSql.Should().Contain("@Email nvarchar(100)");
            indexSql.Should().Contain("@lkp_Status nvarchar(50)");
        }

        [Fact]
        public async Task EndToEnd_SimpleTable_GeneratesBasicProcedures()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Product")
                .WithColumn(c => c.WithName("ID").AsInt().AsIdentity().Build())
                .WithColumn(c => c.WithName("Name").AsNvarchar(100).NotNullable().Build())
                .WithColumn(c => c.WithName("Price").AsDecimal(18, 2).NotNullable().Build())
                .WithColumn(c => c.WithName("Stock").AsInt().NotNullable().Build())
                .Build();

            // Act
            var getByIdSql = await SpGetByIdTemplate.GenerateAsync(table);
            var updateSql = await SpUpdateTemplate.GenerateAsync(table);

            // Assert
            getByIdSql.Should().Contain("SP_GetProductByID");
            getByIdSql.Should().Contain("SELECT");
            getByIdSql.Should().Contain("[Name]");
            getByIdSql.Should().Contain("[Price]");
            getByIdSql.Should().Contain("[Stock]");

            updateSql.Should().Contain("SP_UpdateProduct");
            updateSql.Should().Contain("@Name nvarchar(100)");
            updateSql.Should().Contain("@Price decimal(18,2)");
            updateSql.Should().Contain("@Stock int");
            updateSql.Should().Contain("UPDATE [Product]");
            updateSql.Should().Contain("[Name] = @Name");
            updateSql.Should().Contain("[Price] = @Price");
            updateSql.Should().Contain("[Stock] = @Stock");
        }

        [Fact]
        public async Task EndToEnd_TableWithOnlyReadOnlyColumns_GeneratesNoOpUpdate()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Report")
                .WithColumn(c => c.WithName("ID").AsInt().AsIdentity().Build())
                .WithColumn(c => c.WithName("clc_TotalSales").AsDecimal(18, 2).AsCalculated().Build())
                .WithColumn(c => c.WithName("blg_Score").AsInt().AsBusinessLogic().Build())
                .WithColumn(c => c.WithName("agg_RecordCount").AsInt().AsAggregate().Build())
                .Build();

            // Act
            var getByIdSql = await SpGetByIdTemplate.GenerateAsync(table);
            var updateSql = await SpUpdateTemplate.GenerateAsync(table);

            // Assert - GetByID should work normally
            getByIdSql.Should().Contain("SP_GetReportByID");
            getByIdSql.Should().Contain("[clc_TotalSales]");
            getByIdSql.Should().Contain("[blg_Score]");
            getByIdSql.Should().Contain("[agg_RecordCount]");

            // Assert - Update should be no-op
            updateSql.Should().Contain("SP_UpdateReport");
            updateSql.Should().Contain("-- No updateable columns in this table");
            updateSql.Should().Contain("-- This procedure exists for API consistency");
            updateSql.Should().NotContain("UPDATE [Report]");
        }

        [Fact]
        public async Task EndToEnd_CompositePrimaryKey_GeneratesCorrectProcedures()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("OrderDetail")
                .WithColumn(c => c.WithName("OrderID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("ProductID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("Quantity").AsInt().NotNullable().Build())
                .WithColumn(c => c.WithName("UnitPrice").AsDecimal(18, 2).NotNullable().Build())
                .WithColumn(c => c.WithName("Discount").AsDecimal(5, 2).Build())
                .Build();

            // Act
            var getByIdSql = await SpGetByIdTemplate.GenerateAsync(table);
            var updateSql = await SpUpdateTemplate.GenerateAsync(table);

            // Assert - GetByID with composite key
            getByIdSql.Should().Contain("@OrderID int");
            getByIdSql.Should().Contain("@ProductID int");
            getByIdSql.Should().Contain("WHERE [OrderID] = @OrderID AND [ProductID] = @ProductID");

            // Assert - Update with composite key
            updateSql.Should().Contain("@OrderID int,");
            updateSql.Should().Contain("@ProductID int,");
            updateSql.Should().Contain("@Quantity int");
            updateSql.Should().Contain("@UnitPrice decimal(18,2)");
            updateSql.Should().Contain("@Discount decimal(5,2) = NULL");
            updateSql.Should().Contain("WHERE [OrderID] = @OrderID AND [ProductID] = @ProductID");
        }

        [Fact]
        public async Task EndToEnd_TableWithMultipleIndexes_GeneratesAllIndexProcedures()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Employee")
                .WithColumn(c => c.WithName("ID").AsInt().AsIdentity().Build())
                .WithColumn(c => c.WithName("EmployeeNumber").AsVarchar(20).NotNullable().Build())
                .WithColumn(c => c.WithName("Email").AsNvarchar(100).NotNullable().Build())
                .WithColumn(c => c.WithName("DepartmentID").AsInt().NotNullable().Build())
                .WithColumn(c => c.WithName("ManagerID").AsInt().Build())
                .WithUniqueIndex("IX_Employee_EmployeeNumber", "EmployeeNumber")
                .WithUniqueIndex("IX_Employee_Email", "Email")
                .WithNonUniqueIndex("IX_Employee_DepartmentID", "DepartmentID")
                .WithNonUniqueIndex("IX_Employee_ManagerID", "ManagerID")
                .Build();

            // Act
            var indexSql = await SpGetByIndexTemplate.GenerateAllIndexProcedures(table);

            // Assert
            indexSql.Should().Contain("SP_GetEmployeeByEmployeeNumber");
            indexSql.Should().Contain("SP_GetEmployeeByEmail");
            indexSql.Should().Contain("SP_FillEmployeeByDepartment");
            indexSql.Should().Contain("SP_FillEmployeeByManager");

            // Verify unique indexes don't have ORDER BY
            var lines = indexSql.Split('\n');
            var getByEmailSection = string.Join('\n',
                lines.SkipWhile(l => !l.Contains("SP_GetEmployeeByEmail"))
                     .TakeWhile(l => !l.Contains("GO")));
            getByEmailSection.Should().NotContain("ORDER BY");

            // Verify non-unique indexes have ORDER BY
            var fillByDeptSection = string.Join('\n',
                lines.SkipWhile(l => !l.Contains("SP_FillEmployeeByDepartment"))
                     .TakeWhile(l => !l.Contains("GO")));
            fillByDeptSection.Should().Contain("ORDER BY [ID]");
        }
    }
}
