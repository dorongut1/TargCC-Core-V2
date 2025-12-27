# סיכום שיחה - TargCC V2 Development
## Session Summary for Next Conversation

**תאריך:** 2025-12-27
**Branch:** `feature/legacy-compatibility`
**Last Commit:** (pending) - Fix: DbContext generators now filter tables correctly

---

## מה הושלם בשיחה הזו (2025-12-27)

### Bug Fix 1: DbContext Generators Filter Issue ✅
הבעיה: `IApplicationDbContext.cs` ו-`ApplicationDbContext.cs` כללו DbSet references לטבלאות שלא קיימות כ-Entities (כמו `IndexFragmentationData`, `CcvwComboListActiveRestrictions`).

**שורש הבעיה:**
- DbContext generators (`ApplicationDbContextInterfaceGenerator`, `DbContextGenerator`) השתמשו בכל הטבלאות מה-schema
- Entity generator ושאר הגנרטורים סיננו טבלאות בלי PK, ccvwComboList views, וטבלאות עם GenerateUI=false

**התיקון:**
- עדכון `ApplicationDbContextInterfaceGenerator.cs` - הוספת פילטור לטבלאות
- עדכון `DbContextGenerator.cs` - הוספת פילטור לטבלאות

**קבצים שנערכו:**
- `src/TargCC.Core.Generators/Data/ApplicationDbContextInterfaceGenerator.cs`
- `src/TargCC.Core.Generators/Data/DbContextGenerator.cs`

**תוצאה:** הפרויקט המיוצר (`NewtestV2`) מתקמפל בהצלחה!

### Bug Fix 2: React Client TypeScript Error ✅
הבעיה: TypeScript build נכשל עם שגיאת `TS2345: Argument of type 'GridSortDirection' is not assignable to parameter of type '"desc" | "asc" | null'`

**שורש הבעיה:**
- MUI DataGrid's `model[0].sort` can be `undefined`, but `handleSortChange` expected `'asc' | 'desc' | null`

**התיקון:**
- עדכון `ReactListComponentGenerator.cs` להוסיף nullish coalescing: `model[0].sort ?? 'asc'`

**קובץ שנערך:**
- `src/TargCC.Core.Generators/UI/Components/ReactListComponentGenerator.cs`

**תוצאה:** React client מתקמפל בהצלחה!

### Feature: @WithParentText Now Works! ✅
הפיצ'ר `@WithParentText` עכשיו עובד מלא!

**הבעיה הקודמת:**
- `Column.IsForeignKey` ו-`Column.ReferencedTable` לא היו מאוכלסים
- לכן הSPs לא ייצרו את הלוגיקה של LEFT JOIN

**התיקון:**
- עדכון `DatabaseAnalyzer.cs` - הוספת מתודה `PopulateForeignKeyInfoOnColumns`
- אחרי שהRelationships נטענים, עוברים על כל relationship ומאכלסים את:
  - `Column.IsForeignKey = true`
  - `Column.ReferencedTable = "ParentTableName"`

**תוצאה:**
- 146 FK column references מאוכלסים
- 292 LEFT JOINs ל-ccvwComboList views
- 120 `IF @WithParentText = 1` conditional blocks

### Feature: ccvwComboList Views Generation ✅
נוספה יכולת לייצר ccvwComboList Views אוטומטית!

**מה נעשה:**
- שילוב `ComboListViewGenerator` ב-`ProjectGenerationService`
- הViews נוצרים בתחילת הקובץ `all_procedures.sql`

**תוצאה:**
- 73 ccvwComboList Views נוצרו אוטומטית
- כל View מכיל: ID, Text, TextNS (for search optimization)

---

## סיכום מהשיחה הקודמת (2025-12-26)

### Phase 5: Extended Properties & Audit ✅
- הוספת properties חדשים ל-`Table.cs`: `CreateMenu`, `CreateEntity`, `CreateCollection`, `AuditLevel`
- עדכון `TableAnalyzer` לטעון Extended Properties מהדאטאבייס
- יצירת `AuditTriggerGenerator.cs` לtriggers עם AuditCommon.dll CLR

### Phase 6: @WithParentText ✅
- עדכון `SpGetAllTemplate.cs` ו-`SpGetFilteredTemplate.cs`
- פרמטר `@WithParentText BIT = 1`
- כאשר = 1, ה-SP עושה LEFT JOIN ל-`ccvwComboList_{ParentTable}`
- מחזיר `{FKColumn}_Text` עם טקסט ההורה

### Bug Fixes ✅
1. **ccvwComboList Views לא צריכות SPs**
   - קובץ: `TableAnalyzer.cs`
   - תיקון: הבחנה בין `ccvwComboList_*` (אוטומטי) ל-`mnccvwComboList_*` (ידני)

2. **טבלאות בלי PK נכשלות**
   - קובץ: `ProjectGenerationService.cs`
   - תיקון: סינון טבלאות בלי Primary Key

3. **Manual vs Auto ComboList Views**
   - `ccvwComboList_*` = אוטומטי → לא מייצרים UI או SPs
   - `mnccvwComboList_*` = ידני (mn = Manual) → מייצרים UI read-only

---

## קבצים שנערכו

| קובץ | שינויים |
|------|---------|
| `src/TargCC.Core.Interfaces/Models/Table.cs` | הוספת `IsManualComboListView`, `CreateMenu`, `CreateEntity`, `CreateCollection`, `AuditLevel` |
| `src/TargCC.Core.Analyzers/Database/TableAnalyzer.cs` | זיהוי auto vs manual ComboList, טעינת Extended Properties |
| `src/TargCC.CLI/Services/Generation/ProjectGenerationService.cs` | סינון טבלאות בלי PK |
| `src/TargCC.Core.Generators/Sql/Templates/SpGetAllTemplate.cs` | @WithParentText + LEFT JOIN |
| `src/TargCC.Core.Generators/Sql/Templates/SpGetFilteredTemplate.cs` | @WithParentText + LEFT JOIN |
| `src/TargCC.Core.Generators/Sql/AuditTriggerGenerator.cs` | חדש - triggers ל-CLR |

---

## פקודת הרצה לבדיקה

```bash
cd /c/Disk1/TargCC-Core-V2/src/TargCC.CLI

dotnet run -- generate project --database "UpayCard_RiskManagement_CCV2" --connection-string "Server=localhost;Database=UpayCard_RiskManagement_CCV2;Trusted_Connection=True;TrustServerCertificate=True" --output "C:\Disk1\TargCC-Core-V2\NewtestV2" --namespace "UpayCard.RiskManagement" --force
```

---

## מה עדיין צריך לבדוק/לעשות

### בדיקות שהושלמו ✅
1. [x] להריץ generation מחדש ולבדוק שאין SPs ל-ccvwComboList - **עובד! 0 SPs ל-ccvwComboList**
2. [x] לעשות dotnet build על הפרויקט המיוצר - **Build succeeded!**
3. [x] לבדוק npm build על ה-React client - **Build succeeded!**
4. [x] @WithParentText עובד - **292 LEFT JOINs, 120 IF blocks**
5. [x] ccvwComboList Views נוצרים - **73 Views generated**

### בדיקות נדרשות
1. [ ] לבדוק ש-mnccvwComboList מקבלות UI
2. [ ] לבדוק API endpoint אחד ידנית
3. [ ] להריץ את ה-API ולבדוק ב-Swagger

### ממצאים חשובים מהבדיקה

#### @WithParentText - עובד! ✅
- 146 FK column references מאוכלסים
- 292 LEFT JOINs ל-ccvwComboList views
- 120 `IF @WithParentText = 1` conditional blocks

#### ccvwComboList Views - עובד! ✅
- 73 ccvwComboList Views נוצרו אוטומטית
- כל View מכיל: ID, Text, TextNS

### שיפורים אפשריים לעתיד
- [ ] יצירת ccvwComboList Views אוטומטית
- [ ] יצירת System Tables (c_Enumeration, c_SystemAudit)
- [ ] יצירת Audit Triggers לטבלאות עם AuditLevel >= 2
- [ ] תמיכה בהעלאת קבצים
- [ ] מערכת התראות

---

## מידע טכני חשוב

### לוגיקת ComboList Views

```
שם View                          | סוג      | UI   | SPs
---------------------------------|----------|------|------
ccvwComboList_Customer           | Auto     | ❌   | ❌
mnccvwComboList_CardForCustomer  | Manual   | ✅   | ❌
```

**הסבר:**
- `ccvwComboList_*` = נוצר אוטומטית ע"י הGenerator → רק לdropdowns
- `mnccvwComboList_*` = נכתב ידנית (mn = Manual) → יוצרים UI read-only

### Extended Properties

| Property | ערכים | השפעה |
|----------|-------|-------|
| `ccUICreateMenu` | 0/1 | יצירת כניסה בתפריט |
| `ccUICreateEntity` | 0/1 | יצירת טופס עריכה |
| `ccUICreateCollection` | 0/1 | יצירת grid/רשימה |
| `ccAuditLevel` | 0/1/2 | 0=None, 1=Track, 2=Full Audit |

### @WithParentText SQL Pattern

```sql
CREATE OR ALTER PROCEDURE [dbo].[SP_GetAllOrders]
    @Skip INT = NULL,
    @Take INT = NULL,
    @WithParentText BIT = 1
AS
BEGIN
    IF @WithParentText = 1
    BEGIN
        SELECT
            t.[ID],
            t.[CustomerID],
            p1.[Text] AS [CustomerID_Text]  -- שם הלקוח
        FROM [Orders] t
        LEFT JOIN [ccvwComboList_Customer] p1 ON t.[CustomerID] = p1.[ID]
        ...
    END
    ELSE
    BEGIN
        SELECT * FROM [Orders] ...
    END
END
```

---

## Git History

```
745420d62 - Fix: Skip tables without PK, distinguish auto vs manual ComboList views
c191c4503 - Phase 6: Add @WithParentText parameter to SP templates
f8154fca8 - Phase 5: Extended Properties & Audit support
(earlier commits from phases 1-4)
```

---

## לשיחה הבאה

**נקודת המשך:** הריצו את פקודת הGeneration מחדש ובדקו:
1. האם ccvwComboList לא מקבלות SPs
2. האם @WithParentText עובד בSQL
3. האם הפרויקט המיוצר מתקמפל

**מסמך בדיקות:** `docs/LEGACY VS NEW/09_TESTING_CHECKLIST.md`
