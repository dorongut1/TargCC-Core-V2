# 🎯 גישה דינמית למטא-דטה - TargCC V2

**תאריך:** 09/12/2025
**עיקרון מנחה:** המערכת צריכה לעבוד עם **כל** בסיס נתונים, עם או בלי טבלאות c_*

---

## 🌟 העיקרון המרכזי

> **TargCC V2 קורא ישירות מה-Database Schema**
>
> טבלאות c_* הן **אופציונליות** ומשמשות רק ל:
> - Override של התנהגות ברירת מחדל
> - שמירת הגדרות קונפיגורציה
> - מעקב אחרי שינויים (Change Detection)

---

## 📊 3 מצבי עבודה

### מצב 1: Pure Dynamic (ללא c_*)
```
בסיס נתונים רגיל
    ↓
TargCC קורא מ-sys.tables, sys.columns, sys.indexes
    ↓
מייצר קוד לפי ברירות מחדל חכמות
    ↓
הכל עובד!
```

**דוגמה:**
```bash
targcc generate all Customer --database "Northwind"
# אין c_Table? אין בעיה!
# TargCC קורא מ-sys.tables ומייצר
```

---

### מצב 2: Hybrid (c_* חלקי)
```
בסיס נתונים + c_Table בלבד
    ↓
TargCC קורא מ-sys.* + בודק אם יש c_Table
    ↓
אם יש → משתמש בקונפיגורציה (CanAdd, CreateUI וכו')
אם אין → ברירת מחדל
    ↓
מייצר קוד
```

**דוגמה:**
```sql
-- יש c_Table עם קונפיגורציה
INSERT INTO c_Table (Name, CanAdd, CreateUIEntity)
VALUES ('Customer', 'Y', 1)

-- TargCC ישתמש בזה במקום ברירת מחדל
```

---

### מצב 3: Full Metadata (c_* מלא)
```
בסיס נתונים + טבלאות c_* מלאות
    ↓
TargCC קורא מ-c_* במקום sys.*
    ↓
יש קונפיגורציה מלאה, Schema Hash, היסטוריה
    ↓
Incremental Generation + Change Detection
```

**דוגמה:**
```bash
# Migration ראשונה
targcc sync --database "MyDB"
# יוצר c_Table, c_Column, c_Index, c_Relationship

# מעתה - עובד עם Change Detection
targcc generate all Customer
# רק מה שהשתנה!
```

---

## 🏗️ ארכיטקטורה: שכבות קריאה

```
┌─────────────────────────────────────────────┐
│         TargCC V2 Generator                 │
└─────────────────┬───────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────┐
│      ISchemaProvider (Interface)            │
│  - GetTables()                              │
│  - GetColumns(table)                        │
│  - GetIndexes(table)                        │
│  - GetRelationships(table)                  │
│  - GetTableConfig(table)                    │
└─────────────────┬───────────────────────────┘
                  │
        ┌─────────┴──────────┐
        │                    │
        ▼                    ▼
┌──────────────────┐  ┌──────────────────┐
│ SystemCatalog    │  │ MetadataTables   │
│ Provider         │  │ Provider         │
│                  │  │                  │
│ Reads from:      │  │ Reads from:      │
│ - sys.tables     │  │ - c_Table        │
│ - sys.columns    │  │ - c_Column       │
│ - sys.indexes    │  │ - c_Index        │
│ - sys.foreign_   │  │ - c_Relationship │
│   keys           │  │                  │
└──────────────────┘  └──────────────────┘
```

---

## 💡 דוגמה מעשית: איך זה עובד?

### קוד: GetTableInfo

```csharp
public class SchemaService
{
    private readonly ISchemaProvider _provider;

    public async Task<TableInfo> GetTableInfoAsync(string tableName)
    {
        // 1. נסה קודם c_Table (אם קיים)
        var config = await _provider.GetTableConfigAsync(tableName);

        if (config != null)
        {
            // יש c_Table! השתמש בקונפיגורציה
            return new TableInfo
            {
                Name = config.Name,
                CanAdd = config.CanAdd,
                CanEdit = config.CanEdit,
                GenerateEntity = config.GenerateEntity,
                // ... מכל השדות ב-c_Table
            };
        }
        else
        {
            // אין c_Table! קרא מ-sys.tables
            var sysTable = await _provider.GetSystemTableAsync(tableName);

            return new TableInfo
            {
                Name = sysTable.Name,
                // ברירות מחדל חכמות:
                CanAdd = true,  // ברירת מחדל
                CanEdit = true,
                GenerateEntity = true,
                // ...
            };
        }
    }
}
```

---

## 🎯 ברירות מחדל חכמות

כשאין c_Table, המערכת מחליטה לפי חוקים:

### 1. זיהוי אוטומטי של Prefix
```csharp
public static string DetectPrefix(string columnName)
{
    var prefixes = new[] { "eno_", "ent_", "lkp_", "enm_", "clc_", "spt_" };

    foreach (var prefix in prefixes)
    {
        if (columnName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return prefix.TrimEnd('_');
    }

    return null; // No prefix
}
```

### 2. זיהוי אוטומטי של Audit Fields
```csharp
public static bool IsAuditField(string columnName)
{
    var auditFields = new[]
    {
        "AddedBy", "AddedOn",
        "ChangedBy", "ChangedOn",
        "DeletedBy", "DeletedOn"
    };

    return auditFields.Contains(columnName, StringComparer.OrdinalIgnoreCase);
}
```

### 3. זיהוי אוטומטי של Primary Key
```sql
SELECT
    c.name AS ColumnName,
    CASE WHEN ic.index_id IS NOT NULL THEN 1 ELSE 0 END AS IsPrimaryKey
FROM sys.columns c
LEFT JOIN sys.index_columns ic
    ON c.object_id = ic.object_id
    AND c.column_id = ic.column_id
LEFT JOIN sys.indexes i
    ON ic.object_id = i.object_id
    AND ic.index_id = i.index_id
WHERE i.is_primary_key = 1
```

---

## 📋 טבלאות c_* - מתי ליצור?

### תרחיש 1: POC מהיר
```bash
# לא צריך c_*!
targcc generate all Customer --database "MyDB"
# עובד מיד
```

### תרחיש 2: פרויקט ייצור
```bash
# יצירת c_* למעקב שינויים
targcc sync --create-metadata --database "MyDB"

# עכשיו יש:
# - c_Table עם כל הטבלאות
# - c_Column עם כל העמודות
# - c_Index עם כל האינדקסים
# - c_Relationship עם כל ה-FKs
# - Schema Hash לכל אחד
```

### תרחיש 3: קונפיגורציה מותאמת
```sql
-- רוצה Override על טבלה ספציפית?
INSERT INTO c_Table (Name, GenerateReactUI, CanDelete)
VALUES ('Customer', 1, 0)  -- יצור React UI, אבל בלי Delete

-- כל השאר ברירת מחדל
```

---

## ✅ יתרונות הגישה הזו

### 1. גמישות מקסימלית
```
✅ עובד עם כל בסיס נתונים
✅ לא תלוי ב-c_* קיימים
✅ אפשר להוסיף c_* בכל שלב
```

### 2. אפס מחסום כניסה
```
✅ התקנה: 0 דקות (אין migration)
✅ שימוש ראשון: מייד
✅ POC: 5 דקות
```

### 3. התקדמות הדרגתית
```
Day 1:  עובדים ללא c_*
        ↓
Day 2:  מוסיפים c_Table למעקב
        ↓
Day 3:  מוסיפים c_Column ל-Prefix support
        ↓
Day 4:  מוסיפים c_Index ל-GetBy methods
        ↓
Day 5:  Incremental Generation מלא!
```

### 4. תאימות לאחור
```
✅ אם יש c_Table ישן (כמו Upay) → קורא ממנו
✅ אם יש c_TableFields → ממיר ל-c_Column
✅ אם אין כלום → עובד מ-sys.*
```

---

## 🏗️ תכנית יישום

### שלב 1: SystemCatalogProvider (יום 1)
```csharp
public class SystemCatalogProvider : ISchemaProvider
{
    public async Task<List<TableInfo>> GetTablesAsync()
    {
        // קריאה מ-sys.tables
        var sql = @"
            SELECT
                s.name AS SchemaName,
                t.name AS TableName,
                t.modify_date AS LastModified
            FROM sys.tables t
            JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE t.is_ms_shipped = 0";

        return await _db.QueryAsync<TableInfo>(sql);
    }

    public async Task<List<ColumnInfo>> GetColumnsAsync(string table)
    {
        // קריאה מ-sys.columns
        var sql = @"
            SELECT
                c.name AS ColumnName,
                t.name AS DataType,
                c.max_length,
                c.is_nullable,
                c.is_identity,
                -- Auto-detect prefix
                CASE
                    WHEN c.name LIKE 'eno_%' THEN 'eno'
                    WHEN c.name LIKE 'ent_%' THEN 'ent'
                    WHEN c.name LIKE 'lkp_%' THEN 'lkp'
                    ELSE NULL
                END AS Prefix
            FROM sys.columns c
            JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE object_id = OBJECT_ID(@TableName)";

        return await _db.QueryAsync<ColumnInfo>(sql, new { TableName = table });
    }
}
```

### שלב 2: MetadataTablesProvider (יום 2)
```csharp
public class MetadataTablesProvider : ISchemaProvider
{
    public async Task<List<TableInfo>> GetTablesAsync()
    {
        // קריאה מ-c_Table (אם קיים)
        if (!await TablesExistAsync("c_Table"))
            return new List<TableInfo>(); // fallback to SystemCatalog

        var sql = @"
            SELECT
                Name,
                CanAdd,
                CanEdit,
                GenerateEntity,
                GenerateReactUI,
                SchemaHash
            FROM c_Table
            WHERE IsActive = 1";

        return await _db.QueryAsync<TableInfo>(sql);
    }
}
```

### שלב 3: HybridProvider (יום 3)
```csharp
public class HybridProvider : ISchemaProvider
{
    private readonly SystemCatalogProvider _system;
    private readonly MetadataTablesProvider _metadata;

    public async Task<List<TableInfo>> GetTablesAsync()
    {
        // נסה c_Table קודם
        var fromMetadata = await _metadata.GetTablesAsync();

        if (fromMetadata.Any())
            return fromMetadata;

        // fallback ל-sys.tables
        return await _system.GetTablesAsync();
    }

    public async Task<TableInfo> GetTableInfoAsync(string tableName)
    {
        // נסה c_Table
        var config = await _metadata.GetTableConfigAsync(tableName);

        if (config != null)
        {
            // מצאנו ב-c_Table - השתמש בקונפיגורציה
            return config;
        }

        // לא מצאנו - קרא מ-sys.* עם ברירות מחדל
        var sysInfo = await _system.GetTableInfoAsync(tableName);
        return ApplyDefaults(sysInfo);
    }

    private TableInfo ApplyDefaults(TableInfo info)
    {
        return info with
        {
            CanAdd = true,
            CanEdit = true,
            CanDelete = true,
            GenerateEntity = true,
            GenerateRepository = true,
            GenerateController = true,
            GenerateReactUI = false  // opt-in
        };
    }
}
```

---

## 🎯 סיכום

### הגישה הנכונה לTargCC V2:

1. ✅ **עובד עם כל בסיס נתונים** - sys.* תמיד קיים
2. ✅ **c_* אופציונלי** - רק למי שרוצה קונפיגורציה/מעקב
3. ✅ **ברירות מחדל חכמות** - Prefix detection, Audit fields, וכו'
4. ✅ **Incremental אופציונלי** - רק אם יש c_* עם Schema Hash
5. ✅ **לא תלוי במערכת קיימת** - לא Upay, לא Legacy, כלום

### אין קשר ל-Upay!
הדוגמאות מ-Upay רק **הראו לנו מה היה ב-Legacy**.
עכשיו אנחנו יודעים מה היה טוב (Prefixes, Audit, Config) ו**בונים משהו טוב יותר**.

---

**זה מה שאתה התכוונת?** 🎯
