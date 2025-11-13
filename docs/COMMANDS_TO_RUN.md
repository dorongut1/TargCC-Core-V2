# 🔧 פקודות לבדיקה - Week 4, Day 1

**תאריך:** 13/11/2025

---

## ✅ לבדיקה מיידית (בVS או Terminal)

### 1. Restore Dependencies
```bash
cd C:\Disk1\TargCC-Core-V2
dotnet restore
```

**צפוי:** הורדת StyleCop.Analyzers + SonarAnalyzer.CSharp

---

### 2. Build הפרויקט
```bash
dotnet build
```

**צפוי:** 
- ⚠️ Warnings מ-StyleCop (תיעוד חסר, סגנון)
- ⚠️ Warnings מ-SonarAnalyzer (complexity, code smells)
- ✅ Build מצליח (עם warnings)

---

### 3. Build עם Analyzers מפורטים
```bash
dotnet build /p:RunCodeAnalysis=true
```

**צפוי:** רשימה מפורטת יותר של issues

---

### 4. ריצת הטסטים
```bash
dotnet test --verbosity normal
```

**צפוי:** כל הטסטים עוברים (60 tests)

---

### 5. Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

**צפוי:** דוח כיסוי (~77%)

---

## 📊 בדיקות נוספות (אופציונלי)

### Build עם TreatWarningsAsErrors
```bash
dotnet build /p:TreatWarningsAsErrors=true
```

**צפוי:** ❌ Build נכשל (הרבה warnings)  
**זה OK!** - נתקן את זה בימים הבאים

---

### בדיקת קוד ספציפי
```bash
dotnet build src/TargCC.Core.Analyzers/TargCC.Core.Analyzers.csproj
```

---

## 🎯 מה לחפש ב-Warnings

### StyleCop (SA-prefixed)
- **SA1600:** Missing XML documentation
- **SA1633:** Missing file header
- **SA1028:** Trailing whitespace
- **SA1101:** Prefix with 'this'

### SonarAnalyzer (S-prefixed)
- **S3776:** High cognitive complexity
- **S107:** Too many parameters
- **S1075:** Hardcoded URIs
- **S125:** Commented code
- **S1135:** TODO tags

### Code Analysis (CA-prefixed)
- **CA1303:** Localized strings
- **CA1062:** Validate arguments
- **CA2007:** ConfigureAwait

---

## 📝 דוגמה לפלט צפוי

```
Building...
  TargCC.Core.Interfaces -> bin\Debug\net9.0\TargCC.Core.Interfaces.dll
  TargCC.Core.Engine -> bin\Debug\net9.0\TargCC.Core.Engine.dll
  TargCC.Core.Analyzers -> bin\Debug\net9.0\TargCC.Core.Analyzers.dll

Build succeeded.

DatabaseAnalyzer.cs(45,9): warning SA1600: Elements should be documented [TargCC.Core.Analyzers]
TableAnalyzer.cs(78,13): warning S3776: Refactor this method to reduce its Cognitive Complexity [TargCC.Core.Analyzers]
ConfigurationManager.cs(120,21): warning SA1101: Prefix local calls with 'this' [TargCC.Core.Engine]

    156 Warning(s)
    0 Error(s)

Time Elapsed 00:00:12.34
```

---

## 🚀 הצעד הבא

1. הרץ את הפקודות למעלה
2. רשום כמה warnings יש (סה"כ)
3. חלק אותם לפי severity:
   - Critical
   - Warning
   - Suggestion

4. **מחר** נתחיל לתקן!

---

**זמן משוער לבדיקה:** 10-15 דקות

---

**נוצר:** 13/11/2025  
**למעקב:** רשום את מספר ה-warnings שקיבלת
