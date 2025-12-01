// <copyright file="ReactListComponentGeneratorTests.cs" company="PlaceholderCompany">
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
    /// Tests for ReactListComponentGenerator.
    /// </summary>
    public class ReactListComponentGeneratorTests
    {
        private readonly ReactListComponentGenerator _generator;
        private readonly ComponentGeneratorConfig _config;

        public ReactListComponentGeneratorTests()
        {
            var logger = new Mock<ILogger<ReactListComponentGenerator>>().Object;
            _generator = new ReactListComponentGenerator(logger);
            _config = new ComponentGeneratorConfig();
        }

        [Fact]
        public async Task GenerateAsync_SimpleTable_GeneratesListComponent()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("export const CustomerList: React.FC", result);
            Assert.Contains("useCustomers", result);
            Assert.Contains("useDeleteCustomer", result);
            Assert.Contains("DataGrid", result);
        }

        [Fact]
        public async Task GenerateAsync_WithMaterialUI_GeneratesMuiComponents()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };
            _config.Framework = UIFramework.MaterialUI;

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("import { DataGrid", result);
            Assert.Contains("from '@mui/x-data-grid'", result);
            Assert.Contains("CircularProgress", result);
            Assert.Contains("Alert", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesColumnDefinitions()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("const columns: GridColDef<Customer>[]", result);
            Assert.Contains("field: 'id'", result);
            Assert.Contains("field: 'name'", result);
            Assert.Contains("field: 'email'", result);
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
            Assert.Contains("Create Customer", result);
        }

        [Fact]
        public async Task GenerateAsync_LkpPrefix_GeneratesLookupValueGetter()
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
            Assert.Contains("statusText || ", result);
            Assert.Contains("statusCode", result);
        }

        [Fact]
        public async Task GenerateAsync_DateColumn_GeneratesDateFormatter()
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
            Assert.Contains("valueFormatter", result);
            Assert.Contains("toLocaleDateString", result);
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
