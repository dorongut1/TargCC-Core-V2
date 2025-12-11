using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using TargCC.Core.Interfaces.Models.Metadata;
using TargCC.Core.Interfaces.Services;

namespace TargCC.Core.Services.Metadata;

/// <summary>
/// Service for managing database metadata using Dapper
/// </summary>
public class MetadataService : IMetadataService
{
    private readonly string _connectionString;
    private readonly ILogger<MetadataService> _logger;

    public MetadataService(string connectionString, ILogger<MetadataService> logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Table Metadata

    public async Task<TableMetadata?> GetTableMetadataAsync(string schemaName, string tableName)
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = @"
            SELECT * FROM c_Table
            WHERE SchemaName = @SchemaName AND TableName = @TableName";

        return await connection.QueryFirstOrDefaultAsync<TableMetadata>(sql, new { SchemaName = schemaName, TableName = tableName });
    }

    public async Task<IEnumerable<TableMetadata>> GetAllTableMetadataAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = @"SELECT * FROM c_Table WHERE IsActive = 1 ORDER BY TableName";

        return await connection.QueryAsync<TableMetadata>(sql);
    }

    public async Task<int> UpsertTableMetadataAsync(TableMetadata metadata)
    {
        using var connection = new SqlConnection(_connectionString);

        var existing = await GetTableMetadataAsync(metadata.SchemaName, metadata.TableName);

        if (existing == null)
        {
            var sql = @"
                INSERT INTO c_Table (
                    TableName, SchemaName, LastGenerated, LastModifiedInDB, SchemaHash, SchemaHashPrevious,
                    ccAuditLevel, ccUICreateMenu, ccUICreateEntity, ccUICreateCollection, ccIsSingleRow, ccUsedForIdentity,
                    GenerateEntity, GenerateRepository, GenerateController, GenerateReactUI, GenerateStoredProcedures, GenerateCQRS,
                    IsSystemTable, IsActive, Notes, AddedOn, AddedBy
                ) VALUES (
                    @TableName, @SchemaName, @LastGenerated, @LastModifiedInDB, @SchemaHash, @SchemaHashPrevious,
                    @CcAuditLevel, @CcUICreateMenu, @CcUICreateEntity, @CcUICreateCollection, @CcIsSingleRow, @CcUsedForIdentity,
                    @GenerateEntity, @GenerateRepository, @GenerateController, @GenerateReactUI, @GenerateStoredProcedures, @GenerateCQRS,
                    @IsSystemTable, @IsActive, @Notes, @AddedOn, @AddedBy
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.ExecuteScalarAsync<int>(sql, metadata);
        }
        else
        {
            metadata.ID = existing.ID;
            metadata.ChangedOn = DateTime.UtcNow;

            var sql = @"
                UPDATE c_Table SET
                    LastGenerated = @LastGenerated,
                    LastModifiedInDB = @LastModifiedInDB,
                    SchemaHash = @SchemaHash,
                    SchemaHashPrevious = @SchemaHashPrevious,
                    GenerateEntity = @GenerateEntity,
                    GenerateRepository = @GenerateRepository,
                    GenerateController = @GenerateController,
                    GenerateReactUI = @GenerateReactUI,
                    GenerateStoredProcedures = @GenerateStoredProcedures,
                    GenerateCQRS = @GenerateCQRS,
                    IsActive = @IsActive,
                    Notes = @Notes,
                    ChangedOn = @ChangedOn,
                    ChangedBy = @ChangedBy
                WHERE ID = @ID";

            await connection.ExecuteAsync(sql, metadata);
            return metadata.ID;
        }
    }

    public async Task DeleteTableMetadataAsync(int tableId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("DELETE FROM c_Table WHERE ID = @ID", new { ID = tableId });
    }

    #endregion

    #region Column Metadata

    public async Task<IEnumerable<ColumnMetadata>> GetColumnMetadataAsync(int tableId)
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = @"
            SELECT * FROM c_Column
            WHERE TableID = @TableID AND IsActive = 1
            ORDER BY OrdinalPosition";

        return await connection.QueryAsync<ColumnMetadata>(sql, new { TableID = tableId });
    }

    public async Task<int> UpsertColumnMetadataAsync(ColumnMetadata metadata)
    {
        using var connection = new SqlConnection(_connectionString);

        if (metadata.ID == 0)
        {
            var sql = @"
                INSERT INTO c_Column (
                    TableID, ColumnName, DataType, MaxLength, Precision, Scale,
                    IsNullable, IsPrimaryKey, IsIdentity, IsComputed, DefaultValue,
                    IsForeignKey, ReferencedTable, ReferencedColumn, ForeignKeyName,
                    OrdinalPosition, Prefix, PrefixType, UIControlType, ValidationRules,
                    ColumnHash, ColumnHashPrevious, IsActive, Notes, AddedOn, AddedBy
                ) VALUES (
                    @TableID, @ColumnName, @DataType, @MaxLength, @Precision, @Scale,
                    @IsNullable, @IsPrimaryKey, @IsIdentity, @IsComputed, @DefaultValue,
                    @IsForeignKey, @ReferencedTable, @ReferencedColumn, @ForeignKeyName,
                    @OrdinalPosition, @Prefix, @PrefixType, @UIControlType, @ValidationRules,
                    @ColumnHash, @ColumnHashPrevious, @IsActive, @Notes, @AddedOn, @AddedBy
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.ExecuteScalarAsync<int>(sql, metadata);
        }
        else
        {
            metadata.ChangedOn = DateTime.UtcNow;

            var sql = @"
                UPDATE c_Column SET
                    DataType = @DataType,
                    MaxLength = @MaxLength,
                    Precision = @Precision,
                    Scale = @Scale,
                    IsNullable = @IsNullable,
                    DefaultValue = @DefaultValue,
                    ColumnHash = @ColumnHash,
                    ColumnHashPrevious = @ColumnHashPrevious,
                    IsActive = @IsActive,
                    Notes = @Notes,
                    ChangedOn = @ChangedOn,
                    ChangedBy = @ChangedBy
                WHERE ID = @ID";

            await connection.ExecuteAsync(sql, metadata);
            return metadata.ID;
        }
    }

    public async Task DeleteColumnMetadataAsync(int columnId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("DELETE FROM c_Column WHERE ID = @ID", new { ID = columnId });
    }

    #endregion

    #region Index Metadata

    public async Task<IEnumerable<IndexMetadata>> GetIndexMetadataAsync(int tableId)
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = @"
            SELECT * FROM c_Index
            WHERE TableID = @TableID AND IsActive = 1
            ORDER BY IndexName";

        return await connection.QueryAsync<IndexMetadata>(sql, new { TableID = tableId });
    }

    public async Task<int> UpsertIndexMetadataAsync(IndexMetadata metadata)
    {
        using var connection = new SqlConnection(_connectionString);

        if (metadata.ID == 0)
        {
            var sql = @"
                INSERT INTO c_Index (
                    TableID, IndexName, IsUnique, IsClustered, IsPrimaryKey, IndexType,
                    FilterDefinition, IncludedColumns, GenerateGetByMethod, MethodName,
                    IsActive, Notes, AddedOn, AddedBy
                ) VALUES (
                    @TableID, @IndexName, @IsUnique, @IsClustered, @IsPrimaryKey, @IndexType,
                    @FilterDefinition, @IncludedColumns, @GenerateGetByMethod, @MethodName,
                    @IsActive, @Notes, @AddedOn, @AddedBy
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.ExecuteScalarAsync<int>(sql, metadata);
        }
        else
        {
            metadata.ChangedOn = DateTime.UtcNow;

            var sql = @"
                UPDATE c_Index SET
                    GenerateGetByMethod = @GenerateGetByMethod,
                    MethodName = @MethodName,
                    IsActive = @IsActive,
                    Notes = @Notes,
                    ChangedOn = @ChangedOn,
                    ChangedBy = @ChangedBy
                WHERE ID = @ID";

            await connection.ExecuteAsync(sql, metadata);
            return metadata.ID;
        }
    }

    public async Task DeleteIndexMetadataAsync(int indexId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("DELETE FROM c_Index WHERE ID = @ID", new { ID = indexId });
    }

    #endregion

    #region Relationship Metadata

    public async Task<IEnumerable<RelationshipMetadata>> GetRelationshipsAsync(int? tableId = null)
    {
        using var connection = new SqlConnection(_connectionString);

        var sql = tableId.HasValue
            ? @"SELECT * FROM c_Relationship
                WHERE (ParentTableID = @TableID OR ChildTableID = @TableID) AND IsActive = 1"
            : @"SELECT * FROM c_Relationship WHERE IsActive = 1";

        return await connection.QueryAsync<RelationshipMetadata>(sql, new { TableID = tableId });
    }

    public async Task<int> UpsertRelationshipMetadataAsync(RelationshipMetadata metadata)
    {
        using var connection = new SqlConnection(_connectionString);

        if (metadata.ID == 0)
        {
            var sql = @"
                INSERT INTO c_Relationship (
                    ForeignKeyName, ParentTable, ParentColumn, ParentTableID,
                    ChildTable, ChildColumn, ChildTableID, RelationshipType, IsNullable,
                    OnDelete, OnUpdate, ParentNavigationProperty, ChildNavigationProperty,
                    IsMasterDetail, GenerateMasterDetailSP, IsActive, Notes, AddedOn, AddedBy
                ) VALUES (
                    @ForeignKeyName, @ParentTable, @ParentColumn, @ParentTableID,
                    @ChildTable, @ChildColumn, @ChildTableID, @RelationshipType, @IsNullable,
                    @OnDelete, @OnUpdate, @ParentNavigationProperty, @ChildNavigationProperty,
                    @IsMasterDetail, @GenerateMasterDetailSP, @IsActive, @Notes, @AddedOn, @AddedBy
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.ExecuteScalarAsync<int>(sql, metadata);
        }
        else
        {
            metadata.ChangedOn = DateTime.UtcNow;

            var sql = @"
                UPDATE c_Relationship SET
                    IsMasterDetail = @IsMasterDetail,
                    GenerateMasterDetailSP = @GenerateMasterDetailSP,
                    IsActive = @IsActive,
                    Notes = @Notes,
                    ChangedOn = @ChangedOn,
                    ChangedBy = @ChangedBy
                WHERE ID = @ID";

            await connection.ExecuteAsync(sql, metadata);
            return metadata.ID;
        }
    }

    public async Task DeleteRelationshipMetadataAsync(int relationshipId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("DELETE FROM c_Relationship WHERE ID = @ID", new { ID = relationshipId });
    }

    #endregion

    #region Generation History

    public async Task<int> LogGenerationAsync(GenerationHistoryMetadata history)
    {
        using var connection = new SqlConnection(_connectionString);

        var sql = @"
            INSERT INTO c_GenerationHistory (
                TableID, TableName, GenerationType, GeneratedAt, GeneratedBy,
                DurationMs, Success, ErrorMessage, StackTrace,
                SchemaHash, OutputPath, ToolVersion, MetadataJson
            ) VALUES (
                @TableID, @TableName, @GenerationType, @GeneratedAt, @GeneratedBy,
                @DurationMs, @Success, @ErrorMessage, @StackTrace,
                @SchemaHash, @OutputPath, @ToolVersion, @MetadataJson
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await connection.ExecuteScalarAsync<int>(sql, history);
    }

    public async Task<IEnumerable<GenerationHistoryMetadata>> GetGenerationHistoryAsync(int? tableId = null, int limit = 100)
    {
        using var connection = new SqlConnection(_connectionString);

        var sql = tableId.HasValue
            ? @"SELECT TOP (@Limit) * FROM c_GenerationHistory
                WHERE TableID = @TableID
                ORDER BY GeneratedAt DESC"
            : @"SELECT TOP (@Limit) * FROM c_GenerationHistory
                ORDER BY GeneratedAt DESC";

        return await connection.QueryAsync<GenerationHistoryMetadata>(sql, new { TableID = tableId, Limit = limit });
    }

    #endregion

    #region Sync Operations

    public async Task<bool> SyncMetadataFromDatabaseAsync(string connectionString)
    {
        try
        {
            _logger.LogInformation("Starting metadata sync from database");

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Get all tables from database
            var tables = await connection.QueryAsync<dynamic>(@"
                SELECT
                    t.TABLE_SCHEMA AS SchemaName,
                    t.TABLE_NAME AS TableName,
                    OBJECT_ID(t.TABLE_SCHEMA + '.' + t.TABLE_NAME) AS ObjectID
                FROM INFORMATION_SCHEMA.TABLES t
                WHERE t.TABLE_TYPE = 'BASE TABLE'
                  AND t.TABLE_NAME NOT LIKE 'c_%'
                ORDER BY t.TABLE_NAME");

            _logger.LogInformation("Found {Count} tables to sync", tables.Count());

            // Sync each table
            foreach (var table in tables)
            {
                await SyncTableMetadataAsync(connectionString, table.SchemaName, table.TableName);
            }

            _logger.LogInformation("Metadata sync completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync metadata from database");
            return false;
        }
    }

    private async Task SyncTableMetadataAsync(string connectionString, string schemaName, string tableName)
    {
        using var connection = new SqlConnection(connectionString);

        // Get column information
        var columns = await connection.QueryAsync<dynamic>(@"
            SELECT
                c.COLUMN_NAME AS ColumnName,
                c.DATA_TYPE AS DataType,
                c.CHARACTER_MAXIMUM_LENGTH AS MaxLength,
                c.NUMERIC_PRECISION AS Precision,
                c.NUMERIC_SCALE AS Scale,
                c.IS_NULLABLE AS IsNullable,
                c.COLUMN_DEFAULT AS DefaultValue,
                c.ORDINAL_POSITION AS OrdinalPosition,
                COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') AS IsIdentity,
                COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsComputed') AS IsComputed
            FROM INFORMATION_SCHEMA.COLUMNS c
            WHERE c.TABLE_SCHEMA = @SchemaName
              AND c.TABLE_NAME = @TableName
            ORDER BY c.ORDINAL_POSITION",
            new { SchemaName = schemaName, TableName = tableName });

        // Compute schema hash
        var schemaHashService = new ChangeDetectionService(_logger);
        var schemaHash = schemaHashService.ComputeSchemaHash(schemaName, tableName, columns);

        // Upsert table metadata
        var tableMetadata = new TableMetadata
        {
            TableName = tableName,
            SchemaName = schemaName,
            LastModifiedInDB = DateTime.UtcNow,
            SchemaHash = schemaHash
        };

        var tableId = await UpsertTableMetadataAsync(tableMetadata);

        _logger.LogDebug("Synced metadata for table {SchemaName}.{TableName} (ID: {TableId})", schemaName, tableName, tableId);
    }

    public async Task<int> GetChangedTablesCountAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        var sql = @"
            SELECT COUNT(*)
            FROM c_Table
            WHERE SchemaHash != SchemaHashPrevious
              OR SchemaHashPrevious IS NULL";

        return await connection.ExecuteScalarAsync<int>(sql);
    }

    #endregion
}
