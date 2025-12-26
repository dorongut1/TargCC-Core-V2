// <copyright file="AuditTriggerGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.Sql
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using TargCC.Core.Interfaces.Models;

    /// <summary>
    /// Generates audit triggers that work with the AuditCommon.dll CLR assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This generator creates SQL triggers that use the existing AuditCommon.dll CLR assembly.
    /// It does NOT recreate the CLR assembly - that should be deployed separately.
    /// </para>
    /// <para>
    /// Audit levels:
    /// </para>
    /// <list type="bullet">
    /// <item><term>0 (None)</term><description>No audit trigger generated</description></item>
    /// <item><term>1 (TrackChanges)</term><description>Track changes via application code only</description></item>
    /// <item><term>2 (FullAudit)</term><description>Full audit with CLR triggers</description></item>
    /// </list>
    /// </remarks>
    public class AuditTriggerGenerator
    {
        private static readonly Action<ILogger, string, Exception?> LogGeneratingTrigger =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1, nameof(LogGeneratingTrigger)),
                "Generating audit trigger for table: {TableName}");

        private static readonly Action<ILogger, string, Exception?> LogSkippingTable =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(2, nameof(LogSkippingTable)),
                "Skipping audit trigger for table {TableName} - audit level is 0 or 1");

        private static readonly Action<ILogger, Exception?> LogGeneratingSetupScript =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(3, nameof(LogGeneratingSetupScript)),
                "Generating audit setup verification script");

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditTriggerGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public AuditTriggerGenerator(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates an audit trigger for a table if the audit level requires it.
        /// </summary>
        /// <param name="table">Table to generate audit trigger for.</param>
        /// <returns>SQL script for creating the audit trigger, or null if not needed.</returns>
        public Task<string?> GenerateAuditTriggerAsync(Table table)
        {
            ArgumentNullException.ThrowIfNull(table);

            // Only generate triggers for AuditLevel 2 (Full Audit)
            if (table.AuditLevel < 2)
            {
                LogSkippingTable(_logger, table.Name, null);
                return Task.FromResult<string?>(null);
            }

            // Skip views - they can't have triggers
            if (table.IsView)
            {
                return Task.FromResult<string?>(null);
            }

            LogGeneratingTrigger(_logger, table.Name, null);

            var sql = GenerateTriggerScript(table);
            return Task.FromResult<string?>(sql);
        }

        /// <summary>
        /// Generates a script to verify AuditCommon CLR assembly is registered.
        /// </summary>
        /// <returns>SQL script to check CLR assembly registration.</returns>
        public Task<string> GenerateAuditSetupScriptAsync()
        {
            LogGeneratingSetupScript(_logger, null);

            var sb = new StringBuilder();

            sb.AppendLine("-- =========================================");
            sb.AppendLine("-- AuditCommon CLR Assembly Verification");
            sb.AppendLine(CultureInfo.InvariantCulture, $"-- Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine("-- =========================================");
            sb.AppendLine();
            sb.AppendLine("-- Check if AuditCommon CLR is registered");
            sb.AppendLine("IF NOT EXISTS (SELECT * FROM sys.assemblies WHERE name = 'AuditCommon')");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    PRINT '========================================='");
            sb.AppendLine("    PRINT 'ERROR: AuditCommon CLR Assembly not registered!'");
            sb.AppendLine("    PRINT '========================================='");
            sb.AppendLine("    PRINT ''");
            sb.AppendLine("    PRINT 'Please register using:'");
            sb.AppendLine("    PRINT 'CREATE ASSEMBLY [AuditCommon] FROM ''path\\to\\AuditCommon.dll'' WITH PERMISSION_SET = SAFE'");
            sb.AppendLine("    PRINT ''");
            sb.AppendLine("    PRINT 'Make sure CLR is enabled:'");
            sb.AppendLine("    PRINT 'EXEC sp_configure ''clr enabled'', 1;'");
            sb.AppendLine("    PRINT 'RECONFIGURE;'");
            sb.AppendLine("END");
            sb.AppendLine("ELSE");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    PRINT 'AuditCommon CLR Assembly is registered.'");
            sb.AppendLine("END");
            sb.AppendLine("GO");
            sb.AppendLine();
            sb.AppendLine("-- Check if c_SystemAudit table exists");
            sb.AppendLine("IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'c_SystemAudit')");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    PRINT 'WARNING: c_SystemAudit table does not exist!'");
            sb.AppendLine("    PRINT 'Please run the system tables script first.'");
            sb.AppendLine("END");
            sb.AppendLine("ELSE");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    PRINT 'c_SystemAudit table exists.'");
            sb.AppendLine("END");
            sb.AppendLine("GO");

            return Task.FromResult(sb.ToString());
        }

        private static string GenerateTriggerScript(Table table)
        {
            var schemaName = string.IsNullOrEmpty(table.SchemaName) ? "dbo" : table.SchemaName;
            var triggerName = $"trc_{table.Name}_Audit";

            var sb = new StringBuilder();

            sb.AppendLine(CultureInfo.InvariantCulture, $"-- Audit Trigger for {table.Name}");
            sb.AppendLine("-- Requires AuditCommon.dll CLR Assembly to be registered");
            sb.AppendLine();

            // Drop existing trigger if exists
            sb.AppendLine(CultureInfo.InvariantCulture, $"IF EXISTS (SELECT * FROM sys.triggers WHERE name = '{triggerName}')");
            sb.AppendLine(CultureInfo.InvariantCulture, $"    DROP TRIGGER [{schemaName}].[{triggerName}]");
            sb.AppendLine("GO");
            sb.AppendLine();

            // Create the trigger
            sb.AppendLine(CultureInfo.InvariantCulture, $"CREATE TRIGGER [{schemaName}].[{triggerName}]");
            sb.AppendLine(CultureInfo.InvariantCulture, $"ON [{schemaName}].[{table.Name}]");
            sb.AppendLine("FOR INSERT, UPDATE, DELETE");
            sb.AppendLine("AS EXTERNAL NAME [AuditCommon].[AuditCommon.Triggers].[AuditCommon]");
            sb.AppendLine("GO");

            return sb.ToString();
        }
    }
}
