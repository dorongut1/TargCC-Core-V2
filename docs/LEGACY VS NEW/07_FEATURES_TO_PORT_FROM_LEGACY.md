# פיצ'רים מ-Legacy שצריך להעביר ל-V2

## סקירה כללית

מסמך זה מפרט את הפיצ'רים והיכולות מ-CodeCreator Legacy שעובדים טוב ויש להוסיף ל-TargCC-Core-V2.

**עקרון מנחה:** לא לשכתב מחדש - ללמוד מהישן ולהטמיע את הרעיונות הטובים בארכיטקטורה החדשה.

---

## 1. ComboList Views - יצירה אוטומטית

### מה זה ולמה צריך?

ב-Legacy, לכל טבלה נוצר אוטומטית View בשם `ccvwComboList_{TableName}` שמחזיר:
- `ID` - מפתח ראשי
- `TextNS` - טקסט ממוין (עם רווחים מובילים למיון מספרים)
- `Text` - טקסט תצוגה

**למה זה חשוב:**
1. **ביצועים** - ה-View הוא Indexed View (SCHEMABINDING) עם קלאסטר אינדקס
2. **אחידות** - כל dropdown/combo בכל מקום משתמש באותו מקור נתונים
3. **ParentText** - בפרוצדורות של Get/Fill, משתמשים ב-JOIN לקבל את טקסט ה-Parent

### דוגמא מ-Legacy

```sql
-- יצירת View אוטומטי
CREATE VIEW [dbo].[ccvwComboList_Customer] WITH SCHEMABINDING
AS
    SELECT
      [Customer].[ID] AS [ID],
      ' ' + CAST(REPLACE([Customer].[FirstName] + ' ' + [Customer].[LastName], ' ','') AS nvarchar(200)) + ' ' AS [TextNS],
      [Customer].[FirstName] + ' ' + [Customer].[LastName] AS [Text]
    FROM [dbo].[Customer]

-- יצירת אינדקס לביצועים
CREATE UNIQUE CLUSTERED INDEX [PK_ccvwComboList_Customer] ON [dbo].[ccvwComboList_Customer] ([ID] ASC)
CREATE NONCLUSTERED INDEX [IXccvwComboList_Customer_TextNS] ON [dbo].[ccvwComboList_Customer] ([TextNS] ASC)
```

### שימוש בפרוצדורות

```sql
-- ב-SP_GetCustomer מקבלים גם את שם המפיץ (Foreign Key)
SELECT
    [Customer].*,
    ccvwComboList_Distributor.[Text] AS DistributorText
FROM [Customer]
    LEFT OUTER JOIN ccvwComboList_Distributor
    ON ccvwComboList_Distributor.ID = [Customer].DistributorID
```

### מה חסר ב-V2

1. **אין יצירת Views** - V2 לא מייצר ccvwComboList Views
2. **TODO בקוד** - בקובץ `ReactFormComponentGenerator.cs` שורה 183:
   ```tsx
   {/* TODO: Load options from lookup table */}
   ```
3. **אין ParentText** - הפרוצדורות לא עושות JOIN לקבל טקסט של Foreign Key

### המלצה להטמעה ב-V2

**אופציה 1: ייצור Views (כמו Legacy)**
```csharp
// להוסיף ל-SqlGenerator.cs
public async Task<string> GenerateComboListViewAsync(Table table)
{
    var textColumn = table.DefaultTextColumn ?? table.Columns
        .FirstOrDefault(c => c.Name.Contains("Name") || c.Name.Contains("Title"));

    return $@"
CREATE VIEW [dbo].[ccvwComboList_{table.Name}] WITH SCHEMABINDING
AS
    SELECT
      [{table.Name}].[{table.PrimaryKey.Name}] AS [ID],
      ' ' + CAST({textColumn.Name} AS nvarchar(200)) + ' ' AS [TextNS],
      {textColumn.Name} AS [Text]
    FROM [dbo].[{table.Name}]";
}
```

**אופציה 2: שימוש ב-EF Core Navigation (מודרני יותר)**
```csharp
// ב-Entity
public class Customer
{
    public int DistributorId { get; set; }
    public virtual Distributor? Distributor { get; set; } // Navigation property
}

// ב-Query
var customers = await _context.Customers
    .Include(c => c.Distributor)  // AutoInclude
    .ToListAsync();

// ב-DTO
public class CustomerDto
{
    public int DistributorId { get; set; }
    public string DistributorText { get; set; } // Auto-mapped
}
```

**אופציה 3: Lookup API Endpoint (מומלץ ל-React)**
```csharp
[HttpGet("lookup")]
public async Task<ActionResult<List<LookupItem>>> GetLookup()
{
    return await _context.Customers
        .Select(c => new LookupItem
        {
            Id = c.Id,
            Text = c.FirstName + " " + c.LastName
        })
        .OrderBy(l => l.Text)
        .ToListAsync();
}
```

---

## 2. Extended Properties - שליטה על יצירת UI

### מה זה?

ב-Legacy, משתמשים ב-Extended Properties על טבלאות ב-SQL Server כדי לקבוע מה לייצר:

| Property | משמעות | ברירת מחדל |
|----------|--------|------------|
| `ccUICreateMenu` | ליצור פריט תפריט | 1 (כן) |
| `ccUICreateEntity` | ליצור טופס עריכה | 1 (כן) |
| `ccUICreateCollection` | ליצור Grid/רשימה | 1 (כן) |
| `ccUISkip` | לדלג על הטבלה לגמרי | 0 (לא) |

### מה חסר ב-V2

V2 **טוען** את ה-Extended Properties אבל **לא משתמש** בהם!

**קובץ:** `TableAnalyzer.cs`
```csharp
// V2 טוען את זה אבל לא עושה איתו כלום
table.ExtendedProperties = await GetExtendedPropertiesAsync(table.Name);
```

### המלצה להטמעה

```csharp
// ב-TableAnalyzer.cs - להוסיף
private void ApplyExtendedPropertiesFlags(Table table)
{
    var props = table.ExtendedProperties;

    table.GenerateMenu = GetBoolProp(props, "ccUICreateMenu", true);
    table.GenerateEntity = GetBoolProp(props, "ccUICreateEntity", true);
    table.GenerateCollection = GetBoolProp(props, "ccUICreateCollection", true);

    if (GetBoolProp(props, "ccUISkip", false))
    {
        table.GenerateMenu = false;
        table.GenerateEntity = false;
        table.GenerateCollection = false;
    }
}

// ב-Generators - לבדוק לפני יצירה
if (!table.GenerateEntity)
{
    // לא לייצר Form component
    return;
}
```

---

## 3. סינון טבלאות מערכת (c_*)

### מה עובד ב-Legacy

טבלאות עם קידומת `c_` מסומנות כטבלאות מערכת ומקבלות טיפול שונה:
- לא נוצר להן UI מלא
- משמשות בעיקר כ-lookups/configuration
- חלקן read-only

### מה חסר ב-V2

V2 רק מסיר את הקידומת `c_` מהשם אבל:
- לא מסמן `IsSystemTable`
- לא מסנן יצירת UI
- מייצר CRUD מלא לכולן

### המלצה

```csharp
// ב-TableAnalyzer
if (tableName.StartsWith("c_", StringComparison.OrdinalIgnoreCase))
{
    table.IsSystemTable = true;
    table.GenerateFullUI = false;

    // רשימת טבלאות מערכת שכן צריכות UI
    var tablesWithUI = new[] { "c_User", "c_Role", "c_Lookup", "c_LookupCategory" };
    if (tablesWithUI.Contains(tableName, StringComparer.OrdinalIgnoreCase))
    {
        table.GenerateFullUI = true;
    }
}
```

---

## 4. סינון ccvwComboList Views

### מה עובד ב-Legacy

Views שמתחילים ב-`ccvwComboList_` מקבלים טיפול מיוחד:
- לא נוצר להם UI
- לא נוצרות פרוצדורות Add/Update/Delete
- משמשים רק כ-data source ל-combos

### מה חסר ב-V2

V2 מייצר UI מלא וגם פרוצדורות GetAll/GetByID ל-ccvwComboList Views.

### המלצה

```csharp
// ב-CanGenerate
if (table.Name.StartsWith("ccvwComboList_", StringComparison.OrdinalIgnoreCase))
{
    return false; // לא לייצר כלום
}
```

---

## 5. @WithParentText Parameter

### מה זה?

ב-Legacy, פרוצדורות Get/Fill מקבלות פרמטר `@WithParentText` שקובע האם לעשות JOIN ולהחזיר גם את הטקסט של Foreign Keys.

```sql
CREATE PROCEDURE [dbo].[SP_GetAllCustomers]
    @Skip INT = NULL,
    @Take INT = NULL,
    @WithParentText bit = 1  -- ברירת מחדל: להחזיר טקסט
AS
BEGIN
    IF (@WithParentText = 0)
        SELECT * FROM [Customer]...
    ELSE
        SELECT [Customer].*,
               ccvwComboList_Distributor.[Text] AS DistributorText
        FROM [Customer]
        LEFT OUTER JOIN ccvwComboList_Distributor...
END
```

### למה זה טוב?

- **גמישות** - לפעמים לא צריך את הטקסט (למשל batch processing)
- **ביצועים** - בלי JOIN יש פחות עומס
- **UI-Ready** - כשמציגים ברשימה, הטקסט כבר שם

### מה חסר ב-V2

V2 לא מייצר את ה-JOIN בכלל. כל הפרוצדורות מחזירות רק את ה-ID של Foreign Key.

### המלצה

```csharp
// להוסיף ל-SpGetAllTemplate.cs
if (table.HasForeignKeys)
{
    sb.AppendLine("    ,  @WithParentText bit = 1");

    // ואז בגוף:
    sb.AppendLine("    IF (@WithParentText = 1)");
    sb.AppendLine("    BEGIN");
    // SELECT עם JOINs
    sb.AppendLine("    END");
    sb.AppendLine("    ELSE");
    sb.AppendLine("    BEGIN");
    // SELECT פשוט
    sb.AppendLine("    END");
}
```

---

## 6. Audit Fields אוטומטיים

### מה עובד ב-Legacy

Legacy מזהה שדות אוטומטיים ומטפל בהם:
- `AddedOn` - GETDATE() ב-Insert
- `AddedBy` - שם המשתמש ב-Insert
- `ChangedOn` - GETDATE() ב-Update
- `ChangedBy` - שם המשתמש ב-Update
- `DeletedOn`, `DeletedBy` - Soft Delete

### מה יש ב-V2

✅ V2 כבר מטפל בזה - ראינו בפרוצדורות:
```sql
[AddedOn] = GETDATE(),
[AddedBy] = @AddedBy
```

**סטטוס: מיושם ב-V2** ✓

---

## 7. Indexed Views עם NOEXPAND

### מה זה?

ב-Legacy, כאשר קוראים מ-ccvwComboList View, משתמשים ב-`WITH (NOEXPAND)` כדי לקרוא ישירות מהאינדקס:

```sql
SELECT * FROM ccvwComboList_Customer WITH (NOEXPAND)
```

### למה זה חשוב?

- **ביצועים** - קריאה ישירה מהאינדקס
- **עקביות** - תמיד תוצאות עדכניות

### מה חסר ב-V2

V2 לא מייצר Indexed Views ולא משתמש ב-NOEXPAND.

### המלצה

אם מחליטים ליצור ccvwComboList Views ב-V2, להוסיף:
1. `WITH SCHEMABINDING` ליצירת ה-View
2. `CLUSTERED INDEX` על ID
3. `WITH (NOEXPAND)` בשאילתות

---

## 8. Default Text Column

### מה זה?

כל טבלה ב-Legacy יכולה לקבוע איזה עמודה/עמודות מייצגות את "הטקסט" שלה:

```vb
' מקובץ clsTable.vb
Public Property DefaultTextColumns As List(Of clsColumn)
```

זה משמש ליצירת ה-Text ב-ccvwComboList.

### דוגמאות

| טבלה | Default Text |
|------|-------------|
| Customer | FirstName + ' ' + LastName |
| Product | ProductName |
| Order | 'Order #' + CAST(ID as varchar) |

### מה חסר ב-V2

V2 לא מנהל DefaultTextColumn לטבלאות.

### המלצה

```csharp
// להוסיף ל-Table.cs
public Column? DefaultTextColumn { get; set; }
public List<Column>? DefaultTextColumns { get; set; }

// ב-TableAnalyzer - לזהות אוטומטית
public Column? DetectDefaultTextColumn(Table table)
{
    // סדר עדיפות:
    // 1. עמודה בשם "Name"
    // 2. עמודה בשם "Title"
    // 3. עמודה בשם "Description"
    // 4. עמודה ראשונה מסוג String

    return table.Columns.FirstOrDefault(c =>
        c.Name.Equals("Name", StringComparison.OrdinalIgnoreCase)) ??
           table.Columns.FirstOrDefault(c =>
        c.Name.Equals("Title", StringComparison.OrdinalIgnoreCase)) ??
           table.Columns.FirstOrDefault(c =>
        c.Name.Equals("Description", StringComparison.OrdinalIgnoreCase)) ??
           table.Columns.FirstOrDefault(c =>
        c.DataType.Contains("NVARCHAR") || c.DataType.Contains("VARCHAR"));
}
```

---

## 9. ParentID Views

### מה זה?

בנוסף ל-ccvwComboList הבסיסי, Legacy מייצר גם Views מסוננים לפי Parent:

```sql
-- View רגיל
ccvwComboList_Card

-- View מסונן לפי Customer
ccvwComboList_CardForCustomer
-- מחזיר רק כרטיסים של לקוח ספציפי
```

### למה זה טוב?

כאשר יש Master-Detail, רוצים להציג ב-combo רק את הפריטים הרלוונטיים ל-Master הנוכחי.

### מה חסר ב-V2

V2 לא מייצר Views מסוננים.

### המלצה

```csharp
// ליצור Views מסוננים לכל FK שיש לטבלה
foreach (var fk in table.ForeignKeys)
{
    if (fk.PrimaryTable.IsIdentityTable)
    {
        GenerateFilteredComboListView(table, fk);
    }
}
```

---

## 10. סיכום והמלצות

### מה להוסיף ל-V2 (לפי עדיפות)

| עדיפות | פיצ'ר | מורכבות | השפעה |
|--------|-------|---------|--------|
| **קריטי** | סינון ccvwComboList | נמוכה | מפחית ~80 קומפוננטות מיותרות |
| **קריטי** | סינון c_* Tables | נמוכה | מפחית ~25 קומפוננטות מיותרות |
| **גבוהה** | שימוש ב-Extended Properties | בינונית | שליטה על מה נוצר |
| **גבוהה** | Lookup API/Service | בינונית | תמיכה ב-dropdowns |
| **בינונית** | יצירת ccvwComboList Views | גבוהה | אופציונלי - יש חלופות |
| **בינונית** | @WithParentText | בינונית | UI-ready data |
| **נמוכה** | ParentID Views | גבוהה | filtered combos |

### קבצים לעדכון ב-V2

| קובץ | תיקון נדרש |
|------|------------|
| `SqlGenerator.cs` | סינון ccvwComboList, אופציה ליצור Views |
| `TableAnalyzer.cs` | שימוש ב-Extended Properties, זיהוי c_* |
| `ReactFormComponentGenerator.cs` | מימוש lookup loading |
| `SpGetAllTemplate.cs` | הוספת @WithParentText |
| `Table.cs` | הוספת IsSystemTable, IsComboListView, DefaultTextColumn |

---

## 11. Enums אוטומטיים מטבלת c_Enumeration

### מה זה?

Legacy קורא את טבלת `c_Enumeration` ומייצר אוטומטית:
1. **Enum types** ב-VB.NET לכל סוג enum
2. **פונקציות תרגום** (Translate) מ-string ל-enum וחזרה
3. **ולידציה** של ערכי enum

### מבנה הטבלה

```sql
c_Enumeration
├── ID (PK)
├── EnumType (nvarchar) -- "EntityType", "BlockAction", etc.
├── EnumValue (nvarchar) -- "Customer", "Card", "Block", "Warn", etc.
└── Description
```

### מה נוצר אוטומטית

```vb
' clsEnums.vb - נוצר אוטומטית

' Enum של כל סוגי ה-Enums
Public Enum enmEnum
    UD
    EntityType
    BlockAction
    UserStatus
End Enum

' Enum לכל סוג
Public Enum enmEntityType
    UD
    Customer
    Card
    Distributor
End Enum

Public Enum enmBlockAction
    UD
    Block
    Warn
    Monitor
End Enum

' פונקציות תרגום
Public Shared Function TranslateEnmEntityType(ByVal vString As String) As enmEntityType
    Select Case vString.ToLowerInvariant()
        Case "customer"
            Return enmEntityType.Customer
        Case "card"
            Return enmEntityType.Card
        Case Else
            Return enmEntityType.UD
    End Select
End Function
```

### מה חסר ב-V2

V2 לא קורא את c_Enumeration ולא מייצר enums אוטומטית.

### המלצה להטמעה ב-V2

**אופציה 1: יצירת C# Enums**
```csharp
// EnumGenerator.cs
public async Task<string> GenerateEnumsAsync(string connectionString)
{
    var enums = await LoadEnumerationsAsync(connectionString);

    var sb = new StringBuilder();
    sb.AppendLine("namespace MyApp.Domain.Enums;");

    foreach (var enumType in enums.GroupBy(e => e.Type))
    {
        sb.AppendLine($"public enum {enumType.Key}");
        sb.AppendLine("{");
        sb.AppendLine("    Undefined = 0,");
        foreach (var value in enumType)
        {
            sb.AppendLine($"    {value.Value},");
        }
        sb.AppendLine("}");
    }

    return sb.ToString();
}
```

**אופציה 2: TypeScript Enums (לפרונט)**
```typescript
// enums.generated.ts
export enum EntityType {
    Undefined = 'UD',
    Customer = 'Customer',
    Card = 'Card',
    Distributor = 'Distributor',
}

export enum BlockAction {
    Undefined = 'UD',
    Block = 'Block',
    Warn = 'Warn',
    Monitor = 'Monitor',
}
```

---

## 12. IntelliCombo - Smart Autocomplete

### מה זה?

Legacy מייצר קומפוננטת `IntelliCombo` שהיא ComboBox חכמה עם:
- **חיפוש בזמן אמת** - מחפשת בשרת תוך כדי הקלדה
- **Paging** - טעינה הדרגתית של תוצאות
- **Smart/Dumb modes** - מצב חכם (שרת) או טיפש (מקומי)
- **Caching** - שמירת תוצאות קודמות

### איך זה עובד

```vb
' הקומפוננטה שולחת לשרת בקשה עם הטקסט המוקלד
SP_FillComboList @ListType='CustomerDefault', @SearchText='יוסי%', @Top=20

' מחזירה ID + Text
ID    | Text
------|------------------
123   | יוסי כהן
456   | יוסי לוי
```

### מה חסר ב-V2

V2 מייצר `<Select>` בסיסי עם `{/* TODO: Load options from lookup table */}`.

### המלצה להטמעה ב-V2

**React Autocomplete Component:**
```tsx
// components/common/SmartAutocomplete.tsx
import { Autocomplete, TextField, CircularProgress } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { debounce } from 'lodash';

interface SmartAutocompleteProps {
    entityType: string;
    parentId?: number;
    onChange: (id: number | null) => void;
}

export function SmartAutocomplete({ entityType, parentId, onChange }: SmartAutocompleteProps) {
    const [inputValue, setInputValue] = useState('');

    const { data: options, isLoading } = useQuery({
        queryKey: ['lookup', entityType, inputValue, parentId],
        queryFn: () => fetchLookupOptions(entityType, inputValue, parentId),
        enabled: inputValue.length >= 2,
    });

    return (
        <Autocomplete
            options={options ?? []}
            loading={isLoading}
            onInputChange={debounce((_, value) => setInputValue(value), 300)}
            onChange={(_, item) => onChange(item?.id ?? null)}
            getOptionLabel={(option) => option.text}
            renderInput={(params) => (
                <TextField
                    {...params}
                    InputProps={{
                        ...params.InputProps,
                        endAdornment: isLoading ? <CircularProgress size={20} /> : null,
                    }}
                />
            )}
        />
    );
}
```

**API Endpoint:**
```csharp
[HttpGet("lookup/{entityType}")]
public async Task<ActionResult<List<LookupItem>>> GetLookup(
    string entityType,
    [FromQuery] string? search = null,
    [FromQuery] int? parentId = null,
    [FromQuery] int top = 20)
{
    return await _lookupService.GetLookupAsync(entityType, search, parentId, top);
}
```

---

## 13. ComboList Queries - שאילתות מוכנות

### מה זה?

Legacy מגדיר שאילתות lookup מוכנות שניתן לקרוא להן בשם:

```vb
Public Enum enmComboListType
    UD
    CustomerDefault
    CustomerDefaultByDistributor
    CardDefault
    CardDefaultByCustomer
    DistributorDefault
End Enum
```

### שימוש

```vb
' טעינת כל הלקוחות
FillComboList(enmComboListType.CustomerDefault)

' טעינת כרטיסים של לקוח ספציפי
FillComboList(enmComboListType.CardDefaultByCustomer, customerId)
```

### מה חסר ב-V2

V2 לא מייצר את ה-enum של ComboListType.

### המלצה

```typescript
// lookupTypes.generated.ts
export type LookupType =
    | 'CustomerDefault'
    | 'CustomerByDistributor'
    | 'CardDefault'
    | 'CardByCustomer'
    | 'DistributorDefault';

// hooks/useLookup.ts
export function useLookup(type: LookupType, parentId?: number) {
    return useQuery({
        queryKey: ['lookup', type, parentId],
        queryFn: () => api.getLookup(type, parentId),
    });
}
```

---

## 14. Entity Classes עם Properties מיוחדים

### מה Legacy מייצר

לכל Entity נוצרים:
1. **Properties בסיסיים** - לכל עמודה
2. **ParentText Properties** - לכל FK
3. **ChildCollections** - לכל קשר ילדים
4. **Validation** - בדיקות תקינות

```vb
Public Class clsCustomer
    ' Properties בסיסיים
    Public Property ID As Long
    Public Property FirstName As String
    Public Property LastName As String
    Public Property DistributorID As Long

    ' ParentText - שם המפיץ
    Public ReadOnly Property DistributorText As String
        Get
            Return _DistributorText
        End Get
    End Property

    ' Child Collection - כרטיסים של הלקוח
    Public ReadOnly Property Cards As clsCardCol
        Get
            If _Cards Is Nothing Then
                _Cards = New clsCardCol()
                _Cards.FillByCustomerID(Me.ID)
            End If
            Return _Cards
        End Get
    End Property

    ' Enum Properties
    Public Property enmEntityType As enmEntityType
End Class
```

### מה חסר ב-V2

V2 מייצר entities בסיסיים אבל:
- אין ParentText properties
- אין Child collections
- אין Lazy loading

### המלצה

```csharp
// Customer.cs
public class Customer
{
    public long Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public long DistributorId { get; set; }

    // Navigation property - EF Core ידאג לזה
    public virtual Distributor? Distributor { get; set; }

    // DTO יכיל גם:
    // public string DistributorText => Distributor?.Name;

    // Child collection
    public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
}
```

---

## 15. Audit Triggers

### מה זה?

Legacy משתמש ב-`AuditCommon.dll` ליצירת triggers לביקורת:

```sql
CREATE TRIGGER [dbo].[trc_Customer_Audit] ON [dbo].[Customer]
AFTER DELETE, UPDATE NOT FOR REPLICATION
AS EXTERNAL NAME [AuditCommon].[AuditCommon.Triggers].[AuditCommon]
```

### מה V2 צריך

**אופציה 1: Reference ל-DLL הקיים**
```xml
<Reference Include="AuditCommon">
    <HintPath>..\libs\AuditCommon.dll</HintPath>
</Reference>
```

**אופציה 2: EF Core Interceptors**
```csharp
public class AuditInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(...)
    {
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Modified)
            {
                await _auditService.LogChangeAsync(entry);
            }
        }
        return base.SavingChangesAsync(...);
    }
}
```

---

## 16. סיכום כולל - רשימת משימות להשלמה ב-V2

### עדיפות קריטית (לעשות מיד)

| # | פיצ'ר | מורכבות | זמן משוער |
|---|-------|---------|-----------|
| 1 | סינון ccvwComboList Views | נמוכה | 2 שעות |
| 2 | סינון c_* Tables | נמוכה | 2 שעות |
| 3 | שימוש ב-Extended Properties | בינונית | 4 שעות |

### עדיפות גבוהה (שבוע ראשון)

| # | פיצ'ר | מורכבות | זמן משוער |
|---|-------|---------|-----------|
| 4 | Lookup API Endpoint | בינונית | 8 שעות |
| 5 | SmartAutocomplete Component | בינונית | 8 שעות |
| 6 | Enum Generation מ-c_Enumeration | בינונית | 8 שעות |
| 7 | ParentText ב-DTOs | בינונית | 4 שעות |

### עדיפות בינונית (שבוע שני)

| # | פיצ'ר | מורכבות | זמן משוער |
|---|-------|---------|-----------|
| 8 | יצירת ccvwComboList Views | גבוהה | 16 שעות |
| 9 | @WithParentText בפרוצדורות | בינונית | 8 שעות |
| 10 | ComboListType Enum | נמוכה | 4 שעות |

### עדיפות נמוכה (בהמשך)

| # | פיצ'ר | מורכבות | זמן משוער |
|---|-------|---------|-----------|
| 11 | ParentID Views (filtered combos) | גבוהה | 16 שעות |
| 12 | Audit via EF Interceptors | בינונית | 8 שעות |
| 13 | Child Collections Lazy Loading | בינונית | 8 שעות |

---

## 17. קבצים לעדכון ב-V2

### קבצי Generators

| קובץ | תיקון |
|------|-------|
| `SqlGenerator.cs` | סינון Views, אופציה ליצור ccvwComboList |
| `TableAnalyzer.cs` | Extended Properties, c_* detection |
| `ReactFormComponentGenerator.cs` | SmartAutocomplete |
| `DtoGenerator.cs` | ParentText properties |
| `SpGetAllTemplate.cs` | @WithParentText parameter |

### קבצים חדשים ליצור

| קובץ | תפקיד |
|------|-------|
| `EnumGenerator.cs` | יצירת C# enums מ-c_Enumeration |
| `TypeScriptEnumGenerator.cs` | יצירת TS enums |
| `LookupController.cs` | API endpoint ל-lookups |
| `SmartAutocomplete.tsx` | React component |
| `useLookup.ts` | React Query hook |

---

*מסמך זה נוצר כדי לוודא שכל הטוב מ-Legacy עובר ל-V2*
