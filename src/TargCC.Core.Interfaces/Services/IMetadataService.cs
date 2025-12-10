using System.Collections.Generic;
using System.Threading.Tasks;
using TargCC.Core.Interfaces.Models.Metadata;

namespace TargCC.Core.Interfaces.Services;

/// <summary>
/// Service for managing database metadata (c_Table, c_Column, etc.)
/// </summary>
public interface IMetadataService
{
    // Table Metadata
    Task<TableMetadata?> GetTableMetadataAsync(string schemaName, string tableName);
    Task<IEnumerable<TableMetadata>> GetAllTableMetadataAsync();
    Task<int> UpsertTableMetadataAsync(TableMetadata metadata);
    Task DeleteTableMetadataAsync(int tableId);

    // Column Metadata
    Task<IEnumerable<ColumnMetadata>> GetColumnMetadataAsync(int tableId);
    Task<int> UpsertColumnMetadataAsync(ColumnMetadata metadata);
    Task DeleteColumnMetadataAsync(int columnId);

    // Index Metadata
    Task<IEnumerable<IndexMetadata>> GetIndexMetadataAsync(int tableId);
    Task<int> UpsertIndexMetadataAsync(IndexMetadata metadata);
    Task DeleteIndexMetadataAsync(int indexId);

    // Relationship Metadata
    Task<IEnumerable<RelationshipMetadata>> GetRelationshipsAsync(int? tableId = null);
    Task<int> UpsertRelationshipMetadataAsync(RelationshipMetadata metadata);
    Task DeleteRelationshipMetadataAsync(int relationshipId);

    // Generation History
    Task<int> LogGenerationAsync(GenerationHistoryMetadata history);
    Task<IEnumerable<GenerationHistoryMetadata>> GetGenerationHistoryAsync(int? tableId = null, int limit = 100);

    // Sync Operations
    Task<bool> SyncMetadataFromDatabaseAsync(string connectionString);
    Task<int> GetChangedTablesCountAsync();
}
