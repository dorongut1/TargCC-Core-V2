# מדריך הדרכה - CodeCreator (Legacy)

## תאריך: דצמבר 2025
## גרסה: 1.0

---

## 1. התקנה והגדרה ראשונית

### 1.1 דרישות מערכת

| רכיב | דרישה מינימלית |
|------|----------------|
| **מערכת הפעלה** | Windows 10/11 |
| **Framework** | .NET Framework 4.8 |
| **IDE** | Visual Studio 2019/2022 |
| **Database** | SQL Server 2016+ |
| **RAM** | 8GB מומלץ |

### 1.2 התקנת CodeCreator

1. **קבלת הקבצים**
   - השג את תיקיית CodeCreator מ-TFS
   - מיקום: `C:\Disk1\CodeCreator`

2. **פתיחה ב-Visual Studio**
   ```
   פתח: CodeCreator.sln
   ```

3. **Build Solution**
   - לחץ `Ctrl+Shift+B`
   - וודא שאין שגיאות

4. **הרצה ראשונה**
   - הגדר TargCC כ-Startup Project
   - לחץ F5

### 1.3 הגדרת Solution חדש

1. **יצירת מבנה תיקיות**
   ```
   C:\Projects\MySolution\
   ├── MySolution.sln
   ├── DBController\
   │   └── _TargCC.def      (ייווצר אוטומטית)
   ├── WebService\
   ├── WSController\
   └── WinF\
   ```

2. **הרצת TargCC**
   - בחר את תיקיית ה-Solution
   - המערכת תיצור קובץ `_TargCC.def`

---

## 2. שימוש ב-TargCC

### 2.1 התחברות לבסיס נתונים

#### שלב 1: הגדרת שרת

1. פתח את TargCC
2. עבור ל-Tab "Database Settings"
3. הזן:
   - **Server**: שם השרת (למשל: `localhost\SQLEXPRESS`)
   - **Database**: שם בסיס הנתונים

4. בחר שיטת אימות:
   - ☑ **Integrated Security** - Windows Authentication
   - או הזן **User** ו-**Password**

5. לחץ **Test Connection**

#### שלב 2: טעינת טבלאות

לאחר התחברות מוצלחת:
1. לחץ **Connect**
2. רשימת הטבלאות תיטען
3. סמן את הטבלאות לעיבוד

### 2.2 הגדרת אפשרויות Code Generation

#### Tab: Code Generation Options

| אפשרות | תיאור | המלצה |
|--------|-------|--------|
| ☑ **Do Stored Procedures** | יצירת SP | מומלץ |
| ☑ **Do DB Controller** | יצירת Data Access | מומלץ |
| ☑ **Do Web Service** | יצירת ASMX | לפי צורך |
| ☑ **Do WS Controller** | יצירת Controller | אם WS מסומן |
| ☑ **Do Windows Forms** | יצירת UI | לפי צורך |

#### Custom WinForm Projects

אם יש לך פרויקטי WinForms נוספים:
1. לחץ **Add**
2. הזן שם הפרויקט
3. הפרויקט יקבל עותק של הטפסים

### 2.3 הגדרות אבטחה

#### Tab: Security Settings

**Authentication Model:**
- `None` - ללא אימות
- `ApplicationCredentials` - אפליקציה עם סיסמה (מומלץ)
- `ActiveUserCredentials` - Windows Authentication
- `SpecificUserCredentials` - משתמש וסיסמה ספציפיים

**User Identification Model:**
- `ByApplicationUser` - זיהוי לפי משתמש אפליקציה (מומלץ)
- `ByDomainUser` - זיהוי לפי משתמש Domain
- `ByDomainGroup` - זיהוי לפי קבוצת Domain

**Credentials:**
- **Application Username**: שם האפליקציה
- **Application Password**: סיסמה (נוצרת אוטומטית)
- **Decipher Key**: מפתח הצפנה (נוצר אוטומטית)
- **Web Service Password**: סיסמת WS

### 2.4 הגדרות ממשק משתמש

#### Tab: UI Colors

בחר צבעים לממשק שנוצר:

```
Form Background         → SeaShell (רקע טופס)
Menu Background         → White (רקע תפריט)
Object Background       → Wheat (רקע שדות)
ReadOnly Text Background → PapayaWhip (שדות לקריאה)
Link Text Foreground    → Brown (קישורים)
Grid Background         → Snow (רקע Grid)
Grid Header             → Maroon (כותרת Grid)
Default Grid Cell       → Beige (תאי Grid)
Alternating Grid Cell   → White (תאים מתחלפים)
```

### 2.5 הגדרות טבלה

#### Tab: Table Settings

לכל טבלה ניתן להגדיר:

| הגדרה | תיאור | ערכים |
|-------|-------|-------|
| **DefaultTextFields** | שדות להצגה בטקסט | למשל: Name, Title |
| **UsedForIdentity** | האם משמש לזיהוי | True/False |
| **IsSingleRow** | טבלה עם שורה אחת | True/False |
| **CanAdd** | האם ניתן להוסיף | C (Code), S (System), N (No) |
| **CanEdit** | האם ניתן לערוך | C (Code), S (System), N (No) |
| **CanDelete** | האם ניתן למחוק | C (Code), S (System), N (No) |
| **AuditAdd** | Audit להוספה | True/False |
| **AuditEdit** | Audit לעריכה | True/False |
| **AuditDelete** | Audit למחיקה | True/False |
| **TrackRowChangers** | מעקב אחר משנים | True/False |
| **CreateUIMenu** | יצירת תפריט | True/False |
| **CreateUICollection** | יצירת Collection | True/False |
| **CreateUIEntity** | יצירת Entity | True/False |

### 2.6 הרצת Code Generation

1. **וודא הגדרות**
   - בדוק שכל ההגדרות נכונות

2. **בחר טבלאות**
   - סמן את הטבלאות הרצויות
   - או לחץ **Select All**

3. **לחץ Update Solution**
   - המערכת תתחיל ליצור קוד
   - עקוב אחר ההתקדמות ב-Results

4. **בדוק תוצאות**
   - וודא שאין שגיאות
   - בדוק את הקבצים שנוצרו

---

## 3. עבודה עם הקוד שנוצר

### 3.1 מבנה הקבצים

לאחר הרצה מוצלחת:

```
MySolution\
├── DBController\
│   ├── _TargCC.def
│   ├── CustomerDBController.cs
│   ├── OrderDBController.cs
│   └── ...
│
├── WebService\
│   ├── CustomerService.asmx
│   ├── OrderService.asmx
│   └── ...
│
├── WSController\
│   ├── CustomerWSController.cs
│   └── ...
│
└── WinF\
    ├── Entity\
    │   ├── ctlCustomer.vb
    │   ├── ctlOrder.vb
    │   └── ...
    │
    └── Collection\
        ├── ctlcCustomer.vb
        ├── ctlcOrder.vb
        └── ...
```

### 3.2 שימוש ב-DBController

```csharp
// יצירת instance
var customerDB = new CustomerDBController();

// קריאת רשומה
Customer customer = customerDB.Select(123);

// קריאת כל הרשומות
List<Customer> customers = customerDB.SelectAll();

// הוספת רשומה
int newId = customerDB.Insert(new Customer {
    Name = "John Doe",
    Email = "john@example.com"
});

// עדכון רשומה
customer.Name = "Jane Doe";
customerDB.Update(customer);

// מחיקת רשומה
customerDB.Delete(123);
```

### 3.3 שימוש ב-Windows Forms

#### Entity Form (ctlCustomer)

```vb
' טעינת רשומה
ctlCustomer1.LoadEntity(123)

' שמירת רשומה
AddHandler ctlCustomer1.EntitySaved, Sub()
    MessageBox.Show("נשמר בהצלחה!")
End Sub
ctlCustomer1.SaveEntity()

' מחיקת רשומה
ctlCustomer1.DeleteEntity()
```

#### Collection Form (ctlcCustomer)

```vb
' טעינת כל הרשומות
ctlcCustomer1.RefreshData()

' טיפול בבחירת פריט
AddHandler ctlcCustomer1.ItemSelected, Sub(id As Integer)
    ctlCustomer1.LoadEntity(id)
End Sub

' סינון
ctlcCustomer1.ApplyFilter("Name LIKE '%John%'")
```

---

## 4. שימוש ב-SolutionManager

### 4.1 הגדרת Solution

1. **פתח SolutionManager**
2. **הזן פרטי Solution**:
   - Solution Name
   - .sln File Name
   - Version
   - .NET Version
   - Master Company Name

### 4.2 הוספת Company

1. לחץ **Company New**
2. הזן:
   - **Friendly Name**: שם לתצוגה
   - **Proper Name**: שם רשמי
   - **Type**: Development / Production
   - **Copyright**: זכויות יוצרים
   - **Support URL**: כתובת תמיכה

3. הגדר Certificate:

   **עבור PFX:**
   - Certificate Name: שם הקובץ
   - Certificate Password: סיסמה

   **עבור USB Token:**
   - Name (CN): שם בתעודה
   - Cert Hash: Hash של התעודה
   - Crypto Provider: ספק ההצפנה
   - Key Container: מיקום המפתח
   - Timestamp URI: שרת חותמת זמן

### 4.3 הוספת Project

1. בחר Company
2. לחץ **Project New**
3. הזן:
   - **Project Name**: שם הפרויקט
   - **Assembly Name**: שם ה-Assembly
   - **Title**: כותרת
   - **Description**: תיאור
   - **Product**: שם המוצר
   - **Suite**: סדרת מוצרים
   - **Version**: גרסה
   - **Type**: ClassLibrary / Console / Web / WinForms / WPF
   - **Icon**: קובץ אייקון
   - **Publish Location**: מיקום פרסום

### 4.4 פרסום פרויקט

#### שלב 1: Copy to Publish Folder
- לחץ **Copy to Publish Root Folder**
- הקבצים יועתקו לתיקיית הפרסום

#### שלב 2: Recompile
- לחץ **Recompile**
- הפרויקט יעבור Build

#### שלב 3: Obfuscate (אופציונלי)
- אם הפרויקט מוגדר ל-Obfuscation
- לחץ **Obfuscate with ConfuserEx**

#### שלב 4: Create Installations
- לחץ **Create ClickOnce and CD Installations**
- ייווצרו:
  - ClickOnce deployment
  - CD installation
  - חתימה דיגיטלית

---

## 5. כלי עזר

### 5.1 TargControllerCreator - הצפנה

#### הצפנת טקסט

1. פתח TargControllerCreator
2. בחר Tab:
   - **AES** - הצפנה מודרנית (מומלץ)
   - **3DES** - הצפנה Legacy

3. הזן:
   - **Clear Text**: הטקסט להצפנה
   - **Key**: מפתח הצפנה

4. לחץ **Encrypt**
5. העתק את **Encrypted Text**

#### פענוח טקסט

1. הזן **Encrypted Text**
2. הזן את אותו **Key**
3. לחץ **Decrypt**
4. קבל את **Clear Text**

### 5.2 Password Tools

#### יצירת Hash

1. Tab: **One-Way Encryption**
2. הזן סיסמה
3. בחר Algorithm: SHA256 (מומלץ)
4. לחץ **Hash**
5. קבל את ה-Hash

#### בדיקת חוזק סיסמה

1. Tab: **Password Validator**
2. הזן סיסמה
3. הגדר:
   - Minimum Length
   - Require Numbers
   - Require Symbols
4. לחץ **Validate**

---

## 6. עבודה עם טבלאות מערכת

### 6.1 טבלאות משתמשים

#### הוספת משתמש

```sql
INSERT INTO c_User (
    LastName, FirstName, UserName,
    enmType_UserIdentityType, spt_enoPassword,
    AddedBy, UpdatingLoginID
)
VALUES (
    N'ישראלי', N'ישראל', 'israel',
    'Global',
    '5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8', -- SHA256 של 'password'
    'Admin', 0
)
```

#### שיוך משתמש לתפקיד

```sql
INSERT INTO c_UserRole (c_UserID, c_RoleID, AddedBy, UpdatingLoginID)
SELECT u.ID, r.ID, 'Admin', 0
FROM c_User u, c_Role r
WHERE u.UserName = 'israel' AND r.Name = 'User'
```

### 6.2 טבלאות Lookup

#### הוספת ערך Lookup

```sql
INSERT INTO c_Lookup (
    enmParentLookupType_Lookup, ParentCode,
    enmLookupType_Lookup, Code,
    locText, AddedBy
)
VALUES (
    'UD', '',
    'Status', 'Active',
    N'פעיל', 'Admin'
)
```

### 6.3 טבלאות Enumeration

#### הוספת ערך Enum

```sql
INSERT INTO c_Enumeration (
    EnumType, EnumValue, IsSystem,
    Text_L1, Text_L2, AddedBy
)
VALUES (
    'OrderStatus', 'Pending', 0,
    'Pending', N'ממתין', 'Admin'
)
```

---

## 7. פתרון בעיות נפוצות

### 7.1 שגיאות התחברות

| שגיאה | פתרון |
|-------|--------|
| "Cannot connect to server" | בדוק ששרת SQL פעיל |
| "Login failed" | בדוק user/password |
| "Database does not exist" | וודא שם DB נכון |

### 7.2 שגיאות Code Generation

| שגיאה | פתרון |
|-------|--------|
| "File is read-only" | Check out מ-Source Control |
| "Access denied" | הרשאות לתיקייה |
| "Table has no primary key" | הוסף PK לטבלה |

### 7.3 שגיאות Publish

| שגיאה | פתרון |
|-------|--------|
| "Certificate not found" | בדוק נתיב PFX |
| "Invalid password" | בדוק סיסמת Certificate |
| "Signing failed" | בדוק USB Token מחובר |

---

## 8. שיטות עבודה מומלצות

### 8.1 לפני Code Generation

1. ☐ עשה Backup ל-DB
2. ☐ Check out קבצים מ-Source Control
3. ☐ וודא שכל הטבלאות תקינות
4. ☐ בדוק שיש PK לכל טבלה

### 8.2 אחרי Code Generation

1. ☐ בדוק שהקבצים נוצרו
2. ☐ עשה Build ובדוק שגיאות
3. ☐ בדוק SP חדשים ב-SQL
4. ☐ Check in לאחר אימות

### 8.3 לפני Publish

1. ☐ עדכן Version
2. ☐ עשה Full Build
3. ☐ בדוק שאין Warnings
4. ☐ וודא Certificate תקף

---

## 9. קיצורי דרך

### 9.1 TargCC

| קיצור | פעולה |
|-------|--------|
| F5 | התחל Code Generation |
| Ctrl+A | בחר את כל הטבלאות |
| Ctrl+S | שמור הגדרות |

### 9.2 SolutionManager

| קיצור | פעולה |
|-------|--------|
| F5 | Publish כל הפרויקטים |
| Ctrl+B | Build Solution |

---

## 10. נספחים

### 10.1 מיקומי קבצים חשובים

| קובץ | מיקום |
|------|-------|
| **_TargCC.def** | {Solution}\DBController\ |
| **App.config** | {Solution}\{Project}\ |
| **Connection Strings** | Web.config או App.config |

### 10.2 טבלאות מערכת חובה

רשימת הטבלאות שחייבות להיות לפני Code Generation:

- c_User
- c_Role
- c_UserRole
- c_Table
- c_Enumeration
- c_AlertMessage
- c_SystemDefault
- c_Language
- c_Lookup

### 10.3 יצירת Script להקמת DB חדש

1. הרץ TargCC על DB Template
2. השתמש ב-GetSystemData לייצוא
3. הרץ את ה-Script על DB חדש

---

מדריך זה מכסה את השימוש הבסיסי ב-CodeCreator.
לשאלות נוספות פנה לצוות הפיתוח.
