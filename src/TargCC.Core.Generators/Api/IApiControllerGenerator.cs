// <copyright file="IApiControllerGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.Api;

using TargCC.Core.Interfaces.Models;

/// <summary>
/// Generator for creating REST API controllers following Clean Architecture patterns.
/// Generates controllers that use MediatR for CQRS pattern integration.
/// </summary>
/// <remarks>
/// <para>
/// The ApiControllerGenerator creates complete REST API controllers with:
/// </para>
/// <list type="bullet">
/// <item><description>GET /{id} - Get single entity by ID</description></item>
/// <item><description>GET / - Get all entities with pagination</description></item>
/// <item><description>POST / - Create new entity</description></item>
/// <item><description>PUT /{id} - Update existing entity</description></item>
/// <item><description>DELETE /{id} - Delete entity</description></item>
/// </list>
/// <para>
/// All endpoints use MediatR to dispatch queries and commands to their handlers,
/// following the CQRS pattern established in the Application layer.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Basic usage
/// var generator = new ApiControllerGenerator(logger);
/// var result = await generator.GenerateAsync(customerTable);
///
/// // Access generated code
/// string controllerCode = result.ControllerCode;
///
/// // Generated controller example:
/// // [ApiController]
/// // [Route("api/[controller]")]
/// // public class CustomersController : ControllerBase
/// // {
/// //     [HttpGet("{id}")]
/// //     public async Task&lt;IActionResult&gt; GetById(int id) { ... }
/// //
/// //     [HttpGet]
/// //     public async Task&lt;IActionResult&gt; GetAll([FromQuery] int pageNumber = 1) { ... }
/// //
/// //     [HttpPost]
/// //     public async Task&lt;IActionResult&gt; Create([FromBody] CreateCustomerCommand command) { ... }
/// //
/// //     [HttpPut("{id}")]
/// //     public async Task&lt;IActionResult&gt; Update(int id, [FromBody] UpdateCustomerCommand command) { ... }
/// //
/// //     [HttpDelete("{id}")]
/// //     public async Task&lt;IActionResult&gt; Delete(int id) { ... }
/// // }
/// </code>
/// </example>
public interface IApiControllerGenerator
{
    /// <summary>
    /// Generates a complete REST API controller for the specified table.
    /// </summary>
    /// <param name="table">The database table to generate the controller for.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>
    /// A <see cref="ApiControllerGenerationResult"/> containing the generated controller code.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="table"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the table has no primary key.</exception>
    /// <remarks>
    /// The generated controller includes:
    /// <list type="bullet">
    /// <item><description>Swagger/OpenAPI annotations for documentation</description></item>
    /// <item><description>Proper HTTP status code responses</description></item>
    /// <item><description>MediatR integration for CQRS</description></item>
    /// <item><description>Comprehensive XML documentation</description></item>
    /// <item><description>Error handling with try-catch blocks</description></item>
    /// <item><description>Structured logging</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var table = new TableBuilder()
    ///     .WithName("Customer")
    ///     .WithIdColumn()
    ///     .WithNameColumn()
    ///     .Build();
    ///
    /// var result = await generator.GenerateAsync(table);
    /// await File.WriteAllTextAsync("CustomersController.cs", result.ControllerCode);
    /// </code>
    /// </example>
    Task<ApiControllerGenerationResult> GenerateAsync(Table table, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contains the result of API controller generation.
/// </summary>
/// <remarks>
/// This class holds the generated controller code and metadata about the generation.
/// </remarks>
public class ApiControllerGenerationResult
{
    /// <summary>
    /// Gets or sets the generated controller class code.
    /// </summary>
    /// <value>
    /// The complete C# source code for the API controller, including all endpoints,
    /// using statements, and XML documentation.
    /// </value>
    public string ControllerCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the table name used for generation.
    /// </summary>
    /// <value>The name of the source database table.</value>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pluralized controller name.
    /// </summary>
    /// <value>
    /// The pluralized name used for the controller (e.g., "Customers" for table "Customer").
    /// </value>
    public string ControllerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the route prefix for the controller.
    /// </summary>
    /// <value>
    /// The API route prefix (e.g., "api/customers").
    /// </value>
    public string RoutePrefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of endpoints generated.
    /// </summary>
    /// <value>The count of HTTP endpoints in the generated controller.</value>
    public int EndpointCount { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the code was generated.
    /// </summary>
    /// <value>UTC timestamp of code generation.</value>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
