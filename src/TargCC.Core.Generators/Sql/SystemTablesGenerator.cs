// <copyright file="SystemTablesGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TargCC.Core.Generators.Sql
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Generates SQL scripts for creating system tables required by TargCC.
    /// </summary>
    public class SystemTablesGenerator
    {
        private static readonly Action<ILogger, Exception?> LogGeneratingSystemTables =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(1, nameof(LogGeneratingSystemTables)),
                "Generating system tables script");

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemTablesGenerator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public SystemTablesGenerator(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates the stored procedure for adding audit records.
        /// </summary>
        /// <returns>SQL script for the audit SP.</returns>
        public static Task<string> GenerateAuditStoredProcedureAsync()
        {
            var sb = new StringBuilder();

            sb.AppendLine("-- =========================================");
            sb.AppendLine("-- Stored Procedure for Audit");
            sb.AppendLine(CultureInfo.InvariantCulture, $"-- Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine("-- =========================================");
            sb.AppendLine();

            sb.AppendLine("IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'c__SystemAuditAdd')");
            sb.AppendLine("    DROP PROCEDURE [dbo].[c__SystemAuditAdd]");
            sb.AppendLine("GO");
            sb.AppendLine();

            sb.AppendLine(@"CREATE PROCEDURE [dbo].[c__SystemAuditAdd]
    @TableName NVARCHAR(128),
    @RecordID INT = NULL,
    @ActionType CHAR(1),
    @UserID INT = NULL,
    @UserName NVARCHAR(100) = NULL,
    @OldValues NVARCHAR(MAX) = NULL,
    @NewValues NVARCHAR(MAX) = NULL,
    @ColumnName NVARCHAR(128) = NULL,
    @OldValue NVARCHAR(MAX) = NULL,
    @NewValue NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[c_SystemAudit] (
        [TableName],
        [RecordID],
        [ActionType],
        [UserID],
        [UserName],
        [OldValues],
        [NewValues],
        [ColumnName],
        [OldValue],
        [NewValue]
    )
    VALUES (
        @TableName,
        @RecordID,
        @ActionType,
        @UserID,
        @UserName,
        @OldValues,
        @NewValues,
        @ColumnName,
        @OldValue,
        @NewValue
    );

    -- Return the new audit ID
    SELECT SCOPE_IDENTITY() AS AuditID;
END");
            sb.AppendLine("GO");
            sb.AppendLine();

            return Task.FromResult(sb.ToString());
        }

        /// <summary>
        /// Generates SQL script for creating all system tables.
        /// </summary>
        /// <param name="checkExists">If true, adds IF NOT EXISTS checks.</param>
        /// <returns>SQL script for creating system tables.</returns>
        public Task<string> GenerateAsync(bool checkExists = true)
        {
            LogGeneratingSystemTables(_logger, null);

            var sb = new StringBuilder();

            sb.AppendLine("-- =========================================");
            sb.AppendLine("-- System Tables for TargCC");
            sb.AppendLine(CultureInfo.InvariantCulture, $"-- Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine("-- =========================================");
            sb.AppendLine();

            // c_Enumeration
            GenerateEnumerationTable(sb, checkExists);

            // c_SystemAudit
            GenerateSystemAuditTable(sb, checkExists);

            // c_Lookup
            GenerateLookupTable(sb, checkExists);

            // c_User
            GenerateUserTable(sb, checkExists);

            // c_Role
            GenerateRoleTable(sb, checkExists);

            // c_UserRole
            GenerateUserRoleTable(sb, checkExists);

            // c_Process
            GenerateProcessTable(sb, checkExists);

            // c_Permission
            GeneratePermissionTable(sb, checkExists);

            return Task.FromResult(sb.ToString());
        }

        private static void GenerateEnumerationTable(StringBuilder sb, bool checkExists)
        {
            sb.AppendLine("-- c_Enumeration: Stores enum types and values for code generation");
            if (checkExists)
            {
                sb.AppendLine("IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'c_Enumeration')");
                sb.AppendLine("BEGIN");
            }

            sb.AppendLine(@"    CREATE TABLE [dbo].[c_Enumeration] (
        [ID] INT IDENTITY(1,1) NOT NULL,
        [EnumType] NVARCHAR(100) NOT NULL,
        [EnumValue] NVARCHAR(100) NOT NULL,
        [EnumText] NVARCHAR(200) NULL,
        [EnumTextNS] NVARCHAR(200) NULL,
        [Description] NVARCHAR(500) NULL,
        [OrderNum] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [AddedOn] DATETIME NOT NULL DEFAULT GETDATE(),
        [AddedBy] INT NULL,
        [ChangedOn] DATETIME NULL,
        [ChangedBy] INT NULL,
        CONSTRAINT [PK_c_Enumeration] PRIMARY KEY CLUSTERED ([ID]),
        CONSTRAINT [UQ_c_Enumeration_Type_Value] UNIQUE ([EnumType], [EnumValue])
    )");

            if (checkExists)
            {
                sb.AppendLine("END");
            }

            sb.AppendLine("GO");
            sb.AppendLine();
        }

        private static void GenerateSystemAuditTable(StringBuilder sb, bool checkExists)
        {
            sb.AppendLine("-- c_SystemAudit: Audit trail for data changes (used by AuditCommon.dll)");
            if (checkExists)
            {
                sb.AppendLine("IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'c_SystemAudit')");
                sb.AppendLine("BEGIN");
            }

            sb.AppendLine(@"    CREATE TABLE [dbo].[c_SystemAudit] (
        [ID] BIGINT IDENTITY(1,1) NOT NULL,
        [TableName] NVARCHAR(128) NOT NULL,
        [RecordID] INT NULL,
        [ActionType] CHAR(1) NOT NULL, -- I=Insert, U=Update, D=Delete
        [ActionDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [UserID] INT NULL,
        [UserName] NVARCHAR(100) NULL,
        [OldValues] NVARCHAR(MAX) NULL,
        [NewValues] NVARCHAR(MAX) NULL,
        [ColumnName] NVARCHAR(128) NULL,
        [OldValue] NVARCHAR(MAX) NULL,
        [NewValue] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_c_SystemAudit] PRIMARY KEY CLUSTERED ([ID])
    )");

            if (checkExists)
            {
                sb.AppendLine("END");
            }

            sb.AppendLine("GO");
            sb.AppendLine();

            // Create index for common queries
            sb.AppendLine("-- Index for querying by table and record");
            sb.AppendLine("IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_c_SystemAudit_TableRecord')");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    CREATE NONCLUSTERED INDEX [IX_c_SystemAudit_TableRecord]");
            sb.AppendLine("    ON [dbo].[c_SystemAudit] ([TableName], [RecordID])");
            sb.AppendLine("    INCLUDE ([ActionType], [ActionDate], [UserID])");
            sb.AppendLine("END");
            sb.AppendLine("GO");
            sb.AppendLine();
        }

        private static void GenerateLookupTable(StringBuilder sb, bool checkExists)
        {
            sb.AppendLine("-- c_Lookup: Dynamic lookup values");
            if (checkExists)
            {
                sb.AppendLine("IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'c_Lookup')");
                sb.AppendLine("BEGIN");
            }

            sb.AppendLine(@"    CREATE TABLE [dbo].[c_Lookup] (
        [ID] INT IDENTITY(1,1) NOT NULL,
        [LookupType] NVARCHAR(100) NOT NULL,
        [LookupKey] NVARCHAR(100) NOT NULL,
        [LookupValue] NVARCHAR(500) NULL,
        [Description] NVARCHAR(500) NULL,
        [OrderNum] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [AddedOn] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_c_Lookup] PRIMARY KEY CLUSTERED ([ID]),
        CONSTRAINT [UQ_c_Lookup_Type_Key] UNIQUE ([LookupType], [LookupKey])
    )");

            if (checkExists)
            {
                sb.AppendLine("END");
            }

            sb.AppendLine("GO");
            sb.AppendLine();
        }

        private static void GenerateUserTable(StringBuilder sb, bool checkExists)
        {
            sb.AppendLine("-- c_User: User accounts");
            if (checkExists)
            {
                sb.AppendLine("IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'c_User')");
                sb.AppendLine("BEGIN");
            }

            sb.AppendLine(@"    CREATE TABLE [dbo].[c_User] (
        [ID] INT IDENTITY(1,1) NOT NULL,
        [UserName] NVARCHAR(100) NOT NULL,
        [PasswordHash] NVARCHAR(256) NULL,
        [Email] NVARCHAR(200) NULL,
        [FirstName] NVARCHAR(100) NULL,
        [LastName] NVARCHAR(100) NULL,
        [DisplayName] NVARCHAR(200) NULL,
        [DomainUser] NVARCHAR(200) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsLocked] BIT NOT NULL DEFAULT 0,
        [LastLoginDate] DATETIME NULL,
        [FailedLoginAttempts] INT NOT NULL DEFAULT 0,
        [AddedOn] DATETIME NOT NULL DEFAULT GETDATE(),
        [AddedBy] INT NULL,
        [ChangedOn] DATETIME NULL,
        [ChangedBy] INT NULL,
        CONSTRAINT [PK_c_User] PRIMARY KEY CLUSTERED ([ID]),
        CONSTRAINT [UQ_c_User_UserName] UNIQUE ([UserName])
    )");

            if (checkExists)
            {
                sb.AppendLine("END");
            }

            sb.AppendLine("GO");
            sb.AppendLine();
        }

        private static void GenerateRoleTable(StringBuilder sb, bool checkExists)
        {
            sb.AppendLine("-- c_Role: User roles for permissions");
            if (checkExists)
            {
                sb.AppendLine("IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'c_Role')");
                sb.AppendLine("BEGIN");
            }

            sb.AppendLine(@"    CREATE TABLE [dbo].[c_Role] (
        [ID] INT IDENTITY(1,1) NOT NULL,
        [RoleName] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [AddedOn] DATETIME NOT NULL DEFAULT GETDATE(),
        [AddedBy] INT NULL,
        [ChangedOn] DATETIME NULL,
        [ChangedBy] INT NULL,
        CONSTRAINT [PK_c_Role] PRIMARY KEY CLUSTERED ([ID]),
        CONSTRAINT [UQ_c_Role_RoleName] UNIQUE ([RoleName])
    )");

            if (checkExists)
            {
                sb.AppendLine("END");
            }

            sb.AppendLine("GO");
            sb.AppendLine();
        }

        private static void GenerateUserRoleTable(StringBuilder sb, bool checkExists)
        {
            sb.AppendLine("-- c_UserRole: Many-to-many relationship between users and roles");
            if (checkExists)
            {
                sb.AppendLine("IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'c_UserRole')");
                sb.AppendLine("BEGIN");
            }

            sb.AppendLine(@"    CREATE TABLE [dbo].[c_UserRole] (
        [ID] INT IDENTITY(1,1) NOT NULL,
        [UserID] INT NOT NULL,
        [RoleID] INT NOT NULL,
        [AddedOn] DATETIME NOT NULL DEFAULT GETDATE(),
        [AddedBy] INT NULL,
        CONSTRAINT [PK_c_UserRole] PRIMARY KEY CLUSTERED ([ID]),
        CONSTRAINT [UQ_c_UserRole_User_Role] UNIQUE ([UserID], [RoleID]),
        CONSTRAINT [FK_c_UserRole_User] FOREIGN KEY ([UserID]) REFERENCES [dbo].[c_User]([ID]),
        CONSTRAINT [FK_c_UserRole_Role] FOREIGN KEY ([RoleID]) REFERENCES [dbo].[c_Role]([ID])
    )");

            if (checkExists)
            {
                sb.AppendLine("END");
            }

            sb.AppendLine("GO");
            sb.AppendLine();
        }

        private static void GenerateProcessTable(StringBuilder sb, bool checkExists)
        {
            sb.AppendLine("-- c_Process: List of all processes/functions in the system");
            if (checkExists)
            {
                sb.AppendLine("IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'c_Process')");
                sb.AppendLine("BEGIN");
            }

            sb.AppendLine(@"    CREATE TABLE [dbo].[c_Process] (
        [ID] INT IDENTITY(1,1) NOT NULL,
        [ProcessName] NVARCHAR(100) NOT NULL,
        [DisplayName] NVARCHAR(200) NULL,
        [Description] NVARCHAR(500) NULL,
        [Category] NVARCHAR(100) NULL,
        [ParentProcessID] INT NULL,
        [OrderNum] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [AddedOn] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_c_Process] PRIMARY KEY CLUSTERED ([ID]),
        CONSTRAINT [UQ_c_Process_ProcessName] UNIQUE ([ProcessName])
    )");

            if (checkExists)
            {
                sb.AppendLine("END");
            }

            sb.AppendLine("GO");
            sb.AppendLine();
        }

        private static void GeneratePermissionTable(StringBuilder sb, bool checkExists)
        {
            sb.AppendLine("-- c_Permission: Role permissions for each process");
            if (checkExists)
            {
                sb.AppendLine("IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'c_Permission')");
                sb.AppendLine("BEGIN");
            }

            sb.AppendLine(@"    CREATE TABLE [dbo].[c_Permission] (
        [ID] INT IDENTITY(1,1) NOT NULL,
        [RoleID] INT NOT NULL,
        [ProcessID] INT NOT NULL,
        [CanView] BIT NOT NULL DEFAULT 0,
        [CanAdd] BIT NOT NULL DEFAULT 0,
        [CanEdit] BIT NOT NULL DEFAULT 0,
        [CanDelete] BIT NOT NULL DEFAULT 0,
        [AddedOn] DATETIME NOT NULL DEFAULT GETDATE(),
        [AddedBy] INT NULL,
        [ChangedOn] DATETIME NULL,
        [ChangedBy] INT NULL,
        CONSTRAINT [PK_c_Permission] PRIMARY KEY CLUSTERED ([ID]),
        CONSTRAINT [UQ_c_Permission_Role_Process] UNIQUE ([RoleID], [ProcessID]),
        CONSTRAINT [FK_c_Permission_Role] FOREIGN KEY ([RoleID]) REFERENCES [dbo].[c_Role]([ID]),
        CONSTRAINT [FK_c_Permission_Process] FOREIGN KEY ([ProcessID]) REFERENCES [dbo].[c_Process]([ID])
    )");

            if (checkExists)
            {
                sb.AppendLine("END");
            }

            sb.AppendLine("GO");
            sb.AppendLine();
        }
    }
}
