# TargCC.CLI 🚀

**TargCC Core V2 - Command Line Interface**

Professional CLI tool for code generation from database schemas.

**Requirements:** .NET 9.0 or later

---

## 📦 Installation

### From Source:
```bash
cd src/TargCC.CLI
dotnet build
dotnet run -- --help
```

### As Global Tool (future):
```bash
dotnet tool install -g TargCC.CLI
targcc --help
```

---

## 🎯 Quick Start

### 1. Initialize Project
```bash
targcc init
```

This will:
- Create `~/.targcc/config.json`
- Prompt for database connection
- Set up default preferences

### 2. Show Configuration
```bash
targcc config show
```

### 3. Set Configuration
```bash
targcc config set ConnectionString "Server=localhost;Database=MyDb"
targcc config set OutputDirectory "./Generated"
targcc config set DefaultNamespace "MyApp"
```

### 4. Reset Configuration
```bash
targcc config reset
```

---

## 📋 Available Commands

### Global Commands
- `targcc --version` - Show version information
- `targcc --help` - Show help
- `targcc init` - Initialize TargCC
- `targcc config` - Manage configuration

### Global Options
- `--verbose, -v` - Enable verbose output

---

## 🔧 Configuration

Configuration is stored in `~/.targcc/config.json`.

### Available Settings:

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `connectionString` | string | null | Database connection string |
| `outputDirectory` | string | null | Output directory for generated code |
| `defaultNamespace` | string | "MyApp" | Default namespace |
| `useCleanArchitecture` | bool | true | Use Clean Architecture structure |
| `generateCqrs` | bool | true | Generate CQRS patterns |
| `generateApiControllers` | bool | true | Generate API controllers |
| `generateRepositories` | bool | true | Generate repositories |
| `generateStoredProcedures` | bool | true | Generate stored procedures |
| `useDapper` | bool | true | Use Dapper for data access |
| `generateValidators` | bool | true | Generate FluentValidation validators |
| `logLevel` | string | "Information" | Logging level |
| `verbose` | bool | false | Verbose output |

---

## 📂 Directory Structure

```
TargCC.CLI/
├── Program.cs                 # Entry point
├── Commands/
│   └── RootCommand.cs         # Main command
├── Configuration/
│   ├── CliConfiguration.cs    # Config model
│   ├── IConfigurationService.cs
│   └── ConfigurationService.cs
├── Services/
│   ├── IOutputService.cs
│   └── OutputService.cs       # Console output
└── TargCC.CLI.csproj
```

---

## 🧪 Testing

```bash
cd tests/TargCC.CLI.Tests
dotnet test
```

---

## 📝 Examples

### Example 1: Initialize with Custom Settings
```bash
targcc init
# Follow prompts to configure
```

### Example 2: View Configuration
```bash
targcc config show
```

### Example 3: Update Settings
```bash
targcc config set LogLevel Debug
targcc config set Verbose true
```

---

## 🎨 Output Features

- ✅ Colored output (success/error/warning/info)
- 📊 Progress bars and spinners
- 📋 Tables for structured data
- 🎯 Interactive prompts
- 🚀 Professional CLI experience

Powered by [Spectre.Console](https://spectreconsole.net/)

---

## 🔍 Troubleshooting

### Configuration File Location
```bash
# Windows
%USERPROFILE%\.targcc\config.json

# macOS/Linux
~/.targcc/config.json
```

### Logs Location
```bash
# Windows
%USERPROFILE%\.targcc\logs\

# macOS/Linux
~/.targcc/logs/
```

### Reset Everything
```bash
targcc config reset
```

---

## 🚀 Coming Soon (Day 2-10)

- `targcc generate entity <table>` - Generate entity
- `targcc generate sql <table>` - Generate SQL
- `targcc generate all <table>` - Generate everything
- `targcc generate project` - Generate entire project
- `targcc analyze schema` - Analyze database
- `targcc analyze impact` - Impact analysis
- `targcc watch` - Watch mode for auto-generation

---

## 📚 Documentation

- [Phase 3 Specification](../../docs/PHASE3_ADVANCED_FEATURES.md)
- [Phase 3 Checklist](../../docs/Phase3_Checklist.md)
- [Project Roadmap](../../docs/PROJECT_ROADMAP.md)

---

**Built with ❤️ by Doron + Claude**  
**Version:** 2.0.0-beta.1  
**Phase:** 3A Day 1
