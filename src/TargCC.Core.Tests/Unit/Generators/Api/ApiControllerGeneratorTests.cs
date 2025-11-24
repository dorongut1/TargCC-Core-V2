// <copyright file="ApiControllerGeneratorTests.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators.Api;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Generators.Api;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Tests.TestHelpers;
using Xunit;

/// <summary>
/// Unit tests for the <see cref="ApiControllerGenerator"/> class.
/// </summary>
public class ApiControllerGeneratorTests
{
    private readonly Mock<ILogger<ApiControllerGenerator>> _loggerMock;
    private readonly ApiControllerGenerator _generator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiControllerGeneratorTests"/> class.
    /// </summary>
    public ApiControllerGeneratorTests()
    {
        _loggerMock = new Mock<ILogger<ApiControllerGenerator>>();
        _generator = new ApiControllerGenerator(_loggerMock.Object);
    }

    #region Constructor Tests

    /// <summary>
    /// Tests that the constructor throws when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ApiControllerGenerator(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Tests that the constructor succeeds with valid logger.
    /// </summary>
    [Fact]
    public void Constructor_WithValidLogger_Succeeds()
    {
        // Act
        var generator = new ApiControllerGenerator(_loggerMock.Object);

        // Assert
        generator.Should().NotBeNull();
    }

    #endregion

    #region GenerateAsync - Validation Tests

    /// <summary>
    /// Tests that GenerateAsync throws when table is null.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithNullTable_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _generator.GenerateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("table");
    }

    /// <summary>
    /// Tests that GenerateAsync throws when table has no primary key.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithNoPrimaryKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("TestTable")
            .WithColumn(ColumnBuilder.NameColumn())
            .Build();
        table.PrimaryKey = null;
        table.PrimaryKeyColumns = new List<string>();

        // Act
        var act = () => _generator.GenerateAsync(table);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*primary key*");
    }

    #endregion

    #region GenerateAsync - Basic Generation Tests

    /// <summary>
    /// Tests that GenerateAsync generates a controller for a simple table.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithSimpleTable_GeneratesController()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.Should().NotBeNull();
        result.ControllerCode.Should().NotBeNullOrWhiteSpace();
        result.TableName.Should().Be("Customer");
        result.ControllerName.Should().Be("CustomersController");
        result.EndpointCount.Should().Be(5);
    }

    /// <summary>
    /// Tests that generated controller has correct class declaration.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_GeneratesCorrectClassDeclaration()
    {
        // Arrange
        var table = CreateSimpleTable("Product");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("public class ProductsController : ControllerBase");
    }

    /// <summary>
    /// Tests that generated controller has ApiController attribute.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesApiControllerAttribute()
    {
        // Arrange
        var table = CreateSimpleTable("Order");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("[ApiController]");
    }

    /// <summary>
    /// Tests that generated controller has Route attribute.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesRouteAttribute()
    {
        // Arrange
        var table = CreateSimpleTable("Invoice");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("[Route(\"api/[controller]\")]");
    }

    /// <summary>
    /// Tests that generated controller has Produces attribute.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesProducesAttribute()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("[Produces(\"application/json\")]");
    }

    #endregion

    #region GenerateAsync - Endpoint Tests

    /// <summary>
    /// Tests that generated controller has GetById endpoint.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesGetByIdEndpoint()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("[HttpGet(\"{id}\")]");
        result.ControllerCode.Should().Contain("public async Task<IActionResult> GetById(");
        result.ControllerCode.Should().Contain("new GetCustomerQuery(id)");
    }

    /// <summary>
    /// Tests that generated controller has GetAll endpoint.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesGetAllEndpoint()
    {
        // Arrange
        var table = CreateSimpleTable("Product");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("[HttpGet]");
        result.ControllerCode.Should().Contain("public async Task<IActionResult> GetAll(");
        result.ControllerCode.Should().Contain("[FromQuery] int pageNumber = 1");
        result.ControllerCode.Should().Contain("[FromQuery] int pageSize = 10");
        result.ControllerCode.Should().Contain("new GetProductsQuery(pageNumber, pageSize)");
    }

    /// <summary>
    /// Tests that generated controller has Create endpoint.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesCreateEndpoint()
    {
        // Arrange
        var table = CreateSimpleTable("Order");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("[HttpPost]");
        result.ControllerCode.Should().Contain("public async Task<IActionResult> Create(");
        result.ControllerCode.Should().Contain("[FromBody] CreateOrderCommand command");
        result.ControllerCode.Should().Contain("CreatedAtAction(nameof(GetById)");
    }

    /// <summary>
    /// Tests that generated controller has Update endpoint.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesUpdateEndpoint()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("[HttpPut(\"{id}\")]");
        result.ControllerCode.Should().Contain("public async Task<IActionResult> Update(");
        result.ControllerCode.Should().Contain("[FromBody] UpdateCustomerCommand command");
        result.ControllerCode.Should().Contain("return NoContent();");
    }

    /// <summary>
    /// Tests that generated controller has Delete endpoint.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesDeleteEndpoint()
    {
        // Arrange
        var table = CreateSimpleTable("Invoice");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("[HttpDelete(\"{id}\")]");
        result.ControllerCode.Should().Contain("public async Task<IActionResult> Delete(");
        result.ControllerCode.Should().Contain("new DeleteInvoiceCommand(id)");
    }

    #endregion

    #region GenerateAsync - Swagger/OpenAPI Tests

    /// <summary>
    /// Tests that generated controller has ProducesResponseType attributes.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesProducesResponseTypeAttributes()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("[ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]");
        result.ControllerCode.Should().Contain("[ProducesResponseType(StatusCodes.Status404NotFound)]");
        result.ControllerCode.Should().Contain("[ProducesResponseType(StatusCodes.Status201Created)]");
        result.ControllerCode.Should().Contain("[ProducesResponseType(StatusCodes.Status204NoContent)]");
        result.ControllerCode.Should().Contain("[ProducesResponseType(StatusCodes.Status400BadRequest)]");
        result.ControllerCode.Should().Contain("[ProducesResponseType(StatusCodes.Status500InternalServerError)]");
    }

    /// <summary>
    /// Tests that generated controller includes response documentation.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesResponseDocumentation()
    {
        // Arrange
        var table = CreateSimpleTable("Product");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("/// <response code=\"200\">");
        result.ControllerCode.Should().Contain("/// <response code=\"201\">");
        result.ControllerCode.Should().Contain("/// <response code=\"204\">");
        result.ControllerCode.Should().Contain("/// <response code=\"400\">");
        result.ControllerCode.Should().Contain("/// <response code=\"404\">");
        result.ControllerCode.Should().Contain("/// <response code=\"500\">");
    }

    #endregion

    #region GenerateAsync - XML Documentation Tests

    /// <summary>
    /// Tests that generated controller has XML documentation.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesXmlDocumentation()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("/// <summary>");
        result.ControllerCode.Should().Contain("/// </summary>");
        result.ControllerCode.Should().Contain("/// <param name=");
        result.ControllerCode.Should().Contain("/// <returns>");
        result.ControllerCode.Should().Contain("/// <remarks>");
    }

    /// <summary>
    /// Tests that generated controller has class-level documentation.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesClassDocumentation()
    {
        // Arrange
        var table = CreateSimpleTable("Order");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("/// API controller for Order management.");
        result.ControllerCode.Should().Contain("/// Provides CRUD operations for Orders using CQRS pattern with MediatR.");
    }

    #endregion

    #region GenerateAsync - MediatR Integration Tests

    /// <summary>
    /// Tests that generated controller uses MediatR.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_UsesMediatR()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("using MediatR;");
        result.ControllerCode.Should().Contain("private readonly IMediator _mediator;");
        result.ControllerCode.Should().Contain("await _mediator.Send(");
    }

    /// <summary>
    /// Tests that generated controller constructor has MediatR parameter.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_ConstructorHasMediatorParameter()
    {
        // Arrange
        var table = CreateSimpleTable("Product");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("public ProductsController(IMediator mediator,");
        result.ControllerCode.Should().Contain("_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));");
    }

    #endregion

    #region GenerateAsync - Error Handling Tests

    /// <summary>
    /// Tests that generated controller has try-catch blocks.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesTryCatchBlocks()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("try");
        result.ControllerCode.Should().Contain("catch (Exception ex)");
        result.ControllerCode.Should().Contain("StatusCode(StatusCodes.Status500InternalServerError");
    }

    /// <summary>
    /// Tests that generated controller handles not found responses.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_HandlesNotFoundResponses()
    {
        // Arrange
        var table = CreateSimpleTable("Order");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("return NotFound(result.Error);");
    }

    /// <summary>
    /// Tests that generated controller handles bad request responses.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_HandlesBadRequestResponses()
    {
        // Arrange
        var table = CreateSimpleTable("Invoice");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("return BadRequest(result.Error);");
        result.ControllerCode.Should().Contain("return BadRequest(\"ID in URL does not match ID in request body.\");");
    }

    #endregion

    #region GenerateAsync - Logging Tests

    /// <summary>
    /// Tests that generated controller includes logging.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesLogging()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("using Microsoft.Extensions.Logging;");
        result.ControllerCode.Should().Contain("private readonly ILogger _logger;");
        result.ControllerCode.Should().Contain("_logger.LogDebug(");
        result.ControllerCode.Should().Contain("_logger.LogInformation(");
        result.ControllerCode.Should().Contain("_logger.LogWarning(");
        result.ControllerCode.Should().Contain("_logger.LogError(");
    }

    /// <summary>
    /// Tests that generated controller constructor has logger parameter.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_ConstructorHasLoggerParameter()
    {
        // Arrange
        var table = CreateSimpleTable("Product");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("ILogger<ProductsController> logger)");
        result.ControllerCode.Should().Contain("_logger = logger ?? throw new ArgumentNullException(nameof(logger));");
    }

    #endregion

    #region GenerateAsync - Different Primary Key Types Tests

    /// <summary>
    /// Tests that generated controller handles int primary key.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithIntPrimaryKey_UsesIntType()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("public async Task<IActionResult> GetById(int id,");
        result.ControllerCode.Should().Contain("public async Task<IActionResult> Update(int id,");
        result.ControllerCode.Should().Contain("public async Task<IActionResult> Delete(int id,");
    }

    /// <summary>
    /// Tests that generated controller handles long primary key.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithLongPrimaryKey_UsesLongType()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("LargeTable")
            .WithColumn(new ColumnBuilder()
                .WithName("ID")
                .AsBigInt()
                .AsIdentity()
                .Build())
            .WithNameColumn()
            .WithPrimaryKey("ID")
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("public async Task<IActionResult> GetById(long id,");
    }

    /// <summary>
    /// Tests that generated controller handles Guid primary key.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithGuidPrimaryKey_UsesGuidType()
    {
        // Arrange
        var table = new TableBuilder()
            .WithName("Document")
            .WithColumn(new ColumnBuilder()
                .WithName("ID")
                .WithDataType("uniqueidentifier")
                .NotNullable()
                .AsPrimaryKey()
                .Build())
            .WithNameColumn()
            .WithPrimaryKey("ID")
            .Build();

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("public async Task<IActionResult> GetById(Guid id,");
    }

    #endregion

    #region GenerateAsync - Pluralization Tests

    /// <summary>
    /// Tests that generated controller pluralizes table name correctly.
    /// </summary>
    [Theory]
    [InlineData("Customer", "Customers")]
    [InlineData("Order", "Orders")]
    [InlineData("Category", "Categories")]
    [InlineData("Status", "Statuses")]
    [InlineData("Box", "Boxes")]
    public async Task GenerateAsync_PluralizesTableNameCorrectly(string tableName, string expectedPlural)
    {
        // Arrange
        var table = CreateSimpleTable(tableName);

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerName.Should().Be($"{expectedPlural}Controller");
        result.ControllerCode.Should().Contain($"public class {expectedPlural}Controller : ControllerBase");
    }

    #endregion

    #region GenerateAsync - Result Properties Tests

    /// <summary>
    /// Tests that result has correct route prefix.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_ResultHasCorrectRoutePrefix()
    {
        // Arrange
        var table = CreateSimpleTable("Product");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.RoutePrefix.Should().Be("api/products");
    }

    /// <summary>
    /// Tests that result has generated timestamp.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_ResultHasGeneratedTimestamp()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.GeneratedAt.Should().BeAfter(beforeGeneration.AddSeconds(-1));
        result.GeneratedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    #endregion

    #region GenerateAsync - File Header Tests

    /// <summary>
    /// Tests that generated code has auto-generated header.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_IncludesAutoGeneratedHeader()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("// <auto-generated>");
        result.ControllerCode.Should().Contain("// This code was generated by TargCC API Controller Generator.");
        result.ControllerCode.Should().Contain("// Source Table: Customer");
        result.ControllerCode.Should().Contain("// Controller: CustomersController");
        result.ControllerCode.Should().Contain("// WARNING: Changes to this file may be overwritten during regeneration.");
        result.ControllerCode.Should().Contain("// </auto-generated>");
    }

    #endregion

    #region GenerateAsync - Namespace and Usings Tests

    /// <summary>
    /// Tests that generated code has correct namespace.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_HasCorrectNamespace()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("namespace TargCC.API.Controllers;");
    }

    /// <summary>
    /// Tests that generated code has all required using statements.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_HasAllRequiredUsings()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("using MediatR;");
        result.ControllerCode.Should().Contain("using Microsoft.AspNetCore.Http;");
        result.ControllerCode.Should().Contain("using Microsoft.AspNetCore.Mvc;");
        result.ControllerCode.Should().Contain("using Microsoft.Extensions.Logging;");
        result.ControllerCode.Should().Contain("using TargCC.Application.Common.Models;");
        result.ControllerCode.Should().Contain("using TargCC.Application.Features.Customers.Commands.CreateCustomer;");
        result.ControllerCode.Should().Contain("using TargCC.Application.Features.Customers.Commands.UpdateCustomer;");
        result.ControllerCode.Should().Contain("using TargCC.Application.Features.Customers.Commands.DeleteCustomer;");
        result.ControllerCode.Should().Contain("using TargCC.Application.Features.Customers.Queries.GetCustomer;");
        result.ControllerCode.Should().Contain("using TargCC.Application.Features.Customers.Queries.GetCustomers;");
    }

    #endregion

    #region GenerateAsync - CancellationToken Tests

    /// <summary>
    /// Tests that generated endpoints support CancellationToken.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_EndpointsSupportCancellationToken()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("CancellationToken cancellationToken = default");
        result.ControllerCode.Should().Contain("await _mediator.Send(query, cancellationToken)");
        result.ControllerCode.Should().Contain("await _mediator.Send(command, cancellationToken)");
    }

    #endregion

    #region GenerateAsync - Update ID Validation Tests

    /// <summary>
    /// Tests that Update endpoint validates ID match.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_UpdateEndpointValidatesIdMatch()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");

        // Act
        var result = await _generator.GenerateAsync(table);

        // Assert
        result.ControllerCode.Should().Contain("if (id != command.Id)");
        result.ControllerCode.Should().Contain("ID mismatch in update request");
    }

    #endregion

    #region GenerateAsync - Logging Verification Tests

    /// <summary>
    /// Tests that generator logs information messages.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_LogsInformationMessages()
    {
        // Arrange
        var table = CreateSimpleTable("Customer");

        // Act
        await _generator.GenerateAsync(table);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Generating API controller")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a simple table for testing with ID and Name columns.
    /// </summary>
    private static Table CreateSimpleTable(string tableName)
    {
        return new TableBuilder()
            .WithName(tableName)
            .WithIdColumn()
            .WithNameColumn()
            .WithPrimaryKey("ID")
            .Build();
    }

    #endregion
}
