# מסמך בדיקות - TargCC V2
## Testing Checklist

**תאריך:** 2025-12-26
**פרויקט בדיקה:** UpayCard.RiskManagement
**מיקום:** `C:\Disk1\TargCC-Core-V2\Newtest`

---

## סטטיסטיקות הרצה
- קבצי C#: 2,089
- קבצי TSX (React): 540
- קבצי SQL: 1

---

## 1. בדיקות SQL - `sql/all_procedures.sql`

### 1.1 בעיות ידועות לתיקון

| # | בעיה | סטטוס | הערות |
|---|------|-------|-------|
| 1 | **ccvwComboList Views מקבלות SPs** | ❌ BUG | Views כמו `mnccvwComboList_CardForCompanyCustomer` לא צריכות SP_GetAll/SP_Add/SP_Update/SP_Delete |
| 2 | **חסרים ccvwComboList Views** | ❓ לבדוק | האם נוצרו CREATE VIEW עבור ccvwComboList? |
| 3 | **חסרות System Tables** | ❓ לבדוק | האם נוצרו c_Enumeration, c_SystemAudit וכו'? |

### 1.2 מה לחפש בקובץ SQL

```bash
# בדוק אם יש CREATE VIEW
grep -c "CREATE VIEW" all_procedures.sql

# בדוק אם יש ccvwComboList views
grep "CREATE.*VIEW.*ccvwComboList" all_procedures.sql

# בדוק אם יש System Tables
grep -c "c_Enumeration\|c_SystemAudit\|c_Lookup" all_procedures.sql

# בדוק @WithParentText
grep -c "@WithParentText" all_procedures.sql

# בדוק LEFT JOIN לccvwComboList (חדש!)
grep -c "LEFT JOIN.*ccvwComboList" all_procedures.sql
```

### 1.3 תוצאות צפויות

| בדיקה | צפוי | בפועל |
|-------|------|-------|
| @WithParentText parameter | כל SP_GetAll ו-SP_GetFiltered | ⬜ לבדוק |
| LEFT JOIN to ccvwComboList | בטבלאות עם FK | ⬜ לבדוק |
| NO SPs for ccvwComboList views | 0 | ⬜ לבדוק |
| CREATE VIEW ccvwComboList | לכל טבלה עם Text column | ⬜ לבדוק |

---

## 2. בדיקות Backend C#

### 2.1 מבנה תיקיות

```
Newtest/src/
├── UpayCard.RiskManagement.API/
│   ├── Controllers/
│   └── Program.cs
├── UpayCard.RiskManagement.Application/
│   ├── Features/{Entity}/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── DTOs/
│   └── Common/
├── UpayCard.RiskManagement.Domain/
│   ├── Entities/
│   └── Interfaces/
└── UpayCard.RiskManagement.Infrastructure/
    ├── Data/
    └── Repositories/
```

### 2.2 בדיקות לביצוע

```bash
# נסה לקמפל את הפרויקט
cd C:\Disk1\TargCC-Core-V2\Newtest
dotnet build

# בדוק שגיאות קומפילציה
dotnet build 2>&1 | grep -i error
```

| בדיקה | צפוי | בפועל |
|-------|------|-------|
| Solution מתקמפל | ✓ Build succeeded | ⬜ לבדוק |
| אין שגיאות CS | 0 errors | ⬜ לבדוק |
| Controllers נוצרו | לכל טבלה | ⬜ לבדוק |
| Repositories נוצרו | לכל טבלה | ⬜ לבדוק |

---

## 3. בדיקות Frontend React

### 3.1 מבנה תיקיות

```
Newtest/client/
├── src/
│   ├── components/
│   │   └── {Entity}/
│   │       ├── {Entity}List.tsx
│   │       ├── {Entity}Form.tsx
│   │       └── {Entity}Detail.tsx
│   ├── hooks/
│   │   └── use{Entity}.ts
│   ├── services/
│   │   └── api.ts
│   └── types/
│       └── {entity}.ts
├── package.json
└── vite.config.ts
```

### 3.2 בדיקות לביצוע

```bash
cd C:\Disk1\TargCC-Core-V2\Newtest\client
npm install
npm run build
```

| בדיקה | צפוי | בפועל |
|-------|------|-------|
| npm install | ✓ Success | ⬜ לבדוק |
| npm run build | ✓ Success | ⬜ לבדוק |
| Components נוצרו | List, Form, Detail לכל entity | ⬜ לבדוק |
| Hooks נוצרו | לכל entity | ⬜ לבדוק |
| RTL Support | direction: rtl | ⬜ לבדוק |

---

## 4. בדיקות Extended Properties (Phase 5)

### 4.1 מה לבדוק

| Extended Property | השפעה צפויה | איפה לבדוק |
|-------------------|-------------|-------------|
| ccUICreateMenu=0 | לא נוצר menu item | Navigation/Menu component |
| ccUICreateEntity=0 | לא נוצר Form | Components folder |
| ccUICreateCollection=0 | לא נוצר List/Grid | Components folder |
| ccAuditLevel=2 | נוצר Audit Trigger | SQL file |

### 4.2 בדיקת Audit Triggers

```bash
grep -c "CREATE TRIGGER.*Audit" all_procedures.sql
grep "AuditCommon" all_procedures.sql
```

---

## 5. בדיקות @WithParentText (Phase 6)

### 5.1 דוגמה לSP תקין עם @WithParentText

```sql
CREATE OR ALTER PROCEDURE [dbo].[SP_GetAllOrders]
    @Skip INT = NULL,
    @Take INT = NULL,
    @WithParentText BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF @WithParentText = 1
    BEGIN
        SELECT
            t.[ID],
            t.[CustomerID],
            t.[Status],
            p1.[Text] AS [CustomerID_Text]  -- שם הלקוח
        FROM [Orders] t
        LEFT JOIN [ccvwComboList_Customer] p1 ON t.[CustomerID] = p1.[ID]
        ORDER BY t.[ID]
        OFFSET ISNULL(@Skip, 0) ROWS
        FETCH NEXT ISNULL(@Take, 2147483647) ROWS ONLY;
    END
    ELSE
    BEGIN
        SELECT * FROM [Orders]
        ORDER BY [ID]
        OFFSET ISNULL(@Skip, 0) ROWS
        FETCH NEXT ISNULL(@Take, 2147483647) ROWS ONLY;
    END
END
```

### 5.2 בדיקות

```bash
# מצא SP עם FK columns
grep -A 30 "SP_GetAllActiveRestrictionss" all_procedures.sql

# בדוק שיש LEFT JOIN
grep -B 5 -A 10 "LEFT JOIN.*ccvwComboList" all_procedures.sql | head -50
```

---

## 6. באגים ידועים לתיקון

### 6.1 קריטי

| # | באג | קובץ | תיאור |
|---|-----|------|-------|
| 1 | ccvwComboList Views מקבלות SPs | SqlGenerator.cs | Views לא צריכות CRUD SPs |
| 2 | טבלאות בלי PK נכשלות | ProjectGenerationService.cs | צריך לדלג על טבלאות בלי PK |

### 6.2 לשיפור

| # | שיפור | קובץ | תיאור |
|---|-------|------|-------|
| 1 | יצירת ccvwComboList Views | ComboListViewGenerator.cs | לייצר את ה-Views אוטומטית |
| 2 | יצירת System Tables | SystemTablesGenerator.cs | לייצר c_Enumeration וכו' |
| 3 | Audit Triggers | AuditTriggerGenerator.cs | לייצר triggers לטבלאות עם AuditLevel >= 2 |

---

## 7. פקודות בדיקה מהירות

```bash
# 1. בדוק build של Backend
cd C:\Disk1\TargCC-Core-V2\Newtest
dotnet build

# 2. בדוק build של Frontend
cd C:\Disk1\TargCC-Core-V2\Newtest\client
npm install && npm run build

# 3. חפש בעיות בSQL
cd C:\Disk1\TargCC-Core-V2\Newtest\sql
grep -c "ccvwComboList" all_procedures.sql     # צפוי: רק בLEFT JOIN, לא בשם SP
grep -c "@WithParentText" all_procedures.sql   # צפוי: מספר גבוה
grep -c "CREATE VIEW" all_procedures.sql       # צפוי: > 0

# 4. הרץ את ה-API
cd C:\Disk1\TargCC-Core-V2\Newtest\src\UpayCard.RiskManagement.API
dotnet run
```

---

## 8. סיכום

### מה עובד ✅
- יצירת Solution structure
- יצירת Entities
- יצירת Repositories
- יצירת Controllers
- יצירת CQRS (Commands, Queries, Handlers)
- יצירת React Components
- יצירת SQL Stored Procedures
- פרמטר @WithParentText נוסף

### מה צריך תיקון ❌
- [ ] לדלג על ccvwComboList Views (לא לייצר להם SPs)
- [ ] לדלג על טבלאות בלי Primary Key
- [ ] לייצר ccvwComboList Views חדשים
- [ ] לייצר System Tables (c_*)
- [ ] לייצר Audit Triggers

### שלב הבא
1. תקן את הבאגים הקריטיים
2. הרץ שוב את הGeneration
3. בדוק שהבuild עובר
4. בדוק API endpoint אחד ידנית
