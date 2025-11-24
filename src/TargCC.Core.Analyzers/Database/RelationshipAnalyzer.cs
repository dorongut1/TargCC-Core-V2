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
    /// Builds relationship graphs and determines parent-child hierarchies.
    /// </summary>
    /// <remarks>
    /// <para>
    /// RelationshipAnalyzer detects and maps foreign key relationships between tables,
    /// enabling code generators to create navigation properties, fill methods, and
    /// entity relationship diagrams.
    /// </para>
    /// <para>
    /// <strong>Terminology:</strong>
    /// <list type="bullet">
    /// <item><term>Parent Table</term><description>The table that HAS the foreign key (e.g., Order has CustomerID)</description></item>
    /// <item><term>Referenced Table</term><description>The table REFERENCED by the foreign key (e.g., Customer is referenced)</description></item>
    /// <item><term>Child Table</term><description>Tables that reference THIS table (from the perspective of the referenced table)</description></item>
    /// <item><term>One-to-Many</term><description>One Customer has Many Orders (most common)</description></item>
    /// <item><term>One-to-One</term><description>One Order has One OrderDetail (FK column is unique)</description></item>
    /// <item><term>Many-to-Many</term><description>Students ↔ Courses via Junction table</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// <list type="bullet">
    /// <item>Generate navigation properties (Customer.Orders, Order.Customer)</item>
    /// <item>Create FillChildren methods (LoadOrders for Customer)</item>
    /// <item>Build entity relationship diagrams</item>
    /// <item>Detect circular references</item>
    /// <item>Incremental code generation (only changed relationships)</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var analyzer = new RelationshipAnalyzer(connectionString, logger);
    ///
    /// // Analyze all relationships
    /// var relationships = await analyzer.AnalyzeRelationshipsAsync(tables);
    ///
    /// // Find all relationships for Customer table
    /// var customerRelationships = relationships
    ///     .Where(r => r.ParentTable == "dbo.Customer" || r.ReferencedTable == "dbo.Customer")
    ///     .ToList();
    ///
    /// // Get parent tables (tables this table references)
    /// var parents = analyzer.GetParentTables("dbo.Order", relationships);
    /// // Result: ["dbo.Customer", "dbo.Shipper"]
    ///
    /// // Get child tables (tables that reference this table)
    /// var children = analyzer.GetChildTables("dbo.Customer", relationships);
    /// // Result: ["dbo.Order", "dbo.CustomerAddress"]
    ///
    /// // Build relationship graph for visualization
    /// var graph = analyzer.BuildRelationshipGraph(relationships);
    /// foreach (var node in graph)
    /// {
    ///     Console.WriteLine($"{node.Key} → {string.Join(", ", node.Value)}");
    /// }
    /// </code>
    /// </example>
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
        /// Analyzes all relationships between tables in the database.
        /// </summary>
        /// <param name="tables">List of tables to analyze relationships for.</param>
        /// <returns>Complete list of all foreign key relationships found in the database.</returns>
        /// <exception cref="ArgumentNullException">Thrown when tables is null.</exception>
        /// <exception cref="SqlException">Thrown when database operation fails.</exception>
        /// <remarks>
        /// <para>
        /// This method performs a full analysis of all foreign key constraints in the database.
        /// It retrieves constraint names, parent/referenced tables, columns, and actions (CASCADE, NO ACTION).
        /// </para>
        /// <para>
        /// The tables parameter is used to determine relationship types (One-to-One vs One-to-Many)
        /// by checking for unique indexes on foreign key columns.
        /// </para>
        /// <para>
        /// <strong>Performance:</strong> For large databases with 100+ tables, consider using
        /// <see cref="AnalyzeRelationshipsForTablesAsync"/> for incremental analysis.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var dbAnalyzer = new DatabaseAnalyzer(connectionString, logger);
        /// var tables = await dbAnalyzer.AnalyzeAsync();
        /// 
        /// var relAnalyzer = new RelationshipAnalyzer(connectionString, logger);
        /// var relationships = await relAnalyzer.AnalyzeRelationshipsAsync(tables.Tables);
        /// 
        /// Console.WriteLine($"Found {relationships.Count} relationships");
        /// 
        /// // Example: Find all Orders → Customer relationships
        /// var orderCustomerRels = relationships
        ///     .Where(r => r.ParentTable == "dbo.Order" &amp;&amp; r.ReferencedTable == "dbo.Customer")
        ///     .ToList();
        /// 
        /// foreach (var rel in orderCustomerRels)
        /// {
        ///     Console.WriteLine($"FK: {rel.ConstraintName}");
        ///     Console.WriteLine($"  {rel.ParentTable}.{rel.ParentColumn}");
        ///     Console.WriteLine($"  → {rel.ReferencedTable}.{rel.ReferencedColumn}");
        ///     Console.WriteLine($"  Delete: {rel.DeleteAction}");
        ///     Console.WriteLine($"  Type: {rel.RelationshipType}");
        /// }
        /// </code>
        /// </example>
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
        /// Analyzes relationships only for specific tables (incremental analysis for change detection).
        /// </summary>
        /// <param name="tableNames">List of table names to analyze (format: "schema.table").</param>
        /// <returns>List of relationships involving any of the specified tables (as parent or referenced).</returns>
        /// <exception cref="ArgumentNullException">Thrown when tableNames is null.</exception>
        /// <exception cref="SqlException">Thrown when database operation fails.</exception>
        /// <remarks>
        /// <para>
        /// This method is optimized for incremental code generation. When only specific tables
        /// change, this avoids analyzing the entire database and only fetches relationships
        /// where the specified tables are involved (either as parent or referenced table).
        /// </para>
        /// <para>
        /// <strong>Use Case - Change Detection:</strong>
        /// When a table schema changes, you only need to regenerate code for:
        /// <list type="bullet">
        /// <item>The changed table itself</item>
        /// <item>Tables that reference it (children)</item>
        /// <item>Tables it references (parents)</item>
        /// </list>
        /// This method retrieves all those relationships efficiently.
        /// </para>
        /// <para>
        /// <strong>Performance:</strong> This is 10-100x faster than full analysis for large databases
        /// when only analyzing 1-5 tables.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var analyzer = new RelationshipAnalyzer(connectionString, logger);
        /// 
        /// // Scenario: Customer table was modified, regenerate only affected code
        /// var changedTables = new List&lt;string&gt; { "dbo.Customer" };
        /// var relationships = await analyzer.AnalyzeRelationshipsForTablesAsync(changedTables);
        /// 
        /// // Result includes:
        /// // - Customer → Address (Customer references Address)
        /// // - Order → Customer (Order references Customer)
        /// // - CustomerPreference → Customer (CustomerPreference references Customer)
        /// 
        /// Console.WriteLine($"Found {relationships.Count} affected relationships");
        /// 
        /// // Example: Multiple changed tables
        /// var changedTables2 = new List&lt;string&gt; { "dbo.Order", "dbo.OrderDetail" };
        /// var relationships2 = await analyzer.AnalyzeRelationshipsForTablesAsync(changedTables2);
        /// 
        /// // This is the CORE of incremental generation!
        /// // Only regenerate code for these relationships, not the entire database.
        /// </code>
        /// </example>
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
        /// Builds a relationship graph for advanced analysis, visualization, and circular reference detection.
        /// </summary>
        /// <param name="relationships">List of relationships to build graph from.</param>
        /// <returns>Dictionary mapping each table name to list of tables it references (adjacency list representation).</returns>
        /// <exception cref="ArgumentNullException">Thrown when relationships is null.</exception>
        /// <remarks>
        /// <para>
        /// The relationship graph is represented as an adjacency list where:
        /// <list type="bullet">
        /// <item><term>Key</term><description>Table name (format: "schema.table")</description></item>
        /// <item><term>Value</term><description>List of table names this table references via foreign keys</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// <strong>Graph Direction:</strong>
        /// The graph is directed from parent → referenced table.
        /// For example: Order → Customer means Order.CustomerID references Customer.ID
        /// </para>
        /// <para>
        /// <strong>Use Cases:</strong>
        /// <list type="bullet">
        /// <item>Detect circular references (A → B → C → A)</item>
        /// <item>Visualize entity relationships</item>
        /// <item>Determine generation order (topological sort)</item>
        /// <item>Find dependency chains</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var analyzer = new RelationshipAnalyzer(connectionString, logger);
        /// var relationships = await analyzer.AnalyzeRelationshipsAsync(tables);
        /// 
        /// // Build the graph
        /// var graph = analyzer.BuildRelationshipGraph(relationships);
        /// 
        /// // Print the graph
        /// foreach (var node in graph)
        /// {
        ///     Console.WriteLine($"{node.Key}:");
        ///     foreach (var target in node.Value)
        ///     {
        ///         Console.WriteLine($"  → {target}");
        ///     }
        /// }
        /// 
        /// // Example output:
        /// // dbo.Order:
        /// //   → dbo.Customer
        /// //   → dbo.Shipper
        /// // dbo.OrderDetail:
        /// //   → dbo.Order
        /// //   → dbo.Product
        /// // dbo.Customer:
        /// //   (no outgoing edges - referenced by others)
        /// 
        /// // Use case: Detect circular references
        /// bool HasCircularReference(Dictionary&lt;string, List&lt;string&gt;&gt; graph, string table)
        /// {
        ///     var visited = new HashSet&lt;string&gt;();
        ///     var stack = new HashSet&lt;string&gt;();
        ///     
        ///     bool DFS(string current)
        ///     {
        ///         if (stack.Contains(current)) return true; // Circular!
        ///         if (visited.Contains(current)) return false;
        ///         
        ///         visited.Add(current);
        ///         stack.Add(current);
        ///         
        ///         if (graph.ContainsKey(current))
        ///         {
        ///             foreach (var neighbor in graph[current])
        ///             {
        ///                 if (DFS(neighbor)) return true;
        ///             }
        ///         }
        ///         
        ///         stack.Remove(current);
        ///         return false;
        ///     }
        ///     
        ///     return DFS(table);
        /// }
        /// </code>
        /// </example>
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
        /// Gets parent tables (tables that THIS table references via foreign keys) for a given table.
        /// </summary>
        /// <param name="tableName">Name of the table (format: "schema.table").</param>
        /// <param name="relationships">List of all relationships.</param>
        /// <returns>List of parent table names that this table depends on.</returns>
        /// <exception cref="ArgumentNullException">Thrown when tableName or relationships is null.</exception>
        /// <remarks>
        /// <para>
        /// <strong>Parent = Dependency = Referenced Table</strong>
        /// </para>
        /// <para>
        /// A parent table is one that THIS table references. For example:
        /// <list type="bullet">
        /// <item>Order table has foreign key to Customer → Customer is a parent of Order</item>
        /// <item>OrderDetail references Order and Product → Order and Product are parents of OrderDetail</item>
        /// </list>
        /// </para>
        /// <para>
        /// <strong>Code Generation Use:</strong>
        /// Parent tables are used to generate:
        /// <list type="bullet">
        /// <item>Navigation properties (Order.Customer, OrderDetail.Product)</item>
        /// <item>LoadParent methods (LoadCustomer for Order)</item>
        /// <item>Dropdown/ComboBox population (CustomerID dropdown in Order form)</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var analyzer = new RelationshipAnalyzer(connectionString, logger);
        /// var relationships = await analyzer.AnalyzeRelationshipsAsync(tables);
        /// 
        /// // Get parent tables for Order
        /// var orderParents = analyzer.GetParentTables("dbo.Order", relationships);
        /// // Result: ["dbo.Customer", "dbo.Shipper", "dbo.Employee"]
        /// 
        /// // Get parent tables for Customer (top-level table)
        /// var customerParents = analyzer.GetParentTables("dbo.Customer", relationships);
        /// // Result: [] (empty - Customer doesn't reference anyone)
        /// 
        /// // Get parent tables for OrderDetail (child of Order)
        /// var detailParents = analyzer.GetParentTables("dbo.OrderDetail", relationships);
        /// // Result: ["dbo.Order", "dbo.Product"]
        /// 
        /// // Use case: Generate navigation properties
        /// foreach (var parent in orderParents)
        /// {
        ///     Console.WriteLine($"public {parent.Split('.')[1]} {parent.Split('.')[1]} {{ get; set; }}");
        /// }
        /// // Output:
        /// // public Customer Customer { get; set; }
        /// // public Shipper Shipper { get; set; }
        /// // public Employee Employee { get; set; }
        /// </code>
        /// </example>
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
        /// Gets child tables (tables that reference THIS table via foreign keys) for a given table.
        /// </summary>
        /// <param name="tableName">Name of the table (format: "schema.table").</param>
        /// <param name="relationships">List of all relationships.</param>
        /// <returns>List of child table names that depend on this table.</returns>
        /// <exception cref="ArgumentNullException">Thrown when tableName or relationships is null.</exception>
        /// <remarks>
        /// <para>
        /// <strong>Child = Dependent = Referencing Table</strong>
        /// </para>
        /// <para>
        /// A child table is one that references THIS table. For example:
        /// <list type="bullet">
        /// <item>Order references Customer → Order is a child of Customer</item>
        /// <item>OrderDetail references Order → OrderDetail is a child of Order</item>
        /// <item>Customer is referenced by Order, CustomerAddress, CustomerPreference → all are children of Customer</item>
        /// </list>
        /// </para>
        /// <para>
        /// <strong>Code Generation Use:</strong>
        /// Child tables are used to generate:
        /// <list type="bullet">
        /// <item>Collection properties (Customer.Orders, Order.OrderDetails)</item>
        /// <item>FillChildren methods (LoadOrders for Customer)</item>
        /// <item>Cascade operations (when Customer deleted, delete all Orders)</item>
        /// <item>Master-detail forms (Customer form shows Orders grid)</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var analyzer = new RelationshipAnalyzer(connectionString, logger);
        /// var relationships = await analyzer.AnalyzeRelationshipsAsync(tables);
        /// 
        /// // Get child tables for Customer
        /// var customerChildren = analyzer.GetChildTables("dbo.Customer", relationships);
        /// // Result: ["dbo.Order", "dbo.CustomerAddress", "dbo.CustomerPreference"]
        /// 
        /// // Get child tables for Order
        /// var orderChildren = analyzer.GetChildTables("dbo.Order", relationships);
        /// // Result: ["dbo.OrderDetail", "dbo.OrderNote"]
        /// 
        /// // Get child tables for OrderDetail (leaf table)
        /// var detailChildren = analyzer.GetChildTables("dbo.OrderDetail", relationships);
        /// // Result: [] (empty - nothing references OrderDetail)
        /// 
        /// // Use case: Generate collection properties
        /// foreach (var child in customerChildren)
        /// {
        ///     var childName = child.Split('.')[1];
        ///     Console.WriteLine($"public List&lt;{childName}&gt; {childName}List {{ get; set; }}");
        /// }
        /// // Output:
        /// // public List&lt;Order&gt; OrderList { get; set; }
        /// // public List&lt;CustomerAddress&gt; CustomerAddressList { get; set; }
        /// // public List&lt;CustomerPreference&gt; CustomerPreferenceList { get; set; }
        /// 
        /// // Use case: Generate FillChildren method
        /// Console.WriteLine($"public void LoadChildren()");
        /// Console.WriteLine("{");
        /// foreach (var child in customerChildren)
        /// {
        ///     var childName = child.Split('.')[1];
        ///     Console.WriteLine($"    this.{childName}List = Load{childName}List(this.ID);");
        /// }
        /// Console.WriteLine("}");
        /// </code>
        /// </example>
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
        /// Determines the relationship type (One-to-One, One-to-Many, Many-to-Many) between two tables.
        /// </summary>
        /// <param name="parentTable">Parent table name (table with FK).</param>
        /// <param name="referencedTable">Referenced table name (table being referenced).</param>
        /// <param name="tables">List of all tables with index information.</param>
        /// <returns>Relationship type enum value.</returns>
        /// <remarks>
        /// <para>
        /// <strong>Relationship Type Detection Logic:</strong>
        /// <list type="bullet">
        /// <item><term>One-to-Many</term><description>Default. FK column in parent is NOT unique (most common)</description></item>
        /// <item><term>One-to-One</term><description>FK column in parent HAS unique index (rare)</description></item>
        /// <item><term>Many-to-Many</term><description>Detected via junction table pattern (future enhancement)</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// <strong>Examples:</strong>
        /// <list type="bullet">
        /// <item>Order.CustomerID → Customer.ID (no unique index on CustomerID) = One-to-Many</item>
        /// <item>Order.InvoiceID → Invoice.ID (unique index on InvoiceID) = One-to-One</item>
        /// <item>StudentCourse.StudentID → Student.ID (junction table) = Many-to-Many (future)</item>
        /// </list>
        /// </para>
        /// <para>
        /// <strong>Current Status:</strong>
        /// Currently defaults to One-to-Many. TODO: Implement unique index detection for One-to-One.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // One-to-Many example (most common):
        /// // Table: Order
        /// //   CustomerID int FK to Customer(ID)  -- No unique index
        /// // Result: RelationshipType.OneToMany
        /// // Meaning: One Customer has Many Orders
        /// 
        /// // One-to-One example:
        /// // Table: Order
        /// //   InvoiceID int FK to Invoice(ID)  -- Unique index on InvoiceID
        /// // Result: RelationshipType.OneToOne
        /// // Meaning: One Order has One Invoice
        /// 
        /// // Many-to-Many example (future):
        /// // Table: StudentCourse (junction table)
        /// //   StudentID int FK to Student(ID)
        /// //   CourseID int FK to Course(ID)
        /// //   Composite PK: (StudentID, CourseID)
        /// // Result: RelationshipType.ManyToMany
        /// // Meaning: Many Students have Many Courses
        /// 
        /// // Usage in code generation:
        /// if (rel.RelationshipType == RelationshipType.OneToMany)
        /// {
        ///     // Generate collection property
        ///     // public List&lt;Order&gt; Orders { get; set; }
        /// }
        /// else if (rel.RelationshipType == RelationshipType.OneToOne)
        /// {
        ///     // Generate single property
        ///     // public Invoice Invoice { get; set; }
        /// }
        /// </code>
        /// </example>
        private RelationshipType DetermineRelationshipType(string parentTable, string referencedTable, List<Table> tables)
        {
            try
            {
                var parent = tables.Find(t => t.FullName == parentTable);
                var referenced = tables.Find(t => t.FullName == referencedTable);

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
