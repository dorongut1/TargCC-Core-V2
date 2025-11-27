# TargCC CLI Reference

**Version:** 2.0.0-beta.1  
**Last Updated:** 27/11/2025

---

## Table of Contents

- [Overview](#overview)
- [Global Options](#global-options)
- [Commands](#commands)
  - [init](#init)
  - [version](#version)
  - [config](#config)
  - [generate](#generate)
  - [analyze](#analyze)
  - [watch](#watch)
- [Configuration File](#configuration-file)
- [Exit Codes](#exit-codes)

---

## Overview

TargCC CLI is a modern code generation platform that creates complete Clean Architecture applications from database schemas.

```bash
# Basic usage
targcc [command] [options]

# Get help
targcc --help
targcc [command] --help
```

---

## Global Options

These options work with all commands:

| Option | Aliases | Description |
|--------|---------|-------------|
| `--verbose` | `-v` | Enable detailed output |
| `--config <path>` | | Path to configuration file (default: targcc.json) |
| `--no-color` | | Disable colored output |
| `--quiet` | `-q` | Minimal output (errors only) |
| `--help` | `-h`, `-?` | Show help information |

**Example:**
```bash
targcc generate all Customer --verbose
targcc config show --config ./my-config.json
```

---

## Commands

### init

Initialize TargCC in the current directory.

**Syntax:**
```bash
targcc init [options]
```

**Options:**
| Option | Aliases | Description |
|--------|---------|-------------|
| `--force` | `-f` | Force initialization even if already initialized |

**What it does:**
1. Creates `targcc.json` configuration file
2. Sets up default project structure
3. Optionally prompts for database connection settings

**Examples:**
```bash
# Initialize with interactive prompts
targcc init

# Force re-initialization
targcc init --force
```

**Output:**
```
Initializing TargCC

✓ Configuration created successfully
Config file: C:\MyProject\targcc.json

Would you like to configure database connection now? (Y/n)
```

---

### version

Show version and system information.

**Syntax:**
```bash
targcc version
```

**Output:**
```
TargCC Core V2 - Version Information

Version: 2.0.0-beta.1
File Version: 2.0.0.0
Runtime: 9.0.0
Platform: Microsoft Windows NT 10.0.22631.0
```

---

### config

Manage TargCC configuration.

#### config show

Display current configuration.

**Syntax:**
```bash
targcc config show
```

**Example Output:**
```
Current Configuration

Connection String: Server=localhost;Database=MyDb;...
Output Directory: C:\MyProject\Generated
Default Namespace: MyApp
Clean Architecture: True
Generate CQRS: True
Generate API Controllers: True
Generate Repositories: True
Generate Stored Procedures: True
Use Dapper: True
Generate Validators: True
Log Level: Information
Verbose: False

Config file: C:\MyProject\targcc.json
```

#### config set

Set a configuration value.

**Syntax:**
```bash
targcc config set <key> <value>
```

**Examples:**
```bash
# Set connection string
targcc config set ConnectionString "Server=localhost;Database=MyDb;..."

# Set output directory
targcc config set OutputDirectory "C:\Output"

# Set default namespace
targcc config set DefaultNamespace "MyCompany.MyApp"

# Enable/disable features
targcc config set GenerateCqrs true
targcc config set UseCleanArchitecture true
targcc config set Verbose true
```

**Available Keys:**
- `ConnectionString` - Database connection string
- `OutputDirectory` - Where to generate files
- `DefaultNamespace` - Base namespace for generated code
- `UseCleanArchitecture` - Enable Clean Architecture (true/false)
- `GenerateCqrs` - Generate CQRS handlers (true/false)
- `GenerateApiControllers` - Generate API controllers (true/false)
- `GenerateRepositories` - Generate repository pattern (true/false)
- `GenerateStoredProcedures` - Generate SQL stored procedures (true/false)
- `UseDapper` - Use Dapper for data access (true/false)
- `GenerateValidators` - Generate FluentValidation validators (true/false)
- `LogLevel` - Logging level (Trace/Debug/Information/Warning/Error/Critical)
- `Verbose` - Enable verbose output (true/false)

#### config reset

Reset configuration to defaults.

**Syntax:**
```bash
targcc config reset
```

**Interactive confirmation:**
```
Are you sure you want to reset configuration? (y/N)
```

---

### generate

Generate code from database tables.

**Syntax:**
```bash
targcc generate <subcommand> [arguments] [options]
```

**Subcommands:**
- `entity` - Generate entity class
- `sql` - Generate SQL stored procedures
- `repo` - Generate repository interface and implementation
- `cqrs` - Generate CQRS queries and commands
- `api` - Generate API controller
- `all` - Generate everything for a table
- `project` - Generate complete project from database

**Common Options:**

| Option | Aliases | Description |
|--------|---------|-------------|
| `--connection` | `-c` | Database connection string |
| `--output` | `-o` | Output directory |
| `--namespace` | `-n` | Namespace for generated code |

---

#### generate entity

Generate C# entity class from database table.

**Syntax:**
```bash
targcc generate entity <table> [options]
```

**Arguments:**
- `<table>` - Name of the database table

**Example:**
```bash
targcc generate entity Customer

# With custom output
targcc generate entity Customer --output "./Domain/Entities" --namespace "MyApp.Domain"
```

**Generated Files:**
```
Domain/Entities/Customer.cs
```

**What it generates:**
- Entity class with properties mapped from table columns
- Navigation properties for foreign keys
- Handles special column prefixes (eno_, ent_, lkp_, etc.)
- XML documentation comments

---

#### generate sql

Generate SQL stored procedures for a table.

**Syntax:**
```bash
targcc generate sql <table> [options]
```

**Arguments:**
- `<table>` - Name of the database table

**Example:**
```bash
targcc generate sql Customer
```

**Generated Files:**
```
SQL/StoredProcedures/Customer_GetByID.sql
SQL/StoredProcedures/Customer_GetAll.sql
SQL/StoredProcedures/Customer_Insert.sql
SQL/StoredProcedures/Customer_Update.sql
SQL/StoredProcedures/Customer_Delete.sql
SQL/StoredProcedures/Customer_Search.sql
```

**What it generates:**
- GetByID - Retrieve single record by primary key
- GetAll - Retrieve all records (with paging)
- Insert - Create new record
- Update - Update existing record
- Delete - Soft or hard delete
- Search - Full-text search procedure

---

#### generate repo

Generate repository pattern for a table.

**Syntax:**
```bash
targcc generate repo <table> [options]
```

**Arguments:**
- `<table>` - Name of the database table

**Example:**
```bash
targcc generate repo Customer
```

**Generated Files:**
```
Application/Interfaces/ICustomerRepository.cs
Infrastructure/Repositories/CustomerRepository.cs
```

**What it generates:**
- Repository interface with CRUD operations
- Repository implementation using Dapper or EF Core
- Async methods for all operations
- XML documentation

---

#### generate cqrs

Generate CQRS queries and commands for a table.

**Syntax:**
```bash
targcc generate cqrs <table> [options]
```

**Arguments:**
- `<table>` - Name of the database table

**Example:**
```bash
targcc generate cqrs Customer
```

**Generated Files:**
```
Application/Customers/Queries/GetCustomerById/GetCustomerByIdQuery.cs
Application/Customers/Queries/GetCustomerById/GetCustomerByIdQueryHandler.cs
Application/Customers/Queries/GetAllCustomers/GetAllCustomersQuery.cs
Application/Customers/Queries/GetAllCustomers/GetAllCustomersQueryHandler.cs
Application/Customers/Commands/CreateCustomer/CreateCustomerCommand.cs
Application/Customers/Commands/CreateCustomer/CreateCustomerCommandHandler.cs
Application/Customers/Commands/UpdateCustomer/UpdateCustomerCommand.cs
Application/Customers/Commands/UpdateCustomer/UpdateCustomerCommandHandler.cs
Application/Customers/Commands/DeleteCustomer/DeleteCustomerCommand.cs
Application/Customers/Commands/DeleteCustomer/DeleteCustomerCommandHandler.cs
Application/Customers/Validators/CreateCustomerValidator.cs
Application/Customers/Validators/UpdateCustomerValidator.cs
Application/Customers/DTOs/CustomerDto.cs
```

**What it generates:**
- Queries with MediatR handlers (GetById, GetAll)
- Commands with MediatR handlers (Create, Update, Delete)
- FluentValidation validators
- DTOs for data transfer
- XML documentation

---

#### generate api

Generate REST API controller for a table.

**Syntax:**
```bash
targcc generate api <table> [options]
```

**Arguments:**
- `<table>` - Name of the database table

**Example:**
```bash
targcc generate api Customer
```

**Generated Files:**
```
API/Controllers/CustomersController.cs
```

**What it generates:**
- REST API controller with CRUD endpoints
- Swagger/OpenAPI documentation
- HTTP status codes and error handling
- Route attributes
- XML documentation

**Endpoints:**
```
GET    /api/customers         - Get all customers
GET    /api/customers/{id}    - Get customer by ID
POST   /api/customers         - Create new customer
PUT    /api/customers/{id}    - Update customer
DELETE /api/customers/{id}    - Delete customer
```

---

#### generate all

Generate complete stack for a table (entity + SQL + repo + CQRS + API).

**Syntax:**
```bash
targcc generate all <table> [options]
```

**Arguments:**
- `<table>` - Name of the database table

**Example:**
```bash
targcc generate all Customer
```

**Output:**
```
Generate All: Customer

✓ Generated 20 file(s) in 2.1s

  Entity:
    ✓ Customer.cs (45 lines)
  SQL:
    ✓ Customer_GetByID.sql (18 lines)
    ✓ Customer_GetAll.sql (12 lines)
    ✓ Customer_Insert.sql (23 lines)
    ✓ Customer_Update.sql (25 lines)
    ✓ Customer_Delete.sql (10 lines)
    ✓ Customer_Search.sql (15 lines)
  Repository:
    ✓ ICustomerRepository.cs (12 lines)
    ✓ CustomerRepository.cs (85 lines)
  CQRS:
    ✓ GetCustomerByIdQuery.cs (8 lines)
    ✓ GetCustomerByIdQueryHandler.cs (22 lines)
    ... (10 CQRS files)
  API:
    ✓ CustomersController.cs (120 lines)

Output directory: C:\MyProject\Generated
```

**This is the most commonly used command** - it generates everything you need for a complete table.

---

#### generate project

Generate complete Clean Architecture solution from database.

**Syntax:**
```bash
targcc generate project [options]
```

**Options:**
| Option | Aliases | Description |
|--------|---------|-------------|
| `--connection` | `-c` | Database connection string (required) |
| `--output` | `-o` | Output directory |
| `--namespace` | `-n` | Base namespace for the project |
| `--name` | | Project name (default: database name) |
| `--architecture` | | Architecture type (CleanArchitecture/ThreeTier/MinimalApi) |

**Example:**
```bash
targcc generate project --connection "Server=localhost;Database=Northwind;..." --name "Northwind"
```

**Output:**
```
Generating Clean Architecture Project...

✓ Creating solution structure...
  ✓ Northwind.sln
  ✓ src/Northwind.Domain/
  ✓ src/Northwind.Application/
  ✓ src/Northwind.Infrastructure/
  ✓ src/Northwind.API/
  ✓ tests/Northwind.Tests/

✓ Generating from 12 tables...
  ✓ Customer (20 files)
  ✓ Order (20 files)
  ✓ OrderDetail (20 files)
  ✓ Product (20 files)
  ✓ Category (20 files)
  ✓ Supplier (20 files)
  ✓ Employee (20 files)
  ✓ Shipper (20 files)
  ✓ Region (20 files)
  ✓ Territory (20 files)
  ✓ CustomerCustomerDemo (20 files)
  ✓ CustomerDemographic (20 files)

✓ Generating infrastructure...
  ✓ Program.cs
  ✓ appsettings.json
  ✓ Startup.cs
  ✓ DbContext.cs
  ✓ DependencyInjection.cs

✓ Project generated: 248 files in 12.5s

Next steps:
1. cd Northwind
2. dotnet restore
3. dotnet build
4. dotnet run --project src/Northwind.API
```

**Project Structure:**
```
Northwind/
├── Northwind.sln
├── src/
│   ├── Northwind.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   └── Interfaces/
│   ├── Northwind.Application/
│   │   ├── Customers/
│   │   ├── Orders/
│   │   ├── Common/
│   │   └── DependencyInjection.cs
│   ├── Northwind.Infrastructure/
│   │   ├── Repositories/
│   │   ├── Data/
│   │   └── DependencyInjection.cs
│   └── Northwind.API/
│       ├── Controllers/
│       ├── Program.cs
│       └── appsettings.json
└── tests/
    └── Northwind.Tests/
```

---

### analyze

Analyze database schema and code quality.

**Syntax:**
```bash
targcc analyze <subcommand> [options]
```

**Subcommands:**
- `schema` - Analyze database schema
- `impact` - Analyze impact of schema changes
- `security` - Analyze security issues
- `quality` - Analyze code quality and naming conventions

**Common Options:**

| Option | Aliases | Description |
|--------|---------|-------------|
| `--connection` | `-c` | Database connection string |
| `--output` | `-o` | Output file for report |
| `--format` | | Output format (text/json/html) |

---

#### analyze schema

Analyze and display database schema.

**Syntax:**
```bash
targcc analyze schema [options]
```

**Options:**
| Option | Aliases | Description |
|--------|---------|-------------|
| `--table` | `-t` | Analyze specific table only |
| `--format` | | Output format (table/tree/json) |

**Example:**
```bash
# Analyze entire database
targcc analyze schema

# Analyze specific table
targcc analyze schema --table Customer

# Export to JSON
targcc analyze schema --format json --output schema.json
```

**Output:**
```
Database Schema Analysis

Tables: 12
Total Columns: 145
Foreign Keys: 18
Indexes: 34

Tables:
┌──────────────────┬─────────┬───────────┬──────────┐
│ Table            │ Columns │ Relations │ Indexes  │
├──────────────────┼─────────┼───────────┼──────────┤
│ Customer         │ 12      │ 2         │ 3        │
│ Order            │ 15      │ 3         │ 4        │
│ OrderDetail      │ 8       │ 2         │ 2        │
│ Product          │ 10      │ 1         │ 2        │
│ ...              │ ...     │ ...       │ ...      │
└──────────────────┴─────────┴───────────┴──────────┘

Special Prefixes Detected:
  eno_ (Hashed): 3 columns
  ent_ (Encrypted): 2 columns
  lkp_ (Lookup): 15 columns
  agg_ (Aggregate): 5 columns
```

---

#### analyze impact

Analyze impact of schema changes on generated code.

**Syntax:**
```bash
targcc analyze impact --table <table> --change <change-description> [options]
```

**Options:**
| Option | Aliases | Description |
|--------|---------|-------------|
| `--table` | `-t` | Table name (required) |
| `--change` | | Change description (required) |
| `--column` | | Affected column name |
| `--new-type` | | New column type |

**Example:**
```bash
# Analyze impact of changing column type
targcc analyze impact --table Customer --column Email --new-type nvarchar(500)

# Analyze impact of adding new column
targcc analyze impact --table Customer --change "Add PhoneNumber column"
```

**Output:**
```
Impact Analysis: Customer

Change: Email type nvarchar(100) → nvarchar(500)

Affected Files: 8

✓ Auto-updated (5 files):
  - Domain/Entities/Customer.cs
  - SQL/Customer_Insert.sql
  - SQL/Customer_Update.sql
  - Application/DTOs/CustomerDto.cs
  - Infrastructure/Repositories/CustomerRepository.cs

⚠️  Manual review required (3 files):
  - Application/Customers/Commands/CreateCustomer/CreateCustomerCommand.prt.cs
  - Application/Customers/Validators/CreateCustomerValidator.prt.cs
  - API/Controllers/CustomersController.prt.cs

Estimated fix time: 15-20 minutes

Build errors expected: 3 (intentional safety net)

Next steps:
1. Run: targcc generate all Customer
2. Build project and review errors
3. Update manual code (*.prt.cs files)
4. Rebuild and test
```

---

#### analyze security

Analyze security issues in database schema.

**Syntax:**
```bash
targcc analyze security [options]
```

**Options:**
| Option | Aliases | Description |
|--------|---------|-------------|
| `--table` | `-t` | Analyze specific table only |
| `--severity` | | Minimum severity (Low/Medium/High/Critical) |

**Example:**
```bash
targcc analyze security
```

**Output:**
```
Security Analysis

Critical Issues: 2
High Issues: 5
Medium Issues: 8
Low Issues: 3

Critical:
  ⚠️  Customer.CreditCard - Not encrypted (ent_ prefix missing)
  ⚠️  User.Password - Not hashed (eno_ prefix missing)

High:
  ⚠️  Customer.Email - Should use ent_ for PII data
  ⚠️  Order.PaymentInfo - Contains sensitive data without encryption
  ⚠️  Employee.SSN - Social security number not encrypted
  ⚠️  Employee.Salary - Sensitive financial data exposed
  ⚠️  User.SecurityQuestion - Should be hashed

Medium:
  ⚠️  Customer.Phone - PII data without protection
  ⚠️  Employee.Address - PII data without protection
  ... (6 more)

Recommendations:
1. Add ent_ prefix to CreditCard column for encryption
2. Add eno_ prefix to Password column for hashing
3. Review PII columns and apply appropriate prefixes
4. Consider adding audit columns (CreatedBy, ModifiedBy)
```

---

#### analyze quality

Analyze code quality and naming conventions.

**Syntax:**
```bash
targcc analyze quality [options]
```

**Options:**
| Option | Aliases | Description |
|--------|---------|-------------|
| `--table` | `-t` | Analyze specific table only |
| `--check` | | Specific check (naming/relationships/indexes/all) |

**Example:**
```bash
targcc analyze quality
```

**Output:**
```
Code Quality Analysis

Overall Score: B+ (87/100)

Naming Conventions: A (95/100)
  ✓ All tables use PascalCase
  ✓ All columns use PascalCase
  ⚠️  2 tables use abbreviations
  ✗ 1 column uses underscore

Relationships: B (85/100)
  ✓ 15 foreign keys properly defined
  ⚠️  3 missing foreign keys detected
  ⚠️  2 circular dependencies

Indexes: B+ (88/100)
  ✓ All primary keys indexed
  ✓ All foreign keys indexed
  ⚠️  5 columns could benefit from indexes

Recommendations:
1. Add foreign key: Order.CustomerID → Customer.ID
2. Add index on Customer.Email (frequently queried)
3. Rename column: user_name → UserName
4. Review circular dependency: Order ↔ OrderDetail
```

---

### watch

Watch database for schema changes and auto-regenerate affected files.

**Syntax:**
```bash
targcc watch [options]
```

**Options:**
| Option | Aliases | Description |
|--------|---------|-------------|
| `--interval` | `-i` | Check interval in seconds (default: 5) |
| `--no-auto-generate` | | Only detect changes, don't regenerate |
| `--tables` | `-t` | Watch only specific tables (comma-separated) |

**Example:**
```bash
# Watch all tables
targcc watch

# Watch with custom interval
targcc watch --interval 10

# Watch specific tables only
targcc watch --tables Customer,Order,Product

# Only detect, don't regenerate
targcc watch --no-auto-generate
```

**Output:**
```
TargCC Watch Mode

Watching database: Northwind
Check interval: 5 seconds
Auto-generate: Enabled

Press Ctrl+C to stop

[12:00:00] Checking for changes...
[12:00:00] ✓ No changes detected

[12:00:05] Checking for changes...
[12:00:05] ⚠️  Change detected in Customer table
[12:00:05]   - Column added: PhoneNumber (varchar(20))
[12:00:05] 🔄 Regenerating affected files...
[12:00:06] ✓ Generated 8 files in 1.2s
[12:00:06]   - Domain/Entities/Customer.cs
[12:00:06]   - SQL/Customer_Insert.sql
[12:00:06]   - SQL/Customer_Update.sql
[12:00:06]   - Application/DTOs/CustomerDto.cs
[12:00:06]   - Infrastructure/Repositories/CustomerRepository.cs
[12:00:06]   - Application/Customers/Commands/CreateCustomer/CreateCustomerCommand.cs
[12:00:06]   - Application/Customers/Commands/UpdateCustomer/UpdateCustomerCommand.cs
[12:00:06]   - Application/Customers/Validators/CreateCustomerValidator.cs

[12:00:11] Checking for changes...
[12:00:11] ✓ No changes detected
```

**What it does:**
1. Takes snapshot of current database schema
2. Periodically checks for changes
3. When changes detected:
   - Identifies affected tables and columns
   - Regenerates only affected files
   - Preserves manual code in *.prt.cs files
4. Logs all changes to `~/.targcc/logs/watch.log`

**Best Practices:**
- Run in development environment only
- Use source control to review generated changes
- Keep *.prt.cs files in separate commits

---

## Configuration File

The `targcc.json` configuration file controls all generation behavior.

**Location:**
- Default: `./targcc.json` (current directory)
- Custom: Use `--config` option

**Example:**
```json
{
  "ConnectionString": "Server=localhost;Database=MyDb;Trusted_Connection=true;",
  "OutputDirectory": "C:\\MyProject\\Generated",
  "DefaultNamespace": "MyCompany.MyApp",
  "UseCleanArchitecture": true,
  "GenerateCqrs": true,
  "GenerateApiControllers": true,
  "GenerateRepositories": true,
  "GenerateStoredProcedures": true,
  "UseDapper": true,
  "GenerateValidators": true,
  "LogLevel": "Information",
  "Verbose": false,
  "PrefixHandling": {
    "eno_": "Hashed",
    "ent_": "Encrypted",
    "lkp_": "Lookup",
    "enm_": "Enum",
    "loc_": "Localized",
    "clc_": "Calculated",
    "blg_": "BusinessLogic",
    "agg_": "Aggregate",
    "spt_": "SeparateUpdate",
    "upl_": "Upload",
    "scb_": "SeparateChangedBy",
    "spl_": "DelimitedList"
  }
}
```

**Field Descriptions:**

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `ConnectionString` | string | null | SQL Server connection string |
| `OutputDirectory` | string | Current dir | Where to generate files |
| `DefaultNamespace` | string | "MyApp" | Base namespace for code |
| `UseCleanArchitecture` | bool | true | Use Clean Architecture structure |
| `GenerateCqrs` | bool | true | Generate CQRS handlers |
| `GenerateApiControllers` | bool | true | Generate API controllers |
| `GenerateRepositories` | bool | true | Generate repository pattern |
| `GenerateStoredProcedures` | bool | true | Generate SQL stored procedures |
| `UseDapper` | bool | true | Use Dapper (vs EF Core) |
| `GenerateValidators` | bool | true | Generate FluentValidation validators |
| `LogLevel` | string | "Information" | Logging level |
| `Verbose` | bool | false | Enable verbose output |

---

## Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | General error |
| 2 | Configuration error |
| 3 | Database connection error |
| 4 | Generation error |
| 5 | Validation error |

---

## Tips & Best Practices

### 1. Use Watch Mode During Development
```bash
# In one terminal
targcc watch

# In another terminal, make database changes
# Watch mode auto-regenerates affected files
```

### 2. Generate Project Once, Then Use "generate all"
```bash
# First time - generate entire project
targcc generate project

# Later - regenerate specific tables
targcc generate all Customer
targcc generate all Order
```

### 3. Review Impact Before Changes
```bash
# Before changing schema
targcc analyze impact --table Customer --column Email --new-type nvarchar(500)

# Make the change in database
# Then regenerate
targcc generate all Customer
```

### 4. Use Quality Analysis Regularly
```bash
# Weekly check
targcc analyze quality > quality-report.txt
targcc analyze security > security-report.txt
```

### 5. Protect Manual Code
- Always put custom logic in `*.prt.cs` files
- Generated files (`*.cs`) are ALWAYS overwritten
- Manual files (`*.prt.cs`) are NEVER touched

---

## Common Workflows

### Workflow 1: New Project
```bash
# 1. Initialize
targcc init

# 2. Configure
targcc config set ConnectionString "Server=..."

# 3. Generate project
targcc generate project

# 4. Build and run
cd MyProject
dotnet build
dotnet run --project src/MyProject.API
```

### Workflow 2: Add New Table
```bash
# 1. Create table in database

# 2. Generate all code
targcc generate all NewTable

# 3. Build and test
dotnet build
```

### Workflow 3: Modify Existing Table
```bash
# 1. Check impact first
targcc analyze impact --table Customer --change "Add PhoneNumber"

# 2. Make database change

# 3. Regenerate
targcc generate all Customer

# 4. Review build errors (expected!)
dotnet build

# 5. Fix manual code (*.prt.cs files)

# 6. Rebuild
dotnet build
```

---

**For more information:**
- [Quickstart Guide](QUICKSTART.md)
- [Usage Examples](USAGE-EXAMPLES.md)
- [Project README](../README.md)
- [Core Principles](CORE_PRINCIPLES.md)

**Last Updated:** 27/11/2025  
**Version:** 2.0.0-beta.1
