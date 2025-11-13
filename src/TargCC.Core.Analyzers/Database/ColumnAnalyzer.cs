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
    /// מנתח עמודות בטבלה - סוגים, Nullability, ברירות מחדל, Extended Properties
    /// </summary>
    public class ColumnAnalyzer
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;

        public ColumnAnalyzer(string connectionString, ILogger logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// מנתח את כל העמודות בטבלה
        /// </summary>
        public async Task<List<Column>> AnalyzeColumnsAsync(string schemaName, string tableName)
        {
            try
            {
                _logger.LogDebug($"מנתח עמודות בטבלה {schemaName}.{tableName}");

                const string query = @"
                    SELECT 
                        c.column_id AS ColumnId,
                        c.name AS Name,
                        TYPE_NAME(c.user_type_id) AS DataType,
                        c.max_length AS MaxLength,
                        c.precision AS Precision,
                        c.scale AS Scale,
                        c.is_nullable AS IsNullable,
                        c.is_identity AS IsIdentity,
                        c.is_computed AS IsComputed,
                        CAST(dc.definition AS NVARCHAR(4000)) AS DefaultValue,
                        CAST(cc.definition AS NVARCHAR(4000)) AS ComputedDefinition,
                        CAST(ep.value AS NVARCHAR(4000)) AS Description
                    FROM 
                        sys.columns c
                        LEFT JOIN sys.default_constraints dc 
                            ON dc.parent_object_id = c.object_id 
                            AND dc.parent_column_id = c.column_id
                        LEFT JOIN sys.computed_columns cc 
                            ON cc.object_id = c.object_id 
                            AND cc.column_id = c.column_id
                        LEFT JOIN sys.extended_properties ep 
                            ON ep.major_id = c.object_id 
                            AND ep.minor_id = c.column_id 
                            AND ep.name = 'MS_Description'
                    WHERE 
                        c.object_id = OBJECT_ID(@FullTableName)
                    ORDER BY 
                        c.column_id";

                using (var connection = new SqlConnection(_connectionString))
                {
                    var columnData = await connection.QueryAsync<dynamic>(
                        query,
                        new { FullTableName = $"{schemaName}.{tableName}" });

                    var columns = new List<Column>();

                    foreach (var col in columnData)
                    {
                        var column = new Column
                        {
                            ColumnId = col.ColumnId,
                            Name = col.Name,
                            DataType = col.DataType,
                            MaxLength = col.MaxLength,
                            Precision = col.Precision,
                            Scale = col.Scale,
                            IsNullable = col.IsNullable,
                            IsIdentity = col.IsIdentity,
                            IsComputed = col.IsComputed,
                            DefaultValue = col.DefaultValue,
                            ComputedDefinition = col.ComputedDefinition,
                            Description = col.Description
                        };

                        // זיהוי Prefix מיוחד (TargCC conventions)
                        AnalyzeColumnPrefix(column);

                        // טעינת Extended Properties
                        await LoadColumnExtendedPropertiesAsync(column, schemaName, tableName);

                        // המרת סוג SQL ל-.NET Type
                        column.DotNetType = MapSqlTypeToDotNet(column.DataType);

                        columns.Add(column);
                    }

                    _logger.LogDebug($"נמצאו {columns.Count} עמודות בטבלה {schemaName}.{tableName}");
                    return columns;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"שגיאה בניתוח עמודות של {schemaName}.{tableName}");
                throw;
            }
        }

        /// <summary>
        /// מזהה Prefix מיוחד של TargCC (eno, ent, lkp, enm, וכו')
        /// </summary>
        private void AnalyzeColumnPrefix(Column column)
        {
            var name = column.Name.ToLower();

            // One-way encryption
            if (name.StartsWith("eno"))
            {
                column.Prefix = ColumnPrefix.OneWayEncryption;
                column.IsEncrypted = true;
            }

            // Two-way encryption
            else if (name.StartsWith("ent"))
            {
                column.Prefix = ColumnPrefix.TwoWayEncryption;
                column.IsEncrypted = true;
            }

            // Enumeration
            else if (name.StartsWith("enm"))
            {
                column.Prefix = ColumnPrefix.Enumeration;
            }

            // Lookup
            else if (name.StartsWith("lkp"))
            {
                column.Prefix = ColumnPrefix.Lookup;
            }

            // Localization
            else if (name.StartsWith("loc"))
            {
                column.Prefix = ColumnPrefix.Localization;
            }

            // Calculated
            else if (name.StartsWith("clc_"))
            {
                column.Prefix = ColumnPrefix.Calculated;
                column.IsReadOnly = true;
            }

            // Business Logic
            else if (name.StartsWith("blg_"))
            {
                column.Prefix = ColumnPrefix.BusinessLogic;
                column.IsReadOnly = true; // Read-only from client side
            }
            // Aggregate
            else if (name.StartsWith("agg_"))
            {
                column.Prefix = ColumnPrefix.Aggregate;
                column.IsReadOnly = true;
            }

            // Separate Update
            else if (name.StartsWith("spt_"))
            {
                column.Prefix = ColumnPrefix.SeparateUpdate;
            }

            // Separate List
            else if (name.StartsWith("spl_"))
            {
                column.Prefix = ColumnPrefix.SeparateList;
            }

            // Upload
            else if (name.StartsWith("upl_"))
            {
                column.Prefix = ColumnPrefix.Upload;
            }

            // Fake Unique Index
            else if (name.StartsWith("fui_"))
            {
                column.Prefix = ColumnPrefix.FakeUniqueIndex;
            }
            else
            {
                column.Prefix = ColumnPrefix.None;
            }
        }

        /// <summary>
        /// טוען Extended Properties של עמודה
        /// </summary>
        private async Task LoadColumnExtendedPropertiesAsync(Column column, string schemaName, string tableName)
        {
            const string query = @"
                SELECT 
                    ep.name AS PropertyName,
                    CAST(ep.value AS NVARCHAR(4000)) AS PropertyValue
                FROM 
                    sys.extended_properties ep
                    INNER JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id
                WHERE 
                    c.object_id = OBJECT_ID(@FullTableName)
                    AND c.name = @ColumnName
                    AND ep.name != 'MS_Description'";

            using (var connection = new SqlConnection(_connectionString))
            {
                var properties = await connection.QueryAsync<dynamic>(
                    query,
                    new
                    {
                        FullTableName = $"{schemaName}.{tableName}",
                        ColumnName = column.Name
                    });

                column.ExtendedProperties = new Dictionary<string, string>();
                foreach (var prop in properties)
                {
                    column.ExtendedProperties[prop.PropertyName] = prop.PropertyValue;

                    // טיפול מיוחד ב-ccType
                    if (prop.PropertyName.Equals("ccType", StringComparison.OrdinalIgnoreCase))
                    {
                        ParseCcType(column, prop.PropertyValue);
                    }

                    // טיפול ב-ccDNA (Do Not Audit)
                    if (prop.PropertyName.Equals("ccDNA", StringComparison.OrdinalIgnoreCase))
                    {
                        column.DoNotAudit = prop.PropertyValue == "1";
                    }
                }
            }
        }

        /// <summary>
        /// מפרסר את ה-ccType Extended Property
        /// </summary>
        private void ParseCcType(Column column, string ccType)
        {
            if (string.IsNullOrWhiteSpace(ccType))
                return;

            var types = ccType.Split(',').Select(t => t.Trim().ToLower()).ToList();

            if (types.Contains("blg"))
            {
                column.Prefix = ColumnPrefix.BusinessLogic;
                column.IsReadOnly = true;
            }

            if (types.Contains("clc"))
            {
                column.Prefix = ColumnPrefix.Calculated;
                column.IsReadOnly = true;
            }

            if (types.Contains("spt"))
            {
                column.Prefix = ColumnPrefix.SeparateUpdate;
            }

            if (types.Contains("agg"))
            {
                column.Prefix = ColumnPrefix.Aggregate;
                column.IsReadOnly = true;
            }
        }

        /// <summary>
        /// ממיר סוג SQL ל-.NET Type
        /// </summary>
        private string MapSqlTypeToDotNet(string sqlType)
        {
            return sqlType.ToLower() switch
            {
                "bigint" => "long",
                "int" => "int",
                "smallint" => "short",
                "tinyint" => "byte",
                "bit" => "bool",
                "decimal" or "numeric" or "money" or "smallmoney" => "decimal",
                "float" => "double",
                "real" => "float",
                "date" or "datetime" or "datetime2" or "smalldatetime" => "DateTime",
                "time" => "TimeSpan",
                "datetimeoffset" => "DateTimeOffset",
                "char" or "varchar" or "text" or "nchar" or "nvarchar" or "ntext" => "string",
                "uniqueidentifier" => "Guid",
                "binary" or "varbinary" or "image" => "byte[]",
                "xml" => "string",
                _ => "object"
            };
        }
    }
}
