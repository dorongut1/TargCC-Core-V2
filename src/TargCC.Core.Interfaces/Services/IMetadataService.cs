using System.Collections.Generic;
using System.Threading.Tasks;
using TargCC.Core.Interfaces.Models.Metadata;

namespace TargCC.Core.Interfaces.Services;

/// <summary>
/// Service for managing database metadata (c_Table, c_Column, etc.).
/// </summary>
public interface IMetadataService
{
    /// <summary>
    /// Retrieves metadata for a specific table by schema and table name.
    /// </summary>
    /// <param name="schemaName">The schema name of the table.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>The table metadata if found, otherwise null.</returns>
    Task<TableMetadata?> GetTableMetadataAsync(string schemaName, string tableName);

    /// <summary>
    /// Retrieves all table metadata records.
    /// </summary>
    /// <returns>A collection of all table metadata.</returns>
    Task<IEnumerable<TableMetadata>> GetAllTableMetadataAsync();

    /// <summary>
    /// Inserts or updates table metadata.
    /// </summary>
    /// <param name="metadata">The table metadata to upsert.</param>
    /// <returns>The ID of the inserted or updated record.</returns>
    Task<int> UpsertTableMetadataAsync(TableMetadata metadata);

    /// <summary>
    /// Deletes table metadata by ID.
    /// </summary>
    /// <param name="tableId">The ID of the table metadata to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteTableMetadataAsync(int tableId);

    /// <summary>
    /// Retrieves all column metadata for a specific table.
    /// </summary>
    /// <param name="tableId">The ID of the table.</param>
    /// <returns>A collection of column metadata.</returns>
    Task<IEnumerable<ColumnMetadata>> GetColumnMetadataAsync(int tableId);

    /// <summary>
    /// Inserts or updates column metadata.
    /// </summary>
    /// <param name="metadata">The column metadata to upsert.</param>
    /// <returns>The ID of the inserted or updated record.</returns>
    Task<int> UpsertColumnMetadataAsync(ColumnMetadata metadata);

    /// <summary>
    /// Deletes column metadata by ID.
    /// </summary>
    /// <param name="columnId">The ID of the column metadata to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteColumnMetadataAsync(int columnId);

    /// <summary>
    /// Retrieves all index metadata for a specific table.
    /// </summary>
    /// <param name="tableId">The ID of the table.</param>
    /// <returns>A collection of index metadata.</returns>
    Task<IEnumerable<IndexMetadata>> GetIndexMetadataAsync(int tableId);

    /// <summary>
    /// Inserts or updates index metadata.
    /// </summary>
    /// <param name="metadata">The index metadata to upsert.</param>
    /// <returns>The ID of the inserted or updated record.</returns>
    Task<int> UpsertIndexMetadataAsync(IndexMetadata metadata);

    /// <summary>
    /// Deletes index metadata by ID.
    /// </summary>
    /// <param name="indexId">The ID of the index metadata to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteIndexMetadataAsync(int indexId);

    /// <summary>
    /// Retrieves relationship metadata, optionally filtered by table ID.
    /// </summary>
    /// <param name="tableId">Optional table ID to filter relationships.</param>
    /// <returns>A collection of relationship metadata.</returns>
    Task<IEnumerable<RelationshipMetadata>> GetRelationshipsAsync(int? tableId = null);

    /// <summary>
    /// Inserts or updates relationship metadata.
    /// </summary>
    /// <param name="metadata">The relationship metadata to upsert.</param>
    /// <returns>The ID of the inserted or updated record.</returns>
    Task<int> UpsertRelationshipMetadataAsync(RelationshipMetadata metadata);

    /// <summary>
    /// Deletes relationship metadata by ID.
    /// </summary>
    /// <param name="relationshipId">The ID of the relationship metadata to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteRelationshipMetadataAsync(int relationshipId);

    /// <summary>
    /// Logs a code generation event to the history table.
    /// </summary>
    /// <param name="history">The generation history metadata to log.</param>
    /// <returns>The ID of the inserted history record.</returns>
    Task<int> LogGenerationAsync(GenerationHistoryMetadata history);

    /// <summary>
    /// Retrieves generation history, optionally filtered by table ID.
    /// </summary>
    /// <param name="tableId">Optional table ID to filter history.</param>
    /// <param name="limit">Maximum number of records to return (default: 100).</param>
    /// <returns>A collection of generation history metadata.</returns>
    Task<IEnumerable<GenerationHistoryMetadata>> GetGenerationHistoryAsync(int? tableId = null, int limit = 100);

    /// <summary>
    /// Synchronizes metadata from the database schema to the c_* tables.
    /// </summary>
    /// <param name="connectionString">The database connection string.</param>
    /// <returns>True if synchronization was successful, otherwise false.</returns>
    Task<bool> SyncMetadataFromDatabaseAsync(string connectionString);

    /// <summary>
    /// Gets the count of tables that have schema changes (SchemaHash != SchemaHashPrevious).
    /// </summary>
    /// <returns>The number of tables with pending changes.</returns>
    Task<int> GetChangedTablesCountAsync();
}
