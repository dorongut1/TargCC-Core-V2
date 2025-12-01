# TargCC Core V2 - Project Capabilities & Roadmap

## תאריך: 2025-12-01 | עדכון סופי יום 37

---

## 🎯 מה הפרויקט כבר מסוגל לעשות - מקצה לקצה

### ✅ **1. ניהול חיבורי מסד נתונים**

#### **Frontend (React + MUI)**
- ✅ דף Connections מלא - הוספה, עריכה, מחיקה של חיבורים
- ✅ בדיקת חיבור (Test Connection) ב-real-time
- ✅ בחירה ויזואלית של חיבור פעיל
- ✅ שמירה ב-localStorage של חיבור נבחר
- ✅ סנכרון עם ConnectionStore גלובלי

#### **Backend (ASP.NET Core 9)**
- ✅ שמירת חיבורים ב-JSON file
- ✅ בניית Connection String אוטומטית מפרטי החיבור
- ✅ ConnectionStringMiddleware - העברת חיבור דרך HTTP headers
- ✅ X-Connection-String header injection אוטומטי

**זרימה מלאה**:
```
User → Add Connection (Server, DB, Auth)
  → Test Connection ✅
  → Save to JSON
  → Auto-select as active
  → Stored in localStorage + ConnectionStore
  → All API calls include X-Connection-String header
```

---

### ✅ **2. ניתוח ו-Inspection של מסד נתונים**

#### **Database Analysis**
- ✅ קריאת Schema מ-SQL Server
- ✅ קריאת רשימת Tables
- ✅ ניתוח Columns (שם, טיפוס, nullable, PK, identity)
- ✅ זיהוי Indexes
- ✅ זיהוי Relationships (Foreign Keys)
- ✅ זיהוי Prefixes מיוחדים:
  - `clc_` - Calculated columns
  - `agg_` - Aggregate columns
  - `blg_` - Business Logic columns
  - `spt_` - Separate Update columns

#### **Frontend Display**
- ✅ דף Tables - רשימת כל הטבלאות
- ✅ הצגת Columns עם כל הפרטים
- ✅ הצגת Indexes
- ✅ הצגת Relationships
- ✅ Skeleton loaders בזמן טעינה

**זרימה מלאה**:
```
User selects connection
  → /api/tables GET (with X-Connection-String header)
  → DatabaseAnalyzer scans SQL Server
  → Returns all tables with full metadata
  → Frontend displays in MUI DataGrid
```

---

### ✅ **3. יצירת קוד אוטומטית (Code Generation)**

#### **מה נוצר עבור כל טבלה:**

##### **A. Entity Class (C#)**
```csharp
// Generated/Entities/Customer.cs
[Table("Customer")]
public partial class Customer
{
    [Column("ID")]
    [Key]
    public long ID { get; set; }

    [Column("CustomerCode")]
    [Required]
    [MaxLength(100)]
    public string CustomerCode { get; set; }

    // ... all columns with proper attributes
}
```

##### **B. Stored Procedures (SQL)** - **כל הבאגים תוקנו ביום 37!**

###### **Basic CRUD:**
- ✅ `SP_GetCustomerByID` - Get single record by PK
- ✅ `SP_UpdateCustomer` - Update record (excludes blg_, agg_, clc_, spt_)
- ✅ `SP_DeleteCustomer` - Hard delete OR Soft delete (if IsActive/IsDeleted exists)

###### **Utility Procedures:**
- ✅ `SP_GetAllCustomer` - Get all records
- ✅ `SP_GetCustomerCount` - Count records
- ✅ `SP_ExistsCustomer` - Check if record exists by PK
- ✅ `SP_CloneCustomer` - Clone record with new ID (resets certain columns)

###### **Index-Based Queries:**
- ✅ `SP_GetCustomerByXXX` - One procedure per index
- ✅ `SP_FillCustomerByXXX` - Fill DataTable by index

###### **Special Update Procedures:**
- ✅ `SP_UpdateCustomerFriend` - Updates including blg_ (business logic) columns
- ✅ `SP_UpdateCustomerAggregates` - Updates only agg_ (aggregate) columns
- ✅ `SP_UpdateCustomerXXX` - Separate procedures for each spt_ column

###### **Advanced Procedures:**
- ✅ `SP_GetCustomerPaged` - Pagination with dynamic ORDER BY (using sp_executesql)
- ✅ `SP_SearchCustomer` - Full-text search on CHAR/VARCHAR columns
- ✅ `SP_BulkInsertCustomer` - Bulk insert using Table Type
- ✅ `SP_GetCustomerAsJSON` - Return results as JSON (if needed)

**כל ה-SPs משתמשים ב-CREATE OR ALTER** - אפשר להריץ מספר פעמים ללא שגיאות!

#### **Frontend - Generation Process:**
```
Tables page → Select table → Click "Generate" button
  → POST /api/generate with tableNames[]
  → Server generates:
    - Entities (.cs files)
    - Stored Procedures (.sql files)
  → Returns paths to generated files
  → User can copy SQL and run in SSMS ✅
```

**זרימה מלאה**:
```
User: Click "Generate" on CustomerDebt table
  ↓
POST /api/generate
  Body: { tableNames: ["CustomerDebt"], options: {...} }
  Header: X-Connection-String: "Server=...;Database=...;"
  ↓
Server:
  1. Analyzes CustomerDebt table (columns, indexes, relationships)
  2. Generates CustomerDebt.cs entity
  3. Generates CustomerDebt_StoredProcedures.sql (20+ procedures!)
  4. Saves to: C:\...\TargCC.WebAPI\Generated\
  ↓
Response:
  {
    success: true,
    filesGenerated: [
      "Generated/Entities/CustomerDebt.cs",
      "Generated/Sql/CustomerDebt_StoredProcedures.sql"
    ]
  }
  ↓
User: Copy SQL → Open SSMS → Run → ✅ No Errors!
```

---

### ✅ **4. Architecture Features**

#### **Design Patterns Implemented:**
- ✅ Singleton Pattern (ConnectionStore)
- ✅ Middleware Pattern (ConnectionStringMiddleware)
- ✅ Context API (ConnectionContext)
- ✅ Repository Pattern (ready for repositories generation)
- ✅ Template Pattern (SQL templates)

#### **Security:**
- ✅ SQL Injection prevention in dynamic ORDER BY (QUOTENAME + whitelist)
- ✅ Parameter validation in all stored procedures
- ✅ Connection string stored securely (not in source control)

#### **Performance:**
- ✅ Parallel API calls where possible
- ✅ Skeleton loaders for better UX
- ✅ Caching of connection data in localStorage
- ✅ Lazy loading of table data

---

## 🚧 מה עוד צריך להוסיף (Roadmap)

### **Priority 1 - קריטי לפונקציונליות מלאה** 🔴

#### **1.1 GenerationWizard - להשלים או להסיר**
**מיקום**: `src/TargCC.WebUI/src/components/wizard/GenerationWizard.tsx:332`
```typescript
const handleFinish = async () => {
    // TODO: Implement actual code generation
    console.log('Generating code with:', wizardData);
};
```

**אופציות**:
- **אופציה A**: להשלים - לחבר ל-/api/generate כמו ב-Tables
- **אופציה B**: להסיר - Tables כבר עושה את העבודה
- **אופציה C**: להפוך לAdvanced mode עם אופציות מתקדמות

**המלצה**: **אופציה B** - Tables מספיק. GenerationWizard מיותר.

---

#### **1.2 Dashboard - Table Count אמיתי**
**מיקום**: `src/TargCC.WebAPI/Program.cs:121`
```csharp
TableCount = 0 // TODO: Get actual table count
```

**מה חסר**: שאילתה אמיתית למספר הטבלאות במסד נתונים

**תיקון מוצע**:
```csharp
var tableCount = await schemaService.GetTableCountAsync(connectionString, s);
TableCount = tableCount
```

---

#### **1.3 Repository Generation**
**סטטוס**: Interface קיים, אבל לא מחובר ל-UI

**מה יש**:
- ✅ `IRepositoryGenerator` interface
- ✅ `RepositoryGenerator` class
- ✅ Templates for Repository pattern

**מה חסר**:
- ❌ חיבור ל-/api/generate endpoint
- ❌ אופציה ב-UI (checkbox "Generate Repository")
- ❌ בדיקה ש-EF Core packages מותקנים

**מה ייווצר**:
```csharp
// ICustomerRepository.cs
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(long id);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer> AddAsync(Customer entity);
    Task UpdateAsync(Customer entity);
    Task DeleteAsync(long id);
}

// CustomerRepository.cs
public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;
    // ... implementation using EF Core
}
```

---

#### **1.4 Service Layer Generation**
**סטטוס**: לא מוּשלם

**מה יצטרך**:
```csharp
// ICustomerService.cs
public interface ICustomerService
{
    Task<CustomerDto> GetByIdAsync(long id);
    Task<PagedResult<CustomerDto>> GetPagedAsync(int page, int size);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request);
    Task UpdateAsync(long id, UpdateCustomerRequest request);
    Task DeleteAsync(long id);
}

// CustomerService.cs - Business logic layer
```

---

#### **1.5 API Controllers Generation**
**סטטוס**: Interface קיים, אבל לא מוּשלם

**מה יש**:
- ✅ `IApiControllerGenerator` interface
- ✅ `ApiControllerGenerator` class

**מה חסר**:
- ❌ חיבור מלא ל-UI
- ❌ Templates עם OpenAPI/Swagger attributes
- ❌ Validation attributes

**מה ייווצר**:
```csharp
[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _service;

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return result != null ? Ok(result) : NotFound();
    }

    // ... all CRUD operations
}
```

---

### **Priority 2 - Features משמעותיים** 🟡

#### **2.1 DTOs Generation (CQRS Pattern)**
**סטטוס**: Infrastructure קיים, אבל לא מחובר

**מה יש**:
- ✅ `IDtoGenerator`, `ICommandGenerator`, `IQueryGenerator`

**מה ייווצר**:
```csharp
// CustomerDto.cs
public class CustomerDto
{
    public long Id { get; set; }
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    // ... only needed fields
}

// Commands
public record CreateCustomerCommand(string Code, string Name) : IRequest<long>;
public record UpdateCustomerCommand(long Id, string Code, string Name) : IRequest;
public record DeleteCustomerCommand(long Id) : IRequest;

// Queries
public record GetCustomerByIdQuery(long Id) : IRequest<CustomerDto?>;
public record GetCustomersPagedQuery(int Page, int Size) : IRequest<PagedResult<CustomerDto>>;
```

---

#### **2.2 DbContext Generation**
**סטטוס**: קיים אבל לא בשימוש

**מה ייווצר**:
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    // ... all entities

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

---

#### **2.3 Entity Configurations (Fluent API)**
**סטטוס**: Interface קיים

**מה ייווצר**:
```csharp
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CustomerCode).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.CustomerCode).IsUnique();

        builder.HasMany(c => c.Orders)
               .WithOne(o => o.Customer)
               .HasForeignKey(o => o.CustomerId);
    }
}
```

---

#### **2.4 Unit Tests Generation**
**סטטוס**: לא קיים

**מה יצטרך**:
```csharp
public class CustomerRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly CustomerRepository _repository;

    public CustomerRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        _context = new AppDbContext(options);
        _repository = new CustomerRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCustomer()
    {
        // Arrange
        var customer = new Customer { Id = 1, CustomerCode = "C001" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("C001", result.CustomerCode);
    }
}
```

---

### **Priority 3 - Mock Endpoints שצריך להשלים** 🟢

#### **3.1 Schema Reading Endpoint**
**מיקום**: `Program.cs:526`
```csharp
// TODO: Implement actual schema reading
```

**מה חסר**: קריאת Schema מלאה עם כל ה-metadata

---

#### **3.2 Security Analysis**
**מיקום**: `Program.cs:578`
```csharp
// TODO: Implement actual security analysis using ISecurityScanner
```

**מה יכול לעשות**:
- סריקת SQL Injection vulnerabilities
- בדיקת permissions
- זיהוי sensitive data (SSN, Credit Cards)
- המלצות אבטחה

---

#### **3.3 Code Quality Analysis**
**מיקום**: `Program.cs:636`
```csharp
// TODO: Implement actual quality analysis using ICodeQualityAnalyzer
```

**מה יכול לעשות**:
- Code complexity metrics
- Naming conventions checks
- Code smells detection
- Best practices recommendations

---

#### **3.4 Interactive Chat**
**מיקום**: `Program.cs:684`
```csharp
// TODO: Implement actual chat using IInteractiveChatService
```

**מה יכול לעשות**:
- שאלות על מסד נתונים ("Show me all customers from last month")
- הצעות לאופטימיזציה
- הסבר על relationships
- AI-powered code generation suggestions

---

### **Priority 4 - UX/UI Enhancements** 🟢

#### **4.1 Code Preview בעת Generation**
- ❌ לא מציג את הקוד שנוצר ישירות ב-UI
- ❌ צריך ללכת לקובץ ידנית

**שיפור מוצע**: Modal עם Code Viewer + Copy button

---

#### **4.2 Generation History**
- ❌ אין היסטוריה של Generations קודמים
- ❌ לא ניתן לראות מה נוצר בעבר

**שיפור מוצע**: דף History עם:
- מתי נוצר
- אילו טבלאות
- אילו קבצים
- הצלחה/כישלון

---

#### **4.3 Batch Generation**
- ❌ לא ניתן לבחור מספר טבלאות ביחד

**שיפור מוצע**: Checkboxes ב-Tables page + "Generate Selected"

---

#### **4.4 Download Generated Files**
- ❌ לא ניתן להוריד את הקבצים מה-UI
- ❌ צריך ללכת לתיקייה ידנית

**שיפור מוצע**: ZIP download של כל הקבצים שנוצרו

---

## 📈 הערכת השלמה

### **מה עובד - 100%** ✅
1. ניהול חיבורים
2. ניתוח DB
3. Entity generation
4. SQL Stored Procedures generation (כל הבאגים תוקנו!)
5. Basic UI flow

### **מה חלקי - ~50%** ⚠️
1. Repository generation (קיים אבל לא מחובר)
2. API Controller generation (קיים אבל לא מושלם)
3. CQRS/DTOs (קיים אבל לא מחובר)

### **מה חסר - 0%** ❌
1. Service layer generation
2. Unit tests generation
3. DbContext full generation
4. Security/Quality analysis
5. Interactive chat
6. UI enhancements

---

## 🎯 המלצה לשלבים הבאים

### **שלב 1 - השלמת Generation בסיסי (שבוע 1)**
1. ✅ תיקון כל באגי SQL (בוצע!)
2. להסיר או להשלים GenerationWizard
3. לתקן Dashboard table count
4. לחבר Repository generation ל-UI

### **שלב 2 - Full Stack Generation (שבועות 2-3)**
1. Service layer generation
2. API Controllers מושלם
3. DTOs + CQRS
4. DbContext מלא

### **שלב 3 - Advanced Features (שבועות 4-6)**
1. Unit tests generation
2. Security analysis
3. Code quality analysis
4. Batch operations

### **שלב 4 - UX Polish (שבוע 7)**
1. Code preview in UI
2. Generation history
3. Download files
4. Better error handling

---

## 🔥 Quick Win - מה אפשר לעשות עכשיו

### **Option A: Repository Generation**
**זמן משוער**: 2-3 שעות
**מה צריך**:
1. להוסיף checkbox "Generate Repository" ב-Tables
2. לחבר `RepositoryGenerator` ל-/api/generate
3. לבדוק שה-templates עובדים

### **Option B: להסיר GenerationWizard**
**זמן משוער**: 30 דקות
**מה צריך**:
1. למחוק `GenerationWizard.tsx`
2. להסיר route מ-App.tsx
3. להסיר מה-navigation

### **Option C: Code Preview Modal**
**זמן משוער**: 2 שעות
**מה צריך**:
1. Modal component עם syntax highlighting
2. Copy button
3. Download button
4. להציג אחרי generation מוצלח

---

**סוף מסמך - עודכן ביום 37 אחרי תיקון כל הבאגים! 🎉**
