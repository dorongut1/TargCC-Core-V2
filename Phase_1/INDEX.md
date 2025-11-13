# 📁 INDEX - מדריך העתקת קבצים

## כל הקבצים שנוצרו + לאן להעתיק אותם

---

## 🎯 קבצי קוד עיקריים (5 קבצים)

### 1. DatabaseAnalyzer.cs
**מיקום יעד:** `C:\Disk1\TargCC-Core-V2\src\TargCC.Core.Analyzers\Database\DatabaseAnalyzer.cs`

**תיאור:** מנתח ראשי של מסד נתונים
- ניתוח מלא + Incremental
- Change Detection
- 300+ שורות קוד

---

### 2. TableAnalyzer.cs
**מיקום יעד:** `C:\Disk1\TargCC-Core-V2\src\TargCC.Core.Analyzers\Database\TableAnalyzer.cs`

**תיאור:** מנתח מבנה טבלה
- Primary Keys
- Indexes
- Extended Properties
- 200+ שורות קוד

---

### 3. ColumnAnalyzer.cs
**מיקום יעד:** `C:\Disk1\TargCC-Core-V2\src\TargCC.Core.Analyzers\Database\ColumnAnalyzer.cs`

**תיאור:** מנתח עמודות
- TargCC Prefix detection
- Type mapping
- ccType support
- 250+ שורות קוד

---

### 4. RelationshipAnalyzer.cs
**מיקום יעד:** `C:\Disk1\TargCC-Core-V2\src\TargCC.Core.Analyzers\Database\RelationshipAnalyzer.cs`

**תיאור:** מנתח קשרים
- Foreign Keys
- Relationship graph
- Parent/Child discovery
- 200+ שורות קוד

---

### 5. Enums.cs
**מיקום יעד:** `C:\Disk1\TargCC-Core-V2\src\TargCC.Core.Interfaces\Models\Enums.cs`

**תיאור:** Enums חדשים
- ColumnPrefix (12 סוגים)
- RelationshipType (3 סוגים)
- 100 שורות קוד

---

## 🧪 קבצי Tests (1 קובץ)

### 6. DatabaseAnalyzerTests.cs
**מיקום יעד:** `C:\Disk1\TargCC-Core-V2\tests\TargCC.Core.Tests\Unit\Analyzers\DatabaseAnalyzerTests.cs`

**תיאור:** 15+ Unit Tests
- Constructor validation
- Connection tests
- Full & Incremental analysis
- Change detection
- 200+ שורות קוד

---

## 📦 Project Files (2 קבצים)

### 7. TargCC.Core.Analyzers.csproj
**מיקום יעד:** `C:\Disk1\TargCC-Core-V2\src\TargCC.Core.Analyzers\TargCC.Core.Analyzers.csproj`

**תיאור:** Project file ל-Analyzers
- .NET 8
- Dapper, SqlClient, Logging
- Project References

---

### 8. TargCC.Core.Tests.csproj
**מיקום יעד:** `C:\Disk1\TargCC-Core-V2\tests\TargCC.Core.Tests\TargCC.Core.Tests.csproj`

**תיאור:** Project file ל-Tests
- xUnit
- Moq
- Code Coverage

---

## 🤖 סקריפט אוטומציה (1 קובץ)

### 9. Setup.ps1
**מיקום יעד:** `C:\Disk1\TargCC-Core-V2\Setup.ps1`

**תיאור:** סקריפט התקנה אוטומטי
- בדיקת .NET 8
- יצירת תיקיות
- העתקת קבצים
- Build + Test

**הרצה:**
```powershell
cd C:\Disk1\TargCC-Core-V2
.\Setup.ps1
```

---

## 📖 תיעוד (3 קבצים)

### 10. README_DatabaseAnalyzer.md
**מיקום יעד:** `C:\Disk1\TargCC-Core-V2\docs\README_DatabaseAnalyzer.md`

**תיאור:** מדריך מקיף (300+ שורות)
- יכולות המערכת
- דוגמאות קוד
- API Reference
- Benchmarks
- Troubleshooting

---

### 11. QUICKSTART.md
**מיקום יעד:** `C:\Disk1\TargCC-Core-V2\docs\QUICKSTART.md`

**תיאור:** התחלה מהירה (200+ שורות)
- התקנה ב-5 דקות
- קוד לדוגמה מוכן
- בדיקה ראשונה
- Checklist

---

### 12. SUMMARY.md
**מיקום יעד:** `C:\Disk1\TargCC-Core-V2\docs\SUMMARY.md`

**תיאור:** סיכום מלא
- מה נוצר
- Statistics
- Achievements
- Next steps

---

## 📋 Checklist העתקה

### שלב 1: הורד קבצים
- [ ] הורד את כל 12 הקבצים מ-outputs

### שלב 2: צור תיקיות (אם לא קיימות)
```powershell
mkdir C:\Disk1\TargCC-Core-V2\src\TargCC.Core.Analyzers\Database
mkdir C:\Disk1\TargCC-Core-V2\src\TargCC.Core.Interfaces\Models
mkdir C:\Disk1\TargCC-Core-V2\tests\TargCC.Core.Tests\Unit\Analyzers
mkdir C:\Disk1\TargCC-Core-V2\docs
```

### שלב 3: העתק קבצי קוד
- [ ] DatabaseAnalyzer.cs → `src\TargCC.Core.Analyzers\Database\`
- [ ] TableAnalyzer.cs → `src\TargCC.Core.Analyzers\Database\`
- [ ] ColumnAnalyzer.cs → `src\TargCC.Core.Analyzers\Database\`
- [ ] RelationshipAnalyzer.cs → `src\TargCC.Core.Analyzers\Database\`
- [ ] Enums.cs → `src\TargCC.Core.Interfaces\Models\`

### שלב 4: העתק Tests
- [ ] DatabaseAnalyzerTests.cs → `tests\TargCC.Core.Tests\Unit\Analyzers\`

### שלב 5: העתק Project Files
- [ ] TargCC.Core.Analyzers.csproj → `src\TargCC.Core.Analyzers\`
- [ ] TargCC.Core.Tests.csproj → `tests\TargCC.Core.Tests\`

### שלב 6: העתק תיעוד
- [ ] README_DatabaseAnalyzer.md → `docs\`
- [ ] QUICKSTART.md → `docs\`
- [ ] SUMMARY.md → `docs\`

### שלב 7: העתק סקריפט
- [ ] Setup.ps1 → שורש הפרויקט

---

## 🚀 אופציה מהירה: הרץ Setup.ps1

במקום להעתיק ידנית, אפשר להריץ את Setup.ps1 והוא יעשה הכל אוטומטית!

```powershell
# 1. העתק את Setup.ps1 לשורש הפרויקט
Copy-Item Setup.ps1 C:\Disk1\TargCC-Core-V2\

# 2. העתק את כל קבצי הקוד לאותה תיקייה
Copy-Item *.cs C:\Disk1\TargCC-Core-V2\
Copy-Item *.csproj C:\Disk1\TargCC-Core-V2\

# 3. הרץ את הסקריפט
cd C:\Disk1\TargCC-Core-V2
.\Setup.ps1
```

---

## 📊 סיכום קבצים

| **סוג** | **מספר** | **שורות** |
|---------|---------|-----------|
| Core Classes | 4 | ~950 |
| Models | 1 | ~100 |
| Tests | 1 | ~200 |
| Project Files | 2 | ~100 |
| Documentation | 3 | ~1000 |
| Scripts | 1 | ~100 |
| **Total** | **12** | **~2450** |

---

## ✅ לאחר ההעתקה

### בדוק Build:
```bash
cd C:\Disk1\TargCC-Core-V2
dotnet restore
dotnet build
```

### הרץ Tests:
```bash
dotnet test
```

### פתח ב-Visual Studio:
```
File → Open → Project/Solution
בחר: TargCC-Core-V2.sln
```

---

## 🎯 תוצאה צפויה

אחרי ההעתקה המוצלחת:
- ✅ Build עובד ללא שגיאות
- ✅ 15+ Tests עוברים
- ✅ DatabaseAnalyzer פועל
- ✅ מוכן לשבוע 3!

---

## 📞 עזרה

### אם Build נכשל:
1. בדוק ש-NuGet packages הורדו: `dotnet restore`
2. בדוק שיש References נכונים ב-.csproj
3. בדוק שכל הקבצים במקום הנכון

### אם Tests נכשלים:
1. עדכן Connection String ב-DatabaseAnalyzerTests.cs
2. ודא ש-SQL Server זמין
3. צור DB חדש אם צריך: `CREATE DATABASE TargCCTest;`

---

## 🎉 מוכן!

אחרי שתעתיק את כל הקבצים ו-Build יעבור:

**🎊 שבוע 1-2 הושלם בהצלחה!**

**⏭️ הצעד הבא: Plugin System (שבוע 3)**

---

**INDEX Created: 13/11/2025**
**TargCC Core V2 - Let's Build The Future! 🚀**
