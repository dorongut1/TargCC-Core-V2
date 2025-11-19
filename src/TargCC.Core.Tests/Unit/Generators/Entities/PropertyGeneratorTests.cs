// <copyright file="PropertyGeneratorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators.Entities;

using TargCC.Core.Generators.Entities;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Tests.TestHelpers;
using Xunit;

/// <summary>
/// Unit tests for PropertyGenerator class.
/// Tests entity property generation with attributes, prefixes, and audit columns.
/// </summary>
public class PropertyGeneratorTests
{
    #region GenerateProperty - Basic Tests

    [Fact]
    public void GenerateProperty_SimpleColumn_ReturnsBasicProperty()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("CustomerName")
            .AsNvarchar(100)
            .NotNullable()
            .Build();

        // Act
        var result = PropertyGenerator.GenerateProperty(column);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public string CustomerName", result);
        Assert.Contains("get; set;", result);
    }

    [Fact]
    public void GenerateProperty_PrimaryKey_HasKeyAttribute()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("ID")
            .AsInt()
            .AsPrimaryKey()
            .Build();

        // Act
        var result = PropertyGenerator.GenerateProperty(column);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("[Key]", result);
        Assert.Contains("public int ID", result);
    }

    [Fact]
    public void GenerateProperty_AuditColumn_HasDatabaseGeneratedAttribute()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("AddedOn")
            .AsDateTime()
            .NotNullable()
            .Build();

        // Act
        var result = PropertyGenerator.GenerateProperty(column);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("[DatabaseGenerated(DatabaseGeneratedOption.Identity)]", result);
        Assert.Contains("private set", result);
    }

    [Fact]
    public void GenerateProperty_NotNullable_HasRequiredAttribute()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("Email")
            .AsNvarchar(100)
            .NotNullable()
            .Build();

        // Act
        var result = PropertyGenerator.GenerateProperty(column);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("[Required]", result);
    }

    [Fact]
    public void GenerateProperty_Nullable_NoRequiredAttribute()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("MiddleName")
            .AsNvarchar(50)
            .Nullable()
            .Build();

        // Act
        var result = PropertyGenerator.GenerateProperty(column);

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain("[Required]", result);
    }

    [Fact]
    public void GenerateProperty_StringWithMaxLength_HasMaxLengthAttribute()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("Description")
            .AsNvarchar(500)
            .Build();

        // Act
        var result = PropertyGenerator.GenerateProperty(column);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("[MaxLength(500)]", result);
    }

    [Fact]
    public void GenerateProperty_StringMaxValue_HasMaxLengthIntMaxValue()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("Notes")
            .AsNvarchar(-1) // MAX
            .Build();

        // Act
        var result = PropertyGenerator.GenerateProperty(column);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("[MaxLength(int.MaxValue)]", result);
    }

    [Fact]
    public void GenerateProperty_HasColumnAttribute()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("CustomerName")
            .AsNvarchar(100)
            .Build();

        // Act
        var result = PropertyGenerator.GenerateProperty(column);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("[Column(\"CustomerName\")]", result);
    }

    [Fact]
    public void GenerateProperty_NullColumn_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PropertyGenerator.GenerateProperty(null!));
    }

    #endregion

    #region GenerateProperty - Prefixed Columns

    [Fact]
    public void GenerateProperty_OneWayEncryption_UsesPrefixHandler()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("eno_Password")
            .WithPrefix(ColumnPrefix.OneWayEncryption)
            .AsVarchar(64)
            .NotNullable()
            .Build();

        // Act
        var result = PropertyGenerator.GenerateProperty(column);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("PasswordHashed", result);
        Assert.Contains("[JsonIgnore]", result);
    }

    [Fact]
    public void GenerateProperty_TwoWayEncryption_UsesPrefixHandler()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("ent_SSN")
            .WithPrefix(ColumnPrefix.TwoWayEncryption)
            .Build();

        // Act
        var result = PropertyGenerator.GenerateProperty(column);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("SSN", result);
        Assert.Contains("DecryptValue", result);
        Assert.Contains("EncryptValue", result);
    }

    [Fact]
    public void GenerateProperty_Lookup_UsesPrefixHandler()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("lkp_Status")
            .WithPrefix(ColumnPrefix.Lookup)
            .Build();

        // Act
        var result = PropertyGenerator.GenerateProperty(column);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Status", result);
        Assert.Contains("StatusText", result);
    }

    [Fact]
    public void GenerateProperty_Calculated_UsesPrefixHandler()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("clc_TotalAmount")
            .WithPrefix(ColumnPrefix.Calculated)
            .AsDecimal()
            .Build();

        // Act
        var result = PropertyGenerator.GenerateProperty(column);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("TotalAmountCalculated", result);
        Assert.Contains("internal set", result);
    }

    #endregion

    #region GenerateBackingField Tests

    [Fact]
    public void GenerateBackingField_TwoWayEncryption_ReturnsBackingField()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("ent_CreditCard")
            .WithPrefix(ColumnPrefix.TwoWayEncryption)
            .Build();

        // Act
        var result = PropertyGenerator.GenerateBackingField(column);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("private string", result);
        Assert.Contains("_creditCardEncrypted", result);
    }

    [Fact]
    public void GenerateBackingField_NotTwoWayEncryption_ReturnsNull()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("CustomerName")
            .WithPrefix(ColumnPrefix.None)
            .Build();

        // Act
        var result = PropertyGenerator.GenerateBackingField(column);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region IsAuditColumn Tests

    [Theory]
    [InlineData("AddedOn")]
    [InlineData("AddedBy")]
    [InlineData("ChangedOn")]
    [InlineData("ChangedBy")]
    public void IsAuditColumn_AuditColumnName_ReturnsTrue(string columnName)
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName(columnName)
            .Build();

        // Act
        var result = PropertyGenerator.IsAuditColumn(column);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("ID")]
    [InlineData("CustomerName")]
    [InlineData("Email")]
    [InlineData("Status")]
    public void IsAuditColumn_NonAuditColumnName_ReturnsFalse(string columnName)
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName(columnName)
            .Build();

        // Act
        var result = PropertyGenerator.IsAuditColumn(column);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void GenerateProperty_ComplexPrefixedColumn_GeneratesCorrectly()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("eno_Password")
            .WithPrefix(ColumnPrefix.OneWayEncryption)
            .AsVarchar(64)
            .NotNullable()
            .Build();

        // Act
        var result = PropertyGenerator.GenerateProperty(column);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("PasswordHashed", result);
        Assert.Contains("[JsonIgnore]", result);
        Assert.Contains("[Required]", result);
        Assert.Contains("private set", result);
    }

    [Fact]
    public void GenerateProperty_WithXmlDocumentation_HasSummary()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("CustomerName")
            .AsNvarchar(100)
            .NotNullable()
            .Build();

        // Act
        var result = PropertyGenerator.GenerateProperty(column);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("/// <summary>", result);
        Assert.Contains("/// </summary>", result);
    }

    #endregion
}
