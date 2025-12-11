using Microsoft.Extensions.Logging;
using Moq;
using TargCC.Core.Interfaces.Models.Metadata;
using TargCC.Core.Services.Metadata;
using Xunit;

namespace TargCC.Core.Tests.Unit.Services;

/// <summary>
/// Unit tests for MetadataService.
/// Note: These are primarily validation and behavior tests.
/// Integration tests with actual database should be in Integration folder.
/// </summary>
public sealed class MetadataServiceTests
{
    private readonly Mock<ILogger<MetadataService>> _mockLogger;
    private const string TestConnectionString = "Server=test;Database=test;";

    public MetadataServiceTests()
    {
        _mockLogger = new Mock<ILogger<MetadataService>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var service = new MetadataService(TestConnectionString, _mockLogger.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullConnectionString_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => new MetadataService(null!, _mockLogger.Object));

        Assert.Equal("connectionString", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => new MetadataService(TestConnectionString, null!));

        Assert.Equal("logger", exception.ParamName);
    }

    #endregion

    #region TableMetadata Tests

    [Fact]
    public void TableMetadata_Properties_AreInitialized()
    {
        // Arrange & Act
        var metadata = new TableMetadata
        {
            ID = 1,
            TableName = "Customer",
            SchemaName = "dbo",
            SchemaHash = "abc123",
            SchemaHashPrevious = "xyz789",
            LastGenerated = DateTime.UtcNow,
            IsActive = true
        };

        // Assert
        Assert.Equal(1, metadata.ID);
        Assert.Equal("Customer", metadata.TableName);
        Assert.Equal("dbo", metadata.SchemaName);
        Assert.Equal("abc123", metadata.SchemaHash);
        Assert.Equal("xyz789", metadata.SchemaHashPrevious);
        Assert.True(metadata.IsActive);
        Assert.NotNull(metadata.LastGenerated);
    }

    [Fact]
    public void TableMetadata_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var metadata = new TableMetadata();

        // Assert
        Assert.Equal(0, metadata.ID);
        Assert.Null(metadata.TableName);
        Assert.Null(metadata.SchemaName);
        Assert.Null(metadata.SchemaHash);
        Assert.Null(metadata.SchemaHashPrevious);
        Assert.Null(metadata.LastGenerated);
    }

    [Fact]
    public void TableMetadata_GenerationFlags_CanBeSet()
    {
        // Arrange & Act
        var metadata = new TableMetadata
        {
            GenerateEntity = true,
            GenerateRepository = true,
            GenerateController = false,
            GenerateReactUI = true,
            GenerateStoredProcedures = true,
            GenerateCQRS = false
        };

        // Assert
        Assert.True(metadata.GenerateEntity);
        Assert.True(metadata.GenerateRepository);
        Assert.False(metadata.GenerateController);
        Assert.True(metadata.GenerateReactUI);
        Assert.True(metadata.GenerateStoredProcedures);
        Assert.False(metadata.GenerateCQRS);
    }

    [Fact]
    public void TableMetadata_LegacyFields_CanBeSet()
    {
        // Arrange & Act
        var metadata = new TableMetadata
        {
            CcAuditLevel = 2,
            CcUICreateMenu = true,
            CcUICreateEntity = true,
            CcUICreateCollection = false,
            CcIsSingleRow = false,
            CcUsedForIdentity = true
        };

        // Assert
        Assert.Equal(2, metadata.CcAuditLevel);
        Assert.True(metadata.CcUICreateMenu);
        Assert.True(metadata.CcUICreateEntity);
        Assert.False(metadata.CcUICreateCollection);
        Assert.False(metadata.CcIsSingleRow);
        Assert.True(metadata.CcUsedForIdentity);
    }

    #endregion

    #region ColumnMetadata Tests

    [Fact]
    public void ColumnMetadata_Properties_AreInitialized()
    {
        // Arrange & Act
        var metadata = new ColumnMetadata
        {
            ID = 1,
            TableID = 10,
            ColumnName = "CustomerName",
            DataType = "nvarchar",
            MaxLength = 100,
            IsNullable = false,
            IsPrimaryKey = false,
            IsForeignKey = true,
            OrdinalPosition = 2
        };

        // Assert
        Assert.Equal(1, metadata.ID);
        Assert.Equal(10, metadata.TableID);
        Assert.Equal("CustomerName", metadata.ColumnName);
        Assert.Equal("nvarchar", metadata.DataType);
        Assert.Equal(100, metadata.MaxLength);
        Assert.False(metadata.IsNullable);
        Assert.False(metadata.IsPrimaryKey);
        Assert.True(metadata.IsForeignKey);
        Assert.Equal(2, metadata.OrdinalPosition);
    }

    [Fact]
    public void ColumnMetadata_PrefixFields_CanBeSet()
    {
        // Arrange & Act
        var metadata = new ColumnMetadata
        {
            Prefix = "eno_",
            PrefixType = "Encrypted",
            UIControlType = "PasswordBox",
            ValidationRules = "Required"
        };

        // Assert
        Assert.Equal("eno_", metadata.Prefix);
        Assert.Equal("Encrypted", metadata.PrefixType);
        Assert.Equal("PasswordBox", metadata.UIControlType);
        Assert.Equal("Required", metadata.ValidationRules);
    }

    #endregion

    #region IndexMetadata Tests

    [Fact]
    public void IndexMetadata_Properties_AreInitialized()
    {
        // Arrange & Act
        var metadata = new IndexMetadata
        {
            ID = 1,
            TableID = 10,
            IndexName = "IX_Customer_Email",
            IsUnique = true,
            IsPrimaryKey = false,
            IsClustered = false,
            Columns = new List<IndexColumnMetadata>
            {
                new() { ColumnName = "Email", KeyOrdinal = 1 }
            }
        };

        // Assert
        Assert.Equal(1, metadata.ID);
        Assert.Equal(10, metadata.TableID);
        Assert.Equal("IX_Customer_Email", metadata.IndexName);
        Assert.True(metadata.IsUnique);
        Assert.False(metadata.IsPrimaryKey);
        Assert.False(metadata.IsClustered);
        Assert.Single(metadata.Columns);
        Assert.Equal("Email", metadata.Columns[0].ColumnName);
    }

    [Fact]
    public void IndexMetadata_ColumnsList_CanContainMultipleColumns()
    {
        // Arrange & Act
        var metadata = new IndexMetadata
        {
            Columns = new List<IndexColumnMetadata>
            {
                new() { ColumnName = "LastName", KeyOrdinal = 1 },
                new() { ColumnName = "FirstName", KeyOrdinal = 2 },
                new() { ColumnName = "MiddleName", KeyOrdinal = 3 }
            }
        };

        // Assert
        Assert.Equal(3, metadata.Columns.Count);
        Assert.Equal("LastName", metadata.Columns[0].ColumnName);
        Assert.Equal("FirstName", metadata.Columns[1].ColumnName);
        Assert.Equal("MiddleName", metadata.Columns[2].ColumnName);
    }

    #endregion

    #region RelationshipMetadata Tests

    [Fact]
    public void RelationshipMetadata_Properties_AreInitialized()
    {
        // Arrange & Act
        var metadata = new RelationshipMetadata
        {
            ID = 1,
            ParentTableID = 5,
            ChildTableID = 10,
            ParentTable = "Customer",
            ChildTable = "Order",
            ParentColumn = "CustomerID",
            ChildColumn = "CustomerID",
            ForeignKeyName = "FK_Order_Customer"
        };

        // Assert
        Assert.Equal(1, metadata.ID);
        Assert.Equal(5, metadata.ParentTableID);
        Assert.Equal(10, metadata.ChildTableID);
        Assert.Equal("Customer", metadata.ParentTable);
        Assert.Equal("Order", metadata.ChildTable);
        Assert.Equal("CustomerID", metadata.ParentColumn);
        Assert.Equal("CustomerID", metadata.ChildColumn);
        Assert.Equal("FK_Order_Customer", metadata.ForeignKeyName);
    }

    #endregion

    #region GenerationHistoryMetadata Tests

    [Fact]
    public void GenerationHistoryMetadata_Properties_AreInitialized()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;

        // Act
        var metadata = new GenerationHistoryMetadata
        {
            ID = 1,
            TableID = 10,
            TableName = "Customer",
            GeneratedAt = timestamp,
            GeneratedBy = "admin",
            GenerationType = "Full",
            DurationMs = 1500,
            Success = true,
            ErrorMessage = null,
            SchemaHash = "abc123",
            ToolVersion = "2.0.0"
        };

        // Assert
        Assert.Equal(1, metadata.ID);
        Assert.Equal(10, metadata.TableID);
        Assert.Equal("Customer", metadata.TableName);
        Assert.Equal(timestamp, metadata.GeneratedAt);
        Assert.Equal("admin", metadata.GeneratedBy);
        Assert.Equal("Full", metadata.GenerationType);
        Assert.Equal(1500, metadata.DurationMs);
        Assert.True(metadata.Success);
        Assert.Null(metadata.ErrorMessage);
        Assert.Equal("abc123", metadata.SchemaHash);
        Assert.Equal("2.0.0", metadata.ToolVersion);
    }

    [Fact]
    public void GenerationHistoryMetadata_FailureScenario_IsHandled()
    {
        // Arrange & Act
        var metadata = new GenerationHistoryMetadata
        {
            TableName = "Customer",
            Success = false,
            ErrorMessage = "Database connection failed",
            DurationMs = 500,
            StackTrace = "at System..."
        };

        // Assert
        Assert.False(metadata.Success);
        Assert.NotNull(metadata.ErrorMessage);
        Assert.Equal("Database connection failed", metadata.ErrorMessage);
        Assert.Equal(500, metadata.DurationMs);
        Assert.NotNull(metadata.StackTrace);
    }

    #endregion

    #region Audit Fields Tests

    [Fact]
    public void TableMetadata_AuditFields_CanBeSet()
    {
        // Arrange
        var addedOn = DateTime.UtcNow.AddDays(-10);
        var changedOn = DateTime.UtcNow;

        // Act
        var metadata = new TableMetadata
        {
            AddedOn = addedOn,
            AddedBy = "system",
            ChangedOn = changedOn,
            ChangedBy = "admin"
        };

        // Assert
        Assert.Equal(addedOn, metadata.AddedOn);
        Assert.Equal("system", metadata.AddedBy);
        Assert.Equal(changedOn, metadata.ChangedOn);
        Assert.Equal("admin", metadata.ChangedBy);
    }

    [Fact]
    public void ColumnMetadata_AuditFields_CanBeSet()
    {
        // Arrange
        var addedOn = DateTime.UtcNow;

        // Act
        var metadata = new ColumnMetadata
        {
            AddedOn = addedOn,
            AddedBy = "system"
        };

        // Assert
        Assert.Equal(addedOn, metadata.AddedOn);
        Assert.Equal("system", metadata.AddedBy);
    }

    #endregion

    #region Schema Hash Tests

    [Fact]
    public void TableMetadata_SchemaHashComparison_DetectsChanges()
    {
        // Arrange
        var metadata = new TableMetadata
        {
            SchemaHash = "current_hash_123",
            SchemaHashPrevious = "previous_hash_456"
        };

        // Act
        var hasChanged = metadata.SchemaHash != metadata.SchemaHashPrevious;

        // Assert
        Assert.True(hasChanged);
    }

    [Fact]
    public void TableMetadata_SchemaHashComparison_DetectsNoChanges()
    {
        // Arrange
        var sameHash = "same_hash_789";
        var metadata = new TableMetadata
        {
            SchemaHash = sameHash,
            SchemaHashPrevious = sameHash
        };

        // Act
        var hasChanged = metadata.SchemaHash != metadata.SchemaHashPrevious;

        // Assert
        Assert.False(hasChanged);
    }

    [Fact]
    public void TableMetadata_NullPreviousHash_IndicatesNewTable()
    {
        // Arrange
        var metadata = new TableMetadata
        {
            SchemaHash = "new_hash_abc",
            SchemaHashPrevious = null
        };

        // Act
        var isNew = metadata.SchemaHashPrevious == null;

        // Assert
        Assert.True(isNew);
        Assert.NotNull(metadata.SchemaHash);
    }

    #endregion
}
