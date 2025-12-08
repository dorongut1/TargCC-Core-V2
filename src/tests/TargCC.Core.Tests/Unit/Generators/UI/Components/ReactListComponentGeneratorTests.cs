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

        [Fact]
        public async Task GenerateAsync_WithFilters_GeneratesLocalAndAppliedFilterState()
        {
            // Arrange
            var table = new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "Name", DataType = "nvarchar(100)", IsNullable = false },
                },
                Indexes = new List<TargCC.Core.Interfaces.Models.Index>
                {
                    new TargCC.Core.Interfaces.Models.Index
                    {
                        Name = "IX_Customer_Name",
                        ColumnNames = new List<string> { "Name" },
                        IsUnique = false
                    }
                }
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            // Should have both localFilters and filters state
            Assert.Contains("const [filters, setFilters] = React.useState<CustomerFilters>(", result);
            Assert.Contains("const [localFilters, setLocalFilters] = React.useState<CustomerFilters>(", result);

            // Should have Apply and Clear handlers
            Assert.Contains("const handleApplyFilters = () =>", result);
            Assert.Contains("const handleClearFilters = () =>", result);
            Assert.Contains("setFilters(localFilters)", result);
            Assert.Contains("setLocalFilters({})", result);
        }

        [Fact]
        public async Task GenerateAsync_WithFilters_GeneratesApplyAndClearButtons()
        {
            // Arrange
            var table = new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "Email", DataType = "nvarchar(100)", IsNullable = false },
                },
                Indexes = new List<TargCC.Core.Interfaces.Models.Index>
                {
                    new TargCC.Core.Interfaces.Models.Index
                    {
                        Name = "IX_Customer_Email",
                        ColumnNames = new List<string> { "Email" },
                        IsUnique = true
                    }
                }
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            // Should have Apply Filters button
            Assert.Contains("Apply Filters", result);
            Assert.Contains("variant=\"contained\"", result);
            Assert.Contains("onClick={handleApplyFilters}", result);

            // Should have Clear button
            Assert.Contains("Clear", result);
            Assert.Contains("variant=\"outlined\"", result);
            Assert.Contains("onClick={handleClearFilters}", result);
        }

        [Fact]
        public async Task GenerateAsync_WithFilters_GeneratesEnterKeyHandling()
        {
            // Arrange
            var table = new Table
            {
                Name = "Product",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "Name", DataType = "nvarchar(100)", IsNullable = false },
                },
                Indexes = new List<TargCC.Core.Interfaces.Models.Index>
                {
                    new TargCC.Core.Interfaces.Models.Index
                    {
                        Name = "IX_Product_Name",
                        ColumnNames = new List<string> { "Name" },
                        IsUnique = false
                    }
                }
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            // Should handle Enter key in filter inputs
            Assert.Contains("onKeyDown={(e) => { if (e.key === 'Enter') handleApplyFilters(); }}", result);
        }

        [Fact]
        public async Task GenerateAsync_WithFilters_UsesLocalFiltersInInputs()
        {
            // Arrange
            var table = new Table
            {
                Name = "Order",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "CustomerName", DataType = "nvarchar(100)", IsNullable = false },
                },
                Indexes = new List<TargCC.Core.Interfaces.Models.Index>
                {
                    new TargCC.Core.Interfaces.Models.Index
                    {
                        Name = "IX_Order_CustomerName",
                        ColumnNames = new List<string> { "CustomerName" },
                        IsUnique = false
                    }
                }
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            // Filter inputs should use localFilters, not filters
            Assert.Contains("value={localFilters.customerName ?? ''}", result);
            Assert.Contains("setLocalFilters(prev => ({ ...prev,", result);
        }

        [Fact]
        public async Task GenerateAsync_DataGrid_UsesV7ValueGetterSignature()
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
            // Should use v7 signature: (value, row) instead of v6 (params)
            Assert.Contains("valueGetter: (value, row) => row.", result);
            Assert.DoesNotContain("valueGetter: (params) => params.row.", result);
        }

        [Fact]
        public async Task GenerateAsync_DataGrid_GeneratesFlexboxLayoutForHeight()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            // Should have outer Box with flexbox
            Assert.Contains("display: 'flex'", result);
            Assert.Contains("flexDirection: 'column'", result);
            Assert.Contains("height: 'calc(100vh - 200px)'", result);

            // Should have inner Box with flex: 1
            Assert.Contains("flex: 1", result);
            Assert.Contains("minHeight: 0", result);
        }

        [Fact]
        public async Task GenerateAsync_WithMultipleFilterColumns_GeneratesAllFilterInputs()
        {
            // Arrange
            var table = new Table
            {
                Name = "Product",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "Name", DataType = "nvarchar(100)", IsNullable = false },
                    new Column { Name = "Category", DataType = "nvarchar(50)", IsNullable = true },
                    new Column { Name = "Price", DataType = "decimal", IsNullable = false },
                },
                Indexes = new List<TargCC.Core.Interfaces.Models.Index>
                {
                    new TargCC.Core.Interfaces.Models.Index
                    {
                        Name = "IX_Product_Name",
                        ColumnNames = new List<string> { "Name" },
                        IsUnique = false
                    },
                    new TargCC.Core.Interfaces.Models.Index
                    {
                        Name = "IX_Product_Category",
                        ColumnNames = new List<string> { "Category" },
                        IsUnique = false
                    }
                }
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            // Should generate inputs for both Name and Category
            Assert.Contains("value={localFilters.name ?? ''}", result);
            Assert.Contains("value={localFilters.category ?? ''}", result);
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
