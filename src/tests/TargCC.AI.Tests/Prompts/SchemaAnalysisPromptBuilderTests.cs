// <copyright file="SchemaAnalysisPromptBuilderTests.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

using FluentAssertions;
using TargCC.AI.Prompts;
using TargCC.Core.Interfaces.Models;
using Xunit;
using TableIndex = TargCC.Core.Interfaces.Models.Index;

namespace TargCC.AI.Tests.Prompts;

/// <summary>
/// Unit tests for SchemaAnalysisPromptBuilder.
/// </summary>
public class SchemaAnalysisPromptBuilderTests
{
    [Fact]
    public void Constructor_WithValidTable_ShouldCreateInstance()
    {
        // Arrange
        var table = CreateSampleTable();

        // Act
        var builder = new SchemaAnalysisPromptBuilder(table);

        // Assert
        builder.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullTable_ShouldThrowArgumentNullException()
    {
        // Arrange
        Table? table = null;

        // Act
        var act = () => new SchemaAnalysisPromptBuilder(table!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("table");
    }

    [Fact]
    public void Build_WithValidTable_ShouldIncludeTableName()
    {
        // Arrange
        var table = CreateSampleTable();
        var builder = new SchemaAnalysisPromptBuilder(table);

        // Act
        var prompt = builder.Build();

        // Assert
        prompt.Should().Contain("Customer");
        prompt.Should().Contain("dbo");
    }

    [Fact]
    public void Build_WithValidTable_ShouldIncludeAllColumns()
    {
        // Arrange
        var table = CreateSampleTable();
        var builder = new SchemaAnalysisPromptBuilder(table);

        // Act
        var prompt = builder.Build();

        // Assert
        prompt.Should().Contain("ID");
        prompt.Should().Contain("Name");
        prompt.Should().Contain("Email");
        prompt.Should().Contain("int");
        prompt.Should().Contain("nvarchar");
    }

    [Fact]
    public void Build_WithIndexes_ShouldIncludeIndexInformation()
    {
        // Arrange
        var table = CreateSampleTable();
        var builder = new SchemaAnalysisPromptBuilder(table);

        // Act
        var prompt = builder.Build();

        // Assert
        prompt.Should().Contain("**Indexes:**");
        prompt.Should().Contain("PK_Customer");
        prompt.Should().Contain("Unique");
    }

    [Fact]
    public void Build_WithForeignKey_ShouldIncludeReferencedTable()
    {
        // Arrange
        var table = new Table
        {
            Name = "Order",
            SchemaName = "dbo",
            Columns = new List<Column>
            {
                new()
                {
                    Name = "CustomerID",
                    DataType = "int",
                    IsForeignKey = true,
                    ReferencedTable = "Customer",
                },
            },
        };
        var builder = new SchemaAnalysisPromptBuilder(table);

        // Act
        var prompt = builder.Build();

        // Assert
        prompt.Should().Contain("Foreign Key: True");
        prompt.Should().Contain("References: Customer");
    }

    [Fact]
    public void Build_ShouldRequestJSONFormat()
    {
        // Arrange
        var table = CreateSampleTable();
        var builder = new SchemaAnalysisPromptBuilder(table);

        // Act
        var prompt = builder.Build();

        // Assert
        prompt.Should().Contain("Please provide analysis in the following JSON format:");
        prompt.Should().Contain("```json");
        prompt.Should().Contain("tableName");
        prompt.Should().Contain("summary");
        prompt.Should().Contain("qualityScore");
        prompt.Should().Contain("suggestions");
    }

    [Fact]
    public void GetSystemMessage_ShouldIncludeTargCCConventions()
    {
        // Arrange
        var table = CreateSampleTable();
        var builder = new SchemaAnalysisPromptBuilder(table);

        // Act
        var systemMessage = builder.GetSystemMessage();

        // Assert
        systemMessage.Should().Contain("eno_");
        systemMessage.Should().Contain("ent_");
        systemMessage.Should().Contain("clc_");
        systemMessage.Should().Contain("blg_");
        systemMessage.Should().Contain("agg_");
        systemMessage.Should().Contain("spt_");
    }

    [Fact]
    public void GetSystemMessage_ShouldIncludeBestPractices()
    {
        // Arrange
        var table = CreateSampleTable();
        var builder = new SchemaAnalysisPromptBuilder(table);

        // Act
        var systemMessage = builder.GetSystemMessage();

        // Assert
        systemMessage.Should().Contain("Best Practices to Check:");
        systemMessage.Should().Contain("Table names should be singular");
        systemMessage.Should().Contain("Primary keys should exist");
        systemMessage.Should().Contain("Foreign key relationships");
    }

    [Fact]
    public void GetSystemMessage_ShouldDescribeRole()
    {
        // Arrange
        var table = CreateSampleTable();
        var builder = new SchemaAnalysisPromptBuilder(table);

        // Act
        var systemMessage = builder.GetSystemMessage();

        // Assert
        systemMessage.Should().Contain("expert database schema analyzer");
        systemMessage.Should().Contain("TargCC");
        systemMessage.Should().Contain("code generation system");
    }

    private static Table CreateSampleTable()
    {
        return new Table
        {
            Name = "Customer",
            SchemaName = "dbo",
            Columns = new List<Column>
            {
                new()
                {
                    Name = "ID",
                    DataType = "int",
                    IsNullable = false,
                    IsPrimaryKey = true,
                    MaxLength = null,
                },
                new()
                {
                    Name = "Name",
                    DataType = "nvarchar",
                    IsNullable = false,
                    MaxLength = 100,
                },
                new()
                {
                    Name = "Email",
                    DataType = "nvarchar",
                    IsNullable = true,
                    MaxLength = 255,
                },
            },
            Indexes = new List<TableIndex>
            {
                new()
                {
                    Name = "PK_Customer",
                    IsUnique = true,
                    IsPrimaryKey = true,
                    ColumnNames = new List<string> { "ID" },
                },
            },
        };
    }
}
