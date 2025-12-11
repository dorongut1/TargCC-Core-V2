-- =============================================================================
-- TargCC V2 - Job Scheduler Tables Migration
-- Version: 2.1.0
-- Date: 2025-12-10
-- Description: Creates tables for Hangfire-based job scheduling system
-- =============================================================================

-- Check if script has already been run
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'c_LoggedJob' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Job tables already exist. Skipping creation.'
    RETURN
END

PRINT 'Starting TargCC Job Tables creation...'
GO

-- =============================================================================
-- 1. c_LoggedJob - Job execution history
-- =============================================================================

PRINT 'Creating table: c_LoggedJob'
GO

CREATE TABLE [dbo].[c_LoggedJob] (
    -- Identity
    [ID] BIGINT IDENTITY(1,1) NOT NULL,

    -- Job Identity
    [JobName] NVARCHAR(100) NOT NULL,
    [JobId] NVARCHAR(100) NULL,              -- Hangfire Job ID
    [JobType] NVARCHAR(50) NOT NULL,         -- Recurring, FireAndForget, Delayed, Manual

    -- Execution
    [StartedAt] DATETIME2 NOT NULL,
    [CompletedAt] DATETIME2 NULL,
    [DurationMs] INT NULL,

    -- Result
    [Success] BIT NOT NULL,
    [ResultMessage] NVARCHAR(MAX) NULL,
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [StackTrace] NVARCHAR(MAX) NULL,

    -- Context
    [ServerName] NVARCHAR(100) NULL,
    [TriggeredBy] NVARCHAR(100) NULL,        -- System, Manual, API, User

    -- Performance Metrics
    [MemoryUsedMB] INT NULL,
    [CpuTimeMs] INT NULL,

    -- Metadata (JSON)
    [MetadataJson] NVARCHAR(MAX) NULL,

    -- Audit
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [PK_c_LoggedJob] PRIMARY KEY CLUSTERED ([ID] ASC)
)
GO

-- Indexes for c_LoggedJob
CREATE NONCLUSTERED INDEX [IX_c_LoggedJob_JobName_StartedAt]
    ON [dbo].[c_LoggedJob]([JobName], [StartedAt] DESC)
GO

CREATE NONCLUSTERED INDEX [IX_c_LoggedJob_Success_StartedAt]
    ON [dbo].[c_LoggedJob]([Success], [StartedAt] DESC)
    WHERE [Success] = 0  -- Index only failed jobs for faster queries
GO

CREATE NONCLUSTERED INDEX [IX_c_LoggedJob_StartedAt]
    ON [dbo].[c_LoggedJob]([StartedAt] DESC)
GO

CREATE NONCLUSTERED INDEX [IX_c_LoggedJob_JobId]
    ON [dbo].[c_LoggedJob]([JobId])
    WHERE [JobId] IS NOT NULL
GO

PRINT 'Created table: c_LoggedJob with indexes'
GO

-- =============================================================================
-- 2. c_JobAlert - Alert configuration for jobs
-- =============================================================================

PRINT 'Creating table: c_JobAlert'
GO

CREATE TABLE [dbo].[c_JobAlert] (
    -- Identity
    [ID] INT IDENTITY(1,1) NOT NULL,
    [JobName] NVARCHAR(100) NOT NULL,

    -- Alert Configuration
    [AlertOnFailure] BIT NOT NULL DEFAULT 1,
    [AlertOnSuccess] BIT NOT NULL DEFAULT 0,
    [AlertOnDurationExceeds] INT NULL,       -- Milliseconds - alert if job takes longer

    -- Email Recipients
    [EmailRecipients] NVARCHAR(500) NULL,    -- Comma-separated email addresses

    -- Webhook URLs
    [SlackWebhook] NVARCHAR(500) NULL,
    [TeamsWebhook] NVARCHAR(500) NULL,
    [CustomWebhook] NVARCHAR(500) NULL,

    -- Throttling (prevent alert spam)
    [MaxAlertsPerHour] INT NULL DEFAULT 5,
    [LastAlertSent] DATETIME2 NULL,
    [AlertCount] INT NOT NULL DEFAULT 0,

    -- Alert Template
    [CustomMessageTemplate] NVARCHAR(MAX) NULL,

    -- System
    [IsActive] BIT NOT NULL DEFAULT 1,

    -- Audit
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [CreatedBy] NVARCHAR(100) NULL,
    [UpdatedBy] NVARCHAR(100) NULL,

    CONSTRAINT [PK_c_JobAlert] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [UQ_c_JobAlert_JobName] UNIQUE ([JobName])
)
GO

-- Indexes for c_JobAlert
CREATE NONCLUSTERED INDEX [IX_c_JobAlert_Active]
    ON [dbo].[c_JobAlert]([IsActive])
    WHERE [IsActive] = 1
GO

CREATE NONCLUSTERED INDEX [IX_c_JobAlert_LastAlertSent]
    ON [dbo].[c_JobAlert]([LastAlertSent])
    WHERE [LastAlertSent] IS NOT NULL
GO

PRINT 'Created table: c_JobAlert with indexes'
GO

-- =============================================================================
-- 3. Helper Stored Procedures
-- =============================================================================

PRINT 'Creating stored procedure: SP_GetJobHistory'
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_GetJobHistory]
    @JobName NVARCHAR(100) = NULL,
    @Limit INT = 50,
    @OnlyFailures BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Limit)
        [ID],
        [JobName],
        [JobId],
        [JobType],
        [StartedAt],
        [CompletedAt],
        [DurationMs],
        [Success],
        [ResultMessage],
        [ErrorMessage],
        [StackTrace],
        [ServerName],
        [TriggeredBy],
        [MemoryUsedMB],
        [CpuTimeMs],
        [MetadataJson],
        [CreatedAt]
    FROM [dbo].[c_LoggedJob]
    WHERE
        (@JobName IS NULL OR [JobName] = @JobName)
        AND (@OnlyFailures = 0 OR [Success] = 0)
    ORDER BY [StartedAt] DESC
END
GO

PRINT 'Created stored procedure: SP_GetJobHistory'
GO

PRINT 'Creating stored procedure: SP_GetJobStatistics'
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_GetJobStatistics]
    @JobName NVARCHAR(100) = NULL,
    @DaysBack INT = 7
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATETIME2 = DATEADD(DAY, -@DaysBack, GETDATE());

    SELECT
        [JobName],
        COUNT(*) AS [TotalExecutions],
        SUM(CASE WHEN [Success] = 1 THEN 1 ELSE 0 END) AS [SuccessCount],
        SUM(CASE WHEN [Success] = 0 THEN 1 ELSE 0 END) AS [FailureCount],
        AVG([DurationMs]) AS [AvgDurationMs],
        MIN([DurationMs]) AS [MinDurationMs],
        MAX([DurationMs]) AS [MaxDurationMs],
        MAX([StartedAt]) AS [LastExecution],
        CAST((SUM(CASE WHEN [Success] = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*)) AS DECIMAL(5,2)) AS [SuccessRate]
    FROM [dbo].[c_LoggedJob]
    WHERE
        [StartedAt] >= @StartDate
        AND (@JobName IS NULL OR [JobName] = @JobName)
    GROUP BY [JobName]
    ORDER BY [JobName]
END
GO

PRINT 'Created stored procedure: SP_GetJobStatistics'
GO

-- =============================================================================
-- 4. Sample Data (optional - for development/testing)
-- =============================================================================

-- Uncomment to insert sample alert configuration
/*
INSERT INTO [dbo].[c_JobAlert] (
    [JobName],
    [AlertOnFailure],
    [AlertOnSuccess],
    [AlertOnDurationExceeds],
    [EmailRecipients],
    [MaxAlertsPerHour],
    [IsActive]
)
VALUES
    ('SampleDailyJob', 1, 0, 300000, 'admin@example.com', 5, 1),  -- Alert if fails or takes > 5 minutes
    ('SampleManualJob', 1, 1, NULL, 'admin@example.com', 10, 1);  -- Alert on both success and failure
*/

-- =============================================================================
-- Summary
-- =============================================================================

PRINT ''
PRINT '========================================='
PRINT 'TargCC Job Tables Migration Complete!'
PRINT '========================================='
PRINT ''
PRINT 'Created tables:'
PRINT '  1. c_LoggedJob       - Job execution history'
PRINT '  2. c_JobAlert        - Alert configuration'
PRINT ''
PRINT 'Created indexes: 7'
PRINT 'Created stored procedures: 2'
PRINT '  - SP_GetJobHistory'
PRINT '  - SP_GetJobStatistics'
PRINT ''
PRINT 'Next steps:'
PRINT '  - Configure Hangfire in your application'
PRINT '  - Add job alert configurations to c_JobAlert'
PRINT '  - Start creating your background jobs!'
PRINT ''
GO
