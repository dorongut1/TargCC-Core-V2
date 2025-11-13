using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Analyzers.Database
{
    /// <summary>
    /// מנתח קשרים בין טבלאות (Foreign Keys)
    /// </summary>
    public class RelationshipAnalyzer
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;

        public RelationshipAnalyzer(string connectionString, ILogger logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// מנתח את כל הקשרים בין הטבלאות
        /// </summary>
        /// <param name="tables">רשימת הטבלאות לניתוח</param>
        /// <returns>רשימת קשרים</returns>
        public async Task<List<Relationship>> AnalyzeRelationshipsAsync(List<Table> tables)
        {
            try
            {
                _logger.LogDebug("מנתח קשרים בין טבלאות...");

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

                using (var connection = new SqlConnection(_connectionString))
                {
                    var relationshipData = await connection.QueryAsync<dynamic>(query);

                    var relationships = new List<Relationship>();

                    foreach (var rel in relationshipData)
                    {
                        var relationship = new Relationship
                        {
                            ConstraintName = rel.ConstraintName,
                            ParentTable = rel.ParentTable,
                            ReferencedTable = rel.ReferencedTable,
                            ParentColumn = rel.ParentColumn,
                            ReferencedColumn = rel.ReferencedColumn,
                            DeleteAction = rel.DeleteAction,
                            UpdateAction = rel.UpdateAction,
                            IsDisabled = rel.IsDisabled,
                            RelationshipType = DetermineRelationshipType(rel.ParentTable, rel.ReferencedTable, tables)
                        };

                        relationships.Add(relationship);
                    }

                    _logger.LogDebug($"נמצאו {relationships.Count} קשרים");
                    return relationships;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בניתוח קשרים");
                throw;
            }
        }

        /// <summary>
        /// קורא קשרים רק עבור טבלאות ספציפיות (Incremental)
        /// </summary>
        public async Task<List<Relationship>> AnalyzeRelationshipsForTablesAsync(List<string> tableNames)
        {
            try
            {
                _logger.LogDebug($"מנתח קשרים עבור {tableNames.Count} טבלאות...");

                if (!tableNames.Any())
                    return new List<Relationship>();

                // בניית רשימת שמות טבלאות ל-IN clause
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

                using (var connection = new SqlConnection(_connectionString))
                {
                    var relationshipData = await connection.QueryAsync<dynamic>(query);

                    var relationships = new List<Relationship>();

                    foreach (var rel in relationshipData)
                    {
                        var relationship = new Relationship
                        {
                            ConstraintName = rel.ConstraintName,
                            ParentTable = rel.ParentTable,
                            ReferencedTable = rel.ReferencedTable,
                            ParentColumn = rel.ParentColumn,
                            ReferencedColumn = rel.ReferencedColumn,
                            DeleteAction = rel.DeleteAction,
                            UpdateAction = rel.UpdateAction,
                            IsDisabled = rel.IsDisabled,
                            RelationshipType = RelationshipType.OneToMany // Default
                        };

                        relationships.Add(relationship);
                    }

                    _logger.LogDebug($"נמצאו {relationships.Count} קשרים לטבלאות שצוינו");
                    return relationships;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בניתוח קשרים עבור טבלאות");
                throw;
            }
        }

        /// <summary>
        /// קובע את סוג הקשר (One-to-One, One-to-Many, Many-to-Many)
        /// </summary>
        private RelationshipType DetermineRelationshipType(string parentTable, string referencedTable, List<Table> tables)
        {
            try
            {
                // מוצא את הטבלאות
                var parent = tables.FirstOrDefault(t => t.FullName == parentTable);
                var referenced = tables.FirstOrDefault(t => t.FullName == referencedTable);

                if (parent == null || referenced == null)
                    return RelationshipType.OneToMany; // Default

                // בדיקה אם יש Unique Index על עמודת ה-FK בטבלת ה-Parent
                // אם כן, זה One-to-One, אחרת One-to-Many
                // (זיהוי Many-to-Many דורש ניתוח מורכב יותר של טבלאות ביניים)

                return RelationshipType.OneToMany; // לעת עתה, ברירת מחדל
            }
            catch
            {
                return RelationshipType.OneToMany;
            }
        }

        /// <summary>
        /// בונה גרף של קשרים (עבור ניתוחים מתקדמים)
        /// </summary>
        public Dictionary<string, List<string>> BuildRelationshipGraph(List<Relationship> relationships)
        {
            var graph = new Dictionary<string, List<string>>();

            foreach (var rel in relationships)
            {
                // Parent -> Referenced
                if (!graph.ContainsKey(rel.ParentTable))
                {
                    graph[rel.ParentTable] = new List<string>();
                }
                graph[rel.ParentTable].Add(rel.ReferencedTable);

                // Referenced -> Parent (reverse)
                if (!graph.ContainsKey(rel.ReferencedTable))
                {
                    graph[rel.ReferencedTable] = new List<string>();
                }
                // לא מוסיפים reverse כי זה יוצר קשרים כפולים
            }

            return graph;
        }

        /// <summary>
        /// מזהה טבלאות שהן Parent של טבלה נתונה
        /// </summary>
        public List<string> GetParentTables(string tableName, List<Relationship> relationships)
        {
            return relationships
                .Where(r => r.ParentTable == tableName)
                .Select(r => r.ReferencedTable)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// מזהה טבלאות שהן Children של טבלה נתונה
        /// </summary>
        public List<string> GetChildTables(string tableName, List<Relationship> relationships)
        {
            return relationships
                .Where(r => r.ReferencedTable == tableName)
                .Select(r => r.ParentTable)
                .Distinct()
                .ToList();
        }
    }
}
