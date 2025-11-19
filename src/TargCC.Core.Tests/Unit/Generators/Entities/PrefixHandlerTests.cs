// <copyright file="PrefixHandlerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators.Entities;

using TargCC.Core.Generators.Entities;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Tests.TestHelpers;
using Xunit;

/// <summary>
/// Unit tests for PrefixHandler class.
/// Tests TargCC column prefix handling and property generation for all 12 prefix types.
/// </summary>
public class PrefixHandlerTests
{
    #region GetPropertyName Tests

    [Fact]
    public void GetPropertyName_NoPrefix_ReturnsOriginalName()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("CustomerName")
            .WithPrefix(ColumnPrefix.None)
            .Build();

        // Act
        var result = PrefixHandler.GetPropertyName(column);

        // Assert
        Assert.Equal("CustomerName", result);
    }

    [Fact]
    public void GetPropertyName_OneWayEncryption_RemovesPrefixAndAddsHashed()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("eno_Password")
            .WithPrefix(ColumnPrefix.OneWayEncryption)
            .Build();

        // Act
        var result = PrefixHandler.GetPropertyName(column);

        // Assert
        Assert.Equal("PasswordHashed", result);
    }

    [Fact]
    public void GetPropertyName_TwoWayEncryption_RemovesPrefix()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("ent_CreditCard")
            .WithPrefix(ColumnPrefix.TwoWayEncryption)
            .Build();

        // Act
        var result = PrefixHandler.GetPropertyName(column);

        // Assert
        Assert.Equal("CreditCard", result);
    }

    [Fact]
    public void GetPropertyName_Lookup_RemovesPrefix()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("lkp_Status")
            .WithPrefix(ColumnPrefix.Lookup)
            .Build();

        // Act
        var result = PrefixHandler.GetPropertyName(column);

        // Assert
        Assert.Equal("Status", result);
    }

    [Fact]
    public void GetPropertyName_Enumeration_RemovesPrefix()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("enm_Priority")
            .WithPrefix(ColumnPrefix.Enumeration)
            .Build();

        // Act
        var result = PrefixHandler.GetPropertyName(column);

        // Assert
        Assert.Equal("Priority", result);
    }

    [Fact]
    public void GetPropertyName_Localization_RemovesPrefix()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("loc_Description")
            .WithPrefix(ColumnPrefix.Localization)
            .Build();

        // Act
        var result = PrefixHandler.GetPropertyName(column);

        // Assert
        Assert.Equal("Description", result);
    }

    [Fact]
    public void GetPropertyName_Calculated_RemovesPrefixAndAddsCalculated()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("clc_TotalAmount")
            .WithPrefix(ColumnPrefix.Calculated)
            .Build();

        // Act
        var result = PrefixHandler.GetPropertyName(column);

        // Assert
        Assert.Equal("TotalAmountCalculated", result);
    }

    [Fact]
    public void GetPropertyName_BusinessLogic_RemovesPrefixAndAddsBL()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("blg_CreditScore")
            .WithPrefix(ColumnPrefix.BusinessLogic)
            .Build();

        // Act
        var result = PrefixHandler.GetPropertyName(column);

        // Assert
        Assert.Equal("CreditScoreBL", result);
    }

    [Fact]
    public void GetPropertyName_Aggregate_RemovesPrefixAndAddsAggregate()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("agg_OrderCount")
            .WithPrefix(ColumnPrefix.Aggregate)
            .Build();

        // Act
        var result = PrefixHandler.GetPropertyName(column);

        // Assert
        Assert.Equal("OrderCountAggregate", result);
    }

    [Fact]
    public void GetPropertyName_SeparateUpdate_RemovesPrefixAndAddsSeparate()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("spt_Comments")
            .WithPrefix(ColumnPrefix.SeparateUpdate)
            .Build();

        // Act
        var result = PrefixHandler.GetPropertyName(column);

        // Assert
        Assert.Equal("CommentsSeparate", result);
    }

    [Fact]
    public void GetPropertyName_Upload_RemovesPrefixAndAddsUpload()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("upl_Contract")
            .WithPrefix(ColumnPrefix.Upload)
            .Build();

        // Act
        var result = PrefixHandler.GetPropertyName(column);

        // Assert
        Assert.Equal("ContractUpload", result);
    }

    [Fact]
    public void GetPropertyName_SeparateList_RemovesPrefix()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("spl_Applications")
            .WithPrefix(ColumnPrefix.SeparateList)
            .Build();

        // Act
        var result = PrefixHandler.GetPropertyName(column);

        // Assert
        Assert.Equal("Applications", result);
    }

    [Fact]
    public void GetPropertyName_FakeUniqueIndex_RemovesPrefix()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("FUI_InvoiceNo")
            .WithPrefix(ColumnPrefix.FakeUniqueIndex)
            .Build();

        // Act
        var result = PrefixHandler.GetPropertyName(column);

        // Assert
        Assert.Equal("InvoiceNo", result);
    }

    [Fact]
    public void GetPropertyName_NullColumn_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PrefixHandler.GetPropertyName(null!));
    }

    #endregion

    #region GeneratePrefixedProperty Tests

    [Fact]
    public void GeneratePrefixedProperty_NoPrefix_ReturnsNull()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("CustomerName")
            .WithPrefix(ColumnPrefix.None)
            .Build();

        // Act
        var result = PrefixHandler.GeneratePrefixedProperty(column, "string");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GeneratePrefixedProperty_OneWayEncryption_ContainsJsonIgnore()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("eno_Password")
            .WithPrefix(ColumnPrefix.OneWayEncryption)
            .AsVarchar(64)
            .NotNullable()
            .Build();

        // Act
        var result = PrefixHandler.GeneratePrefixedProperty(column, "string");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("[JsonIgnore]", result);
        Assert.Contains("public string PasswordHashed", result);
        Assert.Contains("private set", result);
    }

    [Fact]
    public void GeneratePrefixedProperty_OneWayEncryption_NotNullable_HasRequired()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("eno_Password")
            .WithPrefix(ColumnPrefix.OneWayEncryption)
            .NotNullable()
            .Build();

        // Act
        var result = PrefixHandler.GeneratePrefixedProperty(column, "string");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("[Required]", result);
    }

    [Fact]
    public void GeneratePrefixedProperty_TwoWayEncryption_HasBackingField()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("ent_SSN")
            .WithPrefix(ColumnPrefix.TwoWayEncryption)
            .Build();

        // Act
        var result = PrefixHandler.GeneratePrefixedProperty(column, "string");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("get =>", result);
        Assert.Contains("set =>", result);
        Assert.Contains("DecryptValue", result);
        Assert.Contains("EncryptValue", result);
    }

    [Fact]
    public void GeneratePrefixedProperty_Lookup_HasTextProperty()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("lkp_Status")
            .WithPrefix(ColumnPrefix.Lookup)
            .Build();

        // Act
        var result = PrefixHandler.GeneratePrefixedProperty(column, "string");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public string Status", result);
        Assert.Contains("public string StatusText", result);
        Assert.Contains("[NotMapped]", result);
    }

    [Fact]
    public void GeneratePrefixedProperty_Enumeration_HasEnumProperty()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("enm_Priority")
            .WithPrefix(ColumnPrefix.Enumeration)
            .Build();

        // Act
        var result = PrefixHandler.GeneratePrefixedProperty(column, "string");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public string Priority", result);
        Assert.Contains("public int PriorityEnum", result);
        Assert.Contains("[NotMapped]", result);
    }

    [Fact]
    public void GeneratePrefixedProperty_Localization_HasLocalizedProperty()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("loc_Description")
            .WithPrefix(ColumnPrefix.Localization)
            .Build();

        // Act
        var result = PrefixHandler.GeneratePrefixedProperty(column, "string");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public string Description", result);
        Assert.Contains("public string DescriptionLocalized", result);
        Assert.Contains("[NotMapped]", result);
    }

    [Fact]
    public void GeneratePrefixedProperty_Calculated_IsReadOnly()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("clc_TotalAmount")
            .WithPrefix(ColumnPrefix.Calculated)
            .AsDecimal()
            .Build();

        // Act
        var result = PrefixHandler.GeneratePrefixedProperty(column, "decimal");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public decimal TotalAmountCalculated", result);
        Assert.Contains("internal set", result);
    }

    [Fact]
    public void GeneratePrefixedProperty_BusinessLogic_IsReadOnly()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("blg_CreditScore")
            .WithPrefix(ColumnPrefix.BusinessLogic)
            .AsInt()
            .Build();

        // Act
        var result = PrefixHandler.GeneratePrefixedProperty(column, "int");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public int CreditScoreBL", result);
        Assert.Contains("internal set", result);
    }

    [Fact]
    public void GeneratePrefixedProperty_Aggregate_IsReadOnly()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("agg_OrderCount")
            .WithPrefix(ColumnPrefix.Aggregate)
            .AsInt()
            .Build();

        // Act
        var result = PrefixHandler.GeneratePrefixedProperty(column, "int");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public int OrderCountAggregate", result);
        Assert.Contains("internal set", result);
    }

    [Fact]
    public void GeneratePrefixedProperty_SeparateUpdate_IsPublic()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("spt_Comments")
            .WithPrefix(ColumnPrefix.SeparateUpdate)
            .Build();

        // Act
        var result = PrefixHandler.GeneratePrefixedProperty(column, "string");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public string CommentsSeparate", result);
        Assert.Contains("get; set", result);
    }

    [Fact]
    public void GeneratePrefixedProperty_Upload_IsPublic()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("upl_Contract")
            .WithPrefix(ColumnPrefix.Upload)
            .Build();

        // Act
        var result = PrefixHandler.GeneratePrefixedProperty(column, "string");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public string ContractUpload", result);
    }

    [Fact]
    public void GeneratePrefixedProperty_SeparateList_IsPublic()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("spl_Applications")
            .WithPrefix(ColumnPrefix.SeparateList)
            .Build();

        // Act
        var result = PrefixHandler.GeneratePrefixedProperty(column, "string");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public string Applications", result);
    }

    [Fact]
    public void GeneratePrefixedProperty_NullColumn_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            PrefixHandler.GeneratePrefixedProperty(null!, "string"));
    }

    [Fact]
    public void GeneratePrefixedProperty_NullCSharpType_ThrowsArgumentNullException()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("eno_Password")
            .WithPrefix(ColumnPrefix.OneWayEncryption)
            .Build();

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => 
            PrefixHandler.GeneratePrefixedProperty(column, null!));
        Assert.NotNull(ex);
    }

    [Fact]
    public void GeneratePrefixedProperty_EmptyCSharpType_ThrowsArgumentException()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("eno_Password")
            .WithPrefix(ColumnPrefix.OneWayEncryption)
            .Build();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            PrefixHandler.GeneratePrefixedProperty(column, string.Empty));
        Assert.NotNull(ex);
    }

    [Fact]
    public void GeneratePrefixedProperty_WhiteSpaceCSharpType_ThrowsArgumentException()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("eno_Password")
            .WithPrefix(ColumnPrefix.OneWayEncryption)
            .Build();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            PrefixHandler.GeneratePrefixedProperty(column, "   "));
        Assert.NotNull(ex);
    }

    #endregion

    #region RequiresBackingField Tests

    [Fact]
    public void RequiresBackingField_TwoWayEncryption_ReturnsTrue()
    {
        // Act
        var result = PrefixHandler.RequiresBackingField(ColumnPrefix.TwoWayEncryption);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(ColumnPrefix.None)]
    [InlineData(ColumnPrefix.OneWayEncryption)]
    [InlineData(ColumnPrefix.Lookup)]
    [InlineData(ColumnPrefix.Enumeration)]
    [InlineData(ColumnPrefix.Localization)]
    [InlineData(ColumnPrefix.Calculated)]
    [InlineData(ColumnPrefix.BusinessLogic)]
    [InlineData(ColumnPrefix.Aggregate)]
    [InlineData(ColumnPrefix.SeparateUpdate)]
    [InlineData(ColumnPrefix.Upload)]
    [InlineData(ColumnPrefix.SeparateList)]
    [InlineData(ColumnPrefix.FakeUniqueIndex)]
    public void RequiresBackingField_OtherPrefixes_ReturnsFalse(ColumnPrefix prefix)
    {
        // Act
        var result = PrefixHandler.RequiresBackingField(prefix);

        // Assert
        Assert.False(result);
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
        var result = PrefixHandler.GenerateBackingField(column, "string");

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
            .WithName("eno_Password")
            .WithPrefix(ColumnPrefix.OneWayEncryption)
            .Build();

        // Act
        var result = PrefixHandler.GenerateBackingField(column, "string");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GenerateBackingField_NoPrefix_ReturnsNull()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("CustomerName")
            .WithPrefix(ColumnPrefix.None)
            .Build();

        // Act
        var result = PrefixHandler.GenerateBackingField(column, "string");

        // Assert
        Assert.Null(result);
    }

    #endregion
}
