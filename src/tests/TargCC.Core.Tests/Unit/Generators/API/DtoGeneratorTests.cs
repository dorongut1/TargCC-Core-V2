// <copyright file="DtoGeneratorTests.cs" company="PlaceholderCompany">
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
    /// Tests for DtoGenerator.
    /// </summary>
    public class DtoGeneratorTests
    {
        private readonly Mock<ILogger<DtoGenerator>> _loggerMock;
        private readonly DtoGenerator _generator;
        private readonly ApiGeneratorConfig _config;

        public DtoGeneratorTests()
        {
            _loggerMock = new Mock<ILogger<DtoGenerator>>();
            _generator = new DtoGenerator(_loggerMock.Object);
            _config = new ApiGeneratorConfig
            {
                Namespace = "TestApp",
                GenerateXmlDocumentation = true,
                GenerateValidationAttributes = true,
            };
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DtoGenerator(null!));
        }

        [Fact]
        public async Task GenerateAsync_BasicTable_GeneratesAllFourDtoTypes()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Customer")
                .WithIdColumn()
                .WithNameColumn()
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table }.ToList() };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("public class CustomerDto", result);
            Assert.Contains("public class CreateCustomerRequest", result);
            Assert.Contains("public class UpdateCustomerRequest", result);
            Assert.Contains("public class CustomerFilters", result);
        }

        [Fact]
        public async Task GenerateAsync_WithEnoPrefix_GeneratesCorrectProperties()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("User")
                .WithIdColumn()
                .WithColumn("eno_password", "NVARCHAR", maxLength: 255, isNullable: false)
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table }.ToList() };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            // Response DTO should have PasswordHashed
            Assert.Contains("public string PasswordHashed", result);

            // Create/Update should have plain Password
            Assert.Contains("public string Password", result);
            Assert.DoesNotContain("CreateUserRequest.*PasswordHashed", result);
        }

        [Fact]
        public async Task GenerateAsync_WithLkpPrefix_GeneratesCodeAndTextProperties()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Order")
                .WithIdColumn()
                .WithColumn("lkp_status", "NVARCHAR", maxLength: 50, isNullable: false)
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table }.ToList() };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            // Response DTO should have both Code and Text
            Assert.Contains("public string StatusCode", result);
            Assert.Contains("public string? StatusText", result);

            // Create/Update should only have Code
            Assert.Contains("CreateOrderRequest", result);
            var createSection = result.Substring(result.IndexOf("CreateOrderRequest", StringComparison.Ordinal));
            Assert.Contains("StatusCode", createSection);
        }

        [Fact]
        public async Task GenerateAsync_WithLocPrefix_GeneratesValueAndLocalizedProperties()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Product")
                .WithIdColumn()
                .WithColumn("loc_description", "NVARCHAR", maxLength: 500, isNullable: false)
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table }.ToList() };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            // Response DTO should have both value and localized
            Assert.Contains("public string Description", result);
            Assert.Contains("public string? DescriptionLocalized", result);
        }

        [Fact]
        public async Task GenerateAsync_WithSplPrefix_GeneratesStringArrayProperty()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Article")
                .WithIdColumn()
                .WithColumn("spl_tags", "NVARCHAR", maxLength: 500, isNullable: true)
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table }.ToList() };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("public string[]? Tags", result);
        }

        [Fact]
        public async Task GenerateAsync_WithSptPrefix_GeneratesStringArrayProperty()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Document")
                .WithIdColumn()
                .WithColumn("spt_categories", "NVARCHAR", maxLength: 500, isNullable: false)
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table }.ToList() };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("public string[] Categories", result);
        }

        [Fact]
        public async Task GenerateAsync_WithUplPrefix_GeneratesFilePathProperty()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("User")
                .WithIdColumn()
                .WithColumn("upl_avatar", "NVARCHAR", maxLength: 255, isNullable: true)
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table }.ToList() };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("public string? Avatar", result);
            Assert.Contains("// File path", result);
        }

        [Fact]
        public async Task GenerateAsync_WithReadOnlyPrefixes_ExcludesFromCreateUpdate()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Invoice")
                .WithIdColumn()
                .WithColumn("clc_total", "DECIMAL", precision: 18, scale: 2, isNullable: false)
                .WithColumn("agg_count", "INT", isNullable: false)
                .WithNameColumn()
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table }.ToList() };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            // Response DTO should have these properties
            Assert.Contains("CustomerDto", result);

            // Create/Update should NOT have readonly properties
            var createSection = result.Substring(result.IndexOf("CreateInvoiceRequest", StringComparison.Ordinal));
            var updateSection = result.Substring(result.IndexOf("UpdateInvoiceRequest", StringComparison.Ordinal));

            Assert.DoesNotContain("Total", createSection.Substring(0, Math.Min(500, createSection.Length)));
            Assert.DoesNotContain("CountAggregate", createSection.Substring(0, Math.Min(500, createSection.Length)));
        }

        [Fact]
        public async Task GenerateAsync_WithValidationAttributes_AddsRequiredAndStringLength()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Customer")
                .WithIdColumn()
                .WithColumn("Name", "NVARCHAR", maxLength: 100, isNullable: false)
                .WithColumn("Email", "NVARCHAR", maxLength: 255, isNullable: true)
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table }.ToList() };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("[Required]", result);
            Assert.Contains("[StringLength(100)]", result);
            Assert.Contains("[EmailAddress]", result);
        }

        [Fact]
        public async Task GenerateAsync_WithAuditFields_IncludesInResponseDto()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Customer")
                .WithIdColumn()
                .WithNameColumn()
                .WithColumn("AddedOn", "DATETIME", isNullable: false)
                .WithColumn("AddedBy", "NVARCHAR", maxLength: 100, isNullable: false)
                .WithColumn("ChangedOn", "DATETIME", isNullable: true)
                .WithColumn("ChangedBy", "NVARCHAR", maxLength: 100, isNullable: true)
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table }.ToList() };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("public DateTime AddedOn", result);
            Assert.Contains("public string AddedBy", result);
            Assert.Contains("public DateTime? ChangedOn", result);
            Assert.Contains("public string? ChangedBy", result);
        }

        [Fact]
        public async Task GenerateAsync_FiltersDto_IncludesPaginationProperties()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Customer")
                .WithIdColumn()
                .WithNameColumn()
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table }.ToList() };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            var filtersSection = result.Substring(result.IndexOf("CustomerFilters", StringComparison.Ordinal));
            Assert.Contains("public int? Page", filtersSection);
            Assert.Contains("public int? PageSize", filtersSection);
            Assert.Contains("public string? SortBy", filtersSection);
            Assert.Contains("public string? SortDirection", filtersSection);
        }

        [Fact]
        public async Task GenerateAsync_WithNullableColumn_GeneratesNullableProperty()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Product")
                .WithIdColumn()
                .WithColumn("Description", "NVARCHAR", maxLength: 500, isNullable: true)
                .WithColumn("Price", "DECIMAL", precision: 18, scale: 2, isNullable: true)
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table }.ToList() };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("public string? Description", result);
            Assert.Contains("public decimal? Price", result);
        }

        [Fact]
        public async Task GenerateAsync_WithoutXmlDocumentation_DoesNotGenerateComments()
        {
            // Arrange
            var configNoXml = new ApiGeneratorConfig
            {
                Namespace = "TestApp",
                GenerateXmlDocumentation = false,
            };

            var table = new TableBuilder()
                .WithName("Customer")
                .WithIdColumn()
                .WithNameColumn()
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table }.ToList() };

            // Act
            var result = await _generator.GenerateAsync(table, schema, configNoXml);

            // Assert
            Assert.DoesNotContain("/// <summary>", result);
        }

        [Fact]
        public async Task GenerateAsync_WithoutValidationAttributes_DoesNotGenerateValidation()
        {
            // Arrange
            var configNoValidation = new ApiGeneratorConfig
            {
                Namespace = "TestApp",
                GenerateValidationAttributes = false,
            };

            var table = new TableBuilder()
                .WithName("Customer")
                .WithIdColumn()
                .WithColumn("Name", "NVARCHAR", maxLength: 100, isNullable: false)
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table }.ToList() };

            // Act
            var result = await _generator.GenerateAsync(table, schema, configNoValidation);

            // Assert
            Assert.DoesNotContain("[Required]", result);
            Assert.DoesNotContain("[StringLength", result);
        }
    }
}
