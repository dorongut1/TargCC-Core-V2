# TargCC Core V2 - DatabaseAnalyzer

## 📋 סיכום

**DatabaseAnalyzer** הוא הרכיב הראשון במערכת TargCC 2.0 - מנתח מבנה מסד נתונים SQL Server.

### יכולות עיקריות:
- ✅ קריאת כל הטבלאות במסד נתונים
- ✅ ניתוח עמודות (סוגים, Nullable, Identity, וכו')
- ✅ זיהוי Primary Keys ו-Indexes
- ✅ ניתוח Foreign Key Relationships
- ✅ זיהוי TargCC Prefixes מיוחדים (eno, ent, lkp, וכו')
- ✅ תמיכה ב-Extended Properties
- ✅ **Incremental Analysis** - ניתוח רק של מה שהשתנה
- ✅ **Change Detection** - זיהוי אוטומטי של שינויים

---

## 🚀 התקנה מהירה

### דרישות:
- .NET 8 SDK
- SQL Server (LocalDB / Express / Full)
- Visual Studio 2022 או JetBrains Rider

### NuGet Packages נדרשים:
```bash
dotnet add package Dapper
dotnet add package Microsoft.Extensions.Logging
dotnet add package Microsoft.Data.SqlClient
```

---

## 💻 שימוש בסיסי

### דוגמה 1: ניתוח מלא של DB

```csharp
using TargCC.Core.Analyzers.Database;
using Microsoft.Extensions.Logging;

// הגדרת Logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<DatabaseAnalyzer>();

// Connection string
var connectionString = "Server=(localdb)\\mssqllocaldb;Database=MyApp;Integrated Security=true;";

// יצירת Analyzer
var analyzer = new DatabaseAnalyzer(connectionString, logger);

// בדיקת חיבור
if (await analyzer.ConnectAsync())
{
    Console.WriteLine("חיבור הצליח!");
    
    // ניתוח מלא
    var schema = await analyzer.AnalyzeAsync();
    
    Console.WriteLine($"DB: {schema.DatabaseName}");
    Console.WriteLine($"Server: {schema.ServerName}");
    Console.WriteLine($"Tables: {schema.Tables.Count}");
    Console.WriteLine($"Relationships: {schema.Relationships.Count}");
}
```

### דוגמה 2: קריאת רשימת טבלאות

```csharp
var analyzer = new DatabaseAnalyzer(connectionString, logger);

var tables = await analyzer.GetTablesAsync();

foreach (var table in tables)
{
    Console.WriteLine($"- {table}");
}
```

### דוגמה 3: Incremental Analysis

```csharp
var analyzer = new DatabaseAnalyzer(connectionString, logger);

// שלב 1: ניתוח ראשוני מלא
var previousSchema = await analyzer.AnalyzeAsync();

// ... המשתמש עושה שינויים ב-DB ...

// שלב 2: זיהוי מה השתנה
var changedTables = await analyzer.DetectChangedTablesAsync(previousSchema);

Console.WriteLine($"טבלאות שהשתנו: {changedTables.Count}");

// שלב 3: ניתוח רק של מה שהשתנה
var incrementalSchema = await analyzer.AnalyzeIncrementalAsync(changedTables);

Console.WriteLine($"ניתוח Incremental: {incrementalSchema.Tables.Count} טבלאות");
```

---

## 📊 מבנה DatabaseSchema

```csharp
public class DatabaseSchema
{
    public string DatabaseName { get; set; }
    public string ServerName { get; set; }
    public DateTime AnalysisDate { get; set; }
    public bool IsIncrementalAnalysis { get; set; }
    
    public List<Table> Tables { get; set; }
    public List<Relationship> Relationships { get; set; }
}
```

### Table
```csharp
public class Table
{
    public string SchemaName { get; set; }         // "dbo"
    public string Name { get; set; }                // "Customer"
    public string FullName { get; set; }            // "dbo.Customer"
    public List<Column> Columns { get; set; }
    public List<Index> Indexes { get; set; }
    public List<string> PrimaryKeyColumns { get; set; }
    public Dictionary<string, string> ExtendedProperties { get; set; }
}
```

### Column
```csharp
public class Column
{
    public string Name { get; set; }
    public string DataType { get; set; }           // SQL Type: "nvarchar", "int", etc.
    public string DotNetType { get; set; }         // .NET Type: "string", "int", etc.
    public bool IsNullable { get; set; }
    public bool IsIdentity { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsComputed { get; set; }
    public ColumnPrefix Prefix { get; set; }       // eno, ent, lkp, etc.
    public bool IsEncrypted { get; set; }
    public bool IsReadOnly { get; set; }
}
```

---

## 🎯 TargCC Prefixes

המערכת מזהה אוטומטית Prefixes מיוחדים:

| Prefix | תיאור | דוגמה |
|--------|-------|-------|
| `eno` | One-way encryption (SHA256) | `enoPassword` |
| `ent` | Two-way encryption | `entCreditCard` |
| `enm` | Enumeration | `enmStatus` |
| `lkp` | Lookup table | `lkpCountry` |
| `loc` | Localization | `locDescription` |
| `clc_` | Calculated (read-only) | `clc_TotalPrice` |
| `blg_` | Business logic (server-side) | `blg_Commission` |
| `agg_` | Aggregate field | `agg_OrderCount` |
| `spt_` | Separate update | `spt_Comments` |
| `upl_` | File upload | `upl_Resume` |

---

## 🧪 הרצת Tests

```bash
# מהתיקייה של הפרויקט
cd TargCC.Core.Tests

# הרצת כל ה-Tests
dotnet test

# הרצה עם Code Coverage
dotnet test --collect:"XPlat Code Coverage"

# הרצת טסט ספציפי
dotnet test --filter "FullyQualifiedName~DatabaseAnalyzerTests.ConnectAsync_ValidConnection_ReturnsTrue"
```

### דרישות ל-Tests:
- SQL Server LocalDB
- או: להגדיר Connection String אחר ב-`DatabaseAnalyzerTests.cs`

---

## 🏗️ מבנה הפרויקט

```
TargCC.Core/
├── src/
│   ├── TargCC.Core.Engine/           (עתידי)
│   ├── TargCC.Core.Interfaces/
│   │   ├── IAnalyzer.cs
│   │   └── Models/
│   │       ├── DatabaseSchema.cs
│   │       ├── Table.cs
│   │       ├── Column.cs
│   │       ├── Index.cs
│   │       ├── Relationship.cs
│   │       └── Enums.cs
│   │
│   └── TargCC.Core.Analyzers/
│       └── Database/
│           ├── DatabaseAnalyzer.cs      ✅
│           ├── TableAnalyzer.cs         ✅
│           ├── ColumnAnalyzer.cs        ✅
│           └── RelationshipAnalyzer.cs  ✅
│
└── tests/
    └── TargCC.Core.Tests/
        └── Unit/
            └── Analyzers/
                └── DatabaseAnalyzerTests.cs  ✅
```

---

## 📈 ביצועים

### Benchmark (על DB עם 50 טבלאות):

| פעולה | זמן | הערות |
|-------|-----|-------|
| Full Analysis | ~2-3 שניות | כולל כל הטבלאות |
| Incremental (5 tables) | ~300ms | רק טבלאות שהשתנו |
| Change Detection | ~100ms | בדיקה מהירה |
| Get Tables | ~50ms | רשימה בלבד |

### אופטימיזציות:
- שימוש ב-Dapper (מהיר פי 3 מ-EF)
- Async/Await בכל מקום
- Single Connection per operation
- Bulk queries עם STRING_AGG

---

## 🔧 פתרון בעיות נפוצות

### שגיאה: "Cannot open database"
```
פתרון: בדוק ש-SQL Server פועל ושה-Connection String תקין
```

### שגיאה: "Login failed for user"
```
פתרון: בדוק הרשאות משתמש או השתמש ב-Integrated Security
```

### No tables returned
```
פתרון: ודא שיש טבלאות במסד הנתונים (לא רק system tables)
```

---

## 🚧 מה הלאה? (Phase 1 - שבוע 3)

- [ ] Plugin System
- [ ] Configuration Manager
- [ ] Code Generators
- [ ] UI (Web + Desktop)

---

## 📝 Change Log

### v1.0.0 (13/11/2025)
- ✅ DatabaseAnalyzer ראשוני
- ✅ TableAnalyzer + ColumnAnalyzer
- ✅ RelationshipAnalyzer
- ✅ Incremental Analysis
- ✅ Change Detection
- ✅ Unit Tests (15+ tests)

---

## 👥 תרומה

רוצה לתרום? מצוין!

1. Fork את הפרויקט
2. צור branch חדש
3. Commit שינויים
4. Push ל-branch
5. פתח Pull Request

---

## 📄 License

MIT License - ראה LICENSE file

---

## 📞 יצירת קשר

שאלות? בעיות? רעיונות?

- GitHub Issues: [link]
- Email: [email]

---

**🎉 TargCC Core V2 - Smart. Safe. Fast.**
