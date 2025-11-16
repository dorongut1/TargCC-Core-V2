# TargCC Core V2 - API Documentation 📚

**Version:** 1.0.0-rc1  
**Last Updated:** November 14, 2025  
**Status:** Phase 1 Complete (71%)

---

## 📖 Table of Contents

1. [Overview](#overview)
2. [Core Components](#core-components)
3. [DatabaseAnalyzer](#databaseanalyzer)
4. [TableAnalyzer](#tableanalyzer)
5. [ColumnAnalyzer](#columnanalyzer)
6. [RelationshipAnalyzer](#relationshipanalyzer)
7. [Usage Examples](#usage-examples)
8. [Best Practices](#best-practices)

---

## Overview

TargCC Core V2 is a modern C# .NET 8 library for analyzing SQL Server database schemas and generating code automatically. It supports both **full analysis** and **incremental change detection** for efficient code generation.

### Key Features

✅ **Full Database Analysis** - Complete schema introspection  
✅ **Incremental Analysis** - Only analyze changed tables  
✅ **Change Detection** - Smart comparison with previous schema  
✅ **Prefix Detection** - 10+ field prefixes (eno, ent, enm, lkp, etc.)  
✅ **Relationship Mapping** - Foreign keys and graph building  
✅ **80%+ Test Coverage** - Comprehensive test suite  

---

## Core Components

### Project Structure

```
TargCC.Core/
├── TargCC.Core.Engine/          # Core orchestration
├── TargCC.Core.Interfaces/      # Models and contracts
├── TargCC.Core.Analyzers/       # Database analyzers
│   └── Database/
│       ├── DatabaseAnalyzer.cs  ⭐ Main analyzer
│       ├── TableAnalyzer.cs     📋 Table structure
│       ├── ColumnAnalyzer.cs    🔤 Column metadata
│       └── RelationshipAnalyzer.cs 🔗 FK relationships
└── TargCC.Core.Tests/           # Test suite
```

---

## DatabaseAnalyzer

### Purpose
The **DatabaseAnalyzer** is the main entry point for database analysis. It orchestrates table and relationship analysis.

### Constructor

```csharp
public DatabaseAnalyzer(
    string connectionString,  // SQL Server connection string
    ILogger<DatabaseAnalyzer> logger)
```

**Example:**
```csharp
var logger = LoggerFactory.Create(builder => builder.AddConsole())
    .CreateLogger<DatabaseAnalyzer>();

var analyzer = new DatabaseAnalyzer(
    "Server=localhost;Database=MyDb;Integrated Security=true;",
    logger);
```

### Key Methods

#### 1. ConnectAsync()
Test database connection before analysis.

```csharp
if (await analyzer.ConnectAsync())
{
    Console.WriteLine("✅ Connected!");
}
```

**Returns:** `bool` - true if successful

---

#### 2. AnalyzeAsync() ⭐
**Full database analysis** - analyzes all tables.

```csharp
var schema = await analyzer.AnalyzeAsync();

Console.WriteLine($"Database: {schema.DatabaseName}");
Console.WriteLine($"Tables: {schema.Tables.Count}");
Console.WriteLine($"Relationships: {schema.Relationships.Count}");
```

**Returns:** `DatabaseSchema` - complete schema with all metadata

**When to use:**
- First-time analysis
- Major schema changes
- Complete regeneration needed

**Performance:** 5-30 seconds for medium databases (50-200 tables)

---

#### 3. DetectChangedTablesAsync() 🎯
**Change detection** - identifies new and modified tables.

```csharp
// Load previous schema (from file, DB, cache)
var previousSchema = await LoadPreviousSchemaAsync();

// Detect what changed
var changedTables = await analyzer.DetectChangedTablesAsync(previousSchema);

Console.WriteLine($"Changed: {changedTables.Count} tables");
foreach (var table in changedTables)
{
    Console.WriteLine($"  - {table}");
}
```

**Returns:** `List<string>` - fully-qualified table names (e.g., "dbo.Customer")

**Detection includes:**
- ✅ New tables
- ✅ Modified tables (schema changes)
- ❌ Removed tables (handled by generator)

**How it works:**
1. Compares current tables with previous schema
2. Checks `sys.tables.modify_date` for existing tables
3. Returns combined list

---

#### 4. AnalyzeIncrementalAsync() ⚡
**Incremental analysis** - analyzes only changed tables (FAST!)

```csharp
var changedTables = await analyzer.DetectChangedTablesAsync(previousSchema);

var incrementalSchema = await analyzer.AnalyzeIncrementalAsync(changedTables);

// Generate code only for changed tables
foreach (var table in incrementalSchema.Tables)
{
    GenerateCode(table);  // Your code generation here
}
```

**Returns:** `DatabaseSchema` - schema with only changed tables  
**Property:** `IsIncrementalAnalysis = true`

**Performance:** 1-5 seconds (vs 5-30 seconds for full analysis)

---

## Complete Workflow Example

### Scenario: Daily Code Generation Job

```csharp
using TargCC.Core.Analyzers.Database;
using Microsoft.Extensions.Logging;

public class CodeGenerationJob
{
    private readonly DatabaseAnalyzer _analyzer;
    private readonly string _schemaFilePath = "previous-schema.json";
    
    public CodeGenerationJob(string connectionString, ILogger logger)
    {
        _analyzer = new DatabaseAnalyzer(connectionString, logger);
    }
    
    public async Task RunAsync()
    {
        // Step 1: Test connection
        if (!await _analyzer.ConnectAsync())
        {
            Console.WriteLine("❌ Connection failed!");
            return;
        }
        
        // Step 2: Load previous schema
        DatabaseSchema? previousSchema = null;
        if (File.Exists(_schemaFilePath))
        {
            var json = await File.ReadAllTextAsync(_schemaFilePath);
            previousSchema = JsonSerializer.Deserialize<DatabaseSchema>(json);
        }
        
        // Step 3: Detect changes or do full analysis
        DatabaseSchema currentSchema;
        
        if (previousSchema != null)
        {
            // Incremental mode (FAST!)
            var changedTables = await _analyzer.DetectChangedTablesAsync(previousSchema);
            
            if (changedTables.Count == 0)
            {
                Console.WriteLine("✅ No changes detected!");
                return;
            }
            
            Console.WriteLine($"🔄 Found {changedTables.Count} changed tables");
            currentSchema = await _analyzer.AnalyzeIncrementalAsync(changedTables);
        }
        else
        {
            // Full analysis (first time)
            Console.WriteLine("🚀 Running full analysis...");
            currentSchema = await _analyzer.AnalyzeAsync();
        }
        
        // Step 4: Generate code
        foreach (var table in currentSchema.Tables)
        {
            Console.WriteLine($"📝 Generating code for: {table.Name}");
            await GenerateCodeForTable(table);
        }
        
        // Step 5: Save current schema for next run
        var schemaJson = JsonSerializer.Serialize(currentSchema);
        await File.WriteAllTextAsync(_schemaFilePath, schemaJson);
        
        Console.WriteLine("✅ Code generation complete!");
    }
    
    private async Task GenerateCodeForTable(Table table)
    {
        // Your code generation logic here
        // - Generate Entity class
        // - Generate Repository
        // - Generate API endpoints
        // etc.
    }
}
```

---

## TableAnalyzer

### Purpose
Analyzes individual table structure including columns, indexes, and keys.

### Key Methods

```csharp
public async Task<Table> AnalyzeTableAsync(string fullTableName)
```

**Example:**
```csharp
var tableAnalyzer = new TableAnalyzer(connectionString, logger);
var table = await tableAnalyzer.AnalyzeTableAsync("dbo.Customer");

Console.WriteLine($"Table: {table.Name}");
Console.WriteLine($"Schema: {table.SchemaName}");
Console.WriteLine($"Columns: {table.Columns.Count}");
Console.WriteLine($"Primary Key: {table.PrimaryKeyColumn}");
Console.WriteLine($"Indexes: {table.Indexes.Count}");
```

**Returns:** `Table` object with:
- Columns
- Indexes (unique + non-unique)
- Primary key
- Schema name

---

## ColumnAnalyzer

### Purpose
Analyzes column metadata including type, nullability, and special prefixes.

### Supported Prefixes (10+)

| Prefix | Meaning | Example |
|--------|---------|---------|
| `eno` | One-way encryption (SHA256) | `enoPassword` |
| `ent` | Two-way encryption | `entCreditCard` |
| `enm` | Enumeration | `enmStatus` |
| `lkp` | Lookup table reference | `lkpCountry` |
| `loc` | Localizable field | `locDescription` |
| `clc_` | Calculated field (read-only) | `clc_TotalPrice` |
| `blg_` | Business logic field | `blg_Score` |
| `agg_` | Aggregate field | `agg_OrderCount` |
| `spt_` | Separate update field | `spt_Notes` |
| `FUI_` | Fake Unique Index | `FUI_InvoiceNo` |

### Key Methods

```csharp
public async Task<ColumnPrefix> DetermineColumnPrefixAsync(Column column)
```

**Example:**
```csharp
var columnAnalyzer = new ColumnAnalyzer(connectionString, logger);

foreach (var column in table.Columns)
{
    var prefix = await columnAnalyzer.DetermineColumnPrefixAsync(column);
    
    if (prefix == ColumnPrefix.OneWayEncryption)
    {
        Console.WriteLine($"🔒 {column.Name} is one-way encrypted");
    }
}
```

---

## RelationshipAnalyzer

### Purpose
Analyzes foreign key relationships and builds relationship graph.

### Key Methods

```csharp
public async Task<List<Relationship>> AnalyzeRelationshipsAsync(List<Table> tables)
```

**Example:**
```csharp
var relationshipAnalyzer = new RelationshipAnalyzer(connectionString, logger);
var relationships = await relationshipAnalyzer.AnalyzeRelationshipsAsync(schema.Tables);

foreach (var rel in relationships)
{
    Console.WriteLine($"{rel.ParentTable}.{rel.ParentColumn} → " +
                      $"{rel.ChildTable}.{rel.ChildColumn}");
}
```

**Returns:** `List<Relationship>` with:
- Parent table/column
- Child table/column
- Relationship type (OneToMany, OneToOne, ManyToMany)
- Constraint name

---

## Best Practices

### 1. Use Incremental Analysis for Daily Jobs

```csharp
// ✅ GOOD - Fast incremental updates
var changes = await analyzer.DetectChangedTablesAsync(previousSchema);
if (changes.Count > 0)
{
    var schema = await analyzer.AnalyzeIncrementalAsync(changes);
    GenerateCode(schema);
}

// ❌ BAD - Full analysis every time (slow!)
var schema = await analyzer.AnalyzeAsync();
GenerateCode(schema);
```

### 2. Save Schema After Each Analysis

```csharp
// Save for next run's comparison
var json = JsonSerializer.Serialize(schema);
await File.WriteAllTextAsync("schema.json", json);
```

### 3. Always Test Connection First

```csharp
if (!await analyzer.ConnectAsync())
{
    logger.LogError("Connection failed!");
    return;
}
```

### 4. Use Structured Logging

```csharp
_logger.LogInformation("Starting analysis for {TableCount} tables", tables.Count);
```

### 5. Handle Errors Gracefully

```csharp
try
{
    var schema = await analyzer.AnalyzeAsync();
}
catch (InvalidOperationException ex)
{
    _logger.LogError(ex, "Analysis failed");
    // Fallback logic
}
```

---

## Performance Tips

### Large Databases (200+ tables)

1. **Use incremental mode** - 10x faster
2. **Parallel analysis** - analyze tables concurrently (future feature)
3. **Cache schemas** - avoid repeated full analysis
4. **Filter tables** - only analyze relevant tables

### Connection Pooling

```csharp
// Use connection pooling in connection string
"Server=localhost;Database=MyDb;Integrated Security=true;Max Pool Size=100;"
```

### Logging Levels

```csharp
// Production: Information
logger.SetMinimumLevel(LogLevel.Information);

// Development: Debug
logger.SetMinimumLevel(LogLevel.Debug);
```

---

## Troubleshooting

### Issue: "Connection failed"
**Solution:** Check connection string, SQL Server running, network access

### Issue: "No tables found"
**Solution:** User needs `EXECUTE` permission on database

### Issue: "Analysis is slow"
**Solution:** Use incremental mode, check database performance

### Issue: "Change detection not working"
**Solution:** Ensure previous schema has correct `AnalysisDate`

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0-rc1 | Nov 2025 | Initial release - Phase 1 complete |

---

## See Also

- [Core Principles](../CORE_PRINCIPLES.md)
- [Phase 1 Checklist](../Phase1_Checklist.md)
- [Project README](../README.md)

---

**Need Help?** Open an issue on GitHub or contact the development team.

**📖 Happy Coding with TargCC Core V2!** 🚀
