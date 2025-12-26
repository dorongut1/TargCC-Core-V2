// <copyright file="UIGeneratorOrchestrator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.UI
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Orchestrates all UI generators in the correct order.
    /// This is the main entry point for UI generation.
    /// </summary>
    public class UIGeneratorOrchestrator
    {
        private static readonly Action<ILogger, string, Exception?> LogGeneratingUI =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1, nameof(LogGeneratingUI)),
                "Generating UI for table {TableName}");

        private static readonly Action<ILogger, string, Exception?> LogGeneratedTypes =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(2, nameof(LogGeneratedTypes)),
                "Generated TypeScript types for {TableName}");

        private static readonly Action<ILogger, string, Exception?> LogGeneratedApi =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(3, nameof(LogGeneratedApi)),
                "Generated API client for {TableName}");

        private static readonly Action<ILogger, string, Exception?> LogGeneratedHooks =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(4, nameof(LogGeneratedHooks)),
                "Generated React hooks for {TableName}");

        private static readonly Action<ILogger, string, Exception?> LogGeneratedForm =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(5, nameof(LogGeneratedForm)),
                "Generated entity form for {TableName}");

        private static readonly Action<ILogger, string, Exception?> LogGeneratedGrid =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(6, nameof(LogGeneratedGrid)),
                "Generated collection grid for {TableName}");

        private static readonly Action<ILogger, string, Exception?> LogGeneratedPage =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(7, nameof(LogGeneratedPage)),
                "Generated page component for {TableName}");

        private static readonly Action<ILogger, string, int, Exception?> LogSuccessfulGeneration =
            LoggerMessage.Define<string, int>(
                LogLevel.Information,
                new EventId(8, nameof(LogSuccessfulGeneration)),
                "Successfully generated UI for table {TableName} ({FileCount} files)");

        private static readonly Action<ILogger, string, Exception> LogFailedGeneration =
            LoggerMessage.Define<string>(
                LogLevel.Error,
                new EventId(9, nameof(LogFailedGeneration)),
                "Failed to generate UI for table {TableName}");

        private static readonly Action<ILogger, int, Exception?> LogGeneratingAllTables =
            LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(10, nameof(LogGeneratingAllTables)),
                "Generating UI for all tables ({TableCount} tables)");

        private static readonly Action<ILogger, int, int, Exception?> LogGenerationComplete =
            LoggerMessage.Define<int, int>(
                LogLevel.Information,
                new EventId(11, nameof(LogGenerationComplete)),
                "UI generation complete: {SuccessCount}/{TotalCount} tables successful");

        private static readonly Action<ILogger, string, string, Exception?> LogSkippingUIGeneration =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(12, nameof(LogSkippingUIGeneration)),
                "Skipping UI generation for {TableName} - {Reason}");

        private readonly ILogger<UIGeneratorOrchestrator> _logger;

        // TODO: Inject these generators via DI once they're implemented
        // private readonly IUIGenerator _typeGenerator;
        // private readonly IUIGenerator _apiGenerator;
        // private readonly IUIGenerator _hookGenerator;
        // private readonly IUIGenerator _formGenerator;
        // private readonly IUIGenerator _gridGenerator;
        // private readonly IUIGenerator _pageGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIGeneratorOrchestrator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public UIGeneratorOrchestrator(ILogger<UIGeneratorOrchestrator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates all UI code for a single table.
        /// </summary>
        /// <param name="table">Table to generate UI for.</param>
        /// <param name="schema">Complete database schema.</param>
        /// <param name="config">Generator configuration.</param>
        /// <returns>Dictionary of file type to generated code.</returns>
        public async Task<UIGenerationResult> GenerateUIAsync(Table table, DatabaseSchema schema, UIGeneratorConfig config)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(config);

            config.Validate();

            // Skip UI generation for tables that don't need it
            if (!ShouldGenerateUI(table))
            {
                var skipReason = GetSkipReason(table);
                LogSkippingUIGeneration(_logger, table.Name, skipReason, null);

                return await Task.FromResult(new UIGenerationResult
                {
                    TableName = table.Name,
                    Success = true,
                    Skipped = true,
                    SkipReason = skipReason,
                }).ConfigureAwait(false);
            }

            LogGeneratingUI(_logger, table.Name, null);

            var result = new UIGenerationResult
            {
                TableName = table.Name,
            };

            try
            {
                // Step 1: Generate TypeScript Types
                // TODO: Uncomment when TypeScriptTypeGenerator is implemented
                // result.TypesCode = await _typeGenerator.GenerateAsync(table, schema, config);
                LogGeneratedTypes(_logger, table.Name, null);

                // Step 2: Generate API Client (depends on types)
                // TODO: Uncomment when ReactApiGenerator is implemented
                // result.ApiCode = await _apiGenerator.GenerateAsync(table, schema, config);
                LogGeneratedApi(_logger, table.Name, null);

                // Step 3: Generate React Hooks (depends on types + API)
                // TODO: Uncomment when ReactHookGenerator is implemented
                // result.HooksCode = await _hookGenerator.GenerateAsync(table, schema, config);
                LogGeneratedHooks(_logger, table.Name, null);

                // Step 4: Generate Entity Form (depends on types + hooks)
                // TODO: Uncomment when ReactEntityFormGenerator is implemented
                // result.FormCode = await _formGenerator.GenerateAsync(table, schema, config);
                LogGeneratedForm(_logger, table.Name, null);

                // Step 5: Generate Collection Grid (depends on types + hooks)
                // TODO: Uncomment when ReactCollectionGridGenerator is implemented
                // result.GridCode = await _gridGenerator.GenerateAsync(table, schema, config);
                LogGeneratedGrid(_logger, table.Name, null);

                // Step 6: Generate Page (depends on form + grid)
                // TODO: Uncomment when ReactPageGenerator is implemented
                // result.PageCode = await _pageGenerator.GenerateAsync(table, schema, config);
                LogGeneratedPage(_logger, table.Name, null);

                result.Success = true;
                LogSuccessfulGeneration(_logger, table.Name, result.GetFileCount(), null);

                await Task.CompletedTask.ConfigureAwait(false); // TODO: Remove when generators are implemented
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                LogFailedGeneration(_logger, table.Name, ex);
                throw;
            }

            return result;
        }

        /// <summary>
        /// Generates UI code for all tables in the schema.
        /// </summary>
        /// <param name="schema">Database schema.</param>
        /// <param name="config">Generator configuration.</param>
        /// <returns>List of generation results.</returns>
        public async Task<List<UIGenerationResult>> GenerateAllAsync(DatabaseSchema schema, UIGeneratorConfig config)
        {
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(config);

            LogGeneratingAllTables(_logger, schema.Tables.Count, null);

            var results = new List<UIGenerationResult>();

            foreach (var table in schema.Tables)
            {
                try
                {
                    var result = await GenerateUIAsync(table, schema, config).ConfigureAwait(false);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    LogFailedGeneration(_logger, table.Name, ex);

                    // Add failed result
                    results.Add(new UIGenerationResult
                    {
                        TableName = table.Name,
                        Success = false,
                        ErrorMessage = ex.Message,
                    });
                }
            }

            var successCount = results.Count(r => r.Success);
            LogGenerationComplete(_logger, successCount, results.Count, null);

            return results;
        }

        /// <summary>
        /// Determines if UI should be generated for the given table.
        /// </summary>
        /// <param name="table">Table to check.</param>
        /// <returns>True if UI should be generated; otherwise, false.</returns>
        private static bool ShouldGenerateUI(Table table)
        {
            // Skip ComboList views - they are for dropdowns only
            if (table.IsComboListView)
            {
                return false;
            }

            // Skip tables with GenerateUI explicitly set to false
            if (!table.GenerateUI)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the reason why UI generation was skipped for a table.
        /// </summary>
        /// <param name="table">Table that was skipped.</param>
        /// <returns>Skip reason string.</returns>
        private static string GetSkipReason(Table table)
        {
            if (table.IsComboListView)
            {
                return "ComboList view (ccvwComboList_*) - used for dropdowns only";
            }

            if (!table.GenerateUI)
            {
                return "GenerateUI flag is false";
            }

            return "Unknown reason";
        }
    }
}
