// <copyright file="ReactFormComponentGeneratorTests.cs" company="PlaceholderCompany">
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
    /// Tests for ReactFormComponentGenerator - all 12 prefix cases.
    /// </summary>
    public class ReactFormComponentGeneratorTests
    {
        private readonly ReactFormComponentGenerator _generator;
        private readonly ComponentGeneratorConfig _config;

        public ReactFormComponentGeneratorTests()
        {
            var logger = new Mock<ILogger<ReactFormComponentGenerator>>().Object;
            _generator = new ReactFormComponentGenerator(logger);
            _config = new ComponentGeneratorConfig();
        }

        [Fact]
        public async Task GenerateAsync_SimpleTable_GeneratesFormComponent()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("export const CustomerForm: React.FC", result);
            Assert.Contains("useForm", result);
            Assert.Contains("useCreateCustomer", result);
            Assert.Contains("useUpdateCustomer", result);
        }

        [Fact]
        public async Task GenerateAsync_EnoPrefix_GeneratesPasswordField()
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
            Assert.Contains("type=\"password\"", result);
            Assert.Contains("passwordHashed", result);
        }

        [Fact]
        public async Task GenerateAsync_LkpPrefix_GeneratesSelectField()
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
            Assert.Contains("Select", result);
            Assert.Contains("statusCode", result);
            Assert.Contains("MenuItem", result);
        }

        [Fact]
        public async Task GenerateAsync_LocPrefix_GeneratesTextareaField()
        {
            // Arrange
            var table = new Table
            {
                Name = "Product",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "loc_Description", DataType = "nvarchar(max)", IsNullable = true },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("multiline", result);
            Assert.Contains("rows={4}", result);
            Assert.Contains("Localized text", result);
        }

        [Fact]
        public async Task GenerateAsync_EnmPrefix_GeneratesEnumSelect()
        {
            // Arrange
            var table = new Table
            {
                Name = "Order",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "enm_OrderType", DataType = "int", IsNullable = false },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("Select", result);
            Assert.Contains("orderType", result);
        }

        [Fact]
        public async Task GenerateAsync_SplPrefix_GeneratesArrayField()
        {
            // Arrange
            var table = new Table
            {
                Name = "Product",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "spl_Tags", DataType = "nvarchar(500)", IsNullable = true },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("comma-separated", result);
            Assert.Contains("tags", result);
        }

        [Fact]
        public async Task GenerateAsync_UplPrefix_GeneratesFileUpload()
        {
            // Arrange
            var table = new Table
            {
                Name = "Document",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "upl_Attachment", DataType = "nvarchar(500)", IsNullable = true },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("type=\"file\"", result);
            Assert.Contains("Upload", result);
        }

        [Fact]
        public async Task GenerateAsync_ClcPrefix_GeneratesReadOnlyField()
        {
            // Arrange
            var table = new Table
            {
                Name = "Order",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "clc_TotalAmount", DataType = "decimal(10,2)", IsNullable = true },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("disabled", result);
            Assert.Contains("Read-only", result);
        }

        [Fact]
        public async Task GenerateAsync_BlgPrefix_NotInCreateForm()
        {
            // Arrange
            var table = new Table
            {
                Name = "Order",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "blg_Status", DataType = "varchar(20)", IsNullable = true },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            // BLG fields should be disabled in forms
            Assert.Contains("disabled", result);
        }

        [Fact]
        public async Task GenerateAsync_AggPrefix_NotInCreateForm()
        {
            // Arrange
            var table = new Table
            {
                Name = "Order",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "agg_ItemCount", DataType = "int", IsNullable = true },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            // AGG fields should be disabled
            Assert.Contains("disabled", result);
        }

        [Fact]
        public async Task GenerateAsync_SptPrefix_GeneratesTextField()
        {
            // Arrange
            var table = new Table
            {
                Name = "Order",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "spt_Notes", DataType = "nvarchar(500)", IsNullable = true },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("TextField", result);
            Assert.Contains("notes", result);
        }

        [Fact]
        public async Task GenerateAsync_EntPrefix_GeneratesTextField()
        {
            // Arrange
            var table = new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "ent_SSN", DataType = "varchar(64)", IsNullable = true },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("TextField", result);
            Assert.Contains("ssn", result);
        }

        [Fact]
        public async Task GenerateAsync_RequiredField_GeneratesValidation()
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
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("required:", result);
            Assert.Contains("is required", result);
        }

        [Fact]
        public async Task GenerateAsync_MaxLength_GeneratesValidation()
        {
            // Arrange
            var table = new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "Name", DataType = "nvarchar(100)", IsNullable = false, MaxLength = 100 },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("maxLength:", result);
            Assert.Contains("100", result);
        }

        [Fact]
        public async Task GenerateAsync_BooleanColumn_GeneratesCheckbox()
        {
            // Arrange
            var table = new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "IsActive", DataType = "bit", IsNullable = false },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("Checkbox", result);
            Assert.Contains("FormControlLabel", result);
        }

        [Fact]
        public async Task GenerateAsync_NumberColumn_GeneratesNumberInput()
        {
            // Arrange
            var table = new Table
            {
                Name = "Product",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "Price", DataType = "decimal(10,2)", IsNullable = false },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("type=\"number\"", result);
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
