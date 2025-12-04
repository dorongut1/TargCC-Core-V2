# TargCC Database Migrations

This directory contains database migration scripts for TargCC system tables.

## Overview

TargCC uses SQL Server system tables (prefixed with `c_`) to track metadata about your database schema, enabling intelligent code generation and change detection.

## Running Migrations

### Option 1: Manual Execution (SQL Server Management Studio)

1. Connect to your database
2. Open the migration script (e.g., `001_Create_System_Tables.sql`)
3. Execute the script
4. Verify success in the output messages

### Option 2: Using TargCC CLI (coming soon)

```bash
targcc migrate --connection "Server=localhost;Database=MyDB"
```

### Option 3: Using sqlcmd

```bash
sqlcmd -S localhost -d MyDB -i 001_Create_System_Tables.sql
```

## Migration Files

### 001_Create_System_Tables.sql

**Version:** 2.0.0
**Date:** 2025-12-02

Creates the core TargCC system tables:

- **c_Table** - Tracks all tables in your database with generation settings
- **c_Column** - Stores column metadata including prefixes (eno_, ent_, lkp_, etc.)
- **c_Index** - Index metadata for generating GetBy methods
- **c_IndexColumn** - Index column details
- **c_Relationship** - Foreign key relationships for navigation properties
- **c_GenerationHistory** - Tracks generation runs (when, what, results)
- **c_Project** - Project-level settings (optional)
- **c_Enumeration** - Enum values for code generation
- **c_Lookup** - Dynamic lookup values (countries, statuses, etc.)

**What it does:**
- Creates all tables with proper indexes
- Adds foreign key constraints
- Populates c_Table with system table metadata
- Creates SP_GetLookup helper procedure
- Includes safety check (won't run twice)

**Rollback:**
```sql
-- WARNING: This will delete all TargCC metadata!
DROP TABLE IF EXISTS c_Lookup
DROP TABLE IF EXISTS c_Enumeration
DROP TABLE IF EXISTS c_Project
DROP TABLE IF EXISTS c_GenerationHistory
DROP TABLE IF EXISTS c_Relationship
DROP TABLE IF EXISTS c_IndexColumn
DROP TABLE IF EXISTS c_Index
DROP TABLE IF EXISTS c_Column
DROP TABLE IF EXISTS c_Table
DROP PROCEDURE IF EXISTS SP_GetLookup
```

## After Running Migrations

### 1. Verify Installation

```sql
-- Check that all tables exist
SELECT name FROM sys.tables WHERE name LIKE 'c_%' ORDER BY name

-- Should return:
-- c_Column
-- c_Enumeration
-- c_GenerationHistory
-- c_Index
-- c_IndexColumn
-- c_Lookup
-- c_Project
-- c_Relationship
-- c_Table
```

### 2. Sync Your Database

Run TargCC to populate c_Table and c_Column with your existing database structure:

```bash
targcc sync --database "Server=localhost;Database=MyDB"
```

This will:
- Read all tables from sys.tables
- Read all columns from sys.columns
- Populate c_Table with table metadata
- Populate c_Column with column details
- Calculate initial schema hashes

### 3. Configure Generation Options

Update c_Table to control what gets generated:

```sql
-- Enable React UI generation for specific tables
UPDATE c_Table
SET GenerateReactUI = 1
WHERE TableName IN ('Customer', 'Order', 'Product')

-- Disable stored procedures for lookup tables
UPDATE c_Table
SET GenerateStoredProcedures = 0
WHERE TableName LIKE 'lkp_%'

-- Mark table as inactive (skip generation)
UPDATE c_Table
SET IsActive = 0
WHERE TableName = 'TempTable'
```

### 4. Populate Enumerations

Add your enum values to c_Enumeration:

```sql
-- Example: CardOrderStatus enum
INSERT INTO c_Enumeration (EnumType, EnumValue, locText, OrdinalPosition)
VALUES
    ('CardOrderStatus', 'Imported', N'Imported', 1),
    ('CardOrderStatus', 'Dispatched', N'Dispatched', 2),
    ('CardOrderStatus', 'Arrived', N'Arrived', 3)

-- TargCC will generate:
-- public enum CardOrderStatus
-- {
--     Imported = 1,
--     Dispatched = 2,
--     Arrived = 3
-- }
```

### 5. Populate Lookups

Add lookup data to c_Lookup:

```sql
-- Example: Country lookup
INSERT INTO c_Lookup (enmLookupType, Code, locText, OrdinalPosition)
VALUES
    ('Country', 'IL', N'Israel', 1),
    ('Country', 'US', N'United States', 2),
    ('Country', 'UK', N'United Kingdom', 3)

-- TargCC will generate:
-- - API endpoint: GET /api/lookup/Country
-- - Repository method: GetCountryLookupAsync()
-- - React component: CountrySelect
```

## Troubleshooting

### "System tables already exist"

The migration script includes a safety check. If tables already exist, it will skip creation. To force recreation:

```sql
-- Drop existing tables (WARNING: deletes all metadata!)
-- Run the rollback script above, then re-run migration
```

### Foreign Key Constraint Errors

If you see FK errors:
1. Ensure c_Table is created before c_Column
2. Check that parent tables exist before creating child tables
3. The migration script handles this automatically

### Performance Issues

If c_Table or c_Column become large:
1. Indexes are already optimized in the migration
2. Consider archiving old c_GenerationHistory records
3. Use `WHERE IsActive = 1` in queries

## Best Practices

### 1. Version Control

Keep migration scripts in source control:
```bash
git add database/migrations/
git commit -m "Add TargCC system tables migration"
```

### 2. Backup Before Migration

```sql
BACKUP DATABASE [MyDB] TO DISK = 'C:\Backup\MyDB_before_targcc.bak'
```

### 3. Test on Dev First

Always test migrations on a development database before production.

### 4. Document Custom Changes

If you modify system tables, document changes in a new migration file:
```
002_Add_Custom_Column_To_c_Table.sql
```

## Future Migrations

Future migration files will be numbered sequentially:
- `002_*.sql` - Next migration
- `003_*.sql` - After that
- etc.

Each migration should:
- Check if already applied
- Be idempotent (safe to run multiple times)
- Include rollback instructions
- Update this README

## Support

For issues or questions:
- Check the main TargCC documentation: `/docs/`
- See examples: `/examples/`
- Report issues: GitHub Issues

---

**Last Updated:** 2025-12-02
**Compatible with:** SQL Server 2019+, Azure SQL Database
