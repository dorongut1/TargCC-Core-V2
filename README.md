# TargCC Core V2 🚀

**Modern Code Generation Platform - Clean Architecture Edition**

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![.NET Version](https://img.shields.io/badge/.NET-8.0-blue)]()
[![Phase 1](https://img.shields.io/badge/Phase%201-100%25-brightgreen)]()
[![Phase 1.5](https://img.shields.io/badge/Phase%201.5-100%25-brightgreen)]()
[![Tests](https://img.shields.io/badge/tests-205%20passing-success)]()
[![Coverage](https://img.shields.io/badge/coverage-85%25-brightgreen)]()
[![License](https://img.shields.io/badge/license-MIT-green)]()

---

## 🌟 What is TargCC 2.0?

TargCC is a **next-generation code generation platform** that creates complete, production-ready applications from database schemas using **Clean Architecture** and modern best practices.

**Key Features:**
- ⚡ **Incremental Generation** - Only what changed
- 🛡️ **Build Errors as Safety Net** - Intentional, not bugs!
- 🏛️ **Clean Architecture** - 5 layers, SOLID principles
- 🤖 **AI Assistant** - Smart suggestions (Phase 3)
- 🎨 **React UI** - Modern, responsive (Phase 3)
- 📦 **Git Integration** - Built-in version control

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

**Read more:** [Core Principles](CORE_PRINCIPLES.md)

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
│  5. UI Layer (Phase 3)         │
│     └── React + Material-UI    │
└─────────────────────────────────┘
```

### Generated Code Includes:

**Phase 1 ✅ (Complete):**
- ✅ Database Analyzers (4 analyzers)
- ✅ Schema Models (DatabaseSchema, Table, Column, etc.)
- ✅ Plugin System
- ✅ Configuration Management

**Phase 1.5 ✅ (Complete):**
- ✅ **SQL Generators** - Stored Procedures
- ✅ **Entity Generators** - C# Domain Entities
- ✅ **12 Prefix Handlers** (eno_, ent_, lkp_, etc.)
- ✅ **Type Mapping** - SQL → C# types

**Phase 2 🔨 (In Progress):**
- 🔨 **Repository Pattern** - Data access abstraction
- 🔨 **CQRS + MediatR** - Query/Command separation
- 🔨 **REST API** - Modern HTTP endpoints
- 🔨 **Swagger/OpenAPI** - Auto-generated documentation
- 🔨 **FluentValidation** - Declarative validation
- 🔨 **AutoMapper** - DTO mapping

**Phase 3 📋 (Planned):**
- 📋 **React UI** - Modern SPA with Material-UI
- 📋 **AI Assistant** - Smart code suggestions
- 📋 **Migration Tool** - Legacy VB.NET → Modern C#
- 📋 **Smart Error Guide** - Build error analysis

---

## 🚀 Quick Start

### Prerequisites

- **Visual Studio 2022** (17.8+) or **VS Code**
- **.NET 8 SDK**
- **SQL Server** 2019+ (or SQL Server Express)
- **Git**

### Installation

```bash
# 1. Clone the repository
git clone https://github.com/doron/TargCC-Core-V2.git
cd TargCC-Core-V2

# 2. Restore dependencies
dotnet restore

# 3. Build the solution
dotnet build

# 4. Run tests
dotnet test

# 5. Run the generators
cd src/TargCC.Core.Generators
dotnet run -- analyze --connection "your-connection-string"
```

---

## 📖 Usage Example

### Step 1: Analyze Your Database

```csharp
using TargCC.Core.Analyzers;

var analyzer = new DatabaseAnalyzer();
var schema = await analyzer.AnalyzeAsync(connectionString);

// Result: Complete schema with tables, columns, relationships
Console.WriteLine($"Found {schema.Tables.Count} tables");
```

### Step 2: Generate Code

```csharp
using TargCC.Core.Generators;

// Generate Entity
var entityGen = new EntityGenerator();
var customerEntity = await entityGen.GenerateAsync(schema.Tables["Customer"]);
// → Customer.cs (Domain layer)

// Generate Repository
var repoGen = new RepositoryGenerator();
var customerRepo = await repoGen.GenerateAsync(schema.Tables["Customer"]);
// → CustomerRepository.cs (Infrastructure layer)

// Generate API Controller
var apiGen = new ApiControllerGenerator();
var customerController = await apiGen.GenerateAsync(schema.Tables["Customer"]);
// → CustomersController.cs (API layer)
```

### Step 3: Review & Customize

```csharp
// All generated code respects *.prt (partial) files
// Your custom logic in *.prt.cs is NEVER overwritten!

// Example: Customer.prt.cs
public partial class Customer
{
    // Your custom business logic here
    public void ApplyDiscount(decimal percentage)
    {
        // This code is protected!
    }
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

## 📊 Project Status

### Phase 1: Core Engine ✅ (100%)

- ✅ DatabaseAnalyzer - Full DB analysis
- ✅ TableAnalyzer - Tables + Indexes
- ✅ ColumnAnalyzer - Columns + Types + Prefixes
- ✅ RelationshipAnalyzer - Foreign Keys
- ✅ Plugin System - Modular architecture
- ✅ Configuration Manager - JSON + Encryption
- ✅ Code Quality Tools - StyleCop, SonarQube
- ✅ Testing Framework - 63 tests, 80%+ coverage
- ✅ Documentation - XML Comments

### Phase 1.5: MVP Generators ✅ (100%)

- ✅ SQL Generator - Stored Procedures (6 types)
- ✅ Entity Generator - C# Classes
- ✅ Type Mapper - SQL → C# types (44 tests)
- ✅ Prefix Handler - 12 prefixes (36 tests)
- ✅ Property Generator - C# properties (22 tests)
- ✅ Method Generator - Constructors, ToString, etc. (33 tests)
- ✅ Relationship Generator - Navigation properties (17 tests)
- ✅ File Writer - With *.prt protection
- ✅ 205+ Tests passing

### Phase 2: Modern Architecture 🔨 (In Progress)

**Week 1-2: Repository Pattern**
- [ ] RepositoryInterfaceGenerator
- [ ] RepositoryGenerator
- [ ] DbContextGenerator

**Week 3: CQRS + MediatR**
- [ ] QueryGenerator (GetById, GetAll, GetByIndex)
- [ ] CommandGenerator (Create, Update, Delete)
- [ ] ValidatorGenerator (FluentValidation)
- [ ] DtoGenerator

**Week 4: API Layer**
- [ ] ApiControllerGenerator
- [ ] Middleware (Exception, Logging, Performance)
- [ ] Swagger configuration

**Week 5: Integration & Testing**
- [ ] End-to-End tests
- [ ] Performance tests
- [ ] Documentation

**Target:** v2.0.0-rc1 (4-5 weeks)

### Phase 3: Advanced Features 📋 (Planned)

- [ ] React UI Generator (Material-UI)
- [ ] AI Assistant (Claude/OpenAI)
- [ ] Smart Error Guide
- [ ] Migration Tool (VB.NET → C#)
- [ ] Visual Schema Designer
- [ ] Version Control Integration

**Target:** v2.0.0 (6-8 weeks after Phase 2)

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test project
dotnet test src/TargCC.Core.Tests/

# Run tests by category
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

**Current Stats:**
- **Total Tests:** 205+
- **Coverage:** 85%+
- **Pass Rate:** 100%

---

## 📚 Documentation

- [Architecture Decision](docs/ARCHITECTURE_DECISION.md) - Why Clean Architecture?
- [Phase 2 Specification](docs/PHASE2_MODERN_ARCHITECTURE.md) - Detailed plan
- [Phase 3 Features](docs/PHASE3_ADVANCED_FEATURES.md) - Future features
- [Core Principles](docs/CORE_PRINCIPLES.md) - Build Errors philosophy
- [Project Roadmap](docs/PROJECT_ROADMAP.md) - Complete timeline
- [Entity Generator Spec](docs/ENTITY_GENERATOR_SPEC.md) - Generator details

---

## 🤝 Contributing

Contributions are welcome! Please read our contributing guidelines first.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- **Inspiration:** Original TargCC system (VB.NET)
- **Architecture:** Clean Architecture by Robert C. Martin
- **Patterns:** CQRS by Greg Young
- **Tools:** MediatR, Dapper, FluentValidation, AutoMapper

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
🔨 Phase 2: Modern Architecture (4-5 weeks) - IN PROGRESS
📋 Phase 3: UI + AI (6-8 weeks) - PLANNED
💡 Phase 4: Enterprise Features (TBD) - FUTURE
```

**Timeline:** ~5-6 months to v2.0.0

---

## ⭐ Star History

If you find this project useful, please consider giving it a star! ⭐

---

**Built with ❤️ by Doron**

**Powered by:** C# • .NET 8 • Clean Architecture • CQRS • React
