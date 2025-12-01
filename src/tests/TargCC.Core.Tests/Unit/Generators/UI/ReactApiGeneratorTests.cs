// <copyright file="ReactApiGeneratorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators.UI
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Moq;
    using TargCC.Core.Generators.UI;
    using TargCC.Core.Interfaces.Models;
    using Xunit;

    /// <summary>
    /// Tests for ReactApiGenerator.
    /// </summary>
    public class ReactApiGeneratorTests
    {
        private readonly ReactApiGenerator _generator;
        private readonly UIGeneratorConfig _config;

        public ReactApiGeneratorTests()
        {
            var logger = new Mock<ILogger<ReactApiGenerator>>().Object;
            _generator = new ReactApiGenerator(logger);
            _config = new UIGeneratorConfig();
        }

        [Fact]
        public async Task GenerateAsync_SimpleTable_GeneratesApiObject()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("export const customerApi = {", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesGetById()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("getById: async (id: number): Promise<Customer>", result);
            Assert.Contains("api.get<Customer>(`/api/customers/${id}`)", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesGetAll()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("getAll: async (filters?: CustomerFilters): Promise<Customer[]>", result);
            Assert.Contains("params: filters", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesCreate()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("create: async (data: CreateCustomerRequest): Promise<Customer>", result);
            Assert.Contains("api.post<Customer>('/api/customers', data)", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesUpdate()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("update: async (id: number, data: UpdateCustomerRequest): Promise<Customer>", result);
            Assert.Contains("api.put<Customer>(`/api/customers/${id}`, data)", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesDelete()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("delete: async (id: number): Promise<void>", result);
            Assert.Contains("api.delete(`/api/customers/${id}`)", result);
        }

        [Fact]
        public async Task GenerateAsync_WithUniqueIndex_GeneratesGetByMethod()
        {
            // Arrange
            var table = new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "Email", DataType = "nvarchar(255)", IsRequired = true },
                },
                Indexes = new List<Index>
                {
                    new Index { Name = "IX_Customer_Email", Columns = new List<string> { "Email" }, IsUnique = true },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("getByEmail: async (email: string): Promise<Customer>", result);
        }

        [Fact]
        public async Task GenerateAsync_WithNonUniqueIndex_GeneratesGetByArrayMethod()
        {
            // Arrange
            var table = new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "lkp_Status", DataType = "varchar(10)", IsRequired = true },
                },
                Indexes = new List<Index>
                {
                    new Index { Name = "IX_Customer_Status", Columns = new List<string> { "lkp_Status" }, IsUnique = false },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("getByStatus: async (status: string): Promise<Customer[]>", result);
        }

        [Fact]
        public async Task GenerateAsync_IncludesImports()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("import { api } from '../config';", result);
            Assert.Contains("import type {", result);
            Assert.Contains("Customer,", result);
            Assert.Contains("CreateCustomerRequest,", result);
            Assert.Contains("UpdateCustomerRequest,", result);
            Assert.Contains("CustomerFilters,", result);
            Assert.Contains("} from '../types/Customer.types';", result);
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
            Assert.Contains("ReactApi", result);
        }

        [Fact]
        public void GeneratorType_Returns_ReactApi()
        {
            // Act
            var type = _generator.GeneratorType;

            // Assert
            Assert.Equal(UIGeneratorType.ReactApi, type);
        }

        private Table CreateSimpleTable()
        {
            return new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "Name", DataType = "nvarchar(100)", IsRequired = true },
                    new Column { Name = "Email", DataType = "nvarchar(255)", IsRequired = true },
                },
                Indexes = new List<Index>(),
            };
        }
    }
}
