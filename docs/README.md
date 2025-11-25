# TargCC Core V2 🚀

**Modern Code Generation Platform - CLI + AI + Web UI Edition**

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![.NET Version](https://img.shields.io/badge/.NET-8.0-blue)]()
[![Phase 1](https://img.shields.io/badge/Phase%201-100%25-brightgreen)]()
[![Phase 1.5](https://img.shields.io/badge/Phase%201.5-100%25-brightgreen)]()
[![Phase 2](https://img.shields.io/badge/Phase%202-100%25-brightgreen)]()
[![Phase 3](https://img.shields.io/badge/Phase%203-Starting-blue)]()
[![Tests](https://img.shields.io/badge/tests-360%2B%20passing-success)]()
[![Coverage](https://img.shields.io/badge/coverage-85%25%2B-brightgreen)]()
[![License](https://img.shields.io/badge/license-MIT-green)]()

---

## 🌟 What is TargCC 2.0?

**TargCC Core V2** is a modern code generation platform that creates complete applications from database schemas using **Clean Architecture**, **CQRS**, and **REST API**.

### 🎯 Core Philosophy:

1. ⚡ **Incremental Generation** - Only what changed
2. 🛡️ **Build Errors as Safety Net** - Not bugs, features!
3. 🤖 **Smart Assistance** - System suggests, you decide

### Final Product Vision:

```
┌─────────────────────────────────────────────────────┐
│                   TargCC 2.0                        │
│                                                     │
│   CLI (Professional)    +    Web UI (Visual)        │
│   ─────────────────         ──────────────          │
│   $ targcc generate all     • Wizard interface      │
│   $ targcc analyze          • Schema Designer       │
│   $ targcc suggest          • AI Chat panel         │
│                                                     │
│              Powered by AI Assistant                │
│              ────────────────────────               │
│              • Smart suggestions                    │
│              • Security scanning                    │
│              • Code review                          │
└─────────────────────────────────────────────────────┘
```

---

## 🏗️ What Gets Generated?

### From ONE Database Table → Complete Application:

```
Input: Customer Table
        ↓
Output: 5-Layer Clean Architecture

✅ Domain/
   └── Customer.cs                    (Entity)
   └── ICustomerRepository.cs         (Interface)

✅ Application/
   ├── GetCustomerQuery.cs            (CQRS Query)
   ├── CreateCustomerCommand.cs       (CQRS Command)
   └── CustomerDto.cs                 (DTO)

✅ Infrastructure/
   ├── CustomerRepository.cs          (Data Access)
   ├── SP_GetCustomer.sql            (Stored Procedures)
   └── ApplicationDbContext.cs        (EF Core)

✅ API/
   └── CustomersController.cs         (REST API)

→ Complete CRUD API ready in minutes! 🚀
```

---

## 💡 The Central Principle

> **"Incremental Generation + Mandatory Manual Review"**

The system generates smart code quickly, but **you** are always in control.

### Build Errors = Good! ✅

```
Changed CustomerID from string to int?

✅ Auto-generated files updated automatically
⚠️  3 Build Errors in manual code (*.prt files)

→ This is exactly what we want!
   Now you know exactly where to fix.
```

**Read more:** [Core Principles](CORE_PRINCIPLES.md)

---

## 🚀 Usage (After Phase 3)

### CLI Usage:

```bash
# Initialize project
$ targcc init

# Generate from single table
$ targcc generate all Customer

🔍 Analyzing table: Customer...
  ✓ 8 columns found
  ✓ 2 indexes detected

📦 Generating code...
  ✓ Domain/Entities/Customer.cs
  ✓ Domain/Interfaces/ICustomerRepository.cs
  ✓ Infrastructure/Repositories/CustomerRepository.cs
  ✓ Infrastructure/Sql/SP_*.sql (6 files)
  ✓ Application/Features/Customers/* (10 files)
  ✓ API/Controllers/CustomersController.cs

✅ Generated 20 files in 2.1s

# Generate entire project
$ targcc generate project --database MyDatabase

# Get AI suggestions
$ targcc suggest --table Customer

# Launch Web UI
$ targcc ui
```

### Web UI:

```bash
$ targcc ui

🌐 Starting TargCC Web UI...
   Open http://localhost:5000 in your browser
```

- **Dashboard** - Project overview
- **Generation Wizard** - Step-by-step generation
- **Schema Designer** - Visual table design
- **AI Chat** - Interactive AI assistance

---

## 📊 Project Status

### ✅ Phase 1: Core Engine (100% Complete)
- DatabaseAnalyzer, TableAnalyzer, ColumnAnalyzer, RelationshipAnalyzer
- Plugin System, Configuration Manager
- 63 Tests, 80%+ coverage, Grade A

### ✅ Phase 1.5: MVP Generators (100% Complete)
- SQL Generators (6 SP templates)
- Entity Generators (12 prefix types)
- 205+ Tests, 85%+ coverage

### ✅ Phase 2: Modern Architecture (100% Complete)
- Repository Pattern with Dapper
- CQRS + MediatR
- REST API Controllers
- DbContext + Entity Configuration
- 160+ Tests, 85%+ coverage

### 🆕 Phase 3: CLI + AI + Web UI (Starting Now)
- **Phase 3A:** CLI Core (2 weeks)
- **Phase 3B:** AI Integration (2 weeks)
- **Phase 3C:** Local Web UI (3 weeks)
- **Phase 3D:** Migration & Polish (2 weeks)

**Read:** [Phase 3 Specification](PHASE3_ADVANCED_FEATURES.md)

---

## 🎯 Success Metrics

### Time Savings:

| Task | Before | After | Savings |
|------|--------|-------|---------|
| Add Entity | 2-4 hours | **10-20 min** | **90%** |
| Add Field | 1-2 hours | **5-10 min** | **90%** |
| CRUD Screen | 4-8 hours | **30-60 min** | **87%** |
| API Endpoint | 2-3 hours | **10-20 min** | **90%** |
| Full Project | 1-2 weeks | **Minutes** | **99%** |

---

## 📁 Project Structure

```
TargCC-Core-V2/
├── src/
│   ├── TargCC.Core.Engine/         ✅ Core functionality
│   ├── TargCC.Core.Interfaces/     ✅ Contracts
│   ├── TargCC.Core.Analyzers/      ✅ Database analysis
│   ├── TargCC.Core.Generators/     ✅ Code generation
│   ├── TargCC.CLI/                 🆕 CLI application (Phase 3A)
│   ├── TargCC.AI/                  🆕 AI service (Phase 3B)
│   ├── TargCC.Web/                 🆕 React UI (Phase 3C)
│   └── TargCC.API/                 🆕 Backend API (Phase 3C)
│
├── tests/
│   └── TargCC.Core.Tests/          ✅ 360+ tests
│
└── docs/
    ├── README.md                    ✅ This file
    ├── ARCHITECTURE_DECISION.md    ✅ Architecture
    ├── PHASE2_MODERN_ARCHITECTURE.md ✅ Phase 2 (Done)
    ├── PHASE3_ADVANCED_FEATURES.md  🆕 Phase 3 (Current)
    ├── Phase3_Checklist.md          🆕 Daily tasks
    └── PROJECT_ROADMAP.md           ✅ Roadmap
```

---

## 📚 Documentation

### Phase Specifications:
- [Phase 1: Core Engine](Phase1_Checklist.md) ✅ Complete
- [Phase 1.5: MVP Generators](PHASE1_5_MVP_GENERATORS.md) ✅ Complete
- [Phase 2: Modern Architecture](PHASE2_MODERN_ARCHITECTURE.md) ✅ Complete
- [Phase 3: CLI + AI + Web UI](PHASE3_ADVANCED_FEATURES.md) 🆕 Starting
- [Phase 3 Checklist](Phase3_Checklist.md) 🆕 Daily tasks

### Architecture:
- [Architecture Decision](ARCHITECTURE_DECISION.md)
- [Core Principles](CORE_PRINCIPLES.md)
- [Project Roadmap](PROJECT_ROADMAP.md)

---

## 🚀 Roadmap

```
✅ Phase 1: Core Engine (6 weeks) - DONE
✅ Phase 1.5: MVP Generators (2 weeks) - DONE
✅ Phase 2: Modern Architecture (4 weeks) - DONE
🆕 Phase 3: CLI + AI + Web UI (9 weeks) - STARTING NOW
📋 Phase 4: Enterprise & Cloud (TBD) - FUTURE
```

**Current Focus:** Phase 3A - CLI Core

**See:** [Complete Roadmap](PROJECT_ROADMAP.md)

---

**Built with ❤️ by Doron + Claude**

**Last Updated:** 24/11/2025  
**Version:** 2.0.0-beta (Phase 2 Complete, Phase 3 Starting)
