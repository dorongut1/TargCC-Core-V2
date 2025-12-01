// <copyright file="MappingProfileGeneratorTests.cs" company="PlaceholderCompany">
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
    /// Tests for MappingProfileGenerator.
    /// </summary>
    public class MappingProfileGeneratorTests
    {
        private readonly Mock<ILogger<MappingProfileGenerator>> _loggerMock;
        private readonly MappingProfileGenerator _generator;
        private readonly ApiGeneratorConfig _config;

        public MappingProfileGeneratorTests()
        {
            _loggerMock = new Mock<ILogger<MappingProfileGenerator>>();
            _generator = new MappingProfileGenerator(_loggerMock.Object);
            _config = new ApiGeneratorConfig
            {
                Namespace = "TestApp",
                GenerateXmlDocumentation = true,
            };
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new MappingProfileGenerator(null!));
        }

        [Fact]
        public async Task GenerateAsync_BasicTable_GeneratesMappingProfile()
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
            Assert.Contains("public class CustomerMappingProfile : Profile", result);
            Assert.Contains("public CustomerMappingProfile()", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesEntityToDtoMapping()
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
            Assert.Contains("CreateMap<Product, ProductDto>()", result);
            Assert.Contains(".ReverseMap();", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesCreateRequestToEntityMapping()
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
            Assert.Contains("CreateMap<CreateOrderRequest, Order>()", result);
        }

        [Fact]
        public async Task GenerateAsync_GeneratesUpdateRequestToEntityMapping()
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
            Assert.Contains("CreateMap<UpdateCustomerRequest, Customer>()", result);
        }

        [Fact]
        public async Task GenerateAsync_WithLkpPrefix_GeneratesLookupMapping()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Order")
                .WithIdColumn()
                .WithColumn("lkp_status", "NVARCHAR", maxLength: 50, isNullable: false)
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("ForMember(dest => dest.StatusCode", result);
            Assert.Contains("ForMember(dest => dest.StatusText", result);
        }

        [Fact]
        public async Task GenerateAsync_WithLocPrefix_GeneratesLocalizationMapping()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Product")
                .WithIdColumn()
                .WithColumn("loc_description", "NVARCHAR", maxLength: 500, isNullable: false)
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("ForMember(dest => dest.DescriptionLocalized", result);
        }

        [Fact]
        public async Task GenerateAsync_WithSplPrefix_GeneratesSplitMapping()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Article")
                .WithIdColumn()
                .WithColumn("spl_tags", "NVARCHAR", maxLength: 500, isNullable: true)
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("Split('|'", result);
            Assert.Contains("string.Join(\"|\"", result);
        }

        [Fact]
        public async Task GenerateAsync_WithSptPrefix_GeneratesSplitMapping()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("Document")
                .WithIdColumn()
                .WithColumn("spt_categories", "NVARCHAR", maxLength: 500, isNullable: false)
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("Split(',', StringSplitOptions.RemoveEmptyEntries)", result);
            Assert.Contains("string.Join(\",\"", result);
        }

        [Fact]
        public async Task GenerateAsync_WithEnoPrefix_GeneratesHashPasswordMapping()
        {
            // Arrange
            var table = new TableBuilder()
                .WithName("User")
                .WithIdColumn()
                .WithColumn("eno_password", "NVARCHAR", maxLength: 255, isNullable: false)
                .Build();

            var schema = new DatabaseSchema { Tables = new[] { table } };

            // Act
            var result = await _generator.GenerateAsync(table, schema, _config);

            // Assert
            Assert.Contains("HashPassword", result);
            Assert.Contains("TODO: Implement password hashing", result);
        }

        [Fact]
        public async Task GenerateAsync_IncludesAutoMapperUsing()
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
            Assert.Contains("using AutoMapper;", result);
        }
    }
}
