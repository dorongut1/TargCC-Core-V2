# 🚀 QuickStart - DatabaseAnalyzer

## התקנה מהירה (5 דקות)

### שלב 1: הורד את הקבצים
כל הקבצים שיצרנו נמצאים כאן:
- DatabaseAnalyzer.cs
- TableAnalyzer.cs
- ColumnAnalyzer.cs
- RelationshipAnalyzer.cs
- DatabaseAnalyzerTests.cs
- Enums.cs
- TargCC.Core.Analyzers.csproj
- TargCC.Core.Tests.csproj

### שלב 2: הרץ סקריפט התקנה

**Windows PowerShell:**
```powershell
cd C:\Disk1\TargCC-Core-V2
.\Setup.ps1
```

**או ידנית:**
```bash
# 1. צור תיקיות
mkdir src\TargCC.Core.Analyzers\Database
mkdir tests\TargCC.Core.Tests\Unit\Analyzers

# 2. העתק קבצים
# (ראה מבנה למטה)

# 3. Restore + Build
dotnet restore
dotnet build
```

### שלב 3: מבנה תיקיות נכון

```
C:\Disk1\TargCC-Core-V2\
├── src/
│   ├── TargCC.Core.Interfaces/
│   │   ├── Models/
│   │   │   ├── DatabaseSchema.cs     (קיים)
│   │   │   ├── Table.cs              (קיים)
│   │   │   ├── Column.cs             (קיים)
│   │   │   ├── Index.cs              (קיים)
│   │   │   ├── Relationship.cs       (קיים)
│   │   │   └── Enums.cs              ⬅️ חדש!
│   │   └── IAnalyzer.cs              (קיים)
│   │
│   └── TargCC.Core.Analyzers/
│       ├── Database/
│       │   ├── DatabaseAnalyzer.cs   ⬅️ חדש!
│       │   ├── TableAnalyzer.cs      ⬅️ חדש!
│       │   ├── ColumnAnalyzer.cs     ⬅️ חדש!
│       │   └── RelationshipAnalyzer.cs ⬅️ חדש!
│       └── TargCC.Core.Analyzers.csproj ⬅️ חדש!
│
└── tests/
    └── TargCC.Core.Tests/
        ├── Unit/
        │   └── Analyzers/
        │       └── DatabaseAnalyzerTests.cs ⬅️ חדש!
        └── TargCC.Core.Tests.csproj ⬅️ חדש!
```

---

## 🧪 בדיקה ראשונה

### 1. עדכן Connection String

פתח את `tests\TargCC.Core.Tests\Unit\Analyzers\DatabaseAnalyzerTests.cs`

מצא את השורה:
```csharp
_testConnectionString = "Server=(localdb)\\mssqllocaldb;Database=TargCCTest;Integrated Security=true;";
```

שנה ל-DB שלך:
```csharp
// אופציה 1: LocalDB
_testConnectionString = "Server=(localdb)\\mssqllocaldb;Database=TargCCOrders;Integrated Security=true;";

// אופציה 2: SQL Server מלא
_testConnectionString = "Server=localhost;Database=TargCCOrders;User Id=sa;Password=YourPassword;";

// אופציה 3: SQL Express
_testConnectionString = "Server=.\\SQLEXPRESS;Database=TargCCOrders;Integrated Security=true;";
```

### 2. הרץ Test

```bash
cd tests\TargCC.Core.Tests
dotnet test --filter "ConnectAsync_ValidConnection_ReturnsTrue"
```

אם הטסט עובר ✅ - מזל טוב! הכל עובד!

---

## 💻 קוד לדוגמה - שימוש ראשון

צור קובץ חדש: `Program.cs`

```csharp
using TargCC.Core.Analyzers.Database;
using Microsoft.Extensions.Logging;

// 1. הגדרת Logger
var loggerFactory = LoggerFactory.Create(builder => 
    builder.AddConsole().SetMinimumLevel(LogLevel.Information));
var logger = loggerFactory.CreateLogger<DatabaseAnalyzer>();

// 2. Connection String
var connectionString = "Server=(localdb)\\mssqllocaldb;Database=TargCCOrders;Integrated Security=true;";

// 3. יצירת Analyzer
var analyzer = new DatabaseAnalyzer(connectionString, logger);

// 4. בדיקת חיבור
Console.WriteLine("בודק חיבור ל-DB...");
if (!await analyzer.ConnectAsync())
{
    Console.WriteLine("❌ החיבור נכשל!");
    return;
}

Console.WriteLine("✅ החיבור הצליח!");
Console.WriteLine();

// 5. קריאת טבלאות
Console.WriteLine("קורא רשימת טבלאות...");
var tables = await analyzer.GetTablesAsync();

Console.WriteLine($"נמצאו {tables.Count} טבלאות:");
foreach (var table in tables)
{
    Console.WriteLine($"  - {table}");
}
Console.WriteLine();

// 6. ניתוח מלא
Console.WriteLine("מבצע ניתוח מלא...");
var schema = await analyzer.AnalyzeAsync();

Console.WriteLine($"✅ ניתוח הושלם!");
Console.WriteLine($"  DB: {schema.DatabaseName}");
Console.WriteLine($"  Server: {schema.ServerName}");
Console.WriteLine($"  Tables: {schema.Tables.Count}");
Console.WriteLine($"  Relationships: {schema.Relationships.Count}");
Console.WriteLine();

// 7. הצגת פרטי טבלה ראשונה
if (schema.Tables.Any())
{
    var firstTable = schema.Tables.First();
    Console.WriteLine($"דוגמה - טבלה: {firstTable.FullName}");
    Console.WriteLine($"  Columns: {firstTable.Columns.Count}");
    Console.WriteLine($"  Indexes: {firstTable.Indexes.Count}");
    Console.WriteLine($"  PK: {string.Join(", ", firstTable.PrimaryKeyColumns)}");
    
    Console.WriteLine("\n  עמודות:");
    foreach (var col in firstTable.Columns.Take(5))
    {
        Console.WriteLine($"    - {col.Name} ({col.DataType}) -> {col.DotNetType}");
    }
}
```

### הרצה:
```bash
dotnet run
```

---

## 📊 תוצאה צפויה

```
בודק חיבור ל-DB...
✅ החיבור הצליח!

קורא רשימת טבלאות...
נמצאו 15 טבלאות:
  - dbo.Customer
  - dbo.Order
  - dbo.OrderItem
  - dbo.Product
  ...

מבצע ניתוח מלא...
✅ ניתוח הושלם!
  DB: TargCCOrders
  Server: MyComputer\SQLEXPRESS
  Tables: 15
  Relationships: 12

דוגמה - טבלה: dbo.Customer
  Columns: 8
  Indexes: 3
  PK: CustomerID

  עמודות:
    - CustomerID (int) -> int
    - CustomerName (nvarchar) -> string
    - Email (nvarchar) -> string
    - Phone (varchar) -> string
    - CreatedDate (datetime) -> DateTime
```

---

## 🎓 למידה נוספת

### דוגמאות נוספות:
1. **Incremental Analysis** - ראה `README_DatabaseAnalyzer.md` דוגמה 3
2. **Change Detection** - זיהוי שינויים אוטומטי
3. **TargCC Prefixes** - עבודה עם eno, ent, lkp

### קריאה מומלצת:
- 📖 `README_DatabaseAnalyzer.md` - מדריך מלא
- 📖 `CORE_PRINCIPLES.md` - פילוסופיה של המערכת
- 📖 `Phase1_Checklist.md` - מה הלאה?

---

## ❓ פתרון בעיות

### Build error: "Cannot find IAnalyzer"
```
פתרון: ודא שיש Project Reference ל-TargCC.Core.Interfaces
```

### Test fails: "Cannot open database"
```
פתרון: עדכן Connection String או הרץ את הסקריפט:
CREATE DATABASE TargCCOrders;
```

### NuGet restore error
```
פתרון:
dotnet nuget locals all --clear
dotnet restore
```

---

## ✅ Checklist

- [ ] .NET 8 SDK מותקן
- [ ] Visual Studio 2022 / Rider
- [ ] SQL Server זמין
- [ ] Setup.ps1 רץ בהצלחה
- [ ] Build עובד
- [ ] Test ראשון עובר
- [ ] קוד לדוגמה רץ

---

## 🎉 סיימת!

**אתה מוכן להמשיך ל-Phase 1, שבוע 3!**

צעדים הבאים:
1. ✅ שבוע 1-2: DatabaseAnalyzer - **הושלם!**
2. ⏭️ שבוע 3: Plugin System + Config Manager
3. ⏭️ שבוע 4-5: Quality + Testing
4. ⏭️ שבוע 6: Integration

**Happy Coding! 🚀**
