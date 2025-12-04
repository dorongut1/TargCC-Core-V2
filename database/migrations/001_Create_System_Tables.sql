-- =============================================================================
-- TargCC V2 - System Tables Migration
-- Version: 2.0.0
-- Date: 2025-12-02
-- Description: Creates all system tables (c_*) required for TargCC operation
-- =============================================================================

-- Check if script has already been run
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'c_Table' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'System tables already exist. Skipping creation.'
    RETURN
END

PRINT 'Starting TargCC System Tables creation...'
GO

-- =============================================================================
-- 1. c_Table - Main table metadata
-- =============================================================================

PRINT 'Creating table: c_Table'
GO

CREATE TABLE [dbo].[c_Table] (
    -- Identity
    [ID] INT IDENTITY(1,1) NOT NULL,
    [TableName] NVARCHAR(128) NOT NULL,
    [SchemaName] NVARCHAR(128) NOT NULL DEFAULT 'dbo',

    -- Tracking & Change Detection
    [LastGenerated] DATETIME2 NULL,
    [LastModifiedInDB] DATETIME2 NULL,
    [SchemaHash] VARCHAR(64) NULL,
    [SchemaHashPrevious] VARCHAR(64) NULL,

    -- Metadata (Legacy compatible)
    [ccAuditLevel] INT NOT NULL DEFAULT 0,
    [ccUICreateMenu] BIT NOT NULL DEFAULT 1,
    [ccUICreateEntity] BIT NOT NULL DEFAULT 1,
    [ccUICreateCollection] BIT NOT NULL DEFAULT 1,
    [ccIsSingleRow] BIT NOT NULL DEFAULT 0,
    [ccUsedForIdentity] BIT NOT NULL DEFAULT 0,

    -- Generation Options
    [GenerateEntity] BIT NOT NULL DEFAULT 1,
    [GenerateRepository] BIT NOT NULL DEFAULT 1,
    [GenerateController] BIT NOT NULL DEFAULT 1,
    [GenerateReactUI] BIT NOT NULL DEFAULT 0,
    [GenerateStoredProcedures] BIT NOT NULL DEFAULT 1,
    [GenerateCQRS] BIT NOT NULL DEFAULT 1,

    -- System
    [IsSystemTable] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [Notes] NVARCHAR(MAX) NULL,

    -- Audit
    [AddedOn] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [AddedBy] NVARCHAR(100) NULL,
    [ChangedOn] DATETIME2 NULL,
    [ChangedBy] NVARCHAR(100) NULL,

    CONSTRAINT [PK_c_Table] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [UQ_c_Table] UNIQUE ([SchemaName], [TableName])
)
GO

CREATE NONCLUSTERED INDEX [IX_c_Table_LastModified]
    ON [dbo].[c_Table]([LastModifiedInDB])
GO

CREATE NONCLUSTERED INDEX [IX_c_Table_Active]
    ON [dbo].[c_Table]([IsActive])
    WHERE [IsActive] = 1
GO

CREATE NONCLUSTERED INDEX [IX_c_Table_SchemaHash]
    ON [dbo].[c_Table]([SchemaHash])
GO

PRINT 'Created table: c_Table with indexes'
GO

-- =============================================================================
-- 2. c_Column - Column metadata
-- =============================================================================

PRINT 'Creating table: c_Column'
GO

CREATE TABLE [dbo].[c_Column] (
    [ID] INT IDENTITY(1,1) NOT NULL,
    [TableID] INT NOT NULL,
    [ColumnName] NVARCHAR(128) NOT NULL,

    -- Type Information
    [DataType] NVARCHAR(128) NOT NULL,
    [MaxLength] INT NULL,
    [Precision] INT NULL,
    [Scale] INT NULL,
    [IsNullable] BIT NOT NULL DEFAULT 0,
    [DefaultValue] NVARCHAR(MAX) NULL,

    -- Key Information
    [IsPrimaryKey] BIT NOT NULL DEFAULT 0,
    [IsIdentity] BIT NOT NULL DEFAULT 0,
    [IsForeignKey] BIT NOT NULL DEFAULT 0,
    [IsComputed] BIT NOT NULL DEFAULT 0,
    [ReferencedTable] NVARCHAR(128) NULL,
    [ReferencedColumn] NVARCHAR(128) NULL,

    -- TargCC Metadata
    [Prefix] NVARCHAR(10) NULL,
    [OrdinalPosition] INT NOT NULL,
    [ColumnHash] VARCHAR(64) NULL,

    -- Generation
    [IncludeInGeneration] BIT NOT NULL DEFAULT 1,

    -- Audit
    [AddedOn] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [ChangedOn] DATETIME2 NULL,

    CONSTRAINT [PK_c_Column] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_c_Column_Table] FOREIGN KEY ([TableID])
        REFERENCES [dbo].[c_Table]([ID]) ON DELETE CASCADE,
    CONSTRAINT [UQ_c_Column] UNIQUE ([TableID], [ColumnName])
)
GO

CREATE NONCLUSTERED INDEX [IX_c_Column_Table]
    ON [dbo].[c_Column]([TableID])
GO

CREATE NONCLUSTERED INDEX [IX_c_Column_Prefix]
    ON [dbo].[c_Column]([Prefix])
    WHERE [Prefix] IS NOT NULL
GO

PRINT 'Created table: c_Column with indexes'
GO

-- =============================================================================
-- 3. c_Index - Index metadata
-- =============================================================================

PRINT 'Creating table: c_Index'
GO

CREATE TABLE [dbo].[c_Index] (
    [ID] INT IDENTITY(1,1) NOT NULL,
    [TableID] INT NOT NULL,
    [IndexName] NVARCHAR(128) NOT NULL,

    [IsUnique] BIT NOT NULL DEFAULT 0,
    [IsPrimaryKey] BIT NOT NULL DEFAULT 0,
    [IsClustered] BIT NOT NULL DEFAULT 0,
    [IndexType] NVARCHAR(50) NULL,

    -- Generation Impact
    [GeneratesGetByMethod] BIT NOT NULL DEFAULT 1,

    [AddedOn] DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [PK_c_Index] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_c_Index_Table] FOREIGN KEY ([TableID])
        REFERENCES [dbo].[c_Table]([ID]) ON DELETE CASCADE,
    CONSTRAINT [UQ_c_Index] UNIQUE ([TableID], [IndexName])
)
GO

CREATE NONCLUSTERED INDEX [IX_c_Index_Table]
    ON [dbo].[c_Index]([TableID])
GO

PRINT 'Created table: c_Index with indexes'
GO

-- =============================================================================
-- 4. c_IndexColumn - Index column details
-- =============================================================================

PRINT 'Creating table: c_IndexColumn'
GO

CREATE TABLE [dbo].[c_IndexColumn] (
    [ID] INT IDENTITY(1,1) NOT NULL,
    [IndexID] INT NOT NULL,
    [ColumnName] NVARCHAR(128) NOT NULL,
    [OrdinalPosition] INT NOT NULL,
    [IsDescending] BIT NOT NULL DEFAULT 0,

    CONSTRAINT [PK_c_IndexColumn] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_c_IndexColumn_Index] FOREIGN KEY ([IndexID])
        REFERENCES [dbo].[c_Index]([ID]) ON DELETE CASCADE
)
GO

CREATE NONCLUSTERED INDEX [IX_c_IndexColumn_Index]
    ON [dbo].[c_IndexColumn]([IndexID])
GO

PRINT 'Created table: c_IndexColumn with indexes'
GO

-- =============================================================================
-- 5. c_Relationship - Foreign key relationships
-- =============================================================================

PRINT 'Creating table: c_Relationship'
GO

CREATE TABLE [dbo].[c_Relationship] (
    [ID] INT IDENTITY(1,1) NOT NULL,
    [ParentTableID] INT NOT NULL,
    [ChildTableID] INT NOT NULL,
    [RelationshipName] NVARCHAR(128) NOT NULL,

    [ParentColumn] NVARCHAR(128) NOT NULL,
    [ChildColumn] NVARCHAR(128) NOT NULL,
    [RelationshipType] NVARCHAR(20) NOT NULL,

    [CascadeOnDelete] BIT NOT NULL DEFAULT 0,
    [CascadeOnUpdate] BIT NOT NULL DEFAULT 0,

    -- Generation
    [GenerateNavigationProperty] BIT NOT NULL DEFAULT 1,

    [AddedOn] DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [PK_c_Relationship] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_c_Relationship_ParentTable] FOREIGN KEY ([ParentTableID])
        REFERENCES [dbo].[c_Table]([ID]),
    CONSTRAINT [FK_c_Relationship_ChildTable] FOREIGN KEY ([ChildTableID])
        REFERENCES [dbo].[c_Table]([ID]),
    CONSTRAINT [UQ_c_Relationship] UNIQUE ([ParentTableID], [ChildTableID], [RelationshipName]),
    CONSTRAINT [CK_c_Relationship_Type] CHECK ([RelationshipType] IN ('OneToMany', 'OneToOne', 'ManyToMany'))
)
GO

CREATE NONCLUSTERED INDEX [IX_c_Relationship_ParentTable]
    ON [dbo].[c_Relationship]([ParentTableID])
GO

CREATE NONCLUSTERED INDEX [IX_c_Relationship_ChildTable]
    ON [dbo].[c_Relationship]([ChildTableID])
GO

PRINT 'Created table: c_Relationship with indexes'
GO

-- =============================================================================
-- 6. c_GenerationHistory - Generation history tracking
-- =============================================================================

PRINT 'Creating table: c_GenerationHistory'
GO

CREATE TABLE [dbo].[c_GenerationHistory] (
    [ID] INT IDENTITY(1,1) NOT NULL,
    [GeneratedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),

    -- Context
    [ProjectName] NVARCHAR(128) NULL,
    [ProjectPath] NVARCHAR(500) NULL,
    [MachineName] NVARCHAR(128) NULL,
    [UserName] NVARCHAR(128) NULL,

    -- What was generated
    [TablesGenerated] NVARCHAR(MAX) NULL,
    [FilesGenerated] INT NOT NULL DEFAULT 0,
    [StoredProcsGenerated] INT NOT NULL DEFAULT 0,

    -- Performance
    [DurationMs] INT NULL,

    -- Result
    [Success] BIT NOT NULL DEFAULT 1,
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [WarningCount] INT NOT NULL DEFAULT 0,

    -- Changes Detected
    [ChangeSummary] NVARCHAR(MAX) NULL,

    -- Version
    [TargCCVersion] NVARCHAR(20) NULL,

    CONSTRAINT [PK_c_GenerationHistory] PRIMARY KEY CLUSTERED ([ID] ASC)
)
GO

CREATE NONCLUSTERED INDEX [IX_c_GenerationHistory_Date]
    ON [dbo].[c_GenerationHistory]([GeneratedDate] DESC)
GO

CREATE NONCLUSTERED INDEX [IX_c_GenerationHistory_Project]
    ON [dbo].[c_GenerationHistory]([ProjectName])
    WHERE [ProjectName] IS NOT NULL
GO

PRINT 'Created table: c_GenerationHistory with indexes'
GO

-- =============================================================================
-- 7. c_Project - Project tracking (optional)
-- =============================================================================

PRINT 'Creating table: c_Project'
GO

CREATE TABLE [dbo].[c_Project] (
    [ID] INT IDENTITY(1,1) NOT NULL,
    [ProjectName] NVARCHAR(128) NOT NULL,
    [ProjectPath] NVARCHAR(500) NULL,
    [SolutionName] NVARCHAR(128) NULL,

    [Architecture] NVARCHAR(50) NOT NULL,
    [TargetFramework] NVARCHAR(20) NULL,

    [ConnectionString] NVARCHAR(MAX) NULL,

    [CreatedOn] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [LastGeneratedOn] DATETIME2 NULL,

    [IsActive] BIT NOT NULL DEFAULT 1,

    CONSTRAINT [PK_c_Project] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [UQ_c_Project_Name] UNIQUE ([ProjectName]),
    CONSTRAINT [CK_c_Project_Architecture] CHECK ([Architecture] IN ('CleanArchitecture', 'MinimalApi', 'ThreeTier'))
)
GO

CREATE NONCLUSTERED INDEX [IX_c_Project_Active]
    ON [dbo].[c_Project]([IsActive])
    WHERE [IsActive] = 1
GO

PRINT 'Created table: c_Project with indexes'
GO

-- =============================================================================
-- 8. c_Enumeration - Enum values
-- =============================================================================

PRINT 'Creating table: c_Enumeration'
GO

CREATE TABLE [dbo].[c_Enumeration] (
    [ID] INT IDENTITY(1,1) NOT NULL,
    [IsSystem] BIT NOT NULL DEFAULT 0,
    [EnumType] VARCHAR(50) NOT NULL,
    [EnumValue] VARCHAR(50) NOT NULL,
    [locText] NVARCHAR(50) NULL,
    [OrdinalPosition] INT NULL,

    [AddedBy] NVARCHAR(50) NULL,
    [AddedOn] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [ChangedBy] NVARCHAR(50) NULL,
    [ChangedOn] DATETIME2 NULL,
    [DeletedBy] NVARCHAR(50) NULL,
    [DeletedOn] DATETIME2 NULL,

    CONSTRAINT [PK_c_Enumeration] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [UQ_c_Enumeration] UNIQUE ([EnumType], [EnumValue])
)
GO

CREATE NONCLUSTERED INDEX [IX_c_Enumeration_Type]
    ON [dbo].[c_Enumeration]([EnumType])
GO

CREATE NONCLUSTERED INDEX [IX_c_Enumeration_Active]
    ON [dbo].[c_Enumeration]([EnumType])
    WHERE [DeletedOn] IS NULL
GO

PRINT 'Created table: c_Enumeration with indexes'
GO

-- =============================================================================
-- 9. c_Lookup - Dynamic lookup values
-- =============================================================================

PRINT 'Creating table: c_Lookup'
GO

CREATE TABLE [dbo].[c_Lookup] (
    [ID] BIGINT IDENTITY(1,1) NOT NULL,
    [enmParentLookupType] VARCHAR(50) NULL,
    [ParentCode] VARCHAR(50) NULL,
    [enmLookupType] VARCHAR(50) NOT NULL,
    [Code] VARCHAR(50) NOT NULL,
    [locText] NVARCHAR(100) NULL,
    [locDescription] NVARCHAR(500) NULL,
    [OrdinalPosition] INT NULL,

    [AddedBy] NVARCHAR(50) NULL,
    [AddedOn] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [ChangedBy] NVARCHAR(50) NULL,
    [ChangedOn] DATETIME2 NULL,
    [DeletedBy] NVARCHAR(50) NULL,
    [DeletedOn] DATETIME2 NULL,

    CONSTRAINT [PK_c_Lookup] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [UQ_c_Lookup] UNIQUE ([enmLookupType], [Code])
)
GO

CREATE NONCLUSTERED INDEX [IX_c_Lookup_Type]
    ON [dbo].[c_Lookup]([enmLookupType])
GO

CREATE NONCLUSTERED INDEX [IX_c_Lookup_Active]
    ON [dbo].[c_Lookup]([enmLookupType], [Code])
    WHERE [DeletedOn] IS NULL
GO

CREATE NONCLUSTERED INDEX [IX_c_Lookup_Parent]
    ON [dbo].[c_Lookup]([enmParentLookupType], [ParentCode])
    WHERE [enmParentLookupType] IS NOT NULL
GO

PRINT 'Created table: c_Lookup with indexes'
GO

-- =============================================================================
-- 10. Populate system tables with metadata about themselves
-- =============================================================================

PRINT 'Populating c_Table with system table metadata...'
GO

INSERT INTO [dbo].[c_Table]
    ([TableName], [SchemaName], [IsSystemTable], [IsActive], [GenerateEntity], [GenerateRepository], [GenerateController], [GenerateStoredProcedures], [GenerateCQRS], [GenerateReactUI], [Notes])
VALUES
    ('c_Table', 'dbo', 1, 0, 0, 0, 0, 0, 0, 0, 'TargCC system table - do not generate code'),
    ('c_Column', 'dbo', 1, 0, 0, 0, 0, 0, 0, 0, 'TargCC system table - do not generate code'),
    ('c_Index', 'dbo', 1, 0, 0, 0, 0, 0, 0, 0, 'TargCC system table - do not generate code'),
    ('c_IndexColumn', 'dbo', 1, 0, 0, 0, 0, 0, 0, 0, 'TargCC system table - do not generate code'),
    ('c_Relationship', 'dbo', 1, 0, 0, 0, 0, 0, 0, 0, 'TargCC system table - do not generate code'),
    ('c_GenerationHistory', 'dbo', 1, 0, 0, 0, 0, 0, 0, 0, 'TargCC system table - do not generate code'),
    ('c_Project', 'dbo', 1, 0, 0, 0, 0, 0, 0, 0, 'TargCC system table - do not generate code'),
    ('c_Enumeration', 'dbo', 1, 0, 0, 0, 0, 0, 0, 0, 'TargCC system table - do not generate code'),
    ('c_Lookup', 'dbo', 1, 0, 0, 0, 0, 0, 0, 0, 'TargCC system table - do not generate code')
GO

PRINT 'System table metadata populated'
GO

-- =============================================================================
-- 11. Create helper stored procedure for getting lookup values
-- =============================================================================

PRINT 'Creating stored procedure: SP_GetLookup'
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_GetLookup]
    @LookupType VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [Code],
        [locText],
        [locDescription],
        [OrdinalPosition]
    FROM [dbo].[c_Lookup]
    WHERE [enmLookupType] = @LookupType
      AND [DeletedOn] IS NULL
    ORDER BY
        CASE WHEN [OrdinalPosition] IS NULL THEN 1 ELSE 0 END,
        [OrdinalPosition],
        [locText]
END
GO

PRINT 'Created stored procedure: SP_GetLookup'
GO

-- =============================================================================
-- Summary
-- =============================================================================

PRINT ''
PRINT '========================================='
PRINT 'TargCC System Tables Migration Complete!'
PRINT '========================================='
PRINT ''
PRINT 'Created tables:'
PRINT '  1. c_Table'
PRINT '  2. c_Column'
PRINT '  3. c_Index'
PRINT '  4. c_IndexColumn'
PRINT '  5. c_Relationship'
PRINT '  6. c_GenerationHistory'
PRINT '  7. c_Project'
PRINT '  8. c_Enumeration'
PRINT '  9. c_Lookup'
PRINT ''
PRINT 'Created indexes: 15+'
PRINT 'Created stored procedures: 1 (SP_GetLookup)'
PRINT ''
PRINT 'Next steps:'
PRINT '  - Run TargCC to sync existing database tables'
PRINT '  - Populate c_Enumeration with your enum values'
PRINT '  - Populate c_Lookup with your lookup data'
PRINT ''
GO
