# סיכום שיחה - TargCC V2 Development
## Session Summary for Next Conversation

**תאריך:** 2025-12-26
**Branch:** `feature/legacy-compatibility`
**Last Commit:** `745420d62` - Fix: Skip tables without PK, distinguish auto vs manual ComboList views

---

## מה הושלם בשיחה הזו

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

### בדיקות נדרשות
1. [ ] להריץ generation מחדש ולבדוק שאין SPs ל-ccvwComboList
2. [ ] לבדוק ש-mnccvwComboList מקבלות UI
3. [ ] לבדוק @WithParentText בSQL output
4. [ ] לעשות dotnet build על הפרויקט המיוצר
5. [ ] לבדוק npm build על ה-React client

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
