# TargCC Core V2 🚀

**דור חדש של יצירת קוד חכמה - Incremental, Safe, Smart**

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![.NET Version](https://img.shields.io/badge/.NET-8.0-blue)]()
[![Phase 1](https://img.shields.io/badge/Phase%201-71%25-yellow)]()
[![Tests](https://img.shields.io/badge/tests-63%20passing-success)]()
[![Coverage](https://img.shields.io/badge/coverage-80%25%2B-brightgreen)]()
[![License](https://img.shields.io/badge/license-MIT-green)]()

---

## 🌟 מה זה TargCC 2.0?

TargCC הוא מערכת מתקדמת ליצירת אפליקציות 5-שכבות מלאות, שמבוססת על מבנה מסד נתונים.

**הגרסה החדשה משלבת:**
- ⚡ **Incremental Generation** - רק מה שהשתנה
- 🛡️ **Build Errors כ-Safety Net** - לא באג, Feature!
- 🤖 **AI Assistant** - מציע, לא מחליט
- 🎨 **Visual Designer** - UI מודרני
- 📦 **Git Integration** - Version control מובנה

---

## 🔑 העיקרון המרכזי

> **"Incremental Generation + Mandatory Manual Review"**

המערכת מייצרת קוד חכם ומהיר, אבל **אתה** בשליטה תמיד.

### 💡 Build Errors = טוב!

```
שינית CustomerID מ-string ל-int?

✅ קבצים אוטומטיים עודכנו
⚠️  3 Build Errors בקוד ידני

→ זה בדיוק מה שרצינו!
   עכשיו אתה יודע בדיוק איפה לתקן.
```

**קרא עוד:** [עקרונות מנחים](docs/CORE_PRINCIPLES.md)

---

## 📋 מה יוצא מהמערכת?

### אוטומטית:
- ✅ **8 פרויקטים מוכנים**
  - DBController (Business Logic)
  - DBStdController (.NET Standard)
  - TaskManager (Background Jobs)
  - WS (Web Service)
  - WSController (Client Logic)
  - WSStdController (.NET Standard)
  - WinF (Windows Forms)
  - Dependencies

- ✅ **Stored Procedures**
- ✅ **REST API** (עתידי)
- ✅ **React UI** (עתידי)
- ✅ **Unit Tests** (עתידי)

### עם שליטה מלאה:
- 🎯 **קוד ידני (*.prt)** - מוגן תמיד
- 🎯 **Build Errors ממוקדים** - רק איפה שצריך
- 🎯 **Preview לפני Generate**
- 🎯 **Rollback** - אפשר תמיד לחזור

---

## 🚀 התחלה מהירה

### דרישות מקדימות
- Visual Studio 2022 (17.8+)
- .NET 8 SDK
- SQL Server
- Git

### התקנה

```powershell
# 1. Clone הפרויקט
git clone https://github.com/dorongut1/TargCC-Core-V2.git
cd TargCC-Core-V2

# 2. הרץ Setup
.\scripts\setup-final.ps1

# 3. פתח ב-Visual Studio
start TargCC.Core.sln

# 4. Build!
# Press F6
```

**הוראות מפורטות:** [START_HERE.md](START_HERE.md)

---

## 📚 מסמכים

| מסמך | תיאור |
|------|-------|
| [עקרונות מנחים](docs/CORE_PRINCIPLES.md) | העקרונות המרכזיים של TargCC 2.0 |
| [אפיון מלא](docs/TargCC_אפיון_מעודכן_v2.1.docx) | מסמך אפיון מקיף |
| [שלבי פיתוח](docs/Phase1_פירוק_משימות.docx) | 14 משימות מפורטות |
| [תכנית שבועית](docs/Phase1_תכנית_שבועית.md) | 6 שבועות, שלב אחר שלב |
| [Checklist יומי](docs/Phase1_Checklist.md) | מעקב התקדמות |

---

## 🎯 מבנה הפרויקט

```
TargCC-Core-V2/
├── src/
│   ├── TargCC.Core.Engine/        # מנוע ליבה + Change Detection
│   ├── TargCC.Core.Interfaces/    # ממשקים ומודלים
│   ├── TargCC.Core.Analyzers/     # מנתחי DB
│   └── TargCC.Core.Tests/         # בדיקות
├── docs/                          # תיעוד מקיף
└── scripts/                       # סקריפטים עזר
```

---

## 💪 מה השתנה מהגרסה הישנה?

| **אספקט** | **CC ישן** | **CC 2.0** |
|-----------|-----------|------------|
| Generation | הכל מחדש (10 דק') | רק מה שהשתנה (1 דק') |
| Build Errors | הרבה, מבלבל | ממוקדים, מועילים |
| Preview | אין | יש - מפורט |
| Rollback | אין | Git integrated |
| UI | WinForms ישן | Web + Desktop |
| AI | אין | כן - חכם |
| קוד ידני (*.prt) | ✅ מעולה | ✅ נשמר! |

---

## 🛠️ טכנולוגיות

- **Backend:** C# .NET 8
- **Database:** SQL Server
- **UI:** WPF (Desktop) + React (Web)
- **AI:** OpenAI / Anthropic Claude
- **Testing:** xUnit + Moq + Playwright
- **CI/CD:** GitHub Actions
- **Version Control:** LibGit2Sharp

---

## 📊 סטטוס הפרויקט

### ✅ Phase 1: Core Engine Refactoring (71% - 10/14 משימות)

#### הושלם:
- [x] משימה 1-5: DBAnalyser מלא (Database, Table, Column, Relationship Analyzers)
- [x] משימה 6-7: Plugin System + Configuration Manager
- [x] משימה 8: Code Quality Tools (StyleCop, SonarQube, CI/CD)
- [x] משימה 9: רפקטורינג (32 helper methods, Grade A)
- [x] משימה 10: Testing Framework (63 tests, 80%+ coverage) 🎉

#### בעבודה:
- [ ] משימה 11: תיעוד (XML Comments + README + ADR)
- [ ] משימה 12: VB.NET Bridge
- [ ] משימה 13: בדיקות מערכת
- [ ] למשימה 14: Release Candidate

### 📅 Phase 2: Visual Designer + Smart Error Guide
- [ ] Web UI (ראקט)
- [ ] Visual Schema Designer
- [ ] Smart Error Guide
- [ ] Impact Analysis UI

### 📅 Phase 3: AI Integration
- [ ] AI Assistant
- [ ] Smart Suggestions
- [ ] Best Practices Analyzer
- [ ] Auto-naming conventions

**עדכון אחרון:** 14/11/2025  
**התקדמות מפורטת:** [Phase1_Checklist.md](docs/Phase1_Checklist.md)

---

## 🤝 תרומה

הפרויקט נמצא בפיתוח פעיל. רעיונות והצעות מתקבלים בברכה!

### איך לתרום?
1. Fork הפרויקט
2. צור branch חדש
3. Commit השינויים
4. Push ל-branch
5. פתח Pull Request

---

## 📞 יצירת קשר

- **GitHub:** [@dorongut1](https://github.com/dorongut1)
- **Project:** [TargCC-Core-V2](https://github.com/dorongut1/TargCC-Core-V2)

---

## 📝 רישיון

[ציין רישיון]

---

## 🎓 למידה ושימוש

### מתחילים?
1. קרא את [עקרונות מנחים](docs/CORE_PRINCIPLES.md)
2. עקוב אחרי [START_HERE.md](START_HERE.md)
3. בדוק את [Phase1 Checklist](docs/Phase1_Checklist.md)

### מתקדמים?
1. בדוק את [מסמך האפיון](docs/TargCC_אפיון_מעודכן_v2.1.docx)
2. עיין ב-[Architecture](docs/Architecture.md) (בקרוב)
3. תרום לפרויקט!

---

**🌟 TargCC 2.0 - Smart Code Generation, Your Way!**

> "המערכת חכמה, אבל אתה מחליט"
