using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Interfaces.Models;
using TargCC.Core.Interfaces.Models.Metadata;
using TargCC.Core.Interfaces.Services;
using TargCC.Core.Services.Metadata;
using Xunit;

namespace TargCC.Core.Tests.Unit.Services;

/// <summary>
/// Unit tests for IncrementalGenerationService.
/// </summary>
public sealed class IncrementalGenerationServiceTests
{
    private readonly Mock<IMetadataService> _mockMetadataService;
    private readonly Mock<ILogger<ChangeDetectionService>> _mockChangeLogger;
    private readonly Mock<ILogger<IncrementalGenerationService>> _mockLogger;
    private readonly ChangeDetectionService _changeDetectionService;
    private readonly IncrementalGenerationService _service;

    public IncrementalGenerationServiceTests()
    {
        _mockMetadataService = new Mock<IMetadataService>();
        _mockChangeLogger = new Mock<ILogger<ChangeDetectionService>>();
        _mockLogger = new Mock<ILogger<IncrementalGenerationService>>();
        _changeDetectionService = new ChangeDetectionService(_mockChangeLogger.Object);
        _service = new IncrementalGenerationService(_mockMetadataService.Object, _changeDetectionService, _mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var service = new IncrementalGenerationService(
            _mockMetadataService.Object,
            _changeDetectionService,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullMetadataService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new IncrementalGenerationService(null!, _changeDetectionService, _mockLogger.Object));

        Assert.Equal("metadataService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullChangeDetectionService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new IncrementalGenerationService(_mockMetadataService.Object, null!, _mockLogger.Object));

        Assert.Equal("changeDetectionService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new IncrementalGenerationService(_mockMetadataService.Object, _changeDetectionService, null!));

        Assert.Equal("logger", exception.ParamName);
    }

    #endregion

    #region CreateGenerationPlanAsync Tests

    [Fact]
    public async Task CreateGenerationPlanAsync_WithEmptySchema_ReturnsEmptyPlan()
    {
        // Arrange
        var schema = new DatabaseSchema
        {
            Tables = new List<Table>()
        };

        _mockMetadataService.Setup(m => m.GetAllTableMetadataAsync())
            .ReturnsAsync(new List<TableMetadata>());

        // Act
        var plan = await _service.CreateGenerationPlanAsync(schema);

        // Assert
        Assert.NotNull(plan);
        Assert.Equal(0, plan.TotalTables);
        Assert.Equal(0, plan.TablesToGenerate);
        Assert.Equal(0, plan.TablesUnchanged);
        Assert.Empty(plan.TablePlans);
    }

    [Fact]
    public async Task CreateGenerationPlanAsync_WithNewTables_MarksAllForGeneration()
    {
        // Arrange
        var schema = new DatabaseSchema
        {
            Tables = new List<Table>
            {
                CreateTestTable("Customer"),
                CreateTestTable("Order")
            }
        };

        _mockMetadataService.Setup(m => m.GetAllTableMetadataAsync())
            .ReturnsAsync(new List<TableMetadata>());

        // Act
        var plan = await _service.CreateGenerationPlanAsync(schema);

        // Assert
        Assert.Equal(2, plan.TotalTables);
        Assert.Equal(2, plan.TablesToGenerate);
        Assert.Equal(0, plan.TablesUnchanged);
        Assert.All(plan.TablePlans, tp =>
        {
            Assert.True(tp.IsNew);
            Assert.True(tp.RequiresGeneration);
            Assert.Equal("New table", tp.Reason);
        });
    }

    [Fact]
    public async Task CreateGenerationPlanAsync_WithUnchangedTables_SkipsGeneration()
    {
        // Arrange
        var table = CreateTestTable("Customer");
        var tableHash = _changeDetectionService.ComputeTableHash(table);

        var schema = new DatabaseSchema
        {
            Tables = new List<Table> { table }
        };

        var metadata = new TableMetadata
        {
            TableName = "Customer",
            SchemaName = "dbo",
            SchemaHash = tableHash,
            SchemaHashPrevious = tableHash
        };

        _mockMetadataService.Setup(m => m.GetAllTableMetadataAsync())
            .ReturnsAsync(new List<TableMetadata> { metadata });

        // Act
        var plan = await _service.CreateGenerationPlanAsync(schema);

        // Assert
        Assert.Equal(1, plan.TotalTables);
        Assert.Equal(0, plan.TablesToGenerate);
        Assert.Equal(1, plan.TablesUnchanged);

        var tablePlan = plan.TablePlans.First();
        Assert.False(tablePlan.RequiresGeneration);
        Assert.False(tablePlan.IsNew);
        Assert.False(tablePlan.HasSchemaChanged);
        Assert.Equal("No changes detected", tablePlan.Reason);
    }

    [Fact]
    public async Task CreateGenerationPlanAsync_WithModifiedTables_MarksForRegeneration()
    {
        // Arrange
        var table = CreateTestTable("Customer");
        var currentHash = _changeDetectionService.ComputeTableHash(table);
        var previousHash = "different_hash_value";

        var schema = new DatabaseSchema
        {
            Tables = new List<Table> { table }
        };

        var metadata = new TableMetadata
        {
            TableName = "Customer",
            SchemaName = "dbo",
            SchemaHash = previousHash,
            SchemaHashPrevious = previousHash
        };

        _mockMetadataService.Setup(m => m.GetAllTableMetadataAsync())
            .ReturnsAsync(new List<TableMetadata> { metadata });

        // Act
        var plan = await _service.CreateGenerationPlanAsync(schema);

        // Assert
        Assert.Equal(1, plan.TotalTables);
        Assert.Equal(1, plan.TablesToGenerate);
        Assert.Equal(0, plan.TablesUnchanged);

        var tablePlan = plan.TablePlans.First();
        Assert.True(tablePlan.RequiresGeneration);
        Assert.False(tablePlan.IsNew);
        Assert.True(tablePlan.HasSchemaChanged);
        Assert.Equal("Schema changed", tablePlan.Reason);
        Assert.Equal(currentHash, tablePlan.CurrentHash);
        Assert.Equal(previousHash, tablePlan.PreviousHash);
    }

    [Fact]
    public async Task CreateGenerationPlanAsync_WithMixedScenario_IdentifiesCorrectly()
    {
        // Arrange
        var newTable = CreateTestTable("NewTable");
        var unchangedTable = CreateTestTable("UnchangedTable");
        var modifiedTable = CreateTestTable("ModifiedTable");

        var unchangedHash = _changeDetectionService.ComputeTableHash(unchangedTable);

        var schema = new DatabaseSchema
        {
            Tables = new List<Table> { newTable, unchangedTable, modifiedTable }
        };

        var existingMetadata = new List<TableMetadata>
        {
            new() { TableName = "UnchangedTable", SchemaName = "dbo", SchemaHash = unchangedHash, SchemaHashPrevious = unchangedHash },
            new() { TableName = "ModifiedTable", SchemaName = "dbo", SchemaHash = "old_hash", SchemaHashPrevious = "old_hash" }
        };

        _mockMetadataService.Setup(m => m.GetAllTableMetadataAsync())
            .ReturnsAsync(existingMetadata);

        // Act
        var plan = await _service.CreateGenerationPlanAsync(schema);

        // Assert
        Assert.Equal(3, plan.TotalTables);
        Assert.Equal(2, plan.TablesToGenerate); // New + Modified
        Assert.Equal(1, plan.TablesUnchanged);

        var newPlan = plan.TablePlans.First(t => t.TableName == "NewTable");
        Assert.True(newPlan.IsNew);
        Assert.True(newPlan.RequiresGeneration);

        var unchangedPlan = plan.TablePlans.First(t => t.TableName == "UnchangedTable");
        Assert.False(unchangedPlan.RequiresGeneration);

        var modifiedPlan = plan.TablePlans.First(t => t.TableName == "ModifiedTable");
        Assert.True(modifiedPlan.RequiresGeneration);
        Assert.True(modifiedPlan.HasSchemaChanged);
    }

    #endregion

    #region GetChangesSummaryAsync Tests

    [Fact]
    public async Task GetChangesSummaryAsync_WithEmptySchema_ReturnsEmptySummary()
    {
        // Arrange
        var schema = new DatabaseSchema
        {
            Tables = new List<Table>()
        };

        _mockMetadataService.Setup(m => m.GetAllTableMetadataAsync())
            .ReturnsAsync(new List<TableMetadata>());

        // Act
        var summary = await _service.GetChangesSummaryAsync(schema);

        // Assert
        Assert.NotNull(summary);
        Assert.Equal(0, summary.TotalTables);
        Assert.Equal(0, summary.NewTables);
        Assert.Equal(0, summary.ModifiedTables);
        Assert.Equal(0, summary.UnchangedTables);
        Assert.Empty(summary.NewTableNames);
        Assert.Empty(summary.ModifiedTableNames);
    }

    [Fact]
    public async Task GetChangesSummaryAsync_WithNewTables_IdentifiesNewTables()
    {
        // Arrange
        var schema = new DatabaseSchema
        {
            Tables = new List<Table>
            {
                CreateTestTable("Customer"),
                CreateTestTable("Order")
            }
        };

        _mockMetadataService.Setup(m => m.GetAllTableMetadataAsync())
            .ReturnsAsync(new List<TableMetadata>());

        // Act
        var summary = await _service.GetChangesSummaryAsync(schema);

        // Assert
        Assert.Equal(2, summary.TotalTables);
        Assert.Equal(2, summary.NewTables);
        Assert.Equal(0, summary.ModifiedTables);
        Assert.Equal(0, summary.UnchangedTables);
        Assert.Contains("Customer", summary.NewTableNames);
        Assert.Contains("Order", summary.NewTableNames);
    }

    [Fact]
    public async Task GetChangesSummaryAsync_WithModifiedTables_IdentifiesModifiedTables()
    {
        // Arrange
        var table = CreateTestTable("Customer");
        var schema = new DatabaseSchema
        {
            Tables = new List<Table> { table }
        };

        var metadata = new TableMetadata
        {
            TableName = "Customer",
            SchemaName = "dbo",
            SchemaHash = "old_hash",
            SchemaHashPrevious = "old_hash"
        };

        _mockMetadataService.Setup(m => m.GetAllTableMetadataAsync())
            .ReturnsAsync(new List<TableMetadata> { metadata });

        // Act
        var summary = await _service.GetChangesSummaryAsync(schema);

        // Assert
        Assert.Equal(1, summary.TotalTables);
        Assert.Equal(0, summary.NewTables);
        Assert.Equal(1, summary.ModifiedTables);
        Assert.Equal(0, summary.UnchangedTables);
        Assert.Contains("Customer", summary.ModifiedTableNames);
    }

    [Fact]
    public async Task GetChangesSummaryAsync_WithUnchangedTables_IdentifiesUnchangedTables()
    {
        // Arrange
        var table = CreateTestTable("Customer");
        var tableHash = _changeDetectionService.ComputeTableHash(table);

        var schema = new DatabaseSchema
        {
            Tables = new List<Table> { table }
        };

        var metadata = new TableMetadata
        {
            TableName = "Customer",
            SchemaName = "dbo",
            SchemaHash = tableHash,
            SchemaHashPrevious = tableHash
        };

        _mockMetadataService.Setup(m => m.GetAllTableMetadataAsync())
            .ReturnsAsync(new List<TableMetadata> { metadata });

        // Act
        var summary = await _service.GetChangesSummaryAsync(schema);

        // Assert
        Assert.Equal(1, summary.TotalTables);
        Assert.Equal(0, summary.NewTables);
        Assert.Equal(0, summary.ModifiedTables);
        Assert.Equal(1, summary.UnchangedTables);
        Assert.Empty(summary.NewTableNames);
        Assert.Empty(summary.ModifiedTableNames);
    }

    #endregion

    #region Model Tests

    [Fact]
    public void IncrementalGenerationPlan_TablesUnchanged_CalculatesCorrectly()
    {
        // Arrange & Act
        var plan = new IncrementalGenerationPlan
        {
            TotalTables = 10,
            TablesToGenerate = 3
        };

        // Assert
        Assert.Equal(7, plan.TablesUnchanged);
    }

    [Fact]
    public void TableGenerationPlan_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var plan = new TableGenerationPlan();

        // Assert
        Assert.Equal(string.Empty, plan.TableName);
        Assert.Equal("dbo", plan.SchemaName);
        Assert.False(plan.IsNew);
        Assert.False(plan.HasSchemaChanged);
        Assert.False(plan.RequiresGeneration);
        Assert.True(plan.GenerateEntity);
        Assert.True(plan.GenerateRepository);
        Assert.True(plan.GenerateController);
        Assert.False(plan.GenerateReactUI);
        Assert.True(plan.GenerateStoredProcedures);
        Assert.True(plan.GenerateCQRS);
    }

    [Fact]
    public void GenerationExecutionResult_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var result = new GenerationExecutionResult();

        // Assert
        Assert.Equal(0, result.TotalDurationMs);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ChangesSummary_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var summary = new ChangesSummary();

        // Assert
        Assert.Equal(0, summary.TotalTables);
        Assert.Equal(0, summary.NewTables);
        Assert.Equal(0, summary.ModifiedTables);
        Assert.Equal(0, summary.UnchangedTables);
        Assert.Empty(summary.NewTableNames);
        Assert.Empty(summary.ModifiedTableNames);
    }

    #endregion

    #region Helper Methods

    private static Table CreateTestTable(string tableName)
    {
        return new Table
        {
            Name = tableName,
            SchemaName = "dbo",
            Columns = new List<Column>
            {
                new()
                {
                    Name = "ID",
                    DataType = "int",
                    IsNullable = false,
                    IsPrimaryKey = true
                },
                new()
                {
                    Name = "Name",
                    DataType = "nvarchar",
                    MaxLength = 100,
                    IsNullable = false
                }
            },
            Indexes = new List<Index>
            {
                new()
                {
                    Name = $"PK_{tableName}",
                    IsPrimaryKey = true,
                    Columns = new List<IndexColumn>
                    {
                        new() { ColumnName = "ID", IsDescending = false }
                    }
                }
            }
        };
    }

    #endregion
}
