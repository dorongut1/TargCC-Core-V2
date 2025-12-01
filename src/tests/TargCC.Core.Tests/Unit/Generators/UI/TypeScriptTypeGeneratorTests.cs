// <copyright file="TypeScriptTypeGeneratorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators.UI
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Moq;
    using TargCC.Core.Generators.UI;
    using TargCC.Core.Interfaces.Models;
    using Xunit;

    /// <summary>
    /// Tests for TypeScriptTypeGenerator.
    /// </summary>
    public class TypeScriptTypeGeneratorTests
    {
        private readonly TypeScriptTypeGenerator _generator;
        private readonly UIGeneratorConfig _config;

        public TypeScriptTypeGeneratorTests()
        {
            var logger = new Mock<ILogger<TypeScriptTypeGenerator>>().Object;
            _generator = new TypeScriptTypeGenerator(logger);
            _config = new UIGeneratorConfig();
        }

        [Fact]
        public async Task GenerateAsync_SimpleTable_GeneratesInterface()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("export interface Customer", result);
            Assert.Contains("id: number;", result);
            Assert.Contains("name: string;", result);
            Assert.Contains("email: string;", result);
        }

        [Fact]
        public async Task GenerateAsync_TableWithNullableColumn_AddsOptional()
        {
            // Arrange
            var table = new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true, IsNullable = false },
                    new Column { Name = "Name", DataType = "nvarchar(100)", IsNullable = false },
                    new Column { Name = "Email", DataType = "nvarchar(255)", IsNullable = true },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("name: string;", result);
            Assert.Contains("email?: string;", result);
        }

        [Fact]
        public async Task GenerateAsync_EnoPrefix_GeneratesHashedProperty()
        {
            // Arrange
            var table = new Table
            {
                Name = "Customer",
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
            Assert.Contains("readonly passwordHashed: string;", result);
            Assert.Contains("plainPasswordPassword: string;", result); // In CreateRequest
        }

        [Fact]
        public async Task GenerateAsync_LkpPrefix_GeneratesTwoProperties()
        {
            // Arrange
            var table = new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "lkp_Status", DataType = "varchar(10)", IsNullable = false },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("statusCode: string;", result);
            Assert.Contains("readonly statusText?: string;", result);
        }

        [Fact]
        public async Task GenerateAsync_EnmPrefix_GeneratesEnumType()
        {
            // Arrange
            var table = new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "enm_Type", DataType = "varchar(20)", IsNullable = false },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("export enum Type", result);
            Assert.Contains("type: Type;", result);
        }

        [Fact]
        public async Task GenerateAsync_LocPrefix_GeneratesTwoProperties()
        {
            // Arrange
            var table = new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "loc_Description", DataType = "nvarchar(500)", IsNullable = false },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("description: string;", result);
            Assert.Contains("readonly descriptionLocalized?: string;", result);
        }

        [Fact]
        public async Task GenerateAsync_ClcPrefix_GeneratesReadonlyProperty()
        {
            // Arrange
            var table = new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "clc_Total", DataType = "decimal(18,2)", IsNullable = false },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("readonly total: number;", result);
            Assert.DoesNotContain("total:", result.Split('\n').Last(l => l.Contains("CreateCustomerRequest")));
        }

        [Fact]
        public async Task GenerateAsync_AggPrefix_GeneratesReadonlyAggregate()
        {
            // Arrange
            var table = new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "agg_OrderCount", DataType = "int", IsNullable = false },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("readonly orderCountAggregate: number;", result);
        }

        [Fact]
        public async Task GenerateAsync_SplPrefix_GeneratesStringArray()
        {
            // Arrange
            var table = new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "spl_Tags", DataType = "nvarchar(max)", IsNullable = true },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("tags?: string[];", result);
        }

        [Fact]
        public async Task GenerateAsync_UplPrefix_GeneratesFilePathComment()
        {
            // Arrange
            var table = new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "upl_Photo", DataType = "varchar(69)", IsNullable = true },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("photo?: string; // File path", result);
        }

        [Fact]
        public async Task GenerateAsync_CreatesRequestInterface()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("export interface CreateCustomerRequest", result);
            Assert.DoesNotContain("id:", result.Split('\n').FirstOrDefault(l => l.Contains("CreateCustomerRequest") && l.Contains("id:")));
        }

        [Fact]
        public async Task GenerateAsync_CreatesUpdateRequestInterface()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("export interface UpdateCustomerRequest extends CreateCustomerRequest", result);
            Assert.Contains("id: number;", result.Split('\n').FirstOrDefault(l => l.Contains("UpdateCustomerRequest")));
        }

        [Fact]
        public async Task GenerateAsync_CreatesFiltersInterface()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("export interface CustomerFilters", result);
            Assert.Contains("page?: number;", result);
            Assert.Contains("pageSize?: number;", result);
            Assert.Contains("sortBy?: string;", result);
            Assert.Contains("sortDirection?: 'asc' | 'desc';", result);
        }

        [Theory]
        [InlineData("int", "number")]
        [InlineData("bigint", "number")]
        [InlineData("decimal(18,2)", "number")]
        [InlineData("float", "number")]
        [InlineData("money", "number")]
        [InlineData("bit", "boolean")]
        [InlineData("datetime", "Date")]
        [InlineData("date", "Date")]
        [InlineData("varchar(100)", "string")]
        [InlineData("nvarchar(max)", "string")]
        [InlineData("text", "string")]
        [InlineData("varbinary(max)", "Uint8Array")]
        public void GetTypeScriptType_Various_ReturnsExpected(string sqlType, string expected)
        {
            // This tests the private method indirectly through generation
            // Create a table with this type and verify output contains expected TS type
            var table = new Table
            {
                Name = "Test",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "TestField", DataType = sqlType, IsNullable = false },
                },
            };

            var schema = new DatabaseSchema { Tables = new List<Table> { table } };
            var result = _generator.GenerateAsync(table, schema, _config).Result;

            Assert.Contains($"testField: {expected};", result);
        }

        [Fact]
        public async Task GenerateAsync_IncludesFileHeader()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("<auto-generated>", result);
            Assert.Contains("TargCC UI Generator", result);
            Assert.Contains("TypeScriptTypes", result);
            Assert.Contains("Customer", result);
        }

        [Fact]
        public void GeneratorType_Returns_TypeScriptTypes()
        {
            // Act
            var type = _generator.GeneratorType;

            // Assert
            Assert.Equal(UIGeneratorType.TypeScriptTypes, type);
        }

        private Table CreateSimpleTable()
        {
            return new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true, IsNullable = false, IsIdentity = true },
                    new Column { Name = "Name", DataType = "nvarchar(100)", IsNullable = false },
                    new Column { Name = "Email", DataType = "nvarchar(255)", IsNullable = false },
                },
            };
        }
    }
}
