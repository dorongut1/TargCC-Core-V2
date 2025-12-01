// <copyright file="BaseApiGeneratorTests.cs" company="PlaceholderCompany">
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
    using Xunit;

    /// <summary>
    /// Tests for BaseApiGenerator.
    /// </summary>
    public class BaseApiGeneratorTests
    {
        [Fact]
        public void GetClassName_ValidTableName_ReturnsPascalCase()
        {
            // Arrange
            var tableName = "customer";

            // Act
            var result = TestableGenerator.PublicGetClassName(tableName);

            // Assert
            Assert.Equal("Customer", result);
        }

        [Fact]
        public void GetClassName_WithUnderscores_ReturnsPascalCase()
        {
            // Arrange
            var tableName = "order_item";

            // Act
            var result = TestableGenerator.PublicGetClassName(tableName);

            // Assert
            Assert.Equal("OrderItem", result);
        }

        [Fact]
        public void GetClassName_WithCPrefix_RemovesPrefix()
        {
            // Arrange
            var tableName = "c_customer";

            // Act
            var result = TestableGenerator.PublicGetClassName(tableName);

            // Assert
            Assert.Equal("Customer", result);
        }

        [Theory]
        [InlineData("customer_id", "CustomerId")]
        [InlineData("order_date", "OrderDate")]
        [InlineData("ID", "Id")]
        [InlineData("EmailAddress", "EmailAddress")]
        public void GetPropertyName_ValidNames_ReturnsExpected(string input, string expected)
        {
            // Act
            var result = TestableGenerator.PublicGetPropertyName(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("eno_password", "PasswordHashed")]
        [InlineData("lkp_status", "Status")]
        [InlineData("loc_description", "Description")]
        [InlineData("agg_count", "CountAggregate")]
        [InlineData("scb_approved", "ApprovedChangedBy")]
        [InlineData("spl_tags", "Tags")]
        [InlineData("spt_categories", "Categories")]
        [InlineData("upl_avatar", "Avatar")]
        [InlineData("ent_reference", "Reference")]
        [InlineData("enm_status", "Status")]
        public void GetPropertyName_WithPrefixes_HandlesCorrectly(string input, string expected)
        {
            // Act
            var result = TestableGenerator.PublicGetPropertyName(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("eno_password", "ENO", "password")]
        [InlineData("lkp_status", "LKP", "status")]
        [InlineData("loc_description", "LOC", "description")]
        [InlineData("customer_id", "", "customer_id")]
        [InlineData("ENO_PASSWORD", "ENO", "PASSWORD")]
        [InlineData("Lkp_Status", "LKP", "Status")]
        public void SplitPrefix_Various_ReturnsTuple(string input, string expectedPrefix, string expectedBase)
        {
            // Act
            var result = TestableGenerator.PublicSplitPrefix(input);

            // Assert
            Assert.Equal(expectedPrefix, result.prefix);
            Assert.Equal(expectedBase, result.baseName);
        }

        [Theory]
        [InlineData("customer_id", "CustomerId")]
        [InlineData("order item", "OrderItem")]
        [InlineData("product-name", "ProductName")]
        [InlineData("status", "Status")]
        [InlineData("ID", "Id")]
        [InlineData("customer", "Customer")]
        public void ToPascalCase_Various_ReturnsExpected(string input, string expected)
        {
            // Act
            var result = TestableGenerator.PublicToPascalCase(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("CustomerId", "customerId")]
        [InlineData("OrderItem", "orderItem")]
        [InlineData("ID", "id")]
        [InlineData("Customer", "customer")]
        public void ToCamelCase_Various_ReturnsExpected(string input, string expected)
        {
            // Act
            var result = TestableGenerator.PublicToCamelCase(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("INT", false, "int")]
        [InlineData("INT", true, "int?")]
        [InlineData("VARCHAR", false, "string")]
        [InlineData("VARCHAR", true, "string")]
        [InlineData("DATETIME", false, "DateTime")]
        [InlineData("DATETIME", true, "DateTime?")]
        [InlineData("BIT", false, "bool")]
        [InlineData("BIT", true, "bool?")]
        [InlineData("DECIMAL", false, "decimal")]
        [InlineData("DECIMAL", true, "decimal?")]
        [InlineData("UNIQUEIDENTIFIER", false, "Guid")]
        [InlineData("UNIQUEIDENTIFIER", true, "Guid?")]
        public void GetCSharpType_Various_ReturnsExpected(string sqlType, bool isNullable, string expected)
        {
            // Act
            var result = TestableGenerator.PublicGetCSharpType(sqlType, isNullable);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GenerateFileHeader_Valid_ContainsTableName()
        {
            // Arrange
            var tableName = "Customer";
            var generatorType = "Test Generator";

            // Act
            var result = TestableGenerator.PublicGenerateFileHeader(tableName, generatorType);

            // Assert
            Assert.Contains("Customer", result);
            Assert.Contains("Test Generator", result);
            Assert.Contains("<auto-generated>", result);
        }

        [Theory]
        [InlineData("AddedBy", true)]
        [InlineData("AddedOn", true)]
        [InlineData("ChangedBy", true)]
        [InlineData("ChangedOn", true)]
        [InlineData("CustomerId", false)]
        [InlineData("Name", false)]
        public void IsAuditField_Various_ReturnsExpected(string columnName, bool expected)
        {
            // Act
            var result = TestableGenerator.PublicIsAuditField(columnName);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("clc_total", true)]
        [InlineData("blg_count", true)]
        [InlineData("agg_sum", true)]
        [InlineData("scb_approved", true)]
        [InlineData("customer_id", false)]
        [InlineData("lkp_status", false)]
        public void IsReadOnlyPrefix_Various_ReturnsExpected(string columnName, bool expected)
        {
            // Act
            var result = TestableGenerator.PublicIsReadOnlyPrefix(columnName);

            // Assert
            Assert.Equal(expected, result);
        }

        // Testable wrapper class to expose protected methods
        private class TestableGenerator : BaseApiGenerator
        {
            public TestableGenerator()
                : base(new Mock<ILogger>().Object)
            {
            }

            protected override string GeneratorTypeName => "TestGenerator";

            protected override string Generate(Table table, DatabaseSchema schema, ApiGeneratorConfig config)
            {
                return "Test output";
            }

            public static string PublicGetClassName(string tableName) => GetClassName(tableName);

            public static string PublicGetPropertyName(string columnName) => GetPropertyName(columnName);

            public static (string prefix, string baseName) PublicSplitPrefix(string columnName) => SplitPrefix(columnName);

            public static string PublicToPascalCase(string input) => ToPascalCase(input);

            public static string PublicToCamelCase(string input) => ToCamelCase(input);

            public static string PublicGetCSharpType(string sqlType, bool isNullable) => GetCSharpType(sqlType, isNullable);

            public static string PublicGenerateFileHeader(string tableName, string generatorType) =>
                GenerateFileHeader(tableName, generatorType);

            public static bool PublicIsAuditField(string columnName) => IsAuditField(columnName);

            public static bool PublicIsReadOnlyPrefix(string columnName) => IsReadOnlyPrefix(columnName);
        }
    }
}
