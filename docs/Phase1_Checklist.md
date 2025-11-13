# שלב 1: Core Engine Refactoring - Checklist יומי 📋

## סטטוס כללי
- **זמן משוער**: 4-6 שבועות
- **התקדמות**: 0/14 משימות הושלמו
- **תאריך התחלה**: ______ 
- **יעד סיום**: ______

---

## 📅 שבוע 1-2: הקמה והעברת DBAnalyser (10-12 ימים)

### משימה 1: הקמת Solution (יום 1) ✅/❌
- [ ] יצירת Solution חדש: `TargCC.Core`
- [ ] פרויקט: `TargCC.Core.Engine` (Class Library .NET 8)
- [ ] פרויקט: `TargCC.Core.Interfaces` (Class Library .NET 8)
- [ ] פרויקט: `TargCC.Core.Tests` (xUnit)
- [ ] NuGet: `Dapper`, `Serilog`, `xUnit`, `Moq`
- [ ] Git: Repository + .gitignore + README.md

**צ'ק פוינט**: פתרון קומפילטי עם structure בסיסי

---

### משימה 2: תכנון מבנה (1-2 ימים) ✅/❌
- [ ] `IAnalyzer.cs` - ממשק בסיסי למנתחים
- [ ] `IGenerator.cs` - ממשק למחוללי קוד
- [ ] `IValidator.cs` - ממשק למאמתים
- [ ] `DatabaseSchema.cs` - מודל Schema
- [ ] `Table.cs`, `Column.cs` - מודלי נתונים
- [ ] מסמך: `Architecture.md`

**צ'ק פוינט**: Interfaces מוגדרים ומתועדים

---

### משימה 3: DatabaseAnalyzer (2-3 ימים) ✅/❌
- [ ] `DatabaseAnalyzer.cs` - מחלקה חדשה
- [ ] חיבור ל-DB (Connection String handling)
- [ ] קריאת רשימת טבלאות
- [ ] Dapper במקום ADO.NET ישיר
- [ ] Unit Test: `DatabaseAnalyzerTests.cs`
- [ ] בדיקה מול DB אמיתי

**צ'ק פוינט**: מצליח לקרוא רשימת טבלאות מ-DB

```csharp
// דוגמה למבנה הצפוי
public class DatabaseAnalyzer : IAnalyzer
{
    public Task<DatabaseSchema> AnalyzeAsync(string connectionString);
}
```

---

### משימה 4: TableAnalyzer + ColumnAnalyzer (3-4 ימים) ✅/❌
- [ ] `TableAnalyzer.cs`
- [ ] זיהוי Primary Keys
- [ ] זיהוי Indexes (Unique + Non-Unique)
- [ ] `ColumnAnalyzer.cs`
- [ ] זיהוי Types + Nullable
- [ ] זיהוי Foreign Keys
- [ ] Extended Properties (ccType, etc.)
- [ ] Unit Tests מקיפים
- [ ] השוואה לפלט VB.NET

**צ'ק פוינט**: מנתח טבלה מלאה כולל כל המטא-דאטה

---

### משימה 5: RelationshipAnalyzer (1-2 ימים) ✅/❌
- [ ] `RelationshipAnalyzer.cs`
- [ ] זיהוי Foreign Key Constraints
- [ ] בניית גרף קשרים
- [ ] One-to-Many, Many-to-One
- [ ] Unit Tests

**צ'ק פוינט**: DBAnalyser מלא ב-C# עם 60%+ כיסוי

---

## 📅 שבוע 3: Plugin System + Configuration (5 ימים)

### משימה 6: Plugin Architecture (2-3 ימים) ✅/❌
- [ ] `IPlugin.cs` interface
- [ ] `PluginLoader.cs` - טעינה דינמית
- [ ] Assembly Scanning אוטומטי
- [ ] DI Container (Microsoft.Extensions.DependencyInjection)
- [ ] `DatabaseAnalyzerPlugin.cs` - דוגמה
- [ ] Tests: Unit + Integration

**צ'ק פוינט**: Plugin נטען ומופעל באופן דינמי

```csharp
public interface IPlugin
{
    string Name { get; }
    string Version { get; }
    Task InitializeAsync(IServiceProvider services);
}
```

---

### משימה 7: Configuration System (2 ימים) ✅/❌
- [ ] `ConfigurationManager.cs`
- [ ] תמיכה ב-JSON configuration
- [ ] Environment Variables override
- [ ] הצפנת Sensitive data (Connection strings, passwords)
- [ ] Schema validation (JSON Schema)
- [ ] Unit Tests

**צ'ק פוינט**: Config נטען מ-JSON + הצפנה עובדת

---

## 📅 שבוע 4-5: Quality + Testing (8-10 ימים)

### משימה 8: Code Quality Tools (1 יום) ✅/❌
- [ ] StyleCop.Analyzers
- [ ] `.editorconfig` מוגדר
- [ ] SonarAnalyzer.CSharp
- [ ] GitHub Actions - CI Pipeline בסיסי
- [ ] Build + Test אוטומטי

**צ'ק פוינט**: CI רץ על כל commit

---

### משימה 9: רפקטורינג (3-4 ימים) ✅/❌
- [ ] פונקציות < 50 שורות
- [ ] Single Responsibility
- [ ] Serilog logging בכל מקום
- [ ] Try-Catch מסודר
- [ ] Async/Await בכל I/O
- [ ] Performance Profiling

**צ'ק פוינט**: SonarQube Grade A

---

### משימה 10: Testing Framework (2-3 ימים) ✅/❌
- [ ] Unit Tests - 80%+ coverage
- [ ] Integration Tests
- [ ] In-Memory DB לטסטים
- [ ] Moq לכל התלויות
- [ ] Test Data Builders
- [ ] CI מריץ טסטים

**צ'ק פוינט**: 80%+ Code Coverage

---

### משימה 11: תיעוד (1-2 ימים) ✅/❌
- [ ] XML Comments לכל API
- [ ] README.md מפורט
- [ ] Architecture Decision Records (ADR)
- [ ] DocFX documentation site
- [ ] Examples בתיעוד

**צ'ק פוינט**: 100% documented APIs

---

## 📅 שבוע 6: אינטגרציה (3-5 ימים)

### משימה 12: VB.NET Bridge (2 ימים) ✅/❌
- [ ] `TargCC.Bridge` project
- [ ] COM Interop או C++/CLI
- [ ] Wrapper functions לכל APIs
- [ ] בדיקות אינטגרציה

**צ'ק פוינט**: VB.NET קורא ל-C# API

---

### משימה 13: בדיקות מערכת (2-3 ימים) ✅/❌
- [ ] בדיקה מול TargCCOrders
- [ ] השוואת פלטים: VB vs C#
- [ ] Performance Benchmarks
- [ ] Edge Cases
- [ ] תיקון באגים

**צ'ק פוינט**: פלט זהה ב-95%+

---

### משימה 14: Release Candidate (1 יום) ✅/❌
- [ ] Tag: v1.0.0-rc1
- [ ] Release Notes
- [ ] NuGet Package
- [ ] הכנה להטמעה

**צ'ק פוינט**: RC1 מוכן לשימוש! 🎉

---

## 🎯 קריטריונים להצלחה

| קריטריון | יעד | סטטוס |
|---------|-----|-------|
| Code Coverage | 80%+ | ___% |
| SonarQube Grade | A | ___ |
| Performance | Same or Better | ___ |
| Documented APIs | 100% | ___% |
| Backward Compat | 100% | ___% |

---

## 📊 התקדמות שבועית

### שבוע 1
- **משימות**: ___ / 5
- **בעיות**: ___________
- **למידה**: ___________

### שבוע 2
- **משימות**: ___ / 5
- **בעיות**: ___________
- **למידה**: ___________

### שבוע 3
- **משימות**: ___ / 2
- **בעיות**: ___________
- **למידה**: ___________

### שבוע 4
- **משימות**: ___ / 4
- **בעיות**: ___________
- **למידה**: ___________

### שבוע 5
- **משימות**: ___ / 4
- **בעיות**: ___________
- **למידה**: ___________

### שבוע 6
- **משימות**: ___ / 3
- **בעיות**: ___________
- **למידה**: ___________

---

## 🚀 צעדים מיידיים - התחלה היום

1. [ ] יצירת GitHub Repository: `TargCC-Core-V2`
2. [ ] Clone locally
3. [ ] Visual Studio 2022 - Solution חדש
4. [ ] 4 פרויקטים ראשונים
5. [ ] Git: First commit "Initial structure"
6. [ ] משימה 1: `DatabaseAnalyzer.cs` - שורה ראשונה!

---

## 💡 טיפים חשובים

- **Commit קטנים** - אחרי כל משימה משנית
- **Test First** - TDD כשאפשר
- **תיעוד בזמן אמת** - לא בסוף
- **Code Review** - לפני merge לראשי
- **Break גדול** - כל 90 דקות

---

## ❓ שאלות ובעיות

רשום כאן כל שאלה או בעיה שעולה:

1. _______________________________________________
2. _______________________________________________
3. _______________________________________________

---

**עדכון אחרון**: _______________  
**מעדכן**: _______________
