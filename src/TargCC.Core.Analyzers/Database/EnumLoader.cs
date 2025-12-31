// <copyright file="EnumLoader.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Analyzers.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Dapper;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Loads enum data from c_Enumeration table.
    /// </summary>
    public class EnumLoader
    {
        private readonly string _connectionString;
        private readonly ILogger<EnumLoader> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumLoader"/> class.
        /// </summary>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="logger">Logger instance.</param>
        public EnumLoader(string connectionString, ILogger<EnumLoader> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Loads all enum records from c_Enumeration table.
        /// </summary>
        /// <returns>List of enum records.</returns>
        public async Task<List<EnumRecord>> LoadAllEnumsAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Check if c_Enumeration table exists
                var tableExists = await CheckTableExistsAsync(connection);
                if (!tableExists)
                {
                    _logger.LogWarning("c_Enumeration table does not exist. No enums will be loaded.");
                    return new List<EnumRecord>();
                }

                var sql = @"
                    SELECT
                        EnumType,
                        EnumValue,
                        locText AS EnumText,
                        REPLACE(REPLACE(REPLACE(locText, ' ', ''), '-', ''), '''', '') AS EnumTextNS,
                        ISNULL(OrdinalPosition, 0) AS OrderNum
                    FROM dbo.c_Enumeration
                    WHERE DeletedOn IS NULL
                    ORDER BY EnumType, ISNULL(OrdinalPosition, 0), EnumValue";

                var enums = await connection.QueryAsync<EnumRecord>(sql);
                var enumList = enums.AsList();

                _logger.LogInformation("Loaded {Count} enum records from c_Enumeration", enumList.Count);

                return enumList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load enums from c_Enumeration table");
                return new List<EnumRecord>();
            }
        }

        /// <summary>
        /// Loads enums for a specific enum type.
        /// </summary>
        /// <param name="enumType">The enum type to load.</param>
        /// <returns>List of enum records for the specified type.</returns>
        public async Task<List<EnumRecord>> LoadEnumsByTypeAsync(string enumType)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(enumType);

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var tableExists = await CheckTableExistsAsync(connection);
                if (!tableExists)
                {
                    _logger.LogWarning("c_Enumeration table does not exist");
                    return new List<EnumRecord>();
                }

                var sql = @"
                    SELECT
                        EnumType,
                        EnumValue,
                        locText AS EnumText,
                        REPLACE(REPLACE(REPLACE(locText, ' ', ''), '-', ''), '''', '') AS EnumTextNS,
                        ISNULL(OrdinalPosition, 0) AS OrderNum
                    FROM dbo.c_Enumeration
                    WHERE EnumType = @EnumType
                      AND DeletedOn IS NULL
                    ORDER BY ISNULL(OrdinalPosition, 0), EnumValue";

                var enums = await connection.QueryAsync<EnumRecord>(sql, new { EnumType = enumType });
                var enumList = enums.AsList();

                _logger.LogDebug("Loaded {Count} values for enum type {EnumType}", enumList.Count, enumType);

                return enumList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load enum type {EnumType}", enumType);
                return new List<EnumRecord>();
            }
        }

        /// <summary>
        /// Gets all distinct enum types from c_Enumeration.
        /// </summary>
        /// <returns>List of enum type names.</returns>
        public async Task<List<string>> GetEnumTypesAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var tableExists = await CheckTableExistsAsync(connection);
                if (!tableExists)
                {
                    return new List<string>();
                }

                var sql = @"
                    SELECT DISTINCT EnumType
                    FROM dbo.c_Enumeration
                    WHERE DeletedOn IS NULL
                    ORDER BY EnumType";

                var enumTypes = await connection.QueryAsync<string>(sql);
                return enumTypes.AsList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get enum types");
                return new List<string>();
            }
        }

        private static async Task<bool> CheckTableExistsAsync(IDbConnection connection)
        {
            var sql = @"
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = 'dbo'
                  AND TABLE_NAME = 'c_Enumeration'";

            var count = await connection.ExecuteScalarAsync<int>(sql);
            return count > 0;
        }
    }
}
