// <copyright file="ReactComponentOrchestratorGeneratorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators.UI.Components
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Moq;
    using TargCC.Core.Generators.UI.Components;
    using TargCC.Core.Interfaces.Models;
    using Xunit;

    /// <summary>
    /// Tests for ReactComponentOrchestratorGenerator.
    /// </summary>
    public class ReactComponentOrchestratorGeneratorTests
    {
        private readonly ReactComponentOrchestratorGenerator _orchestrator;
        private readonly ComponentGeneratorConfig _config;

        public ReactComponentOrchestratorGeneratorTests()
        {
            var logger = new Mock<ILogger<ReactComponentOrchestratorGenerator>>().Object;
            var listLogger = new Mock<ILogger<ReactListComponentGenerator>>().Object;
            var formLogger = new Mock<ILogger<ReactFormComponentGenerator>>().Object;
            var detailLogger = new Mock<ILogger<ReactDetailComponentGenerator>>().Object;

            var listGenerator = new ReactListComponentGenerator(listLogger);
            var formGenerator = new ReactFormComponentGenerator(formLogger);
            var detailGenerator = new ReactDetailComponentGenerator(detailLogger);

            _orchestrator = new ReactComponentOrchestratorGenerator(logger, listGenerator, formGenerator, detailGenerator);
            _config = new ComponentGeneratorConfig();
        }

        [Fact]
        public async Task GenerateAllComponentsAsync_SimpleTable_GeneratesAllFiles()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _orchestrator.GenerateAllComponentsAsync(table, schema, _config);

            // Assert
            Assert.Equal(5, result.Count);
            Assert.True(result.ContainsKey("CustomerList.tsx"));
            Assert.True(result.ContainsKey("CustomerForm.tsx"));
            Assert.True(result.ContainsKey("CustomerDetail.tsx"));
            Assert.True(result.ContainsKey("index.ts"));
            Assert.True(result.ContainsKey("CustomerRoutes.tsx"));
        }

        [Fact]
        public async Task GenerateAllComponentsAsync_GeneratesBarrelExport()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _orchestrator.GenerateAllComponentsAsync(table, schema, _config);

            // Assert
            var barrelExport = result["index.ts"];
            Assert.Contains("export { CustomerList }", barrelExport);
            Assert.Contains("export { CustomerForm }", barrelExport);
            Assert.Contains("export { CustomerDetail }", barrelExport);
        }

        [Fact]
        public async Task GenerateAllComponentsAsync_GeneratesRoutes()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _orchestrator.GenerateAllComponentsAsync(table, schema, _config);

            // Assert
            var routes = result["CustomerRoutes.tsx"];
            Assert.Contains("export const CustomerRoutes", routes);
            Assert.Contains("Route path=\"/\"", routes);
            Assert.Contains("Route path=\"/new\"", routes);
            Assert.Contains("Route path=\"/:id\"", routes);
            Assert.Contains("Route path=\"/:id/edit\"", routes);
        }

        [Fact]
        public async Task GenerateAllTablesAsync_MultipleTable_GeneratesAllComponents()
        {
            // Arrange
            var schema = new DatabaseSchema
            {
                Tables = new List<Table>
                {
                    CreateSimpleTable(),
                    CreateOrderTable(),
                },
            };

            // Act
            var result = await _orchestrator.GenerateAllTablesAsync(schema, _config);

            // Assert
            Assert.Equal(3, result.Count); // 2 tables + routing
            Assert.True(result.ContainsKey("Customer"));
            Assert.True(result.ContainsKey("Order"));
            Assert.True(result.ContainsKey("__routing__"));
        }

        [Fact]
        public async Task GenerateAllTablesAsync_GeneratesRootRouting()
        {
            // Arrange
            var schema = new DatabaseSchema
            {
                Tables = new List<Table>
                {
                    CreateSimpleTable(),
                    CreateOrderTable(),
                },
            };

            // Act
            var result = await _orchestrator.GenerateAllTablesAsync(schema, _config);

            // Assert
            var routing = result["__routing__"]["routes.config.ts"];
            Assert.Contains("AppRoutes", routing);
            Assert.Contains("/customers/*", routing);
            Assert.Contains("/orders/*", routing);
        }

        [Fact]
        public async Task GenerateAllComponentsAsync_ListComponent_ContainsExpectedContent()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _orchestrator.GenerateAllComponentsAsync(table, schema, _config);

            // Assert
            var listComponent = result["CustomerList.tsx"];
            Assert.Contains("CustomerList", listComponent);
            Assert.Contains("DataGrid", listComponent);
        }

        [Fact]
        public async Task GenerateAllComponentsAsync_FormComponent_ContainsExpectedContent()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _orchestrator.GenerateAllComponentsAsync(table, schema, _config);

            // Assert
            var formComponent = result["CustomerForm.tsx"];
            Assert.Contains("CustomerForm", formComponent);
            Assert.Contains("useForm", formComponent);
        }

        [Fact]
        public async Task GenerateAllComponentsAsync_DetailComponent_ContainsExpectedContent()
        {
            // Arrange
            var table = CreateSimpleTable();
            var schema = new DatabaseSchema { Tables = new List<Table> { table } };

            // Act
            var result = await _orchestrator.GenerateAllComponentsAsync(table, schema, _config);

            // Assert
            var detailComponent = result["CustomerDetail.tsx"];
            Assert.Contains("CustomerDetail", detailComponent);
            Assert.Contains("Card", detailComponent);
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

        private static Table CreateOrderTable()
        {
            return new Table
            {
                Name = "Order",
                Columns = new List<Column>
                {
                    new Column { Name = "ID", DataType = "int", IsPrimaryKey = true, IsNullable = false },
                    new Column { Name = "OrderNumber", DataType = "nvarchar(50)", IsNullable = false },
                    new Column { Name = "OrderDate", DataType = "datetime", IsNullable = false },
                },
            };
        }
    }
}
