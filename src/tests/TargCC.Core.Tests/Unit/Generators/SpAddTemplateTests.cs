// <copyright file="SpAddTemplateTests.cs" company="PlaceholderCompany">
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
    /// Unit tests for <see cref="SpAddTemplate"/>.
    /// </summary>
    public class SpAddTemplateTests
    {
        [Fact]
        public async Task GenerateAsync_SimpleTable_GeneratesCorrectProcedure()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Customer")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().AsIdentity().Build())
                .WithColumn(c => c.WithName("Name").AsNvarchar(100).NotNullable().Build())
                .WithColumn(c => c.WithName("Email").AsNvarchar(100).Build())
                .WithColumn(c => c.WithName("Phone").AsVarchar(20).Build())
                .Build();

            // Act
            var result = await SpAddTemplate.GenerateAsync(table);

            // Assert
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Contain("CREATE OR ALTER PROCEDURE [dbo].[SP_AddCustomer]");
            result.Should().Contain("@Name nvarchar(100)");
            result.Should().Contain("@Email nvarchar(100) = NULL");
            result.Should().Contain("@Phone varchar(20) = NULL");
            result.Should().Contain("INSERT INTO [Customer]");
            result.Should().Contain("[Name]");
            result.Should().Contain("[Email]");
            result.Should().Contain("[Phone]");
            result.Should().Contain("VALUES");
            result.Should().Contain("@Name");
            result.Should().Contain("@Email");
            result.Should().Contain("@Phone");
            result.Should().Contain("SELECT SCOPE_IDENTITY() AS NewID");
            result.Should().NotContain("@ID"); // IDENTITY column excluded
        }

        [Fact]
        public async Task GenerateAsync_WithAuditColumns_AutoSetsAddedOnAndAddedBy()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Product")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().AsIdentity().Build())
                .WithColumn(c => c.WithName("Name").AsNvarchar(100).Build())
                .WithColumn(c => c.WithName("AddedOn").AsDateTime().NotNullable().Build())
                .WithColumn(c => c.WithName("AddedBy").AsNvarchar(100).NotNullable().Build())
                .Build();

            // Act
            var result = await SpAddTemplate.GenerateAsync(table);

            // Assert
            result.Should().Contain("@AddedBy nvarchar(100)");
            result.Should().Contain("[AddedOn]");
            result.Should().Contain("[AddedBy]");
            result.Should().Contain("GETDATE()"); // AddedOn auto-set
            result.Should().Contain("@AddedBy"); // AddedBy from parameter
            result.Should().NotContain("@AddedOn"); // AddedOn not a parameter
        }

        [Fact]
        public async Task GenerateAsync_WithChangedOnChangedBy_ExcludesFromInsert()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Product")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().AsIdentity().Build())
                .WithColumn(c => c.WithName("Name").AsNvarchar(100).Build())
                .WithColumn(c => c.WithName("AddedOn").AsDateTime().NotNullable().Build())
                .WithColumn(c => c.WithName("AddedBy").AsNvarchar(100).NotNullable().Build())
                .WithColumn(c => c.WithName("ChangedOn").AsDateTime().Build())
                .WithColumn(c => c.WithName("ChangedBy").AsNvarchar(100).Build())
                .Build();

            // Act
            var result = await SpAddTemplate.GenerateAsync(table);

            // Assert
            result.Should().NotContain("@ChangedOn");
            result.Should().NotContain("@ChangedBy");
            result.Should().NotContain("[ChangedOn]");
            result.Should().NotContain("[ChangedBy]");
        }

        [Fact]
        public async Task GenerateAsync_WithPrefixedColumns_ExcludesCalculatedBusinessLogicAggregate()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Order")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().AsIdentity().Build())
                .WithColumn(c => c.WithName("CustomerName").AsNvarchar(100).Build())
                .WithColumn(c => c.WithName("clc_TotalAmount").AsDecimal().AsCalculated().Build())
                .WithColumn(c => c.WithName("blg_InternalScore").AsInt().AsBusinessLogic().Build())
                .WithColumn(c => c.WithName("agg_ItemCount").AsInt().AsAggregate().Build())
                .WithColumn(c => c.WithName("scb_SecurityToken").AsVarchar(50).Build())
                .Build();

            // Act
            var result = await SpAddTemplate.GenerateAsync(table);

            // Assert
            result.Should().Contain("@CustomerName");
            result.Should().Contain("[CustomerName]");
            result.Should().NotContain("@clc_TotalAmount");
            result.Should().NotContain("@blg_InternalScore");
            result.Should().NotContain("@agg_ItemCount");
            result.Should().NotContain("@scb_SecurityToken");
            result.Should().NotContain("[clc_TotalAmount]");
            result.Should().NotContain("[blg_InternalScore]");
            result.Should().NotContain("[agg_ItemCount]");
            result.Should().NotContain("[scb_SecurityToken]");
        }

        [Fact]
        public async Task GenerateAsync_WithEncryptedColumns_IncludesTwoWayExcludesOneWay()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("User")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().AsIdentity().Build())
                .WithColumn(c => c.WithName("Username").AsNvarchar(50).Build())
                .WithColumn(c => c.WithName("eno_Password").AsVarchar(64).WithOneWayEncryption().Build())
                .WithColumn(c => c.WithName("ent_CreditCard").AsVarchar(-1).WithTwoWayEncryption().Build())
                .Build();

            // Act
            var result = await SpAddTemplate.GenerateAsync(table);

            // Assert
            result.Should().Contain("@ent_CreditCard varchar(MAX) = NULL");
            result.Should().Contain("[ent_CreditCard]");
            result.Should().Contain("@ent_CreditCard");
            result.Should().NotContain("@eno_Password"); // One-way encryption excluded
            result.Should().NotContain("[eno_Password]");
        }

        [Fact]
        public async Task GenerateAsync_NoInsertableColumns_GeneratesNoOpProcedure()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("ReadOnlyTable")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().AsIdentity().Build())
                .WithColumn(c => c.WithName("clc_Calculated").AsDecimal().AsCalculated().Build())
                .WithColumn(c => c.WithName("agg_Counter").AsInt().AsAggregate().Build())
                .Build();

            // Act
            var result = await SpAddTemplate.GenerateAsync(table);

            // Assert
            result.Should().Contain("CREATE OR ALTER PROCEDURE [dbo].[SP_AddReadOnlyTable]");
            result.Should().Contain("@DummyParam int = NULL");
            result.Should().Contain("-- No insertable columns in this table");
            result.Should().Contain("-- This procedure exists for API consistency");
            result.Should().Contain("SELECT 0 AS NewID");
            result.Should().NotContain("INSERT INTO [ReadOnlyTable]");
        }

        [Fact]
        public async Task GenerateAsync_NullTable_ThrowsArgumentNullException()
        {
            // Arrange
            // Act
            Func<Task> act = async () => await SpAddTemplate.GenerateAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GenerateAsync_WithDecimalColumn_GeneratesCorrectType()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Product")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().AsIdentity().Build())
                .WithColumn(c => c.WithName("Price").AsDecimal(18, 2).Build())
                .Build();

            // Act
            var result = await SpAddTemplate.GenerateAsync(table);

            // Assert
            result.Should().Contain("@Price decimal(18,2) = NULL");
            result.Should().Contain("[Price]");
            result.Should().Contain("@Price");
        }

        [Fact]
        public async Task GenerateAsync_WithMaxLengthString_GeneratesVarcharMax()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Article")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().AsIdentity().Build())
                .WithColumn(c => c.WithName("Content").AsNvarchar(-1).Build()) // -1 = MAX
                .Build();

            // Act
            var result = await SpAddTemplate.GenerateAsync(table);

            // Assert
            result.Should().Contain("@Content nvarchar(MAX) = NULL");
        }

        [Fact]
        public async Task GenerateAsync_OnlyAddedByWithoutOtherColumns_GeneratesValidProcedure()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("AuditLog")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().AsIdentity().Build())
                .WithColumn(c => c.WithName("AddedOn").AsDateTime().NotNullable().Build())
                .WithColumn(c => c.WithName("AddedBy").AsNvarchar(100).NotNullable().Build())
                .Build();

            // Act
            var result = await SpAddTemplate.GenerateAsync(table);

            // Assert
            result.Should().Contain("CREATE OR ALTER PROCEDURE [dbo].[SP_AddAuditLog]");
            result.Should().Contain("@AddedBy nvarchar(100)");
            result.Should().Contain("INSERT INTO [AuditLog]");
            result.Should().Contain("[AddedOn]");
            result.Should().Contain("[AddedBy]");
            result.Should().Contain("GETDATE()");
            result.Should().Contain("@AddedBy");
        }

        [Fact]
        public async Task GenerateAsync_WithBitColumn_GeneratesCorrectType()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Feature")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().AsIdentity().Build())
                .WithColumn(c => c.WithName("IsActive").AsBit().Build())
                .Build();

            // Act
            var result = await SpAddTemplate.GenerateAsync(table);

            // Assert
            result.Should().Contain("@IsActive bit = NULL");
            result.Should().Contain("[IsActive]");
            result.Should().Contain("@IsActive");
        }

        [Fact]
        public async Task GenerateAsync_MultipleInsertableColumns_GeneratesCorrectColumnList()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Customer")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().AsIdentity().Build())
                .WithColumn(c => c.WithName("FirstName").AsNvarchar(50).NotNullable().Build())
                .WithColumn(c => c.WithName("LastName").AsNvarchar(50).NotNullable().Build())
                .WithColumn(c => c.WithName("Email").AsNvarchar(100).Build())
                .WithColumn(c => c.WithName("Phone").AsVarchar(20).Build())
                .WithColumn(c => c.WithName("IsActive").AsBit().NotNullable().Build())
                .Build();

            // Act
            var result = await SpAddTemplate.GenerateAsync(table);

            // Assert
            result.Should().Contain("@FirstName nvarchar(50)");
            result.Should().Contain("@LastName nvarchar(50)");
            result.Should().Contain("@Email nvarchar(100) = NULL");
            result.Should().Contain("@Phone varchar(20) = NULL");
            result.Should().Contain("@IsActive bit");
            result.Should().Contain("[FirstName]");
            result.Should().Contain("[LastName]");
            result.Should().Contain("[Email]");
            result.Should().Contain("[Phone]");
            result.Should().Contain("[IsActive]");
        }
    }
}
