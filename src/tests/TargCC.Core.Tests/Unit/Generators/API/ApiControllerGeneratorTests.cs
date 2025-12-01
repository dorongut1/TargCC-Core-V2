// <copyright file="ApiControllerGeneratorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators.API
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Moq;
    using TargCC.Core.Generators.API;
    using TargCC.Core.Interfaces.Models;
    using TargCC.Core.Tests.TestHelpers;
    using Xunit;

    /// <summary>
    /// Tests for ApiControllerGenerator.
    /// </summary>
    public class ApiControllerGeneratorTests
    {
        private readonly Mock<ILogger<ApiControllerGenerator>> _loggerMock;
        private readonly ApiControllerGenerator _generator;
        private readonly ApiGeneratorConfig _config;

        public ApiControllerGeneratorTests()
        {
            _loggerMock = new Mock<ILogger<ApiControllerGenerator>>();
            _generator = new ApiControllerGenerator(_loggerMock.Object);
            _config = new ApiGeneratorConfig
            {
                Namespace = "TestApp",
                GenerateXmlDocumentation = true,
                GenerateSwaggerAttributes = true,
            };
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ApiControllerGenerator(null!));
        }

        [Fact]
        public async Task GenerateAsync_BasicTable_GeneratesControllerClass()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Customer")
                .WithIdColumn()
                .WithNameColumn()
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("public class CustomersController : ControllerBase", result);
            Assert.Contains("[ApiController]", result);
            Assert.Contains("[Route(\"api/[controller]\")]", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesAllCrudMethods()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Product")
                .WithIdColumn()
                .WithNameColumn()
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("public async Task<ActionResult<ProductDto>> GetById(int id)", result);
            Assert.Contains("public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll", result);
            Assert.Contains("public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest request)", result);
            Assert.Contains("public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] UpdateProductRequest request)", result);
            Assert.Contains("public async Task<IActionResult> Delete(int id)", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesDependencyInjectionFields()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Order")
                .WithIdColumn()
                .WithNameColumn()
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("private readonly IRepository<Order> _repository;", result);
            Assert.Contains("private readonly IMapper _mapper;", result);
            Assert.Contains("private readonly ILogger<OrdersController> _logger;", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesConstructorWithDI()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Customer")
                .WithIdColumn()
                .WithNameColumn()
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("public CustomersController(", result);
            Assert.Contains("IRepository<Customer> repository", result);
            Assert.Contains("IMapper mapper", result);
            Assert.Contains("ILogger<CustomersController> logger", result);
            Assert.Contains("_repository = repository ?? throw new ArgumentNullException(nameof(repository));", result);
        }

        [Fact]
        public async Task GenerateAsync_WithSwaggerAttributes_GeneratesProducesResponseType()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Product")
                .WithIdColumn()
                .WithNameColumn()
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("[ProducesResponseType(typeof(ProductDto), 200)]", result);
            Assert.Contains("[ProducesResponseType(404)]", result);
            Assert.Contains("[ProducesResponseType(typeof(ProductDto), 201)]", result);
            Assert.Contains("[ProducesResponseType(400)]", result);
            Assert.Contains("[ProducesResponseType(204)]", result);
        }

        [Fact]
        public async Task GenerateAsync_WithoutSwaggerAttributes_DoesNotGenerateProducesResponseType()
        {
            // Arrange
            var configNoSwagger = new ApiGeneratorConfig
            {
                Namespace = "TestApp",
                GenerateSwaggerAttributes = false,
            };

            var table = new TableBuilder()
                .WithName("Customer")
                .WithIdColumn()
                .WithNameColumn()
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, configNoSwagger);

            // Assert
            Assert.DoesNotContain("[ProducesResponseType", result);
            Assert.DoesNotContain("[Produces(\"application/json\")]", result);
        }

        [Fact]
        public async Task GenerateAsync_PluralizesControllerName()
        {
            // Arrange
            var table1 = new TableBuilder().WithName("Customer").WithIdColumn().Build();
            var table2 = new TableBuilder().WithName("Category").WithIdColumn().Build();
            var table3 = new TableBuilder().WithName("Box").WithIdColumn().Build();

            var schema = new DatabaseSchema { Tables = new[] { table1 } };

            // Act
            var result1 = await _generator.GenerateAsync(table1, schema, _config);
            var result2 = await _generator.GenerateAsync(table2, schema, _config);
            var result3 = await _generator.GenerateAsync(table3, schema, _config);

            // Assert
            Assert.Contains("public class CustomersController", result1);
            Assert.Contains("public class CategoriesController", result2); // y -> ies
            Assert.Contains("public class BoxesController", result3); // x -> es
        }

        [Fact]
        public async Task GenerateAsync_GeneratesXmlDocumentation()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Product")
                .WithIdColumn()
                .WithNameColumn()
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("/// <summary>", result);
            Assert.Contains("/// API controller for Product entities.", result);
            Assert.Contains("/// Gets a Product by ID.", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesModelStateValidation()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Customer")
                .WithIdColumn()
                .WithNameColumn()
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("if (!ModelState.IsValid)", result);
            Assert.Contains("return BadRequest(ModelState);", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesConfigureAwait()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Customer")
                .WithIdColumn()
                .WithNameColumn()
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains(".ConfigureAwait(false)", result);
        }
    }
}
