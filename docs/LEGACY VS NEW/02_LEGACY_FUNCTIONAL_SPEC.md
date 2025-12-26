# מסמך אפיון פונקציונלי - CodeCreator (Legacy)

## תאריך: דצמבר 2025
## גרסה: 1.0

---

## 1. סקירת יכולות

### 1.1 מטרת המערכת

CodeCreator נועד להאיץ פיתוח אפליקציות עסקיות על ידי יצירת אוטומטית של:

1. **Database Layer** - Stored Procedures ל-CRUD
2. **Data Access Layer** - DBController classes
3. **Service Layer** - Web Services / Web API
4. **Controller Layer** - WSController
5. **UI Layer** - Windows Forms (Entity ו-Collection)
6. **Infrastructure** - Task Managers, Encryption, Auditing

---

## 2. מודול ניתוח בסיס נתונים (DBAnalyser)

### 2.1 יכולות

| יכולת | תיאור |
|-------|-------|
| **קריאת סכמה** | קריאת כל הטבלאות, עמודות ואינדקסים |
| **זיהוי קשרים** | זיהוי Foreign Keys וסוגי קשרים |
| **תמיכה בהצפנה** | זיהוי Always Encrypted columns |
| **סינון טבלאות מערכת** | הבחנה בין טבלאות משתמש לטבלאות c_* |
| **ComboList Queries** | יצירת שאילתות לרשימות נפתחות |

### 2.2 תהליך ניתוח טבלה

```
Input: Connection String, Table Name

1. קריאת מטא-דאטה מ-SQL Server:
   - sys.tables - שמות טבלאות
   - sys.columns - עמודות וסוגי נתונים
   - sys.indexes - אינדקסים
   - sys.foreign_keys - קשרים

2. המרת סוגי נתונים SQL → .NET:
   - int → Int32
   - varchar → String
   - datetime → DateTime
   - bit → Boolean
   - decimal → Decimal

3. זיהוי תכונות מיוחדות:
   - Identity columns
   - Primary Keys
   - Foreign Keys
   - Nullable columns
   - Default values

4. בניית מודל אובייקטים:
   - clsTable עם Collection של clsColumn
   - clsRelationship לכל FK

Output: clsDatabase object עם כל המידע
```

### 2.3 טבלאות מערכת מזוהות

| Prefix | משמעות |
|--------|---------|
| **c_** | טבלאות Controller (מערכת) |
| **mn_** | טבלאות Manual (נוצרו ידנית) |
| **zz** | טבלאות זמניות/גיבוי |

### 2.4 סוגי קשרים נתמכים

- **OneToOne** - קשר 1:1
- **OneToMany** - קשר 1:N (הנפוץ ביותר)
- **ManyToMany** - דרך טבלת קישור

---

## 3. מודול ניתוח קוד (CodeAnalyser)

### 3.1 יכולות ניתוח

| מודול | מה מנתח |
|-------|---------|
| **clsStoredProcedures** | SP קיימים, פרמטרים, לוגיקה |
| **clsController** | DBController classes |
| **clsWebService** | ASMX Web Services |
| **clsWebAPI** | Web API Controllers |
| **clsWinF** | Windows Forms קיימים |
| **clsConfig** | הגדרות קונפיגורציה |

### 3.2 הגדרות קונפיגורציה (clsConfig)

#### מודלי אימות (Authentication Models)

```vb
Public Enum enmAuthenticatonModel
  UD                        ' לא מוגדר
  SpecificUserCredentials   ' משתמש וסיסמה ספציפיים
  ActiveUserCredentials     ' Windows Authentication
  ApplicationCredentials    ' אפליקציה עם סיסמה
  None                      ' ללא אימות
End Enum
```

#### מודלי זיהוי משתמש

```vb
Public Enum enmUserIdentificationModel
  UD                ' לא מוגדר
  ByApplicationUser ' משתמש אפליקציה
  ByDomainUser      ' משתמש Domain
  ByDomainGroup     ' קבוצת Domain
End Enum
```

#### מודלי עדכון Grid

```vb
Public Enum enmGridUpdateMethod
  UD      ' לא מוגדר
  ByRow   ' עדכון שורה-שורה
  ByGrid  ' עדכון כל ה-Grid בבת אחת
End Enum
```

#### מודלי תצוגת טפסים

```vb
Public Enum enmFormsModel
  UD          ' לא מוגדר
  MenuTree    ' תפריט עץ
  MenuRibbon  ' Ribbon כמו Office
End Enum
```

### 3.3 פרמטרים לקוד שנוצר

| פרמטר | תיאור | ברירת מחדל |
|-------|-------|-------------|
| **DoSprocs** | יצירת Stored Procedures | True |
| **DoDBController** | יצירת DB Controller | True |
| **DoWS** | יצירת Web Service | True |
| **DoWSController** | יצירת WS Controller | True |
| **DoWinF** | יצירת Windows Forms | True |
| **ExposeRowChangers** | חשיפת שדות AddedBy, ChangedBy | True |
| **IncludeSMSModule** | כלול מודול SMS | False |
| **DefaultActionsDoubleClickTableCreatesPopup** | Double-click פותח Popup | False |
| **UsePrettySystemEntityDesign** | עיצוב מיוחד לטבלאות מערכת | False |

### 3.4 הגדרות צבעים

```vb
' צבעי ממשק משתמש
ColourFormBackground              = "SeaShell"
ColourMenuBackground              = "White"
ColourObjectBackground            = "Wheat"
ColourObjectReadOnlyTextBackground = "PapayaWhip"
ColourObjectLinkTextForeground    = "Brown"
ColourGridBackground              = "Snow"
ColourGridHeader                  = "Maroon"
ColourDefaultGridCell             = "Beige"
ColourAlternatingGridCell         = "White"
```

---

## 4. מודול יצירת קוד (CodeWriter)

### 4.1 שלבי יצירת קוד (Phases)

```vb
Public Enum enmPhase
  StoredProcedure   ' יצירת SP
  DBController      ' יצירת Data Access
  WS                ' יצירת Web Service
  WebAPI            ' יצירת Web API
  WSController      ' יצירת WS Controller
  WinF              ' יצירת Windows Forms
  Intitialize       ' אתחול
  TaskManager       ' יצירת Task Manager
End Enum
```

### 4.2 Stored Procedures שנוצרים

לכל טבלה נוצרים ה-SP הבאים:

| SP | תיאור | דוגמה |
|----|-------|-------|
| **{Table}_Select** | בחירת רשומה בודדת | Customer_Select @ID |
| **{Table}_SelectAll** | בחירת כל הרשומות | Customer_SelectAll |
| **{Table}_SelectByFK** | בחירה לפי FK | Customer_SelectByCountryID |
| **{Table}_Insert** | הכנסת רשומה | Customer_Insert @Name, @Email |
| **{Table}_Update** | עדכון רשומה | Customer_Update @ID, @Name |
| **{Table}_Delete** | מחיקת רשומה | Customer_Delete @ID |
| **{Table}_SoftDelete** | מחיקה רכה | Customer_SoftDelete @ID |
| **ccvwComboList_{Table}** | View ל-Dropdown | ccvwComboList_Customer |

### 4.3 DBController Class

לכל טבלה נוצר DBController עם:

```csharp
public class CustomerDBController
{
    // CRUD Operations
    public Customer Select(int id)
    public List<Customer> SelectAll()
    public int Insert(Customer entity)
    public bool Update(Customer entity)
    public bool Delete(int id)

    // FK-based selections
    public List<Customer> SelectByCountryID(int countryId)

    // Combo List
    public DataTable GetComboList()

    // Audit support
    private void LogAudit(string action, int id)
}
```

### 4.4 Web Service / Web API

**Web Service (ASMX):**
```csharp
[WebService]
public class CustomerService : System.Web.Services.WebService
{
    [WebMethod]
    public Customer Select(int id)

    [WebMethod]
    public List<Customer> SelectAll()

    [WebMethod]
    public int Insert(Customer entity)

    [WebMethod]
    public bool Update(Customer entity)

    [WebMethod]
    public bool Delete(int id)
}
```

**Web API:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    [HttpGet("{id}")]
    public Customer Get(int id)

    [HttpGet]
    public List<Customer> GetAll()

    [HttpPost]
    public int Post([FromBody] Customer entity)

    [HttpPut]
    public bool Put([FromBody] Customer entity)

    [HttpDelete("{id}")]
    public bool Delete(int id)
}
```

### 4.5 Windows Forms

#### Entity Form (ctlXXX.vb)

טופס לעריכת רשומה בודדת:

```vb
Public Class ctlCustomer
    ' Properties
    Private _ID As Integer
    Private _Entity As Customer
    Private _Mode As FormMode  ' Add, Edit, View

    ' Events
    Public Event EntitySaved()
    Public Event EntityDeleted()

    ' Controls (נוצרים אוטומטית)
    ' - TextBox לכל שדה String
    ' - NumericUpDown לכל שדה מספרי
    ' - ComboBox לכל FK
    ' - DateTimePicker לכל שדה תאריך
    ' - CheckBox לכל שדה Boolean

    ' Methods
    Public Sub LoadEntity(id As Integer)
    Public Sub SaveEntity()
    Public Sub DeleteEntity()
End Class
```

#### Collection Form (ctlcXXX.vb)

טופס להצגת רשימת רשומות:

```vb
Public Class ctlcCustomer
    ' Properties
    Private _DataSource As List(Of Customer)
    Private _SelectedID As Integer

    ' Controls
    ' - DataGridView עם כל העמודות
    ' - כפתורי Add, Edit, Delete
    ' - Search/Filter controls

    ' Events
    Public Event ItemSelected(id As Integer)
    Public Event ItemDoubleClicked(id As Integer)

    ' Methods
    Public Sub RefreshData()
    Public Sub ApplyFilter(filter As String)
End Class
```

### 4.6 סכום שורות קוד לכל טבלה

| רכיב | שורות קוד (בערך) |
|------|------------------|
| Stored Procedures | ~300 |
| DBController | ~400 |
| Web Service | ~200 |
| WS Controller | ~150 |
| Entity Form | ~800 |
| Collection Form | ~400 |
| **סה"כ** | **~2,250** |

---

## 5. מודול Coordinator

### 5.1 תפקיד

clsCoordinator הוא המנהל המרכזי שמתאם את כל תהליך יצירת הקוד.

### 5.2 תהליך עבודה

```
1. Initialize
   ├── קריאת Config מקובץ _TargCC.def
   ├── יצירת Connection String
   └── הכנת מערכת הקבצים

2. PrepareDatabase
   ├── עדכון טבלאות מערכת
   ├── הוספת Enums חסרים
   └── עדכון גרסה

3. לכל טבלה שנבחרה:
   │
   ├── Phase: StoredProcedure
   │   └── יצירת כל ה-SP לטבלה
   │
   ├── Phase: DBController
   │   └── יצירת class ב-C#
   │
   ├── Phase: WS
   │   └── יצירת Web Service method
   │
   ├── Phase: WebAPI
   │   └── יצירת API endpoint
   │
   ├── Phase: WSController
   │   └── יצירת controller class
   │
   └── Phase: WinF
       ├── יצירת Entity Form
       └── יצירת Collection Form

4. Phase: TaskManager
   └── יצירת scheduled tasks

5. Finalize
   ├── עדכון version ב-DB
   └── כתיבת log
```

### 5.3 אירועים

```vb
Public Event evtStatusNMessage(ByVal ePhase As enmPhase, ByVal eMassage As String)
```

משמש לעדכון UI על ההתקדמות.

---

## 6. אפליקציית TargCC (GUI ראשי)

### 6.1 מסכים

#### מסך ראשי

| אזור | תוכן |
|------|------|
| **חיבור לDB** | Server, Database, User, Password |
| **אפשרויות יצירה** | Sprocs, DBController, WS, WinF |
| **בחירת טבלאות** | רשימת טבלאות עם checkboxes |
| **הגדרות** | Authentication, Colors, etc. |
| **תוצאות** | Log של הפעולות |

#### Tab: Database Settings

- Server Name
- Database Name
- Integrated Security או User/Password
- Connection Test button

#### Tab: Code Generation Options

- Do Stored Procedures
- Do DB Controller
- Do Web Service
- Do WS Controller
- Do Windows Forms
- Custom WinForm Projects

#### Tab: Security Settings

- Authentication Model
- User Identification Model
- Application Credentials
- Decipher Key

#### Tab: UI Colors

- Form Background
- Menu Background
- Object Background
- Grid Colors (Header, Rows, Alternating)

#### Tab: Table Settings

- DefaultTextFields - שדות להצגה בטקסט
- UsedForIdentity - האם משמש לזיהוי משתמשים
- IsSingleRow - טבלה עם שורה אחת
- CanAdd / CanEdit / CanDelete
- Audit settings
- CreateUIMenu / CreateUICollection / CreateUIEntity

### 6.2 פעולות עיקריות

| כפתור | פעולה |
|--------|--------|
| **Connect** | התחברות ל-DB וטעינת טבלאות |
| **Update Solution** | הרצת Code Generation |
| **Update Settings** | שמירת הגדרות לקובץ |
| **Change Solution** | החלפת Solution פעיל |

---

## 7. אפליקציית SolutionManager

### 7.1 יכולות

| יכולת | תיאור |
|-------|-------|
| **ניהול Companies** | הגדרת חברות עם certificates |
| **ניהול Projects** | הגדרת פרויקטים לכל חברה |
| **ClickOnce** | יצירת deployment packages |
| **Obfuscation** | הצפנת קוד עם ConfuserEx |
| **Versioning** | ניהול גרסאות אוטומטי |
| **Signing** | חתימה דיגיטלית עם PFX/USB |

### 7.2 מבנה היררכי

```
Solution
└── Company (Development/Production/Other)
    ├── Certificate Settings (PFX or USB)
    ├── Publish Root Folder
    └── Projects
        ├── Assembly Name
        ├── Version
        ├── Icon
        ├── Publish Location
        ├── Configuration Settings
        └── Obfuscation Settings
```

### 7.3 תהליך פרסום (Publishing)

```
1. Copy to Publish Root Folder
2. Recompile Solution
3. Obfuscate (אם מוגדר)
4. Create ClickOnce Installation
5. Create CD Installation
6. Sign with Certificate
```

---

## 8. כלי עזר

### 8.1 TargControllerCreator

כלי להצפנה ופענוח:

| פונקציה | תיאור |
|---------|-------|
| **AES Encrypt** | הצפנת טקסט ב-AES |
| **AES Decrypt** | פענוח טקסט AES |
| **3DES Encrypt** | הצפנת טקסט ב-Triple DES |
| **3DES Decrypt** | פענוח טקסט 3DES |
| **Password Hash** | יצירת Hash לסיסמה |

### 8.2 ControllerPinger

ניטור בריאות אפליקציות:

| פונקציה | תיאור |
|---------|-------|
| **Application Monitoring** | בדיקת זמינות אפליקציות |
| **Email Alerts** | שליחת התראות בעת כשל |
| **Logging** | רישום אירועים |

### 8.3 TargCCDesignerTools

כלי עיצוב לטפסים:

- עיצוב Controls
- Theme Editor
- Layout Tools

---

## 9. תמיכה ב-Audit

### 9.1 טבלאות Audit

| טבלה | תוכן |
|------|------|
| **c_LoggedLogin** | התחברויות משתמשים |
| **c_LoggedAlert** | שגיאות והודעות מערכת |
| **c_Job** | עבודות מתוזמנות |

### 9.2 שדות Audit בטבלאות

כל טבלה יכולה לכלול:

```sql
AddedBy      NVARCHAR(50)   -- מי הוסיף
AddedOn      DATETIME       -- מתי הוסיף
ChangedBy    NVARCHAR(50)   -- מי שינה
ChangedOn    DATETIME       -- מתי שינה
DeletedBy    NVARCHAR(50)   -- מי מחק
DeletedOn    DATETIME       -- מתי מחק
UpdatingLoginID BIGINT      -- ID של המשתמש המעדכן
```

### 9.3 Triggers

AuditCommon.dll מספק triggers אוטומטיים לרישום שינויים.

---

## 10. תמיכה רב-לשונית

### 10.1 טבלאות שפה

| טבלה | תוכן |
|------|------|
| **c_Language** | הגדרות שפות (en, he, etc.) |
| **c_ObjectToTranslate** | אובייקטים לתרגום |
| **c_ObjectTranslation** | תרגומים בפועל |
| **c_Enumeration** | ערכי Enum מתורגמים |

### 10.2 פורמט שדות רב-לשוניים

```sql
Text_L1   NVARCHAR(100)   -- שפה 1 (English)
Text_L2   NVARCHAR(100)   -- שפה 2 (Hebrew)
Text_L3   NVARCHAR(100)   -- שפה 3
Text_L4   NVARCHAR(100)   -- שפה 4
```

### 10.3 c_Lookup - Lookup Tables

טבלת Lookup גנרית עם תמיכה בהיררכיה:

```sql
enmParentLookupType_Lookup  -- סוג ההורה
ParentCode                   -- קוד ההורה
enmLookupType_Lookup        -- סוג ה-Lookup
Code                        -- קוד הערך
locText                     -- טקסט מתורגם
locDescription              -- תיאור מתורגם
```

---

## 11. ניהול משתמשים והרשאות

### 11.1 טבלאות משתמשים

| טבלה | תוכן |
|------|------|
| **c_User** | משתמשים |
| **c_Role** | תפקידים |
| **c_UserRole** | שיוך משתמש-תפקיד |

### 11.2 תפקידים מובנים

```sql
Master          -- גישה מלאה
Administrator   -- מנהל מערכת
UserManager     -- ניהול משתמשים
SysAdmin        -- מנהל טכני
User            -- משתמש רגיל
_App_WinF       -- אפליקציית Windows
```

### 11.3 שדות משתמש

```sql
LastName, FirstName         -- שם
UserName                    -- שם משתמש
enmType_UserIdentityType    -- סוג זהות
spt_enoPassword             -- סיסמה מוצפנת (SHA256)
LastPasswords               -- סיסמאות קודמות
Disabled                    -- מושבת
spt_LoggedInIP              -- IP התחברות אחרון
spt_LastSuccessfulLogin     -- זמן התחברות אחרון
```

### 11.4 MFA - אימות דו-שלבי

טבלת c_MFA:

```sql
CellOrEmail       -- טלפון או email
ProtectedFunction -- הפונקציה המוגנת
enoCode           -- קוד מוצפן
AttemptNo         -- מספר ניסיון
IsSuccessful      -- האם הצליח
WhenExpires       -- תאריך תפוגה
```

---

## 12. סיכום יכולות

### מה CodeCreator עושה:

1. **קורא סכמת DB** - מנתח טבלאות, עמודות, קשרים
2. **מייצר SP** - CRUD operations לכל טבלה
3. **מייצר Data Layer** - DBController classes
4. **מייצר Services** - Web Service או Web API
5. **מייצר UI** - Windows Forms לעריכה וצפייה
6. **מנהל אבטחה** - משתמשים, תפקידים, הצפנה
7. **תומך Audit** - לוגים ומעקב שינויים
8. **תומך רב-לשוניות** - תרגום שדות וממשק

### מה CodeCreator לא עושה:

1. לא מייצר React/Angular/Vue
2. לא מייצר Mobile apps
3. לא תומך ב-NoSQL
4. לא מייצר Unit Tests
5. לא מייצר Documentation אוטומטי
6. לא תומך ב-CI/CD

---

המסמך הבא יתאר כיצד להשתמש במערכת (מדריך הדרכה).
