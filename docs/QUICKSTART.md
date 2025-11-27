# TargCC Quickstart Guide

**Get from zero to a running application in 5 minutes! 🚀**

---

## Prerequisites

✅ Visual Studio 2022 or VS Code  
✅ .NET 9 SDK  
✅ SQL Server 2019+ (or Express)  
✅ Basic SQL knowledge

---

## Step 1: Install TargCC (1 minute)

```bash
# Clone the repository
git clone https://github.com/doron/TargCC-Core-V2.git
cd TargCC-Core-V2

# Build the CLI
cd src/TargCC.CLI
dotnet build

# Add to PATH (optional but recommended)
# Windows:
setx PATH "%PATH%;C:\path\to\TargCC-Core-V2\src\TargCC.CLI\bin\Debug\net9.0"

# Or create an alias
doskey targcc=dotnet run --project C:\path\to\TargCC-Core-V2\src\TargCC.CLI\TargCC.CLI.csproj -- $*
```

---

## Step 2: Initialize Project (30 seconds)

```bash
# Create and navigate to your project directory
mkdir MyAwesomeApp
cd MyAwesomeApp

# Initialize TargCC
targcc init
```

**You'll see:**
```
Initializing TargCC

✓ Configuration created successfully
Config file: C:\MyAwesomeApp\targcc.json

Would you like to configure database connection now? (Y/n) y
```

**Enter your details:**
```
Enter connection string: Server=localhost;Database=Northwind;Trusted_Connection=true;
Enter output directory: .
Enter default namespace: MyAwesomeApp
```

**Result:**
```
✓ Configuration saved successfully

TargCC initialized successfully!
Run 'targcc --help' to see available commands
```

---

## Step 3: Generate Complete Project (2 minutes)

```bash
# Generate entire Clean Architecture project from your database
targcc generate project
```

**You'll see:**
```
Generating Clean Architecture Project...

✓ Creating solution structure...
  ✓ MyAwesomeApp.sln
  ✓ src/MyAwesomeApp.Domain/
  ✓ src/MyAwesomeApp.Application/
  ✓ src/MyAwesomeApp.Infrastructure/
  ✓ src/MyAwesomeApp.API/
  ✓ tests/MyAwesomeApp.Tests/

✓ Generating from 8 tables...
  ✓ Customer (20 files)
  ✓ Order (20 files)
  ✓ Product (20 files)
  ✓ Category (20 files)
  ✓ Supplier (20 files)
  ✓ Employee (20 files)
  ✓ Region (20 files)
  ✓ Territory (20 files)

✓ Generating infrastructure...
  ✓ Program.cs
  ✓ appsettings.json
  ✓ DbContext.cs
  ✓ DependencyInjection.cs

✓ Project generated: 168 files in 8.2s
```

**Your project structure:**
```
MyAwesomeApp/
├── MyAwesomeApp.sln
├── targcc.json
└── src/
    ├── MyAwesomeApp.Domain/
    │   ├── Entities/
    │   │   ├── Customer.cs
    │   │   ├── Order.cs
    │   │   └── ...
    │   └── Interfaces/
    ├── MyAwesomeApp.Application/
    │   ├── Customers/
    │   │   ├── Queries/
    │   │   ├── Commands/
    │   │   └── Validators/
    │   └── Common/
    ├── MyAwesomeApp.Infrastructure/
    │   ├── Repositories/
    │   └── Data/
    └── MyAwesomeApp.API/
        ├── Controllers/
        ├── Program.cs
        └── appsettings.json
```

---

## Step 4: Build & Run (1 minute)

```bash
# Restore packages
dotnet restore

# Build the solution
dotnet build

# Run the API
dotnet run --project src/MyAwesomeApp.API
```

**You'll see:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

---

## Step 5: Test Your API (30 seconds)

Open your browser and navigate to:

```
https://localhost:5001/swagger
```

**You'll see Swagger UI with all your APIs:**

```
Customers
  GET    /api/customers
  GET    /api/customers/{id}
  POST   /api/customers
  PUT    /api/customers/{id}
  DELETE /api/customers/{id}

Orders
  GET    /api/orders
  GET    /api/orders/{id}
  POST   /api/orders
  PUT    /api/orders/{id}
  DELETE /api/orders/{id}

... (and all other tables)
```

### Try it out!

1. Click **GET /api/customers**
2. Click **Try it out**
3. Click **Execute**
4. See your data! 🎉

---

## 🎉 Congratulations!

You now have a fully functional Clean Architecture application with:

✅ Entity classes  
✅ SQL stored procedures  
✅ Repository pattern  
✅ CQRS with MediatR  
✅ REST API with Swagger  
✅ FluentValidation  
✅ Dependency injection  
✅ Logging with Serilog  

**Total time:** ~5 minutes  
**Lines of code generated:** ~5,000+  
**Files created:** 150+

---

## What's Next?

### 1. Add Custom Business Logic

Edit the `*.prt.cs` (partial) files to add your custom logic:

```csharp
// src/MyAwesomeApp.Domain/Entities/Customer.prt.cs
public partial class Customer
{
    // Your custom methods here
    public decimal CalculateLifetimeValue()
    {
        return Orders.Sum(o => o.TotalAmount);
    }
    
    public bool IsVIP()
    {
        return CalculateLifetimeValue() > 10000;
    }
}
```

**Important:** Only edit `*.prt.cs` files! Regular `*.cs` files are regenerated and will overwrite your changes.

---

### 2. Modify Database Schema

When you change your database:

```bash
# Check impact of your change
targcc analyze impact --table Customer --change "Add PhoneNumber column"

# Make the database change
# Then regenerate
targcc generate all Customer
```

**You'll get build errors** - this is intentional! Review the errors, update your `*.prt.cs` files, and rebuild.

---

### 3. Use Watch Mode During Development

```bash
# Auto-regenerate when database changes
targcc watch
```

Leave this running in a terminal. When you modify the database schema, TargCC automatically regenerates affected files!

---

### 4. Add a New Table

```bash
# Create the table in your database
# Then generate all code for it
targcc generate all NewTable

# Rebuild
dotnet build
```

Your new API endpoints will appear automatically in Swagger!

---

## Common Commands Cheat Sheet

```bash
# Initialize new project
targcc init

# Show configuration
targcc config show

# Set connection string
targcc config set ConnectionString "Server=..."

# Generate complete project
targcc generate project

# Generate for single table
targcc generate all Customer

# Analyze database
targcc analyze schema

# Check impact of changes
targcc analyze impact --table Customer --change "..."

# Watch for changes
targcc watch

# Get help
targcc --help
targcc generate --help
```

---

## Project Structure Explained

```
MyAwesomeApp/
├── src/
│   ├── Domain/                  ← Business entities (pure C#)
│   │   ├── Entities/           ← Your data models
│   │   └── Interfaces/         ← Repository interfaces
│   │
│   ├── Application/            ← Use cases (CQRS)
│   │   ├── Customers/
│   │   │   ├── Queries/       ← Read operations
│   │   │   ├── Commands/      ← Write operations
│   │   │   └── Validators/    ← Validation rules
│   │   └── Common/
│   │
│   ├── Infrastructure/         ← Data access
│   │   ├── Repositories/      ← Repository implementations
│   │   └── Data/              ← DbContext
│   │
│   └── API/                    ← REST API
│       ├── Controllers/       ← API endpoints
│       ├── Program.cs         ← Application startup
│       └── appsettings.json   ← Configuration
│
└── tests/                      ← Unit tests (coming soon)
```

---

## Understanding Generated Files

### Entity (Domain Layer)
```csharp
// Customer.cs - GENERATED (don't edit)
public partial class Customer : BaseEntity
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// Customer.prt.cs - YOUR CODE (safe to edit)
public partial class Customer
{
    public decimal CalculateLifetimeValue()
    {
        // Your custom logic
    }
}
```

### CQRS Handler (Application Layer)
```csharp
// GetCustomerByIdQueryHandler.cs
public class GetCustomerByIdQueryHandler 
    : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly ICustomerRepository _repository;
    
    public async Task<CustomerDto> Handle(
        GetCustomerByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.Id);
        return _mapper.Map<CustomerDto>(customer);
    }
}
```

### API Controller
```csharp
// CustomersController.cs
[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _mediator.Send(new GetAllCustomersQuery());
        return Ok(customers);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _mediator.Send(new GetCustomerByIdQuery(id));
        return customer != null ? Ok(customer) : NotFound();
    }
}
```

---

## Special Column Prefixes

TargCC recognizes special column prefixes for advanced behavior:

| Prefix | Behavior | Example |
|--------|----------|---------|
| `eno_` | Hashed (one-way) | `eno_Password` → Hashed property |
| `ent_` | Encrypted (two-way) | `ent_CreditCard` → Encrypted property |
| `lkp_` | Lookup | `lkp_Status` → StatusCode + StatusText |
| `agg_` | Aggregate | `agg_OrderCount` → Read-only aggregate |
| `clc_` | Calculated | `clc_Total` → Calculated property |

**Example:**
```sql
-- Database
CREATE TABLE Customer (
    ID INT PRIMARY KEY,
    Name NVARCHAR(100),
    eno_Password VARCHAR(64),    -- Hashed
    ent_CreditCard VARCHAR(MAX),  -- Encrypted
    lkp_Status VARCHAR(10)        -- Lookup
);
```

```csharp
// Generated Entity
public class Customer
{
    public int ID { get; set; }
    public string Name { get; set; }
    
    // eno_ → Hashed
    public string PasswordHashed { get; private set; }
    public void SetPassword(string plainText) { ... }
    
    // ent_ → Encrypted
    public string CreditCard { get; set; } // Auto encrypt/decrypt
    
    // lkp_ → Lookup
    public string StatusCode { get; set; }
    public string StatusText { get; set; }
}
```

---

## Troubleshooting

### Build Errors After Regeneration?

**This is intentional!** TargCC uses "Build Errors as Safety Net" to force you to review schema changes.

**Solution:**
1. Read the error message - it tells you exactly what to fix
2. Update your `*.prt.cs` files
3. Rebuild

**Example:**
```
Error: 'Customer' does not contain a definition for 'OldColumnName'
Location: CustomerController.prt.cs, line 45

→ You need to update line 45 to use the new column name
```

---

### Connection String Issues?

```bash
# Check current configuration
targcc config show

# Update connection string
targcc config set ConnectionString "Server=localhost;Database=MyDb;Trusted_Connection=true;"

# Test connection
targcc analyze schema
```

---

### Files Not Generating?

```bash
# Run with verbose output
targcc generate all Customer --verbose

# Check logs
# Windows: C:\Users\YourName\.targcc\logs\
# Linux/Mac: ~/.targcc/logs/
```

---

## Next Steps

📖 **Read the full CLI Reference:** [CLI-REFERENCE.md](CLI-REFERENCE.md)  
📚 **Explore usage examples:** [USAGE-EXAMPLES.md](USAGE-EXAMPLES.md)  
🏛️ **Understand the architecture:** [ARCHITECTURE_DECISION.md](ARCHITECTURE_DECISION.md)  
💡 **Learn the philosophy:** [CORE_PRINCIPLES.md](CORE_PRINCIPLES.md)

---

## Get Help

- **CLI Help:** `targcc --help`
- **Command Help:** `targcc [command] --help`
- **GitHub Issues:** [Report a bug](https://github.com/doron/TargCC-Core-V2/issues)
- **Discussions:** [Ask a question](https://github.com/doron/TargCC-Core-V2/discussions)

---

**Happy coding! 🚀**

*Built with ❤️ by Doron*
