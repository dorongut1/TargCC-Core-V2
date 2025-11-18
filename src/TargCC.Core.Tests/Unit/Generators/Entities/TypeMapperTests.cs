// <copyright file="TypeMapperTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Tests.Unit.Generators.Entities;

using TargCC.Core.Generators.Entities;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Tests.TestHelpers;
using Xunit;

/// <summary>
/// Unit tests for TypeMapper class.
/// Tests SQL Server data type to C# type mapping functionality.
/// </summary>
public class TypeMapperTests
{
    #region MapSqlTypeToCSharp - Integer Types

    [Fact]
    public void MapSqlTypeToCSharp_Int_NotNullable_ReturnsInt()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("INT")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("int", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_Int_Nullable_ReturnsNullableInt()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("INT")
            .Nullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("int?", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_BigInt_NotNullable_ReturnsLong()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("BIGINT")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("long", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_BigInt_Nullable_ReturnsNullableLong()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("BIGINT")
            .Nullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("long?", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_SmallInt_ReturnsShort()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("SMALLINT")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("short", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_TinyInt_ReturnsByte()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("TINYINT")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("byte", result);
    }

    #endregion

    #region MapSqlTypeToCSharp - Decimal Types

    [Fact]
    public void MapSqlTypeToCSharp_Decimal_ReturnsDecimal()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("DECIMAL")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("decimal", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_Numeric_ReturnsDecimal()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("NUMERIC")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("decimal", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_Money_ReturnsDecimal()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("MONEY")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("decimal", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_Float_ReturnsDouble()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("FLOAT")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("double", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_Real_ReturnsFloat()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("REAL")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("float", result);
    }

    #endregion

    #region MapSqlTypeToCSharp - String Types

    [Fact]
    public void MapSqlTypeToCSharp_Varchar_ReturnsString()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("VARCHAR")
            .WithMaxLength(100)
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("string", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_Nvarchar_ReturnsString()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("NVARCHAR")
            .WithMaxLength(200)
            .Nullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("string", result); // Strings are always nullable
    }

    [Fact]
    public void MapSqlTypeToCSharp_Char_ReturnsString()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("CHAR")
            .WithMaxLength(10)
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("string", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_Text_ReturnsString()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("TEXT")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("string", result);
    }

    #endregion

    #region MapSqlTypeToCSharp - Date/Time Types

    [Fact]
    public void MapSqlTypeToCSharp_DateTime_ReturnsDateTime()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("DATETIME")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("DateTime", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_DateTime_Nullable_ReturnsNullableDateTime()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("DATETIME")
            .Nullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("DateTime?", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_DateTime2_ReturnsDateTime()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("DATETIME2")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("DateTime", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_Date_ReturnsDateTime()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("DATE")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("DateTime", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_Time_ReturnsTimeSpan()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("TIME")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("TimeSpan", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_DateTimeOffset_ReturnsDateTimeOffset()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("DATETIMEOFFSET")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("DateTimeOffset", result);
    }

    #endregion

    #region MapSqlTypeToCSharp - Other Types

    [Fact]
    public void MapSqlTypeToCSharp_Bit_ReturnsBool()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("BIT")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("bool", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_UniqueIdentifier_ReturnsGuid()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("UNIQUEIDENTIFIER")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("Guid", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_Binary_ReturnsByteArray()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("BINARY")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("byte[]", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_Varbinary_ReturnsByteArray()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("VARBINARY")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("byte[]", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_Xml_ReturnsString()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("XML")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("string", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_UnknownType_ReturnsObject()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("UNKNOWNTYPE")
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("object", result);
    }

    #endregion

    #region GetDefaultValue Tests

    [Fact]
    public void GetDefaultValue_Int_ReturnsZero()
    {
        // Act
        var result = TypeMapper.GetDefaultValue("int");

        // Assert
        Assert.Equal("0", result);
    }

    [Fact]
    public void GetDefaultValue_Long_ReturnsZeroL()
    {
        // Act
        var result = TypeMapper.GetDefaultValue("long");

        // Assert
        Assert.Equal("0L", result);
    }

    [Fact]
    public void GetDefaultValue_Decimal_ReturnsZeroM()
    {
        // Act
        var result = TypeMapper.GetDefaultValue("decimal");

        // Assert
        Assert.Equal("0m", result);
    }

    [Fact]
    public void GetDefaultValue_Double_ReturnsZeroPointZero()
    {
        // Act
        var result = TypeMapper.GetDefaultValue("double");

        // Assert
        Assert.Equal("0.0", result);
    }

    [Fact]
    public void GetDefaultValue_Bool_ReturnsFalse()
    {
        // Act
        var result = TypeMapper.GetDefaultValue("bool");

        // Assert
        Assert.Equal("false", result);
    }

    [Fact]
    public void GetDefaultValue_DateTime_ReturnsDateTimeNow()
    {
        // Act
        var result = TypeMapper.GetDefaultValue("DateTime");

        // Assert
        Assert.Equal("DateTime.Now", result);
    }

    [Fact]
    public void GetDefaultValue_Guid_ReturnsGuidEmpty()
    {
        // Act
        var result = TypeMapper.GetDefaultValue("Guid");

        // Assert
        Assert.Equal("Guid.Empty", result);
    }

    [Fact]
    public void GetDefaultValue_String_ReturnsNull()
    {
        // Act
        var result = TypeMapper.GetDefaultValue("string");

        // Assert
        Assert.Equal("null", result);
    }

    [Fact]
    public void GetDefaultValue_NullableInt_ReturnsNull()
    {
        // Act
        var result = TypeMapper.GetDefaultValue("int?");

        // Assert
        Assert.Equal("null", result);
    }

    [Fact]
    public void GetDefaultValue_ByteArray_ReturnsNull()
    {
        // Act
        var result = TypeMapper.GetDefaultValue("byte[]");

        // Assert
        Assert.Equal("null", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void MapSqlTypeToCSharp_CaseInsensitive_WorksCorrectly()
    {
        // Arrange
        var column = new ColumnBuilder()
            .WithName("TestColumn")
            .WithSqlType("varchar") // lowercase
            .NotNullable()
            .Build();

        // Act
        var result = TypeMapper.MapSqlTypeToCSharp(column);

        // Assert
        Assert.Equal("string", result);
    }

    [Fact]
    public void MapSqlTypeToCSharp_NullColumn_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => TypeMapper.MapSqlTypeToCSharp(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetDefaultValue_NullOrWhiteSpace_ThrowsArgumentException(string? csharpType)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => TypeMapper.GetDefaultValue(csharpType!));
    }

    #endregion
}
