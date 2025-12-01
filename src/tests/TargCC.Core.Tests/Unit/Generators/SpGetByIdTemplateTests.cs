// <copyright file="SpGetByIdTemplateTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using TargCC.Core.Generators.Sql.Templates;
    using TargCC.Core.Interfaces.Models;
    using TargCC.Core.Tests.TestHelpers;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="SpGetByIdTemplate"/>.
    /// </summary>
    public class SpGetByIdTemplateTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly SpGetByIdTemplate _template;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpGetByIdTemplateTests"/> class.
        /// </summary>
        public SpGetByIdTemplateTests()
        {
            _mockLogger = new Mock<ILogger>();
            _template = new SpGetByIdTemplate(_mockLogger.Object);
        }

        [Fact]
        public async Task GenerateAsync_SimpleSinglePK_GeneratesCorrectProcedure()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Customer")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("Name").AsNvarchar(100).Build())
                .WithColumn(c => c.WithName("Email").AsNvarchar(100).Build())
                .Build();

            // Act
            var result = await _template.GenerateAsync(table);

            // Assert
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Contain("CREATE OR ALTER PROCEDURE [dbo].[SP_GetCustomerByID]");
            result.Should().Contain("@ID int");
            result.Should().Contain("SELECT");
            result.Should().Contain("[ID]");
            result.Should().Contain("[Name]");
            result.Should().Contain("[Email]");
            result.Should().Contain("FROM [Customer]");
            result.Should().Contain("WHERE [ID] = @ID");
        }

        [Fact]
        public async Task GenerateAsync_CompositePrimaryKey_GeneratesCorrectProcedure()
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
            var result = await _template.GenerateAsync(table);

            // Assert
            result.Should().Contain("SP_GetOrderDetailByID");
            result.Should().Contain("@OrderID int");
            result.Should().Contain("@ProductID int");
            result.Should().Contain("WHERE [OrderID] = @OrderID AND [ProductID] = @ProductID");
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
            Func<Task> act = async () => await _template.GenerateAsync(table);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*has no primary key*");
        }

        [Fact]
        public async Task GenerateAsync_WithPrefixedColumns_IncludesAllColumns()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("User")
                .WithColumn(c => c.WithName("ID").AsInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("Username").AsNvarchar(50).Build())
                .WithColumn(c => c.WithName("eno_Password").AsVarchar(64).Build())
                .WithColumn(c => c.WithName("ent_CreditCard").AsVarchar(-1).Build())
                .WithColumn(c => c.WithName("lkp_Status").AsVarchar(10).Build())
                .Build();

            // Act
            var result = await _template.GenerateAsync(table);

            // Assert
            result.Should().Contain("[eno_Password]");
            result.Should().Contain("[ent_CreditCard]");
            result.Should().Contain("[lkp_Status]");
        }

        [Fact]
        public async Task GenerateAsync_VariousColumnTypes_GeneratesCorrectTypes()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Product")
                .WithColumn(c => c.WithName("ID").AsBigInt().AsPrimaryKey().Build())
                .WithColumn(c => c.WithName("Description").AsNvarchar(-1).Build()) // MAX
                .WithColumn(c => c.WithName("Price").AsDecimal(18, 2).Build())
                .WithColumn(c => c.WithName("Stock").AsInt().Build())
                .Build();

            // Act
            var result = await _template.GenerateAsync(table);

            // Assert
            result.Should().Contain("@ID bigint");
            result.Should().Contain("[Description]");
            result.Should().Contain("[Price]");
            result.Should().Contain("[Stock]");
        }

        [Fact]
        public async Task GenerateAsync_NullTable_ThrowsArgumentNullException()
        {
            // Arrange
            // Act
            Func<Task> act = async () => await _template.GenerateAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }
    }
}
