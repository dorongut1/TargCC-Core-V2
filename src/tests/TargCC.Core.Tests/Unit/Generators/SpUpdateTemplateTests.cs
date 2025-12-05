// <copyright file="SpUpdateTemplateTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using TargCC.Core.Generators.Sql.Templates;
    using TargCC.Core.Tests.TestHelpers;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="SpUpdateTemplate"/>.
    /// </summary>
    public class SpUpdateTemplateTests
    {

        [Fact]
        public async Task GenerateAsync_SimpleTable_GeneratesCorrectProcedure()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Customer")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("Name").AsNvarchar(100).NotNullable().Build())
                .WithColumn(c => c.WithName("Email").AsNvarchar(100).Build())
                .WithColumn(c => c.WithName("Phone").AsVarchar(20).Build())
                .Build();

            // Act
            var result = await SpUpdateTemplate.GenerateAsync(table);

            // Assert
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Contain("CREATE OR ALTER PROCEDURE [dbo].[SP_UpdateCustomer]");
            result.Should().Contain("@ID int");
            result.Should().Contain("@Name nvarchar(100)");
            result.Should().Contain("@Email nvarchar(100) = NULL");
            result.Should().Contain("@Phone varchar(20) = NULL");
            result.Should().Contain("UPDATE [Customer]");
            result.Should().Contain("SET");
            result.Should().Contain("[Name] = @Name");
            result.Should().Contain("[Email] = @Email");
            result.Should().Contain("[Phone] = @Phone");
            result.Should().Contain("WHERE [ID] = @ID");
        }

        [Fact]
        public async Task GenerateAsync_WithAuditColumns_AutoUpdatesChangedOnAndChangedBy()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Product")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("Name").AsNvarchar(100).Build())
                .WithColumn(c => c.WithName("AddedOn").AsDateTime().NotNullable().Build())
                .WithColumn(c => c.WithName("AddedBy").AsNvarchar(100).NotNullable().Build())
                .WithColumn(c => c.WithName("ChangedOn").AsDateTime().Build())
                .WithColumn(c => c.WithName("ChangedBy").AsNvarchar(100).Build())
                .Build();

            // Act
            var result = await SpUpdateTemplate.GenerateAsync(table);

            // Assert
            result.Should().Contain("@ChangedBy nvarchar(100)");
            result.Should().Contain("[ChangedOn] = GETDATE()");
            result.Should().Contain("[ChangedBy] = @ChangedBy");
            result.Should().NotContain("@AddedOn");
            result.Should().NotContain("@AddedBy");
            result.Should().NotContain("[AddedOn] = @AddedOn");
            result.Should().NotContain("[AddedBy] = @AddedBy");
        }

        [Fact]
        public async Task GenerateAsync_WithPrefixedColumns_ExcludesCalculatedBusinessLogicAggregate()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Order")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("CustomerName").AsNvarchar(100).Build())
                .WithColumn(c => c.WithName("clc_TotalAmount").AsDecimal().AsCalculated().Build())
                .WithColumn(c => c.WithName("blg_InternalScore").AsInt().AsBusinessLogic().Build())
                .WithColumn(c => c.WithName("agg_ItemCount").AsInt().AsAggregate().Build())
                .Build();

            // Act
            var result = await SpUpdateTemplate.GenerateAsync(table);

            // Assert
            result.Should().Contain("@CustomerName");
            result.Should().Contain("[CustomerName] = @CustomerName");
            result.Should().NotContain("@clc_TotalAmount");
            result.Should().NotContain("@blg_InternalScore");
            result.Should().NotContain("@agg_ItemCount");
            result.Should().NotContain("[clc_TotalAmount] =");
            result.Should().NotContain("[blg_InternalScore] =");
            result.Should().NotContain("[agg_ItemCount] =");
        }

        [Fact]
        public async Task GenerateAsync_WithEncryptedColumns_IncludesOnlyTwoWay()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("User")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("Username").AsNvarchar(50).Build())
                .WithColumn(c => c.WithName("eno_Password").AsVarchar(64).WithOneWayEncryption().Build())
                .WithColumn(c => c.WithName("ent_CreditCard").AsVarchar(-1).WithTwoWayEncryption().Build())
                .Build();

            // Act
            var result = await SpUpdateTemplate.GenerateAsync(table);

            // Assert
            result.Should().Contain("@ent_CreditCard varchar(MAX) = NULL");
            result.Should().Contain("[ent_CreditCard] = @ent_CreditCard");
            result.Should().NotContain("@eno_Password"); // One-way encryption excluded
            result.Should().NotContain("[eno_Password] =");
        }

        [Fact]
        public async Task GenerateAsync_NoUpdateableColumns_GeneratesNoOpProcedure()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("ReadOnlyTable")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("clc_Calculated").AsDecimal().AsCalculated().Build())
                .WithColumn(c => c.WithName("agg_Counter").AsInt().AsAggregate().Build())
                .Build();

            // Act
            var result = await SpUpdateTemplate.GenerateAsync(table);

            // Assert
            result.Should().Contain("CREATE OR ALTER PROCEDURE [dbo].[SP_UpdateReadOnlyTable]");
            result.Should().Contain("@ID int");
            result.Should().Contain("-- No updateable columns in this table");
            result.Should().Contain("-- This procedure exists for API consistency");
            result.Should().NotContain("UPDATE [ReadOnlyTable]");
        }

        [Fact]
        public async Task GenerateAsync_NoPrimaryKey_ThrowsException()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("InvalidTable")
                .WithColumn(c => c.WithName("Name").AsNvarchar(100).Build())
                .Build();

            // Act
            Func<Task> act = async () => await SpUpdateTemplate.GenerateAsync(table);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*has no primary key*");
        }

        [Fact]
        public async Task GenerateAsync_NullTable_ThrowsArgumentNullException()
        {
            // Arrange
            // Act
            Func<Task> act = async () => await SpUpdateTemplate.GenerateAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GenerateAsync_CompositePrimaryKey_GeneratesCorrectWhereClause()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("OrderDetail")
                .WithColumn(c => c.WithName("OrderID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("ProductID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("Quantity").AsInt().Build())
                .WithColumn(c => c.WithName("Price").AsDecimal(18, 2).Build())
                .Build();

            // Act
            var result = await SpUpdateTemplate.GenerateAsync(table);

            // Assert
            result.Should().Contain("@OrderID int");
            result.Should().Contain("@ProductID int");
            result.Should().Contain("@Quantity int = NULL");
            result.Should().Contain("@Price decimal(18,2) = NULL");
            result.Should().Contain("WHERE [OrderID] = @OrderID AND [ProductID] = @ProductID");
        }
    }
}
