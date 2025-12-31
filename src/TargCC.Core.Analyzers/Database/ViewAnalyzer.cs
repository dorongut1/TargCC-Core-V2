using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Analyzers.Database;

/// <summary>
/// Analyzes database views and extracts metadata for code generation.
/// </summary>
public class ViewAnalyzer
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewAnalyzer"/> class.
    /// </summary>
    /// <param name="connectionString">The database connection string.</param>
    public ViewAnalyzer(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Analyzes all views in the database and returns view metadata.
    /// </summary>
    /// <returns>A list of view information objects.</returns>
    public async Task<List<ViewInfo>> AnalyzeViewsAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var views = await GetAllViewsAsync(connection);
        var viewInfos = new List<ViewInfo>();

        foreach (var view in views)
        {
            var viewInfo = new ViewInfo
            {
                ViewName = view.ViewName,
                SchemaName = view.SchemaName,
                Type = DetermineViewType(view.ViewName),
                Columns = await GetViewColumnsAsync(connection, view.SchemaName, view.ViewName)
            };

            viewInfos.Add(viewInfo);
        }

        return viewInfos;
    }

    private static async Task<List<(string SchemaName, string ViewName)>> GetAllViewsAsync(IDbConnection connection)
    {
        const string sql = @"
            SELECT
                TABLE_SCHEMA AS SchemaName,
                TABLE_NAME AS ViewName
            FROM INFORMATION_SCHEMA.VIEWS
            WHERE TABLE_SCHEMA NOT IN ('sys', 'INFORMATION_SCHEMA')
            ORDER BY TABLE_SCHEMA, TABLE_NAME";

        var results = await connection.QueryAsync<(string SchemaName, string ViewName)>(sql);
        return results.ToList();
    }

    private static async Task<List<ViewColumn>> GetViewColumnsAsync(IDbConnection connection, string schemaName, string viewName)
    {
        const string sql = @"
            SELECT
                COLUMN_NAME AS Name,
                DATA_TYPE AS DataType,
                CHARACTER_MAXIMUM_LENGTH AS MaxLength,
                CASE WHEN IS_NULLABLE = 'YES' THEN 1 ELSE 0 END AS IsNullable,
                ORDINAL_POSITION AS OrdinalPosition
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = @SchemaName
              AND TABLE_NAME = @ViewName
            ORDER BY ORDINAL_POSITION";

        var columns = await connection.QueryAsync<ViewColumn>(sql, new { SchemaName = schemaName, ViewName = viewName });
        return columns.ToList();
    }

    private static ViewType DetermineViewType(string viewName)
    {
        var lowerName = viewName.ToLowerInvariant();

        // Check for MN prefix (Manual views)
        if (lowerName.StartsWith("mn", StringComparison.OrdinalIgnoreCase))
        {
            return ViewType.Manual;
        }

        // Check for ComboList prefix
        if (lowerName.StartsWith("ccvwcombolist_", StringComparison.OrdinalIgnoreCase))
        {
            return ViewType.ComboList;
        }

        // Unknown pattern
        return ViewType.Other;
    }
}
