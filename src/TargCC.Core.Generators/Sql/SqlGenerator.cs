// <copyright file="SqlGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.Sql
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Generators.Sql.Templates;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Main SQL generator that orchestrates generation of all stored procedures for a table.
    /// </summary>
    public class SqlGenerator : ISqlGenerator
    {
        private readonly ILogger _logger;
        private readonly bool _includeAdvancedProcedures;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="includeAdvancedProcedures">Whether to include advanced stored procedures.</param>
        public SqlGenerator(ILogger logger, bool includeAdvancedProcedures = true)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _includeAdvancedProcedures = includeAdvancedProcedures;
        }

        /// <inheritdoc/>
        public string Name => "SQL Stored Procedures Generator";

        /// <inheritdoc/>
        public string SqlObjectType => "StoredProcedure";

        /// <inheritdoc/>
        public bool CanGenerate(Table table)
        {
            if (table == null)
            {
                return false;
            }

            // Must have at least one column
            return table.Columns != null && table.Columns.Any();
        }

        /// <inheritdoc/>
        public async Task<string> GenerateAsync(Table table)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            _logger.LogInformation("Generating stored procedures for table: {TableName}", table.Name);

            var sb = new StringBuilder();

            // Header
            sb.AppendLine($"-- =========================================");
            sb.AppendLine($"-- Stored Procedures for Table: {table.Name}");
            sb.AppendLine($"-- Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine($"-- =========================================");
            sb.AppendLine();

            // GetByID
            try
            {
                var getByIdSql = await SpGetByIdTemplate.GenerateAsync(table);
                sb.AppendLine(getByIdSql);
                sb.AppendLine("GO");
                sb.AppendLine();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Could not generate GetByID procedure for {TableName}", table.Name);
            }

            // Update
            try
            {
                var updateSql = await SpUpdateTemplate.GenerateAsync(table);
                sb.AppendLine(updateSql);
                sb.AppendLine("GO");
                sb.AppendLine();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not generate Update procedure for {TableName}", table.Name);
            }

            // Delete
            try
            {
                var deleteSql = await SpDeleteTemplate.GenerateAsync(table);
                sb.AppendLine(deleteSql);
                sb.AppendLine("GO");
                sb.AppendLine();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not generate Delete procedure for {TableName}", table.Name);
            }

            // Index procedures
            if (_includeAdvancedProcedures && table.Indexes != null && table.Indexes.Any())
            {
                try
                {
                    var indexSql = await SpGetByIndexTemplate.GenerateAllIndexProcedures(table);
                    if (!string.IsNullOrWhiteSpace(indexSql))
                    {
                        sb.AppendLine(indexSql);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not generate index procedures for {TableName}", table.Name);
                }
            }

            _logger.LogInformation("Completed stored procedure generation for {TableName}", table.Name);

            return sb.ToString();
        }

        /// <inheritdoc/>
        public async Task<string> GenerateAsync(DatabaseSchema schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            _logger.LogInformation("Generating stored procedures for schema: {SchemaName}", schema.Name);

            var sb = new StringBuilder();

            sb.AppendLine($"-- =========================================");
            sb.AppendLine($"-- Stored Procedures for Database Schema: {schema.Name}");
            sb.AppendLine($"-- Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine($"-- =========================================");
            sb.AppendLine();

            foreach (var table in schema.Tables.OrderBy(t => t.Name))
            {
                if (CanGenerate(table))
                {
                    var tableSql = await GenerateAsync(table);
                    sb.AppendLine(tableSql);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}
