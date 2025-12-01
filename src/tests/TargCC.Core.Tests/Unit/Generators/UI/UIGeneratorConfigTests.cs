// <copyright file="UIGeneratorConfigTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators.UI
{
    using System;
    using TargCC.Core.Generators.UI;
    using Xunit;

    /// <summary>
    /// Tests for UIGeneratorConfig.
    /// </summary>
    public class UIGeneratorConfigTests
    {
        [Fact]
        public void Constructor_Default_SetsExpectedDefaults()
        {
            // Act
            var config = new UIGeneratorConfig();

            // Assert
            Assert.Equal("./generated", config.OutputDirectory);
            Assert.Equal("generated", config.TypeScriptNamespace);
            Assert.True(config.UseReactQuery);
            Assert.True(config.UseMaterialUI);
            Assert.True(config.UseFormik);
            Assert.True(config.UseYupValidation);
            Assert.True(config.GenerateComments);
            Assert.True(config.GenerateJsDoc);
            Assert.Equal(2, config.IndentSize);
            Assert.False(config.OverwritePartialFiles);
        }

        [Fact]
        public void Validate_ValidConfig_DoesNotThrow()
        {
            // Arrange
            var config = new UIGeneratorConfig
            {
                OutputDirectory = "/output",
                TypeScriptNamespace = "myapp",
                IndentSize = 4,
            };

            // Act & Assert
            config.Validate(); // Should not throw
        }

        [Fact]
        public void Validate_EmptyOutputDirectory_ThrowsArgumentException()
        {
            // Arrange
            var config = new UIGeneratorConfig
            {
                OutputDirectory = string.Empty,
            };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => config.Validate());
            Assert.Contains("OutputDirectory", ex.Message);
        }

        [Fact]
        public void Validate_EmptyNamespace_ThrowsArgumentException()
        {
            // Arrange
            var config = new UIGeneratorConfig
            {
                TypeScriptNamespace = string.Empty,
            };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => config.Validate());
            Assert.Contains("TypeScriptNamespace", ex.Message);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(9)]
        [InlineData(100)]
        public void Validate_InvalidIndentSize_ThrowsArgumentException(int indentSize)
        {
            // Arrange
            var config = new UIGeneratorConfig
            {
                IndentSize = indentSize,
            };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => config.Validate());
            Assert.Contains("IndentSize", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        public void Validate_ValidIndentSize_DoesNotThrow(int indentSize)
        {
            // Arrange
            var config = new UIGeneratorConfig
            {
                IndentSize = indentSize,
            };

            // Act & Assert
            config.Validate(); // Should not throw
        }
    }
}
