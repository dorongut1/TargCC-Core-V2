# 📘 TargCC V2 - אפיון מלא ומפורט

**גרסה:** 2.0
**תאריך:** 02/12/2025
**מטרה:** מערכת יצירת קוד אוטומטית מודרנית מבוססת Database Schema

---

## 🎯 1. סקירה כללית

### 1.1 מהי המערכת?

**TargCC V2** היא מערכת יצירת קוד אוטומטית (Code Generator) המייצרת אפליקציה מלאה מתוך Database Schema, תוך שמירה על הפילוסופיה של TargCC Legacy אך עם טכנולוגיות מודרניות של 2025.

### 1.2 תרחישי שימוש עיקריים

#### **Scenario A: Greenfield - פרויקט חדש**
```bash
targcc init \
  --name MySolution \
  --architecture clean \
  --database "Server=localhost;Database=MyDB" \
  --tables Customer,Order,Product
```

**תוצאה:**
- Solution מלא עם 5 פרויקטים
- Entity classes
- Repositories
- Controllers
- Stored Procedures
- React UI (אופציונלי)
- מוכן לבנייה והרצה

#### **Scenario B: Brownfield - עדכון פרויקט קיים**
```bash
cd /path/to/existing/project
targcc generate --tables Customer --force
```

**תוצאה:**
- מזהה שטבלה השתנתה
- מציג מה השתנה (2 עמודות חדשות, 1 index)
- מגנרט מחדש את הקבצים הרלוונטיים
- שומר על קוד ידני (*.prt.cs)

#### **Scenario C: Integration - הוספה לפרויקט קיים**
```bash
cd /path/to/external/project
targcc integrate --tables Customer,Order
```

**תוצאה:**
- מזהה solution קיים
- מוסיף קבצים לפרויקטים הקיימים
- מעדכן DbContext
- מעדכן Program.cs עם DI

---

## 🗄️ 2. Database Schema - טבלאות המערכת

### 2.1 c_Table - טבלת הטבלאות המרכזית

```sql
CREATE TABLE [dbo].[c_Table] (
    -- Identity
    [ID] INT IDENTITY(1,1) PRIMARY KEY,
    [TableName] NVARCHAR(128) NOT NULL,
    [SchemaName] NVARCHAR(128) NOT NULL DEFAULT 'dbo',

    -- Tracking & Change Detection
    [LastGenerated] DATETIME2 NULL,                    -- מתי נוצר לאחרונה
    [LastModifiedInDB] DATETIME2 NULL,                 -- מתי השתנה ב-DB (מ-sys.tables)
    [SchemaHash] VARCHAR(64) NULL,                     -- SHA256 של ה-schema המלא
    [SchemaHashPrevious] VARCHAR(64) NULL,             -- Hash קודם (לזיהוי מה השתנה)

    -- Metadata (כמו Legacy)
    [ccAuditLevel] INT NOT NULL DEFAULT 0,             -- 0=None, 1=Track, 2=Full audit
    [ccUICreateMenu] BIT NOT NULL DEFAULT 1,           -- צור menu entry
    [ccUICreateEntity] BIT NOT NULL DEFAULT 1,         -- צור entity form
    [ccUICreateCollection] BIT NOT NULL DEFAULT 1,     -- צור collection grid
    [ccIsSingleRow] BIT NOT NULL DEFAULT 0,            -- טבלה עם שורה יחידה
    [ccUsedForIdentity] BIT NOT NULL DEFAULT 0,        -- משמש לזיהוי משתמשים

    -- Generation Options (NEW!)
    [GenerateEntity] BIT NOT NULL DEFAULT 1,           -- צור Entity class
    [GenerateRepository] BIT NOT NULL DEFAULT 1,       -- צור Repository
    [GenerateController] BIT NOT NULL DEFAULT 1,       -- צור Controller
    [GenerateReactUI] BIT NOT NULL DEFAULT 0,          -- צור React UI
    [GenerateStoredProcedures] BIT NOT NULL DEFAULT 1, -- צור SPs
    [GenerateCQRS] BIT NOT NULL DEFAULT 1,             -- צור Commands/Queries

    -- System
    [IsSystemTable] BIT NOT NULL DEFAULT 0,            -- טבלת מערכת (c_ prefix)
    [IsActive] BIT NOT NULL DEFAULT 1,                 -- האם פעיל לgeneration
    [Notes] NVARCHAR(MAX) NULL,                        -- הערות למפתח

    -- Audit
    [AddedOn] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [AddedBy] NVARCHAR(100) NULL,
    [ChangedOn] DATETIME2 NULL,
    [ChangedBy] NVARCHAR(100) NULL,

    CONSTRAINT [UQ_c_Table] UNIQUE ([SchemaName], [TableName])
)
GO

CREATE INDEX [IX_c_Table_LastModified] ON [c_Table]([LastModifiedInDB])
GO

CREATE INDEX [IX_c_Table_Active] ON [c_Table]([IsActive]) WHERE [IsActive] = 1
GO
```

### 2.2 c_Column - פירוט עמודות

```sql
CREATE TABLE [dbo].[c_Column] (
    [ID] INT IDENTITY(1,1) PRIMARY KEY,
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
    [Prefix] NVARCHAR(10) NULL,                   -- eno_, ent_, lkp_, enm_, etc.
    [OrdinalPosition] INT NOT NULL,
    [ColumnHash] VARCHAR(64) NULL,                -- Hash של העמודה הספציפית

    -- Generation
    [IncludeInGeneration] BIT NOT NULL DEFAULT 1,

    -- Audit
    [AddedOn] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [ChangedOn] DATETIME2 NULL,

    CONSTRAINT [FK_c_Column_Table] FOREIGN KEY ([TableID])
        REFERENCES [c_Table]([ID]) ON DELETE CASCADE,
    CONSTRAINT [UQ_c_Column] UNIQUE ([TableID], [ColumnName])
)
GO

CREATE INDEX [IX_c_Column_Table] ON [c_Column]([TableID])
GO
```

### 2.3 c_Index - אינדקסים

```sql
CREATE TABLE [dbo].[c_Index] (
    [ID] INT IDENTITY(1,1) PRIMARY KEY,
    [TableID] INT NOT NULL,
    [IndexName] NVARCHAR(128) NOT NULL,

    [IsUnique] BIT NOT NULL DEFAULT 0,
    [IsPrimaryKey] BIT NOT NULL DEFAULT 0,
    [IsClustered] BIT NOT NULL DEFAULT 0,
    [IndexType] NVARCHAR(50) NULL,                -- CLUSTERED, NONCLUSTERED, etc.

    -- Generation Impact
    [GeneratesGetByMethod] BIT NOT NULL DEFAULT 1, -- צור GetByXXX method

    [AddedOn] DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [FK_c_Index_Table] FOREIGN KEY ([TableID])
        REFERENCES [c_Table]([ID]) ON DELETE CASCADE,
    CONSTRAINT [UQ_c_Index] UNIQUE ([TableID], [IndexName])
)
GO

CREATE TABLE [dbo].[c_IndexColumn] (
    [ID] INT IDENTITY(1,1) PRIMARY KEY,
    [IndexID] INT NOT NULL,
    [ColumnName] NVARCHAR(128) NOT NULL,
    [OrdinalPosition] INT NOT NULL,
    [IsDescending] BIT NOT NULL DEFAULT 0,

    CONSTRAINT [FK_c_IndexColumn_Index] FOREIGN KEY ([IndexID])
        REFERENCES [c_Index]([ID]) ON DELETE CASCADE
)
GO
```

### 2.4 c_Relationship - קשרים בין טבלאות

```sql
CREATE TABLE [dbo].[c_Relationship] (
    [ID] INT IDENTITY(1,1) PRIMARY KEY,
    [ParentTableID] INT NOT NULL,
    [ChildTableID] INT NOT NULL,
    [RelationshipName] NVARCHAR(128) NOT NULL,

    [ParentColumn] NVARCHAR(128) NOT NULL,
    [ChildColumn] NVARCHAR(128) NOT NULL,
    [RelationshipType] NVARCHAR(20) NOT NULL,     -- OneToMany, OneToOne, ManyToMany

    [CascadeOnDelete] BIT NOT NULL DEFAULT 0,
    [CascadeOnUpdate] BIT NOT NULL DEFAULT 0,

    -- Generation
    [GenerateNavigationProperty] BIT NOT NULL DEFAULT 1,

    [AddedOn] DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [FK_c_Relationship_ParentTable] FOREIGN KEY ([ParentTableID])
        REFERENCES [c_Table]([ID]),
    CONSTRAINT [FK_c_Relationship_ChildTable] FOREIGN KEY ([ChildTableID])
        REFERENCES [c_Table]([ID]),
    CONSTRAINT [UQ_c_Relationship] UNIQUE ([ParentTableID], [ChildTableID], [RelationshipName])
)
GO
```

### 2.5 c_GenerationHistory - היסטוריית generation

```sql
CREATE TABLE [dbo].[c_GenerationHistory] (
    [ID] INT IDENTITY(1,1) PRIMARY KEY,
    [GeneratedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),

    -- Context
    [ProjectName] NVARCHAR(128) NULL,
    [ProjectPath] NVARCHAR(500) NULL,
    [MachineName] NVARCHAR(128) NULL,
    [UserName] NVARCHAR(128) NULL,

    -- What was generated
    [TablesGenerated] NVARCHAR(MAX) NULL,         -- JSON array
    [FilesGenerated] INT NOT NULL DEFAULT 0,
    [StoredProcsGenerated] INT NOT NULL DEFAULT 0,

    -- Performance
    [DurationMs] INT NULL,

    -- Result
    [Success] BIT NOT NULL DEFAULT 1,
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [WarningCount] INT NOT NULL DEFAULT 0,

    -- Changes Detected
    [ChangeSummary] NVARCHAR(MAX) NULL,           -- JSON: { addedColumns: [...], removedColumns: [...] }

    -- Version
    [TargCCVersion] NVARCHAR(20) NULL
)
GO

CREATE INDEX [IX_c_GenerationHistory_Date] ON [c_GenerationHistory]([GeneratedDate] DESC)
GO
```

### 2.6 c_Project - מעקב אחרי פרויקטים (אופציונלי)

```sql
CREATE TABLE [dbo].[c_Project] (
    [ID] INT IDENTITY(1,1) PRIMARY KEY,
    [ProjectName] NVARCHAR(128) NOT NULL,
    [ProjectPath] NVARCHAR(500) NULL,
    [SolutionName] NVARCHAR(128) NULL,

    [Architecture] NVARCHAR(50) NOT NULL,         -- CleanArchitecture, MinimalApi, ThreeTier
    [TargetFramework] NVARCHAR(20) NULL,          -- net9.0

    [ConnectionString] NVARCHAR(MAX) NULL,        -- encrypted

    [CreatedOn] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [LastGeneratedOn] DATETIME2 NULL,

    [IsActive] BIT NOT NULL DEFAULT 1,

    CONSTRAINT [UQ_c_Project_Name] UNIQUE ([ProjectName])
)
GO
```

---

## 🔄 3. Change Detection - מנגנון זיהוי שינויים

### 3.1 הזרימה המלאה

```
Step 1: Quick Check
├─ Query: sys.tables.modify_date > c_Table.LastGenerated
└─ Result: Customer table modified!

Step 2: Deep Analysis
├─ Read current schema from sys.* views
├─ Calculate current schema hash
├─ Compare to c_Table.SchemaHash
└─ If different → Run differential analysis

Step 3: Differential Analysis
├─ Compare: c_Column (stored) vs sys.columns (current)
└─ Found Changes:
    ├─ ✓ 2 columns added: PhoneNumber, Address
    ├─ ✓ 1 column modified: Email (length 100 → 200)
    ├─ ✓ 1 index added: IX_Customer_Phone
    └─ ✗ No relationships changed

Step 4: Impact Analysis
├─ ✓ Entity - columns changed
├─ ✓ Repository - new index → new GetByPhone method
├─ ✓ Controller - entity changed
├─ ✓ Commands/Queries - entity changed
├─ ✓ React UI - new fields
└─ ✓ Stored Procedures - schema changed

Step 5: User Confirmation
└─ [Regenerate Selected] [Cancel]

Step 6: Generation + Update Metadata
├─ Backup existing files (*.bak)
├─ Delete old generated files (NOT *.prt.cs!)
├─ Generate new files
├─ Update c_Table metadata
└─ Insert to c_GenerationHistory
```

### 3.2 Schema Hash Calculation

**Algorithm:**
1. Serialize table schema to deterministic JSON
2. Include: columns, indexes, relationships, extended properties
3. Order all collections deterministically
4. Calculate SHA256 hash
5. Store as lowercase hex string

### 3.3 Differential Analysis

**Process:**
1. Load stored schema from c_* tables
2. Read current schema from sys.* views
3. Compare column-by-column
4. Compare index-by-index
5. Compare relationship-by-relationship
6. Generate SchemaDiff object with detailed changes

---

## 🏗️ 4. Code Generation Strategy

### 4.1 מה נוצר לכל טבלה?

```
Customer Table
├── Stored Procedures (Database)
│   ├── SP_GetCustomerByID.sql
│   ├── SP_GetCustomerByEmail.sql         (unique index)
│   ├── SP_GetCustomersByStatus.sql       (non-unique index)
│   ├── SP_InsertCustomer.sql
│   ├── SP_UpdateCustomer.sql
│   ├── SP_DeleteCustomer.sql
│   └── SP_GetCustomersOnTheFly.sql       (dynamic filters)
│
├── Domain Layer (Entities)
│   ├── Customer.cs                        (Generated)
│   └── Customer.prt.cs                    (Manual)
│
├── Application Layer (CQRS)
│   ├── Commands/
│   │   ├── CreateCustomerCommand.cs
│   │   ├── CreateCustomerCommandHandler.cs
│   │   ├── UpdateCustomerCommand.cs
│   │   └── DeleteCustomerCommand.cs
│   │
│   ├── Queries/
│   │   ├── GetCustomerByIdQuery.cs
│   │   ├── GetCustomerByEmailQuery.cs
│   │   └── GetCustomersByStatusQuery.cs
│   │
│   └── DTOs/
│       ├── CustomerDto.cs
│       └── CustomerListDto.cs
│
├── Infrastructure Layer (Data Access)
│   ├── Repositories/
│   │   ├── ICustomerRepository.cs         (Generated)
│   │   ├── CustomerRepository.cs          (Generated - calls SPs via Dapper)
│   │   └── CustomerRepository.prt.cs      (Manual)
│   │
│   └── Configurations/
│       └── CustomerConfiguration.cs       (EF Core configuration)
│
├── API Layer (Controllers)
│   ├── CustomerController.cs              (Generated)
│   └── CustomerController.prt.cs          (Manual)
│
└── React UI (Optional)
    └── components/Customer/
        ├── types.ts
        ├── api.ts
        ├── useCustomer.ts
        ├── CustomerForm.tsx
        ├── CustomerList.tsx
        ├── CustomerDetail.tsx
        ├── Routes.tsx
        └── index.ts
```

### 4.2 Repository Implementation Strategy

**Default: Stored Procedures + Dapper**

Benefits:
- ✅ Performance - compiled, execution plan cached
- ✅ Separation - Data access in SPs, Logic in C#
- ✅ DBA friendly - can optimize queries
- ✅ Auto-generated - no maintenance burden

**Repository calls SPs via Dapper:**
```csharp
public async Task<Customer?> GetByIdAsync(int id)
{
    return await _connection.QueryFirstOrDefaultAsync<Customer>(
        sql: "SP_GetCustomerByID",
        param: new { ID = id },
        commandType: CommandType.StoredProcedure
    );
}
```

### 4.3 Partial Classes Pattern

**Generated files:** Deleted and recreated on every generation
**Manual files (*.prt.cs):** Never touched by TargCC

```
Customer.cs           ← Generated (safe to delete)
Customer.prt.cs       ← Manual (never touched)
```

---

## 📋 5. Implementation Plan

### Phase 1: Infrastructure & Database (Week 1-2)
- **Task 1.1:** Database Schema Setup (2-3 days)
  - Create migration scripts for c_* tables
  - Add indexes and constraints
  - Test on empty DB

- **Task 1.2:** Metadata Service (3-4 days)
  - Implement IMetadataService
  - Sync from sys.* to c_*
  - CRUD operations for metadata

- **Task 1.3:** Schema Hash Calculator (1-2 days)
  - Deterministic hash calculation
  - Unit tests

- **Task 1.4:** Change Detection Service (4-5 days)
  - Quick check via modify_date
  - Deep analysis via hash comparison
  - Differential analyzer

### Phase 2: Stored Procedures Generation (Week 3)
- **Task 2.1:** SP Template Engine (5-6 days)
  - Templates for GetByID, Insert, Update, Delete
  - Templates for indexes (unique/non-unique)
  - Unit tests

- **Task 2.2:** SP Generator Service (3-4 days)
  - Generate SPs for table
  - Deploy to database
  - Integration tests

### Phase 3: Repository Generation (Week 4)
- **Task 3.1:** Repository Template Engine (5-6 days)
  - Interface generator
  - Implementation generator (Dapper + SPs)
  - Partial class support
  - Unit tests

### Phase 4: Entity & CQRS Generation (Week 5)
- **Task 4.1:** Entity Generator Enhancement (2-3 days)
  - Support for prefixes (eno_, ent_, etc.)
  - Partial classes
  - Navigation properties

- **Task 4.2:** CQRS Generator (4-5 days)
  - Command/Query classes
  - Handler classes
  - Unit tests

### Phase 5: Project Management (Week 6)
- **Task 5.1:** Project Detector (2-3 days)
  - Detect Greenfield/Brownfield/Integration
  - Detect architecture

- **Task 5.2:** Code Integrator (4-5 days)
  - DbContext integration
  - Program.cs DI registration
  - Solution file management

### Phase 6: CLI & Web UI (Week 7-8)
- **Task 6.1:** CLI Commands (3-4 days)
  - Enhance existing commands
  - Add new commands (detect-changes, integrate)

- **Task 6.2:** Web UI Enhancements (5-6 days)
  - Change detection UI
  - Selective generation UI
  - History viewer

### Phase 7: Testing & Polish (Week 9-10)
- **Task 7.1:** Integration Tests (4-5 days)
  - End-to-end Greenfield
  - End-to-end Brownfield

- **Task 7.2:** Documentation (3-4 days)
  - User guide
  - API documentation
  - Migration guide

- **Task 7.3:** Performance Optimization (2-3 days)
  - Profile and optimize
  - Memory optimization

---

## 📅 6. Timeline Summary

```
Week 1-2:  Phase 1 - Infrastructure & Database
Week 3:    Phase 2 - Stored Procedures Generation
Week 4:    Phase 3 - Repository Generation
Week 5:    Phase 4 - Entity & CQRS Generation
Week 6:    Phase 5 - Project Management
Week 7-8:  Phase 6 - CLI & Web UI
Week 9-10: Phase 7 - Testing & Polish

Total: 10 weeks (2.5 months)
```

### Milestones:
- **M1 (Week 2):** Metadata system ready
- **M2 (Week 3):** SP generation working
- **M3 (Week 4):** Repository generation working
- **M4 (Week 6):** Full Greenfield flow working
- **M5 (Week 8):** Full Brownfield flow working
- **M6 (Week 10):** Production ready

---

## 🎯 7. Success Criteria

### Must Have (MVP):
- ✅ Greenfield: יצירת פרויקט חדש מ-DB
- ✅ SPs: Generation + deployment
- ✅ Repositories: Dapper + SPs
- ✅ Entities: עם prefixes
- ✅ Change Detection: זיהוי שינויים
- ✅ Brownfield: עדכון פרויקט קיים
- ✅ CLI: כל הcommands עובדים
- ✅ Web UI: בסיסי עובד

### Should Have:
- ✅ CQRS: Commands + Queries
- ✅ React UI generation
- ✅ Integration: הוספה לפרויקט קיים
- ✅ Differential analysis מפורט
- ✅ Selective generation

### Nice to Have:
- 🔲 Git integration
- 🔲 AI code review
- 🔲 Performance profiling
- 🔲 Multi-database support

---

## 📝 8. Key Decisions

1. **Metadata Storage:** SQL Tables (c_*) - like Legacy, proven approach
2. **Change Detection:** sys.tables.modify_date + Schema Hash + Differential Analysis
3. **SP Strategy:** Default generation with Dapper repositories
4. **Code Separation:** Partial classes (*.prt.cs) for manual code
5. **Project Detection:** Auto-detect with manual override option
6. **Code Integration:** Markers for Program.cs, Partial for DbContext

---

## 🚀 9. Getting Started

After approval, development will begin with:
1. Database migration scripts
2. Metadata service implementation
3. Tests for core functionality

**Ready to start Phase 1?**
