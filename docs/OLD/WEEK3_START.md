# 🚀 TargCC Core V2 - Phase 1, Week 3: Plugin System + Configuration

**תאריך:** [הכנס תאריך]  
**שלב נוכחי:** Week 3 - Plugin Architecture  
**מצב:** Week 1-2 הושלמו בהצלחה ✅

---

## 📋 סיכום Week 1-2 (הושלם!)

### מה השגנו:
✅ **DatabaseAnalyzer מלא** - ניתוח DB + Incremental + Change Detection  
✅ **4 Analyzers** - Database, Table, Column, Relationship  
✅ **15+ Unit Tests** - כולם עוברים, ~70% coverage  
✅ **.NET 9 Support** - עדכון מ-.NET 8  
✅ **תיקון כל הבאגים** - Build + Tests עובדים מצוין  

### קבצים שנוצרו:
- `DatabaseAnalyzer.cs` - 300+ שורות
- `TableAnalyzer.cs` - 200+ שורות  
- `ColumnAnalyzer.cs` - 250+ שורות
- `RelationshipAnalyzer.cs` - 200+ שורות
- `DatabaseAnalyzerTests.cs` - 15+ tests
- Models עודכנו (DatabaseSchema, Table, Column, Relationship, Enums)

---

## 🎯 Week 3 - המשימות שלנו

### משימה 6: Plugin Architecture (2-3 ימים)

**קבצים ליצירה:**
```
src/TargCC.Core.Engine/
├── PluginSystem/
│   ├── IPlugin.cs                    ← ממשק בסיסי
│   ├── PluginMetadata.cs            ← מטא-דאטה
│   ├── PluginLoader.cs              ← טעינה דינמית
│   └── PluginManager.cs             ← ניהול plugins
└── DependencyInjection/
    └── ServiceCollectionExtensions.cs ← DI helpers
```

**תכונות:**
- IPlugin interface עם Name, Version, Initialize
- Assembly scanning אוטומטי מ-`/plugins` folder
- DI Container (Microsoft.Extensions.DependencyInjection)
- DatabaseAnalyzerPlugin כדוגמה
- Unit + Integration Tests

---

### משימה 7: Configuration System (2 ימים)

**קבצים ליצירה:**
```
src/TargCC.Core.Engine/
└── Configuration/
    ├── IConfigurationManager.cs     ← ממשק
    ├── ConfigurationManager.cs      ← מימוש
    ├── ConfigModels.cs             ← מודלים
    └── EncryptionHelper.cs         ← הצפנה
```

**תכונות:**
- קריאה מ-JSON (`appsettings.json`)
- Environment Variables override
- הצפנת Connection Strings + Passwords
- Schema Validation (JSON Schema)
- Hot reload support
- Unit Tests

---

## 📂 קבצים חשובים לקרוא

### תכנון ומסמכים:
1. **`Phase1_Checklist.md`** - Checklist מפורט של Week 3
2. **`Phase1_תכנית_שבועית.md`** - תכנית שבועית
3. **`CORE_PRINCIPLES.md`** - עקרונות מנחים
4. **`FUTURE_FEATURES.md`** ⭐ **חדש!** - תכונות לעתיד (Views, SPs, Code Gen)

### קוד קיים:
5. **`src/TargCC.Core.Interfaces/`** - כל ה-Interfaces
6. **`src/TargCC.Core.Analyzers/`** - DatabaseAnalyzer עובד
7. **`src/TargCC.Core.Tests/`** - Tests שעוברים

---

## ⚙️ Configuration - מה נצטרך?

### appsettings.json structure:
```json
{
  "TargCC": {
    "Database": {
      "ConnectionString": "encrypted_string_here",
      "Provider": "SqlServer"
    },
    "Plugins": {
      "Directory": "./plugins",
      "AutoLoad": true,
      "Enabled": ["DatabaseAnalyzer", "TableAnalyzer"]
    },
    "Analysis": {
      "IncludeViews": false,
      "IncludeStoredProcs": false,
      "IncrementalMode": true
    },
    "Output": {
      "BasePath": "C:\\Output\\Generated",
      "BackupEnabled": true
    }
  }
}
```

---

## 🚨 חשוב לזכור!

### ✅ עשה:
- Plugin System עם DI
- Configuration מ-JSON
- Tests לכל דבר
- תיעוד ב-XML Comments

### ❌ אל תעשה עכשיו:
- Views Support (Week 4+)
- SP Analysis (Week 4+)  
- Code Generation (Week 4-5)
- Legacy Import (Week 6)

**→ ראה `FUTURE_FEATURES.md` למה נדחה**

---

## 💻 סביבת פיתוח

**מיקום:** `C:\Disk1\TargCC-Core-V2\`

**SDK:** .NET 9.0.304

**מבנה Solution:**
```
TargCC-Core-V2/
├── src/
│   ├── TargCC.Core.Interfaces/      ✅ קיים
│   ├── TargCC.Core.Analyzers/       ✅ קיים
│   ├── TargCC.Core.Engine/          ⏭️ נשתמש בו עכשיו!
│   └── TargCC.Core.Tests/           ✅ קיים
├── TestAnalyzer/                    ✅ קיים - לבדיקות מהירות
├── FUTURE_FEATURES.md               ✅ חדש!
└── Phase1_*.md                      ✅ מסמכי תכנון
```

---

## 🎯 יעדי Week 3

### Success Criteria:
- [ ] IPlugin interface מוגדר ומתועד
- [ ] PluginLoader טוען plugins מ-Assembly
- [ ] DI Container מוגדר (ServiceCollection)
- [ ] DatabaseAnalyzerPlugin עובד כדוגמה
- [ ] ConfigurationManager קורא JSON + Environment
- [ ] הצפנה/פענוח של Sensitive data
- [ ] 10+ Unit Tests עוברים
- [ ] תיעוד מלא

### זמן משוער: 5 ימי עבודה

---

## 🚀 להתחיל ב:

**אופציה 1: Plugin System קודם**
```
1. IPlugin interface
2. PluginLoader
3. DI Container
4. DatabaseAnalyzerPlugin example
```

**אופציה 2: Configuration קודם**
```
1. ConfigurationManager
2. JSON support
3. Encryption
4. Schema validation
```

**אופציה 3: במקביל** (מומלץ!)
```
יום 1-2: Plugin System
יום 3: DI + Config together
יום 4-5: Tests + Integration
```

---

## 📝 שאלות לפני שמתחילים?

1. **Plugin System או Configuration קודם?**
2. **DI Container - מה בדיוק צריך?**
3. **Configuration structure - מסכים עם ה-JSON למעלה?**
4. **יש שאלות על Week 1-2?**

---

## ✨ מוכנים להתחיל Week 3!

**המוטו:** Build working infrastructure, then extend!

**העיקרון:** Incremental, Tested, Documented

**היעד:** Plugin System + Configuration מוכנים ל-Code Generation בשבוע 4

---

**🎊 בהצלחה! Let's build the future of TargCC! 🚀**
