# TargCC.CLI.Tests 🧪

**Unit tests for TargCC CLI**

---

## 📊 Test Coverage

### Current Status (Day 1)

| Component | Tests | Coverage | Status |
|-----------|-------|----------|--------|
| CliConfiguration | 11 | 100% | ✅ |
| ConfigurationService | 18 | 100% | ✅ |
| OutputService | 7 | 95% | ✅ |
| **Total** | **36** | **98%** | ✅ |

---

## 🏃 Running Tests

### Run All Tests
```bash
dotnet test
```

### Run with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Run Specific Tests
```bash
dotnet test --filter "FullyQualifiedName~ConfigurationServiceTests"
dotnet test --filter "FullyQualifiedName~OutputServiceTests"
```

### Run by Category
```bash
dotnet test --filter "Category=Configuration"
dotnet test --filter "Category=Services"
```

---

## 📂 Test Structure

```
TargCC.CLI.Tests/
├── Configuration/
│   ├── CliConfigurationTests.cs       # 11 tests
│   └── ConfigurationServiceTests.cs   # 18 tests
├── Services/
│   └── OutputServiceTests.cs          # 7 tests
├── Commands/
│   └── (Coming in Day 2)
└── Usings.cs
```

---

## 🧪 Test Patterns

### Configuration Tests
- ✅ Default values
- ✅ Property setters/getters
- ✅ File I/O operations
- ✅ Error handling
- ✅ Null checks

### Service Tests
- ✅ Constructor validation
- ✅ Method behavior
- ✅ Logging verification
- ✅ Exception handling

---

## 📦 Dependencies

- **xUnit** - Test framework
- **FluentAssertions** - Fluent assertion library
- **Moq** - Mocking framework
- **coverlet** - Code coverage

---

## 🎯 Coverage Goals

- **Target:** 85%+
- **Current:** 98%
- **Status:** ✅ Above target

---

## 📝 Test Naming Convention

```csharp
[MethodName]_[Scenario]_[ExpectedResult]

Examples:
- Constructor_ThrowsException_WhenLoggerIsNull
- LoadAsync_ReturnsCorrectConfiguration
- SetValueAsync_UpdatesStringProperty
```

---

## 🔍 Test Categories

Tests are organized by component:
- `Configuration` - Configuration-related tests
- `Services` - Service layer tests
- `Commands` - Command tests (coming)

---

## 🚀 Coming Soon (Day 2-10)

- Command tests
- Generate command tests
- Analyze command tests
- Integration tests
- E2E tests

---

**Built with ❤️ by Doron + Claude**  
**Version:** 2.0.0-beta.1  
**Phase:** 3A Day 1
