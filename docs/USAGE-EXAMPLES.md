# TargCC Usage Examples

**Real-world scenarios and workflows with TargCC CLI**

---

## Table of Contents

- [New Project from Scratch](#new-project-from-scratch)
- [Adding a New Table](#adding-a-new-table)
- [Modifying Existing Tables](#modifying-existing-tables)
- [Database Schema Evolution](#database-schema-evolution)
- [Working with Special Prefixes](#working-with-special-prefixes)
- [Security Best Practices](#security-best-practices)
- [Development Workflows](#development-workflows)
- [Team Collaboration](#team-collaboration)
- [CI/CD Integration](#cicd-integration)

---

## New Project from Scratch

### Scenario: E-Commerce Application

You have an existing e-commerce database with tables: `Customer`, `Order`, `Product`, `Category`.

#### Step 1: Setup

```bash
# Create project directory
mkdir EcommerceApp
cd EcommerceApp

# Initialize TargCC
targcc init
```

**Interactive prompts:**
```
Enter connection string: Server=localhost;Database=Ecommerce;Trusted_Connection=true;
Enter output directory: .
Enter default namespace: Ecommerce
```

#### Step 2: Analyze Your Database

```bash
# See what you're working with
targcc analyze schema
```

**Output:**
```
Database Schema Analysis

Tables: 4
Total Columns: 42
Foreign Keys: 3
Indexes: 8

Tables:
┌──────────┬─────────┬───────────┬──────────┐
│ Table    │ Columns │ Relations │ Indexes  │
├──────────┼─────────┼───────────┼──────────┤
│ Customer │ 8       │ 0         │ 2        │
│ Order    │ 12      │ 1         │ 3        │
│ Product  │ 10      │ 1         │ 2        │
│ Category │ 5       │ 1         │ 1        │
└──────────┴─────────┴───────────┴──────────┘
```

#### Step 3: Generate Complete Project

```bash
targcc generate project --name "EcommerceApp"
```

**Generated structure:**
```
EcommerceApp/
├── EcommerceApp.sln
├── targcc.json
└── src/
    ├── EcommerceApp.Domain/
    ├── EcommerceApp.Application/
    ├── EcommerceApp.Infrastructure/
    └── EcommerceApp.API/
```

#### Step 4: Build & Run

```bash
dotnet restore
dotnet build
dotnet run --project src/EcommerceApp.API
```

#### Step 5: Test Your API

Open browser: `https://localhost:5001/swagger`

**Available endpoints:**
- `/api/customers` - CRUD for customers
- `/api/orders` - CRUD for orders
- `/api/products` - CRUD for products
- `/api/categories` - CRUD for categories

**Total time:** 5 minutes  
**Files generated:** ~80  
**Lines of code:** ~4,000+

---

## Adding a New Table

### Scenario: Add Reviews Table

You need to add product reviews to your e-commerce application.

#### Step 1: Create Table in Database

```sql
CREATE TABLE Review (
    ID INT PRIMARY KEY IDENTITY(1,1),
    ProductID INT NOT NULL,
    CustomerID INT NOT NULL,
    Rating INT NOT NULL,
    Title NVARCHAR(100),
    Comment NVARCHAR(MAX),
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ProductID) REFERENCES Product(ID),
    FOREIGN KEY (CustomerID) REFERENCES Customer(ID)
);
```

#### Step 2: Generate All Code

```bash
targcc generate all Review
```

**Output:**
```
Generate All: Review

✓ Generated 20 file(s) in 1.8s

  Entity:
    ✓ Review.cs (35 lines)
  SQL:
    ✓ Review_GetByID.sql (15 lines)
    ✓ Review_GetAll.sql (10 lines)
    ✓ Review_Insert.sql (20 lines)
    ✓ Review_Update.sql (22 lines)
    ✓ Review_Delete.sql (8 lines)
    ✓ Review_Search.sql (12 lines)
  Repository:
    ✓ IReviewRepository.cs (10 lines)
    ✓ ReviewRepository.cs (75 lines)
  CQRS:
    [10 files for queries/commands]
  API:
    ✓ ReviewsController.cs (110 lines)

Output directory: C:\EcommerceApp\src
```

#### Step 3: Update Dependencies

Add the new repository to dependency injection:

```csharp
// Infrastructure/DependencyInjection.cs (manual edit)
services.AddScoped<IReviewRepository, ReviewRepository>();
```

#### Step 4: Build & Test

```bash
dotnet build
dotnet run --project src/EcommerceApp.API
```

**New endpoints available:**
```
GET    /api/reviews
GET    /api/reviews/{id}
POST   /api/reviews
PUT    /api/reviews/{id}
DELETE /api/reviews/{id}
```

**Total time:** 2 minutes  
**Manual work:** 1 line (DI registration)

---

## Modifying Existing Tables

### Scenario: Change Email Column Length

You need to increase the `Email` column in `Customer` table from `NVARCHAR(100)` to `NVARCHAR(255)`.

#### Step 1: Check Impact First

```bash
targcc analyze impact --table Customer --column Email --new-type "nvarchar(255)"
```

**Output:**
```
Impact Analysis: Customer

Change: Email type nvarchar(100) → nvarchar(255)

Affected Files: 8

✓ Auto-updated (5 files):
  - Domain/Entities/Customer.cs
  - SQL/Customer_Insert.sql
  - SQL/Customer_Update.sql
  - Application/DTOs/CustomerDto.cs
  - Infrastructure/Repositories/CustomerRepository.cs

⚠️  Manual review required (3 files):
  - Application/Customers/Validators/CreateCustomerValidator.prt.cs
  - Application/Customers/Validators/UpdateCustomerValidator.prt.cs
  - API/Controllers/CustomersController.prt.cs

Build errors expected: 2 (intentional safety net)

Estimated fix time: 5-10 minutes
```

#### Step 2: Make Database Change

```sql
ALTER TABLE Customer
ALTER COLUMN Email NVARCHAR(255) NOT NULL;
```

#### Step 3: Regenerate Code

```bash
targcc generate all Customer
```

#### Step 4: Build (Expect Errors!)

```bash
dotnet build
```

**Expected errors:**
```
Error CS0103: The name 'MaxLength' does not exist in the current context
  Location: CreateCustomerValidator.prt.cs, line 12
  
  RuleFor(x => x.Email).MaxLength(100); // ← Old value!
```

#### Step 5: Fix Manual Code

```csharp
// CreateCustomerValidator.prt.cs
RuleFor(x => x.Email).MaxLength(255); // ← Updated!
```

#### Step 6: Rebuild & Test

```bash
dotnet build    # ✓ Success
dotnet test     # ✓ All tests pass
```

**Total time:** 10 minutes  
**Build errors:** 2 (expected and helpful!)

---

## Database Schema Evolution

### Scenario: Complex Refactoring

You need to split `Customer` table into `Customer` and `CustomerAddress`.

#### Before:
```sql
CREATE TABLE Customer (
    ID INT PRIMARY KEY,
    Name NVARCHAR(100),
    Email NVARCHAR(255),
    Street NVARCHAR(200),
    City NVARCHAR(100),
    State NVARCHAR(50),
    ZipCode NVARCHAR(20)
);
```

#### After:
```sql
CREATE TABLE Customer (
    ID INT PRIMARY KEY,
    Name NVARCHAR(100),
    Email NVARCHAR(255)
);

CREATE TABLE CustomerAddress (
    ID INT PRIMARY KEY,
    CustomerID INT NOT NULL,
    Street NVARCHAR(200),
    City NVARCHAR(100),
    State NVARCHAR(50),
    ZipCode NVARCHAR(20),
    IsPrimary BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (CustomerID) REFERENCES Customer(ID)
);
```

#### Step 1: Check Impact

```bash
targcc analyze impact --table Customer --change "Remove address fields, add CustomerAddress table"
```

**Output shows:** ~15 affected files

#### Step 2: Make Database Changes

```sql
-- 1. Create new table
CREATE TABLE CustomerAddress (...);

-- 2. Migrate data
INSERT INTO CustomerAddress (CustomerID, Street, City, State, ZipCode, IsPrimary)
SELECT ID, Street, City, State, ZipCode, 1
FROM Customer;

-- 3. Drop old columns
ALTER TABLE Customer DROP COLUMN Street;
ALTER TABLE Customer DROP COLUMN City;
ALTER TABLE Customer DROP COLUMN State;
ALTER TABLE Customer DROP COLUMN ZipCode;
```

#### Step 3: Regenerate Both Tables

```bash
targcc generate all Customer
targcc generate all CustomerAddress
```

#### Step 4: Build & Review Errors

```bash
dotnet build
```

**Errors tell you exactly what to fix:**
```
Error: 'Customer' does not contain a definition for 'Street'
Error: 'Customer' does not contain a definition for 'City'
... (13 more errors)
```

#### Step 5: Fix Manual Code

Update all `*.prt.cs` files that referenced the old address fields.

```csharp
// Before
var address = $"{customer.Street}, {customer.City}, {customer.State}";

// After
var primaryAddress = customer.Addresses.FirstOrDefault(a => a.IsPrimary);
var address = primaryAddress != null 
    ? $"{primaryAddress.Street}, {primaryAddress.City}, {primaryAddress.State}"
    : "No address";
```

#### Step 6: Add Navigation Property (Manual)

```csharp
// Customer.prt.cs
public partial class Customer
{
    public ICollection<CustomerAddress> Addresses { get; set; }
}
```

**Total time:** 30-45 minutes  
**Build errors:** 15 (all pointing to exact locations)  
**Result:** Clean refactoring with no hidden bugs

---

## Working with Special Prefixes

### Scenario: Secure User Authentication

You're building a user authentication system with encrypted and hashed fields.

#### Database Schema:
```sql
CREATE TABLE User (
    ID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    eno_Password VARCHAR(64) NOT NULL,        -- Hashed (SHA-256)
    ent_SecurityQuestion NVARCHAR(500),       -- Encrypted
    ent_SecurityAnswer NVARCHAR(500),         -- Encrypted
    lkp_Status VARCHAR(20) NOT NULL,          -- Lookup (Active/Inactive/Locked)
    agg_LoginAttempts INT NOT NULL DEFAULT 0, -- Aggregate
    CreatedDate DATETIME NOT NULL,
    LastLoginDate DATETIME
);
```

#### Generate Code:
```bash
targcc generate all User
```

#### Generated Entity:
```csharp
public class User : BaseEntity
{
    public int ID { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    
    // eno_ → Hashed (private setter)
    public string PasswordHashed { get; private set; }
    
    public void SetPassword(string plainText)
    {
        PasswordHashed = HashingService.ComputeSHA256Hash(plainText);
    }
    
    public bool VerifyPassword(string plainText)
    {
        var hash = HashingService.ComputeSHA256Hash(plainText);
        return hash == PasswordHashed;
    }
    
    // ent_ → Encrypted
    private string _securityQuestion;
    public string SecurityQuestion
    {
        get => EncryptionService.Decrypt(_securityQuestion);
        set => _securityQuestion = EncryptionService.Encrypt(value);
    }
    
    private string _securityAnswer;
    public string SecurityAnswer
    {
        get => EncryptionService.Decrypt(_securityAnswer);
        set => _securityAnswer = EncryptionService.Encrypt(value);
    }
    
    // lkp_ → Lookup
    public string StatusCode { get; set; }
    public string StatusText { get; set; }
    
    // agg_ → Aggregate
    public int LoginAttemptsAggregate { get; private set; }
    
    public void IncrementLoginAttempts()
    {
        LoginAttemptsAggregate++;
    }
    
    public void ResetLoginAttempts()
    {
        LoginAttemptsAggregate = 0;
    }
}
```

#### Usage:
```csharp
// Creating a user
var user = new User
{
    Username = "john.doe",
    Email = "john@example.com",
    SecurityQuestion = "Mother's maiden name?", // ← Automatically encrypted
    SecurityAnswer = "Smith",                   // ← Automatically encrypted
    StatusCode = "Active"
};
user.SetPassword("MySecretPassword"); // ← Automatically hashed

await repository.CreateAsync(user);

// Verifying password
if (user.VerifyPassword(loginAttempt.Password))
{
    user.ResetLoginAttempts();
    // Login successful
}
else
{
    user.IncrementLoginAttempts();
    if (user.LoginAttemptsAggregate >= 5)
    {
        user.StatusCode = "Locked";
    }
}
```

**Security benefits:**
- ✅ Passwords never stored in plain text
- ✅ Security Q&A encrypted at rest
- ✅ Status managed via lookup table
- ✅ Login attempts tracked automatically

---

## Security Best Practices

### Scenario: Security Audit

Run a security audit on your database before deployment.

#### Step 1: Run Security Scan

```bash
targcc analyze security
```

#### Output:
```
Security Analysis

Critical Issues: 2
High Issues: 3
Medium Issues: 5

Critical:
  ⚠️  User.CreditCard - Not encrypted (ent_ prefix missing)
  ⚠️  User.Password - Not hashed (eno_ prefix missing)

High:
  ⚠️  Customer.Email - Should use ent_ for PII data
  ⚠️  Order.PaymentInfo - Contains sensitive data
  ⚠️  Employee.SSN - Social security number not encrypted

Recommendations:
1. Add ent_ prefix to CreditCard column
2. Add eno_ prefix to Password column
3. Consider ent_ for Email addresses
```

#### Step 2: Fix Critical Issues

```sql
-- Rename columns to add prefixes
EXEC sp_rename 'User.CreditCard', 'ent_CreditCard', 'COLUMN';
EXEC sp_rename 'User.Password', 'eno_Password', 'COLUMN';
EXEC sp_rename 'Customer.Email', 'ent_Email', 'COLUMN';
EXEC sp_rename 'Order.PaymentInfo', 'ent_PaymentInfo', 'COLUMN';
EXEC sp_rename 'Employee.SSN', 'ent_SSN', 'COLUMN';
```

#### Step 3: Regenerate All Affected Tables

```bash
targcc generate all User
targcc generate all Customer
targcc generate all Order
targcc generate all Employee
```

#### Step 4: Update Manual Code

Review and fix build errors (expected).

#### Step 5: Verify

```bash
targcc analyze security
```

**New output:**
```
Security Analysis

Critical Issues: 0  ✓
High Issues: 0      ✓
Medium Issues: 2    ← Acceptable
```

**Result:** Production-ready security!

---

## Development Workflows

### Workflow 1: Feature Branch Development

```bash
# 1. Create feature branch
git checkout -b feature/add-reviews

# 2. Start watch mode
targcc watch &

# 3. Make database changes
# (Watch mode auto-regenerates)

# 4. Develop features
# (Code in *.prt.cs files)

# 5. Test
dotnet test

# 6. Commit everything
git add .
git commit -m "feat: Add reviews feature"
git push origin feature/add-reviews
```

### Workflow 2: Code Review

```bash
# Reviewer checks generated vs manual files
git diff --name-only | grep -E "\\.prt\\.cs$"

# Review only manual changes
git diff $(git diff --name-only | grep -E "\\.prt\\.cs$")

# Ignore generated file changes
git diff '*.prt.cs'
```

### Workflow 3: Database Migration

```bash
# 1. Export current schema
targcc analyze schema --format json > schema-before.json

# 2. Make migrations
dotnet ef migrations add AddReviewsTable

# 3. Apply to database
dotnet ef database update

# 4. Regenerate affected code
targcc generate all Review

# 5. Export new schema
targcc analyze schema --format json > schema-after.json

# 6. Compare schemas
diff schema-before.json schema-after.json
```

---

## Team Collaboration

### Scenario: Multiple Developers

**Developer A** adds `Review` table  
**Developer B** modifies `Product` table

#### Developer A's Branch:
```bash
git checkout -b feature/reviews
# Add Review table in database
targcc generate all Review
git commit -am "feat: Add reviews"
git push
```

#### Developer B's Branch:
```bash
git checkout -b feature/product-improvements
# Modify Product table in database
targcc generate all Product
git commit -am "feat: Improve products"
git push
```

#### Merging:
```bash
git checkout main
git merge feature/reviews
git merge feature/product-improvements  # ← No conflicts!
```

**Why no conflicts?**
- Generated files are separate per table
- Manual files (*.prt.cs) rarely conflict
- Each developer works on different tables

**Best practice:** Review generated code changes separately from manual changes.

---

## CI/CD Integration

### GitHub Actions Example

```yaml
# .github/workflows/targcc-verify.yml
name: TargCC Verification

on: [push, pull_request]

jobs:
  verify:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
    
    - name: Install TargCC CLI
      run: |
        cd tools/TargCC.CLI
        dotnet build
    
    - name: Verify Schema Matches Generated Code
      run: |
        # Regenerate all code
        targcc generate project --force
        
        # Check for differences
        git diff --exit-code || {
          echo "Generated code is out of sync!"
          echo "Run 'targcc generate project' locally"
          exit 1
        }
    
    - name: Security Scan
      run: |
        targcc analyze security --severity High
    
    - name: Quality Check
      run: |
        targcc analyze quality --min-score 80
    
    - name: Build & Test
      run: |
        dotnet build
        dotnet test
```

**This ensures:**
- Generated code is always up-to-date
- No security issues in production
- Code quality standards maintained
- All tests pass

---

## Tips & Tricks

### Tip 1: Use Watch Mode for Rapid Development

```bash
# Terminal 1: Watch mode
targcc watch

# Terminal 2: Make changes
sqlcmd -S localhost -d MyDb -Q "ALTER TABLE Customer ADD PhoneNumber NVARCHAR(20)"

# Terminal 1 shows:
# ⚠️  Change detected in Customer table
# 🔄 Regenerating...
# ✓ Generated 8 files in 1.2s
```

### Tip 2: Preview Changes with Dry Run

```bash
# See what would change without actually changing
targcc generate all Customer --dry-run
```

### Tip 3: Export Analysis Reports

```bash
# Security report for compliance
targcc analyze security --format html --output security-report.html

# Schema documentation for team
targcc analyze schema --format json --output schema.json
```

### Tip 4: Batch Generate Multiple Tables

```bash
# Generate for multiple tables at once
for table in Customer Order Product Category; do
    targcc generate all $table
done
```

### Tip 5: Use Config Profiles

```bash
# Development config
targcc --config targcc.dev.json generate project

# Production config  
targcc --config targcc.prod.json analyze security
```

---

## Common Pitfalls & Solutions

### Pitfall 1: Editing Generated Files

**Wrong:**
```csharp
// Customer.cs (GENERATED FILE)
public partial class Customer
{
    // Adding custom logic here - WILL BE OVERWRITTEN!
    public decimal CalculateLifetimeValue() { ... }
}
```

**Right:**
```csharp
// Customer.prt.cs (MANUAL FILE)
public partial class Customer
{
    // Custom logic here - SAFE!
    public decimal CalculateLifetimeValue() { ... }
}
```

### Pitfall 2: Ignoring Build Errors

**Wrong:** Commenting out errors and moving on

**Right:** Fix errors immediately - they're telling you what to update!

### Pitfall 3: Not Using Impact Analysis

**Wrong:** Make database change → regenerate → deal with 50 errors

**Right:** Check impact first → know what to expect → fix systematically

---

**For more help:**
- [CLI Reference](CLI-REFERENCE.md)
- [Quickstart Guide](QUICKSTART.md)
- [Core Principles](CORE_PRINCIPLES.md)

**Last Updated:** 27/11/2025
