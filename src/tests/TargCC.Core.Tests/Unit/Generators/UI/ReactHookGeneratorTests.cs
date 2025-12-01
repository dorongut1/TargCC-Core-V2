// <copyright file="ReactHookGeneratorTests.cs" company="PlaceholderCompany">
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
    /// Tests for ReactHookGenerator.
    /// </summary>
    public class ReactHookGeneratorTests
    {
        private readonly ReactHookGenerator _generator;
        private readonly UIGeneratorConfig _config;

        public ReactHookGeneratorTests()
        {
            var logger = new Mock<ILogger<ReactHookGenerator>>().Object;
            _generator = new ReactHookGenerator(logger);
            _config = new UIGeneratorConfig();
        }

        [Fact]
        public async Task GenerateAsync_SimpleTable_GeneratesHooks()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("export const useCustomer", result);
            Assert.Contains("export const useCustomers", result);
            Assert.Contains("export const useCreateCustomer", result);
            Assert.Contains("export const useUpdateCustomer", result);
            Assert.Contains("export const useDeleteCustomer", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesUseEntityHook()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("export const useCustomer = (id: number | null)", result);
            Assert.Contains("return useQuery({", result);
            Assert.Contains("queryKey: ['customer', id]", result);
            Assert.Contains("queryFn: () => customerApi.getById(id!)", result);
            Assert.Contains("enabled: id !== null", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesUseEntitiesHook()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("export const useCustomers = (filters?: CustomerFilters)", result);
            Assert.Contains("queryKey: ['customers', filters]", result);
            Assert.Contains("queryFn: () => customerApi.getAll(filters)", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesUseCreateHook()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("export const useCreateCustomer = ()", result);
            Assert.Contains("const queryClient = useQueryClient()", result);
            Assert.Contains("return useMutation({", result);
            Assert.Contains("mutationFn: (data: CreateCustomerRequest) => customerApi.create(data)", result);
            Assert.Contains("onSuccess: () => {", result);
            Assert.Contains("queryClient.invalidateQueries({ queryKey: ['customers'] })", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesUseUpdateHook()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("export const useUpdateCustomer = ()", result);
            Assert.Contains("mutationFn: ({ id, data }: { id: number; data: UpdateCustomerRequest })", result);
            Assert.Contains("customerApi.update(id, data)", result);
            Assert.Contains("onSuccess: (_, variables) => {", result);
            Assert.Contains("queryClient.invalidateQueries({ queryKey: ['customer', variables.id] })", result);
            Assert.Contains("queryClient.invalidateQueries({ queryKey: ['customers'] })", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesUseDeleteHook()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("export const useDeleteCustomer = ()", result);
            Assert.Contains("mutationFn: (id: number) => customerApi.delete(id)", result);
            Assert.Contains("queryClient.invalidateQueries({ queryKey: ['customers'] })", result);
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
            Assert.Contains("import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';", result);
            Assert.Contains("import { customerApi } from '../api/customerApi';", result);
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
            Assert.Contains("ReactHooks", result);
        }

        [Fact]
        public async Task GenerateAsync_UsesCamelCaseForApi()
        {
            // Arrange
            var table = new Table
            {
                Name = "OrderItem",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "OrderID", DataType = "int", IsNullable = false },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("import { orderItemApi }", result);
            Assert.Contains("orderItemApi.getById", result);
            Assert.Contains("orderItemApi.getAll", result);
        }

        [Fact]
        public async Task GenerateAsync_UsesPascalCaseForTypes()
        {
            // Arrange
            var table = new Table
            {
                Name = "order_item",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "OrderID", DataType = "int", IsNullable = false },
                },
            };
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("export const useOrderItem", result);
            Assert.Contains("export const useOrderItems", result);
            Assert.Contains("export const useCreateOrderItem", result);
            Assert.Contains("CreateOrderItemRequest", result);
            Assert.Contains("UpdateOrderItemRequest", result);
            Assert.Contains("OrderItemFilters", result);
        }

        [Fact]
        public async Task GenerateAsync_IncludesJSDocComments()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("/**", result);
            Assert.Contains("* Hook to fetch single Customer by ID.", result);
            Assert.Contains("* Hook to fetch all Customers with optional filters.", result);
            Assert.Contains("* Hook to create Customer.", result);
            Assert.Contains("* Hook to update Customer.", result);
            Assert.Contains("* Hook to delete Customer.", result);
        }

        [Fact]
        public async Task GenerateAsync_UsesCorrectQueryKeys()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("queryKey: ['customer', id]", result);
            Assert.Contains("queryKey: ['customers', filters]", result);
        }

        [Fact]
        public async Task GenerateAsync_InvalidatesCorrectQueries_OnCreate()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            // Create should invalidate the list query
            var createSection = result.Substring(result.IndexOf("useCreateCustomer"));
            var createEndIndex = result.IndexOf("useUpdateCustomer");
            var createHookContent = result.Substring(result.IndexOf("useCreateCustomer"), createEndIndex - result.IndexOf("useCreateCustomer"));

            Assert.Contains("queryClient.invalidateQueries({ queryKey: ['customers'] })", createHookContent);
        }

        [Fact]
        public async Task GenerateAsync_InvalidatesCorrectQueries_OnUpdate()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            // Update should invalidate both the single item and list queries
            var updateSection = result.Substring(result.IndexOf("useUpdateCustomer"));
            var updateEndIndex = result.IndexOf("useDeleteCustomer");
            var updateHookContent = result.Substring(result.IndexOf("useUpdateCustomer"), updateEndIndex - result.IndexOf("useUpdateCustomer"));

            Assert.Contains("queryClient.invalidateQueries({ queryKey: ['customer', variables.id] })", updateHookContent);
            Assert.Contains("queryClient.invalidateQueries({ queryKey: ['customers'] })", updateHookContent);
        }

        [Fact]
        public async Task GenerateAsync_InvalidatesCorrectQueries_OnDelete()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            // Delete should invalidate the list query
            var deleteSection = result.Substring(result.IndexOf("useDeleteCustomer"));
            Assert.Contains("queryClient.invalidateQueries({ queryKey: ['customers'] })", deleteSection);
        }

        [Fact]
        public void GeneratorType_Returns_ReactHooks()
        {
            // Act
            var type = _generator.GeneratorType;

            // Assert
            Assert.Equal(UIGeneratorType.ReactHooks, type);
        }

        [Fact]
        public async Task GenerateAsync_NullTable_ThrowsArgumentNullException()
        {
            // Arrange
            var schema = new DatabaseSchema { Tables = new List<Table>() };

            // Act & Assert
            await Assert.ThrowsAsync<System.ArgumentNullException>(
                async () => await _generator.GenerateAsync(null!, schema, _config));
        }

        [Fact]
        public async Task GenerateAsync_NullSchema_ThrowsArgumentNullException()
        {
            // Arrange
            var table = CreateSimpleTable();

            // Act & Assert
            await Assert.ThrowsAsync<System.ArgumentNullException>(
                async () => await _generator.GenerateAsync(table, null!, _config));
        }

        [Fact]
        public async Task GenerateAsync_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act & Assert
            await Assert.ThrowsAsync<System.ArgumentNullException>(
                async () => await _generator.GenerateAsync(table, schema, null!));
        }

        private Table CreateSimpleTable()
        {
            return new Table
            {
                Name = "Customer",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true },
                    new Column { Name = "Name", DataType = "nvarchar(100)", IsNullable = false },
                    new Column { Name = "Email", DataType = "nvarchar(255)", IsNullable = false },
                },
                Indexes = new List<Index>(),
            };
        }
    }
}
