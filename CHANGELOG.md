# Changelog

All notable changes to TargCC Core V2 will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [2.0.0-beta.1] - 2025-11-27

### 🎉 Phase 3A Complete - CLI Core (100%)

This release marks the completion of Phase 3A, delivering a professional command-line interface for TargCC with 16 commands and comprehensive project generation capabilities.

### Added

#### CLI Commands (16 total)
- **`targcc init`** - Initialize TargCC in current directory with interactive configuration
- **`targcc version`** - Display version and system information
- **`targcc config`** - Configuration management
  - `config show` - Display current configuration
  - `config set` - Set configuration values
  - `config reset` - Reset configuration to defaults
- **`targcc generate`** - Code generation commands
  - `generate entity <table>` - Generate entity class
  - `generate sql <table>` - Generate SQL stored procedures
  - `generate repo <table>` - Generate repository pattern
  - `generate cqrs <table>` - Generate CQRS queries and commands
  - `generate api <table>` - Generate REST API controller
  - `generate all <table>` - Generate complete stack for a table (most commonly used)
  - `generate project` - Generate complete Clean Architecture solution from database
- **`targcc analyze`** - Analysis and reporting
  - `analyze schema` - Analyze database schema structure
  - `analyze impact` - Analyze impact of schema changes on code
  - `analyze security` - Security vulnerability scanning
  - `analyze quality` - Code quality and naming convention analysis
- **`targcc watch`** - Watch mode for automatic regeneration on schema changes

#### Global Options
- `--verbose, -v` - Enable detailed output
- `--config <path>` - Specify custom configuration file path
- `--no-color` - Disable colored output
- `--quiet, -q` - Minimal output (errors only)

#### Project Generation Features
- **Complete solution generation** from database schema
- **Clean Architecture structure** with 5 layers (Domain, Application, Infrastructure, API, Tests)
- **Solution file (.sln)** generation
- **Project files (.csproj)** with proper dependencies and NuGet packages
- **Program.cs** generation with dependency injection
- **appsettings.json** generation with connection strings
- **DependencyInjection.cs** files for each layer
- **Supports multiple architecture patterns**: Clean Architecture, Three-Tier, Minimal API

#### Code Generation Improvements
- **Entity generation** with all 12 prefix handlers (eno_, ent_, lkp_, enm_, loc_, clc_, blg_, agg_, spt_, upl_, scb_, spl_)
- **SQL stored procedures** (GetByID, GetAll, Insert, Update, Delete, Search)
- **Repository pattern** (interface + Dapper implementation)
- **CQRS handlers** with MediatR (Queries, Commands, Validators, DTOs)
- **REST API controllers** with Swagger documentation
- **Preserves manual code** in *.prt.cs files (never overwritten)

#### Watch Mode & Schema Change Detection
- **Real-time monitoring** of database schema changes
- **Automatic regeneration** of affected files only (incremental generation)
- **Generation tracking** to avoid unnecessary regeneration
- **Configurable check interval** (default: 5 seconds)
- **Table-specific watching** with `--tables` option
- **Detection-only mode** with `--no-auto-generate` flag

#### Analysis & Impact Assessment
- **Schema analysis** with table/column/relationship visualization
- **Impact analysis** showing affected files when schema changes
- **Security scanning** for unencrypted sensitive data (eno_/ent_ prefix validation)
- **Quality analysis** for naming conventions, relationships, and indexes
- **Estimated fix time** for manual code updates
- **Export to JSON/HTML** for reports

#### CLI Infrastructure
- **System.CommandLine** integration for professional CLI experience
- **Spectre.Console** for beautiful terminal output with colors, tables, and spinners
- **Serilog** integration for structured logging
- **Configuration service** with JSON persistence (targcc.json)
- **Output service** for consistent formatting (heading, info, success, error, warning)
- **Error handling** with friendly messages and suggestions
- **Progress indicators** for long-running operations
- **Dependency injection** throughout CLI

### Testing
- **145 tests passing** (207% of target)
- **~95% code coverage** (exceeds 85% target)
- **Unit tests** for all commands and services
- **Integration tests** for end-to-end workflows
- **Error scenario tests** for edge cases
- **Performance tests** for generation speed
- **Zero flaky tests** - all tests reliable

### Documentation
- **CLI-REFERENCE.md** - Complete command reference with examples (1,200+ lines)
- **QUICKSTART.md** - 5-minute guide from zero to running app (500+ lines)
- **XML documentation** on all public APIs
- **Help system** with `--help` on every command
- **Example workflows** in documentation

### Fixed
- **Database connection** handling with proper error messages
- **.NET version alignment** across all projects (now .NET 9)
- **Dependency injection** configuration issues
- **Interface method signatures** matching implementations
- **NuGet package** version conflicts resolved
- **Project reference** paths corrected
- **StyleCop** warnings eliminated (clean builds)

### Performance
- **Fast generation** - Complete project (10 tables) in ~10 seconds
- **Efficient schema analysis** - Database scan in <1 second
- **Incremental regeneration** - Only changed files regenerated
- **Parallel processing** where applicable
- **Optimized SQL queries** for schema detection

---

## [2.0.0-alpha.2] - 2025-11-15

### Phase 1.5 Complete - MVP Generators (100%)

### Added
- SQL Generator with 6 stored procedure types
- Entity Generator with C# class generation
- Type Mapper (SQL → C#) with 44 tests
- Prefix Handler for 12 special prefixes (36 tests)
- Property Generator (22 tests)
- Method Generator (33 tests)
- Relationship Generator (17 tests)
- File Writer with *.prt protection
- 205+ tests passing

---

## [2.0.0-alpha.1] - 2025-11-01

### Phase 1 Complete - Core Engine (100%)

### Added
- DatabaseAnalyzer - Full database schema analysis
- TableAnalyzer - Tables and indexes
- ColumnAnalyzer - Columns, types, and prefixes
- RelationshipAnalyzer - Foreign key relationships
- Plugin System - Modular architecture
- Configuration Manager - JSON + encryption
- Code Quality Tools - StyleCop, SonarQube
- Testing Framework - 63 tests, 80%+ coverage
- Documentation - Comprehensive XML comments

---

## Planned Releases

### [2.0.0-beta.2] - Phase 3B: AI Integration (Expected: December 2025)
- Claude API integration for smart suggestions
- AI-powered schema analysis
- Security vulnerability detection with AI
- Code quality recommendations with AI
- Interactive chat for development assistance
- Smart error guide with fix suggestions

### [2.0.0-beta.3] - Phase 3C: Local Web UI (Expected: January 2026)
- React + TypeScript web interface
- Visual schema designer with React Flow
- Generation wizard with preview
- Dashboard for project overview
- AI chat panel in UI
- Real-time generation progress

### [2.0.0-rc1] - Phase 3D: Migration & Polish (Expected: February 2026)
- Migration tool from legacy VB.NET TargCC
- Git integration with LibGit2Sharp
- Auto-commit and rollback features
- Comprehensive regression testing
- Performance optimization
- Final bug fixes

### [2.0.0] - General Availability (Expected: March 2026)
- Production-ready release
- Complete documentation
- Tutorial videos
- Example projects
- Community support channels

---

## Version History

| Version | Date | Phase | Status | Highlights |
|---------|------|-------|--------|------------|
| 2.0.0-beta.1 | 2025-11-27 | 3A | ✅ Complete | CLI with 16 commands, 145 tests |
| 2.0.0-alpha.2 | 2025-11-15 | 1.5 | ✅ Complete | MVP Generators, 205 tests |
| 2.0.0-alpha.1 | 2025-11-01 | 1 | ✅ Complete | Core Engine, 63 tests |
| 2.0.0-beta.2 | TBD | 3B | 📋 Planned | AI Integration |
| 2.0.0-beta.3 | TBD | 3C | 📋 Planned | Local Web UI |
| 2.0.0-rc1 | TBD | 3D | 📋 Planned | Migration & Polish |
| 2.0.0 | TBD | GA | 📋 Planned | General Availability |

---

## Statistics

### Phase 3A (2.0.0-beta.1) Final Numbers

| Metric | Target | Actual | Achievement |
|--------|--------|--------|-------------|
| CLI Commands | 15 | 16 | 107% |
| Tests Passing | 70+ | 145 | 207% |
| Code Coverage | 85% | ~95% | 112% |
| Code Files | ~50 | 96 | 192% |
| Lines of Code | ~5,000 | ~11,600 | 232% |
| Documentation | Complete | Complete | 100% |
| Build Warnings | 0 | 0 | ✅ |
| StyleCop Issues | 0 | 0 | ✅ |

### Cumulative Project Statistics

| Metric | Value |
|--------|-------|
| **Total Tests** | 145 |
| **Test Pass Rate** | 100% |
| **Code Coverage** | ~95% |
| **Total Code Files** | 96 |
| **Total Lines of Code** | ~11,600 |
| **Supported Prefixes** | 12 |
| **Database Providers** | SQL Server (more planned) |
| **Architecture Patterns** | 3 (Clean, Three-Tier, Minimal API) |
| **Generation Modes** | Manual, Watch, Batch |
| **Documentation Pages** | 15+ |

---

## Breaking Changes

### 2.0.0-beta.1
- **None** - First beta release, no prior stable version

### Future Breaking Changes
- Configuration file format may change between beta versions
- CLI command structure is subject to change until 2.0.0 release
- API surface may change between beta versions

---

## Upgrade Guide

### From 2.0.0-alpha.2 to 2.0.0-beta.1

No upgrade needed - Phase 1.5 focused on core generators. Phase 3A adds CLI on top.

**New users:**
1. Install TargCC CLI
2. Run `targcc init` in your project directory
3. Follow the quickstart guide

---

## Known Issues

### 2.0.0-beta.1
- **Watch mode** may miss very rapid schema changes (< 1 second apart)
- **Project generation** only supports SQL Server (MySQL, PostgreSQL planned)
- **UI generation** not yet available (coming in Phase 3C)
- **AI features** not yet available (coming in Phase 3B)

### Workarounds
- For rapid schema changes: Increase watch interval or regenerate manually
- For other databases: Use custom connection strings (experimental)

---

## Contributing

We welcome contributions! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### How to Report Issues
1. Check [existing issues](https://github.com/doron/TargCC-Core-V2/issues)
2. Create new issue with:
   - TargCC version (`targcc version`)
   - Detailed description
   - Steps to reproduce
   - Expected vs actual behavior
   - Logs from `~/.targcc/logs/`

---

## License

MIT License - See [LICENSE](LICENSE) for details

---

## Acknowledgments

- **Original TargCC** (VB.NET) - Inspiration and proven concepts
- **Clean Architecture** - Robert C. Martin
- **CQRS Pattern** - Greg Young
- **System.CommandLine** - .NET Foundation
- **Spectre.Console** - Patrik Svensson
- **MediatR** - Jimmy Bogard
- **Dapper** - Stack Overflow team
- **FluentValidation** - Jeremy Skinner

---

**Last Updated:** 27/11/2025  
**Maintained by:** Doron Vaida  
**Repository:** https://github.com/doron/TargCC-Core-V2
