// <copyright file="SqlGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.Sql
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using TargCC.Application.Common.Models;
    using TargCC.Core.Generators.Sql.Templates;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Main SQL generator that orchestrates generation of all stored procedures for a table.
    /// </summary>
    public class SqlGenerator : ISqlGenerator
    {
        private static readonly Action<ILogger, string, Exception?> LogGeneratingTableProcedures =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1, nameof(LogGeneratingTableProcedures)),
                "Generating stored procedures for table: {TableName}");

        private static readonly Action<ILogger, string, Exception?> LogGetByIdGenerationWarning =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(2, nameof(LogGetByIdGenerationWarning)),
                "Could not generate GetByID procedure for {TableName}");

        private static readonly Action<ILogger, string, Exception?> LogAddGenerationWarning =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(10, nameof(LogAddGenerationWarning)),
                "Could not generate Add procedure for {TableName}");

        private static readonly Action<ILogger, string, Exception?> LogUpdateGenerationWarning =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(3, nameof(LogUpdateGenerationWarning)),
                "Could not generate Update procedure for {TableName}");

        private static readonly Action<ILogger, string, Exception?> LogDeleteGenerationWarning =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(4, nameof(LogDeleteGenerationWarning)),
                "Could not generate Delete procedure for {TableName}");

        private static readonly Action<ILogger, string, Exception?> LogIndexProceduresWarning =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(5, nameof(LogIndexProceduresWarning)),
                "Could not generate index procedures for {TableName}");

        private static readonly Action<ILogger, string, Exception?> LogCompletedTableGeneration =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(6, nameof(LogCompletedTableGeneration)),
                "Completed stored procedure generation for {TableName}");

        private static readonly Action<ILogger, string, Exception?> LogGeneratingSchemaProcedures =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(7, nameof(LogGeneratingSchemaProcedures)),
                "Generating stored procedures for schema: {SchemaName}");

        private static readonly Action<ILogger, string, Exception?> LogGetAllGenerationWarning =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(8, nameof(LogGetAllGenerationWarning)),
                "Could not generate GetAll procedure for {TableName}");

        private static readonly Action<ILogger, string, Exception?> LogGetFilteredGenerationWarning =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(9, nameof(LogGetFilteredGenerationWarning)),
                "Could not generate GetFiltered procedure for {TableName}");

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
            return table.Columns != null && table.Columns.Count != 0;
        }

        /// <inheritdoc/>
        public async Task<string> GenerateAsync(Table table)
        {
            ArgumentNullException.ThrowIfNull(table);

            LogGeneratingTableProcedures(_logger, table.Name, null);

            var sb = new StringBuilder();

            // Header
            sb.AppendLine($"-- =========================================");
            sb.AppendLine(CultureInfo.InvariantCulture, $"-- Stored Procedures for Table: {table.Name}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"-- Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine($"-- =========================================");
            sb.AppendLine();

            // GetAll
            try
            {
                var getAllSql = await SpGetAllTemplate.GenerateAsync(table);
                sb.AppendLine(getAllSql);
                sb.AppendLine("GO");
                sb.AppendLine();
            }
            catch (Exception ex)
            {
                LogGetAllGenerationWarning(_logger, table.Name, ex);
            }

            // GetFiltered (based on indexes)
            try
            {
                var getFilteredSql = await SpGetFilteredTemplate.GenerateAsync(table);
                if (!string.IsNullOrWhiteSpace(getFilteredSql))
                {
                    sb.AppendLine(getFilteredSql);
                    sb.AppendLine("GO");
                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                LogGetFilteredGenerationWarning(_logger, table.Name, ex);
            }

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
                LogGetByIdGenerationWarning(_logger, table.Name, ex);
            }

            // Add
            try
            {
                var addSql = await SpAddTemplate.GenerateAsync(table);
                sb.AppendLine(addSql);
                sb.AppendLine("GO");
                sb.AppendLine();
            }
            catch (Exception ex)
            {
                LogAddGenerationWarning(_logger, table.Name, ex);
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
                LogUpdateGenerationWarning(_logger, table.Name, ex);
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
                LogDeleteGenerationWarning(_logger, table.Name, ex);
            }

            // Index procedures
            if (_includeAdvancedProcedures && table.Indexes != null && table.Indexes.Count > 0)
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
                    LogIndexProceduresWarning(_logger, table.Name, ex);
                }
            }

            LogCompletedTableGeneration(_logger, table.Name, null);

            return sb.ToString();
        }

        /// <inheritdoc/>
        public async Task<string> GenerateAsync(DatabaseSchema schema)
        {
            ArgumentNullException.ThrowIfNull(schema);

            LogGeneratingSchemaProcedures(_logger, schema.DatabaseName, null);

            var sb = new StringBuilder();

            sb.AppendLine($"-- =========================================");
            sb.AppendLine(CultureInfo.InvariantCulture, $"-- Stored Procedures for Database Schema: {schema.DatabaseName}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"-- Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine($"-- =========================================");
            sb.AppendLine();

            var validTables = schema.Tables
                .Where(CanGenerate)
                .OrderBy(t => t.Name);

            foreach (var table in validTables)
            {
                var tableSql = await GenerateAsync(table);
                sb.AppendLine(tableSql);
                sb.AppendLine();
            }

            // Generate FK relationship procedures (Master-Detail Views)
            if (_includeAdvancedProcedures && schema.Relationships != null && schema.Relationships.Count > 0)
            {
                sb.AppendLine("-- =========================================");
                sb.AppendLine("-- Foreign Key Relationship Procedures");
                sb.AppendLine("-- (Master-Detail Views)");
                sb.AppendLine("-- =========================================");
                sb.AppendLine();

                foreach (var table in validTables)
                {
                    try
                    {
                        var relatedSql = await SpGetRelatedTemplate.GenerateAllRelatedProcedures(table, schema);
                        if (!string.IsNullOrWhiteSpace(relatedSql))
                        {
                            sb.AppendLine(relatedSql);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not generate related procedures for table: {TableName}", table.Name);
                    }
                }
            }

            return sb.ToString();
        }
    }
}
