# TargCC Core V2 🚀

**Modern Code Generation Platform - Clean Architecture Edition**

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![.NET Version](https://img.shields.io/badge/.NET-9.0-blue)]()
[![Phase 1](https://img.shields.io/badge/Phase%201-100%25-brightgreen)]()
[![Phase 1.5](https://img.shields.io/badge/Phase%201.5-100%25-brightgreen)]()
[![Phase 3A](https://img.shields.io/badge/Phase%203A-100%25-brightgreen)]()
[![Phase 3B](https://img.shields.io/badge/Phase%203B-100%25-brightgreen)]()
[![Phase 3C](https://img.shields.io/badge/Phase%203C-95%25-brightgreen)]()
[![Phase 3E](https://img.shields.io/badge/Phase%203E-100%25-brightgreen)]()
[![Phase 3F](https://img.shields.io/badge/Phase%203F-Planning-yellow)]()
[![Backend Tests](https://img.shields.io/badge/backend%20tests-727%20passing-success)]()
[![Frontend Tests](https://img.shields.io/badge/frontend%20tests-403%20passing-success)]()
[![Coverage](https://img.shields.io/badge/coverage-95%25-brightgreen)]()
[![License](https://img.shields.io/badge/license-MIT-green)]()

---

## 🌟 What is TargCC 2.0?

TargCC is a **next-generation code generation platform** that creates complete, production-ready applications from database schemas using **Clean Architecture** and modern best practices.

**Key Features:**
- ⚡ **Incremental Generation** - Only what changed
- 🛡️ **Build Errors as Safety Net** - Intentional, not bugs!
- 🏛️ **Clean Architecture** - 5 layers, SOLID principles
- 🖥️ **Professional CLI** - 16 commands for everything
- ⏱️ **Watch Mode** - Auto-regenerate on schema changes
- 📊 **Impact Analysis** - Know what will break before it breaks
- 🔒 **Security Scanning** - Find unencrypted sensitive data
- 🎨 **React UI Generator** - Auto-generate forms, grids, types, hooks ✅ **NEW!**
- 🌐 **Web Dashboard** - Modern React-based management interface (95%)
- 🤖 **AI Code Editor** - Modify generated code with natural language 🚧 **COMING SOON!**

---

## 🚀 Quick Start (5 Minutes)

### From Zero to Running API

```bash
# 1. Initialize
mkdir MyApp && cd MyApp
targcc init

# 2. Configure
targcc config set ConnectionString "Server=localhost;Database=MyDb;Trusted_Connection=true;"

# 3. Generate complete project
targcc generate project

# 4. Run!
dotnet run --project src/MyApp.API
```

**Open browser:** `https://localhost:5001/swagger` 🎉

**Read the full guide:** [QUICKSTART.md](docs/current/QUICKSTART.md)

---

## 🔑 Core Philosophy

> **"Incremental Generation + Mandatory Manual Review"**

The system generates code intelligently and quickly, but **you're always in control**.

### 💡 Build Errors = Good!

```
Changed CustomerID from string to int?

✅ Auto-generated files updated automatically
⚠️  3 Build Errors in manual code (*.prt files)

→ This is exactly what we want!
   Now you know exactly where to update your custom logic.
```

**Read more:** [Core Principles](docs/current/CORE_PRINCIPLES.md)

---

## 🖥️ CLI Commands

TargCC 2.0 is a **command-line first** tool with 16 powerful commands:

### 🎯 Core Commands
```bash
# Initialize project
targcc init

# Manage configuration
targcc config show
targcc config set ConnectionString "..."

# Show version
targcc version
```

### 🏗️ Generation Commands
```bash
# Generate everything for a table (most common)
targcc generate all Customer

# Generate specific components
targcc generate entity Customer      # Entity class
targcc generate sql Customer         # SQL stored procedures
targcc generate repo Customer        # Repository pattern
targcc generate cqrs Customer        # CQRS handlers
targcc generate api Customer         # REST API controller

# Generate complete project from database
targcc generate project
```

### 📊 Analysis Commands
```bash
# Analyze database schema
targcc analyze schema

# Check impact of schema changes
targcc analyze impact --table Customer --change "Add PhoneNumber"

# Security vulnerability scan
targcc analyze security

# Code quality check
targcc analyze quality
```

### ⏱️ Watch Mode
```bash
# Auto-regenerate on schema changes
targcc watch
```

**Complete reference:** [CLI-REFERENCE.md](docs/CLI-REFERENCE.md)

---

## 🏗️ What Does It Generate?

### From a Database Schema → Complete Application

```
Database (SQL Server)
        ↓
    Analyzers
        ↓
   DatabaseSchema
        ↓
    Generators
        ↓
┌─────────────────────────────────┐
│  Modern Clean Architecture:    │
│                                 │
│  1. Domain Layer               │
│     └── Entities + Interfaces  │
│                                 │
│  2. Application Layer          │
│     └── CQRS (Queries/Commands)│
│                                 │
│  3. Infrastructure Layer       │
│     └── Repositories + Data    │
│                                 │
│  4. API Layer                  │
│     └── REST Controllers       │
│                                 │
│  5. Tests Layer                │
│     └── Unit + Integration     │
└─────────────────────────────────┘
```

### Example: Generate Complete Stack for "Customer" Table

```bash
$ targcc generate all Customer

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
    ✓ GetAllCustomersQuery.cs (6 lines)
    ✓ GetAllCustomersQueryHandler.cs (18 lines)
    ✓ CreateCustomerCommand.cs (10 lines)
    ✓ CreateCustomerCommandHandler.cs (28 lines)
    ✓ UpdateCustomerCommand.cs (12 lines)
    ✓ UpdateCustomerCommandHandler.cs (30 lines)
    ✓ DeleteCustomerCommand.cs (6 lines)
    ✓ DeleteCustomerCommandHandler.cs (15 lines)
  API:
    ✓ CustomersController.cs (120 lines)

Output directory: C:\MyProject\Generated
```

---

## 📊 Project Status

### ✅ Phase 1: Core Engine (100% Complete)

- ✅ DatabaseAnalyzer - Full DB analysis
- ✅ TableAnalyzer - Tables + Indexes
- ✅ ColumnAnalyzer - Columns + Types + Prefixes
- ✅ RelationshipAnalyzer - Foreign Keys
- ✅ Plugin System - Modular architecture
- ✅ Configuration Manager - JSON + Encryption
- ✅ Code Quality Tools - StyleCop, SonarQube
- ✅ Testing Framework - 63 tests, 80%+ coverage

### ✅ Phase 1.5: MVP Generators (100% Complete)

- ✅ SQL Generator - Stored Procedures (6 types)
- ✅ Entity Generator - C# Classes
- ✅ Type Mapper - SQL → C# types (44 tests)
- ✅ Prefix Handler - 12 prefixes (36 tests)
- ✅ Property Generator - C# properties (22 tests)
- ✅ Method Generator - Constructors, ToString, etc. (33 tests)
- ✅ Relationship Generator - Navigation properties (17 tests)
- ✅ File Writer - With *.prt protection

### ✅ Phase 3A: CLI Core (100% Complete) 🎉

**Just completed!** Professional command-line interface with:

- ✅ **16 CLI Commands** - init, config, generate, analyze, watch
- ✅ **Project Generation** - Complete solution from database
- ✅ **Watch Mode** - Auto-regenerate on schema changes
- ✅ **Impact Analysis** - Know what breaks before it breaks
- ✅ **Security Scanning** - Find unencrypted sensitive data
- ✅ **Quality Analysis** - Naming conventions, relationships
- ✅ **145 Tests** (207% of target)
- ✅ **~95% Code Coverage** (exceeds 85% target)
- ✅ **Comprehensive Documentation** - CLI Reference, Quickstart

**What's New in 3A:**
- 🆕 `targcc generate project` - Complete project generation
- 🆕 `targcc watch` - Auto-regenerate on changes
- 🆕 `targcc analyze impact` - Impact assessment
- 🆕 `targcc analyze security` - Security scanning
- 🆕 `targcc analyze quality` - Quality metrics

### ✅ Phase 3E: React UI Generators (100% Complete) 🎉 **NEW!**

**Just completed!** Automatic React component generation:

- ✅ **TypeScript Type Generator** - Auto-generate types, interfaces, enums
- ✅ **React API Generator** - API client functions
- ✅ **React Hook Generator** - React Query hooks
- ✅ **Form Component Generator** - Entity forms with validation
- ✅ **Grid Component Generator** - DataGrid with sorting/filtering
- ✅ **Page Generator** - Complete pages with navigation
- ✅ **Supports all 12 prefix types** (eno_, ent_, lkp_, enm_, etc.)

**Output per table:** ~900-1000 lines of production-ready React code!

**What's New in 3E:**
- 🆕 Generate complete React UI from database schema
- 🆕 Material-UI components with Formik validation
- 🆕 Automatic foreign key ComboBoxes
- 🆕 Parent/child relationship panels
- 🆕 TypeScript types with strict mode

### 📋 Phase 3C: Local Web UI (95% Complete)

- [x] React + TypeScript interface
- [x] WebAPI backend with RESTful endpoints
- [x] Connection management system (full CRUD)
- [x] Schema browsing and table listing
- [x] Dashboard with statistics widgets
- [x] Generation history tracking (backend complete)
- [x] Code generation integration
- [ ] Code preview modal
- [ ] Batch generation UI
- [ ] Download generated files

**Current:** Core functionality complete, polish and final features remaining

### 🚧 Phase 3F: AI-Powered Code Editor (Planning - Next Phase!)

**The future is here!** Modify generated code using natural language:

- 🤖 **AI-Powered Editing** - "Make the save button blue, move email field left"
- 👁️ **Live Preview** - See changes in real-time
- 🔄 **Version Control** - Undo/Redo, compare versions
- ✅ **Smart Validation** - Prevents breaking changes
- 🎨 **Monaco Editor** - Professional code editing experience
- 💬 **Context-Aware AI** - Understands schema, relationships, conventions

**What You'll Be Able To Do:**
```
1. Generate CustomerForm.tsx (auto-generated)
   ↓
2. Open in AI Editor
   ↓
3. You: "Move Email field to the left, make Save button blue"
   ↓
4. AI modifies code + shows live preview
   ↓
5. You approve → Saved!
```

**Status:**
- [x] Specification complete
- [x] Architecture designed
- [ ] Backend service (12h)
- [ ] Frontend components (8h)
- [ ] Testing & polish (4h)

**Read the full spec:** [SPEC_AI_CODE_EDITOR.md](docs/SPEC_AI_CODE_EDITOR.md)

### 📋 Phase 3D: Migration & Polish (Planned - Q1 2026)

- [ ] Migration tool (VB.NET → C#)
- [ ] Git integration (auto-commit)
- [ ] Performance optimization
- [ ] Multi-database support (PostgreSQL, MySQL)
- [ ] Final bug fixes and documentation

**Target GA:** v2.0.0 (Q1 2026)

---

## 🎯 Supported Prefixes

TargCC recognizes **12 special column prefixes** for advanced behavior:

| Prefix | Purpose | Example | Generated Code |
|--------|---------|---------|----------------|
| `eno_` | **Hashed** (one-way) | `eno_Password` | `PasswordHashed` (private setter) |
| `ent_` | **Encrypted** (two-way) | `ent_CreditCard` | Encrypt/Decrypt property |
| `lkp_` | **Lookup** | `lkp_Status` | `StatusCode` + `StatusText` |
| `enm_` | **Enum** | `enm_Type` | `CustomerType` enum |
| `loc_` | **Localized** | `loc_Name` | `NameLocalized` |
| `clc_` | **Calculated** | `clc_Total` | Read-only property |
| `blg_` | **Business Logic** | `blg_Discount` | Server-calculated |
| `agg_` | **Aggregate** | `agg_OrderCount` | Aggregate property |
| `spt_` | **Separate Update** | `spt_Notes` | Separate SP |
| `upl_` | **Upload** | `upl_Photo` | File upload support |
| `scb_` | **Separate Changed By** | `scb_ApprovedBy` | Audit field |
| `spl_` | **Delimited List** | `spl_Tags` | CSV support |

**Example:**

```sql
-- Database
CREATE TABLE Customer (
    ID INT PRIMARY KEY,
    Name NVARCHAR(100),
    eno_Password VARCHAR(64),    -- Hashed
    ent_CreditCard VARCHAR(MAX),  -- Encrypted
    lkp_Status VARCHAR(10),       -- Lookup
    agg_OrderCount INT            -- Aggregate
);
```

```csharp
// Generated Entity
public class Customer : BaseEntity
{
    public int ID { get; set; }
    public string Name { get; set; }
    
    // eno_ → Hashed (private setter)
    public string PasswordHashed { get; private set; }
    public void SetPassword(string plainText) { ... }
    
    // ent_ → Encrypted
    public string CreditCard { get; set; } // Auto encrypt/decrypt
    
    // lkp_ → Lookup
    public string StatusCode { get; set; }
    public string StatusText { get; set; }
    
    // agg_ → Aggregate
    public int OrderCountAggregate { get; private set; }
    public void UpdateAggregates(int count) { ... }
}
```

---

## 🏛️ Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────┐
│              API Layer                  │  ← REST Controllers
│         (ASP.NET Core)                  │
├─────────────────────────────────────────┤
│         Application Layer               │  ← CQRS (Use Cases)
│      (MediatR + Handlers)               │
├─────────────────────────────────────────┤
│       Infrastructure Layer              │  ← Repositories + Data
│    (EF Core + Dapper + SQL)             │
├─────────────────────────────────────────┤
│           Domain Layer                  │  ← Entities + Interfaces
│      (Pure Business Logic)              │
└─────────────────────────────────────────┘

Dependencies flow inward: API → Application → Domain
Infrastructure depends on Domain only
```

**Read more:** [Architecture Decision](docs/ARCHITECTURE_DECISION.md)

---

## 📖 Usage Examples

### Example 1: Generate Complete Project

```bash
# From database to running API in 30 seconds
targcc init
targcc config set ConnectionString "Server=localhost;Database=Northwind;..."
targcc generate project

cd Northwind
dotnet run --project src/Northwind.API

# Open browser: https://localhost:5001/swagger
```

### Example 2: Add New Table

```bash
# After adding a table in SQL Server Management Studio
targcc generate all NewTable

# Build and test
dotnet build
```

### Example 3: Modify Existing Table

```bash
# Check impact first
targcc analyze impact --table Customer --change "Change Email type to nvarchar(500)"

# Make database change
# Then regenerate
targcc generate all Customer

# Review build errors (intentional!)
dotnet build

# Fix manual code in *.prt.cs files
# Rebuild
dotnet build
```

### Example 4: Watch Mode During Development

```bash
# In one terminal
targcc watch

# In another terminal, modify your database
# Files automatically regenerate!
```

**More examples:** [USAGE-EXAMPLES.md](docs/current/USAGE-EXAMPLES.md)

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test project
dotnet test src/tests/TargCC.CLI.Tests/

# Run tests by category
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

**Current Stats:**
- **Total Tests:** 1130+ (727 C#, 403 React)
- **Coverage:** 95%+
- **Pass Rate:** 100% (C#), 76% (React - 124 skipped pending library updates)
- **Zero Flaky Tests**

---

## 📚 Documentation

### Quick Start & Guides
- [**Quickstart Guide**](docs/current/QUICKSTART.md) - 5 minutes from zero to running app
- [**CLI Reference**](docs/current/CLI-REFERENCE.md) - Complete command reference
- [**Usage Examples**](docs/current/USAGE-EXAMPLES.md) - Common scenarios

### Architecture & Design
- [Architecture Decision](docs/current/ARCHITECTURE_DECISION.md) - Why Clean Architecture?
- [Core Principles](docs/current/CORE_PRINCIPLES.md) - Build Errors philosophy
- [Project Roadmap](docs/progress/PROJECT_ROADMAP.md) - Complete timeline

### Phase Documentation
- [Phase 3 Checklist](docs/progress/Phase3_Checklist.md) - Detailed phase 3 plan
- [Phase 3 Progress](docs/progress/PHASE3_PROGRESS.md) - Current progress
- [Phase 3 Advanced Features](docs/progress/PHASE3_ADVANCED_FEATURES.md) - Future features

### Technical Reference
- [Entity Generator Spec](docs/ENTITY_GENERATOR_SPEC.md) - Generator details
- [CHANGELOG](CHANGELOG.md) - Version history

---

## 🤝 Contributing

Contributions are welcome! Please read our contributing guidelines first.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📊 Statistics

### Phase 3A Final Numbers

| Metric | Target | Actual | Achievement |
|--------|--------|--------|-------------|
| CLI Commands | 15 | 16 | 107% ✅ |
| Tests Passing | 70+ | 145 | 207% ✅ |
| Code Coverage | 85% | ~95% | 112% ✅ |
| Code Files | ~50 | 96 | 192% ✅ |
| Lines of Code | ~5,000 | ~11,600 | 232% ✅ |

### Cumulative Project Stats

- **Total Code Files:** 96
- **Total Lines of Code:** ~11,600
- **Supported Prefixes:** 12
- **Architecture Patterns:** 3 (Clean, Three-Tier, Minimal API)
- **Generation Modes:** Manual, Watch, Batch

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- **Inspiration:** Original TargCC system (VB.NET)
- **Architecture:** Clean Architecture by Robert C. Martin
- **Patterns:** CQRS by Greg Young
- **Tools:** System.CommandLine, Spectre.Console, MediatR, Dapper, FluentValidation

---

## 📞 Contact & Support

- **Issues:** [GitHub Issues](https://github.com/doron/TargCC-Core-V2/issues)
- **Discussions:** [GitHub Discussions](https://github.com/doron/TargCC-Core-V2/discussions)
- **Email:** support@targcc.com

---

## 🗺️ Roadmap

```
✅ Phase 1: Core Engine (6 weeks) - DONE
✅ Phase 1.5: MVP Generators (2 weeks) - DONE
✅ Phase 3A: CLI Core (2 weeks) - DONE
✅ Phase 3E: React UI Generators (4 weeks) - DONE ⭐
🚧 Phase 3C: Local Web UI (3 weeks) - IN PROGRESS (95%)
🔜 Phase 3F: AI Code Editor (2-3 weeks) - PLANNING ← You are here!
📋 Phase 3D: Migration & Polish (2 weeks) - PLANNED (Q1 2026)
🎯 Phase 4: General Availability (Q1 2026)
```

**Current Progress:** Phase 3F - Planning complete, ready to implement
**Latest:** React UI Generators complete, WebUI operational, AI Code Editor spec ready
**Next:** Implement AI-powered interactive code editor

---

## ⭐ Star History

If you find this project useful, please consider giving it a star! ⭐

---

## 🚀 Get Started Now!

```bash
# 1. Clone
git clone https://github.com/doron/TargCC-Core-V2.git
cd TargCC-Core-V2

# 2. Build
cd src/TargCC.CLI
dotnet build

# 3. Initialize your project
mkdir MyProject
cd MyProject
targcc init

# 4. Follow the quickstart guide
# See: docs/current/QUICKSTART.md
```

---

**Built with ❤️ by Doron**

**Powered by:** C# • .NET 9 • Clean Architecture • CQRS • System.CommandLine
