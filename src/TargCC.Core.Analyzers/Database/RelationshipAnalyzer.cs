using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Analyzers.Database
{
    /// <summary>
    /// Analyzes relationships between tables through foreign key constraints.
    /// </summary>
    public class RelationshipAnalyzer
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipAnalyzer"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string to the database.</param>
        /// <param name="logger">Logger for tracking operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when connectionString or logger is null.</exception>
        public RelationshipAnalyzer(string connectionString, ILogger logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Adds an edge to the relationship graph.
        /// </summary>
        /// <param name="graph">Graph dictionary.</param>
        /// <param name="fromTable">Source table.</param>
        /// <param name="toTable">Destination table.</param>
        private static void AddToGraph(Dictionary<string, List<string>> graph, string fromTable, string toTable)
        {
            if (!graph.ContainsKey(fromTable))
            {
                graph[fromTable] = new List<string>();
            }

            graph[fromTable].Add(toTable);
        }

        /// <summary>
        /// Ensures a node exists in the graph (for tables with no outgoing relationships).
        /// </summary>
        /// <param name="graph">Graph dictionary.</param>
        /// <param name="tableName">Table name.</param>
        private static void EnsureNodeExists(Dictionary<string, List<string>> graph, string tableName)
        {
            if (!graph.ContainsKey(tableName))
            {
                graph[tableName] = new List<string>();
            }
        }

        /// <summary>
        /// Analyzes all relationships between tables.
        /// </summary>
        /// <param name="tables">List of tables to analyze relationships for.</param>
        /// <returns>List of relationships found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when tables is null.</exception>
        /// <exception cref="SqlException">Thrown when database operation fails.</exception>
        public async Task<List<Relationship>> AnalyzeRelationshipsAsync(List<Table> tables)
        {
            if (tables == null)
            {
                throw new ArgumentNullException(nameof(tables));
            }

            try
            {
                _logger.LogDebug("Starting relationship analysis for {TableCount} tables", tables.Count);

                var relationshipData = await FetchAllRelationshipsAsync();
                var relationships = ProcessRelationships(relationshipData, tables);

                LogAnalysisComplete(relationships.Count);
                return relationships;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error analyzing relationships");
                throw new InvalidOperationException("Failed to analyze relationships", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error analyzing relationships");
                throw;
            }
        }

        /// <summary>
        /// Analyzes relationships only for specific tables (incremental analysis).
        /// </summary>
        /// <param name="tableNames">List of table names to analyze.</param>
        /// <returns>List of relationships involving specified tables.</returns>
        /// <exception cref="ArgumentNullException">Thrown when tableNames is null.</exception>
        /// <exception cref="SqlException">Thrown when database operation fails.</exception>
        public async Task<List<Relationship>> AnalyzeRelationshipsForTablesAsync(List<string> tableNames)
        {
            if (tableNames == null)
            {
                throw new ArgumentNullException(nameof(tableNames));
            }

            if (!tableNames.Any())
            {
                _logger.LogDebug("No tables specified for relationship analysis, returning empty list");
                return new List<Relationship>();
            }

            try
            {
                _logger.LogDebug("Analyzing relationships for {TableCount} specific tables", tableNames.Count);

                var relationshipData = await FetchRelationshipsForTablesAsync(tableNames);
                var relationships = ProcessRelationships(relationshipData, null);

                LogAnalysisComplete(relationships.Count);
                return relationships;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error analyzing relationships for specific tables");
                throw new InvalidOperationException("Failed to analyze relationships for specified tables", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error analyzing relationships for specific tables");
                throw;
            }
        }

        /// <summary>
        /// Builds a relationship graph for advanced analysis.
        /// </summary>
        /// <param name="relationships">List of relationships.</param>
        /// <returns>Dictionary mapping table names to their related tables.</returns>
        /// <exception cref="ArgumentNullException">Thrown when relationships is null.</exception>
        public Dictionary<string, List<string>> BuildRelationshipGraph(List<Relationship> relationships)
        {
            if (relationships == null)
            {
                throw new ArgumentNullException(nameof(relationships));
            }

            var graph = new Dictionary<string, List<string>>();

            foreach (var rel in relationships)
            {
                AddToGraph(graph, rel.ParentTable, rel.ReferencedTable);
                EnsureNodeExists(graph, rel.ReferencedTable);
            }

            _logger.LogTrace("Built relationship graph with {NodeCount} nodes", graph.Count);
            return graph;
        }

        /// <summary>
        /// Gets parent tables (tables referenced by foreign keys) for a given table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="relationships">List of relationships.</param>
        /// <returns>List of parent table names.</returns>
        /// <exception cref="ArgumentNullException">Thrown when tableName or relationships is null.</exception>
        public List<string> GetParentTables(string tableName, List<Relationship> relationships)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (relationships == null)
            {
                throw new ArgumentNullException(nameof(relationships));
            }

            var parents = relationships
                .Where(r => r.ParentTable == tableName)
                .Select(r => r.ReferencedTable)
                .Distinct()
                .ToList();

            _logger.LogTrace("Found {ParentCount} parent tables for {TableName}", parents.Count, tableName);
            return parents;
        }

        /// <summary>
        /// Gets child tables (tables that reference this table) for a given table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="relationships">List of relationships.</param>
        /// <returns>List of child table names.</returns>
        /// <exception cref="ArgumentNullException">Thrown when tableName or relationships is null.</exception>
        public List<string> GetChildTables(string tableName, List<Relationship> relationships)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (relationships == null)
            {
                throw new ArgumentNullException(nameof(relationships));
            }

            var children = relationships
                .Where(r => r.ReferencedTable == tableName)
                .Select(r => r.ParentTable)
                .Distinct()
                .ToList();

            _logger.LogTrace("Found {ChildCount} child tables for {TableName}", children.Count, tableName);
            return children;
        }

        #region Private Helper Methods

        /// <summary>
        /// Fetches all relationships from the database.
        /// </summary>
        /// <returns>Dynamic result set from database.</returns>
        private async Task<IEnumerable<dynamic>> FetchAllRelationshipsAsync()
        {
            const string query = @"
                SELECT 
                    fk.name AS ConstraintName,
                    SCHEMA_NAME(tp.schema_id) + '.' + tp.name AS ParentTable,
                    SCHEMA_NAME(tr.schema_id) + '.' + tr.name AS ReferencedTable,
                    cp.name AS ParentColumn,
                    cr.name AS ReferencedColumn,
                    fk.delete_referential_action_desc AS DeleteAction,
                    fk.update_referential_action_desc AS UpdateAction,
                    fk.is_disabled AS IsDisabled
                FROM 
                    sys.foreign_keys fk
                    INNER JOIN sys.foreign_key_columns fkc 
                        ON fk.object_id = fkc.constraint_object_id
                    INNER JOIN sys.tables tp 
                        ON fk.parent_object_id = tp.object_id
                    INNER JOIN sys.tables tr 
                        ON fk.referenced_object_id = tr.object_id
                    INNER JOIN sys.columns cp 
                        ON fkc.parent_object_id = cp.object_id 
                        AND fkc.parent_column_id = cp.column_id
                    INNER JOIN sys.columns cr 
                        ON fkc.referenced_object_id = cr.object_id 
                        AND fkc.referenced_column_id = cr.column_id
                ORDER BY 
                    ParentTable, fk.name";

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<dynamic>(query);
        }

        /// <summary>
        /// Fetches relationships only for specific tables.
        /// </summary>
        /// <param name="tableNames">List of table names.</param>
        /// <returns>Dynamic result set from database.</returns>
        private async Task<IEnumerable<dynamic>> FetchRelationshipsForTablesAsync(List<string> tableNames)
        {
            var tableNamesList = string.Join(",", tableNames.Select(t => $"'{t}'"));

            var query = $@"
                SELECT 
                    fk.name AS ConstraintName,
                    SCHEMA_NAME(tp.schema_id) + '.' + tp.name AS ParentTable,
                    SCHEMA_NAME(tr.schema_id) + '.' + tr.name AS ReferencedTable,
                    cp.name AS ParentColumn,
                    cr.name AS ReferencedColumn,
                    fk.delete_referential_action_desc AS DeleteAction,
                    fk.update_referential_action_desc AS UpdateAction,
                    fk.is_disabled AS IsDisabled
                FROM 
                    sys.foreign_keys fk
                    INNER JOIN sys.foreign_key_columns fkc 
                        ON fk.object_id = fkc.constraint_object_id
                    INNER JOIN sys.tables tp 
                        ON fk.parent_object_id = tp.object_id
                    INNER JOIN sys.tables tr 
                        ON fk.referenced_object_id = tr.object_id
                    INNER JOIN sys.columns cp 
                        ON fkc.parent_object_id = cp.object_id 
                        AND fkc.parent_column_id = cp.column_id
                    INNER JOIN sys.columns cr 
                        ON fkc.referenced_object_id = cr.object_id 
                        AND fkc.referenced_column_id = cr.column_id
                WHERE
                    SCHEMA_NAME(tp.schema_id) + '.' + tp.name IN ({tableNamesList})
                    OR SCHEMA_NAME(tr.schema_id) + '.' + tr.name IN ({tableNamesList})
                ORDER BY 
                    ParentTable, fk.name";

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<dynamic>(query);
        }

        /// <summary>
        /// Processes raw relationship data into Relationship objects.
        /// </summary>
        /// <param name="relationshipData">Raw data from database.</param>
        /// <param name="tables">Optional list of tables for determining relationship type.</param>
        /// <returns>List of processed Relationship objects.</returns>
        private List<Relationship> ProcessRelationships(IEnumerable<dynamic> relationshipData, List<Table> tables)
        {
            var relationships = new List<Relationship>();

            foreach (var rel in relationshipData)
            {
                var relationship = CreateRelationshipFromData(rel, tables);
                relationships.Add(relationship);
            }

            return relationships;
        }

        /// <summary>
        /// Creates a Relationship object from dynamic database result.
        /// </summary>
        /// <param name="data">Dynamic data from database.</param>
        /// <param name="tables">Optional list of tables for determining relationship type.</param>
        /// <returns>Relationship object.</returns>
        private Relationship CreateRelationshipFromData(dynamic data, List<Table> tables)
        {
            return new Relationship
            {
                ConstraintName = data.ConstraintName,
                ParentTable = data.ParentTable,
                ReferencedTable = data.ReferencedTable,
                ParentColumn = data.ParentColumn,
                ReferencedColumn = data.ReferencedColumn,
                DeleteAction = data.DeleteAction,
                UpdateAction = data.UpdateAction,
                IsDisabled = data.IsDisabled,
                RelationshipType = tables != null
                    ? DetermineRelationshipType(data.ParentTable, data.ReferencedTable, tables)
                    : RelationshipType.OneToMany
            };
        }

        /// <summary>
        /// Determines the relationship type (One-to-One, One-to-Many, Many-to-Many).
        /// </summary>
        /// <param name="parentTable">Parent table name.</param>
        /// <param name="referencedTable">Referenced table name.</param>
        /// <param name="tables">List of tables.</param>
        /// <returns>Relationship type.</returns>
        private RelationshipType DetermineRelationshipType(string parentTable, string referencedTable, List<Table> tables)
        {
            try
            {
                var parent = tables.FirstOrDefault(t => t.FullName == parentTable);
                var referenced = tables.FirstOrDefault(t => t.FullName == referencedTable);

                if (parent == null || referenced == null)
                {
                    _logger.LogTrace("Table not found for relationship type detection, defaulting to OneToMany");
                    return RelationshipType.OneToMany;
                }

                // TODO: Check for unique index on FK column in parent table for One-to-One detection
                // For now, default to One-to-Many
                return RelationshipType.OneToMany;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error determining relationship type, defaulting to OneToMany");
                return RelationshipType.OneToMany;
            }
        }

        /// <summary>
        /// Logs analysis completion summary.
        /// </summary>
        /// <param name="relationshipCount">Number of relationships found.</param>
        private void LogAnalysisComplete(int relationshipCount)
        {
            _logger.LogDebug(
                "Relationship analysis complete: {RelationshipCount} relationships found",
                relationshipCount);
        }

        #endregion
    }
}
