# תכנית עבודה - שלב 1 📅

## סיכום מהיר: 6 שבועות, 14 משימות

---

## 🗓️ שבוע 1: יסודות

**יעד**: Solution + DatabaseAnalyzer מוכן

### יום 1 (2-3 שעות)
```bash
# בוקר
- יצירת GitHub repo
- VS 2022: Solution חדש + 4 projects
- Git: .gitignore + first commit

# צהריים  
- NuGet packages: Dapper, Serilog, xUnit
- Interfaces: IAnalyzer.cs
- Models: DatabaseSchema.cs, Table.cs
```

### יום 2-3 (4-6 שעות)
```bash
# DatabaseAnalyzer.cs
- Constructor + Connection String
- ConnectAsync() method
- GetTablesAsync() - קריאת רשימת טבלאות
- Test: DatabaseAnalyzerTests.cs (first test!)
```

### יום 4-5 (4-6 שעות)  
```bash
# TableAnalyzer.cs
- AnalyzeTableAsync(tableName)
- Primary Key detection
- Indexes (unique + non-unique)
- Tests: 5-10 test cases
```

**✅ Checkpoint**: מצליח לקרוא טבלה מלאה עם מטא-דאטה

---

## 🗓️ שבוע 2: השלמת Analyzers

**יעד**: DBAnalyser מלא עם 60% coverage

### יום 1-2 (4-5 שעות)
```bash
# ColumnAnalyzer.cs
- Type detection
- Nullable detection  
- Default values
- Extended Properties (ccType, etc.)
- Tests
```

### יום 3-4 (4-5 שעות)
```bash  
# RelationshipAnalyzer.cs
- Foreign Keys detection
- Relationship graph
- One-to-Many mapping
- Tests
```

### יום 5 (2-3 שעות)
```bash
# Integration
- End-to-End test: ניתוח DB מלא
- השוואה ל-VB.NET output
- Bug fixes
```

**✅ Checkpoint**: DBAnalyser C# מוכן ועובד!

---

## 🗓️ שבוע 3: תשתיות

**יעד**: Plugin System + Config Manager

### יום 1-2 (4-5 שעות)
```bash
# Plugin Architecture
- IPlugin.cs interface
- PluginLoader.cs
- DI Container setup
- DatabaseAnalyzerPlugin (example)
```

### יום 3-4 (4-5 שעות)
```bash
# Configuration System  
- ConfigurationManager.cs
- JSON support
- Encryption for sensitive data
- Validation
- Tests
```

### יום 5 (2-3 שעות)
```bash
# Integration
- Load plugin dynamically
- Config from JSON file
- End-to-End test
```

**✅ Checkpoint**: Plugin נטען + Config עובד

---

## 🗓️ שבוע 4: Quality

**יעד**: CI + איכות קוד

### יום 1 (2-3 שעות)
```bash
# Code Quality
- StyleCop + .editorconfig
- SonarAnalyzer
- GitHub Actions: basic CI
```

### יום 2-4 (6-8 שעות)
```bash  
# Refactoring
- פונקציות < 50 שורות
- Logging בכל מקום
- Error handling
- Async/Await
- Performance check
```

### יום 5 (2-3 שעות)
```bash
# Code review + fixes
- SonarQube report
- Coverage report
- Refine
```

**✅ Checkpoint**: Grade A + Clean code

---

## 🗓️ שבוע 5: Testing

**יעד**: 80% Coverage + תיעוד

### יום 1-3 (6-8 שעות)
```bash
# Tests Marathon
- Unit tests לכל class
- Integration tests
- Mocking עם Moq
- Test data builders
- Coverage: 80%+
```

### יום 4-5 (4-5 שעות)
```bash
# Documentation
- XML comments (100%)
- README.md
- Architecture.md
- ADR documents
- DocFX site (optional)
```

**✅ Checkpoint**: 80% coverage + מתועד

---

## 🗓️ שבוע 6: Integration

**יעד**: חיבור ל-VB.NET + RC1

### יום 1-2 (4-5 שעות)
```bash
# VB.NET Bridge
- TargCC.Bridge project
- COM/C++CLI wrapper
- Expose C# APIs
- Integration tests
```

### יום 3-4 (4-5 שעות)
```bash
# System Tests
- Test vs TargCCOrders
- Compare outputs: VB vs C#
- Performance benchmarks
- Bug fixes
```

### יום 5 (2-3 שעות)
```bash
# Release
- Tag: v1.0.0-rc1
- Release notes
- NuGet package
- Celebrate! 🎉
```

**✅ Checkpoint**: Core Engine RC1 מוכן!

---

## 🎯 זמנים צפויים (שעות נטו)

| שבוע | שעות משוערות | מה נעשה |
|------|-------------|---------|
| 1 | 14-18 | Solution + DB Analyzer |
| 2 | 14-18 | השלמת Analyzers |
| 3 | 10-14 | Plugins + Config |
| 4 | 10-14 | Quality + Refactoring |
| 5 | 10-14 | Tests + Docs |
| 6 | 10-14 | Integration + Release |
| **סה"כ** | **68-92** | **6 שבועות** |

---

## 💡 המלצות לניהול זמן

### אם יש 2-3 שעות ביום
- קדימה לפי התכנית
- סיום צפוי: 6-7 שבועות

### אם יש 4-5 שעות ביום  
- אפשר לסיים קצת מהר יותר
- סיום צפוי: 4-5 שבועות

### אם יש רק שעה ביום
- תארגן יותר זמן או הפסקות
- סיום צפוי: 12-14 שבועות

---

## 🚨 אזהרות חשובות

1. **אל תדלג על Tests** - זה חוסך זמן בהמשך
2. **אל תרפקטור מדי** - Perfect is enemy of good
3. **תעשה commits קטנים** - קל יותר לחזור אחורה
4. **תכתוב תיעוד בזמן אמת** - לא בסוף
5. **תבקש Code Review** - זוג עיניים נוסף

---

## 📞 מתי לבקש עזרה?

- תקוע יותר מ-2 שעות באותה בעיה
- לא מבין מושג/טכנולוגיה
- ספק לגבי החלטה ארכיטקטונית
- Performance issue שלא ברור

**אל תתבייש לשאול! 💪**

---

## ✅ Checklist לפני התחלה

- [ ] Visual Studio 2022 מותקן
- [ ] .NET 8 SDK
- [ ] Git + GitHub account
- [ ] SQL Server (או DB אחר)
- [ ] יש DB לבדיקות
- [ ] הבנת VB.NET code הקיים
- [ ] יש זמן פנוי קבוע!

---

**מוכן להתחיל? 🚀**

**צעד ראשון: יצירת GitHub Repository עכשיו!**
