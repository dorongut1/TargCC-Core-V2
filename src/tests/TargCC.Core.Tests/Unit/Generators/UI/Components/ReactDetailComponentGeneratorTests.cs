// <copyright file="ReactDetailComponentGeneratorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators.UI.Components
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Moq;
    using TargCC.Core.Generators.UI.Components;
    using TargCC.Core.Interfaces.Models;
    using Xunit;

    /// <summary>
    /// Tests for ReactDetailComponentGenerator.
    /// </summary>
    public class ReactDetailComponentGeneratorTests
    {
        private readonly ReactDetailComponentGenerator _generator;
        private readonly ComponentGeneratorConfig _config;

        public ReactDetailComponentGeneratorTests()
        {
            var logger = new Mock<ILogger<ReactDetailComponentGenerator>>().Object;
            _generator = new ReactDetailComponentGenerator(logger);
            _config = new ComponentGeneratorConfig();
        }

        [Fact]
        public async Task GenerateAsync_SimpleTable_GeneratesDetailComponent()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("export const CustomerDetail: React.FC", result);
            Assert.Contains("useCustomer", result);
            Assert.Contains("useDeleteCustomer", result);
        }

        [Fact]
        public async Task GenerateAsync_WithMaterialUI_GeneratesCard()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };
            _config.Framework = UIFramework.MaterialUI;

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("Card", result);
            Assert.Contains("CardContent", result);
            Assert.Contains("Typography", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesAllFields()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("Name", result);
            Assert.Contains("Email", result);
            Assert.Contains("entity.name", result);
            Assert.Contains("entity.email", result);
        }

        [Fact]
        public async Task GenerateAsync_EnoPrefix_HidesPassword()
        {
            // Arrange
            var table = new Table
            {
                Name = "User",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "eno_Password", DataType = "varchar(64)", IsNullable = false },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("'********'", result);
        }

        [Fact]
        public async Task GenerateAsync_LkpPrefix_ShowsLookupText()
        {
            // Arrange
            var table = new Table
            {
                Name = "Order",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "lkp_Status", DataType = "varchar(20)", IsNullable = false },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("statusText ||", result);
            Assert.Contains("statusCode", result);
        }

        [Fact]
        public async Task GenerateAsync_DateColumn_FormatsDate()
        {
            // Arrange
            var table = new Table
            {
                Name = "Order",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "OrderDate", DataType = "datetime", IsNullable = false },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("toLocaleDateString", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesActionButtons()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("Edit", result);
            Assert.Contains("Delete", result);
            Assert.Contains("Back", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesLoadingState()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("isLoading", result);
            Assert.Contains("CircularProgress", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesErrorState()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("error", result);
            Assert.Contains("Alert", result);
        }

        private static Table CreateSimpleTable()
        {
            return new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true, IsNullable = false },
                    new Column { Name = "Name", DataType = "nvarchar(100)", IsNullable = false },
                    new Column { Name = "Email", DataType = "nvarchar(255)", IsNullable = true },
                },
            };
        }
    }
}
