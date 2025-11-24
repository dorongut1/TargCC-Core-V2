# Phase 1.5: MVP Generators - תכנית מפורטת 🎯

**תאריך:** 15/11/2025  
**זמן משוער:** 2 שבועות (10 ימי עבודה)  
**מטרה:** Proof of Concept - רואים קוד נוצר end-to-end!

---

## 🎯 מה זה Phase 1.5?

**Phase 1.5 = גשר בין Analysis ל-Full Generation**

```
Phase 1 (Core Engine) ✅
    ↓
    DatabaseSchema מלא ומדויק
    ↓
Phase 1.5 (MVP Generators) ← כאן אנחנו!
    ↓
    SQL + Entity + File Writer
    ↓
Phase 2 (Full Generation)
    ↓
    8 פרויקטים מלאים
```

### למה Phase 1.5?
- ✅ **Proof of Concept** - רואים משהו עובד מהר
- ✅ **מבין אתגרים** - לומדים מה צריך בשלבים הבאים
- ✅ **מוודא Analyzers** - בודק ש-DatabaseSchema מספיק טוב
- ✅ **Quick Win** - מוטיבציה וביטחון
- ✅ **Feedback Loop** - לומדים מהטעויות מוקדם

### מה לא נעשה ב-Phase 1.5?
- ❌ לא DBController (רק Entity בסיסי)
- ❌ לא WinForms
- ❌ לא Web Service
- ❌ רק ה-Minimum שצריך כדי לראות משהו עובד!

---

## 📅 תכנית עבודה - שבוע אחר שבוע

### 🗓️ שבוע 1: SQL & Entity Generators

#### יום 1: SQL Generator - Setup (3-4 שעות)
```csharp
// מבנה בסיסי
src/TargCC.Core.Generators/
├── ISqlGenerator.cs
├── SqlGenerator.cs
└── Templates/
    ├── SpGetByIdTemplate.cs
    ├── SpUpdateTemplate.cs
    └── SpDeleteTemplate.cs
```

**מה נעשה:**
- [ ] יצירת `ISqlGenerator` interface
- [ ] יצירת `SqlGenerator` class בסיסי
- [ ] מבנה Templates
- [ ] 3 tests ראשוניים

**Output:**
```sql
-- יצירת SP בסיסי
CREATE PROCEDURE dbo.SP_GetCustomerByID
    @CustomerID INT
AS
    SELECT * FROM Customer WHERE ID = @CustomerID
GO
```

---

#### יום 2-3: SQL Generator - Templates (6-8 שעות)

**SpGetByIdTemplate:**
```sql
CREATE PROCEDURE dbo.SP_Get{TableName}ByID
    @{TableName}ID {PkType}
AS
BEGIN
    SELECT {ColumnList}
    FROM {TableName}
    WHERE ID = @{TableName}ID
END
```

**SpUpdateTemplate:**
```sql
CREATE PROCEDURE dbo.SP_Update{TableName}
    @{TableName}ID {PkType},
    {Parameters}
AS
BEGIN
    UPDATE {TableName}
    SET {UpdateList}
    WHERE ID = @{TableName}ID
END
```

**SpDeleteTemplate:**
```sql
CREATE PROCEDURE dbo.SP_Delete{TableName}
    @{TableName}ID {PkType}
AS
BEGIN
    DELETE FROM {TableName}
    WHERE ID = @{TableName}ID
END
```

**מה נעשה:**
- [ ] SpGetByIdTemplate מלא
- [ ] SpUpdateTemplate מלא
- [ ] SpDeleteTemplate מלא
- [ ] Parameter mapping (SQL types)
- [ ] 10+ tests (כל template בנפרד)

**טיפול ב-Prefixes:**
```sql
-- eno_ (hashed) = read-only in Get, not in Update
-- ent_ (encrypted) = special handling
-- clc_, blg_, agg_ = read-only (not in Update)
-- spt_ = separate SP for each
```

**זמן:** 2 ימים (6-8 שעות)

---

#### יום 4: Entity Generator - Setup (3-4 שעות)

```csharp
src/TargCC.Core.Generators/
├── IEntityGenerator.cs
├── EntityGenerator.cs
├── PropertyGenerator.cs
├── TypeMapper.cs
└── PrefixHandler.cs
```

**מה נעשה:**
- [ ] יצירת `IEntityGenerator` interface
- [ ] יצירת `EntityGenerator` class
- [ ] `TypeMapper` - SQL → C# types
- [ ] 5 tests ראשוניים

**Output:**
```csharp
// Entity בסיסי
public class Customer
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

**זמן:** 1 יום (3-4 שעות)

---

#### יום 5: Entity Generator - Properties & Prefixes (4-6 שעות)

**PropertyGenerator:**
```csharp
// Column → Property
public string GenerateProperty(Column column)
{
    var type = TypeMapper.Map(column.SqlType);
    var name = PrefixHandler.GetPropertyName(column);
    
    return $"public {type} {name} {{ get; set; }}";
}
```

**PrefixHandler - 12 Prefixes:**
```csharp
public class PrefixHandler
{
    // eno_ → PasswordHashed (read-only)
    // ent_ → CreditCard (encrypt/decrypt)
    // lkp_ → StatusID + StatusText
    // enm_ → StatusEnum
    // loc_ → NameLocalized
    // clc_ → TotalCalculated (read-only)
    // blg_ → DiscountBL (read-only client)
    // agg_ → OrderCountAgg (read-only client)
    // spt_ → CommentsSeparate
    // FUI_ → ignore (fake unique)
    // upl_ → DocumentUpload
    // scb_ → separate changed by
    // spl_ → list delimited
}
```

**דוגמה - eno_Password:**
```csharp
// Column: eno_Password VARCHAR(64)
// Output:
public string PasswordHashed { get; private set; }
```

**דוגמה - ent_CreditCard:**
```csharp
// Column: ent_CreditCard VARCHAR(MAX)
// Output:
private string _creditCard;
public string CreditCard 
{ 
    get => Decrypt(_creditCard); 
    set => _creditCard = Encrypt(value); 
}
```

**מה נעשה:**
- [ ] PropertyGenerator מלא
- [ ] PrefixHandler - כל 12 הסוגים
- [ ] TypeMapper מורחב
- [ ] 15+ tests (כל prefix בנפרד)

**זמן:** 1 יום (4-6 שעות)

---

### ✅ Checkpoint שבוע 1:
- ✅ SQL Generator יוצר 3 SPs בסיסיים
- ✅ Entity Generator יוצר Class עם Properties
- ✅ מטפל ב-12 Prefixes
- ✅ 25+ tests עוברים
- ✅ Code Quality: Grade A

---

### 🗓️ שבוע 2: File Writer & Integration

#### יום 1: File Writer - Basic (3-4 שעות)

```csharp
src/TargCC.Core.Writers/
├── IFileWriter.cs
├── FileWriter.cs
├── FileProtection.cs
└── BackupManager.cs
```

**FileWriter יכולות:**
```csharp
public class FileWriter : IFileWriter
{
    // כתיבת קובץ חדש
    Task WriteFileAsync(string path, string content);
    
    // עדכון קובץ (str_replace)
    Task UpdateFileAsync(string path, 
        string oldContent, string newContent);
    
    // בדיקה: האם קובץ מוגן?
    bool IsProtectedFile(string path);
}
```

**מה נעשה:**
- [ ] IFileWriter interface
- [ ] FileWriter class
- [ ] WriteFileAsync implementation
- [ ] 5 tests

**זמן:** 1 יום (3-4 שעות)

---

#### יום 2: File Writer - *.prt Protection (3-4 שעות)

**הגנה על *.prt:**
```csharp
public class FileProtection
{
    public bool IsProtected(string filePath)
    {
        // *.prt.vb, *.prt.cs
        return filePath.EndsWith(".prt.vb") 
            || filePath.EndsWith(".prt.cs");
    }
    
    public void PreventOverwrite(string filePath)
    {
        if (IsProtected(filePath))
        {
            _logger.Warning($"⚠️ Protected file: {filePath}");
            throw new ProtectedFileException(
                $"Cannot overwrite {filePath}");
        }
    }
}
```

**BackupManager:**
```csharp
public class BackupManager
{
    // גיבוי לפני כתיבה
    Task<string> BackupFileAsync(string path);
    
    // שחזור מגיבוי
    Task RestoreFromBackupAsync(string backupPath);
}
```

**מה נעשה:**
- [ ] FileProtection class
- [ ] IsProtectedFile logic
- [ ] PreventOverwrite exception
- [ ] BackupManager class
- [ ] 10+ tests (כל מקרה קצה)

**Test Cases:**
```csharp
[Fact]
public async Task WriteFile_ProtectedPrt_ThrowsException()
{
    // Arrange
    var path = "Customer.prt.vb";
    
    // Act & Assert
    await Assert.ThrowsAsync<ProtectedFileException>(
        () => _writer.WriteFileAsync(path, "content"));
}

[Fact]
public async Task WriteFile_NormalFile_Success()
{
    // Arrange
    var path = "Customer.cs";
    
    // Act
    await _writer.WriteFileAsync(path, "content");
    
    // Assert
    Assert.True(File.Exists(path));
}
```

**זמן:** 1 יום (3-4 שעות)

---

#### יום 3-4: Integration Tests - End-to-End (6-8 שעות)

**תרחיש מלא:**
```
1. ניתוח DB (TargCCOrders)
   ↓
2. יצירת SQL SPs
   ↓
3. יצירת Entity classes
   ↓
4. כתיבה לדיסק
   ↓
5. Build & Verify
```

**Test Database:**
```sql
-- TestDatabase.sql
CREATE TABLE Customer
(
    ID INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    eno_Password VARCHAR(64),
    ent_CreditCard VARCHAR(MAX),
    lkp_Status VARCHAR(10),
    AddedOn DATETIME DEFAULT GETDATE()
);

CREATE TABLE [Order]
(
    ID INT PRIMARY KEY IDENTITY,
    CustomerID INT FOREIGN KEY REFERENCES Customer(ID),
    OrderDate DATETIME,
    TotalAmount DECIMAL(18,2)
);
```

**End-to-End Test:**
```csharp
[Fact]
public async Task EndToEnd_GenerateFromDatabase_Success()
{
    // Arrange
    var analyzer = new DatabaseAnalyzer();
    var sqlGen = new SqlGenerator();
    var entityGen = new EntityGenerator();
    var writer = new FileWriter();
    
    // Act
    // 1. Analyze
    var schema = await analyzer.AnalyzeAsync(connectionString);
    
    // 2. Generate SQL
    var customerTable = schema.Tables
        .First(t => t.Name == "Customer");
    var getSp = sqlGen.GenerateGetByIdSP(customerTable);
    var updateSp = sqlGen.GenerateUpdateSP(customerTable);
    
    // 3. Generate Entity
    var entity = entityGen.Generate(customerTable);
    
    // 4. Write to disk
    await writer.WriteFileAsync("SP_GetCustomer.sql", getSp);
    await writer.WriteFileAsync("SP_UpdateCustomer.sql", updateSp);
    await writer.WriteFileAsync("Customer.cs", entity);
    
    // Assert
    Assert.True(File.Exists("SP_GetCustomer.sql"));
    Assert.True(File.Exists("SP_UpdateCustomer.sql"));
    Assert.True(File.Exists("Customer.cs"));
    
    // Verify content
    var spContent = await File.ReadAllTextAsync("SP_GetCustomer.sql");
    Assert.Contains("SP_GetCustomerByID", spContent);
    Assert.Contains("@CustomerID INT", spContent);
    
    var entityContent = await File.ReadAllTextAsync("Customer.cs");
    Assert.Contains("public class Customer", entityContent);
    Assert.Contains("public string PasswordHashed", entityContent);
    Assert.Contains("public string CreditCard", entityContent);
}
```

**בדיקות נוספות:**
```csharp
[Fact]
public async Task EndToEnd_ProtectedPrtFile_NotOverwritten()
{
    // כתיבה ראשונית
    await writer.WriteFileAsync("Customer.prt.cs", "manual code");
    
    // ניסיון שני - אמור להיכשל
    await Assert.ThrowsAsync<ProtectedFileException>(
        () => writer.WriteFileAsync("Customer.prt.cs", "new code"));
}

[Fact]
public async Task EndToEnd_PrefixHandling_Correct()
{
    // Verify eno_ → Hashed
    // Verify ent_ → Encrypted
    // Verify lkp_ → Lookup
}
```

**מה נעשה:**
- [ ] TestDatabase.sql
- [ ] EndToEndTests.cs
- [ ] 5+ test scenarios
- [ ] Expected outputs for validation
- [ ] Build verification

**זמן:** 2 ימים (6-8 שעות)

---

#### יום 5: Documentation & Polish (2-3 שעות)

**GENERATORS.md:**
```markdown
# Using Generators

## Quick Start

```csharp
// 1. Analyze database
var analyzer = new DatabaseAnalyzer();
var schema = await analyzer.AnalyzeAsync(connectionString);

// 2. Generate SQL
var sqlGen = new SqlGenerator();
var table = schema.Tables.First();
var sp = sqlGen.GenerateGetByIdSP(table);

// 3. Generate Entity
var entityGen = new EntityGenerator();
var entity = entityGen.Generate(table);

// 4. Write files
var writer = new FileWriter();
await writer.WriteFileAsync($"SP_Get{table.Name}.sql", sp);
await writer.WriteFileAsync($"{table.Name}.cs", entity);
```

## SQL Generator

### Supported SPs
- **SP_GetByID** - Get single record
- **SP_Update** - Update all fields
- **SP_Delete** - Delete record

### Prefix Handling
- `eno_` - Hashed (excluded from Update)
- `ent_` - Encrypted (special handling)
- `clc_`, `blg_`, `agg_` - Read-only
- `spt_` - Separate SP for each

## Entity Generator

### Property Types
SQL Type → C# Type:
- INT → int
- BIGINT → long
- NVARCHAR(n) → string
- DECIMAL → decimal
- DATETIME → DateTime
- BIT → bool

### Prefix Behaviors
...
```

**Examples folder:**
```
Examples/
├── Customer.sql (DB script)
├── SP_GetCustomer.sql (output)
├── SP_UpdateCustomer.sql (output)
├── Customer.cs (output)
└── README.md (explanation)
```

**מה נעשה:**
- [ ] GENERATORS.md מלא
- [ ] Examples folder + files
- [ ] README updates
- [ ] Code comments finalization

**זמן:** 0.5 יום (2-3 שעות)

---

## ✅ Phase 1.5 Deliverables

| רכיב | פלט | Tests | Docs |
|------|-----|-------|------|
| **SQL Generator** | 3 SP templates | 10+ | ✅ |
| **Entity Generator** | C# classes | 15+ | ✅ |
| **File Writer** | Write + protection | 10+ | ✅ |
| **Integration** | End-to-End | 5+ | ✅ |
| **Documentation** | GENERATORS.md | - | ✅ |
| **סה"כ** | 5 רכיבים | **40+ tests** | **מלא** |

---

## 🎯 Success Criteria

### Functional:
- ✅ יוצר SP_GetByID מלא ופועל
- ✅ יוצר SP_Update מלא ופועל
- ✅ יוצר SP_Delete מלא ופועל
- ✅ יוצר Entity class מלא
- ✅ מטפל ב-12 Prefixes נכון
- ✅ מגן על *.prt files
- ✅ End-to-End test עובר

### Quality:
- ✅ Code Coverage: 80%+
- ✅ SonarQube Grade: A
- ✅ All tests passing
- ✅ No build errors
- ✅ Documentation complete

### Performance:
- ✅ Generate SP: < 100ms
- ✅ Generate Entity: < 100ms
- ✅ Write file: < 50ms
- ✅ End-to-End: < 5 seconds

---

## 📊 מעקב התקדמות

### Week 1:
| יום | משימות | שעות | סטטוס |
|-----|--------|------|-------|
| 1 | SQL Setup | 3-4 | ⬜ |
| 2-3 | SQL Templates | 6-8 | ⬜ |
| 4 | Entity Setup | 3-4 | ⬜ |
| 5 | Entity Props | 4-6 | ⬜ |
| **סה"כ** | **4 משימות** | **16-22** | **0%** |

### Week 2:
| יום | משימות | שעות | סטטוס |
|-----|--------|------|-------|
| 1 | File Writer | 3-4 | ⬜ |
| 2 | *.prt Protection | 3-4 | ⬜ |
| 3-4 | Integration | 6-8 | ⬜ |
| 5 | Docs | 2-3 | ⬜ |
| **סה"כ** | **4 משימות** | **14-19** | **0%** |

### סיכום:
- **משימות:** 8 משימות עיקריות
- **זמן:** 30-41 שעות (10 ימי עבודה)
- **Tests:** 40+ tests
- **Coverage:** 80%+

---

## 💡 Tips for Success

### תכנון:
1. **קרא ה-User Manual** - הבן בדיוק מה TargCC עושה
2. **בדוק VB.NET code** - למד מהקוד הקיים
3. **התחל פשוט** - SP בסיסי קודם, אח"כ Prefixes
4. **Test First** - כתוב test לפני implementation

### ביצוע:
1. **Commit קטנים** - אחרי כל משימה משנית
2. **Run tests תמיד** - אחרי כל שינוי
3. **תיעוד בזמן אמת** - לא בסוף
4. **Code Review** - בדוק את עצמך

### איכות:
1. **SonarQube ירוק** - תמיד
2. **Coverage 80%+** - חובה
3. **No warnings** - לפחות במה שכתבת
4. **XML Comments** - בכל API

---

## 🚨 Risks & Mitigation

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Prefix logic מורכב מדי | High | Medium | התחל פשוט, הוסף בהדרגה |
| *.prt protection לא עובד | High | Low | Tests מקיפים |
| Performance issues | Medium | Low | Profiling מראש |
| Incomplete docs | Medium | Medium | כתוב בזמן אמת |

---

## 🎉 מה הלאה?

**אחרי Phase 1.5:**
→ **Phase 2: Full Code Generation**

מה נוסיף:
- DBController Generator (Business Logic)
- WSController Generator (Client)
- Web Service Generator
- WinForms Generator
- TaskManager Generator
- Support projects

**זמן צפוי:** 4-5 שבועות  
**Deliverable:** 8 פרויקטים מלאים מ-DB!

---

## 📞 Questions?

**תקוע?**
1. בדוק `CORE_PRINCIPLES.md`
2. בדוק `PROJECT_ROADMAP.md`
3. בדוק VB.NET code קיים
4. שאל!

---

**תאריך יצירה:** 15/11/2025  
**נוצר על ידי:** Doron + Claude  
**סטטוס:** Ready to Start! 🚀

**הבא:** יום 1 - SQL Generator Setup!
