// <copyright file="ApiOrchestratorGeneratorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators.API
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Moq;
    using TargCC.Core.Generators.API;
    using TargCC.Core.Interfaces.Models;
    using TargCC.Core.Tests.TestHelpers;
    using Xunit;

    /// <summary>
    /// Tests for ApiOrchestratorGenerator.
    /// </summary>
    public class ApiOrchestratorGeneratorTests
    {
        private readonly Mock<DtoGenerator> _dtoGeneratorMock;
        private readonly Mock<ApiControllerGenerator> _controllerGeneratorMock;
        private readonly Mock<MappingProfileGenerator> _mappingGeneratorMock;
        private readonly Mock<ILogger<ApiOrchestratorGenerator>> _loggerMock;
        private readonly ApiOrchestratorGenerator _orchestrator;
        private readonly ApiGeneratorConfig _config;

        public ApiOrchestratorGeneratorTests()
        {
            _dtoGeneratorMock = new Mock<DtoGenerator>(new Mock<ILogger<DtoGenerator>>().Object);
            _controllerGeneratorMock = new Mock<ApiControllerGenerator>(new Mock<ILogger<ApiControllerGenerator>>().Object);
            _mappingGeneratorMock = new Mock<MappingProfileGenerator>(new Mock<ILogger<MappingProfileGenerator>>().Object);
            _loggerMock = new Mock<ILogger<ApiOrchestratorGenerator>>();

            _orchestrator = new ApiOrchestratorGenerator(
                _dtoGeneratorMock.Object,
                _controllerGeneratorMock.Object,
                _mappingGeneratorMock.Object,
                _loggerMock.Object);

            _config = new ApiGeneratorConfig
            {
                Namespace = "TestApp",
                ControllersOutputDirectory = "Controllers",
                DtosOutputDirectory = "DTOs",
                MappingOutputDirectory = "Mapping",
                ExtensionsOutputDirectory = "Extensions",
            };
        }

        [Fact]
        public void Constructor_WithNullDtoGenerator_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ApiOrchestratorGenerator(
                null!,
                _controllerGeneratorMock.Object,
                _mappingGeneratorMock.Object,
                _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithNullControllerGenerator_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ApiOrchestratorGenerator(
                _dtoGeneratorMock.Object,
                null!,
                _mappingGeneratorMock.Object,
                _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithNullMappingGenerator_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ApiOrchestratorGenerator(
                _dtoGeneratorMock.Object,
                _controllerGeneratorMock.Object,
                null!,
                _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ApiOrchestratorGenerator(
                _dtoGeneratorMock.Object,
                _controllerGeneratorMock.Object,
                _mappingGeneratorMock.Object,
                null!));
        }

        [Fact]
        public async Task GenerateAllAsync_WithSingleTable_GeneratesAllFiles()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Customer")
                .WithIdColumn()
                .WithNameColumn()
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            _dtoGeneratorMock
                .Setup(x => x.GenerateAsync(It.IsAny<Table>(), It.IsAny<DatabaseSchema>(), It.IsAny<ApiGeneratorConfig>()))
                .ReturnsAsync("DTO code");

            _controllerGeneratorMock
                .Setup(x => x.GenerateAsync(It.IsAny<Table>(), It.IsAny<DatabaseSchema>(), It.IsAny<ApiGeneratorConfig>()))
                .ReturnsAsync("Controller code");

            _mappingGeneratorMock
                .Setup(x => x.GenerateAsync(It.IsAny<Table>(), It.IsAny<DatabaseSchema>(), It.IsAny<ApiGeneratorConfig>()))
                .ReturnsAsync("Mapping code");

            // Act
            var result = await _orchestrator.GenerateAllAsync(schema, _config);

            // Assert
            Assert.Equal(4, result.Count); // DTOs, Controller, Mapping, Extensions
            Assert.Contains(result.Keys, k => k.Contains("CustomerDto.cs"));
            Assert.Contains(result.Keys, k => k.Contains("CustomersController.cs"));
            Assert.Contains(result.Keys, k => k.Contains("CustomerMappingProfile.cs"));
            Assert.Contains(result.Keys, k => k.Contains("ServiceCollectionExtensions.cs"));
        }

        [Fact]
        public async Task GenerateAllAsync_WithMultipleTables_GeneratesAllFiles()
        {
            // Arrange
            var table1 = new TableBuilder().WithName("Customer").WithIdColumn().Build();
            var table2 = new TableBuilder().WithName("Order").WithIdColumn().Build();

            var schema = new DatabaseSchema { Tables = new[] { table1, table2 } };

            _dtoGeneratorMock
                .Setup(x => x.GenerateAsync(It.IsAny<Table>(), It.IsAny<DatabaseSchema>(), It.IsAny<ApiGeneratorConfig>()))
                .ReturnsAsync("DTO code");

            _controllerGeneratorMock
                .Setup(x => x.GenerateAsync(It.IsAny<Table>(), It.IsAny<DatabaseSchema>(), It.IsAny<ApiGeneratorConfig>()))
                .ReturnsAsync("Controller code");

            _mappingGeneratorMock
                .Setup(x => x.GenerateAsync(It.IsAny<Table>(), It.IsAny<DatabaseSchema>(), It.IsAny<ApiGeneratorConfig>()))
                .ReturnsAsync("Mapping code");

            // Act
            var result = await _orchestrator.GenerateAllAsync(schema, _config);

            // Assert
            Assert.Equal(7, result.Count); // 2 tables × 3 generators + 1 extensions
            Assert.Contains(result.Keys, k => k.Contains("CustomerDto.cs"));
            Assert.Contains(result.Keys, k => k.Contains("OrderDto.cs"));
        }

        [Fact]
        public async Task GenerateAllAsync_GeneratesServiceCollectionExtensions()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Customer")
                .WithIdColumn()
                .WithNameColumn()
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            _dtoGeneratorMock
                .Setup(x => x.GenerateAsync(It.IsAny<Table>(), It.IsAny<DatabaseSchema>(), It.IsAny<ApiGeneratorConfig>()))
                .ReturnsAsync("DTO code");

            _controllerGeneratorMock
                .Setup(x => x.GenerateAsync(It.IsAny<Table>(), It.IsAny<DatabaseSchema>(), It.IsAny<ApiGeneratorConfig>()))
                .ReturnsAsync("Controller code");

            _mappingGeneratorMock
                .Setup(x => x.GenerateAsync(It.IsAny<Table>(), It.IsAny<DatabaseSchema>(), It.IsAny<ApiGeneratorConfig>()))
                .ReturnsAsync("Mapping code");

            // Act
            var result = await _orchestrator.GenerateAllAsync(schema, _config);

            // Assert
            var extensionsKey = result.Keys.First(k => k.Contains("ServiceCollectionExtensions.cs"));
            var extensionsCode = result[extensionsKey];

            Assert.Contains("public static class ServiceCollectionExtensions", extensionsCode);
            Assert.Contains("AddApiServices", extensionsCode);
            Assert.Contains("AddControllers", extensionsCode);
            Assert.Contains("AddAutoMapper", extensionsCode);
            Assert.Contains("CustomerMappingProfile", extensionsCode);
        }

        [Fact]
        public async Task GenerateAllAsync_WithInvalidConfig_ThrowsInvalidOperationException()
        {
            // Arrange
            var invalidConfig = new ApiGeneratorConfig { Namespace = string.Empty };
            var table = new TableBuilder().WithName("Customer").WithIdColumn().Build();
            var schema = new DatabaseSchema { Tables = new[] { table } };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orchestrator.GenerateAllAsync(schema, invalidConfig));
        }

        [Fact]
        public async Task GenerateAllAsync_CallsAllGenerators()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Product")
                .WithIdColumn()
                .WithNameColumn()
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            _dtoGeneratorMock
                .Setup(x => x.GenerateAsync(table, schema, _config))
                .ReturnsAsync("DTO code")
                .Verifiable();

            _controllerGeneratorMock
                .Setup(x => x.GenerateAsync(table, schema, _config))
                .ReturnsAsync("Controller code")
                .Verifiable();

            _mappingGeneratorMock
                .Setup(x => x.GenerateAsync(table, schema, _config))
                .ReturnsAsync("Mapping code")
                .Verifiable();

            // Act
            await _orchestrator.GenerateAllAsync(schema, _config);

            // Assert
            _dtoGeneratorMock.Verify();
            _controllerGeneratorMock.Verify();
            _mappingGeneratorMock.Verify();
        }

        [Fact]
        public async Task GenerateAllAsync_UsesConfiguredOutputDirectories()
        {
            // Arrange
            var customConfig = new ApiGeneratorConfig
            {
                Namespace = "MyApp",
                ControllersOutputDirectory = "CustomControllers",
                DtosOutputDirectory = "CustomDTOs",
                MappingOutputDirectory = "CustomMapping",
                ExtensionsOutputDirectory = "CustomExtensions",
            };

            var table = new TableBuilder().WithName("Customer").WithIdColumn().Build();
            var schema = new DatabaseSchema { Tables = new[] { table } };

            _dtoGeneratorMock
                .Setup(x => x.GenerateAsync(It.IsAny<Table>(), It.IsAny<DatabaseSchema>(), It.IsAny<ApiGeneratorConfig>()))
                .ReturnsAsync("DTO code");

            _controllerGeneratorMock
                .Setup(x => x.GenerateAsync(It.IsAny<Table>(), It.IsAny<DatabaseSchema>(), It.IsAny<ApiGeneratorConfig>()))
                .ReturnsAsync("Controller code");

            _mappingGeneratorMock
                .Setup(x => x.GenerateAsync(It.IsAny<Table>(), It.IsAny<DatabaseSchema>(), It.IsAny<ApiGeneratorConfig>()))
                .ReturnsAsync("Mapping code");

            // Act
            var result = await _orchestrator.GenerateAllAsync(schema, customConfig);

            // Assert
            Assert.Contains(result.Keys, k => k.StartsWith("CustomDTOs/", StringComparison.Ordinal));
            Assert.Contains(result.Keys, k => k.StartsWith("CustomControllers/", StringComparison.Ordinal));
            Assert.Contains(result.Keys, k => k.StartsWith("CustomMapping/", StringComparison.Ordinal));
            Assert.Contains(result.Keys, k => k.StartsWith("CustomExtensions/", StringComparison.Ordinal));
        }
    }
}
