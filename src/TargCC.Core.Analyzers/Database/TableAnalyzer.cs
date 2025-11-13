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
    /// מנתח מבנה של טבלה בודדת - עמודות, אינדקסים, מפתחות
    /// </summary>
    public class TableAnalyzer
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;
        private readonly ColumnAnalyzer _columnAnalyzer;

        public TableAnalyzer(string connectionString, ILogger logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _columnAnalyzer = new ColumnAnalyzer(connectionString, logger);
        }

        /// <summary>
        /// מנתח טבלה מלאה - עמודות, PK, indexes, extended properties
        /// </summary>
        public async Task<Table> AnalyzeTableAsync(string tableName)
        {
            try
            {
                _logger.LogDebug($"מתחיל ניתוח טבלה: {tableName}");

                var parts = tableName.Split('.');
                var schemaName = parts.Length > 1 ? parts[0] : "dbo";
                var tableNameOnly = parts.Length > 1 ? parts[1] : parts[0];

                var table = new Table
                {
                    SchemaName = schemaName,
                    Name = tableNameOnly,
                    Columns = new List<Column>(),
                    Indexes = new List<TargCC.Core.Interfaces.Models.Index>()
                };

                // שלב 1: מידע בסיסי על הטבלה
                await LoadTableInfoAsync(table);

                // שלב 2: ניתוח עמודות
                table.Columns = await _columnAnalyzer.AnalyzeColumnsAsync(schemaName, tableNameOnly);

                // שלב 3: זיהוי Primary Key
                await LoadPrimaryKeyAsync(table);

                // שלב 4: ניתוח אינדקסים
                await LoadIndexesAsync(table);

                // שלב 5: Extended Properties
                await LoadExtendedPropertiesAsync(table);

                _logger.LogDebug($"ניתוח טבלה {tableName} הושלם - {table.Columns.Count} עמודות, {table.Indexes.Count} אינדקסים");
                return table;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"שגיאה בניתוח טבלה {tableName}");
                throw;
            }
        }

        /// <summary>
        /// טוען מידע בסיסי על הטבלה
        /// </summary>
        private async Task LoadTableInfoAsync(Table table)
        {
            const string query = @"
                SELECT 
                    t.object_id AS ObjectId,
                    t.create_date AS CreateDate,
                    t.modify_date AS ModifyDate,
                    CAST(ep.value AS NVARCHAR(4000)) AS Description
                FROM 
                    sys.tables t
                    LEFT JOIN sys.extended_properties ep 
                        ON ep.major_id = t.object_id 
                        AND ep.minor_id = 0 
                        AND ep.name = 'MS_Description'
                WHERE 
                    SCHEMA_NAME(t.schema_id) = @SchemaName
                    AND t.name = @TableName";

            using (var connection = new SqlConnection(_connectionString))
            {
                var info = await connection.QuerySingleOrDefaultAsync<dynamic>(
                    query,
                    new { SchemaName = table.SchemaName, TableName = table.Name });

                if (info != null)
                {
                    table.ObjectId = info.ObjectId;
                    table.CreateDate = info.CreateDate;
                    table.ModifyDate = info.ModifyDate;
                    table.Description = info.Description;
                }
            }
        }

        /// <summary>
        /// מזהה את ה-Primary Key של הטבלה
        /// </summary>
        private async Task LoadPrimaryKeyAsync(Table table)
        {
            const string query = @"
                SELECT 
                    c.name AS ColumnName
                FROM 
                    sys.indexes i
                    INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                    INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                WHERE 
                    i.object_id = @ObjectId
                    AND i.is_primary_key = 1
                ORDER BY 
                    ic.key_ordinal";

            using (var connection = new SqlConnection(_connectionString))
            {
                var pkColumns = await connection.QueryAsync<string>(
                    query,
                    new { ObjectId = table.ObjectId });

                table.PrimaryKeyColumns = pkColumns.ToList();

                // סימון העמודות כ-PK
                foreach (var pkColumn in table.PrimaryKeyColumns)
                {
                    var column = table.Columns.FirstOrDefault(c => c.Name == pkColumn);
                    if (column != null)
                    {
                        column.IsPrimaryKey = true;
                    }
                }
            }
        }

        /// <summary>
        /// טוען את כל האינדקסים של הטבלה
        /// </summary>
        private async Task LoadIndexesAsync(Table table)
        {
            const string query = @"
                SELECT 
                    i.name AS IndexName,
                    i.is_unique AS IsUnique,
                    i.is_primary_key AS IsPrimaryKey,
                    i.type_desc AS TypeDescription,
                    STRING_AGG(c.name, ',') WITHIN GROUP (ORDER BY ic.key_ordinal) AS ColumnNames
                FROM 
                    sys.indexes i
                    INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                    INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                WHERE 
                    i.object_id = @ObjectId
                    AND i.type > 0  -- לא heap
                GROUP BY
                    i.name, i.is_unique, i.is_primary_key, i.type_desc
                ORDER BY
                    i.is_primary_key DESC, i.is_unique DESC, i.name";

            using (var connection = new SqlConnection(_connectionString))
            {
                var indexData = await connection.QueryAsync<dynamic>(
                    query,
                    new { ObjectId = table.ObjectId });

                foreach (var idx in indexData)
                {
                    var index = new TargCC.Core.Interfaces.Models.Index
                    {
                        Name = idx.IndexName,
                        IsUnique = idx.IsUnique,
                        IsPrimaryKey = idx.IsPrimaryKey,
                        TypeDescription = idx.TypeDescription,
                        ColumnNames = ((string)idx.ColumnNames).Split(',').ToList()
                    };

                    table.Indexes.Add(index);
                }
            }
        }

        /// <summary>
        /// טוען Extended Properties של הטבלה
        /// </summary>
        private async Task LoadExtendedPropertiesAsync(Table table)
        {
            const string query = @"
                SELECT 
                    ep.name AS PropertyName,
                    CAST(ep.value AS NVARCHAR(4000)) AS PropertyValue
                FROM 
                    sys.extended_properties ep
                WHERE 
                    ep.major_id = @ObjectId
                    AND ep.minor_id = 0
                    AND ep.name != 'MS_Description'";

            using (var connection = new SqlConnection(_connectionString))
            {
                var properties = await connection.QueryAsync<dynamic>(
                    query,
                    new { ObjectId = table.ObjectId });

                table.ExtendedProperties = new Dictionary<string, string>();
                foreach (var prop in properties)
                {
                    table.ExtendedProperties[prop.PropertyName] = prop.PropertyValue;
                }
            }
        }
    }
}
