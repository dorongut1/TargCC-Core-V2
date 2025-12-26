# מסמך אפיון משימות פיתוח - TargCC-Core-V2

## מטרת המסמך
מסמך זה מרכז את כל משימות הפיתוח הנדרשות להשלמת V2 בסדר כרונולוגי, עם תלויות ברורות בין המשימות.

---

## עקרונות מנחים

### 1. אל תשכתב - עשה Reference!
קיימים DLLים עובדים שיש להשתמש בהם:
- `AuditCommon.dll` - מערכת Audit ב-SQL Server
- `TargetSMS019.dll` - שליחת SMS
- `TargetEncription.dll` - הצפנות

### 2. תמיכה כפולה
המערכת צריכה לתמוך גם בגישה המודרנית וגם בגישה הישנה (ccvwComboList Views).

### 3. יצירה אוטומטית של טבלאות מערכת
V2 צריך לבדוק אם טבלאות מערכת קיימות ואם לא - ליצור אותן.

### 4. עמידה בסטנדרטים של StyleCop - חובה!

**כל הקוד המיוצר חייב לעמוד בכללי StyleCop של הפרויקט. קוד שלא עומד בסטנדרטים לא יתקמפל!**

#### כללים עיקריים:

**1. Complexity - פשטות הקוד:**
```csharp
// ❌ אסור - מורכבות גבוהה מדי
public void ProcessData(Data data)
{
    if (data != null)
    {
        if (data.Items != null)
        {
            foreach (var item in data.Items)
            {
                if (item.IsValid)
                {
                    if (item.Type == "A")
                    {
                        // nested too deep!
                    }
                }
            }
        }
    }
}

// ✅ נכון - Early return, פחות קינון
public void ProcessData(Data data)
{
    if (data?.Items == null)
    {
        return;
    }

    foreach (var item in data.Items.Where(i => i.IsValid && i.Type == "A"))
    {
        ProcessItem(item);
    }
}
```

**2. רווחים ופורמט:**
```csharp
// ❌ אסור - רווחים מיותרים
public void Method( string param )
{
    var x  =  5;
    if( condition ){
    }
}

// ✅ נכון - פורמט תקני
public void Method(string param)
{
    var x = 5;
    if (condition)
    {
    }
}
```

**3. שמות ומשתנים:**
```csharp
// ❌ אסור
string s;           // שם לא ברור
int temp;           // שם גנרי מדי
var MyVariable;     // PascalCase למשתנה מקומי

// ✅ נכון
string customerName;
int recordCount;
var myVariable;     // camelCase למשתנים מקומיים
```

**4. אורך שורה ומתודות:**
- אורך שורה מקסימלי: 120 תווים
- אורך מתודה מקסימלי: ~30 שורות (אם יותר - לפצל)
- מספר פרמטרים מקסימלי: 5 (אם יותר - להשתמש ב-object)

**5. תיעוד XML:**
```csharp
// ❌ לא צריך XML docs לכל דבר
/// <summary>
/// Gets the ID.
/// </summary>
public int Id { get; set; }

// ✅ XML docs רק למתודות ציבוריות מורכבות
/// <summary>
/// Filters tables that should not generate UI components.
/// </summary>
/// <param name="table">The table to check.</param>
/// <returns>True if UI should be generated; otherwise, false.</returns>
public bool ShouldGenerateUI(TableInfo table)
```

**6. Using statements:**
```csharp
// ❌ אסור - usings בתוך namespace
namespace MyApp
{
    using System;
    using System.Linq;
}

// ✅ נכון - usings בחוץ
using System;
using System.Linq;

namespace MyApp
{
}
```

**7. Braces - סוגריים מסולסלים:**
```csharp
// ❌ אסור - בשורה אחת
if (condition) { DoSomething(); }

// ❌ אסור - בלי סוגריים
if (condition)
    DoSomething();

// ✅ נכון - תמיד עם סוגריים בשורות נפרדות
if (condition)
{
    DoSomething();
}
```

**8. Null checks:**
```csharp
// ❌ אסור - בדיקות ארוכות
if (obj != null && obj.Property != null && obj.Property.Value != null)

// ✅ נכון - null-conditional operator
if (obj?.Property?.Value != null)

// ✅ נכון - pattern matching
if (obj is { Property: { Value: not null } })
```

#### הגדרות StyleCop בפרויקט:

הפרויקט משתמש ב-`.editorconfig` ו-`stylecop.json`. וודא שהקוד עובר:
```bash
dotnet build  # חייב לעבור ללא warnings של StyleCop
```

#### רשימת בדיקה לפני Commit:

- [ ] אין שורות ארוכות מ-120 תווים
- [ ] אין מתודות ארוכות מ-30 שורות
- [ ] אין קינון עמוק (מקסימום 3 רמות)
- [ ] שמות משתנים ברורים ב-camelCase
- [ ] שמות מתודות ב-PascalCase
- [ ] אין רווחים מיותרים
- [ ] כל `if/for/foreach` עם `{ }`
- [ ] `using` מחוץ ל-namespace
- [ ] `dotnet build` עובר ללא warnings

---

## שלב 1: תיקוני קריטיים - הפעלת המערכת הבסיסית

### משימה 1.1: סינון Views מיוחדים מייצור UI
**עדיפות:** קריטי
**תלויות:** אין
**קבצים לתיקון:**
- `src/TargCC.Core.Generators/React/ReactFormComponentGenerator.cs`
- `src/TargCC.Core.Generators/React/ReactListComponentGenerator.cs`

**מה לעשות:**
```csharp
// הוסף פונקציה לזיהוי Views שלא צריכים UI
private bool ShouldGenerateUI(TableInfo table)
{
    // סנן ccvwComboList Views
    if (table.Name.StartsWith("ccvwComboList_", StringComparison.OrdinalIgnoreCase))
        return false;

    // סנן Views אחרים שהם לשימוש פנימי
    if (table.Name.StartsWith("ccvw", StringComparison.OrdinalIgnoreCase))
        return false;

    return true;
}
```

**בדיקה:** לאחר הרצה, לא אמורים להיווצר קומפוננטות React עבור ccvwComboList_* Views.

---

### משימה 1.2: סינון טבלאות מערכת (c_*) מייצור UI מלא
**עדיפות:** קריטי
**תלויות:** אין
**קבצים לתיקון:**
- `src/TargCC.Core.Generators/React/ReactFormComponentGenerator.cs`
- `src/TargCC.Core.Generators/React/ReactListComponentGenerator.cs`

**מה לעשות:**
```csharp
// רשימת טבלאות מערכת שלא צריכות UI
private static readonly HashSet<string> SystemTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "c_Enumeration",
    "c_AlertMessage",
    "c_Country",
    "c_Language",
    "c_Lookup",
    "c_Process",
    "c_SystemAudit",
    "c_User",
    "c_Role",
    "c_UserRole"
};

private bool IsSystemTable(string tableName)
{
    return tableName.StartsWith("c_", StringComparison.OrdinalIgnoreCase)
           || SystemTables.Contains(tableName);
}
```

**הערה:** אפשר גם לתת אפשרות לייצר UI עבורן דרך קונפיגורציה.

---

### משימה 1.3: סינון Stored Procedures מיותרים
**עדיפות:** קריטי
**תלויות:** אין
**קבצים לתיקון:**
- `src/TargCC.Core.Generators/Sql/SqlGenerator.cs`

**מה לעשות:**
```csharp
public void GenerateStoredProcedures(TableInfo table)
{
    // לא לייצר SPs עבור ccvwComboList Views
    if (table.Name.StartsWith("ccvwComboList_", StringComparison.OrdinalIgnoreCase))
    {
        return; // רק SP_Fill* נדרש - יטופל בנפרד
    }

    // Views - רק קריאה
    if (table.IsView)
    {
        GenerateSelectProcedures(table);
        return;
    }

    // טבלאות רגילות - כל הפעולות
    GenerateAllProcedures(table);
}
```

**תוצאה צפויה:** במקום ~1,100 SPs, יווצרו רק ~700 SPs רלוונטיים.

---

## שלב 2: טבלאות מערכת ותשתית

### משימה 2.1: יצירת Script לטבלאות מערכת
**עדיפות:** גבוהה
**תלויות:** אין
**קובץ חדש:** `src/TargCC.Core.Generators/Sql/SystemTablesGenerator.cs`

**טבלאות מערכת נדרשות:**
```sql
-- c_Enumeration - לניהול ENUMs
CREATE TABLE c_Enumeration (
    ID INT PRIMARY KEY IDENTITY,
    EnumType NVARCHAR(100) NOT NULL,
    EnumValue NVARCHAR(100) NOT NULL,
    EnumText NVARCHAR(200),
    EnumTextNS NVARCHAR(200),
    OrderNum INT DEFAULT 0,
    IsActive BIT DEFAULT 1
)

-- c_SystemAudit - עבור AuditCommon.dll
CREATE TABLE c_SystemAudit (
    ID BIGINT PRIMARY KEY IDENTITY,
    TableName NVARCHAR(128),
    RecordID INT,
    ActionType CHAR(1), -- I/U/D
    ActionDate DATETIME DEFAULT GETDATE(),
    UserID INT,
    OldValues NVARCHAR(MAX),
    NewValues NVARCHAR(MAX)
)

-- c_Lookup - טבלת lookup כללית
CREATE TABLE c_Lookup (
    ID INT PRIMARY KEY IDENTITY,
    LookupType NVARCHAR(100),
    LookupKey NVARCHAR(100),
    LookupValue NVARCHAR(500),
    IsActive BIT DEFAULT 1
)
```

**לוגיקה:**
```csharp
public class SystemTablesGenerator
{
    public string GenerateSystemTablesScript(bool checkExists = true)
    {
        var sb = new StringBuilder();

        if (checkExists)
        {
            sb.AppendLine("IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'c_Enumeration')");
            sb.AppendLine("BEGIN");
        }

        // CREATE TABLE statements...

        return sb.ToString();
    }
}
```

---

### משימה 2.2: יצירת SP עבור Audit
**עדיפות:** גבוהה
**תלויות:** משימה 2.1

**SP נדרש:**
```sql
CREATE PROCEDURE c__SystemAuditAdd
    @TableName NVARCHAR(128),
    @RecordID INT,
    @ActionType CHAR(1),
    @UserID INT = NULL,
    @OldValues NVARCHAR(MAX) = NULL,
    @NewValues NVARCHAR(MAX) = NULL
AS
BEGIN
    INSERT INTO c_SystemAudit (TableName, RecordID, ActionType, UserID, OldValues, NewValues)
    VALUES (@TableName, @RecordID, @ActionType, @UserID, @OldValues, @NewValues)
END
```

---

## שלב 3: מערכת Combo/Dropdown חכמה

### משימה 3.1: SmartAutocomplete Component
**עדיפות:** גבוהה
**תלויות:** אין
**קובץ חדש:** `templates/React/components/SmartAutocomplete.tsx`

**פיצ'רים נדרשים:**
- חיפוש Server-side עם debounce
- Paging (עמודים)
- Caching תוצאות
- תמיכה ב-RTL

**קוד בסיס:**
```tsx
import React, { useState, useCallback, useRef } from 'react';
import { debounce } from 'lodash';

interface SmartAutocompleteProps {
    apiEndpoint: string;
    valueField: string;
    textField: string;
    onChange: (value: any) => void;
    placeholder?: string;
    minChars?: number;
    debounceMs?: number;
    pageSize?: number;
}

export const SmartAutocomplete: React.FC<SmartAutocompleteProps> = ({
    apiEndpoint,
    valueField = 'id',
    textField = 'text',
    onChange,
    placeholder = 'חפש...',
    minChars = 2,
    debounceMs = 300,
    pageSize = 20
}) => {
    const [options, setOptions] = useState<any[]>([]);
    const [loading, setLoading] = useState(false);
    const [inputValue, setInputValue] = useState('');
    const cache = useRef<Map<string, any[]>>(new Map());

    const fetchOptions = useCallback(
        debounce(async (searchText: string) => {
            if (searchText.length < minChars) {
                setOptions([]);
                return;
            }

            // בדוק cache
            if (cache.current.has(searchText)) {
                setOptions(cache.current.get(searchText)!);
                return;
            }

            setLoading(true);
            try {
                const response = await fetch(
                    `${apiEndpoint}?search=${encodeURIComponent(searchText)}&pageSize=${pageSize}`
                );
                const data = await response.json();
                cache.current.set(searchText, data);
                setOptions(data);
            } catch (error) {
                console.error('Error fetching options:', error);
            } finally {
                setLoading(false);
            }
        }, debounceMs),
        [apiEndpoint, minChars, pageSize]
    );

    return (
        <div className="smart-autocomplete" dir="rtl">
            <input
                type="text"
                value={inputValue}
                onChange={(e) => {
                    setInputValue(e.target.value);
                    fetchOptions(e.target.value);
                }}
                placeholder={placeholder}
            />
            {loading && <div className="loading">טוען...</div>}
            {options.length > 0 && (
                <ul className="options-list">
                    {options.map((option) => (
                        <li
                            key={option[valueField]}
                            onClick={() => {
                                onChange(option);
                                setInputValue(option[textField]);
                                setOptions([]);
                            }}
                        >
                            {option[textField]}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
};
```

---

### משימה 3.2: יצירת ccvwComboList Views אוטומטית
**עדיפות:** גבוהה
**תלויות:** אין
**קובץ חדש:** `src/TargCC.Core.Generators/Sql/ComboListViewGenerator.cs`

**לוגיקה:**
```csharp
public class ComboListViewGenerator
{
    public string GenerateComboListView(TableInfo table)
    {
        // מצא את העמודה הראשית לתצוגה
        var textColumn = FindTextColumn(table);

        var sb = new StringBuilder();
        sb.AppendLine($"CREATE VIEW ccvwComboList_{table.Name}");
        sb.AppendLine("WITH SCHEMABINDING");
        sb.AppendLine("AS");
        sb.AppendLine("SELECT");
        sb.AppendLine($"    {table.PrimaryKey} AS ID,");
        sb.AppendLine($"    {textColumn} AS Text,");
        sb.AppendLine($"    {textColumn} AS TextNS"); // גרסה ללא רווחים/תווים מיוחדים
        sb.AppendLine($"FROM dbo.{table.Name}");

        return sb.ToString();
    }

    private string FindTextColumn(TableInfo table)
    {
        // חפש עמודות בשמות מוכרים
        var candidates = new[] { "Name", "Title", "Description", "Text", table.Name + "Name" };

        foreach (var candidate in candidates)
        {
            if (table.Columns.Any(c => c.Name.Equals(candidate, StringComparison.OrdinalIgnoreCase)))
                return candidate;
        }

        // ברירת מחדל - העמודה הטקסטית הראשונה
        var textCol = table.Columns.FirstOrDefault(c =>
            c.DataType.Contains("varchar", StringComparison.OrdinalIgnoreCase));

        return textCol?.Name ?? table.PrimaryKey;
    }
}
```

---

### משימה 3.3: Lookup API Controller
**עדיפות:** גבוהה
**תלויות:** משימה 3.2
**קובץ חדש:** `templates/API/LookupController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
public class LookupController : ControllerBase
{
    private readonly IDbConnection _db;

    [HttpGet("{tableName}")]
    public async Task<IActionResult> GetLookup(
        string tableName,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Validate table name to prevent SQL injection
        if (!IsValidLookupTable(tableName))
            return BadRequest("Invalid lookup table");

        var viewName = $"ccvwComboList_{tableName}";

        var sql = $@"
            SELECT ID, Text, TextNS
            FROM {viewName}
            WHERE (@search IS NULL OR Text LIKE '%' + @search + '%')
            ORDER BY Text
            OFFSET @offset ROWS
            FETCH NEXT @pageSize ROWS ONLY";

        var results = await _db.QueryAsync<LookupItem>(sql, new
        {
            search,
            offset = (page - 1) * pageSize,
            pageSize
        });

        return Ok(results);
    }

    private bool IsValidLookupTable(string tableName)
    {
        // רשימת טבלאות מותרות או בדיקה מול DB
        return !tableName.Contains("'") && !tableName.Contains(";");
    }
}
```

---

## שלב 4: מערכת Enums

### משימה 4.1: EnumGenerator - יצירת TypeScript Enums
**עדיפות:** בינונית
**תלויות:** משימה 2.1 (טבלת c_Enumeration)
**קובץ חדש:** `src/TargCC.Core.Generators/TypeScript/EnumGenerator.cs`

```csharp
public class EnumGenerator
{
    public async Task<string> GenerateEnumsFromDatabase(IDbConnection db)
    {
        var enums = await db.QueryAsync<EnumRecord>(@"
            SELECT EnumType, EnumValue, EnumText, EnumTextNS, OrderNum
            FROM c_Enumeration
            WHERE IsActive = 1
            ORDER BY EnumType, OrderNum");

        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated enums from c_Enumeration");
        sb.AppendLine("// Do not edit manually");
        sb.AppendLine();

        var grouped = enums.GroupBy(e => e.EnumType);

        foreach (var group in grouped)
        {
            sb.AppendLine($"export enum {group.Key} {{");
            foreach (var item in group)
            {
                sb.AppendLine($"    {item.EnumValue} = '{item.EnumValue}',");
            }
            sb.AppendLine("}");
            sb.AppendLine();

            // פונקציית תרגום
            sb.AppendLine($"export const {group.Key}Labels: Record<{group.Key}, string> = {{");
            foreach (var item in group)
            {
                sb.AppendLine($"    [{group.Key}.{item.EnumValue}]: '{item.EnumText}',");
            }
            sb.AppendLine("};");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
```

**פלט לדוגמה:**
```typescript
export enum EntityType {
    UD = 'UD',
    Customer = 'Customer',
    Card = 'Card',
}

export const EntityTypeLabels: Record<EntityType, string> = {
    [EntityType.UD]: 'משתמש',
    [EntityType.Customer]: 'לקוח',
    [EntityType.Card]: 'כרטיס',
};
```

---

### משימה 4.2: EnumGenerator - יצירת C# Enums
**עדיפות:** בינונית
**תלויות:** משימה 2.1
**קובץ חדש:** `src/TargCC.Core.Generators/CSharp/CSharpEnumGenerator.cs`

```csharp
public class CSharpEnumGenerator
{
    public async Task<string> GenerateEnumsFromDatabase(IDbConnection db)
    {
        var enums = await db.QueryAsync<EnumRecord>(@"
            SELECT EnumType, EnumValue, EnumText, OrderNum
            FROM c_Enumeration
            WHERE IsActive = 1
            ORDER BY EnumType, OrderNum");

        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated enums");
        sb.AppendLine("namespace Generated.Enums");
        sb.AppendLine("{");

        var grouped = enums.GroupBy(e => e.EnumType);

        foreach (var group in grouped)
        {
            sb.AppendLine($"    public enum {group.Key}");
            sb.AppendLine("    {");
            foreach (var item in group)
            {
                sb.AppendLine($"        {item.EnumValue},");
            }
            sb.AppendLine("    }");
            sb.AppendLine();

            // Extension method לתרגום
            sb.AppendLine($"    public static class {group.Key}Extensions");
            sb.AppendLine("    {");
            sb.AppendLine($"        public static string ToDisplayText(this {group.Key} value)");
            sb.AppendLine("        {");
            sb.AppendLine("            return value switch");
            sb.AppendLine("            {");
            foreach (var item in group)
            {
                sb.AppendLine($"                {group.Key}.{item.EnumValue} => \"{item.EnumText}\",");
            }
            sb.AppendLine("                _ => value.ToString()");
            sb.AppendLine("            };");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}
```

---

## שלב 5: Extended Properties ו-Audit

### משימה 5.1: שימוש ב-Extended Properties
**עדיפות:** בינונית
**תלויות:** אין
**קבצים לתיקון:**
- `src/TargCC.Core.Analysis/TableAnalyzer.cs`
- `src/TargCC.Core.Generators/React/ReactFormComponentGenerator.cs`

**Extended Properties נתמכים:**
- `ccUICreateMenu` - האם ליצור תפריט (ברירת מחדל: true)
- `ccUICreateEntity` - האם ליצור מסך עריכה (ברירת מחדל: true)
- `ccUICreateCollection` - האם ליצור מסך רשימה (ברירת מחדל: true)

```csharp
public class TableInfo
{
    // הוסף properties
    public bool CreateMenu { get; set; } = true;
    public bool CreateEntity { get; set; } = true;
    public bool CreateCollection { get; set; } = true;
}

// ב-TableAnalyzer
private void LoadExtendedProperties(TableInfo table, IDbConnection db)
{
    var props = db.Query<ExtendedPropertyRecord>(@"
        SELECT name, value
        FROM fn_listextendedproperty(NULL, 'schema', 'dbo', 'table', @tableName, NULL, NULL)",
        new { tableName = table.Name });

    foreach (var prop in props)
    {
        switch (prop.Name.ToLower())
        {
            case "ccuicreatemenu":
                table.CreateMenu = prop.Value == "1" || prop.Value.ToLower() == "true";
                break;
            case "ccuicreateentity":
                table.CreateEntity = prop.Value == "1" || prop.Value.ToLower() == "true";
                break;
            case "ccuicreatecollection":
                table.CreateCollection = prop.Value == "1" || prop.Value.ToLower() == "true";
                break;
        }
    }
}
```

---

### משימה 5.2: אינטגרציה עם AuditCommon.dll
**עדיפות:** בינונית
**תלויות:** משימה 2.1, משימה 2.2
**קובץ חדש:** `src/TargCC.Core.Generators/Sql/AuditTriggerGenerator.cs`

**הערה:** לא לשכתב את AuditCommon.dll! להשתמש בקיים.

```csharp
public class AuditTriggerGenerator
{
    public string GenerateAuditTrigger(TableInfo table)
    {
        // בדוק שה-CLR Assembly רשום
        // אם לא - תן הודעה למשתמש

        return $@"
-- Audit Trigger for {table.Name}
-- Requires AuditCommon.dll CLR Assembly
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trc_{table.Name}_Audit')
    DROP TRIGGER trc_{table.Name}_Audit
GO

CREATE TRIGGER [dbo].[trc_{table.Name}_Audit]
ON [dbo].[{table.Name}]
FOR INSERT, UPDATE, DELETE
AS EXTERNAL NAME [AuditCommon].[AuditCommon.Triggers].[AuditCommon]
GO
";
    }

    public string GenerateAuditSetupScript()
    {
        return @"
-- Check if AuditCommon CLR is registered
IF NOT EXISTS (SELECT * FROM sys.assemblies WHERE name = 'AuditCommon')
BEGIN
    PRINT 'ERROR: AuditCommon CLR Assembly not registered!'
    PRINT 'Please register using:'
    PRINT 'CREATE ASSEMBLY [AuditCommon] FROM ''path\to\AuditCommon.dll'' WITH PERMISSION_SET = SAFE'
END
GO
";
    }
}
```

---

## שלב 6: שיפורי UI ו-UX

### משימה 6.1: פרמטר @WithParentText ב-SPs
**עדיפות:** בינונית
**תלויות:** אין
**קבצים לתיקון:** `src/TargCC.Core.Generators/Sql/SqlGenerator.cs`

```csharp
// הוסף פרמטר לכל SP_GetAll ו-SP_GetFiltered
public string GenerateGetAllProcedure(TableInfo table)
{
    var sb = new StringBuilder();
    sb.AppendLine($"CREATE PROCEDURE SP_GetAll{table.Name}");
    sb.AppendLine("    @WithParentText BIT = 1");
    sb.AppendLine("AS");
    sb.AppendLine("BEGIN");

    // בנה SELECT עם או בלי JOINs
    sb.AppendLine("    IF @WithParentText = 1");
    sb.AppendLine("    BEGIN");
    sb.AppendLine(GenerateSelectWithJoins(table));
    sb.AppendLine("    END");
    sb.AppendLine("    ELSE");
    sb.AppendLine("    BEGIN");
    sb.AppendLine(GenerateSimpleSelect(table));
    sb.AppendLine("    END");

    sb.AppendLine("END");
    return sb.ToString();
}
```

---

### משימה 6.2: תמיכה בהעלאת קבצים
**עדיפות:** נמוכה
**תלויות:** אין
**קבצים חדשים:**
- `templates/React/components/FileUpload.tsx`
- `templates/API/FileController.cs`

---

### משימה 6.3: מערכת התראות
**עדיפות:** נמוכה
**תלויות:** משימה 2.1

---

## שלב 7: Reference ל-DLLים קיימים

### משימה 7.1: הוספת Reference ל-AuditCommon.dll
**עדיפות:** גבוהה
**תלויות:** אין

**מיקום:** `_Dependencies/AuditCommon.dll`

```xml
<!-- בקובץ .csproj של הפרויקט המיוצר -->
<ItemGroup>
    <Reference Include="AuditCommon">
        <HintPath>..\..\_Dependencies\AuditCommon.dll</HintPath>
    </Reference>
</ItemGroup>
```

---

### משימה 7.2: הוספת Reference ל-TargetSMS019.dll
**עדיפות:** נמוכה (לפי הצורך)

---

## סיכום סדר עדיפויות

### Phase 1 - קריטי (מאפשר הפעלה בסיסית)
1. משימה 1.1 - סינון ccvwComboList מ-UI
2. משימה 1.2 - סינון c_* tables מ-UI
3. משימה 1.3 - סינון SPs מיותרים

### Phase 2 - גבוה (תשתית)
4. משימה 2.1 - Script טבלאות מערכת
5. משימה 2.2 - SP ל-Audit
6. משימה 7.1 - Reference ל-AuditCommon

### Phase 3 - גבוה (Combo/Dropdown)
7. משימה 3.1 - SmartAutocomplete Component
8. משימה 3.2 - יצירת ccvwComboList Views
9. משימה 3.3 - Lookup API Controller

### Phase 4 - בינוני (Enums)
10. משימה 4.1 - TypeScript Enums
11. משימה 4.2 - C# Enums

### Phase 5 - בינוני (Extended Properties & Audit)
12. משימה 5.1 - שימוש ב-Extended Properties
13. משימה 5.2 - אינטגרציה עם Audit

### Phase 6 - נמוך (שיפורים)
14. משימה 6.1 - @WithParentText
15. משימה 6.2 - העלאת קבצים
16. משימה 6.3 - מערכת התראות

---

## תרשים תלויות

```
Phase 1 (קריטי)
├── 1.1 סינון ccvwComboList
├── 1.2 סינון c_* tables
└── 1.3 סינון SPs

Phase 2 (תשתית)
├── 2.1 טבלאות מערכת ─────┐
├── 2.2 SP Audit ◄────────┘
└── 7.1 Reference AuditCommon

Phase 3 (Combo)
├── 3.1 SmartAutocomplete
├── 3.2 ccvwComboList Views
└── 3.3 Lookup Controller ◄─── 3.2

Phase 4 (Enums)
├── 4.1 TS Enums ◄───────────── 2.1
└── 4.2 C# Enums ◄───────────── 2.1

Phase 5 (Extended)
├── 5.1 Extended Properties
└── 5.2 Audit Integration ◄─── 2.1, 2.2
```

---

## בדיקות מומלצות לאחר כל שלב

### לאחר Phase 1:
- [ ] ייצר פרויקט ובדוק שאין קומפוננטות ל-ccvwComboList
- [ ] בדוק שאין מסכים מלאים לטבלאות c_*
- [ ] ספור את מספר ה-SPs שנוצרו (צפי: ~700 במקום ~1100)

### לאחר Phase 2:
- [ ] בדוק שטבלאות מערכת נוצרות אם לא קיימות
- [ ] בדוק שה-SP c__SystemAuditAdd עובד

### לאחר Phase 3:
- [ ] בדוק SmartAutocomplete עם חיפוש
- [ ] בדוק paging ב-dropdown
- [ ] בדוק caching

### לאחר Phase 4:
- [ ] בדוק ש-Enums מיוצרים מ-c_Enumeration
- [ ] בדוק פונקציות תרגום

---

## נספח: קבצים מרכזיים לעריכה

| קובץ | משימות |
|------|---------|
| `SqlGenerator.cs` | 1.3, 3.2, 6.1 |
| `ReactFormComponentGenerator.cs` | 1.1, 1.2, 5.1 |
| `ReactListComponentGenerator.cs` | 1.1, 1.2 |
| `TableAnalyzer.cs` | 5.1 |

---

*מסמך זה נוצר על בסיס ניתוח מקיף של Legacy CodeCreator והשוואתו ל-V2*
*תאריך: 26/12/2024*
