using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using TargCC.Core.Interfaces;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Analyzers.Database
{
    /// <summary>
    /// מנתח מבנה מסד נתונים - קורא טבלאות, עמודות, אינדקסים וקשרים
    /// </summary>
    public class DatabaseAnalyzer : IAnalyzer
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseAnalyzer> _logger;
        private readonly TableAnalyzer _tableAnalyzer;
        private readonly RelationshipAnalyzer _relationshipAnalyzer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString">Connection string למסד הנתונים</param>
        /// <param name="logger">Logger למעקב אחרי פעולות</param>
        public DatabaseAnalyzer(string connectionString, ILogger<DatabaseAnalyzer> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _tableAnalyzer = new TableAnalyzer(connectionString, logger);
            _relationshipAnalyzer = new RelationshipAnalyzer(connectionString, logger);
        }

        /// <summary>
        /// בודק חיבור למסד נתונים
        /// </summary>
        /// <returns>true אם החיבור הצליח</returns>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                _logger.LogInformation("מנסה להתחבר למסד נתונים...");

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    _logger.LogInformation("חיבור למסד נתונים הצליח");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בחיבור למסד נתונים");
                return false;
            }
        }

        /// <summary>
        /// קורא רשימת כל הטבלאות במסד הנתונים (ללא system tables)
        /// </summary>
        /// <returns>רשימת שמות טבלאות</returns>
        public async Task<List<string>> GetTablesAsync()
        {
            try
            {
                _logger.LogInformation("קורא רשימת טבלאות...");

                const string query = @"
                    SELECT 
                        SCHEMA_NAME(t.schema_id) + '.' + t.name AS TableName
                    FROM 
                        sys.tables t
                    WHERE 
                        t.is_ms_shipped = 0
                    ORDER BY 
                        SCHEMA_NAME(t.schema_id), t.name";

                using (var connection = new SqlConnection(_connectionString))
                {
                    var tables = await connection.QueryAsync<string>(query);
                    var tableList = tables.ToList();

                    _logger.LogInformation($"נמצאו {tableList.Count} טבלאות");
                    return tableList;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בקריאת רשימת טבלאות");
                throw;
            }
        }

        /// <summary>
        /// מבצע ניתוח מלא של Schema - טבלאות, עמודות, אינדקסים וקשרים
        /// </summary>
        /// <returns>DatabaseSchema מלא</returns>
        public async Task<DatabaseSchema> AnalyzeAsync()
        {
            try
            {
                _logger.LogInformation("מתחיל ניתוח מלא של מסד הנתונים...");

                var schema = new DatabaseSchema
                {
                    DatabaseName = await GetDatabaseNameAsync(),
                    ServerName = await GetServerNameAsync(),
                    AnalysisDate = DateTime.UtcNow,
                    Tables = new List<Table>()
                };

                // שלב 1: קריאת כל הטבלאות
                var tableNames = await GetTablesAsync();
                _logger.LogInformation($"נמצאו {tableNames.Count} טבלאות לניתוח");

                // שלב 2: ניתוח כל טבלה
                foreach (var tableName in tableNames)
                {
                    _logger.LogDebug($"מנתח טבלה: {tableName}");
                    var table = await _tableAnalyzer.AnalyzeTableAsync(tableName);
                    schema.Tables.Add(table);
                }

                // שלב 3: ניתוח קשרים בין טבלאות
                _logger.LogInformation("מנתח קשרים בין טבלאות...");
                schema.Relationships = await _relationshipAnalyzer.AnalyzeRelationshipsAsync(schema.Tables);

                _logger.LogInformation($"ניתוח הושלם: {schema.Tables.Count} טבלאות, {schema.Relationships.Count} קשרים");
                return schema;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בניתוח מסד נתונים");
                throw;
            }
        }

        /// <summary>
        /// מבצע ניתוח חלקי - רק טבלאות שהשתנו
        /// </summary>
        /// <param name="changedTables">רשימת טבלאות שהשתנו</param>
        /// <returns>DatabaseSchema עם רק הטבלאות שהשתנו</returns>
        public async Task<DatabaseSchema> AnalyzeIncrementalAsync(List<string> changedTables)
        {
            try
            {
                _logger.LogInformation($"מתחיל ניתוח Incremental של {changedTables.Count} טבלאות...");

                var schema = new DatabaseSchema
                {
                    DatabaseName = await GetDatabaseNameAsync(),
                    ServerName = await GetServerNameAsync(),
                    AnalysisDate = DateTime.UtcNow,
                    Tables = new List<Table>(),
                    IsIncrementalAnalysis = true
                };

                // ניתוח רק הטבלאות שהשתנו
                foreach (var tableName in changedTables)
                {
                    _logger.LogDebug($"מנתח טבלה שהשתנתה: {tableName}");
                    var table = await _tableAnalyzer.AnalyzeTableAsync(tableName);
                    schema.Tables.Add(table);
                }

                // ניתוח קשרים רק של הטבלאות שהשתנו
                _logger.LogInformation("מנתח קשרים של טבלאות שהשתנו...");
                schema.Relationships = await _relationshipAnalyzer.AnalyzeRelationshipsAsync(schema.Tables);

                _logger.LogInformation($"ניתוח Incremental הושלם: {schema.Tables.Count} טבלאות");
                return schema;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בניתוח Incremental");
                throw;
            }
        }

        /// <summary>
        /// מזהה אילו טבלאות השתנו מאז ניתוח קודם
        /// </summary>
        /// <param name="previousSchema">Schema קודם להשוואה</param>
        /// <returns>רשימת טבלאות שהשתנו</returns>
        public async Task<List<string>> DetectChangedTablesAsync(DatabaseSchema previousSchema)
        {
            try
            {
                _logger.LogInformation("מזהה שינויים במבנה מסד הנתונים...");

                var changedTables = new List<string>();
                var currentTables = await GetTablesAsync();

                // בדיקה של טבלאות חדשות
                var previousTableNames = previousSchema.Tables.Select(t => t.FullName).ToHashSet();
                var newTables = currentTables.Where(t => !previousTableNames.Contains(t)).ToList();
                changedTables.AddRange(newTables);

                _logger.LogInformation($"נמצאו {newTables.Count} טבלאות חדשות");

                // בדיקה של טבלאות קיימות שהשתנו
                foreach (var tableName in currentTables.Where(t => previousTableNames.Contains(t)))
                {
                    var hasChanged = await HasTableChangedAsync(tableName, previousSchema);
                    if (hasChanged)
                    {
                        changedTables.Add(tableName);
                    }
                }

                _logger.LogInformation($"סה\"כ {changedTables.Count} טבלאות השתנו");
                return changedTables;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בזיהוי שינויים");
                throw;
            }
        }

        #region Helper Methods

        /// <summary>
        /// קורא את שם מסד הנתונים
        /// </summary>
        private async Task<string> GetDatabaseNameAsync()
        {
            const string query = "SELECT DB_NAME()";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QuerySingleAsync<string>(query);
            }
        }

        /// <summary>
        /// קורא את שם השרת
        /// </summary>
        private async Task<string> GetServerNameAsync()
        {
            const string query = "SELECT @@SERVERNAME";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QuerySingleAsync<string>(query);
            }
        }

        /// <summary>
        /// בודק אם טבלה השתנתה מאז ניתוח קודם
        /// </summary>
        private async Task<bool> HasTableChangedAsync(string tableName, DatabaseSchema previousSchema)
        {
            try
            {
                var previousTable = previousSchema.Tables.FirstOrDefault(t => t.FullName == tableName);
                if (previousTable == null)
                {
                    return true; // טבלה חדשה
                }

                // קריאת modify_date של הטבלה
                const string query = @"
                    SELECT modify_date 
                    FROM sys.tables 
                    WHERE SCHEMA_NAME(schema_id) + '.' + name = @TableName";

                using (var connection = new SqlConnection(_connectionString))
                {
                    var modifyDate = await connection.QuerySingleOrDefaultAsync<DateTime?>(
                        query, 
                        new { TableName = tableName });

                    if (!modifyDate.HasValue)
                    {
                        return false;
                    }

                    // השוואה לתאריך הניתוח הקודם
                    return modifyDate.Value > previousSchema.AnalysisDate;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"שגיאה בבדיקת שינויים לטבלה {tableName}");
                return true; // במקרה של ספק, מחזירים שהטבלה השתנתה
            }
        }

        #endregion

        #region IAnalyzer Implementation

        string IAnalyzer.Name => "Database Analyzer";
        string IAnalyzer.Version => "1.0.0";

        async Task<object> IAnalyzer.AnalyzeAsync(object input, CancellationToken cancellationToken = default)
        {
            return await AnalyzeAsync();
        }

        #endregion
    }
}
