# 🚀 TargCC Core V2 - הוראות התחלה

## ✅ מה נוצר?

נוצר פרויקט **C# .NET 8** חדש לגמרי עם המבנה הבא:

```
C:\Disk1\TargCC-Core-V2/
├── src/
│   ├── TargCC.Core.Engine/         ✅ מנוע ליבה
│   ├── TargCC.Core.Interfaces/     ✅ ממשקים ומודלים
│   ├── TargCC.Core.Analyzers/      ✅ מנתחי DB
│   └── TargCC.Core.Tests/          ✅ בדיקות
├── docs/                           ✅ תיעוד
├── scripts/                        ✅ סקריפטים
├── TargCC.Core.sln                 ✅ Solution File
├── .gitignore                      ✅ Git Ignore
├── .editorconfig                   ✅ Code Style
└── README.md                       ✅ תיעוד
```

---

## 🎯 צעדים הבאים (2 דקות!)

### שלב 1: הרץ את הסקריפט ⚡

פתח **PowerShell** ב-Administrator והרץ:

```powershell
cd C:\Disk1\TargCC-Core-V2
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
.\scripts\setup.ps1
```

**מה הסקריפט עושה?**
- ✅ בודק Git ו-.NET 8
- ✅ יוצר Git repository
- ✅ מוריד NuGet packages
- ✅ בונה את הפרויקט
- ✅ ה-commit הראשון

---

### שלב 2: פתח ב-Visual Studio 🎨

1. פתח **Visual Studio 2022**
2. **File > Open > Project/Solution**
3. בחר: `C:\Disk1\TargCC-Core-V2\TargCC.Core.sln`
4. לחץ **F6** לבנייה

**אמור לראות:**
```
Build succeeded
    4 Projects built successfully
```

---

### שלב 3: העלאה ל-GitHub (אופציונלי) 🌐

1. לך ל-https://github.com/new
2. שם Repository: **TargCC-Core-V2**
3. **אל תבחר** "Initialize with README" (יש כבר!)
4. לחץ **Create repository**

5. בחזרה ב-PowerShell:
```powershell
cd C:\Disk1\TargCC-Core-V2
git remote add origin https://github.com/YOUR-USERNAME/TargCC-Core-V2.git
git branch -M main
git push -u origin main
```

✅ **זהו! הפרויקט החדש מוכן!**

---

## 📋 מה יש בפרויקט?

### TargCC.Core.Interfaces ✨
ממשקים בסיסיים:
- ✅ `IAnalyzer` - ממשק למנתחים
- ✅ `IGenerator` - ממשק למחוללי קוד
- ✅ `IValidator` - ממשק למאמתים

מודלים:
- ✅ `DatabaseSchema` - מבנה מסד נתונים
- ✅ `Table` - טבלה
- ✅ `Column` - עמודה
- ✅ `Index` - אינדקס
- ✅ `Relationship` - קשר בין טבלאות

### TargCC.Core.Engine 🔧
מוכן לקבל:
- Plugin System
- Configuration Manager
- Logging Infrastructure

### TargCC.Core.Analyzers 🔍
מוכן לקבל:
- DatabaseAnalyzer
- TableAnalyzer
- ColumnAnalyzer
- RelationshipAnalyzer

### TargCC.Core.Tests 🧪
מוכן לכתיבת טסטים עם:
- xUnit
- Moq
- FluentAssertions

---

## 🎯 המשימה הראשונה - DBAnalyzer

עכשיו נתחיל בכתיבת **DatabaseAnalyzer** הראשון!

ראה את המסמך: `Phase1_Checklist.md`

**משימה 3 מחכה לנו!** 🚀

---

## 💡 טיפים

### VS 2022 Shortcuts:
- **F6** - Build Solution
- **Ctrl+Shift+B** - Build
- **Ctrl+K, Ctrl+D** - Format Document
- **F5** - Run Tests

### Git Commands:
```bash
git status          # מצב נוכחי
git add .           # הוסף הכל
git commit -m ""    # commit עם הודעה
git push            # העלה לGitHub
```

---

## ❓ בעיות נפוצות

**שגיאה: "execution policy"**
```powershell
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
```

**שגיאה: ".NET SDK not found"**
- הורד .NET 8: https://dotnet.microsoft.com/download

**שגיאה: "Git not found"**
- הורד Git: https://git-scm.com/downloads

---

## 📞 צריך עזרה?

פשוט שאל! 🙋‍♂️

---

**🎉 מזל טוב! יצרת את התשתית ל-TargCC 2.0!**

**הצעד הבא: בואו נכתוב את ה-DatabaseAnalyzer הראשון!** 💪
